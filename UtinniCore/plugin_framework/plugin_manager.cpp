/**
 * MIT License
 *
 * Copyright (c) 2020 Philip Klatt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
**/

#include "plugin_manager.h"
#include <filesystem>
#include "utility/string_utility.h"

namespace utinni
{
    using pCreatePlugin = UtinniPlugin*(*)();

    struct PluginManager::Impl
    {
        std::vector<PluginConfig> pluginConfigs;
        std::vector<UtinniPlugin*> plugins;
    };

    PluginManager::PluginManager() : pImpl(new Impl) { }

    PluginManager::~PluginManager()
    {
        for (const auto& plugin : pImpl->plugins)
        {
            delete plugin;
        }

        delete pImpl;
    }

    void PluginManager::loadPlugins() const
    {
        const std::string pluginDir = getPath() + "/Plugins/";

        auto& cfg = getConfig();

        // Get the [Plugins] load order list from ut.ini
        int i = 0;
        while (true)
        {
            std::string curStr = cfg.getString("Plugins", std::string("plugin_" + stringUtility::toString(i, 2)).c_str());
            if (!curStr.empty())
            {
                i++;
                const int splitterPos = curStr.find_first_of(',');
                if (splitterPos == -1)
                {
                    log::error(std::string("Failed to parse [Plugins] priority list value due to missing separator: " + curStr).c_str());
                    continue;
                }

                std::string directoryName = curStr.substr(splitterPos + 1);
                std::string isEnabled = curStr.substr(0, splitterPos);
                PluginConfig pluginInfo = { stringUtility::trim(directoryName),  stringUtility::toBool(stringUtility::trim(isEnabled)) };
                pImpl->pluginConfigs.emplace_back(pluginInfo);
            }
            else
            {
                // Stop if [Plugins] plugin_ + i doesn't exist
                break;
            }
        }

        // Create directory in case it doesn't exist for whatever reason
        CreateDirectory(pluginDir.c_str(), nullptr);

        // Check if each found plugin directory exists in the [Plugins] load order, if not, add it at the back
        std::vector<std::string> directories;
        for (const auto& dir : std::filesystem::recursive_directory_iterator(pluginDir))
        {
            if (dir.is_directory())
            {
                const std::string& dirFilename = dir.path().filename().string();

                auto it = std::find_if(pImpl->pluginConfigs.begin(), pImpl->pluginConfigs.end(), [&dirFilename](const PluginConfig& pluginInfo)
                {
                    return pluginInfo.directoryName == dirFilename;
                });

                if (it == pImpl->pluginConfigs.end())
                {
                    PluginConfig pluginInfo = { dirFilename, true };
                    pImpl->pluginConfigs.emplace_back(pluginInfo);
                    // ToDo potentially add a cfg setting to not automatically enable the plugin if it didn't exist in the list
                }
            }
        }

        // Update the [Plugins] section in ut.ini & load the plugins in the correct load order set in the cfg
        cfg.DeleteSection("Plugins");
        for (size_t j = 0; j < pImpl->pluginConfigs.size(); j++)
        {
            const auto pluginCfg = pImpl->pluginConfigs[j];
            cfg.setString("Plugins", std::string("plugin_" + stringUtility::toString(j, 2)).c_str(), 
                      std::string(stringUtility::toString(pluginCfg.isEnabled) + ", " + pluginCfg.directoryName).c_str());

            // if the plugin is enabled, scan its directory for .DLL's and see if they implement createPlugin
            if (pluginCfg.isEnabled)
            {
                auto currentPluginDir = pluginDir + pluginCfg.directoryName + "/";

                if (!std::filesystem::exists(currentPluginDir))
                {
                    pImpl->pluginConfigs.erase(pImpl->pluginConfigs.begin() + j);
                    j--;
                    continue;
                }

                for (const auto& file : std::filesystem::recursive_directory_iterator(currentPluginDir))
                {
                    if (file.path().extension() == ".dll")
                    {
                        // Try to load the found .dll so that it that the createPlugin function can be looked up
                        const HINSTANCE hDllInstance = LoadLibrary(file.path().string().c_str());
                        if (hDllInstance != nullptr)
                        {
                            // Check if it is a valid UtinniPlugin, which contains the createPlugin function
                            const auto createPlugin = (pCreatePlugin) GetProcAddress(hDllInstance, "createPlugin");
                            if (createPlugin != nullptr)
                            {
                                // If the function exists in the dll, call the createPlugin function and emplace it in the plugin manager plugin list
                                UtinniPlugin* plugin = createPlugin();
                                if (plugin != nullptr)
                                {
                                    pImpl->plugins.emplace_back(plugin);
                                }
                            }
                        }
                    }
                }
            }
        }
        cfg.save();
    }

    int PluginManager::getPluginConfigCount() const
    {
        return pImpl->pluginConfigs.size();
    }

    const PluginManager::PluginConfig& PluginManager::getPluginConfigAt(int i)
    {
        return pImpl->pluginConfigs.at(i);
    }
}

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

#include "utini.h"

#include "LeksysINI/iniparser.hpp"

namespace utinni
{
const static UtINI::Value utinniSettings[] = {

    // Launcher settings
    { "Launcher", "swgClientName", "", UtINI::Value::vt_string },
    { "Launcher", "swgClientName", "", UtINI::Value::vt_string },

    // UtinniCore settings
    { "UtinniCore", "enableInternalUi", "false", UtINI::Value::vt_bool },
    { "UtinniCore", "enableOfflineScenes", "false", UtINI::Value::vt_bool },
    { "UtinniCore", "useSwgOverrideCfg", "false", UtINI::Value::vt_bool },
    { "UtinniCore", "autoLoadScene", "false", UtINI::Value::vt_bool },

    // Log settings
    { "Log", "writeClassName", "false", UtINI::Value::vt_bool },
    { "Log", "writeFunctionName", "false", UtINI::Value::vt_bool },

    // Editor settings
    { "Editor", "enableEditorMode", "true", UtINI::Value::vt_bool },
    { "Editor", "defaultPluginPanel", "Main Controls", UtINI::Value::vt_string },
    { "Editor", "autoOpenLogWindow", "false", UtINI::Value::vt_bool },
    { "Editor", "width", "1200", UtINI::Value::vt_int },
    { "Editor", "height", "500", UtINI::Value::vt_int },

};

struct UtINI::Impl
{
    INI::File ini;
    std::vector<Value> settings;
};

UtINI::UtINI() : pImpl(new Impl) { }

UtINI::UtINI(const std::string& filename) : pImpl(new Impl)
{
    iniFilename = filename;
}

UtINI::~UtINI()
{
    delete pImpl;
}

void UtINI::load() const
{
    if (!pImpl->ini.Load(iniFilename))
    {
        // Save the default values to file
        for (const auto& iniValue : pImpl->settings)
        {
            pImpl->ini.GetSection(iniValue.sectionName)->SetValue(iniValue.valueName, iniValue.value);
        }

        save();
    }
    else
    {
        // Validate the loaded values to make sure all needed values exist, if not, create them
        bool missingValues = false;
        for (const auto& iniValue : pImpl->settings)
        {
            if (pImpl->ini.GetSection(iniValue.sectionName)->GetValue(iniValue.valueName).AsString().empty())
            {
                pImpl->ini.GetSection(iniValue.sectionName)->SetValue(iniValue.valueName, iniValue.value);
                missingValues = true;
            }
        }

        if (missingValues)
        {
            save();
        }
    }
}

void UtINI::load(const std::string& filename)
{
    iniFilename = filename;
    load();
}

void UtINI::save() const
{
    pImpl->ini.Save(iniFilename);
}

void UtINI::createUtinniSettings() const
{
    for (const Value& value : utinniSettings)
    {
        pImpl->settings.emplace_back(value);
    }
}

void UtINI::addSetting(const Value& value) const
{
    pImpl->settings.emplace_back(value);
}

void UtINI::addSetting(const char* sectionName, const char* valueName, const char* value, Value::Types type) const
{
    Value val = { sectionName, valueName, value, type };
    pImpl->settings.emplace_back(val);
}

void UtINI::DeleteSection(const char* sectionName)
{
    pImpl->ini.DeleteSection(sectionName);
}

std::string UtINI::getString(const char* sectionName, const char* valueName) const
{
    return pImpl->ini.GetSection(sectionName)->GetValue(valueName).AsString();
}

bool UtINI::getBool(const char* sectionName, const char* valueName) const
{
    return pImpl->ini.GetSection(sectionName)->GetValue(valueName).AsBool();
}

int UtINI::getInt(const char* sectionName, const char* valueName) const
{
    return pImpl->ini.GetSection(sectionName)->GetValue(valueName).AsInt();
}

float UtINI::getFloat(const char* sectionName, const char* valueName) const
{
    return (float) pImpl->ini.GetSection(sectionName)->GetValue(valueName).AsDouble();
}

void UtINI::setString(const char* sectionName, const char* valueName, const char* value) const
{
    pImpl->ini.GetSection(sectionName)->SetValue(valueName, value);
}

void UtINI::setBool(const char* sectionName, const char* valueName, bool value) const
{
    pImpl->ini.GetSection(sectionName)->SetValue(valueName, value);
}

void UtINI::setInt(const char* sectionName, const char* valueName, int value) const
{
    pImpl->ini.GetSection(sectionName)->SetValue(valueName, value);
}

void UtINI::setFloat(const char* sectionName, const char* valueName, float value) const
{
    pImpl->ini.GetSection(sectionName)->SetValue(valueName, value);
}

}

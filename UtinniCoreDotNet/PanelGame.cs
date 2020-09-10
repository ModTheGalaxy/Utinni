﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UtinniCore.ImguiImpl;
using UtinniCore.Utinni;
using UtinniCoreDotNet.Hotkeys;
using UtinniCoreDotNet.PluginFramework;

namespace UtinniCoreDotNet
{
    public class PanelGame : Panel
    {
        [DllImport("user32.dll")]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        protected override void WndProc(ref Message m)
        {
            IntPtr swgWndProc = new IntPtr(0x00AA0970);
            CallWindowProc(swgWndProc, m.HWnd, m.Msg, m.WParam, m.LParam); // Call and handle SWG's WndProc
            base.WndProc(ref m);
        }

        public bool HasFocus;
        private bool isCursorVisible;

        private readonly PluginLoader pluginLoader;

        public PanelGame(PluginLoader pluginLoader)
        {
            Dock = DockStyle.Fill;

            Disposed += PanelGame_Disposed;

            MouseEnter += PanelGame_MouseEnter;
            MouseLeave += PanelGame_MouseLeave;
            MouseMove += PanelGame_MouseMove;

            KeyDown += PanelGame_KeyDown;

            Client.SetHwnd(Handle);
            Client.SetHInstance(Process.GetCurrentProcess().Handle);

            this.pluginLoader = pluginLoader;
        }

        private void PanelGame_Disposed(object sender, EventArgs e)
        {
            Game.Quit();
        }

        private void PanelGame_MouseEnter(object sender, EventArgs e)
        {
            isCursorVisible = false;
            Client.ResumeInput();
            Cursor.Hide();
            HasFocus = true;
        }

        private void PanelGame_MouseLeave(object sender, EventArgs e)
        {
            isCursorVisible = true;
            Client.SuspendInput();
            Cursor.Show();
            HasFocus = false;
        }

        private void PanelGame_MouseMove(object sender, MouseEventArgs e)
        {
            if (imgui_impl.IsInternalUiHovered() && !isCursorVisible)
            {
                isCursorVisible = true;
                Cursor.Show();
            }
            else if (!imgui_impl.IsInternalUiHovered() && isCursorVisible)
            {
                isCursorVisible = false;
                Cursor.Hide();
            }
        }

        private void PanelGame_KeyDown(object sender, KeyEventArgs e)
        {
            foreach (IPlugin plugin in pluginLoader.Plugins)
            {
                IEditorPlugin editorPlugin = (IEditorPlugin)plugin;
                if (editorPlugin != null)
                {
                    HotkeyManager hotkeyManager = editorPlugin.GetHotkeyManager();

                    if (hotkeyManager != null && hotkeyManager.OnGameFocusOnly)
                    {
                        hotkeyManager.ProcessInput(e.Modifiers, e.KeyCode);
                    }
                }
            }
        }

    }
}

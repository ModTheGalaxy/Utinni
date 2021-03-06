﻿/**
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using UtinniCoreDotNet.Properties;
using UtinniCoreDotNet.UI.Controls;
using UtinniCoreDotNet.UI.Theme;
using UtinniCoreDotNet.Utility;

namespace UtinniCoreDotNet.UI.Forms
{
    public partial class UtinniForm : Form
    {
        [Description("Resizable"), Category("Data")]
        public bool Resizable { get; set; } = true;

        [Description("Enable Shadow"), Category("Data")]
        public bool EnableShadow { get; set; } = true;

        [Description("Draw Name"), Category("Data")]
        public bool DrawName { get; set; } = false;

        private Image iconImage = null;
        [Description("Icon Image"), Category("Data")]
        public Image IconImage
        {
            get { return iconImage; }
            set
            {
                iconImage = new Bitmap(value, 24, 24);
            }
        }

        [Browsable(false)]
        public new FormBorderStyle FormBorderStyle
        {
            get { return base.FormBorderStyle; }
            set { base.FormBorderStyle = value; }
        }

        private const int titleBarHeight = 32;
        private int leftTitlebarOffset = 0;

        private int resizeBorderHitWidth = 5;

        private readonly Font nameFont = new Font("Arcon", 12);

        public List<UtinniTitlebarButton> LeftTitleBarButtons = new List<UtinniTitlebarButton>();
        public List<UtinniTitlebarButton> RightTitleBarButtons = new List<UtinniTitlebarButton>();

        private SolidBrush foreColorBrush;

        public UtinniForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            Padding = new Padding(0, titleBarHeight, 0, 0);
            FormBorderStyle = FormBorderStyle.None;
            this.Icon = Resources.TJT;

            UtinniTitlebarButton closeButton = new UtinniTitlebarButton(new Bitmap(Resources.close), Color.Red);
            closeButton.Click += CloseButton_Click;
            RightTitleBarButtons.Add(closeButton);

            base.BackColor = Colors.Primary(); // ToDo do this proper
            UpdateForeColor();
        }

        protected override void OnLoad(EventArgs e)
        {
            if (MinimizeBox)
            {
                UtinniTitlebarButton minimizeButton = new UtinniTitlebarButton(new Bitmap(Resources.min));
                minimizeButton.Click += MinimizeButton_Click;
                RightTitleBarButtons.Insert(1, minimizeButton);
            }

            if (MaximizeBox)
            {
                UtinniTitlebarButton maximizeButton = new UtinniTitlebarButton(new Bitmap(Resources.maximize));
                maximizeButton.Click += MaximizeButton_Click;
                RightTitleBarButtons.Insert(1, maximizeButton);
            }

            int rightButtonEdge = 0;
            foreach (UtinniTitlebarButton btn in RightTitleBarButtons)
            {
                btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                Controls.Add(btn);

                btn.Location = new Point(Width - rightButtonEdge - btn.Width, 2);
                rightButtonEdge += btn.Width;
            }

            int leftButtonEdge = leftTitlebarOffset;
            foreach (UtinniTitlebarButton btn in LeftTitleBarButtons)
            {
                btn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                Controls.Add(btn);
                btn.Location = new Point(32 + leftButtonEdge, 2);
                leftButtonEdge += btn.Width;
            }

            if (IconImage != null)
            {
                IconImage = new Bitmap(IconImage, 24, 24);
            }
        }

        private void UpdateForeColor()
        {
            foreColorBrush = new SolidBrush(Colors.Font());
            ForeColor = Colors.Font();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color backColor = Colors.Primary();

            e.Graphics.Clear(backColor);

            using (SolidBrush b = new SolidBrush(backColor))
            {
                Rectangle topRect = new Rectangle(0, 0, Width, 5);
                e.Graphics.FillRectangle(b, topRect);
            }

            if (IconImage != null)
            {
                leftTitlebarOffset = 27;
                e.Graphics.DrawImage(IconImage, 5, 5);
            }

            e.Graphics.DrawLine(new Pen(Colors.Secondary(), 2), 0, 0, Width, 0);

            if (DrawName) // ToDo potentially add an offset for this, for when you have left titlebar buttons
            {
                e.Graphics.DrawString(Text, nameFont, foreColorBrush, leftTitlebarOffset + 3, 5, new StringFormat());
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (Resizable)
            {
                if (m.Msg == Native.WM_NCHITTEST || m.Msg == Native.WM_MOUSEMOVE)
                {
                    Point screenPoint = new Point(m.LParam.ToInt32());
                    Point clientPoint = this.PointToClient(screenPoint);

                    Dictionary<Native.WM_HitTests, Rectangle> hitBoxes = new Dictionary<Native.WM_HitTests, Rectangle>()
                    {
                        { Native.WM_HitTests.HTBOTTOM, new Rectangle(resizeBorderHitWidth, Size.Height - resizeBorderHitWidth, Size.Width - 2 * resizeBorderHitWidth, resizeBorderHitWidth) },
                        { Native.WM_HitTests.HTBOTTOMRIGHT, new Rectangle(Size.Width - resizeBorderHitWidth, Size.Height - resizeBorderHitWidth, resizeBorderHitWidth, resizeBorderHitWidth) },
                        { Native.WM_HitTests.HTRIGHT, new Rectangle(Size.Width - resizeBorderHitWidth, resizeBorderHitWidth, resizeBorderHitWidth, Size.Height - 2 * resizeBorderHitWidth) },
                    };

                    foreach (KeyValuePair<Native.WM_HitTests, Rectangle> hitBox in hitBoxes)
                    {
                        if (hitBox.Value.Contains(clientPoint))
                        {
                            m.Result = (IntPtr)hitBox.Key;
                            return;
                        }
                    }
                }
            }
            
            base.WndProc(ref m);
        }

        private Native.WM_HitTests HitTest(IntPtr hwnd, IntPtr wparam, IntPtr lparam)
        {
            Point mousePos = this.PointToClient(new Point((short)lparam, (short)((int)lparam >> 16)));

            if (Resizable)
            {
                if (new Rectangle(Width - 15, Height - 15, 15, 15).Contains(mousePos))
                {
                    return Native.WM_HitTests.HTBOTTOMRIGHT;
                }
            }

            return Native.WM_HitTests.HTCLIENT;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
            {
                if (WindowState != FormWindowState.Maximized && e.Y <= titleBarHeight)
                {
                    Native.ReleaseCapture();
                    Native.SendMessage(this.Handle, Native.WM_SYSCOMMAND, Native.SC_DRAGMOVE, 0);
                }
            }

            base.OnMouseDown(e);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;

                if (EnableShadow)
                {
                    cp.ClassStyle |= Native.CS_DROPSHADOW;
                }

                return cp;
            }
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, EventArgs e)
        {
            UtinniTitlebarButton btn = (UtinniTitlebarButton)sender;
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
                btn.SetImage(new Bitmap(Properties.Resources.maximized));
            }
            else
            {
                WindowState = FormWindowState.Normal;
                btn.SetImage(new Bitmap(Properties.Resources.maximize));
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
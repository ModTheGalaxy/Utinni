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
using System.Drawing;
using System.Windows.Forms;
using UtinniCoreDotNet.UI.Theme;

namespace UtinniCoreDotNet.UI.Controls
{
    public class UndoRedoTitlebarButton : UtinniTitlebarButton
    {
        public readonly UndoRedoToolStripDropDown DropDown;

        public bool IsDropDownClickAreaPressed;
        private const int dropDownClickArea = 15;

        private SolidBrush fontBrush;
        private Pen fontPen;

        public UndoRedoTitlebarButton(Form parentForm, string cmdTypeText, Bitmap image, Action<int> undoRedoCallback) : base(image)
        {
            UpdateColors();
            DropDown = new UndoRedoToolStripDropDown(this, parentForm, cmdTypeText, undoRedoCallback);
            Enabled = false;
        }

        private void UpdateColors()
        {
            fontBrush = new SolidBrush(Colors.Font());
            fontPen = new Pen(Colors.Font(), 1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw the arrow
            e.Graphics.DrawLine(fontPen, Width - 11, 15, Width - 7, 15);
            e.Graphics.DrawLine(fontPen, Width - 10, 16, Width - 8, 16);
            e.Graphics.FillRectangle(fontBrush, Width - 9, 17, 1, 1);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            IsDropDownClickAreaPressed = e.Button == MouseButtons.Left && e.X >= Width - dropDownClickArea;

            base.OnMouseDown(e);
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            fontPen = new Pen(ForeColor, 1);
            fontBrush = new SolidBrush(ForeColor);
        }
    }
}

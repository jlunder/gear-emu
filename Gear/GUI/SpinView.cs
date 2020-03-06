/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * SpinView.cs
 * Spin object viewer class
 * --------------------------------------------------------------------------------
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * --------------------------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using Gear.PluginSupport;
using Gear.EmulationCore;

namespace Gear.GUI
{
    class SpinView : PluginBase
    {
        private const int bytesPerLine = 16;

        private Font MonoSpace;
        private TreeView objectView;
        private DoubleBufferedPanel hexView;
        private ToolStrip toolStrip1;
        private ToolStripButton analyzeButton;
        private VScrollBar scrollPosition;
        private Splitter splitter1;

        private int lineHeight = -1;
        private Brush[] colorBrushes;
        private byte[] colorMap = new byte[PropellerCPU.TOTAL_MEMORY];
        private int highlightStart = 0;
        private int highlightEnd = 0;

        public override string Title
        {
            get
            {
                return "Spin Map";
            }
        }

        public override Boolean IsClosable
        {
            get
            {
                return false;
            }
        }

        public override bool IsUserPlugin
        {
            get
            {
                return false;
            }
        }

        enum MemoryType
        {
            Unknown,
            Config,

        }

        public SpinView(PropellerCPU chip) : base(chip)
        {
            MonoSpace = new Font(FontFamily.GenericMonospace, 8);
            if (MonoSpace == null)
                MonoSpace = Font;

            lineHeight = TextRenderer.MeasureText("0000:", MonoSpace).Height;

            InitializeComponent();

            colorBrushes = new Brush[]
            {
                Brushes.Gray,
                Brushes.White,
                Brushes.LightYellow,
                Brushes.LightGray,
                Brushes.LightBlue,
                Brushes.LightPink,
                Brushes.LavenderBlush,
                Brushes.LightGreen,
                Brushes.Yellow,
            };

            for (int i = 0; i < colorMap.Length; i++)
            {
                colorMap[i] = 0;
            }
        }

        public override void PresentChip()
        {

        }

        private void ColorCode()
        {
            int i;

            objectView.Nodes.Clear();
            TreeNode root = objectView.Nodes.Add("Spin");
            TreeNode node;

            node = root.Nodes.Add(String.Format("System Frequency: {0}mhz", Chip.DirectReadLong(0)));
            node.Tag = Tuple.Create(0, 4);
            node = root.Nodes.Add(String.Format("Clock Mode: {0:X2}", Chip.DirectReadByte(4)));
            node.Tag = Tuple.Create(4, 5);
            node = root.Nodes.Add(String.Format("Check Sum: {0:X2}", Chip.DirectReadByte(5)));
            node.Tag = Tuple.Create(5, 6);
            node = root.Nodes.Add(String.Format("Root Object: {0:X4}", Chip.DirectReadWord(6)));
            node.Tag = Tuple.Create(6, 8);
            node = root.Nodes.Add(String.Format("Variable Base: {0:X4}", Chip.DirectReadWord(8)));
            node.Tag = Tuple.Create(8, 10);
            node = root.Nodes.Add(String.Format("Local Frame: {0:X4}", Chip.DirectReadWord(10)));
            node.Tag = Tuple.Create(10, 12);
            node = root.Nodes.Add(String.Format("Entry PC: {0:X4}", Chip.DirectReadWord(12)));
            node.Tag = Tuple.Create(12, 14);
            node = root.Nodes.Add(String.Format("Starting Stack: {0:X4}", Chip.DirectReadWord(14)));
            node.Tag = Tuple.Create(14, 16);

            for (i = 0; i < 16; i++)
                colorMap[i] = 1;

            for (i = Chip.DirectReadWord(0x8); i < Chip.DirectReadWord(0xA); i++)
                colorMap[i] = 2;

            for (; i < 0x8000; i++)
                colorMap[i] = 3;

            ColorObject(Chip.DirectReadWord(0x6), Chip.DirectReadWord(0x8), root);
        }

        private void ColorObject(uint objFrame, uint varFrame, TreeNode parent)
        {
            uint i, addr, addrnext;

            var objectNode = parent.Nodes.Add(String.Format("Object {0:X}", objFrame));

            objectNode.Nodes.Add(String.Format("Variable Space {0:X4}", varFrame)).Tag =
                Tuple.Create((int)varFrame, (int)varFrame);
            colorMap[varFrame] = 4;

            ushort size = Chip.DirectReadWord(objFrame);
            uint clippedSize = Math.Min(size, (uint)PropellerCPU.TOTAL_MEMORY - objFrame);
            objectNode.Tag = Tuple.Create((int)objFrame, (int)(objFrame + clippedSize));
            byte longs = Chip.DirectReadByte(objFrame + 2);
            byte objects = Chip.DirectReadByte(objFrame + 3);

            for (i = 0; i < longs * 4 && i < clippedSize; i++)
                colorMap[i + objFrame] = 5;
            for (; i < (longs + objects) * 4 && i < clippedSize; i++)
                colorMap[i + objFrame] = 6;
            for (; i + 4 < clippedSize; i++)
                colorMap[i + objFrame] = 7;

            addr = Chip.DirectReadWord(objFrame + 4) + objFrame;
            for (i = 1; i < longs - 1; i++)
            {
                addrnext = Chip.DirectReadWord(objFrame + 4 + i * 4) + objFrame;
                ColorFunction(addr, addrnext, objectNode);
                addr = addrnext;
            }
            if (longs > 0)
            {
                ColorFunction(addr, objFrame + clippedSize, objectNode);
            }

            for (i = 0; i < objects; i++)
                ColorObject(Chip.DirectReadWord((longs + i) * 4 + objFrame) + objFrame,
                    Chip.DirectReadWord((longs + i) * 4 + 2 + objFrame) + varFrame, objectNode);
        }

        private void ColorFunction(uint functFrame, uint functFrameEnd, TreeNode parent)
        {
            var functionNode = parent.Nodes.Add(String.Format("Function {0:X} ({1:d})", functFrame, functFrameEnd - functFrame));
            functionNode.Tag = Tuple.Create((int)functFrame, (int)functFrameEnd);

            for(uint i = functFrame; i < functFrameEnd; ++i)
            {
                colorMap[i] = 8;
            }
        }

        #region FormCode

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpinView));
            this.objectView = new System.Windows.Forms.TreeView();
            this.hexView = new DoubleBufferedPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.analyzeButton = new System.Windows.Forms.ToolStripButton();
            this.scrollPosition = new System.Windows.Forms.VScrollBar();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // objectView
            //
            this.objectView.Dock = System.Windows.Forms.DockStyle.Left;
            this.objectView.Indent = 15;
            this.objectView.Location = new System.Drawing.Point(0, 25);
            this.objectView.Name = "objectView";
            this.objectView.Size = new System.Drawing.Size(193, 424);
            this.objectView.TabIndex = 0;
            this.objectView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SelectChanged);
            //
            // hexView
            //
            this.hexView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hexView.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.hexView.Location = new System.Drawing.Point(193, 25);
            this.hexView.Name = "hexView";
            this.hexView.Size = new System.Drawing.Size(415, 424);
            this.hexView.TabIndex = 1;
            this.hexView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.hexView_MouseClick);
            this.hexView.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.hexView.SizeChanged += new System.EventHandler(this.OnSize);
            //
            // toolStrip1
            //
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.analyzeButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(625, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            //
            // analyzeButton
            //
            this.analyzeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.analyzeButton.Image = ((System.Drawing.Image)(resources.GetObject("analyzeButton.Image")));
            this.analyzeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.analyzeButton.Name = "analyzeButton";
            this.analyzeButton.Size = new System.Drawing.Size(57, 22);
            this.analyzeButton.Text = "Reanalyze";
            this.analyzeButton.Click += new System.EventHandler(this.analyzeButton_Click);
            //
            // scrollPosition
            //
            this.scrollPosition.Dock = System.Windows.Forms.DockStyle.Right;
            this.scrollPosition.SmallChange = 3;
            this.scrollPosition.LargeChange = 16;
            this.scrollPosition.Location = new System.Drawing.Point(608, 25);
            this.scrollPosition.Maximum = 0;
            this.scrollPosition.Name = "scrollPosition";
            this.scrollPosition.Size = new System.Drawing.Size(17, 424);
            this.scrollPosition.TabIndex = 0;
            this.scrollPosition.TabStop = true;
            this.scrollPosition.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);
            //
            // splitter1
            //
            this.splitter1.Location = new System.Drawing.Point(193, 25);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 424);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            //
            // SpinView
            //
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.hexView);
            this.Controls.Add(this.objectView);
            this.Controls.Add(this.scrollPosition);
            this.Controls.Add(this.toolStrip1);
            this.Name = "SpinView";
            this.Size = new System.Drawing.Size(625, 449);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            ASCIIEncoding ascii = new ASCIIEncoding();

            g.Clear(SystemColors.Control);

            var addrSize = TextRenderer.MeasureText("0000:", MonoSpace);
            var addrWidth = addrSize.Width;
            lineHeight = addrSize.Height;
            var byteWidth = TextRenderer.MeasureText("00", MonoSpace).Width;

            int lineBase = Math.Max(scrollPosition.Value, 0) * bytesPerLine;
            var clientRect = hexView.ClientRectangle;
            int yPos = clientRect.Top;
            while (lineBase < PropellerCPU.TOTAL_MEMORY && yPos < clientRect.Height)
            {
                int xPos = clientRect.Left;
                // Draw the address
                g.FillRectangle(Brushes.White, new Rectangle(xPos, yPos, addrWidth, lineHeight));
                g.DrawString(String.Format("{0:X4}:", lineBase), MonoSpace, SystemBrushes.ControlText, xPos, yPos);
                xPos += addrWidth;
                // Draw the line of data
                for (int addr = lineBase; (addr < lineBase + bytesPerLine) && (addr < PropellerCPU.TOTAL_MEMORY); ++addr)
                {
                    string dataString = (Chip != null
                        ? Chip.DirectReadByte((uint)(addr)).ToString("X2")
                        : "--");

                    Brush brush;
                    if(addr >= highlightStart && addr < highlightEnd)
                    {
                        brush = Brushes.Magenta;
                    }
                    else
                    {
                        brush = colorBrushes[colorMap[addr]];
                    }

                    g.FillRectangle(brush, new Rectangle(xPos, yPos, byteWidth, lineHeight));

                    // if (data > 32 && data < 127)
                    // {
                    //    g.DrawString(ascii.GetString(new byte[] { data }), MonoSpace, SystemBrushes.ControlText, dx, dy);
                    // }
                    // else
                    // {
                    g.DrawString(dataString, MonoSpace, SystemBrushes.ControlText, xPos, yPos);
                    // }
                    xPos += byteWidth;
                }

                yPos += lineHeight;
                lineBase += bytesPerLine;
            }
        }

        public override void UpdateGui()
        {
            var totalLines = (PropellerCPU.TOTAL_MEMORY + bytesPerLine - 1) / bytesPerLine;
            var linesPerPage = hexView.ClientRectangle.Height / lineHeight;
            var largeChange = Math.Max(linesPerPage - 3, 1);
            scrollPosition.LargeChange = linesPerPage;
            scrollPosition.SmallChange = Math.Min(3, largeChange);
            scrollPosition.Maximum = totalLines;

            hexView.Invalidate();
        }

        private void SelectChanged(object sender, TreeViewEventArgs e)
        {
            var highlight = objectView.SelectedNode.Tag as Tuple<int, int>;

            if (highlight == null)
            {
                highlightStart = 0;
                highlightEnd = 0;
            }
            else
            {
                var linesPerPage = hexView.Height / lineHeight;

                var pageStart = scrollPosition.Value;
                var pageEnd = pageStart + linesPerPage;
                highlightStart = highlight.Item1;
                highlightEnd = highlight.Item2;
                var highlightStartLine = highlightStart / bytesPerLine;
                var highlightEndLine = (highlightEnd + bytesPerLine - 1) / bytesPerLine;

                // s e
                // 1 1 s
                // 1 2 e
                // 1 3 -
                // 2 1 ?
                // 2 2 -
                // 2 3 s
                // 3 1 ?
                // 3 2 ?
                // 3 3 s

                // Is the newly highlighted area offscreen?
                if (((highlightStartLine <= pageStart) && (highlightEndLine >= pageEnd))
                    || ((highlightStartLine >= pageStart) && (highlightEndLine <= pageEnd)))
                {
                    // Do nothing, already scrolled to the right place
                }
                else if((highlightStartLine > pageEnd) || (highlightEndLine < pageStart))
                {
                    scrollPosition.Value = highlightStartLine;
                }
                else
                {
                    var optionA = highlightStartLine;
                    var optionB = highlightEndLine - (linesPerPage - 1);

                    if (Math.Abs(scrollPosition.Value - optionA) <= Math.Abs(scrollPosition.Value - optionB))
                    {
                        scrollPosition.Value = optionA;
                    }
                    else
                    {
                        scrollPosition.Value = optionB;
                    }
                }
            }

            UpdateGui();
        }

        private void analyzeButton_Click(object sender, EventArgs e)
        {
            ColorCode();
            objectView.ExpandAll();
            UpdateGui();
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            UpdateGui();
        }

        private void OnSize(object sender, EventArgs e)
        {
            UpdateGui();
        }

        private void hexView_MouseClick(object sender, MouseEventArgs e)
        {
            scrollPosition.Focus();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void openStimulusFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}

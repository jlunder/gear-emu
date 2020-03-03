/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * CogView.cs
 * Generic real-time cog information viewer
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Gear.EmulationCore;
using Gear.PluginSupport;

/// @copydoc Gear.GUI
/// 
namespace Gear.GUI
{
    public partial class CogView : Gear.PluginSupport.PluginBase
    {
        private int HostID;
        private Font MonoFont;
        private Font MonoFontBold;
        private uint[] InterpAddress;
        private int  StackMargin = 180;
        private uint LastLine    = 0;       //!< @brief Last line in NativeCog view.
        private uint StringX;
        private uint StringY;
        private Brush StringBrush;
        private bool displayAsHexadecimal;
        private bool useShortOpcodes;

        public override string Title
        {
            get
            {
                return "Cog " + HostID.ToString();
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

        public CogView(int hostId, PropellerCPU chip) : base (chip)
        {
            HostID = hostId;

            InterpAddress = new uint[80];   // Allow for up to 80 lines of displayed interpreted text

            displayAsHexadecimal = false;
            useShortOpcodes = true;

            MonoFont = new Font(FontFamily.GenericMonospace, 10);
            if (MonoFont == null)
                MonoFont = this.Font;

            MonoFontBold = new Font(MonoFont, FontStyle.Bold);

            InitializeComponent();
        }

        public Cog GetViewCog()
        {
            return Chip.GetCog(HostID);
        }

        private void DrawString(Graphics g, string s)
        {
            g.DrawString(s, MonoFont, StringBrush, StringX, StringY);
            StringY += (uint)MonoFont.Height;
        }

        private void PaintBackBufferNative(NativeCog host)
        {
            Graphics g = Graphics.FromImage((Image)BackBuffer);
            g.Clear(SystemColors.Control);
            Brush brush;

            String display;
            uint topLine, bottomLine;
            topLine = 5;
            bottomLine = (uint)((ClientRectangle.Height / MonoFont.Height) - 5);

            for (uint i = (uint)positionScroll.Value, y = 0, line = 1;
                y < ClientRectangle.Height;
                y += (uint)MonoFont.Height, i++, line++)
            {
                if ((i > 0x1FF) || (i < 0))
                    continue;

                uint mem = host[(int)i];

                if (memoryViewButton.Checked)
                {
                    string binary = Convert.ToString((long)mem, 2);

                    while (binary.Length < 32)
                        binary = "0" + binary;

                    display = String.Format("{0:X4}:  {1:X8}   {2}   {1}",
                        i,
                        mem,
                        binary);
                }
                else
                {
                    display = String.Format("{0:X3}:  {2:X8}   {1}",
                        i,
                        InstructionDisassembler.AssemblyText(mem),
                        mem);
                }

                if ((uint)positionScroll.Value + line - 1 == host.BreakPoint)
                    brush = System.Drawing.Brushes.Pink;
                else if ((!followPCButton.Checked) || (line <= topLine) || (line >= bottomLine))
                    brush = SystemBrushes.Control;
                else
                    brush = SystemBrushes.Window;
                g.FillRectangle(brush, 0, y, assemblyPanel.Width, y + MonoFont.Height);


                g.DrawString(
                    display,
                    (host.ProgramCursor == i) ? MonoFontBold : MonoFont,
                    SystemBrushes.ControlText, 0, y);
            }
        }

        private void PaintBackBufferInterpreted(InterpretedCog host)
        {
            Graphics g = Graphics.FromImage((Image)BackBuffer);
            Brush brush;

            g.Clear(SystemColors.Control);

            String display;
            uint topLine, bottomLine;
            topLine = 5;
            bottomLine = (uint)((ClientRectangle.Height / MonoFont.Height) - 5);

            if (memoryViewButton.Checked)
            {
                for (uint i = (uint)positionScroll.Value, y = 0;
                    y < ClientRectangle.Height;
                    y += (uint)MonoFont.Height, i++)
                {
                    if ((i > 0xFFFF) || (i < 0))
                        continue;

                    uint mem = host[(int)i];

                    string binary = Convert.ToString((long)mem, 2);

                    while (binary.Length < 32)
                        binary = "0" + binary;

                    display = String.Format("{0:X4}:  {1:X8}   {2}   ",
                              i, mem, binary);
                    if (displayAsHexadecimal)
                        display = display + String.Format("{0:X8}", mem);
                    else
                        display = display + String.Format("{0}", mem);

                    g.FillRectangle(SystemBrushes.Control, 0, y, assemblyPanel.Width, y + MonoFont.Height);

                    g.DrawString(
                        display,
                        (host.ProgramCursor == i) ? MonoFontBold : MonoFont,
                        SystemBrushes.ControlText, 0, y);
                }
            }
            else
            {
                uint y = 0;

                for (uint i = (uint)positionScroll.Value, line = 1; y < ClientRectangle.Height; y += (uint)MonoFont.Height, line++)
                {
                    if (i > 0xFFFF)
                        continue;

                    uint start = i;

                    Propeller.MemoryManager mem = new Propeller.MemoryManager(Chip, i);
                    string inst = InstructionDisassembler.InterpreterText(mem, displayAsHexadecimal, useShortOpcodes);
                    i = mem.Address;
                    display = String.Format("{0:X4}: ", start);
                    InterpAddress[line] = start;

                    for (uint q = start; q < start + 4; q++)
                    {
                        if (q < i)
                        {
                            byte b = Chip.DirectReadByte(q);
                            display += String.Format(" {0:X2}", b);
                        }
                        else
                            display += "   ";
                    }


                    display += "  " + inst;

                    if (InterpAddress[line] == host.BreakPoint)
                        brush = System.Drawing.Brushes.Pink;
                    else if ((!followPCButton.Checked) || (line <= topLine) || (line >= bottomLine))
                        brush = SystemBrushes.Control;
                    else
                        brush = SystemBrushes.Window;
                    g.FillRectangle(brush, 0, y, assemblyPanel.Width, y + MonoFont.Height);

                    g.DrawString(
                        display,
                        (host.ProgramCursor == start) ? MonoFontBold : MonoFont,
                        SystemBrushes.ControlText, 0, y);
                }

                StringBrush = SystemBrushes.ControlText;
                StringY = 0;
                StringX = (uint)(assemblyPanel.Width - StackMargin);

                DrawString(g, String.Format("@Stk[0] = ${0:X4} {0}", host.Stack));
                DrawString(g, String.Format("@Obj[0] = ${0:X4} {0}", host.Object));
                DrawString(g, String.Format("@Loc[0] = ${0:X4} {0}", host.Local));
                DrawString(g, String.Format("@Var[0] = ${0:X4} {0}", host.Variable));
                g.DrawLine(Pens.Black, assemblyPanel.Width - StackMargin, StringY, assemblyPanel.Width, StringY);
                DrawString(g, String.Format("Caller& = ${0:X4} {0}", Chip.DirectReadWord(host.Local - 8)));
                DrawString(g, String.Format("          ${0:X4} {0}", Chip.DirectReadWord(host.Local - 6)));
                DrawString(g, String.Format("          ${0:X4} {0}", Chip.DirectReadWord(host.Local - 4)));
                DrawString(g, String.Format("Return& = ${0:X4}", Chip.DirectReadWord(host.Local - 2)));
                g.DrawLine(Pens.Black, assemblyPanel.Width - StackMargin, StringY, assemblyPanel.Width, StringY);

                for (uint i = host.Local; i < host.Stack && StringY < ClientRectangle.Height; i += 4)
                {
                    DrawString(g, String.Format("${0:X8}  {0}", (int)Chip.DirectReadLong(i)));
                }
            }
        }

        /// @brief Repaint the Cog state and data.
        /// @param force 
        public override void UpdateGui()
        {
            Cog host = Chip?.GetCog(HostID);

            if (host == null)
            {
                processorStateLabel.Text = "CPU State: Cog is stopped.";
                programCounterLabel.Text = "";
                zeroFlagLabel.Text = "";
                carryFlagLabel.Text = "";
                return;
            }

            positionScroll.Minimum = 0;

            if (host is InterpretedCog) positionScroll.Maximum = 0xFFFF;
            else if (host is NativeCog) positionScroll.Maximum = 0x200;

            positionScroll.LargeChange = 10;
            positionScroll.SmallChange = 1;

            if (positionScroll.Maximum < positionScroll.Value)
                positionScroll.Value = positionScroll.Maximum;

            if (followPCButton.Checked)
            {
                uint topLine, bottomLine;
                topLine = 5;
                bottomLine = (uint)((ClientRectangle.Height / MonoFont.Height) - 5);
                if (host is NativeCog)
                {
                    if (host.ProgramCursor < topLine)
                        positionScroll.Value = 0;
                    else if (host.ProgramCursor - positionScroll.Value >= bottomLine - 1)
                        positionScroll.Value = (int)host.ProgramCursor - (int)topLine;
                    else if (host.ProgramCursor - positionScroll.Value < topLine)
                        positionScroll.Value = (int)host.ProgramCursor - (int)topLine;
                }
                else
                    positionScroll.Value = (int)host.ProgramCursor;
            }

            if (host is NativeCog)
            {
                NativeCog nativeHost = (NativeCog)host;

                OpcodeSize.Visible = false;
                DisplayUnits.Visible = false;
                zeroFlagLabel.Text = "Zero: " + (nativeHost.ZeroFlag ? "True" : "False");
                carryFlagLabel.Text = "Carry: " + (nativeHost.CarryFlag ? "True" : "False");
            }
            else if (host is InterpretedCog)
            {
                zeroFlagLabel.Text = "";
                carryFlagLabel.Text = "";
                OpcodeSize.Visible = true;
                DisplayUnits.Visible = true;
            }

            programCounterLabel.Text = "PC: " + String.Format("{0:X8}", host.ProgramCursor);
            processorStateLabel.Text = "CPU State: " + host.CogState;

            base.UpdateGui();
        }

        private void PaintBackBuffer()
        {
            EnsureBackBuffer(assemblyPanel.Width, assemblyPanel.Height);

            if (BackBufferDirty)
            {
                Cog host = Chip?.GetCog(HostID);
                if (host is NativeCog) PaintBackBufferNative((NativeCog)host);
                else if (host is InterpretedCog) PaintBackBufferInterpreted((InterpretedCog)host);

                BackBufferDirty = false;
            }
        }

        private void UpdateOnScroll(object sender, ScrollEventArgs e)
        {
            UpdateGui();
        }

        private void AssemblyView_Paint(object sender, PaintEventArgs e)
        {
            PaintBackBuffer();

            assemblyPanel.CreateGraphics().DrawImageUnscaled(BackBuffer, 0, 0);
        }

        private void memoryViewButton_Click(object sender, EventArgs e)
        {
            UpdateGui();
        }

        private void assemblyPanel_MouseDown(object sender, MouseEventArgs e)
        {
            int bp = 0;
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //Make sure it's a valid breakpoint environment
                if (Chip == null) return;
                Cog Host = Chip.GetCog(HostID);
                if (Host == null) return;
                //Find the line that was clicked on
                bp = (assemblyPanel.PointToClient(MousePosition).Y / MonoFont.Height);
                //What type of cog?
                if (Host is NativeCog) bp += positionScroll.Value;
                else if (Host is InterpretedCog) bp = (int)InterpAddress[bp + 1];
                //Toggle/move the breakpoint
                if (bp == Host.BreakPoint) Host.BreakPoint = -1;
                else Host.BreakPoint = bp;
                //Show the user what happened
                UpdateGui();
            }
        }

        private void followPCButton_Click(object sender, EventArgs e)
        {
            UpdateGui();
        }

        private void hexadecimalUnits_Click(object sender, EventArgs e)
        {
            displayAsHexadecimal = true;
            hexadecimalUnits.Checked = true;
            decimalUnits.Checked = false;
            UpdateGui();
        }

        private void decimalUnits_Click(object sender, EventArgs e)
        {
            displayAsHexadecimal = false;
            hexadecimalUnits.Checked = false;
            decimalUnits.Checked = true;
            UpdateGui();
        }

        private void longOpcodes_Click(object sender, EventArgs e)
        {
            useShortOpcodes = false;
            longOpcodes.Checked = true;
            shortOpcodes.Checked = false;
            UpdateGui();
        }

        private void shortOpcodes_Click(object sender, EventArgs e)
        {
            useShortOpcodes = true;
            longOpcodes.Checked = false;
            shortOpcodes.Checked = true;
            UpdateGui();
        }

        private void assemblyPanel_MouseClick(object sender, MouseEventArgs e)
        {
            positionScroll.Focus();
        }

        private void assemblyPanel_MouseHover(object sender, EventArgs e)
        {

        }

        private void assemblyPanel_MouseMove(object sender, MouseEventArgs e)
        {
            uint line;
            uint mem;
            if (!(Chip.GetCog(HostID) is NativeCog))
                return;
            NativeCog host = (NativeCog)Chip.GetCog(HostID);
            line = (uint)positionScroll.Value + (uint)(e.Y / MonoFont.Height);
            if (line > 0x1FF)
                return;

            //Update tooltip only if line has change to prevent flickering
            if (line != LastLine)
            {
                mem = host.ReadLong(line);
                toolTip1.SetToolTip(assemblyPanel, String.Format("${0:x3}= ${1:x8}, {1}\n${2:x3}= ${3:x8}, {3}",
                                                                 mem >> 9 & 0x1ff, host.ReadLong(mem >> 9 & 0x1ff),
                                                                 mem      & 0x1ff, host.ReadLong(mem      & 0x1ff))
                );
                LastLine = line;
            }
        }

    }
}

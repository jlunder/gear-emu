/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
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

namespace Gear.GUI
{
    partial class Emulator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Emulator));
            this.controlBar = new System.Windows.Forms.ToolStrip();
            this.openBinaryButton = new System.Windows.Forms.ToolStripButton();
            this.resetEmulatorButton = new System.Windows.Forms.ToolStripButton();
            this.runEmulatorButton = new System.Windows.Forms.ToolStripButton();
            this.stopEmulatorButton = new System.Windows.Forms.ToolStripButton();
            this.stepInstructionButton = new System.Windows.Forms.ToolStripButton();
            this.stepClockButton = new System.Windows.Forms.ToolStripButton();
            this.unpinButton = new System.Windows.Forms.ToolStripButton();
            this.pinButton = new System.Windows.Forms.ToolStripButton();
            this.floatButton = new System.Windows.Forms.ToolStripButton();
            this.openPluginButton = new System.Windows.Forms.ToolStripButton();
            this.pinnedPanel = new System.Windows.Forms.Panel();
            this.documentsTab = new System.Windows.Forms.TabControl();
            this.pinnedSplitter = new Gear.GUI.CollapsibleSplitter();
            this.hubViewSplitter = new Gear.GUI.CollapsibleSplitter();
            this.hubView = new Gear.GUI.HubView();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.controlBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // controlBar
            // 
            this.controlBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openBinaryButton,
            toolStripSeparator1,
            this.resetEmulatorButton,
            this.runEmulatorButton,
            this.stopEmulatorButton,
            this.stepInstructionButton,
            this.stepClockButton,
            toolStripSeparator2,
            this.unpinButton,
            this.pinButton,
            this.floatButton,
            this.openPluginButton});
            this.controlBar.Location = new System.Drawing.Point(215, 0);
            this.controlBar.Name = "controlBar";
            this.controlBar.Size = new System.Drawing.Size(648, 25);
            this.controlBar.TabIndex = 2;
            this.controlBar.Text = "Control Bar";
            // 
            // openBinaryButton
            // 
            this.openBinaryButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.openBinaryButton.Image = ((System.Drawing.Image)(resources.GetObject("openBinaryButton.Image")));
            this.openBinaryButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openBinaryButton.Name = "openBinaryButton";
            this.openBinaryButton.Size = new System.Drawing.Size(37, 22);
            this.openBinaryButton.Text = "Open";
            this.openBinaryButton.Click += new System.EventHandler(this.openBinary_Click);
            // 
            // resetEmulatorButton
            // 
            this.resetEmulatorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.resetEmulatorButton.Image = ((System.Drawing.Image)(resources.GetObject("resetEmulatorButton.Image")));
            this.resetEmulatorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.resetEmulatorButton.Name = "resetEmulatorButton";
            this.resetEmulatorButton.Size = new System.Drawing.Size(39, 22);
            this.resetEmulatorButton.Text = "Reset";
            this.resetEmulatorButton.Click += new System.EventHandler(this.resetEmulator_Click);
            // 
            // runEmulatorButton
            // 
            this.runEmulatorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.runEmulatorButton.Image = ((System.Drawing.Image)(resources.GetObject("runEmulatorButton.Image")));
            this.runEmulatorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runEmulatorButton.Name = "runEmulatorButton";
            this.runEmulatorButton.Size = new System.Drawing.Size(30, 22);
            this.runEmulatorButton.Text = "Run";
            this.runEmulatorButton.Click += new System.EventHandler(this.runEmulator_Click);
            // 
            // stopEmulatorButton
            // 
            this.stopEmulatorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.stopEmulatorButton.Image = ((System.Drawing.Image)(resources.GetObject("stopEmulatorButton.Image")));
            this.stopEmulatorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stopEmulatorButton.Name = "stopEmulatorButton";
            this.stopEmulatorButton.Size = new System.Drawing.Size(33, 22);
            this.stopEmulatorButton.Text = "Stop";
            this.stopEmulatorButton.Click += new System.EventHandler(this.stopEmulator_Click);
            // 
            // stepInstructionButton
            // 
            this.stepInstructionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.stepInstructionButton.Image = ((System.Drawing.Image)(resources.GetObject("stepInstructionButton.Image")));
            this.stepInstructionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stepInstructionButton.Name = "stepInstructionButton";
            this.stepInstructionButton.Size = new System.Drawing.Size(88, 22);
            this.stepInstructionButton.Text = "Step Instruction";
            this.stepInstructionButton.Click += new System.EventHandler(this.stepInstruction_Click);
            // 
            // stepClockButton
            // 
            this.stepClockButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.stepClockButton.Image = ((System.Drawing.Image)(resources.GetObject("stepClockButton.Image")));
            this.stepClockButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stepClockButton.Name = "stepClockButton";
            this.stepClockButton.Size = new System.Drawing.Size(61, 22);
            this.stepClockButton.Text = "Step Clock";
            this.stepClockButton.Click += new System.EventHandler(this.stepEmulator_Click);
            // 
            // unpinButton
            // 
            this.unpinButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.unpinButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.unpinButton.Image = ((System.Drawing.Image)(resources.GetObject("unpinButton.Image")));
            this.unpinButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.unpinButton.Name = "unpinButton";
            this.unpinButton.Size = new System.Drawing.Size(43, 22);
            this.unpinButton.Text = "Unsplit";
            this.unpinButton.Click += new System.EventHandler(this.unpinButton_Click);
            // 
            // pinButton
            // 
            this.pinButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.pinButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.pinButton.Image = ((System.Drawing.Image)(resources.GetObject("pinButton.Image")));
            this.pinButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pinButton.Name = "pinButton";
            this.pinButton.Size = new System.Drawing.Size(31, 22);
            this.pinButton.Text = "Split";
            this.pinButton.Click += new System.EventHandler(this.pinActiveTab_Click);
            // 
            // floatButton
            // 
            this.floatButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.floatButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.floatButton.Image = ((System.Drawing.Image)(resources.GetObject("floatButton.Image")));
            this.floatButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.floatButton.Name = "floatButton";
            this.floatButton.Size = new System.Drawing.Size(35, 22);
            this.floatButton.Text = "Float";
            this.floatButton.Click += new System.EventHandler(this.floatActiveTab_Click);
            // 
            // openPluginButton
            // 
            this.openPluginButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.openPluginButton.Image = ((System.Drawing.Image)(resources.GetObject("openPluginButton.Image")));
            this.openPluginButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openPluginButton.Name = "openPluginButton";
            this.openPluginButton.Size = new System.Drawing.Size(68, 22);
            this.openPluginButton.Text = "Open Plugin";
            this.openPluginButton.Click += new System.EventHandler(this.OpenPlugin_Click);
            // 
            // pinnedPanel
            // 
            this.pinnedPanel.AutoScroll = true;
            this.pinnedPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pinnedPanel.Location = new System.Drawing.Point(215, 436);
            this.pinnedPanel.Name = "pinnedPanel";
            this.pinnedPanel.Size = new System.Drawing.Size(648, 100);
            this.pinnedPanel.TabIndex = 3;
            // 
            // documentsTab
            // 
            this.documentsTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.documentsTab.Location = new System.Drawing.Point(215, 25);
            this.documentsTab.Name = "documentsTab";
            this.documentsTab.SelectedIndex = 0;
            this.documentsTab.Size = new System.Drawing.Size(648, 403);
            this.documentsTab.TabIndex = 5;
            // 
            // pinnedSplitter
            // 
            this.pinnedSplitter.AnimationDelay = 20;
            this.pinnedSplitter.AnimationStep = 20;
            this.pinnedSplitter.BorderStyle3D = System.Windows.Forms.Border3DStyle.Raised;
            this.pinnedSplitter.ControlToHide = this.pinnedPanel;
            this.pinnedSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.pinnedSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pinnedSplitter.ExpandParentForm = false;
            this.pinnedSplitter.Location = new System.Drawing.Point(215, 428);
            this.pinnedSplitter.Name = "collapsibleSplitter1";
            this.pinnedSplitter.TabIndex = 4;
            this.pinnedSplitter.TabStop = false;
            this.pinnedSplitter.UseAnimations = false;
            this.pinnedSplitter.VisualStyle = Gear.GUI.VisualStyles.Mozilla;
            // 
            // hubViewSplitter
            // 
            this.hubViewSplitter.AnimationDelay = 20;
            this.hubViewSplitter.AnimationStep = 20;
            this.hubViewSplitter.BorderStyle3D = System.Windows.Forms.Border3DStyle.Raised;
            this.hubViewSplitter.ControlToHide = this.hubView;
            this.hubViewSplitter.ExpandParentForm = false;
            this.hubViewSplitter.Location = new System.Drawing.Point(207, 0);
            this.hubViewSplitter.Name = "HubSplitter";
            this.hubViewSplitter.TabIndex = 1;
            this.hubViewSplitter.TabStop = false;
            this.hubViewSplitter.UseAnimations = false;
            this.hubViewSplitter.VisualStyle = Gear.GUI.VisualStyles.Mozilla;
            // 
            // hubView
            // 
            this.hubView.Dock = System.Windows.Forms.DockStyle.Left;
            this.hubView.Location = new System.Drawing.Point(0, 0);
            this.hubView.Name = "hubView";
            this.hubView.Size = new System.Drawing.Size(207, 536);
            this.hubView.TabIndex = 6;
            // 
            // Emulator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(863, 536);
            this.Controls.Add(this.documentsTab);
            this.Controls.Add(this.pinnedSplitter);
            this.Controls.Add(this.pinnedPanel);
            this.Controls.Add(this.controlBar);
            this.Controls.Add(this.hubViewSplitter);
            this.Controls.Add(this.hubView);
            this.Name = "Emulator";
            this.Text = "Emulator";
            this.Deactivate += new System.EventHandler(this.OnDeactivate);
            this.controlBar.ResumeLayout(false);
            this.controlBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Gear.GUI.CollapsibleSplitter hubViewSplitter;
        private System.Windows.Forms.ToolStrip controlBar;
        private System.Windows.Forms.Panel pinnedPanel;
        private Gear.GUI.CollapsibleSplitter pinnedSplitter;
        private System.Windows.Forms.TabControl documentsTab;
        private System.Windows.Forms.ToolStripButton openBinaryButton;
        private System.Windows.Forms.ToolStripButton resetEmulatorButton;
        private System.Windows.Forms.ToolStripButton runEmulatorButton;
        private System.Windows.Forms.ToolStripButton stopEmulatorButton;
        private System.Windows.Forms.ToolStripButton stepClockButton;
        private System.Windows.Forms.ToolStripButton pinButton;
        private System.Windows.Forms.ToolStripButton floatButton;
        private System.Windows.Forms.ToolStripButton unpinButton;
        private HubView hubView;
        private System.Windows.Forms.ToolStripButton stepInstructionButton;
        private System.Windows.Forms.ToolStripButton openPluginButton;
    }
}
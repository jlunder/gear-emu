/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * Emulator.cs
 * Emulator window class
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
using System.IO;
using System.Windows.Forms;
using System.Xml;

using Gear.EmulationCore;
using Gear.PluginSupport;

/// @copydoc Gear.GUI
/// 
namespace Gear.GUI
{
    /// @brief View class for PropellerCPU emulator instance.
    /// @details This class implements a view over a propeller emulator, with interface to control 
    /// the chip, like start, go through steps, reset or reload.
    public partial class Emulator : System.Windows.Forms.Form
    {
        private PropellerCPU Chip;          //!< @brief Reference to PropellerCPU running instance.
        private String Source;              //!< @brief Name of Binary program loaded.
        private String LastFileName;        //!< @brief Last file name opened.
        public uint stepInterval;           //!< @brief How many steps to update screen.
        private List<Control> FloatControls;//!< @brief List of floating controls.

        /// @brief Stopwatch to periodically rerun a step of the emulation
        private Timer runTimer;             

        /// @brief Default Constructor.
        /// @param[in] source Binary program loaded (path & name)
        public Emulator(string source)
        {
            Chip = new PropellerCPU(this);
            Source = source;
            FloatControls = new List<Control>();

            InitializeComponent();

            this.Text = "Propeller: " + source;

            // Create default layout
            for (int i = 0; i < PropellerCPU.TOTAL_COGS; i++)  //using constant TOTAL_COGS
                AttachPlugin(new CogView(i, Chip));

            AttachPlugin(new MemoryView(Chip));
            AttachPlugin(new SpinView(Chip));
            AttachPlugin(new LogicProbe.LogicView(Chip));   //changed to LogicProbe be the last tab
            documentsTab.SelectedIndex = 0;

            // TEMPORARY RUN FUNCTION
            runTimer = new Timer();
            runTimer.Interval = 10;
            runTimer.Tick += new EventHandler(RunEmulatorStep);

            hubView.Host = Chip;
            stepInterval = Properties.Settings.Default.UpdateEachSteps;
        }

        /// @brief Get the last binary opened successfully.
        /// 
        public string GetLastBinary
        {
            get
            {
                return LastFileName;
            }
        }

        /// @brief Make a stop on the emulation.
        /// @details This method would be called when a plugin want to stop, for example
        /// when a breakpoint condition is satisfied.
        public void BreakPoint()
        {
            runTimer.Stop();
            UpdateViewGuis();
        }

        /// @brief Include a plugin to a propeller chip instance.
        /// @details Attach a plugin, linking the propeller instance to the plugin, opening a new 
        /// tab window and enabling the close button by plugin's isClosable property.
        /// @param[in] plugin Instance of a Gear.PluginSupport.PluginBase class to be attached.
        private void AttachPlugin(PluginBase plugin)
        {
            Chip.IncludePlugin(plugin);     //include into plugin lists of a PropellerCPU instance
            plugin.PresentChip();       //invoke initial setup of plugin.

            TabPage t = new TabPage(plugin.Title);
            t.Parent = documentsTab;
            plugin.Dock = DockStyle.Fill;
            plugin.Parent = t;
            documentsTab.SelectedTab = t;
            //Maintain the close button availability
            if (plugin.IsClosable)
            {
                closeButton.Enabled = true;
            }
            else
            {
                closeButton.Enabled = false;
            }
        }

        /// @brief Delete a plugin from a propeller chip instance.
        /// @details Delete a plugin from the actives plugins of the propeller instance, 
        /// effectively stopping the plugin. Remove also from pins and clock watch list.
        /// @param[in] plugin Instance of a Gear.PluginSupport.PluginCommon class to be detached.
        /// @since V15.03.26 - Added.
        //Added method to detach a plugin from the active plugin list of the propeller instance.
        private void DetachPlugin(PluginBase plugin)
        {
            if (plugin.IsClosable)      //check if the plugin is able to close, then remove...
            {
                Chip.RemoveOnPins(plugin);  //from pins watch list
                Chip.RemoveOnClock(plugin); //from clock watch list
                Chip.RemovePlugin(plugin);  //from the plugins registered to the propeller emulator
            };
        }

        /// @brief Run the emulator updating the screen between a number of steps.
        /// @details The program property "UpdateEachSteps" gives the number of steps before 
        /// screen repaint.
        /// Adjusting this number in configuration (like increasing the number) enable to obtain 
        /// faster execution at expense of less screen responsiveness.
        /// @param[in] sender Reference to object where event was raised.
        /// @param[in] e Event data arguments.
        private void RunEmulatorStep(object sender, EventArgs e)
        {
            for (uint i = 0; i < stepInterval; i++)
                if (!Chip.Step())
                {
                    runTimer.Stop();
                    break;
                }
            UpdateViewGuis();
        }

        /// @brief Unfloat the tab object.
        /// @param c Tab object.
        public void Unfloat(Control c)
        {
            TabPage tp = new TabPage(c.Text);

            tp.Parent = documentsTab;
            c.Parent = tp;
            c.Dock = DockStyle.Fill;

            FloatControls.Remove(c.Parent);
        }

        /// @brief Load a binary image from file.
        /// @details Generate a new instance of a `PropellerCPU` and load the program from 
        /// the binary.
        public bool OpenFile(string FileName)
        {
            try
            {
                Chip.Initialize(File.ReadAllBytes(FileName));
                LastFileName = FileName;
                UpdateViewGuis();
                return true;
            }
            catch (IOException ioe)
            {
                MessageBox.Show(this,
                    ioe.Message,
                    "Failed to load program binary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return false;
            }
        }

        /// @brief Load a plugin from XML file.
        /// @details Try to open the XML definition for the plugin from the file name given as 
        /// parameter. Then extract information from the XML (class name, auxiliary references 
        /// and source code to compile), trying to compile the C# source code (based on 
        /// Gear.PluginSupport.PluginBase class) and returning the new class instance. If the 
        /// compilation fails, then it opens the plugin editor to show errors and source code.
        /// @param[in] FileName Name and path to the XML plugin file to open
        /// @returns Reference to the new plugin instance (on success) or NULL (on fail).
        public PluginBase LoadPlugin(string FileName)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            XmlReader tr = XmlReader.Create(FileName, settings);
            bool ReadText = false;

            List<string> references = new List<string>();
            string instanceName = "";
            string code = "";

            try
            {

                while (tr.Read())
                {
                    if (tr.NodeType == XmlNodeType.Text && ReadText)
                    {
                        code = tr.Value;
                        ReadText = false;
                    }

                    switch (tr.Name.ToLower())
                    {
                        case "reference":
                            if (!tr.IsEmptyElement)     //prevent empty element generates error
                                references.Add(tr.GetAttribute("name"));
                            break;
                        case "instance":
                            instanceName = tr.GetAttribute("class");
                            break;
                        case "code":
                            ReadText = true;
                            break;
                    }
                }

                //Dynamic load and compile the plugin module as a class, giving the chip 
                // instance as a parameter.
                PluginBase plugin = ModuleCompiler.LoadModule(
                    code, 
                    instanceName, 
                    references.ToArray(), 
                    Chip
                );

                if (plugin == null)     //if it fails...
                {
                    // ...open plugin editor in other window
                    PluginEditor pe = new PluginEditor(false);   
                    pe.OpenFile(FileName, true);
                    pe.MdiParent = this.MdiParent;
                    pe.Show();
                    //the compilation errors are displayed in the error grid
                    ModuleCompiler.EnumerateErrors(pe.EnumErrors);
                    //show the error list
                    pe.ShowErrorGrid(true);
                }
                else               //if success compiling & instantiate the new class...
                {
                    //...add the reference to the plugin list of the emulator instance
                    AttachPlugin(plugin);   
                    Properties.Settings.Default.LastPlugin = FileName;  //update location of last plugin
                    Properties.Settings.Default.Save();
                }

                return plugin;
            }
            catch (Exception ex)
            {
                if ((ex is IOException) | (ex is XmlException))
                {
                    MessageBox.Show(this,
                        ex.Message,
                        "Failed to load program binary",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    return null;
                }
                else throw;
            }
            finally
            {
                tr.Close();
            }
        }

        /// @brief Select binary propeller image to load.
        /// @param[in] sender Reference to object where event was raised.
        /// @param[in] e Event data arguments.
        private void openBinary_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Propeller Runtime Image (*.binary;*.eeprom)|*.binary;" + 
                "*.eeprom|All Files (*.*)|*.*";
            openFileDialog.Title = "Open Propeller Binary...";
            openFileDialog.FileName = Source;

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                OpenFile(openFileDialog.FileName);
        }

        /// @brief Event to reload the whole %Propeller program from binary file.
        /// @details It also reset the %Propeller Chip and all the plugins.
        /// @param[in] sender Reference to object where event was raised.
        /// @param[in] e Event data arguments.
        private void reloadBinary_Click(object sender, EventArgs e)
        {
            OpenFile(LastFileName);
            Chip.Reset();
        }

        /// @todo Document Gear.GUI.Emulator.OnClosed()
        /// 
        protected override void OnClosed(EventArgs e)
        {
            foreach (Control c in FloatControls)
                c.Parent = null;

            FloatControls.Clear();

            base.OnClosed(e);
        }

        /// @brief Repaint the Views, including float windows.
        private void UpdateViewGuis()
        {
            foreach (Control s in FloatControls)
                s.Refresh();

            Control c = pinnedPanel.GetNextControl(null, true);

            if (c != null)
                ((PluginBase)c).UpdateGui();

            if ( (documentsTab.SelectedTab != null) && 
                 ((c = documentsTab.SelectedTab.GetNextControl(null, true)) != null) )
                ((PluginBase)c).UpdateGui();

            hubView.DataChanged();
        }

        /// @brief Event to reset the whole %Propeller Chip.
        /// @param[in] sender Reference to object where event was raised.
        /// @param[in] e Event data arguments.
        private void resetEmulator_Click(object sender, EventArgs e)
        {
            Chip.Reset();
            UpdateViewGuis();
        }

        /// @brief Run only one instruction of the active cog, stopping after executed.
        /// @param[in] sender Reference to object where event was raised.
        /// @param[in] e Event data arguments.
        private void stepEmulator_Click(object sender, EventArgs e)
        {
            Chip.Step();
            UpdateViewGuis();
        }

        /// @brief Close the plugin window and terminate the plugin instance.
        /// @details Not only close the tab window, also detach the plugin from the PropellerCPU 
        /// what uses it.
        /// @param[in] sender Reference to object where event was raised.
        /// @param[in] e Event data arguments.
        private void closeActiveTab_Click(object sender, EventArgs e)
        {
            TabPage tp = documentsTab.SelectedTab;
            PluginBase p = (PluginBase)tp.Controls[0];
            
            if (p != null)          //test if cast to PluginBase works...
            {
                if (p.IsClosable)   //... so, test if we can close the tab 
                {
                    if (documentsTab.SelectedIndex > 0)
                    {
                        //select the previous tab
                        documentsTab.SelectedIndex = documentsTab.SelectedIndex - 1;
                        //tab changing housekeeping for plugin close button
                        documentsTab_Click(this, e);
                        //detach the plugin from the emulator
                        this.DetachPlugin(p);           
                        p.Dispose();
                    }
                    tp.Parent = null;   //delete the reference to plugin
                };
            }
        }

        /// @todo Document Gear.GUI.Emulator.floatActiveTab_Click()
        /// 
        private void floatActiveTab_Click(object sender, EventArgs e)
        {
            TabPage tp = documentsTab.SelectedTab;
            tp.Parent = null;

            FloatedWindow fw = new FloatedWindow(this);

            Control c = tp.GetNextControl(null, true);
            c.Dock = DockStyle.Fill;
            c.Parent = fw;
            c.Text = tp.Text;

            fw.MdiParent = this.MdiParent;
            fw.Show();
            fw.Text = tp.Text + ": " + Source;

            FloatControls.Add(fw);
        }

        /// @todo Document Gear.GUI.Emulator.pinActiveTab_Click()
        /// 
        private void pinActiveTab_Click(object sender, EventArgs e)
        {
            Control oldPin = pinnedPanel.GetNextControl(null, true);

            TabPage tp = documentsTab.SelectedTab;
            tp.Parent = null;

            Control newPin = tp.GetNextControl(null, true);
            newPin.Dock = DockStyle.Fill;
            newPin.Parent = pinnedPanel;
            newPin.Text = tp.Text;

            if (pinnedSplitter.IsCollapsed)
                pinnedSplitter.ToggleState();

            if (oldPin != null)
            {
                tp = new TabPage(oldPin.Text);
                tp.Parent = documentsTab;
                oldPin.Parent = tp;
            }
        }

        /// @todo Document Gear.GUI.Emulator.unpinButton_Click()
        /// 
        private void unpinButton_Click(object sender, EventArgs e)
        {
            Control oldPin = pinnedPanel.GetNextControl(null, true);

            if (oldPin != null)
            {
                TabPage tp = new TabPage(oldPin.Text);
                tp.Parent = documentsTab;
                oldPin.Parent = tp;

                if (!pinnedSplitter.IsCollapsed)
                    pinnedSplitter.ToggleState();
            }
        }

        /// @brief Event to run the emulator freely.
        /// @param[in] sender Reference to the object where this event was called.
        /// @param[in] e Class with the details event.
        private void runEmulator_Click(object sender, EventArgs e)
        {
            runTimer.Start();
        }

        /// @brief Stop the emulation.
        /// @version V15.03.26 - Added the refresh of the screen.
        /// @param[in] sender Reference to the object where this event was called.
        /// @param[in] e Class with the details event.
        private void stopEmulator_Click(object sender, EventArgs e)
        {
            runTimer.Stop();
            UpdateViewGuis(); //added the repaint, to refresh the views
        }

        /// @brief Event to run one instruction in emulator.
        /// @param[in] sender Reference to the object where this event was called.
        /// @param[in] e Class with the details event.
        private void stepInstruction_Click(object sender, EventArgs e)
        {
            if (documentsTab.SelectedTab != null)
            {
                Control c = documentsTab.SelectedTab.GetNextControl(null, true);

                if (c != null && c is CogView)
                {
                    Cog cog = ((CogView)c).GetViewCog();

                    if (cog != null)
                        cog.StepInstruction();
                }
            }

            UpdateViewGuis();
        }

        /// @brief Try to open a plugin, compiling it and attaching to the active 
        /// emulator instance.
        /// @param[in] sender Reference to the object where this event was called.
        /// @param[in] e Class with the details event.
        private void OpenPlugin_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Gear Plug-in (*.xml)|*.xml|All Files (*.*)|*.*";
            dialog.Title = "Open Gear Plug-in...";
            if (!String.IsNullOrEmpty(Properties.Settings.Default.LastPlugin))
                dialog.InitialDirectory = Path.GetDirectoryName(
                    Properties.Settings.Default.LastPlugin);

            if (dialog.ShowDialog(this) == DialogResult.OK)
                LoadPlugin(dialog.FileName);
        }

        /// @brief Event when the Emulator windows begin to close.
        /// @param[in] sender Reference to the object where this event was called.
        /// @param[in] e Class with the details event.
        private void Emulator_FormClosing(object sender, FormClosingEventArgs e)
        {
            Chip.OnClose(sender, e);
        }

        /// @todo Document Gear.GUI.Emulator.OnDeactivate()
        /// 
        private void OnDeactivate(object sender, EventArgs e)
        {
            runTimer.Stop();
        }

        /// @brief Determine availability of close plugin button when tab is changed.
        /// @details Enable close plugin button based on if active tab is subclass of 
        /// Gear.PluginSupport.PluginBase and if that class permit close the window. Typically 
        /// the user plugins enabled it; but the cog window, main memory, logic probe, etc, 
        /// don't allow to close.
        /// @param[in] sender Reference to object where event was raised.
        /// @param[in] e Event data arguments.
        /// @since V14.07.03 - Added.
        private void documentsTab_Click(object sender, EventArgs e)
        {
            TabPage tp = documentsTab.SelectedTab;
            if (tp.Controls[0] is PluginBase)
            {
                PluginBase b = (tp.Controls[0]) as PluginBase;
                if (b.IsClosable)
                    closeButton.Enabled = true;
                else
                    closeButton.Enabled = false;
            }
            else
            {
                closeButton.Enabled = false;
            }
        }

        /// @todo Document Gear.GUI.Emulator.documentsTab_KeyPress()
        /// 
        private void documentsTab_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (ActiveControl is PluginBase)
            {
                PluginBase b = ActiveControl as PluginBase;
                if (b.AllowHotKeys != true)
                    return;
            }
            if ((e.KeyChar == 's') | (e.KeyChar == 'S'))
            {
                if (runTimer.Enabled)
                    runTimer.Stop();
                else
                    stepInstruction_Click(sender, e);
            }
            if ((e.KeyChar == 'r') | (e.KeyChar == 'R'))
            {
                if (!runTimer.Enabled)
                    runTimer.Start();
            }
        }

    }
}

// Reference link to MSCGEN: http://www.mcternan.me.uk/mscgen/
// Reference link to DOXYGEN commands: http://www.stack.nl/~dimitri/doxygen/manual/commands.html
//
/// @defgroup PluginDetails Plugin Loading Details
/// 

/// @ingroup PluginDetails
/// @page PluginLoadingSequencePage Loading Sequence for a Plugin.
/// @par Main Sequence.
/// Sequence of plugin loading, since the user presses the button in the emulator window (ideal 
/// flow case).
/// @anchor PluginLoadingSequenceFig1
/// @par
/// @mscfile "Load plugin Callings-fig1.mcsgen" "Fig.1: Main sequence for a Plugin loading."
/// @par Detail for Registering OnPinChange & OnClock Methods.
/// This is a detail of main sequence of 
/// @ref PluginLoadingSequenceFig1 "\"Fig.1: Main sequence for a Plugin loading.\"", to show 
/// the possible flows of invocations when the program calls the Method `PresentChip()`, but not 
/// from PluginBase; is the method defined in the plugin class derived by the loaded & compiled 
/// plugin class. So the plugin programmer could choose to call or not either `OnClock()` and 
/// `OnPinChange()` derived methods.
/// @anchor PluginLoadingSequenceFig2
/// @par
/// @mscfile "Load plugin Callings-fig2.mcsgen" "Fig.2: details of invocation for Plugin members."
/// 

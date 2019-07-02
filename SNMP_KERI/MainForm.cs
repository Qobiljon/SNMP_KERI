using System;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualBasic;

namespace SNMP_KERI
{
    public partial class MainForm : Form
    {
        #region Variables
        private TopologyVisualizer vis;
        #endregion

        public MainForm()
        {
            Thread.CurrentThread.Name = "GUI thread";

            DateTime nowTimestamp = DateTime.Now;
            DateTime now = new DateTime(year: nowTimestamp.Year, month: nowTimestamp.Month, day: nowTimestamp.Day, hour: nowTimestamp.Hour, minute: 0, second: 0);

            Debug.WriteLine(nowTimestamp.Ticks);
            Debug.WriteLine(now.Ticks);
            Application.Exit();

            InitializeComponent();

            vis = new TopologyVisualizer(topologyPictureBox: topologyPictureBox,
                nodeNonLeftMouseDownDelegate: (node, e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        hoverLabelIpAddress.Text = string.Format("IP: {0}", node?.ipAddress == null ? "N/A" : node?.ipAddress.ToString());
                        hoverLabelMacAddress.Text = string.Format("MAC: {0}", node?.phyAddress == null ? "N/A" : Tools.phyAddr2VisualString(node?.phyAddress));
                        hoverLabelPortA.ForeColor = ((SolidBrush)node?.portA.brush).Color;
                        hoverLabelPortB.ForeColor = ((SolidBrush)node?.portB.brush).Color;
                        hoverLabelPortC.ForeColor = ((SolidBrush)node?.portC.brush).Color;
                        hoverLabelMib.ForeColor = ((SolidBrush)node?.mibBrush).Color;

                        // Change the location so that the Node-Details panel would become visible
                        Point mouseLoc = PointToClient(Cursor.Position);
                        if (mouseLoc.X + hoverPanel.Width > topologyPictureBox.Location.X + topologyPictureBox.Width)
                            mouseLoc.X = topologyPictureBox.Location.X + topologyPictureBox.Width - hoverPanel.Width;
                        if (mouseLoc.Y + hoverPanel.Height > topologyPictureBox.Location.Y + topologyPictureBox.Height)
                            mouseLoc.Y = topologyPictureBox.Location.Y + topologyPictureBox.Height - hoverPanel.Height;
                        hoverPanel.Location = mouseLoc;
                        hoverPanel.Visible = true;
                    }
                },
                nodeNonLeftMouseUpDelegate: (e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        hoverPanel.Visible = false;
                        topologyPictureBox.Refresh();
                    }
                }
            );
            Tools.init(vis);

            // hide hover panel
            hoverPanel.Visible = false;
        }

        private void newConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            vis = new TopologyVisualizer(topologyPictureBox);
            ConfigEditorForm form = new ConfigEditorForm();
            form.ShowDialog(this);
            if (form.resConfigUrl != null)
            {
                vis = new TopologyVisualizer(topologyPictureBox: topologyPictureBox,
                    nodeNonLeftMouseDownDelegate: (node, evt) =>
                    {
                        if (evt.Button == MouseButtons.Right)
                        {
                            hoverLabelIpAddress.Text = string.Format("IP: {0}", node?.ipAddress == null ? "N/A" : node?.ipAddress.ToString());
                            hoverLabelMacAddress.Text = string.Format("MAC: {0}", node?.phyAddress == null ? "N/A" : Tools.phyAddr2VisualString(node?.phyAddress));
                            hoverLabelPortA.ForeColor = ((SolidBrush)node?.portA.brush).Color;
                            hoverLabelPortB.ForeColor = ((SolidBrush)node?.portB.brush).Color;
                            hoverLabelPortC.ForeColor = ((SolidBrush)node?.portC.brush).Color;
                            hoverLabelMib.ForeColor = ((SolidBrush)node?.mibBrush).Color;

                            // Change the location so that the Node-Details panel would become visible
                            Point mouseLoc = PointToClient(Cursor.Position);
                            if (mouseLoc.X + hoverPanel.Width > topologyPictureBox.Location.X + topologyPictureBox.Width)
                                mouseLoc.X = topologyPictureBox.Location.X + topologyPictureBox.Width - hoverPanel.Width;
                            if (mouseLoc.Y + hoverPanel.Height > topologyPictureBox.Location.Y + topologyPictureBox.Height)
                                mouseLoc.Y = topologyPictureBox.Location.Y + topologyPictureBox.Height - hoverPanel.Height;
                            hoverPanel.Location = mouseLoc;
                            hoverPanel.Visible = true;
                        }
                    },
                    nodeNonLeftMouseUpDelegate: (evt) =>
                    {
                        if (evt.Button == MouseButtons.Right)
                        {
                            hoverPanel.Visible = false;
                            topologyPictureBox.Refresh();
                        }
                    }
                );
                vis.importNodeXmls(form.resConfigUrl);
                vis.redrawTopology();
                Tools.init(vis);
                testStartServiceToolStripMenuItem.Enabled = true;
                testStopServiceToolStripMenuItem.Enabled = false;
                MessageBox.Show(this, "Configuration file has been successfully loaded!", "File loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Pick the (xml) configuration file";
            dialog.Filter = "XML File (*.xml)|*.*";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    vis = new TopologyVisualizer(topologyPictureBox: topologyPictureBox,
                        nodeNonLeftMouseDownDelegate: (node, evt) =>
                        {
                            if (evt.Button == MouseButtons.Right)
                            {
                                hoverLabelIpAddress.Text = string.Format("IP: {0}", node?.ipAddress == null ? "N/A" : node?.ipAddress.ToString());
                                hoverLabelMacAddress.Text = string.Format("MAC: {0}", node?.phyAddress == null ? "N/A" : Tools.phyAddr2VisualString(node?.phyAddress));
                                hoverLabelPortA.ForeColor = ((SolidBrush)node?.portA.brush).Color;
                                hoverLabelPortB.ForeColor = ((SolidBrush)node?.portB.brush).Color;
                                hoverLabelPortC.ForeColor = ((SolidBrush)node?.portC.brush).Color;
                                hoverLabelMib.ForeColor = ((SolidBrush)node?.mibBrush).Color;

                                // Change the location so that the Node-Details panel would become visible
                                Point mouseLoc = PointToClient(Cursor.Position);
                                if (mouseLoc.X + hoverPanel.Width > topologyPictureBox.Location.X + topologyPictureBox.Width)
                                    mouseLoc.X = topologyPictureBox.Location.X + topologyPictureBox.Width - hoverPanel.Width;
                                if (mouseLoc.Y + hoverPanel.Height > topologyPictureBox.Location.Y + topologyPictureBox.Height)
                                    mouseLoc.Y = topologyPictureBox.Location.Y + topologyPictureBox.Height - hoverPanel.Height;
                                hoverPanel.Location = mouseLoc;
                                hoverPanel.Visible = true;
                            }
                        },
                        nodeNonLeftMouseUpDelegate: (evt) =>
                        {
                            if (evt.Button == MouseButtons.Right)
                            {
                                hoverPanel.Visible = false;
                                topologyPictureBox.Refresh();
                            }
                        }
                    );
                    vis.importNodeXmls(dialog.FileName);
                    vis.redrawTopology();
                }
                catch (Exception ex)
                {
                    vis = new TopologyVisualizer(topologyPictureBox: topologyPictureBox,
                        nodeNonLeftMouseDownDelegate: (node, evt) =>
                        {
                            if (evt.Button == MouseButtons.Right)
                            {
                                hoverLabelIpAddress.Text = string.Format("IP: {0}", node?.ipAddress == null ? "N/A" : node?.ipAddress.ToString());
                                hoverLabelMacAddress.Text = string.Format("MAC: {0}", node?.phyAddress == null ? "N/A" : Tools.phyAddr2VisualString(node?.phyAddress));
                                hoverLabelPortA.ForeColor = ((SolidBrush)node?.portA.brush).Color;
                                hoverLabelPortB.ForeColor = ((SolidBrush)node?.portB.brush).Color;
                                hoverLabelPortC.ForeColor = ((SolidBrush)node?.portC.brush).Color;
                                hoverLabelMib.ForeColor = ((SolidBrush)node?.mibBrush).Color;

                                // Change the location so that the Node-Details panel would become visible
                                Point mouseLoc = PointToClient(Cursor.Position);
                                if (mouseLoc.X + hoverPanel.Width > topologyPictureBox.Location.X + topologyPictureBox.Width)
                                    mouseLoc.X = topologyPictureBox.Location.X + topologyPictureBox.Width - hoverPanel.Width;
                                if (mouseLoc.Y + hoverPanel.Height > topologyPictureBox.Location.Y + topologyPictureBox.Height)
                                    mouseLoc.Y = topologyPictureBox.Location.Y + topologyPictureBox.Height - hoverPanel.Height;
                                hoverPanel.Location = mouseLoc;
                                hoverPanel.Visible = true;
                            }
                        },
                        nodeNonLeftMouseUpDelegate: (evt) =>
                        {
                            if (evt.Button == MouseButtons.Right)
                            {
                                hoverPanel.Visible = false;
                                topologyPictureBox.Refresh();
                            }
                        }
                    );
                    MessageBox.Show(this, "Bad configuration file has been picked, please try again!", "Bad file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Tools.init(vis);
                testStartServiceToolStripMenuItem.Enabled = true;
                testStopServiceToolStripMenuItem.Enabled = false;
                MessageBox.Show(this, "Configuration file has been successfully loaded!", "File loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show(this, "Load operation has been canceled!", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void testStartServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (vis.IsEmpty)
            {
                MessageBox.Show(this, "You need to load a configuration before starting the Test SNMP service", "Load a configuration first !", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Enabled = false;
            testStartServiceToolStripMenuItem.Enabled = false;
            testStopServiceToolStripMenuItem.Enabled = true;
            Tools.start_snmp_master((nodeId, ipAddress, message, color) =>
            {
                logTextBox.BeginInvoke((ThreadStart)delegate ()
                {
                    if (color == Color.DarkOrange && !warningsCheckBox.Checked)
                        return;
                    else if (color == Color.Red && !errorsCheckBox.Checked)
                        return;
                    if (nodeId == default(int) && ipAddress == default(IPAddress) && message != default(string) && color == default(Color))
                    {
                        logTextBox.AppendText(message = string.Format("{0}\n", message));
                        logTextBox.Select(logTextBox.TextLength - message.Length, message.Length);
                        logTextBox.SelectionColor = Color.DarkBlue;
                        logTextBox.Select(logTextBox.TextLength, 0);
                        return;
                    }

                    logTextBox.AppendText(message = string.Format("Node({0}) ipAddress({1})\tMESSAGE: {2}\n", nodeId, ipAddress.ToString(), message));
                    logTextBox.Select(logTextBox.TextLength - message.Length, message.Length);
                    if (color != default(Color))
                        logTextBox.SelectionColor = color;

                    if (logTextBox.Lines.Length == TopologyVisualizer.MAX_LOG_LINES_COUNT)
                    {
                        logTextBox.ReadOnly = false;
                        logTextBox.Select(0, logTextBox.GetFirstCharIndexFromLine(1));
                        logTextBox.SelectedText = "";
                        logTextBox.ReadOnly = true;
                    }

                    logTextBox.Focus();
                    logTextBox.Select(logTextBox.TextLength, 0);
                });
            },
            delegate ()
            {
                BeginInvoke((ThreadStart)delegate ()
                {
                    Enabled = true;
                    vis.redrawTopology();
                });
            });
        }

        private void testStopServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.stop_snmp_master();
            testStartServiceToolStripMenuItem.Enabled = true;
            testStopServiceToolStripMenuItem.Enabled = false;
        }

        private void editConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Pick the existing (xml) configuration file";
            dialog.Filter = "XML File (*.xml)|*.*";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                vis = new TopologyVisualizer(topologyPictureBox);
                ConfigEditorForm form = new ConfigEditorForm();
                form.loadExistingConfig(dialog.FileName);
                form.ShowDialog(this);
                if (form.resConfigUrl != null)
                {
                    vis = new TopologyVisualizer(topologyPictureBox: topologyPictureBox,
                        nodeNonLeftMouseDownDelegate: (node, evt) =>
                        {
                            if (evt.Button == MouseButtons.Right)
                            {
                                hoverLabelIpAddress.Text = string.Format("IP: {0}", node?.ipAddress == null ? "N/A" : node?.ipAddress.ToString());
                                hoverLabelMacAddress.Text = string.Format("MAC: {0}", node?.phyAddress == null ? "N/A" : Tools.phyAddr2VisualString(node?.phyAddress));
                                hoverLabelPortA.ForeColor = ((SolidBrush)node?.portA.brush).Color;
                                hoverLabelPortB.ForeColor = ((SolidBrush)node?.portB.brush).Color;
                                hoverLabelPortC.ForeColor = ((SolidBrush)node?.portC.brush).Color;
                                hoverLabelMib.ForeColor = ((SolidBrush)node?.mibBrush).Color;

                                // Change the location so that the Node-Details panel would become visible
                                Point mouseLoc = PointToClient(Cursor.Position);
                                if (mouseLoc.X + hoverPanel.Width > topologyPictureBox.Location.X + topologyPictureBox.Width)
                                    mouseLoc.X = topologyPictureBox.Location.X + topologyPictureBox.Width - hoverPanel.Width;
                                if (mouseLoc.Y + hoverPanel.Height > topologyPictureBox.Location.Y + topologyPictureBox.Height)
                                    mouseLoc.Y = topologyPictureBox.Location.Y + topologyPictureBox.Height - hoverPanel.Height;
                                hoverPanel.Location = mouseLoc;
                                hoverPanel.Visible = true;
                            }
                        },
                        nodeNonLeftMouseUpDelegate: (evt) =>
                        {
                            if (evt.Button == MouseButtons.Right)
                            {
                                hoverPanel.Visible = false;
                                topologyPictureBox.Refresh();
                            }
                        }
                    );
                    vis.importNodeXmls(dialog.FileName);
                    vis.redrawTopology();
                    vis.importNodeXmls(form.resConfigUrl);
                    vis.redrawTopology();
                    Tools.init(vis);
                    testStartServiceToolStripMenuItem.Enabled = true;
                    testStopServiceToolStripMenuItem.Enabled = false;
                    MessageBox.Show(this, "Configuration file has been successfully loaded!", "File loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
                MessageBox.Show(this, "Edit operation has been canceled!", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void toggleLogButton_Click(object sender, EventArgs e)
        {
            this.Width = this.Width == 1544 ? 997 : 1544;
        }

        private void clearLogWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logTextBox.ReadOnly = false;
            logTextBox.Clear();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            vis?.redrawTopology();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "This application was created in Networking Systems Lab in Inha University.\n\nIf you have any feedback or suggestion, you can directly contact this application's developer by the email: kobiljon@nsl.inha.ac.kr (Kobiljon Toshnazarov)\n\nThank you =)", "About SNMP KERI v1.1 Application", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void openLastEventLogFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filePath = Path.Combine(Tools.EVT_LOGS_DIR, $"{Tools.openLogStreamStamp}.txt");
            string tmpFilePath = Path.GetTempFileName();

            if (File.Exists(filePath))
            {
                File.Copy(filePath, tmpFilePath, true);
                Process.Start("notepad.exe", tmpFilePath);
            }
            else
                MessageBox.Show(this, "SNMP Log file wasn't created yet, please run the service first", "Log doesn't exist yet!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void openEventLogsDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Tools.EVT_LOGS_DIR);
        }

        private void openLastPacketLogFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filePath = Path.Combine(Tools.SNMP_DATA_DIR, $"{Tools.openLogStreamStamp}.csv");
            string tmpFilePath = Path.GetTempFileName();

            if (File.Exists(filePath))
            {
                File.Copy(filePath, tmpFilePath, true);
                Process.Start("notepad.exe", tmpFilePath);
            }
            else
                MessageBox.Show(this, "SNMP Log file wasn't created yet, please run the service first", "Log doesn't exist yet!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void openPacketLogsDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Tools.SNMP_DATA_DIR);
        }

        private void clearAllLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (testStopServiceToolStripMenuItem.Enabled)
                testStopServiceToolStripMenuItem.PerformClick();

            MessageBox.Show(this, "You are about to ERASE all the SNMP LOGS, please confirm the operation in the next window!", "WARNING! POTENTIALLY DESTRUCTIVE OPERATION!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            string input = Interaction.InputBox("Please input: \"I want to erase all logs\" in order to erase all the existing logs (+ snmp values and logs)", "Please confirm deletion", null);
            if (input != null && input.Equals("I want to erase all logs"))
            {
                Tools.releaseStreams();

                Directory.Delete(Tools.EVT_LOGS_DIR, true);
                Directory.CreateDirectory(Tools.EVT_LOGS_DIR);

                Directory.Delete(Tools.SNMP_DATA_DIR, true);
                Directory.CreateDirectory(Tools.SNMP_DATA_DIR);

                MessageBox.Show(this, "All SNMP logs have been erased", "SNMP logs erased", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show(this, "SNMP erase operation has been canceled!", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void transferFocusToPictureBox(object sender, EventArgs e)
        {
            topologyPictureBox.Focus();
        }
    }
}

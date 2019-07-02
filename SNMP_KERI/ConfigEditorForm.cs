using System;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Threading;
using System.Drawing;

namespace SNMP_KERI
{
    public partial class ConfigEditorForm : Form
    {
        #region Variables
        private TopologyVisualizer vis;
        private bool forceCloseForm;
        internal string resConfigUrl;
        private int logLinesCount;
        private Button[] controlButtons;
        #endregion

        public ConfigEditorForm()
        {
            InitializeComponent();

            this.resConfigUrl = null;
            this.forceCloseForm = false;
            controlButtons = new Button[] { setMacAddrButton, translationButton, setIpAddrButton, setNodeTypeButton, setConnectionsButton };
        }

        internal void loadExistingConfig(string fileName)
        {
            vis = new TopologyVisualizer(topologyPictureBox, logTextBox);
            vis.importNodeXmls(fileName);
            log(key: "config", value: string.Format("xml config file loaded (source: {0})", fileName));
        }

        private void ConfigCreator_Load(object sender, EventArgs e)
        {
            bool editMode = this.vis != null;
            int numOfNodes = -1;

            if (editMode)
                numOfNodes = vis.NumOfNodes;
            else
            {
                string resp = Interaction.InputBox(Prompt: "Please enter the number of nodes", DefaultResponse: null);
                if (!int.TryParse(resp, out numOfNodes))
                {
                    forceCloseForm = true;
                    Close();
                    return;
                }
                if (numOfNodes <= 0)
                {
                    MessageBox.Show(this, "Input must be greater than zero!", "Wrong input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    forceCloseForm = true;
                    Close();
                    return;
                }

                vis = new TopologyVisualizer(topologyPictureBox, logTextBox);
            }

            const short MARGIN = 20;
            int numOfCols = (topologyPictureBox.Width - 2 * MARGIN) / (TopologyVisualizer.NODE_WIDTH + 2 * MARGIN);
            TopologyVisualizer.TopologyNode lastNode = null;
            TopologyVisualizer.TopologyNode[] exisNodes = editMode ? vis.Nodes : null;
            for (int n = 0, r = 0, c = 0; n < numOfNodes; n++)
            {
                short posX = (short)((TopologyVisualizer.NODE_WIDTH + 2 * MARGIN) * c + MARGIN);
                short posY = (short)((TopologyVisualizer.NODE_WIDTH + 2 * MARGIN) * r + MARGIN);
                lastNode = editMode ? exisNodes[n] : vis.addNode(posX, posY);

                if (c == numOfCols)
                {
                    r++;
                    c = 0;
                }
                else
                    c++;
            }

            if (topologyPictureBox.Height < lastNode.endLoc.y + MARGIN)
                topologyPictureBox.Height = lastNode.endLoc.y + MARGIN;

            vis.clickAction = TopologyVisualizer.TpClickAction.MOVE_LOCATION;
            log(key: "action", value: "altering node locations");
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Pick the destination";
            dialog.Filter = "XML File (*.xml)|*.*";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                if (!dialog.FileName.EndsWith(".xml"))
                    dialog.FileName += ".xml";
                vis.exportNodeXmls(dialog.FileName, DateTime.UtcNow.Ticks.ToString());
                log(key: "config file", value: string.Format("xml config file exported (destination: {0})", dialog.FileName));
                MessageBox.Show(this, string.Format("File has been successfully saved!\nFile: {0}", dialog.FileName), "Saved successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (MessageBox.Show(this, "Do you want to load the saved configuration now ?", "Load as a configuration?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    this.forceCloseForm = true;
                    this.resConfigUrl = dialog.FileName;
                    Close();
                }
            }
            else
                MessageBox.Show(this, "File not saved!", "Operation Canceled", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void setIpButton_Click(object sender, EventArgs e)
        {
            foreach (Button button in controlButtons)
                if (button == sender)
                    button.BackColor = Color.FromName("Red");
                else
                    button.BackColor = Color.FromName("Control");
            vis.clickAction = TopologyVisualizer.TpClickAction.SET_NODE_IP;
            log(key: "action", value: "altering IP addresses (IP_ADDRESS) of nodes");
            activeModeLabel.Text = string.Format("ACTION: {0}", "SET IP ADDRESS");
        }

        private void setNodeTypeButton_Click(object sender, EventArgs e)
        {
            foreach (Button button in controlButtons)
                if (button == sender)
                    button.BackColor = Color.FromName("Red");
                else
                    button.BackColor = Color.FromName("Control");
            vis.clickAction = TopologyVisualizer.TpClickAction.SET_NODE_TYPE;
            log(key: "action", value: "altering types of nodes");
            activeModeLabel.Text = string.Format("ACTION: {0}", "SET NODE TYPE");
        }

        private void setMacAddrButton_Click(object sender, EventArgs e)
        {
            foreach (Button button in controlButtons)
                if (button == sender)
                    button.BackColor = Color.FromName("Red");
                else
                    button.BackColor = Color.FromName("Control");
            vis.clickAction = TopologyVisualizer.TpClickAction.SET_NODE_MAC;
            log(key: "action", value: "altering Hardware addresses (MAC_ADDRESS) of nodes");
            activeModeLabel.Text = string.Format("ACTION: {0}", "SET MAC ADDRESS");
        }

        private void setConnectionsButton_Click(object sender, EventArgs e)
        {
            foreach (Button button in controlButtons)
                if (button == sender)
                    button.BackColor = Color.FromName("Red");
                else
                    button.BackColor = Color.FromName("Control");
            vis.clickAction = TopologyVisualizer.TpClickAction.SET_CONNECTIONS;
            log(key: "action", value: "altering connections between (ports of) nodes");
            activeModeLabel.Text = string.Format("ACTION: {0}", "SET PORT CONN.");
        }

        private void translationButton_Click(object sender, EventArgs e)
        {
            foreach (Button button in controlButtons)
                if (button == sender)
                    button.BackColor = Color.FromName("Red");
                else
                    button.BackColor = Color.FromName("Control");
            vis.clickAction = TopologyVisualizer.TpClickAction.MOVE_LOCATION;
            log(key: "action", value: "altering node locations");
            activeModeLabel.Text = string.Format("ACTION: {0}", "MOVE (LOCATION)");
        }

        private void ConfigCreator_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.forceCloseForm && MessageBox.Show(this, "Are you sure you want to close this window?\n\nPlease make sure to save the configuration before closing if needed!", "Confirm closing", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                e.Cancel = true;
        }

        private void log(string key, string value)
        {
            this.logTextBox.AppendText($"[{key.ToUpper()}]:\t{value}{Environment.NewLine}");
            logLinesCount++;
            this.logTextBox.Select(this.logTextBox.TextLength, 0);

            if (logLinesCount == TopologyVisualizer.MAX_LOG_LINES_COUNT)
                this.logTextBox.Text = logTextBox.Text.Remove(0, this.logTextBox.Lines[0].Length + Environment.NewLine.Length);
        }

        private void toggleLogButton_Click(object sender, EventArgs e)
        {
            this.Width = this.Width == 1538 ? 993 : 1538;
        }

        private void ConfigCreator_Shown(object sender, EventArgs e)
        {
            Thread redrawThread = new Thread(() =>
            {
                Thread.Sleep(100);
                vis?.redrawTopology();
                topologyPictureBox.BeginInvoke(new Action(() => { topologyPictureBox.Focus(); }));
            });
            redrawThread.Name = "Topology redraw thread";
            redrawThread.Start();

            translationButton.PerformClick();
        }

        private void transferFocusToPictureBox(object sender, EventArgs e)
        {
            topologyPictureBox.Focus();
        }

        private void nodeFlipRotateManualButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Hover over any node and you can click buttons:\n'R' to rotate the node (90 degrees)\n'V' to flip the node vertically\n'H' to flip the node horizontally", "Keyboard manual for node rotation/flipping");
            topologyPictureBox.Focus();
        }
    }
}

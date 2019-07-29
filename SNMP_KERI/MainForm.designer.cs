namespace SNMP_KERI
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.snmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testStartServiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testStopServiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sNMPLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openLastEventLogFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openEventLogsDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sNMPPacketLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openLastPacketLogFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openPacketLogsDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearAllLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logWindowContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearLogWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewFullLogDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hoverPanel = new System.Windows.Forms.Panel();
            this.hoverLabelLre = new System.Windows.Forms.Label();
            this.hoverLabelMib = new System.Windows.Forms.Label();
            this.hoverLabelPortC = new System.Windows.Forms.Label();
            this.hoverLabelPortB = new System.Windows.Forms.Label();
            this.hoverLabelPortA = new System.Windows.Forms.Label();
            this.hoverLabelMacAddress = new System.Windows.Forms.Label();
            this.hoverLabelIpAddress = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.warningsCheckBox = new System.Windows.Forms.CheckBox();
            this.errorsCheckBox = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.topologyPictureBox = new System.Windows.Forms.PictureBox();
            this.logTextBox = new System.Windows.Forms.RichTextBox();
            this.menuStrip1.SuspendLayout();
            this.logWindowContextMenuStrip.SuspendLayout();
            this.hoverPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topologyPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configurationToolStripMenuItem,
            this.snmpToolStripMenuItem,
            this.logToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1161, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // configurationToolStripMenuItem
            // 
            this.configurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadConfigurationToolStripMenuItem,
            this.newConfigurationToolStripMenuItem,
            this.editConfigurationToolStripMenuItem});
            this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            this.configurationToolStripMenuItem.Size = new System.Drawing.Size(93, 20);
            this.configurationToolStripMenuItem.Text = "Configuration";
            // 
            // loadConfigurationToolStripMenuItem
            // 
            this.loadConfigurationToolStripMenuItem.Name = "loadConfigurationToolStripMenuItem";
            this.loadConfigurationToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.loadConfigurationToolStripMenuItem.Text = "Load existing";
            this.loadConfigurationToolStripMenuItem.Click += new System.EventHandler(this.LoadConfigurationToolStripMenuItem_Click);
            // 
            // newConfigurationToolStripMenuItem
            // 
            this.newConfigurationToolStripMenuItem.Name = "newConfigurationToolStripMenuItem";
            this.newConfigurationToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.newConfigurationToolStripMenuItem.Text = "Create new";
            this.newConfigurationToolStripMenuItem.Click += new System.EventHandler(this.NewConfigurationToolStripMenuItem_Click);
            // 
            // editConfigurationToolStripMenuItem
            // 
            this.editConfigurationToolStripMenuItem.Name = "editConfigurationToolStripMenuItem";
            this.editConfigurationToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.editConfigurationToolStripMenuItem.Text = "Edit existing";
            this.editConfigurationToolStripMenuItem.Click += new System.EventHandler(this.EditConfigurationToolStripMenuItem_Click);
            // 
            // snmpToolStripMenuItem
            // 
            this.snmpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testStartServiceToolStripMenuItem,
            this.testStopServiceToolStripMenuItem});
            this.snmpToolStripMenuItem.Name = "snmpToolStripMenuItem";
            this.snmpToolStripMenuItem.Size = new System.Drawing.Size(92, 20);
            this.snmpToolStripMenuItem.Text = "SNMP Service";
            // 
            // testStartServiceToolStripMenuItem
            // 
            this.testStartServiceToolStripMenuItem.Name = "testStartServiceToolStripMenuItem";
            this.testStartServiceToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.testStartServiceToolStripMenuItem.Text = "Start SNMP service";
            this.testStartServiceToolStripMenuItem.Click += new System.EventHandler(this.TestStartServiceToolStripMenuItem_Click);
            // 
            // testStopServiceToolStripMenuItem
            // 
            this.testStopServiceToolStripMenuItem.Enabled = false;
            this.testStopServiceToolStripMenuItem.Name = "testStopServiceToolStripMenuItem";
            this.testStopServiceToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.testStopServiceToolStripMenuItem.Text = "Stop SNMP service";
            this.testStopServiceToolStripMenuItem.Click += new System.EventHandler(this.TestStopServiceToolStripMenuItem_Click);
            // 
            // logToolStripMenuItem
            // 
            this.logToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sNMPLogsToolStripMenuItem,
            this.sNMPPacketLogsToolStripMenuItem,
            this.clearAllLogsToolStripMenuItem});
            this.logToolStripMenuItem.Name = "logToolStripMenuItem";
            this.logToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.logToolStripMenuItem.Text = "SNMP Logs";
            // 
            // sNMPLogsToolStripMenuItem
            // 
            this.sNMPLogsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openLastEventLogFileToolStripMenuItem,
            this.openEventLogsDirectoryToolStripMenuItem});
            this.sNMPLogsToolStripMenuItem.Name = "sNMPLogsToolStripMenuItem";
            this.sNMPLogsToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.sNMPLogsToolStripMenuItem.Text = "Event logs";
            // 
            // openLastEventLogFileToolStripMenuItem
            // 
            this.openLastEventLogFileToolStripMenuItem.Name = "openLastEventLogFileToolStripMenuItem";
            this.openLastEventLogFileToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.openLastEventLogFileToolStripMenuItem.Text = "Open last log file";
            this.openLastEventLogFileToolStripMenuItem.Click += new System.EventHandler(this.OpenLastEventLogFileToolStripMenuItem_Click);
            // 
            // openEventLogsDirectoryToolStripMenuItem
            // 
            this.openEventLogsDirectoryToolStripMenuItem.Name = "openEventLogsDirectoryToolStripMenuItem";
            this.openEventLogsDirectoryToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.openEventLogsDirectoryToolStripMenuItem.Text = "Open event logs directory";
            this.openEventLogsDirectoryToolStripMenuItem.Click += new System.EventHandler(this.OpenEventLogsDirectoryToolStripMenuItem_Click);
            // 
            // sNMPPacketLogsToolStripMenuItem
            // 
            this.sNMPPacketLogsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openLastPacketLogFileToolStripMenuItem,
            this.openPacketLogsDirectoryToolStripMenuItem});
            this.sNMPPacketLogsToolStripMenuItem.Name = "sNMPPacketLogsToolStripMenuItem";
            this.sNMPPacketLogsToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.sNMPPacketLogsToolStripMenuItem.Text = "Packet logs";
            // 
            // openLastPacketLogFileToolStripMenuItem
            // 
            this.openLastPacketLogFileToolStripMenuItem.Name = "openLastPacketLogFileToolStripMenuItem";
            this.openLastPacketLogFileToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.openLastPacketLogFileToolStripMenuItem.Text = "Open last log file";
            this.openLastPacketLogFileToolStripMenuItem.Click += new System.EventHandler(this.OpenLastPacketLogFileToolStripMenuItem_Click);
            // 
            // openPacketLogsDirectoryToolStripMenuItem
            // 
            this.openPacketLogsDirectoryToolStripMenuItem.Name = "openPacketLogsDirectoryToolStripMenuItem";
            this.openPacketLogsDirectoryToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.openPacketLogsDirectoryToolStripMenuItem.Text = "Open packet logs directory";
            this.openPacketLogsDirectoryToolStripMenuItem.Click += new System.EventHandler(this.OpenPacketLogsDirectoryToolStripMenuItem_Click);
            // 
            // clearAllLogsToolStripMenuItem
            // 
            this.clearAllLogsToolStripMenuItem.Name = "clearAllLogsToolStripMenuItem";
            this.clearAllLogsToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.clearAllLogsToolStripMenuItem.Text = "Clear all logs";
            this.clearAllLogsToolStripMenuItem.Click += new System.EventHandler(this.ClearAllLogsToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(114, 20);
            this.aboutToolStripMenuItem.Text = "About SNMP KERI";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // logWindowContextMenuStrip
            // 
            this.logWindowContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearLogWindowToolStripMenuItem,
            this.viewFullLogDatabaseToolStripMenuItem});
            this.logWindowContextMenuStrip.Name = "logWindowContextMenuStrip";
            this.logWindowContextMenuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.logWindowContextMenuStrip.ShowImageMargin = false;
            this.logWindowContextMenuStrip.Size = new System.Drawing.Size(145, 48);
            // 
            // clearLogWindowToolStripMenuItem
            // 
            this.clearLogWindowToolStripMenuItem.Name = "clearLogWindowToolStripMenuItem";
            this.clearLogWindowToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.clearLogWindowToolStripMenuItem.Text = "Clear log";
            this.clearLogWindowToolStripMenuItem.Click += new System.EventHandler(this.ClearLogWindowToolStripMenuItem_Click);
            // 
            // viewFullLogDatabaseToolStripMenuItem
            // 
            this.viewFullLogDatabaseToolStripMenuItem.Enabled = false;
            this.viewFullLogDatabaseToolStripMenuItem.Name = "viewFullLogDatabaseToolStripMenuItem";
            this.viewFullLogDatabaseToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.viewFullLogDatabaseToolStripMenuItem.Text = "View log database";
            // 
            // hoverPanel
            // 
            this.hoverPanel.BackColor = System.Drawing.SystemColors.Info;
            this.hoverPanel.Controls.Add(this.hoverLabelLre);
            this.hoverPanel.Controls.Add(this.hoverLabelMib);
            this.hoverPanel.Controls.Add(this.hoverLabelPortC);
            this.hoverPanel.Controls.Add(this.hoverLabelPortB);
            this.hoverPanel.Controls.Add(this.hoverLabelPortA);
            this.hoverPanel.Controls.Add(this.hoverLabelMacAddress);
            this.hoverPanel.Controls.Add(this.hoverLabelIpAddress);
            this.hoverPanel.Location = new System.Drawing.Point(616, 13);
            this.hoverPanel.Name = "hoverPanel";
            this.hoverPanel.Size = new System.Drawing.Size(134, 160);
            this.hoverPanel.TabIndex = 17;
            // 
            // hoverLabelLre
            // 
            this.hoverLabelLre.AutoSize = true;
            this.hoverLabelLre.Location = new System.Drawing.Point(3, 137);
            this.hoverLabelLre.Name = "hoverLabelLre";
            this.hoverLabelLre.Size = new System.Drawing.Size(28, 13);
            this.hoverLabelLre.TabIndex = 0;
            this.hoverLabelLre.Text = "LRE";
            // 
            // hoverLabelMib
            // 
            this.hoverLabelMib.AutoSize = true;
            this.hoverLabelMib.Location = new System.Drawing.Point(3, 115);
            this.hoverLabelMib.Name = "hoverLabelMib";
            this.hoverLabelMib.Size = new System.Drawing.Size(26, 13);
            this.hoverLabelMib.TabIndex = 0;
            this.hoverLabelMib.Text = "MIB";
            // 
            // hoverLabelPortC
            // 
            this.hoverLabelPortC.AutoSize = true;
            this.hoverLabelPortC.ForeColor = System.Drawing.Color.DarkGreen;
            this.hoverLabelPortC.Location = new System.Drawing.Point(3, 94);
            this.hoverLabelPortC.Name = "hoverLabelPortC";
            this.hoverLabelPortC.Size = new System.Drawing.Size(50, 13);
            this.hoverLabelPortC.TabIndex = 0;
            this.hoverLabelPortC.Text = "PORT_C";
            // 
            // hoverLabelPortB
            // 
            this.hoverLabelPortB.AutoSize = true;
            this.hoverLabelPortB.ForeColor = System.Drawing.Color.DarkGreen;
            this.hoverLabelPortB.Location = new System.Drawing.Point(3, 73);
            this.hoverLabelPortB.Name = "hoverLabelPortB";
            this.hoverLabelPortB.Size = new System.Drawing.Size(50, 13);
            this.hoverLabelPortB.TabIndex = 0;
            this.hoverLabelPortB.Text = "PORT_B";
            // 
            // hoverLabelPortA
            // 
            this.hoverLabelPortA.AutoSize = true;
            this.hoverLabelPortA.ForeColor = System.Drawing.Color.DarkGreen;
            this.hoverLabelPortA.Location = new System.Drawing.Point(3, 52);
            this.hoverLabelPortA.Name = "hoverLabelPortA";
            this.hoverLabelPortA.Size = new System.Drawing.Size(50, 13);
            this.hoverLabelPortA.TabIndex = 0;
            this.hoverLabelPortA.Text = "PORT_A";
            // 
            // hoverLabelMacAddress
            // 
            this.hoverLabelMacAddress.AutoSize = true;
            this.hoverLabelMacAddress.Location = new System.Drawing.Point(3, 31);
            this.hoverLabelMacAddress.Name = "hoverLabelMacAddress";
            this.hoverLabelMacAddress.Size = new System.Drawing.Size(57, 13);
            this.hoverLabelMacAddress.TabIndex = 0;
            this.hoverLabelMacAddress.Text = "MAC: ###";
            // 
            // hoverLabelIpAddress
            // 
            this.hoverLabelIpAddress.AutoSize = true;
            this.hoverLabelIpAddress.Location = new System.Drawing.Point(3, 10);
            this.hoverLabelIpAddress.Name = "hoverLabelIpAddress";
            this.hoverLabelIpAddress.Size = new System.Drawing.Size(44, 13);
            this.hoverLabelIpAddress.TabIndex = 0;
            this.hoverLabelIpAddress.Text = "IP: ###";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.warningsCheckBox);
            this.panel1.Controls.Add(this.errorsCheckBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 495);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1161, 38);
            this.panel1.TabIndex = 19;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Right;
            this.label1.Font = new System.Drawing.Font("Gulim", 12F);
            this.label1.Location = new System.Drawing.Point(857, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 38);
            this.label1.TabIndex = 4;
            this.label1.Text = "LOGS FILTER:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // warningsCheckBox
            // 
            this.warningsCheckBox.AutoSize = true;
            this.warningsCheckBox.Checked = true;
            this.warningsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.warningsCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.warningsCheckBox.Location = new System.Drawing.Point(987, 0);
            this.warningsCheckBox.Name = "warningsCheckBox";
            this.warningsCheckBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.warningsCheckBox.Size = new System.Drawing.Size(94, 38);
            this.warningsCheckBox.TabIndex = 3;
            this.warningsCheckBox.Text = "WARNINGS";
            this.warningsCheckBox.UseVisualStyleBackColor = true;
            // 
            // errorsCheckBox
            // 
            this.errorsCheckBox.AutoSize = true;
            this.errorsCheckBox.Checked = true;
            this.errorsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.errorsCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.errorsCheckBox.Location = new System.Drawing.Point(1081, 0);
            this.errorsCheckBox.Name = "errorsCheckBox";
            this.errorsCheckBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.errorsCheckBox.Size = new System.Drawing.Size(80, 38);
            this.errorsCheckBox.TabIndex = 1;
            this.errorsCheckBox.Text = "ERRORS";
            this.errorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.topologyPictureBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.logTextBox);
            this.splitContainer1.Size = new System.Drawing.Size(1161, 471);
            this.splitContainer1.SplitterDistance = 614;
            this.splitContainer1.TabIndex = 20;
            // 
            // topologyPictureBox
            // 
            this.topologyPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.topologyPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topologyPictureBox.Location = new System.Drawing.Point(0, 0);
            this.topologyPictureBox.Name = "topologyPictureBox";
            this.topologyPictureBox.Size = new System.Drawing.Size(614, 471);
            this.topologyPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.topologyPictureBox.TabIndex = 2;
            this.topologyPictureBox.TabStop = false;
            // 
            // logTextBox
            // 
            this.logTextBox.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.logTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logTextBox.ContextMenuStrip = this.logWindowContextMenuStrip;
            this.logTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logTextBox.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.logTextBox.Location = new System.Drawing.Point(0, 0);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = new System.Drawing.Size(543, 471);
            this.logTextBox.TabIndex = 15;
            this.logTextBox.Text = "";
            this.logTextBox.TextChanged += new System.EventHandler(this.logTextBox_TextChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1161, 533);
            this.Controls.Add(this.hoverPanel);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(10, 10);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "SNMP KERI";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.logWindowContextMenuStrip.ResumeLayout(false);
            this.hoverPanel.ResumeLayout(false);
            this.hoverPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.topologyPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem snmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testStartServiceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testStopServiceToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip logWindowContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem clearLogWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewFullLogDatabaseToolStripMenuItem;
        private System.Windows.Forms.Panel hoverPanel;
        private System.Windows.Forms.Label hoverLabelMacAddress;
        private System.Windows.Forms.Label hoverLabelIpAddress;
        private System.Windows.Forms.Label hoverLabelLre;
        private System.Windows.Forms.Label hoverLabelMib;
        private System.Windows.Forms.Label hoverLabelPortC;
        private System.Windows.Forms.Label hoverLabelPortB;
        private System.Windows.Forms.Label hoverLabelPortA;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sNMPLogsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sNMPPacketLogsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearAllLogsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openLastEventLogFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openEventLogsDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openLastPacketLogFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openPacketLogsDirectoryToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox topologyPictureBox;
        private System.Windows.Forms.RichTextBox logTextBox;
        private System.Windows.Forms.CheckBox errorsCheckBox;
        private System.Windows.Forms.CheckBox warningsCheckBox;
        private System.Windows.Forms.Label label1;
    }
}


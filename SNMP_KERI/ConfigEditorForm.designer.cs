namespace SNMP_KERI
{
    partial class ConfigEditorForm
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
            this.setIpAddrButton = new System.Windows.Forms.Button();
            this.setMacAddrButton = new System.Windows.Forms.Button();
            this.setConnectionsButton = new System.Windows.Forms.Button();
            this.activeModeLabel = new System.Windows.Forms.Label();
            this.topologyPictureBox = new System.Windows.Forms.PictureBox();
            this.saveTopologyButton = new System.Windows.Forms.Button();
            this.setNodeTypeButton = new System.Windows.Forms.Button();
            this.translationButton = new System.Windows.Forms.Button();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.toggleLogButton = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.topologyPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // setIpAddrButton
            // 
            this.setIpAddrButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.setIpAddrButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.setIpAddrButton.Location = new System.Drawing.Point(1117, 0);
            this.setIpAddrButton.Name = "setIpAddrButton";
            this.setIpAddrButton.Size = new System.Drawing.Size(108, 47);
            this.setIpAddrButton.TabIndex = 2;
            this.setIpAddrButton.Text = "SET IP ADDR.";
            this.setIpAddrButton.UseVisualStyleBackColor = true;
            this.setIpAddrButton.Click += new System.EventHandler(this.setIpButton_Click);
            // 
            // setMacAddrButton
            // 
            this.setMacAddrButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.setMacAddrButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.setMacAddrButton.Location = new System.Drawing.Point(866, 0);
            this.setMacAddrButton.Name = "setMacAddrButton";
            this.setMacAddrButton.Size = new System.Drawing.Size(117, 47);
            this.setMacAddrButton.TabIndex = 3;
            this.setMacAddrButton.Text = "SET MAC ADDR.";
            this.setMacAddrButton.UseVisualStyleBackColor = true;
            this.setMacAddrButton.Click += new System.EventHandler(this.setMacAddrButton_Click);
            // 
            // setConnectionsButton
            // 
            this.setConnectionsButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.setConnectionsButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.setConnectionsButton.Location = new System.Drawing.Point(1350, 0);
            this.setConnectionsButton.Name = "setConnectionsButton";
            this.setConnectionsButton.Size = new System.Drawing.Size(141, 47);
            this.setConnectionsButton.TabIndex = 4;
            this.setConnectionsButton.Text = "SET CONNECTIONS";
            this.setConnectionsButton.UseVisualStyleBackColor = true;
            this.setConnectionsButton.Click += new System.EventHandler(this.setConnectionsButton_Click);
            // 
            // activeModeLabel
            // 
            this.activeModeLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.activeModeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.activeModeLabel.Location = new System.Drawing.Point(0, 0);
            this.activeModeLabel.Name = "activeModeLabel";
            this.activeModeLabel.Size = new System.Drawing.Size(674, 47);
            this.activeModeLabel.TabIndex = 10;
            this.activeModeLabel.Text = "SETTING: [DUMMY]";
            this.activeModeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // topologyPictureBox
            // 
            this.topologyPictureBox.BackColor = System.Drawing.SystemColors.Control;
            this.topologyPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.topologyPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topologyPictureBox.Location = new System.Drawing.Point(0, 0);
            this.topologyPictureBox.Name = "topologyPictureBox";
            this.topologyPictureBox.Size = new System.Drawing.Size(979, 762);
            this.topologyPictureBox.TabIndex = 8;
            this.topologyPictureBox.TabStop = false;
            // 
            // saveTopologyButton
            // 
            this.saveTopologyButton.BackColor = System.Drawing.Color.LightGreen;
            this.saveTopologyButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.saveTopologyButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.saveTopologyButton.Location = new System.Drawing.Point(721, 0);
            this.saveTopologyButton.Name = "saveTopologyButton";
            this.saveTopologyButton.Size = new System.Drawing.Size(145, 47);
            this.saveTopologyButton.TabIndex = 5;
            this.saveTopologyButton.Text = "SAVE TOPOLOGY AS";
            this.saveTopologyButton.UseVisualStyleBackColor = false;
            this.saveTopologyButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // setNodeTypeButton
            // 
            this.setNodeTypeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.setNodeTypeButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.setNodeTypeButton.Location = new System.Drawing.Point(1225, 0);
            this.setNodeTypeButton.Name = "setNodeTypeButton";
            this.setNodeTypeButton.Size = new System.Drawing.Size(125, 47);
            this.setNodeTypeButton.TabIndex = 1;
            this.setNodeTypeButton.Text = "SET NODE TYPE";
            this.setNodeTypeButton.UseVisualStyleBackColor = true;
            this.setNodeTypeButton.Click += new System.EventHandler(this.setNodeTypeButton_Click);
            // 
            // translationButton
            // 
            this.translationButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.translationButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.translationButton.Location = new System.Drawing.Point(983, 0);
            this.translationButton.Name = "translationButton";
            this.translationButton.Size = new System.Drawing.Size(134, 47);
            this.translationButton.TabIndex = 0;
            this.translationButton.Text = "MOVE (LOCATION)";
            this.translationButton.UseVisualStyleBackColor = true;
            this.translationButton.Click += new System.EventHandler(this.translationButton_Click);
            // 
            // logTextBox
            // 
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.logTextBox.Location = new System.Drawing.Point(0, 0);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = new System.Drawing.Size(508, 762);
            this.logTextBox.TabIndex = 11;
            this.logTextBox.Text = "LOG:\r\n";
            // 
            // toggleLogButton
            // 
            this.toggleLogButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.toggleLogButton.Location = new System.Drawing.Point(1125, 293);
            this.toggleLogButton.Name = "toggleLogButton";
            this.toggleLogButton.Size = new System.Drawing.Size(12, 83);
            this.toggleLogButton.TabIndex = 12;
            this.toggleLogButton.UseVisualStyleBackColor = true;
            this.toggleLogButton.Click += new System.EventHandler(this.toggleLogButton_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.topologyPictureBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.logTextBox);
            this.splitContainer1.Size = new System.Drawing.Size(1491, 762);
            this.splitContainer1.SplitterDistance = 979;
            this.splitContainer1.TabIndex = 13;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.saveTopologyButton);
            this.panel1.Controls.Add(this.setMacAddrButton);
            this.panel1.Controls.Add(this.translationButton);
            this.panel1.Controls.Add(this.setIpAddrButton);
            this.panel1.Controls.Add(this.setNodeTypeButton);
            this.panel1.Controls.Add(this.setConnectionsButton);
            this.panel1.Controls.Add(this.activeModeLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 715);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1491, 47);
            this.panel1.TabIndex = 14;
            // 
            // ConfigEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1491, 762);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toggleLogButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigEditorForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Confuguration creator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigCreator_FormClosing);
            this.Load += new System.EventHandler(this.ConfigCreator_Load);
            this.Shown += new System.EventHandler(this.ConfigCreator_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.topologyPictureBox)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button setIpAddrButton;
        private System.Windows.Forms.Button setMacAddrButton;
        private System.Windows.Forms.Button setConnectionsButton;
        private System.Windows.Forms.Label activeModeLabel;
        private System.Windows.Forms.PictureBox topologyPictureBox;
        private System.Windows.Forms.Button saveTopologyButton;
        private System.Windows.Forms.Button setNodeTypeButton;
        private System.Windows.Forms.Button translationButton;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.Button toggleLogButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
    }
}
namespace MetaMoveResize
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
            this.btnSetMetaKey = new System.Windows.Forms.Button();
            this.lblMetaKey = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnSetMetaKey
            // 
            this.btnSetMetaKey.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnSetMetaKey.ForeColor = System.Drawing.Color.Black;
            this.btnSetMetaKey.Location = new System.Drawing.Point(142, 7);
            this.btnSetMetaKey.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSetMetaKey.Name = "btnSetMetaKey";
            this.btnSetMetaKey.Size = new System.Drawing.Size(75, 34);
            this.btnSetMetaKey.TabIndex = 0;
            this.btnSetMetaKey.Tag = "164";
            this.btnSetMetaKey.Text = "164";
            this.btnSetMetaKey.UseVisualStyleBackColor = true;
            this.btnSetMetaKey.Click += new System.EventHandler(this.btnSetMetaKey_Click);
            // 
            // lblMetaKey
            // 
            this.lblMetaKey.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblMetaKey.AutoSize = true;
            this.lblMetaKey.Location = new System.Drawing.Point(8, 16);
            this.lblMetaKey.Name = "lblMetaKey";
            this.lblMetaKey.Size = new System.Drawing.Size(132, 17);
            this.lblMetaKey.TabIndex = 1;
            this.lblMetaKey.Text = "Configured meta key:";
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            this.ClientSize = new System.Drawing.Size(224, 48);
            this.Controls.Add(this.lblMetaKey);
            this.Controls.Add(this.btnSetMetaKey);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Opacity = 0.99D;
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MetaMoveResize";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Deactivate += new System.EventHandler(this.MainForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSetMetaKey;
        private System.Windows.Forms.Label lblMetaKey;
    }
}


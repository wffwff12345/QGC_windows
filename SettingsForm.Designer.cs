namespace UavApp
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtUavIPAddress = new System.Windows.Forms.TextBox();
            this.txtWebSocketIPAddress = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblUavIPAddress = new System.Windows.Forms.Label();
            this.lblWebSocketIPAddress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtUavIPAddress
            // 
            this.txtUavIPAddress.Location = new System.Drawing.Point(12, 35);
            this.txtUavIPAddress.Name = "txtUavIPAddress";
            this.txtUavIPAddress.Size = new System.Drawing.Size(260, 20);
            this.txtUavIPAddress.TabIndex = 0;
            // 
            // txtWebSocketIPAddress
            // 
            this.txtWebSocketIPAddress.Location = new System.Drawing.Point(12, 85);
            this.txtWebSocketIPAddress.Name = "txtWebSocketIPAddress";
            this.txtWebSocketIPAddress.Size = new System.Drawing.Size(260, 20);
            this.txtWebSocketIPAddress.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(116, 121);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(197, 121);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblUavIPAddress
            // 
            this.lblUavIPAddress.AutoSize = true;
            this.lblUavIPAddress.Location = new System.Drawing.Point(12, 19);
            this.lblUavIPAddress.Name = "lblUavIPAddress";
            this.lblUavIPAddress.Size = new System.Drawing.Size(101, 13);
            this.lblUavIPAddress.TabIndex = 4;
            this.lblUavIPAddress.Text = "请输入 UAV_IP 地址:";
            // 
            // lblWebSocketIPAddress
            // 
            this.lblWebSocketIPAddress.AutoSize = true;
            this.lblWebSocketIPAddress.Location = new System.Drawing.Point(12, 69);
            this.lblWebSocketIPAddress.Name = "lblWebSocketIPAddress";
            this.lblWebSocketIPAddress.Size = new System.Drawing.Size(127, 13);
            this.lblWebSocketIPAddress.TabIndex = 5;
            this.lblWebSocketIPAddress.Text = "请输入 WebSocket_IP 地址:";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 156);
            this.Controls.Add(this.lblWebSocketIPAddress);
            this.Controls.Add(this.lblUavIPAddress);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtWebSocketIPAddress);
            this.Controls.Add(this.txtUavIPAddress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "设置 IP 地址";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.TextBox txtUavIPAddress;
        private System.Windows.Forms.TextBox txtWebSocketIPAddress;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblUavIPAddress;
        private System.Windows.Forms.Label lblWebSocketIPAddress;
    }
}
using System;
using System.Windows.Forms;

namespace UavApp
{
    public partial class SettingsForm : Form
    {
        public string UavIPAddress { get; private set; }
        public string WebSocketIPAddress { get; private set; }

        public SettingsForm(string uavIpAddress, string websocketIpAddress)
        {
            InitializeComponent();
            txtUavIPAddress.Text = uavIpAddress;
            txtWebSocketIPAddress.Text = websocketIpAddress;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            UavIPAddress = txtUavIPAddress.Text;
            WebSocketIPAddress = txtWebSocketIPAddress.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
using Serilog;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UavApp.MAVLINK;
using Timer = System.Windows.Forms.Timer;
using WebSocket = WebSocketSharp.WebSocket;
namespace UavApp
{
    public partial class Form1 : Form
    {
        private string UavUrl = "192.168.1.10:8235";

        private int UavPort = 8235;

        private string WebSocketUrl = "223.112.179.125:29031";

        private delegate void SetMsgCallback(string value, string st = "");

        private bool v_bol_hide = true;

        private string channelName = "DeviceTest-001";

        private bool isconnect = false;

        private IContainer components = null;

        private ListBox lst_Msg;

        private Timer tmConnect;

        private ContextMenuStrip contMain;

        private ToolStripMenuItem cms_Show;

        private ToolStripSeparator toolStripSeparator3;

        private ToolStripMenuItem cms_Closed;

        private NotifyIcon notifyIcon1;

        private StatusStrip statusStrip1;

        private ToolStripStatusLabel tss_01;

        private ToolStrip toolStrip1;

        private ToolStripButton tsb_Runing;

        private ToolStripButton tsb_Stop;

        private ToolStripSeparator toolStripSeparator1;

        private ToolStripButton ts_setting;

        private ToolStripSeparator toolStripSeparator4;

        private ToolStripButton tsb_Exit;

        private ToolStripButton tsb_Clear;

        private MavlinkCommunicator _mavlinkCommunicator;

        private WebSocket _webSocket;

        private CancellationTokenSource cancellationTokenSource;

        private CancellationToken cancellationToken;

        private bool stopFalg = false;
        private bool settigFalg = false;
        private int  sysId;

        public Form1()
        {
            InitializeComponent();
            // 订阅Form的Shown事件
            this.Shown += new EventHandler(Form1_Shown);
        }

        // 当Form1显示后触发的事件处理方法
        private void Form1_Shown(object sender, EventArgs e)
        {
            // 调用工具栏按钮的点击事件处理方法
            ts_setting_Click(sender, EventArgs.Empty);
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //string nodeValue = XmlHandler.GetNodeValue("LineNo");
                isconnect = true;
                //tsb_Runing_Click(sender, e);
                //ts_setting_Click(sender, e);
            }
            catch(Exception ex)
            {
                Log.Fatal("异常{0}",ex.Message);
            }
        }

        public void SetMsg(string value, string st = "")
        {
            try
            {
                if (lst_Msg.InvokeRequired)
                {
                    SetMsgCallback method = SetMsg;
                    lst_Msg.Invoke(method, value, st);
                    return;
                }
                if (lst_Msg.Items.Count >= 500)
                {
                    lst_Msg.Items.Clear();
                    lst_Msg.Refresh();
                }
                if (st == "")
                {
                    lst_Msg.Items.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " | " + value);
                }
                else
                {
                    lst_Msg.Items.Add(st + " | " + value);
                }
                lst_Msg.SelectedIndex = lst_Msg.Items.Count - 1;
            }
            catch
            {
            }
        }



        private void tmConnect_Tick(object sender, EventArgs e)
        {

        }



        private void cms_Show_Click(object sender, EventArgs e)
        {
            v_bol_hide = true;
            Show();
            if (base.WindowState == FormWindowState.Minimized)
            {
                base.WindowState = FormWindowState.Normal;
            }
            Activate();
        }

        private void cms_Closed_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("您确定退出【" + Text + "】吗？", "系统提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Dispose();
                Application.Exit();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            v_bol_hide = true;
            Show();
            if (base.WindowState == FormWindowState.Minimized)
            {
                base.WindowState = FormWindowState.Normal;
            }
        }

        private void FormMain_Activated(object sender, EventArgs e)
        {
            if (!v_bol_hide)
            {
                Hide();
                base.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
            else
            {
                base.ShowInTaskbar = true;
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.ShowInTaskbar = false;
            notifyIcon1.Visible = true;
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            try
            {
                if (base.WindowState == FormWindowState.Minimized)
                {
                    Hide();
                    base.ShowInTaskbar = false;
                    notifyIcon1.Visible = true;
                }
            }
            catch
            {
            }
        }

        private async void tsb_Runing_Click(object sender, EventArgs e)
        {
            try
            {
                if (_webSocket == null)
                {
                    //Console.WriteLine($"tsb_Runing_Click stopFalg {stopFalg}");
                    if (stopFalg)
                    {
                        _webSocket = new WebSocket($"ws://{WebSocketUrl}/websocket/{sysId}");
                        _webSocket.Connect();
                        Log.Information("_webSocket ",_webSocket.IsAlive == true ? "连接成功！": "连接失败！"  );
                        string msg = _webSocket.IsAlive == true ? "连接成功！" : "连接失败！";
                        SetMsg($"_webSocket {msg}");
                        _webSocket.OnMessage += (s, ev) =>
                        {
                            //Console.WriteLine($"Received message: {ev.Data}");
                            // Process the received message here
                            if (_mavlinkCommunicator != null)
                            {
                                _mavlinkCommunicator.ReceiveMessage(ev.Data);
                            }
                        };
                        char[] separators = { ':' };

                        string[] parts = UavUrl.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length == 2)
                        {
                            string uavIpAddress = parts[0].Trim();
                            string uavIpPort = parts[1].Trim();
                            _mavlinkCommunicator = new MavlinkCommunicator(uavIpAddress, int.Parse(uavIpPort), lst_Msg); // 替换为你的无人机IP和端口

                            cancellationTokenSource = new CancellationTokenSource();
                            cancellationToken = cancellationTokenSource.Token;
                            //_mavlinkCommunicator._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            //_mavlinkCommunicator._socket.Connect(uavIpAddress, UavPort);
                            _mavlinkCommunicator.cancellationTokenSource = cancellationTokenSource;
                            _mavlinkCommunicator._webSocket = _webSocket;
                            await Task.Run(() => _mavlinkCommunicator.StartListenAsync(), cancellationToken);
                        }
                        stopFalg = !stopFalg;
                    }
                    else
                    {
                        MessageBox.Show("WebSocket正在连接请稍后再试...");
                    }
                }
                else
                {
                    _mavlinkCommunicator._webSocket = _webSocket;
                }
            } catch(Exception ex){
                Log.Fatal("tsb_Runing_Click 运行异常{0}", ex.Message);
            }
        }
        private void MavlinkCommunicator_SysIdReceived(object sender, MavlinkEventArgs e)
        {
            // 处理接收到的 sysid
            // 例如，更新 UI 或进行其他操作
            try {
                if (_webSocket == null)
                {
                    _webSocket = new WebSocket($"ws://{WebSocketUrl}/websocket/{e.SysId}");
                    this.sysId = e.SysId;
                    _webSocket.Connect();
                    Log.Information("_webSocket ", _webSocket.IsAlive == true ? "连接成功！" : "连接失败！");
                    string msg = _webSocket.IsAlive == true ? "连接成功！" : "连接失败！";
                    SetMsg($"_webSocket {msg}");
                    _webSocket.OnMessage += (s, ev) =>
                    {
                        //Console.WriteLine($"Received message: {ev.Data}");
                        // Process the received message here
                        if (_mavlinkCommunicator != null)
                        {
                            _mavlinkCommunicator.ReceiveMessage(ev.Data);
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("MavlinkCommunicator_SysIdReceived 运行异常{0}", ex.Message);
            }
        }
        private void tsb_Stop_Click(object sender, EventArgs e)
        {
            try
            {
                if (_webSocket != null && _webSocket.IsAlive)
                {
                    _webSocket.Close();
                    _webSocket = null;
                    cancellationTokenSource.Cancel();
                    stopFalg = !stopFalg;
                    Console.WriteLine($"tsb_Stop_Click stopFalg {stopFalg}");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("tsb_Stop_Click 运行异常{0}", ex.Message);
            }
        }
        private async void ts_setting_Click(object sender, EventArgs e)
        {
            using (SettingsForm settingsForm = new SettingsForm(UavUrl,WebSocketUrl))
            {
                settingsForm.StartPosition = FormStartPosition.CenterParent; // 在父窗体中央显示
                DialogResult result = settingsForm.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    UavUrl = settingsForm.UavIPAddress;
                    WebSocketUrl = settingsForm.WebSocketIPAddress;
                    // 处理 IP 地址，例如保存到配置文件或更新连接设置
                    //MessageBox.Show($"设置的 UAV IP 地址: {UavUrl}\n设置的 WebSocket IP 地址: {WebSocketUrl}");
                    try
                    {
                        if (_webSocket != null && _webSocket.IsAlive)
                        {
                            _webSocket.Close();
                            _webSocket = null;
                            cancellationTokenSource.Cancel();
                            Console.WriteLine($"tsb_Stop_Click stopFalg {stopFalg}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Fatal("tsb_Stop_Click 运行异常{0}", ex.Message);
                    }
                    char[] separators = { ':' };

                    string[] parts = UavUrl.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 2)
                    {
                        string uavIpAddress = parts[0].Trim();
                        string uavIpPort = parts[1].Trim();
                        _mavlinkCommunicator = new MavlinkCommunicator(uavIpAddress, int.Parse(uavIpPort), lst_Msg); // 替换为你的无人机IP和端口
                        cancellationTokenSource = new CancellationTokenSource();
                        cancellationToken = cancellationTokenSource.Token;
                        //_mavlinkCommunicator._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        //_mavlinkCommunicator._socket.Connect(uavIpAddress, UavPort);
                        _mavlinkCommunicator.cancellationTokenSource = cancellationTokenSource;
                        _mavlinkCommunicator.MavlinkMessageReceived += MavlinkCommunicator_SysIdReceived;
                        await Task.Run(() => _mavlinkCommunicator.StartListenAsync(), cancellationToken);
                    }
                }
            }

        }

        private void tsb_Exit_Click(object sender, EventArgs e)
        {
            //Close();
            if (MessageBox.Show("您确定退出【" + Text + "】吗？", "系统提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Dispose();
                Application.Exit();
            }
        }

        private void tsb_Clear_Click(object sender, EventArgs e)
        {
            lst_Msg.Items.Clear();
            lst_Msg.Refresh();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lst_Msg = new System.Windows.Forms.ListBox();
            this.tmConnect = new System.Windows.Forms.Timer(this.components);
            this.contMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cms_Show = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.cms_Closed = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tss_01 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsb_Runing = new System.Windows.Forms.ToolStripButton();
            this.tsb_Stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ts_setting = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsb_Exit = new System.Windows.Forms.ToolStripButton();
            this.tsb_Clear = new System.Windows.Forms.ToolStripButton();
            this.contMain.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lst_Msg
            // 
            this.lst_Msg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lst_Msg.FormattingEnabled = true;
            this.lst_Msg.ItemHeight = 12;
            this.lst_Msg.Location = new System.Drawing.Point(0, 27);
            this.lst_Msg.Name = "lst_Msg";
            this.lst_Msg.Size = new System.Drawing.Size(864, 498);
            this.lst_Msg.TabIndex = 1;
            this.lst_Msg.SelectedIndexChanged += new System.EventHandler(this.lst_Msg_SelectedIndexChanged);
            // 
            // tmConnect
            // 
            this.tmConnect.Interval = 10000;
            this.tmConnect.Tick += new System.EventHandler(this.tmConnect_Tick);
            // 
            // contMain
            // 
            this.contMain.BackColor = System.Drawing.Color.White;
            this.contMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cms_Show,
            this.toolStripSeparator3,
            this.cms_Closed});
            this.contMain.Name = "contextMenuStrip1";
            this.contMain.Size = new System.Drawing.Size(134, 70);
            // 
            // cms_Show
            // 
            this.cms_Show.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cms_Show.Name = "cms_Show";
            this.cms_Show.Size = new System.Drawing.Size(133, 30);
            this.cms_Show.Text = "打开...";
            this.cms_Show.Click += new System.EventHandler(this.cms_Show_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(130, 6);
            // 
            // cms_Closed
            // 
            this.cms_Closed.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cms_Closed.Name = "cms_Closed";
            this.cms_Closed.Size = new System.Drawing.Size(133, 30);
            this.cms_Closed.Text = "关闭...";
            this.cms_Closed.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.cms_Closed.Click += new System.EventHandler(this.cms_Closed_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contMain;
            this.notifyIcon1.Icon = global::UavApp.Properties.Resources.Subscribe;
            this.notifyIcon1.Text = "QGC服务控制台";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tss_01});
            this.statusStrip1.Location = new System.Drawing.Point(0, 525);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(864, 22);
            this.statusStrip1.TabIndex = 13;
            // 
            // tss_01
            // 
            this.tss_01.Name = "tss_01";
            this.tss_01.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsb_Runing,
            this.tsb_Stop,
            this.toolStripSeparator1,
            this.ts_setting,
            this.toolStripSeparator4,
            this.tsb_Exit,
            this.tsb_Clear});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(864, 27);
            this.toolStrip1.TabIndex = 14;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsb_Runing
            // 
            this.tsb_Runing.BackColor = System.Drawing.SystemColors.Control;
            this.tsb_Runing.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tsb_Runing.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_Runing.Name = "tsb_Runing";
            this.tsb_Runing.Size = new System.Drawing.Size(41, 24);
            this.tsb_Runing.Text = "运行";
            this.tsb_Runing.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsb_Runing.Click += new System.EventHandler(this.tsb_Runing_Click);
            // 
            // tsb_Stop
            // 
            this.tsb_Stop.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tsb_Stop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_Stop.Name = "tsb_Stop";
            this.tsb_Stop.Size = new System.Drawing.Size(41, 24);
            this.tsb_Stop.Text = "停止";
            this.tsb_Stop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsb_Stop.Click += new System.EventHandler(this.tsb_Stop_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // ts_setting
            // 
            this.ts_setting.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ts_setting.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ts_setting.Name = "ts_Err";
            this.ts_setting.Size = new System.Drawing.Size(41, 24);
            this.ts_setting.Text = "设置";
            this.ts_setting.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ts_setting.Click += new System.EventHandler(this.ts_setting_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 27);
            // 
            // tsb_Exit
            // 
            this.tsb_Exit.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tsb_Exit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_Exit.Name = "tsb_Exit";
            this.tsb_Exit.Size = new System.Drawing.Size(41, 24);
            this.tsb_Exit.Text = "退出";
            this.tsb_Exit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsb_Exit.Click += new System.EventHandler(this.tsb_Exit_Click);

            // 
            // tsb_Clear
            // 
            this.tsb_Clear.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tsb_Clear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_Clear.Name = "tsb_Clear";
            this.tsb_Clear.Size = new System.Drawing.Size(41, 24);
            this.tsb_Clear.Text = "清屏";
            this.tsb_Clear.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsb_Clear.Click += new System.EventHandler(this.tsb_Clear_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(864, 547);
            this.Controls.Add(this.lst_Msg);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Icon = global::UavApp.Properties.Resources.Subscribe;
            this.Name = "Form1";
            this.Text = "UavApp";
            this.Activated += new System.EventHandler(this.FormMain_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            this.contMain.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void lst_Msg_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

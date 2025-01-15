using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using WebSocketSharp;
using static MAVLink;

namespace UavApp.MAVLINK
{
    public class MavlinkCommunicator
    {
        private delegate void SetMsgCallback(string value, string st = "");
        private ListBox lst_Msg;
        private const double LatLonConversionFactor = 1e-7;
        public Socket _socket;
        private MavlinkParse _parser;
        private byte[] _buffer;
        private int _bufferSize;
        private int _bufferOffset;
        public WebSocket _webSocket;
        private UavVehicleModel uavVehicleModel;
        public CancellationTokenSource cancellationTokenSource;
        public event EventHandler<MavlinkEventArgs> MavlinkMessageReceived;
        private byte sysid;
        private int seq = 0;
        private List<LatLngDto> wayPoints = new List<LatLngDto>();

        internal List<LatLngDto> WayPoints { get => wayPoints; set => wayPoints = value; }

        public MavlinkCommunicator(string hostname, int port, ListBox lst_Msg)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _parser = new MavlinkParse();
            _buffer = new byte[1024];
            _bufferSize = 0;
            _bufferOffset = 0;
            this.lst_Msg = lst_Msg;
            uavVehicleModel = new UavVehicleModel();
            try
            {
                _socket.Connect(hostname, port);
            } catch(SocketException e){
                MessageBox.Show(e.Message);
            }
        }

        public void StartListenAsync()
        {
            while (true)
            {
                try
                {
                    if (cancellationTokenSource.IsCancellationRequested) {
                        Console.WriteLine($"Task cancel");
                        CloseConnection();
                        break;
                    }
                    int bytesReceived = _socket.Receive(_buffer, _bufferSize, _buffer.Length - _bufferSize, SocketFlags.None);
                    if (bytesReceived == 0)
                    {
                        // 连接已关闭
                        Console.WriteLine("Socket connection closed by remote host.");
                        break;
                    }

                    _bufferSize += bytesReceived;

                    try
                    {
                        using (MemoryStream ms = new MemoryStream(_buffer, 0, _bufferSize))
                        {
                            MAVLinkMessage msg;
                            try
                            {
                                while ((msg = _parser.ReadPacket(ms)) != null)
                                {
                                    HandlePacket(msg);
                                    _bufferOffset = (int)ms.Position;
                                }
                            }
                            catch (EndOfStreamException ex)
                            {
                                Console.WriteLine($"EndOfStreamException: {ex.Message}");
                                Log.Fatal("EndOfStreamException MemoryStream 运行异常{0}", ex.Message);
                                // 处理不完整数据包的情况
                                // 重新启动接收操作以继续接收数据
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    // 将剩余的数据移动到缓冲区的开头
                    if (_bufferOffset < _bufferSize)
                    {
                        Buffer.BlockCopy(_buffer, _bufferOffset, _buffer, 0, _bufferSize - _bufferOffset);
                        _bufferSize -= _bufferOffset;
                        _bufferOffset = 0;
                    }
                    else
                    {
                        _bufferSize = 0;
                        _bufferOffset = 0;
                    }
                  /*  if(_webSocket != null && _webSocket.IsAlive)
                    {
                        _webSocket.OnMessage += (ButtonRenderer, e) =>
                        {
                            SetMsg($"接收到的数据：{e.ToString()}");
                            ReceiveMessage(e.Data);
                        };
                    }*/
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Socket exception: {ex.Message}");
                    Log.Fatal("SocketException 运行异常{0}", ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    Log.Fatal("StartListenAsync Exception 运行异常{0}", ex.Message);
                    break;
                }
            }
            CloseConnection();
        }



        private void HandlePacket(MAVLinkMessage message)
        {
            //Console.WriteLine($"Received packet with ID: {message.msgid} Received packet with data: {message.data}");
            Log.Information($"Received mavlink data: {JsonConvert.SerializeObject(message.data)}");
            switch (message.msgid)
            {
                case (uint)MAVLINK_MSG_ID.HEARTBEAT:
                    mavlink_heartbeat_t mavlink_heartbeat_t = (mavlink_heartbeat_t)message.data;
                    uavVehicleModel.customMode = mavlink_heartbeat_t.custom_mode;
                    uavVehicleModel.vehicleId = message.sysid;
                    sysid = message.sysid;
                    if(_webSocket == null)
                    {
                        OnMavlinkMessageReceived(sysid);
                    }
                    if (uavVehicleModel != null && uavVehicleModel.GetVehicleSoc() > 0)
                    {
                        // 发消息
                        if (_webSocket != null && _webSocket.IsAlive)
                        {
                            uavVehicleModel.vehicleId = sysid;
                            string uav = JsonConvert.SerializeObject(uavVehicleModel);
                            _webSocket.Send($"vehicleInfo#{uav}");
                            SetMsg($"发送的数据：{uav}");
                            Console.WriteLine($"_webSocket send msg {uavVehicleModel.ToString()}");
                        }
                    }
                    break;
                case (uint)MAVLINK_MSG_ID.GLOBAL_POSITION_INT:
                    mavlink_global_position_int_t mavlink_global_position_int_t = (mavlink_global_position_int_t)message.data;
                    if (mavlink_global_position_int_t.alt != 0)
                    {
                        uavVehicleModel.vehicleAlt =(double) mavlink_global_position_int_t.alt / 1000;
                    }
                    if (mavlink_global_position_int_t.lon > 0 && mavlink_global_position_int_t.lat > 0)
                    {
                        uavVehicleModel.vehicleLat =(double) mavlink_global_position_int_t.lat * LatLonConversionFactor;
                        uavVehicleModel.vehicleLong =(double) mavlink_global_position_int_t.lon * LatLonConversionFactor;
                    }
                    break;
                case (uint)MAVLINK_MSG_ID.BATTERY_STATUS:
                    mavlink_battery_status_t mavlink_battery_status_t = (mavlink_battery_status_t)message.data;
                    uavVehicleModel.vehicleSoc =  mavlink_battery_status_t.battery_remaining;
                    break;
                case (uint)MAVLINK_MSG_ID.COMMAND_ACK:
                    mavlink_command_ack_t msg_command_ack = (mavlink_command_ack_t)message.data;
                    Console.WriteLine($"Received mavlink_command_ack_t data: {JsonConvert.SerializeObject(msg_command_ack)}");
                    Log.Information($"Received mavlink_command_ack_t data: {JsonConvert.SerializeObject(msg_command_ack)}");
                    break;
                case (uint)MAVLINK_MSG_ID.MISSION_REQUEST:
                    mavlink_mission_request_t mission_request_t = (mavlink_mission_request_t)message.data;
                    Console.WriteLine($"Received mavlink_mission_request_t data: {JsonConvert.SerializeObject(mission_request_t)}");
                    Log.Information($"Received mavlink_mission_request_t data: {JsonConvert.SerializeObject(mission_request_t)}");
                    break;
                case (uint)MAVLINK_MSG_ID.MISSION_ACK:
                    mavlink_mission_ack_t mission_ack_t = (mavlink_mission_ack_t)message.data;
                    Console.WriteLine($"Received mavlink_mission_ack_t data: {JsonConvert.SerializeObject(mission_ack_t)}");
                    Log.Information($"Received mavlink_mission_ack_t data: {JsonConvert.SerializeObject(mission_ack_t)}");
                    break;
                // 处理其他消息类型
                default:
                    //Console.WriteLine($"Received data: {JsonConvert.SerializeObject(message.data)}");
                    break;
            }
        }
        public void CloseConnection()
        {
            if (_socket != null && _socket.Connected)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }

        public void ReceiveMessage(string message)
        {
            /*byte[] packetBytes = packet.pack().encodePacket();
            message.
            _socket.Send(packetBytes);*/
            SetMsg($"接收到的数据：{message}");

            char[] separators = { '#' };
            string[] parts = message.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (parts[0].Equals("LOCKORUNLOCK"))
            {
                mavlink_command_long_t mavlink_command = new mavlink_command_long_t();
                mavlink_command.command = (ushort)MAV_CMD.COMPONENT_ARM_DISARM;
                mavlink_command.param1 = float.Parse(parts[1]);
                // 生成 MAVLink 2.0 数据包
                byte[] packetBytes = _parser.GenerateMAVLinkPacket20(MAVLINK_MSG_ID.COMMAND_LONG, mavlink_command,false,sysid);
                // 发送数据包
                _socket.Send(packetBytes);
            } 
            else if (parts[0].Equals("MODE")){
                mavlink_set_mode_t mavlink_mode = new mavlink_set_mode_t();
                mavlink_mode.base_mode = 1;
                mavlink_mode.custom_mode = uint.Parse(parts[1]);
                // 生成 MAVLink 2.0 数据包
                byte[] packetBytes = _parser.GenerateMAVLinkPacket20(MAVLINK_MSG_ID.SET_MODE, mavlink_mode, false, sysid);
                // 发送数据包
                _socket.Send(packetBytes);
            } 
            else if(parts[0].Equals("TAKEOFF")){
                mavlink_command_long_t mavlink_command = new mavlink_command_long_t();
                mavlink_command.command = (ushort)MAV_CMD.TAKEOFF;
                mavlink_command.param7 = float.Parse(parts[1]);
                // 生成 MAVLink 2.0 数据包
                byte[] packetBytes = _parser.GenerateMAVLinkPacket20(MAVLINK_MSG_ID.COMMAND_LONG, mavlink_command, false, sysid);
                // 发送数据包
                _socket.Send(packetBytes);
            }
            else if (parts[0].Equals("LAND"))
            {
                mavlink_command_long_t mavlink_command = new mavlink_command_long_t();
                mavlink_command.command = (ushort)MAV_CMD.LAND;
                // 生成 MAVLink 2.0 数据包
                byte[] packetBytes = _parser.GenerateMAVLinkPacket20(MAVLINK_MSG_ID.COMMAND_LONG, mavlink_command, false, sysid);
                // 发送数据包
                _socket.Send(packetBytes);
            }
            else if (parts[0].Equals("LAUNCH"))
            {
                mavlink_command_long_t mavlink_command = new mavlink_command_long_t();
                mavlink_command.command = (ushort)MAV_CMD.RETURN_TO_LAUNCH;
                // 生成 MAVLink 2.0 数据包
                byte[] packetBytes = _parser.GenerateMAVLinkPacket20(MAVLINK_MSG_ID.COMMAND_LONG, mavlink_command, false, sysid);
                // 发送数据包
                _socket.Send(packetBytes);
            }
            else if (parts[0].Equals("ADDMISSION"))
            {
                addMissions();
            }
            else if (parts[0].Equals("STARTMISSION"))
            {
                mavlink_command_long_t mavlink_command = new mavlink_command_long_t();
                mavlink_command.command = (ushort)MAV_CMD.MISSION_START;
                mavlink_command.param1 = 0;
                mavlink_command.param2 = 0;
                // 生成 MAVLink 2.0 数据包
                byte[] packetBytes = _parser.GenerateMAVLinkPacket20(MAVLINK_MSG_ID.COMMAND_LONG, mavlink_command, false, sysid);
                // 发送数据包
                _socket.Send(packetBytes);
                seq = 0;
            }
        }

        private void addMissions()
        {
            mavlink_mission_count_t missionCount = new mavlink_mission_count_t();
            missionCount.count = (ushort)wayPoints.Count;
            byte[] packetBytes = _parser.GenerateMAVLinkPacket20(MAVLINK_MSG_ID.MISSION_COUNT, missionCount, false, sysid);
            // 发送数据包
            _socket.Send(packetBytes);
            for (int i = 0; i < wayPoints.Count; i++)
            {
                addMission(_socket, wayPoints[i]);
            }
        }

        private void addMission(Socket socket, LatLngDto latLngDto)
        {
            mavlink_mission_item_int_t missionItem = new mavlink_mission_item_int_t();
            missionItem.seq = (ushort)seq++;
            missionItem.frame = (byte)MAV_FRAME.GLOBAL_RELATIVE_ALT_INT;
            missionItem.command = (ushort)MAV_CMD.WAYPOINT;
            missionItem.current = 0;
            missionItem.autocontinue = 1;
            missionItem.param1 = latLngDto.param1;
            missionItem.param2 = latLngDto.param2;
            missionItem.param3 = latLngDto.param3;
            missionItem.param4 = float.NaN;
            missionItem.x = (int)(latLngDto.lat * 1e7);
            missionItem.y = (int)(latLngDto.lng * 1e7);
            missionItem.z = latLngDto.alt;
            missionItem.mission_type = 0;
            byte[] packetBytes = _parser.GenerateMAVLinkPacket20(MAVLINK_MSG_ID.MISSION_ITEM_INT, missionItem, false, sysid);
            // 发送数据包
            _socket.Send(packetBytes);
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
        private void OnMavlinkMessageReceived(int sysId)
        {
            MavlinkMessageReceived?.Invoke(this, new MavlinkEventArgs(sysId));
        }

    }
}

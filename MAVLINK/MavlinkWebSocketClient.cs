using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class MavlinkWebSocketClient
{
    private ClientWebSocket _webSocket;
    private byte[] _buffer;
    private int _bufferSize;
    private int _bufferOffset;

    public MavlinkWebSocketClient(string uri)
    {
        _webSocket = new ClientWebSocket();
        _buffer = new byte[1024];
        _bufferSize = 0;
        _bufferOffset = 0;
        ConnectAsync(uri).Wait();
    }

    private async Task ConnectAsync(string uri)
    {
        try
        {
            await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
            Console.WriteLine("Connected to WebSocket server.");
            //StartReceive();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }

    private void StartReceive()
    {
        ReceiveLoop().Wait();
    }

    private async Task ReceiveLoop()
    {
        while (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                WebSocketReceiveResult result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(_buffer, _bufferSize, _buffer.Length - _bufferSize), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }

                _bufferSize += result.Count;

                using (MemoryStream ms = new MemoryStream(_buffer, 0, _bufferSize))
                {
                    /*MAVLinkPacket msg;
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
                        // 处理不完整数据包的情况
                        // 重新启动接收操作以继续接收数据
                        continue;
                    }*/
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Receive error: {ex.Message}");
                break;
            }
        }

        //CloseConnection();
    }
    private void HandlePacket(string msg)
    {
        /*switch (packet.msgid)
        {
            case MAVLink.MAVLink.MAVLINK_MSG_ID_HEARTBEAT:
                msg_heartbeat heartbeat = (msg_heartbeat)packet.unpack();
                Console.WriteLine($"Heartbeat received: {heartbeat}");
                break;
            case MAVLink.MAVLink.MAVLINK_MSG_ID_SYS_STATUS:
                msg_sys_status sysStatus = (msg_sys_status)packet.unpack();
                Console.WriteLine($"System Status received: {sysStatus}");
                break;
            // 处理其他消息类型
            default:
                Console.WriteLine($"Received packet with ID: {packet.msgid}");
                break;
        }*/
    }

    public async Task SendMessageAsync(string msg)
    {
        await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    

    public void CloseConnection()
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None).Wait();
            _webSocket.Dispose();
        }
    }
}
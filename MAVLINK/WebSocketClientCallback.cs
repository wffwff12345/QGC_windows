using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UavApp.MAVLINK
{
    public interface WebSocketClientCallback
    {
        void callback(string json);
    }
}

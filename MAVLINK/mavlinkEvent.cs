using System;

namespace UavApp.MAVLINK
{
    public class MavlinkEventArgs : EventArgs
    {
        public int SysId { get; }

        public MavlinkEventArgs(int sysId)
        {
            SysId = sysId;
        }
    }
}
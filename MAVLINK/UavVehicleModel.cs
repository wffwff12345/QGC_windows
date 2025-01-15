namespace UavApp.MAVLINK
{
    public class UavVehicleModel
    {
        /**
        * 无人机设备ID
        */
        public long vehicleId;

        public void SetVehicleId(long vehicleId)
        {
            this.vehicleId = vehicleId;
        }
        public long GetVehicleId()
        {
            return vehicleId;
        }

        /**
         * 无人机mac地址
         */
        public string mac;

        public void SetMac(string mac)
        {
            this.mac = mac;
        }
        public string GetMac()
        {
            return mac;
        }

        /**
         * 无人机速度 km/h
         */
        public double vehicleSpeed;
        public void SetVehicleSpeed(double vehicleSpeed)
        {
            this.vehicleSpeed = vehicleSpeed;
        }
        public double GetVehicleSpeed()
        {
            return vehicleSpeed;
        }

        /**
         * 无人机电量
         */
        public int vehicleSoc;
        public void SetVehicleSoc(int vehicleSoc)
        {
            this.vehicleSoc = vehicleSoc;
        }
        public int GetVehicleSoc()
        {
            return vehicleSoc;
        }

        /**
         * 无人机经度
         */
        public double vehicleLong;
        public void SetVehicleLong(double vehicleLong)
        {
            this.vehicleLong = vehicleLong;
        }
        public double GetVehicleLong()
        {
            return vehicleLong;
        }

        /**
         * 无人机纬度
         */
        public double vehicleLat;
        public void SetVehicleLat(double vehicleLat)
        {
            this.vehicleLat = vehicleLat;
        }
        public double GetVehicleLat()
        {
            return vehicleLat;
        }

        /**
         * 无人机海拔高度
         */
        public double vehicleAlt;
        public void SetVehicleAlt(double vehicleAlt)
        {
            this.vehicleAlt = vehicleAlt;
        }
        public double GetVehicleAlt()
        {
            return vehicleAlt;
        }

        /**
         * 无人机当前模式
         */
        public long customMode;
        public void SetCustomMode(long customMode)
        {
            this.customMode = customMode;
        }
        public long GetCustomMode()
        {
            return customMode;
        }
        /**
         * 错误信息
         */
        public int faultInfo;

        public void SetFaultInfo(int faultInfo)
        {
            this.faultInfo = faultInfo;
        }   
        public int GetFaultInfo()
        {
            return faultInfo;
        }
    }

}

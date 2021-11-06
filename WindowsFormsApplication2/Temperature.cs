using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace DisplayForwarder
{
    public class Temperature
    {
        //public const string TCTL_NAME = "Tctl";
        //public const string TSYS_NAME = "Tsys";
        public const string TCTL_NAME = "Temperature #1";
        public const string TSYS_NAME = "Temperature #2";

        public static double Tctl
        {
            get
            {
                Double temp = 0.0;

                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\OpenHardwareMonitor", "SELECT * FROM Sensor where Name like '" + TCTL_NAME + "%'");
                foreach (ManagementObject obj in searcher.Get())
                {
                    temp = Convert.ToDouble(obj["Value"].ToString());
                }

                return temp;
            }
        }

        public static double Tsys
        {
            get
            {
                Double temp = 0.0;

                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\OpenHardwareMonitor", "SELECT * FROM Sensor where Name like '" + TSYS_NAME + "%'");
                foreach (ManagementObject obj in searcher.Get())
                {
                    temp = Convert.ToDouble(obj["Value"].ToString());
                }

                return temp;
            }
        }

        public double CurrentValue { get; set; }
        public string InstanceName { get; set; }
        public static List<Temperature> Temperatures
        {
            get
            {
                List<Temperature> result = new List<Temperature>();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                foreach (ManagementObject obj in searcher.Get())
                {
                    Double temp = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                    temp = (temp - 2732) / 10.0;
                    result.Add(new Temperature { CurrentValue = temp, InstanceName = obj["InstanceName"].ToString() });
                }
                return result;

            }
        }
    }

}

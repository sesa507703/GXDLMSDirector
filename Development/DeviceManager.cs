using GXDLMS.ManufacturerSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GXDLMSDirector
{
    public class DEVICE_PARAMETERS
    {
        public string address;
        public int powerIndex;
        public string POWER_OBIS_CODE;
        public string name;
        public int MappingSerialNumber;
    }
    public class DeviceManager
    {
        public readonly int DEVICE_COUNT = 6;
        //List<GXDLMSDevice> _listDevices = new List<GXDLMSDevice>();
        static GXDLMSMeterCollection _devices;
        static System.Timers.Timer timer = new System.Timers.Timer(20000);

        static List<DEVICE_PARAMETERS> devicelist = new List<DEVICE_PARAMETERS>();
        static DeviceManager()
        {
           
            devicelist.Add(new DEVICE_PARAMETERS {name="", address = "localhostSample", powerIndex = 2, POWER_OBIS_CODE= "0.0.43.1.8.255" });
            devicelist.Add(new DEVICE_PARAMETERS { name = "", address = "US_2402:3a80:1702:129::1", powerIndex = 2, POWER_OBIS_CODE = "0.0.94.91.14.255", MappingSerialNumber = 10000001 });
            devicelist.Add(new DEVICE_PARAMETERS { name = "", address = "US_2402:3a80:1702:125::1", powerIndex = 2, POWER_OBIS_CODE = "0.0.94.91.14.255", MappingSerialNumber = 10000002 });
        }
    


        public static void InitDevices(GXDLMSMeterCollection Devices)
        {
            _devices = Devices;
            timer.Enabled = true;
            timer.Start();
            timer.Elapsed += Timer_Elapsed;
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            InitTimeBasedReading();
        }

        private static void InitTimeBasedReading()
        {
            foreach (var deviceParameter in devicelist)
            {
                var device = _devices.Find(d => d.Name == deviceParameter.address);
                if (device == null)
                {
                    GXLogWriter.WriteLog(deviceParameter.address + " not found");
                    continue;
                }
                var glsdevice = (GXDLMSDevice)device;
                try
                {
                    glsdevice.InitializeConnection();
                }
                catch
                {
                    GXLogWriter.WriteLog("NO_CONNECTION TO " + deviceParameter.address);
                    continue;
                }
                var objectToBeRead = glsdevice.Objects.Where(d => d.LogicalName == deviceParameter.POWER_OBIS_CODE);
                object powervalue = 0;
                if (objectToBeRead != null && objectToBeRead.Count() > 0)
                {
                    powervalue = glsdevice.Comm.Read(null, objectToBeRead.ElementAt(0), false);
                    GXLogWriter.WriteLog("POWER_READ FROM " + deviceParameter.address + " value:" + powervalue);
                    glsdevice.Disconnect();
                    if (deviceParameter.MappingSerialNumber > 0)
                    {
                        try
                        {
                            DataPacket dp = new DataPacket(deviceParameter.MappingSerialNumber, decimal.Parse(powervalue.ToString()), 0, 0, 0);
                            DataManager.DumpDLMSMeterDataToDb(dp);
                        }
                        catch (Exception ex)
                        {
                            GXLogWriter.WriteLog("POC_ERROR DATA_INSERTION" + ex.ToString());
                        }
                    }
                }
                else
                {
                   GXLogWriter.WriteLog(deviceParameter.POWER_OBIS_CODE + " not found ");
                }
            }
        }
    }
}

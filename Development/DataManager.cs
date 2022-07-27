using GXDLMS.ManufacturerSettings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GXDLMSDirector
{
    public class DataPacket
    {
        public DataPacket(int MeterSerialNo, decimal power,decimal voltage,decimal current, decimal energy)
        {
            this.MeterSerialNo = MeterSerialNo;
            this.Date = DateTime.Now.ToString("dd/MM/yyyy");
            this.Time = DateTime.Now.ToString("HH:mm:ss");
            this.Voltage = voltage;
            this.Current = current;
            this.Power = power;
            this.Energy = energy;
            this.Credit = "10009958";
            this.CreditDays = "NA";
            this.DailyEnergy = 2.046999M;
            this.EnergyLimitPerDay = 414.0000M;
            this.EnergyLimit = 13.80M;
            this.kWhSlab1 = 1.00M;
            this.kWhSlab2 = 1.00M;
            this.kWhSlab3 = 1.00M;
            this.KwhSlab1Rate = "NA";
            this.KwhSlab2Rate = "20.00";
            this.KwhSlab3Rate = "30.00";
            this.PowerFactor = "40.00";
            this.MonthlyEnergyConsumption = "0.999930";
            this.MonthlyCurrencyDecrement = "94.19";
            this.TODStatus = 0;
            this.TODTime1 = "02.00-04.00";
            this.TODTime2 = "06.00-11.00";
            this.TODTime3 = "13.00-18.00";
            this.TODRate1 = "*";
            this.TODRate2 = "*";
            this.TODRate3 = "*";
            this.TODPower1 = "0";
            this.TODPower2 = "0";
            this.TODPower3 = "0";
            this.Frequency = "50.08";
        }
        public decimal MeterSerialNo { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public decimal Voltage { get; set; }
        public decimal Current { get; set; }
        public decimal Power { get; set; }
        public decimal Energy { get; set; }
        public string Credit { get; set; }
        public string CreditDays { get; set; }
        public decimal DailyEnergy { get; set; }
        public decimal EnergyLimitPerDay { get; set; }
        public decimal EnergyLimit { get; set; }
        public decimal kWhSlab1 { get; set; }
        public decimal kWhSlab2 { get; set; }
        public decimal kWhSlab3 { get; set; }
        public string KwhSlab1Rate { get; set; }
        public string KwhSlab2Rate { get; set; }
        public string KwhSlab3Rate { get; set; }
        public string PowerFactor { get; set; }
        public string MonthlyEnergyConsumption { get; set; }
        public string MonthlyCurrencyDecrement { get; set; }
        public byte TODStatus { get; set; } //If TODStatus is 0, only frequency will be valid and rest other below parameters will not be valid
        public string TODTime1 { get; set; }
        public string TODTime2 { get; set; }
        public string TODTime3 { get; set; }
        public string TODRate1 { get; set; }
        public string TODRate2 { get; set; }
        public string TODRate3 { get; set; }
        public string TODPower1 { get; set; }
        public string TODPower2 { get; set; }
        public string TODPower3 { get; set; }
        public string Frequency { get; set; }
    }
    public class DataManager
    {
        public static void DumpDLMSMeterDataToDb(DataPacket dataPacket)  
        {

            var builder = new StringBuilder();
            builder.Append("str=");

            foreach (var property in dataPacket.GetType().GetProperties())
            {
                string name = property.Name;
                var value = property.GetValue(dataPacket);
                if (!(!name.Equals("TODStatus") && name.Contains("TOD") && dataPacket.TODStatus == 0))
                {
                    builder.Append(value);
                    builder.Append(",");
                }
            }

            string deviceData = builder.ToString().TrimEnd(',');

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(VillayaPprDb_ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_AddDataPacket", sqlConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@DataPacket", SqlDbType.VarChar).Value = deviceData;
                        cmd.Parameters.Add("@DateTimeOffset", SqlDbType.DateTimeOffset).Value = DateTimeOffset.Now;
                        cmd.Parameters.Add("@IsDeviceTimeStamp", SqlDbType.Bit).Value = false;

                        sqlConnection.Open();
                        cmd.ExecuteNonQuery();
                        GXLogWriter.WriteLog("DATA_INSERTED "+ deviceData);
                    }
                }
            }
            catch (Exception ex)
            {
                GXLogWriter.WriteLog(ex.ToString());
            }
        }
    }
}

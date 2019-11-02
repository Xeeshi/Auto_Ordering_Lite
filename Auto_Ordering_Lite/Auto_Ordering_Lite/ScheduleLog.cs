using Auto_Ordering_Lite;
using MaxBachat21.Model;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaxBachat21
{
    public partial class ScheduleLog : Syncfusion.Windows.Forms.Office2007Form
    {
        public ScheduleLog()
        {
            InitializeComponent();
        }
        Connection con= new Connection();
        List<ScheduleLogView> myList = new List<ScheduleLogView>();

        private void ScheduleLog_Load(object sender, EventArgs e)
        {
            DisplayRecords();

        }

        private string ifNullReturnPending(string value)
        {
            try
            {
                if (value == "")
                {
                    return "Pending";
                }
                else
                {
                    return double.Parse(value).ToString();
                }
            }
            catch { return ""; }

        }
        private string ifNullReturnZero(string value)
        {
            try
            {
                if (value == "")
                {
                    return "0";
                }
                else
                {
                    return double.Parse(value).ToString();
                }
            }
            catch { return ""; }

        }


        private void DisplayRecords()
        {
            try
            {
                if (con.con.State == ConnectionState.Open)
                { con.con.Close(); }
                con.con.Open();
                string qot = "'";
                System.Data.SqlClient.SqlDataReader sdr;
                string script = File.ReadAllText("SQL//ScheduleOrderLog.sql");
                script = script.Replace("@date", qot + dateTimePicker1.Text + qot);

                script = script.Replace("\r\n", " ");
                script = script.Replace("\t", " ");
                SqlCommand cmd = new SqlCommand(script, con.con);


                sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    ScheduleLogView slv = new ScheduleLogView()
                    {
                        VendorID = sdr["VendorID"].ToString(),
                        Email = sdr["Email"].ToString(),
                        OrderValue = ifNullReturnZero(sdr["OrderValue"].ToString()),
                        PONumber = sdr["PONumber"].ToString(),
                        Status = sdr["Status"].ToString(),
                        Vendor = sdr["Vendor"].ToString(),
                        Schedule_Date_Time = sdr["Schedule Date/Time"].ToString(),
                       


                    };
                    myList.Add(slv);

                }

                con.con.Close();
                InformationGrid.DataSource = myList;
                    
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }


        }
    }
}

using Auto_Ordering_Lite.Model;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Auto_Ordering_Lite
{
 public   class Connection
    {
        public SqlConnection con = null;


        public Connection()
        {
            if (Auto_Ordering_Lite.Properties.Settings.Default.isInternet.Trim() == "" || Auto_Ordering_Lite.Properties.Settings.Default.isInternet == "0")
            {
                con = new SqlConnection(Credential.Local_ConnectionString);

            }
            else
            {
                con = new SqlConnection(Credential.Internet_ConnectionString);
            }

        }


      //  static  string dbstring = "Data Source=" + MaxBachat21.Properties.Settings.Default.ServerName + ";Initial Catalog=" + MaxBachat21.Properties.Settings.Default.Database + ";User ID=" + MaxBachat21.Properties.Settings.Default.Username + ";Password=" + MaxBachat21.Properties.Settings.Default.Password;
      // static string dbstring = @"Data Source=103.75.244.25;initial Catalog=BE-MAXBACHAT;User ID=Purchasingsystem;Password=Future987";
      

     

    }
    
    
}


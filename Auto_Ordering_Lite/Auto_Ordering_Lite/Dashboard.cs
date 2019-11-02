using Auto_Ordering_Lite.Model;
using Auto_Ordering_Lite.Models;
using Auto_Ordering_Lite.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MaxBachat21;
using MaxBachat21.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Auto_Ordering_Lite
{
    public partial class Dashboard : Syncfusion.Windows.Forms.Office2007Form
    {
        static bool isForceStop = false;
        Connection con = new Connection();
        List<PO_Model> myOrderList = new List<PO_Model>();
        static string AutoScheduleLOG = "";
        private int ItemJoPrintHoChuki = 0;

        private Int32 CountDown = 0;
        User user = null;
        public Dashboard()
        {
            InitializeComponent();

            User _user = new User()

            {
                Username = "maxbsystem",
                Userid = "5",
                AllowInternet = "1",
                EmployeeDesignation = "BOT",
                EmployeeEmail = "orders@maxbachat.com",
                EmployeeName = "MaxB Ordering System",


            };
            user = _user;
        }



        public void DisplayStateMessage(string msg)
        {
            Statelabel.BeginInvoke(new MethodInvoker(() =>
            Statelabel.Text = msg
            ));
        }

        private string ifNullReturnZero(object value)
        {
            try
            {
                if (value == null)
                {
                    return "0";
                }
                else if (String.IsNullOrEmpty(value.ToString()))
                {
                    return "0";

                }
                else if (value.ToString() == "")
                {
                    return "0";
                }
                else if (String.IsNullOrWhiteSpace(value.ToString()))
                {
                    return "0";
                }
                return value.ToString();
            }
            catch { return "0"; }

        }
        private string ifNullDefaultMOQUnit(object value)
        {
            try
            {
                if (value == null)
                {
                    return "PC";
                }
                else if (String.IsNullOrEmpty(value.ToString()))
                {
                    return "PC";

                }
                else if (value.ToString() == "")
                {
                    return "PC";
                }
                else if (String.IsNullOrWhiteSpace(value.ToString()))
                {
                    return "PC";
                }
                return value.ToString();
            }
            catch { return "PC"; }

        }
        private double ifNullReturn_Double_Zero(object value)
        {
            try
            {
                if (value == null)
                {
                    return 0;
                }
                else if (String.IsNullOrEmpty(value.ToString()))
                {
                    return 0;

                }
                else if (value.ToString() == "")
                {
                    return 0;
                }
                else if (String.IsNullOrWhiteSpace(value.ToString()))
                {
                    return 0;
                }
                return double.Parse(value.ToString());
            }
            catch { return 0; }

        }
        private string ifNullReturnONE(object value)
        {
            try
            {
                if (value == null)
                {
                    return "1";
                }
                else if (String.IsNullOrEmpty(value.ToString()))
                {
                    return "1";

                }
                else if (value.ToString() == "")
                {
                    return "1";
                }
                else if (String.IsNullOrWhiteSpace(value.ToString()))
                {
                    return "1";
                }
                return value.ToString();
            }
            catch { return "1"; }

        }

        public List<PO_Model> get_Products_Target_VendorID_DB(string vendorid)
        {
            DisplayStateMessage("Getting Data From Database");
            List<PO_Model> ProductItemIDList = new List<PO_Model>();
            try
            {
                if (con.con.State == ConnectionState.Open)
                { con.con.Close(); }
                con.con.Open();
                string qot = "'";
                System.Data.SqlClient.SqlDataReader sdr;
                string script = File.ReadAllText("SQL/TargetOrder_Script.sql");

                script = script.Replace("@VendorId", qot + vendorid + qot);
                script = script.Replace("\r\n", " ");
                script = script.Replace("\t", " ");
                SqlCommand cmd = new SqlCommand(script, con.con);


                sdr = cmd.ExecuteReader();
                DisplayStateMessage("Fetching Information...");
                while (sdr.Read())
                {

                    string aBC = "";

                    if (ifNullReturnEmpty(sdr["AltBarcode"]).Trim() != "")
                    {

                        aBC = sdr.GetString(3).ToString();
                        aBC = aBC.Substring(1);
                    }





                    PO_Model pmt = new PO_Model()
                    {


                        AltBarcode = aBC,
                        Barcode = sdr["Barcode"].ToString(),

                        ItemDescription = sdr["LongName"].ToString(),
                        MOQ = double.Parse(ifNullReturnONE(sdr["MOQ"])),
                        MOQUnit = ifNullDefaultMOQUnit(sdr["MOQUnit"]),
                        Cost = Double.Parse(sdr["CostPrice"].ToString()),


                        JDCInventory = ifNullReturn_Double_Zero(sdr["InvJDC"]),
                        RS = 0,
                        ProductItemID = sdr["Productitemid"].ToString(),
                        FinalPC = 0,
                        Sugg = 0,

                        JDCTarget = ifNullReturnZero(sdr["TargetJDC"]),


                    };


                    ProductItemIDList.Add(pmt);


                }
                myOrderList.Clear();
                DisplayStateMessage("Loaded All Data.BOT About to Start");
                myOrderList.AddRange(ProductItemIDList);

                con.con.Close();
                return myOrderList;




            }
            catch (Exception ex)
            {
                AutoScheduleLOG = "15) Error in Data Loading";
                DisplayStateMessage("Exception occur " + ex.Message);
                UpdateLog(vendorid, "", "0");
            }



            return null;

        }



        private string SendEmail(string Email, string pid, string total)
        {
            try
            {
                string TOTALaMOUNT = total;





                if (Email.Trim() != "0" && Email.Trim() != "")
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + "Purchase Orders PDF//" + pid + ".pdf";
                    string obj = "MaxBachat Order # " + pid + " -- " + getBranchName() + " -- Delivery: " + getDeliveryDate() + " -- Rs " + TOTALaMOUNT;
                    OutlookEmail.SendEmail(Email, obj, path, pid, "");
                    return TOTALaMOUNT.ToString();

                }
                else
                {
                    AutoScheduleLOG = "Email Not Found";

                }


                return TOTALaMOUNT;
            }
            catch (Exception ex)
            {
                AutoScheduleLOG = "error at line 1447";

            }
            return "0";

        }
        private void UpdateLog(string vendorid, string pid, string total)
        {

            try
            {
                InsertInformation("INSERT INTO [mbo].[PSVendorScheduleLog] ([VendorId],[PONumber],[Status],[SentDate],[SentTIme],[OrderValue]) values ('" + vendorid + "','" + pid + "','" + AutoScheduleLOG + "','" + DateTime.Now.ToShortDateString() + "','" + DateTime.Now.ToShortTimeString() + "','" + total + "')");
            }
            catch (Exception ex)
            {
                AutoScheduleLOG = "error at line 1458";


            }

        }
        private double getMaxNumberFromSelectedColumn(string first, string second)
        {
            try
            {


                double maxnumber1 = double.Parse(first);
                double maxnumber2 = double.Parse(second);

                if (maxnumber1 > maxnumber2)
                { return maxnumber1; }
                else
                { return maxnumber2; }



            }
            catch (Exception ex)
            {
                //      MessageBox.Show(ex.Message);
                return 0;
            }
        }
        private string getDeliveryDate()
        {
            return DateTime.Now.AddDays(1).ToString("dd-MMM-yy");
        }
        private List<PO_Model> get_Products_Dos_VendorID_DB(string vendorid)
        {
            DisplayStateMessage("Getting Data From Database");
            List<PO_Model> ProductItemIDList = new List<PO_Model>();
            try
            {
                if (con.con.State == ConnectionState.Open)
                { con.con.Close(); }
                con.con.Open();
                string qot = "'";
                System.Data.SqlClient.SqlDataReader sdr;
                string script = File.ReadAllText("SQL//script.sql");
                script = script.Replace("@BranchId", qot + getBranchID() + qot);
                script = script.Replace("@VendorId", qot + vendorid + qot);
                script = script.Replace("\r\n", " ");
                script = script.Replace("\t", " ");
                SqlCommand cmd = new SqlCommand(script, con.con);

                if (con.con.State == ConnectionState.Open)
                { con.con.Close(); }
                con.con.Open();
                sdr = cmd.ExecuteReader();
                DisplayStateMessage("Loading Data Into Grid");
                while (sdr.Read())
                {

                    string aBC = "";

                    if (sdr["AltBarcode"].ToString().Trim() != "")
                    {

                        aBC = sdr.GetString(3).ToString();
                        aBC = aBC.Substring(1);
                    }

                    string dos = "";
                    string target = "";
                    int today = DateTime.Now.Day;
                    if (today >= 1 && today <= 5)
                    {
                        dos = ifNullReturnEmpty(sdr["DOS1_5"]);
                        target = ifNullReturnEmpty(sdr["Target1_5"]);

                    }
                    else if (today >= 6 && today <= 10)
                    {
                        dos = ifNullReturnEmpty(sdr["DOS6_10"]);
                        target = ifNullReturnEmpty(sdr["Target6_10"]);
                    }
                    else if (today >= 11 && today <= 15)
                    {
                        dos = ifNullReturnEmpty(sdr["DOS11_15"]);
                        target = ifNullReturnEmpty(sdr["Target11_15"]);
                    }
                    else if (today >= 16 && today <= 20)
                    {
                        dos = ifNullReturnEmpty(sdr["DOS16_20"]);
                        target = ifNullReturnEmpty(sdr["Target16_20"]);
                    }
                    else if (today >= 21 && today <= 25)
                    {
                        dos = ifNullReturnEmpty(sdr["DOS21_25"]);
                        target = ifNullReturnEmpty(sdr["Target21_25"]);
                    }
                    else if (today >= 26 && today <= 31)
                    {
                        dos = ifNullReturnEmpty(sdr["DOS26_31"]);
                        target = ifNullReturnEmpty(sdr["Target26_31"]);
                    }




                    PO_Model vp = new PO_Model()
                    {
                        AltBarcode = aBC,
                        Barcode = sdr["Barcode"].ToString(),
                        DOS = dos,

                        ItemDescription = sdr["LongName"].ToString(),
                        MOQ = double.Parse(ifNullReturnONE(sdr["MOQ"])),
                        MOQUnit = ifNullDefaultMOQUnit(sdr["MOQUnit"]),
                        Cost = Double.Parse(sdr["CostPrice"].ToString()),

                        JDCInventory = double.Parse(ifNullReturnZero(sdr["Inv"])),


                        RS = 0,

                        ProductItemID = sdr["Productitemid"].ToString(),

                        FinalPC = 0,
                        Sugg = 0,

                        Total_1M = ifNullReturnZero(sdr["Total_1M"]),
                        Total_2M = ifNullReturnZero(sdr["Total_2M"]),



                        JDCTarget = target
                    };


                    ProductItemIDList.Add(vp);



                }


                DisplayStateMessage("Loaded Data.. About to Start..");

                con.con.Close();
                myOrderList.Clear();
                myOrderList.AddRange(ProductItemIDList);
                return ProductItemIDList;


            }

            catch (Exception ex)
            { AddLogs("Error at Reading Data Fom DB");
                UpdateLog(vendorid, "", "0");
            }

            return null;

        }

        private bool ValidaeEmail(object email)
        {
            try
            {
                string emailPat = @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)";

                var EmaillArr = email.ToString().Split(';');
                for (int i = 0; i < EmaillArr.Length; i++)
                {
                    if (!Regex.IsMatch(EmaillArr[i], emailPat))
                    {
                        return false;
                    }


                }
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        private void SetStatus(string onoff)
        {
            StatusLabel.BeginInvoke(new MethodInvoker(() =>
            StatusLabel.Text = onoff
                            ));

        }
        private string ifNullReturnEmpty(object value)
        {
            try
            {
                if (value == null)
                {
                    return "";
                }
                else if (String.IsNullOrEmpty(value.ToString()))
                {
                    return "";

                }
                else if (value.ToString() == "")
                {
                    return "";
                }
                else if (String.IsNullOrWhiteSpace(value.ToString()))
                {
                    return "";
                }
                return value.ToString();
            }
            catch { return ""; }
        }
        private void Auto_OrderCreating()
        {
            try
            {
                SetStatus("ON");

                AutoScheduleLOG = "";

                DisplayStateMessage("Invoking Auto Ordering");

                var dofw = DateTime.Now.ToString("ddd");
                var dom = DateTime.Now.Day.ToString();
                ////////////////////////
                if (isForceStop)
                { return; }
                /////////////////////////
                string qot = "'";
                string script = File.ReadAllText("SQL/LoadScheduleWithCond.sql");
                script = script.Replace("@dow", dofw);
                script = script.Replace("@dom", qot + dom + qot);
                if (isForceStop)
                { return; }
                script = script.Replace("\r\n", " ");
                script = script.Replace("\t", " ");
                var dt = getDataTableFromDB(script);
                if (dt != null)
                {
                    ////////////////////////
                    if (isForceStop)
                    { return; }
                    /////////////////////////


                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        AutoScheduleLOG = "";
                        try
                        {
                            string vendord = ifNullReturnEmpty(dt.Rows[i]["VendorID"]);
                            string name = ifNullReturnEmpty(dt.Rows[i]["Name"]);


                            string PhoneNo = ifNullReturnEmpty(dt.Rows[i]["PhoneNo"]);
                            if (ValidaeEmail(dt.Rows[i]["Email"]))
                            {
                                string email = dt.Rows[i]["Email"].ToString();
                                ////////////////////////
                                if (isForceStop)
                                { return; }
                                /////////////////////////

                                vendord = vendord.Trim();


                                List<PO_Model> unProcess_List = null;
                                List<PO_Model> ProcessList = null;
                                Clean_Resources();
                               if (dt.Rows[i]["OrderMethod"].ToString().ToLower()=="dos" || dt.Rows[i]["OrderMethod"].ToString().ToLower() == "target")
                               { ////////////////////////
                                    if (isForceStop)
                                    { return; }
                                    /////////////////////////
                                    DisplayStateMessage("Creating Order");
                                    unProcess_List = get_Products_Dos_VendorID_DB(vendord);

                                    ////////////////////////
                                    if (isForceStop)
                                    { return; }
                                    /////////////////////////

                                    DisplayStateMessage("Calculating Order..");

                                    ProcessList = calculate_DOS_SuggestOrder(unProcess_List, vendord);
                                    DisplayStateMessage("Calculated Order.");
                                    ////////////////////////
                                    if (isForceStop)
                                    { return; }
                                    /////////////////////////
                                }
                                if (unProcess_List != null && unProcess_List.Count > 0)
                                {
                                    ////////////////////////
                                    if (isForceStop)
                                    { return; }
                                    /////////////////////////
                                    if (ProcessList != null && ProcessList.Count > 0)
                                    {
                                        ////////////////////////
                                        if (isForceStop)
                                        { return; }
                                        /////////////////////////
                                        DisplayStateMessage("Generating PO ID.");
                                        string getPOID = GetPIDFromDB(vendord);

                                        if (getPOID != "-1")
                                        { ////////////////////////
                                            if (isForceStop)
                                            { return; }
                                            /////////////////////////
                                            DisplayStateMessage("Inserting Record into DB.");
                                            if (Inserting_PO_into_DB(ProcessList, vendord, getPOID))
                                            { ////////////////////////
                                                if (isForceStop)
                                                { return; }
                                                /////////////////////////
                                                DisplayStateMessage("Creating PDF. PO ID: " + getPOID);
                                                if (Creating_PDF(ProcessList, vendord, name, email, getRemarks(), getPOID) != "-1")
                                                { ////////////////////////
                                                    if (isForceStop)
                                                    { return; }
                                                    /////////////////////////

                                                    DisplayStateMessage("Sending Email...");
                                                    string total = SendEmail(email, getPOID, ProcessList.Sum(x => x.RS).ToString());
                                                    ////////////////////////
                                                    if (PhoneNo != "")
                                                    {
                                                        SMS sms = new SMS
                                                        {
                                                            APIKey = "ab3dfd414f906279c96d09ba4e70020d",
                                                            Masking = "MaxBachat",
                                                            MessageBody = "Dear Supplier,\nPlease Check Your Email For Our ORDER NO: " + getPOID + ".\nAmount: RS " + Currency(ProcessList.Sum(x => x.RS).ToString()),
                                                            Receiver = PhoneNo

                                                        };
                                                        SendSms.SendSMS(sms);
                                                    }
                                                    AddLogs("Sent");

                                                    DisplayStateMessage("Completed.");
                                                    UpdateLog(vendord, getPOID, total);
                                                    //   getActiveChildForm().Close();
                                                    ////////////////////////
                                                    if (isForceStop)
                                                    { return; }
                                                    /////////////////////////
                                                }
                                            }
                                        }


                                    }
                                    else
                                    {
                                        AddLogs("Order Below Baseline");


                                        UpdateLog(vendord, "", "0");
                                    }
                                } else
                                {
                                    AddLogs("NO Mapping Item");


                                    UpdateLog(vendord, "", "0"); }




                            }
                            else { AddLogs("Email Not Found");
                                UpdateLog(vendord, "", "0");
                            }
                        }
                        catch (Exception ex)
                        {
                            // CloseAllChildForm();
                            richTextBox1.Text = richTextBox1.Text + Environment.NewLine + "=>" + ex.Message;
                        }

                    }



                }

            }
            catch (Exception ex)
            { }



            DisplayStateMessage("Completed..");
            SetStatus("OFF");
        }

        private string getRemarks()
        {
            return "--";
        }

        private string GetPIDFromDB(string vid)
        {

            try
            {
                Random r = new Random();
                string current = "";
                string yy = "", mm = "", dd = "";
                yy = DateTime.Now.ToString("yy");
                mm = DateTime.Now.ToString("MM");
                dd = DateTime.Now.ToString("dd");
                do
                {

                    current = r.Next(1, 9999).ToString().PadLeft(4, '0');
                    current = yy + mm + dd + current;
                    var result = ValidateInformationDatabase_More_Than_1("select count(*) from PurchaseOrder where [PurchaseOrderNumber]='" + current + "'");
                    if (result == "0")
                    {
                        return current;

                    }
                    else if (result == "-1")
                    {
                        return "-1";
                    }


                } while (ValidateInformationDatabase_More_Than_1("select count(*) from PurchaseOrder where [PurchaseOrderNumber]='" + current + "'") == "1");
            }
            catch
            {


                AddLogs("Error in Generating POID");
                UpdateLog(vid, "", "0");
            }


            return "-1";



        }
        public String ValidateInformationDatabase_More_Than_1(string ss)
        {
            try
            {
                DataTable dt = new DataTable();
                if (con.con.State == ConnectionState.Open)
                { con.con.Close(); }

                SqlDataAdapter sda = new SqlDataAdapter(ss, con.con);


                sda.Fill(dt);
                if (double.Parse(dt.Rows[0][0].ToString()) >= 1)
                    return "1";
                else
                    return "0";
            }
            catch (Exception ex)
            {
                AddLogs("Couldn't Validate Order ID From DB");
                DisplayStateMessage("Couldn't Validate Order ID From DB");
                return "-1";
            }
            return "0";


        }

        private string getPriority()
        {
            string priority = "";
            PriorityComboBox.BeginInvoke(new MethodInvoker(() =>
            priority = PriorityComboBox.Text
            ));
            if (priority.ToLower() != "urgent")
            {
                return "1";
            }
            return "";
        }
        private bool Inserting_PO_into_DB(List<PO_Model> list, string vendorid, string getPOID)
        {
            try
            {
                string priority = getPriority();

                if (getPOID == "-1")
                { return false; }

                var PurchaseOrderID = InsertValuesIntoDataBase("INSERT INTO [dbo].[PurchaseOrder] ([ProductVendorId],[PurchaseOrderDate],[PaymentModeId],[CurrencyId],[VendorQuotationNumber],[NetAmount],[Remark],[PurchaseOrderNumber],[DeliveryDate],[DeliveryType],[BranchDeliveryLocationId],[ShipmentModeId],[ShipmentProvideById],[CompanyBranchId],[UserId],[DataEntryDate],[DataEntryStatus],[DataEntryPost],[InvoiceToClient],[Priority],[IsCalculateInvoiceDiscount]) values ('" + vendorid + "','" + DateTime.Now.ToString("yyyy-MM-dd hh:mm") + "','1','1',NULL,'" + list.Sum(s => s.RS).ToString() + "','" + getRemarks() + "','" + getPOID + "','" + getDeliveryDate() + "',NULL,'" + getBranchID() + "','1','1','1','1','" + DateTime.Now.ToString("yyyy-MM-dd hh:mm") + "','1',NULL,NULL,'" + priority + "','1');SELECT SCOPE_IDENTITY();");
                if (PurchaseOrderID != -1)
                {

                    bool resultQuery = Insert_Order_Items(list, PurchaseOrderID.ToString());
                    if (!resultQuery)
                    {
                        DisplayStateMessage("Couldn't insert Order Item into DB");
                        AddLogs("Couldn't insert Order Item into DB");
                    }
                    else
                    {

                        return true;
                    }

                }
                else
                {
                    DisplayStateMessage("Problem in Generating Purchase Order ID");
                    AddLogs("Problem in Generating Purchase Order ID");
                    return false;
                }
            }
            catch (Exception ex)
            {
                DisplayStateMessage("Error 23) Couldn't Insert Order Into DB");
                AddLogs("23 Couldn't Insert Order Into DB");
                return false;
            }
            return false;

        }
        private string Creating_PDF(List<PO_Model> list, string vendorid, string name, string email, string remakrs, string getPOID)
        {
            try
            {


                if (list.Count == 0)
                {
                    // MessageBox.Show("No Item is Selected", "Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    AddLogs("Empty Order");
                    UpdateLog(vendorid, "", "0");
                    return "-1";
                }

                string branchid = getBranchID();



                string priority = getPriority();





                CompletepdfGenerate(getPOID, list, list.Sum(x => x.RS).ToString(), email, name, remakrs);







                DisplayStateMessage("Order has been Completed. Order ID: " + getPOID);
                return getPOID;
                //   PDFgeneratePanel.Enabled = false;

            }
            catch (Exception ex)
            {
                AddLogs("Error in Generating PDF");
                UpdateLog(vendorid, "", list.Sum(s => s.RS).ToString());
                return "-1";
            }

        }

        private string getBranchID()
        {
            return "1";
        }

        private void CompletepdfGenerate(string orderID, List<PO_Model> list, string total, string email, string name, string remarks)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "temp"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "temp");
            }
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Purchase Orders PDF"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Purchase Orders PDF");
            }
            List<string> pdffiles = new List<string>();

            int PageNumber = 0;

            double temp = (double)list.Count / 20;

            var totalpage = Math.Ceiling(temp);
            for (int loop = ItemJoPrintHoChuki; loop < list.Count; loop += 20)
            {

                ItemJoPrintHoChuki += 20;
                if (ItemJoPrintHoChuki == list.Count)
                {
                    PageNumber++;
                    CreatePdfOfList(list, loop, ItemJoPrintHoChuki, "temp//" + loop + ".pdf", PageNumber, totalpage, orderID, true, total, name, email, remarks);
                    pdffiles.Add("temp//" + loop + ".pdf");
                }
                else if (ItemJoPrintHoChuki <= list.Count)
                {
                    PageNumber++;
                    CreatePdfOfList(list, loop, ItemJoPrintHoChuki, "temp//" + loop + ".pdf", PageNumber, totalpage, orderID, false, total, name, email, remarks);
                    pdffiles.Add("temp//" + loop + ".pdf");

                }
                else
                {
                    PageNumber++;
                    CreatePdfOfList(list, loop, list.Count, "temp//end.pdf", PageNumber, totalpage, orderID, true, total, name, email, remarks);
                    pdffiles.Add("temp//end.pdf");
                }



            }
            MergePdf.MergePDFs(pdffiles, "Purchase Orders PDF//" + orderID + ".pdf");
            for (int i = 0; i < pdffiles.Count; i++)
            {
                try
                {
                    File.Delete(pdffiles[i]);
                }
                catch { }
            }

            ItemJoPrintHoChuki = 0;
        }

        private void CreatePdfOfList(List<PO_Model> list, int st, int end, string savePdf, int PageNumber, double TotalPage, string OrderNumber, bool final, String total, string name, string email, string remarks)
        {

            try
            {

                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var FullBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20);
                var MB_Head = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 30);



                string oldFile = "mb2.pdf";


                // open the reader
                PdfReader reader = new PdfReader(oldFile);
                iTextSharp.text.Rectangle size = reader.GetPageSizeWithRotation(1);
                Document document = new Document(size);

                // open the writer
                FileStream fs = new FileStream(savePdf, FileMode.Create, FileAccess.Write);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                Zen.Barcode.Code128BarcodeDraw barcode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                System.Drawing.Image img = barcode.Draw(OrderNumber, 300);


                iTextSharp.text.Image pic = iTextSharp.text.Image.GetInstance(img, System.Drawing.Imaging.ImageFormat.Jpeg);


                pic.Border = iTextSharp.text.Rectangle.BOX;
                pic.BorderColor = iTextSharp.text.BaseColor.BLACK;
                pic.BorderWidth = 150f;

                pic.SetAbsolutePosition(672, 600);
                pic.BorderWidth = 0f;
                document.Add(pic);



                // the pdf content
                PdfContentByte cb = writer.DirectContent;

                // select the font properties
                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb.SetColorFill(BaseColor.BLACK);
                cb.SetFontAndSize(bf, 8);
                // write the text in the pdf content
                cb.BeginText();

                int VPos = 440;



                for (int i = st; i < end; i++)
                {  // put the alignment and coordinates here
                    cb.ShowTextAligned(0, list[i].Barcode, 37, VPos, 0);
                    if (list[i].AltBarcode.Length < 40)
                    {
                        cb.ShowTextAligned(0, list[i].AltBarcode, 115, VPos, 0);
                    }
                    else
                    {
                        cb.ShowTextAligned(0, list[i].AltBarcode.Substring(0, 40) + "**", 115, VPos, 0);
                    }
                    cb.ShowTextAligned(0, list[i].ItemDescription, 327, VPos, 0);
                    cb.ShowTextAligned(0, list[i].FinalPC.ToString(), 592, VPos, 0);
                    cb.ShowTextAligned(0, list[i].OrderCTN.ToString() + " " + list[i].MOQUnit.ToString(), 625, VPos, 0);
                    cb.ShowTextAligned(0, list[i].Cost.ToString(), 670, VPos, 0);
                    cb.ShowTextAligned(0, list[i].RS.ToString(), 725, VPos, 0);
                    cb.ShowTextAligned(0, "__________________________________________________________________________________________________________________________________________________________________", 38, (VPos - 5), 0);
                    VPos -= 18;
                }
                cb.EndText();

                cb.SetColorFill(BaseColor.BLACK);
                cb.SetFontAndSize(bf, 9);
                cb.BeginText();
                if (name.Length > 30)
                {
                    cb.ShowTextAligned(0, name.Substring(0, 29), 43, 542, 1);
                }
                else
                {
                    cb.ShowTextAligned(0, name, 43, 542, 1);
                }
                cb.ShowTextAligned(0, " Page# " + PageNumber + " of " + TotalPage, 700, 10, 1);
                try
                {
                    var xx = email.Split(new[] { ";" }, StringSplitOptions.None);
                    cb.ShowTextAligned(0, xx[0], 43, 530, 1);
                }
                catch (Exception ex) {
                    MessageBox.Show("66" + ex.Message);

                }
                cb.ShowTextAligned(0, getBranchName(), 204, 542, 1);  //branch
                cb.ShowTextAligned(0, user.EmployeeName, 290, 542, 1);  //Employee Name
                cb.ShowTextAligned(0, user.EmployeeEmail, 290, 531, 1);  //Employee Email
                cb.ShowTextAligned(0, remarks, 432, 542, 1);  //remarks
                cb.ShowTextAligned(0, DateTime.Now.ToString("dd-MMM-yy (ddd)"), 692, 555, 1); //
                cb.SetColorFill(BaseColor.RED);
                cb.SetFontAndSize(bf, 9);
                cb.ShowTextAligned(0, getDeliveryDate(), 692, 529, 1);


                if (final == true)
                {
                    cb.SetFontAndSize(bf, 12);
                    cb.SetColorFill(BaseColor.DARK_GRAY);
                    //  cb.ShowTextAligned(0, "  Total CTN:  " +  list.Sum(ss => ss.OrderCTN), 480, VPos-30, 1);


                    cb.ShowTextAligned(1, "Total(Rs): " + Currency(total.ToString()), 680, VPos - 15, 1);

                }
                else

                {
                    cb.SetColorFill(BaseColor.BLACK);
                    cb.ShowTextAligned(0, "--Continue--", 400, VPos - 40, 1);
                }
                cb.SetColorFill(BaseColor.DARK_GRAY);
                cb.SetFontAndSize(bf, 13);
                cb.ShowTextAligned(0, OrderNumber, 520, 578, 1);
                cb.EndText();




                //try
                //{
                //    var xx = VendorTextBox.Text.Split(new[] { ";" }, StringSplitOptions.None);
                //    cb.ShowTextAligned(1, xx[0], 105, 530, 1);

                //}
                //catch { }


                // create the new page and add it to the pdf
                PdfImportedPage page = writer.GetImportedPage(reader, 1);
                cb.AddTemplate(page, 0, 0);

                // close the streams and voilá the file should be changed :)
                document.Close();
                fs.Close();
                writer.Close();
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("54" + ex.Message); }
        }

        private string getBranchName()
        {
            return "JDC";
        }
        private string Currency(string curr)
        {

            decimal parsed = decimal.Parse(curr, CultureInfo.InvariantCulture);
            CultureInfo hindi = new CultureInfo("en-us");
            string text = string.Format(hindi, "{0:#,#}", parsed);
            return text;
        }

        private double IfNegativeMakeZero(string number)
        {
            try
            {
                if (double.Parse(number) < 0)
                {
                    return 0;
                }
                else
                { return double.Parse(number); }



            }
            catch (Exception ex) { MessageBox.Show("53" + ex.Message); }
            return 0;
        }

      

        private PO_Model PerfromDos_Calc_OnSingle(PO_Model list)
        {
            try
            {
                double max = double.Parse(getMaxNumberFromSelectedColumn(list.Total_1M, list.Total_2M).ToString());
                // var result = Math.Round((max / 31) * decimal.Parse(DG.Table.Records[i]["DOS"].ToString()) - decimal.Parse(DG.Table.Records[i]["Inventory"].ToString()), 0) / decimal.Parse(DG.Table.Records[i]["MOQ"].ToString());



                var result = max / 31;
                result = result * double.Parse(list.DOS);
                result = result - IfNegativeMakeZero(list.JDCInventory.ToString());
                result = Math.Round(result, 0);
                result = result / list.MOQ;
                result = Math.Round(result, 0);
                result = result * list.MOQ;

                list.Sugg = result;
                if (result > 0)
                {
                    list.FinalPC = result;
                    list.RS = Math.Round(list.FinalPC * list.Cost);
                    list.OrderCTN = list.FinalPC / list.MOQ;
                    return list;
                }
            } catch (Exception ex)
            {

            }
            return null;
        }


        private List<PO_Model> calculate_DOS_SuggestOrder(List<PO_Model> List, string vendorid)
        {
            try {

                var _processList = new List<PO_Model>();
                for (int i = 0; i < List.Count; i++)
                {
                    try
                    {
                        if (List[i].JDCTarget != "")
                        {

                            var result = PerfromTarget_Calc_OnSingle(List[i]);
                            if (result != null)
                            { _processList.Add(result); }
                        }
                        else
                        {
                            var result = PerfromDos_Calc_OnSingle(List[i]);
                            if (result != null)
                            { _processList.Add(result); }
                        }





                    }
                    catch (Exception ex)
                    {
                        AddLogs("Error in Target Calcuation");
                        UpdateLog(vendorid, "", "0");

                    }


                }
                return _processList;
            }
            catch { }

            return null;
            }
           
        private PO_Model PerfromTarget_Calc_OnSingle(PO_Model list)
        {
            try
            {
                var result = Double.Parse(list.JDCTarget) - list.JDCInventory;
                if (result > 0)
                {
                    list.Sugg = result;
                    result = IfNegativeMakeZero(list.Sugg.ToString());
                    result = Math.Round(result, 0);
                    result = result / (double.Parse(list.MOQ.ToString()));
                    result = Math.Round(result, 0);
                    result = result * (double.Parse(list.MOQ.ToString()));


                    if (result > 0)
                    {
                        list.FinalPC = result;
                        list.RS = Math.Round(list.FinalPC * list.Cost);
                        list.OrderCTN = list.FinalPC / list.MOQ;
                        return list;

                    }
                }
            }
            catch(Exception ex)
            {
                AddLogs("Error in Dos Calcuation");
           
            }
            return null;
        }

        private void Clean_Resources()
        {
            ItemJoPrintHoChuki = 0;
            myOrderList.Clear();
            AutoScheduleLOG = "";

        }



        private void CreateNew_DOS_Order(string name, string vendord)
        {
            throw new NotImplementedException();
        }

        private void AddLogs(string log)
        {
            if (AutoScheduleLOG == "")
            {
                AutoScheduleLOG = log;
            }
        }


        private void Dashboard_Load(object sender, EventArgs e)
        {
            try
            {
               
                PriorityComboBox.Text = Properties.Settings.Default.Priority;
               
                comboBox1.Text = Properties.Settings.Default.SchTime;
                Properties.Settings.Default.Save();
                StartScheduling();
               
            }
            catch { }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Auto_OrderCreating();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
           
        }

        private void MetroButton2_Click(object sender, EventArgs e)
        {
         
            Properties.Settings.Default.Priority = PriorityComboBox.Text;
           
            Properties.Settings.Default.SchTime = comboBox1.Text;
            Properties.Settings.Default.Save();

        }

        private void MetroButton1_Click(object sender, EventArgs e)
        {
            if (!timer1.Enabled)
            {
                isForceStop = false;
                Task.Run(() =>
                Auto_OrderCreating());
            }
            else
            { MessageBox.Show("Please Stop the Schedule Timer Before Invoking Manually", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information); }

          
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void MetroButton6_Click(object sender, EventArgs e)
        {
            StartScheduling();
        }
        private void StartScheduling()
        {
            try
            {
                if (comboBox1.Text != "")
                {
                    SetStatus("ON");
                    int value = int.Parse(comboBox1.Text.Replace("min", "").Trim());
                    timer1.Interval = value * 60000;
                    CountDown = value * 60;
                    CountDowntimer.Start();
                    StartTimermetroButton.Enabled = false;
                    STOPmetroButton.Enabled = true;
                    timer1.Start();
                }
                else
                { MessageBox.Show("Schedule Time is Empty"); }

            }catch(Exception EX)
            { MessageBox.Show(EX.Message); }
            }
        private void GroupBox3_Enter(object sender, EventArgs e)
        {
             }

        private void MetroButton7_Click(object sender, EventArgs e)
        {
            StartTimermetroButton.Enabled = true;
            STOPmetroButton.Enabled = false; ;
            SetStatus("OFF");
            timer1.Stop();
            CountDowntimer.Stop();
        }

        private void MetroButton5_Click(object sender, EventArgs e)
        {
            isForceStop = true;
            DisplayStateMessage("Forcely Stop.");
            timer1.Stop();
            CountDowntimer.Stop();
        }

        private void CountDowntimer_Tick(object sender, EventArgs e)
        {
            CountDown--;
            Countlabel.Text = CountDown.ToString();
            try { label12.Text = StringValueFromDb("SELECT COUNT(*)  FROM [mbo].[PSVendorScheduleLog] WHERE Status='Sent'"); } catch { }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
         
            int value = int.Parse(comboBox1.Text.Replace("min", "").Trim());
            timer1.Interval = value * 60000;
            CountDown = value * 60;
            Application.Restart();
        }

        private void MetroButton3_Click(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Purchase Orders PDF";
           
                Process.Start("explorer.exe", path);
            
        }

        private void Dashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void MetroButton4_Click(object sender, EventArgs e)
        {
            ScheduleLog  sch = new ScheduleLog();

            sch.ShowDialog();
            //MessageBox.Show("This Feature is not Available For Now", "Pending", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }


        public bool InsertInformation(String ss)
        {

            try
            {
                if (con.con.State == ConnectionState.Open)
                { con.con.Close(); }
                con.con.Open();

                SqlCommand cmd = new SqlCommand(ss, con.con);
                cmd.ExecuteNonQuery();
                con.con.Close();

                return true;
            }
            catch (Exception ex)
            {
                AddLogs("Error Inserting DATA INTO DB. ");

                return false;
            }
        }





        string tempString;
        public String StringValueFromDb(string x)
        {
            tempString = "0";

            try
            {
                if (con.con.State == ConnectionState.Open)
                {
                    con.con.Close();
                }
                SqlDataAdapter sda = new SqlDataAdapter(x, con.con);
                DataTable dt = new DataTable();
                sda.Fill(dt);

                tempString = (dt.Rows[0][0]).ToString();



                if (con.con.State == ConnectionState.Closed)
                {
                    con.con.Open();
                }
                return tempString;

            }

            catch (Exception)
            { }
            return tempString;

        }
        public Int32 InsertValuesIntoDataBase(string query)
        {
            Int32 insertedID = -1;
            try
            { 
                 using (SqlCommand cmd = new SqlCommand(query, con.con))
                {
                    if (con.con.State == ConnectionState.Closed)
                    {
                        con.con.Open();
                    }
                    insertedID = Convert.ToInt32(cmd.ExecuteScalar());

                    if (con.con.State == System.Data.ConnectionState.Open)
                        con.con.Close();



                }
                return insertedID;
            }
            catch (Exception ex)
            {

                return -1;
            }

        }




        public DataTable getDataTableFromDB(string x)
        {
            try
            {
                if (con.con.State == ConnectionState.Open)
                { con.con.Close(); }
                con.con.Open();
                SqlCommand cmd = new SqlCommand(x, con.con);
                cmd.ExecuteNonQuery();
                DataTable dtt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtt);

                con.con.Close();
                return dtt;
            }
            catch (Exception ex)
            {
                
                AddLogs("43c) ERROR GET DATATABLE");

                return null;
            }
        }

        public void UpdateProductRecord(string updateString)
        {
            try
            {
                if (con.con.State == ConnectionState.Open)
                { con.con.Close(); }
                con.con.Open();
                SqlCommand cmd = new SqlCommand(updateString, con.con);
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            { AddLogs("4d)"+ex.Message); }

        }




        public bool Insert_Order_Items(List<PO_Model> list, string orderid)
        {
            try
            {
                var records = list;
                if (con.con.State == ConnectionState.Open)
                { con.con.Close(); }

                con.con.Open();

                SqlCommand cmd =
                    new SqlCommand(
                        "INSERT INTO [dbo].[PurchaseOrderItem]([PurchaseOrderId],[ProductItemId],[Quantity],[Price],[DiscountRate],[SystemStock],[ItemStatus])" +
                        " VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7)");
                cmd.CommandType = CommandType.Text;
                cmd.Connection = con.con;

                cmd.Parameters.Add("@param1", SqlDbType.Int);
                cmd.Parameters.Add("@param2", SqlDbType.Int);
                cmd.Parameters.Add("@param3", SqlDbType.Decimal);
                cmd.Parameters.Add("@param4", SqlDbType.Money);
                cmd.Parameters.Add("@param5", SqlDbType.Money);
                cmd.Parameters.Add("@param6", SqlDbType.Decimal);
                cmd.Parameters.Add("@param7", SqlDbType.Int);

                foreach (var item in records)
                {

                    cmd.Parameters[0].Value = orderid;
                    cmd.Parameters[1].Value = item.ProductItemID;
                    cmd.Parameters[2].Value = item.FinalPC;
                    cmd.Parameters[3].Value = item.Cost;
                    cmd.Parameters[4].Value = 0;
                    cmd.Parameters[5].Value = 0;
                    cmd.Parameters[6].Value = 1;



                    cmd.ExecuteNonQuery();
                }

                con.con.Close();
                return true;
            }
            catch (Exception ex)
            {
                AddLogs("2c) Error at Inerting PO item");
                return false;
            }
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
           

        }

        private void Dashboard_Shown(object sender, EventArgs e)
        {
            Auto_OrderCreating();
            //if (comboBox1.Text != "")
            //{ Task.Run(() =>
           
            //);
            //}
        }
    }
}

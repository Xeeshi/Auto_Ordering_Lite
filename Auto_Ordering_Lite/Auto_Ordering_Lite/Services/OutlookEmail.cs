using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Ordering_Lite.Services
{
    public class OutlookEmail
    {
        public static void SendEmail(string Receiver, string Obj, string path, string OrderID, string body)
        {

            Microsoft.Office.Interop.Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
            Microsoft.Office.Interop.Outlook.MailItem mailItem = app.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

            mailItem.Subject = Obj;
            mailItem.To = Receiver;
            mailItem.CC = "ibrahim.b@maxbachat.com";
            if (body == "")
            {
                mailItem.Body = "Dear Supplier,\nPlease find our Purchase Order attached. In case of any questions / concerns, please contact me.";
            }
            else
            {
                mailItem.Body = body;
            }
            if (path != "")
            {
                String sSource = path;
                String sDisplayName = "PO_" + OrderID;
                int iPosition = (int)mailItem.Body.Length + 1;
                int iAttachType = (int)Microsoft.Office.Interop.Outlook.OlAttachmentType.olByValue;
                mailItem.Attachments.Add(sSource, iAttachType, iPosition, sDisplayName);
            }
            mailItem.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh;
            mailItem.Display(false);
            mailItem.Send();



            mailItem = null;
            // Simple error handler.



        }
    }
}

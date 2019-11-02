using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Auto_Ordering_Lite.Services
{
    public class SendSms
    {
        public static string SendSMS(Models.SMS sms)
        {
            
            String URI = "http://csms.voguetech.com.pk" +
            "/api_sms/api.php?" +
            "key=" + sms.APIKey+
            "&receiver=" + sms.Receiver +
            "&sender=" + sms.Masking +
            "&message=" + Uri.UnescapeDataString(sms.MessageBody); // Visual Studio 10-15 
                                                               //   "//&message=" + System.Net.WebUtility.UrlEncode(MessageText);// Visual Studio 12 
            try
            {
                WebRequest req = WebRequest.Create(URI);
                WebResponse resp = req.GetResponse();
                var sr = new System.IO.StreamReader(resp.GetResponseStream());
                return sr.ReadToEnd().Trim();
            }
            catch (WebException ex)
            {
                var httpWebResponse = ex.Response as HttpWebResponse;
                if (httpWebResponse != null)
                {
                    switch (httpWebResponse.StatusCode)
                    {
                        case HttpStatusCode.NotFound:


                            return "404:URL not found :" + URI;

                        case HttpStatusCode.BadRequest:
                            return "400:Bad Request";

                            
                         
                        default:


                            var x = httpWebResponse.StatusCode.ToString();
                            return httpWebResponse.StatusCode.ToString();

                    }
                }
            }
            return null;
        }



    }
}
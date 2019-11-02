using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Auto_Ordering_Lite.Services
{
    class MergePdf
    {
        public static bool MergePDFs(List<string> fileNames, string targetPdf)
        {
            try
            {
                bool merged = true;
                using (FileStream stream = new FileStream(targetPdf, FileMode.Create))
                {
                    Document document = new Document();
                    PdfCopy pdf = new PdfCopy(document, stream);
                    PdfReader reader = null;
                    try
                    {
                        document.Open();
                        foreach (string file in fileNames)
                        {
                            reader = new PdfReader(file);
                            pdf.AddDocument(reader);
                            reader.Close();
                        }
                    }
                    catch (Exception)
                    {
                        merged = false;
                        if (reader != null)
                        {
                            reader.Close();
                        }
                    }
                    finally
                    {
                        if (document != null)
                        {
                            document.Close();
                        }
                    }
                }

                return merged;
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false; }
           
            }
    }
}

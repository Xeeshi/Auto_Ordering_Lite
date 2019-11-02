using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Ordering_Lite.Model
{
   public class PO_Model
    {
        //------- General Model-------
        public string ItemDescription { get; set; }

        
        public Double MOQ { get; set; }

        public string MOQUnit { get; set; }

        public string DOS { get; set; }
        public double Sugg { get; set; }

        public double FinalPC { get; set; }
        public Double Cost { get; set; }
        public Double RS { get; set; }

        public double OrderCTN { get; set; }
        public string ProductItemID { get; set; }

        public string Barcode { get; set; }

        public string AltBarcode { get; set; }

        public double JDCInventory { get; set; }
        public string Total_1M { get; set; }

        public string Total_2M { get; set; }
     
        public string JDCTarget { get; set; }






    }
}

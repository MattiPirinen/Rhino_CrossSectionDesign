using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;

namespace CrossSectionDesign.Static_classes
{
    public static class ExcelGlobalSettings
    {
        public static void Title1Font(Excel.Range r)
        {
            r.Font.Size = 14;
            r.Font.Name = "Calibri";
            r.Font.Bold = true;
           
        }

        public static void Title2Font(Excel.Range r)
        {
            r.Font.Size = 12;
            r.Font.Name = "Calibri";
            r.Font.Bold = true;
        }

        public static void NormalFont(Excel.Range r)
        {
            r.Font.Size = 10;
            r.Font.Name = "Calibri";
        }


    }
}

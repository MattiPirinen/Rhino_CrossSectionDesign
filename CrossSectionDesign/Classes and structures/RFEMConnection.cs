using Dlubal.RFEM5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CrossSectionDesign.Classes_and_structures
{
    public class RFEMConnection
    {
        private static IApplication _iApp;
        private static ICalculation _calc;
        
        public static IModel RModel { get; set; }

        //This method opens connection to the RFEM model
        public static void OpenConnection()
        {
            if (_iApp == null)
            {
                try
                {
                    //Get active RFEM5 application
                    _iApp = Marshal.GetActiveObject("RFEM5.Application") as IApplication;
                    _iApp.LockLicense();
                    RModel = _iApp.GetActiveModel();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //Release COM object
                    if (_iApp != null)
                    {
                        _iApp.UnlockLicense();
                        _iApp = null;
                    }
                    //Cleans Garbage collector for releasing all COM interfaces and objects
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                }
            }
        }


        //This method closes the connection to RFEM
        public static void CloseConnection()
        {
            //Release COM object
            if (_iApp != null)
            {
                _iApp.UnlockLicense();
                _iApp = null;
            }
            if (RModel != null)
            {
                RModel = null;
            }

            //Cleans Garbage collector for releasing all COM interfaces and objects
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }


    }
}

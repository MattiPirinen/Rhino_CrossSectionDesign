using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CrossSectionDesign.Classes_and_structures
{
    public class CountableUserData: Rhino.DocObjects.Custom.UserData
    {
        public bool Selected { get; set; }

        protected static int _idCounter = 0;
        public int Id { get; protected set; }

        public CountableUserData()
        {
            Id = _idCounter;
            _idCounter++;
        }

        public static void setCounter(int numb)
        {
            _idCounter = numb;
        }

        public static int getCounter()
        {
            return _idCounter;
        }


    }
}

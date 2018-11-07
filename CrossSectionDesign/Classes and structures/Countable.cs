using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CrossSectionDesign.Classes_and_structures
{
    public class Countable
    {
        protected static int _idCounter = 0;
        public int Id { get; set; }

        public static void SetCounter(int numb)
        {
            _idCounter = numb;
        }



        public Countable()
        {
            Id = _idCounter;
            _idCounter++;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CrossSectionDesign.Classes_and_structures
{
    public class UserDataList:ObservableCollection<int>
    {
        public UserDataList():base()
        {
        }
        public UserDataList(IEnumerable<int> list) : base()
        {
            foreach (int item in list)
            {
                Add(item);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.EventArgs
{
    public class ModifyElementArgs : IModifyElementArgs
    {
        //public ElementFamily Family { get; private set; }
        //public string Guid { get; private set; }
        public ModifyPropertyArgBase Update { get; private set; }

        public ModifyElementArgs(/*ElementFamily family, string guid, */ModifyPropertyArgBase update)
        {
            //Family = family;
           // Guid = guid;
            Update = update;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Events
{
    public class UpdateElementArgs : IUpdateElementArgs
    {
        //public ElementFamily Family { get; private set; }
        //public string Guid { get; private set; }
        public UpdatePropertyArgBase Update { get; private set; }

        public UpdateElementArgs(/*ElementFamily family, string guid, */UpdatePropertyArgBase update)
        {
            //Family = family;
           // Guid = guid;
            Update = update;
        }
    }
}

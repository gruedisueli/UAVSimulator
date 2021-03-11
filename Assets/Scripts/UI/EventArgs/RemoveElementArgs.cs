using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.EventArgs
{
    public class RemoveElementArgs : IRemoveElementArgs
    {
        public string Guid { get; private set; }
        public ElementFamily Family { get; private set; }

        public RemoveElementArgs(string guid, ElementFamily family)
        {
            Guid = guid;
            Family = family;
        }
    }
}

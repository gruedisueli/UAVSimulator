using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.EventArgs
{
    public interface IRemoveElementArgs
    {
        string Guid { get; }
        ElementFamily Family { get; }
    }
}

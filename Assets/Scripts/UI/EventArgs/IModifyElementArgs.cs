using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.EventArgs
{
    public interface IModifyElementArgs
    {
        //ElementFamily Family { get; }
        //string Guid { get; }
        ModifyPropertyArgBase Update { get; }
        object Sender { get; }
    }
}

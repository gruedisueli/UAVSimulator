using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.EventArgs
{
    public class ModifyElementArgs : IModifyElementArgs
    {
        public ModifyPropertyArgBase Update { get; private set; }
        public object Sender { get; private set; }

        public ModifyElementArgs(object sender, ModifyPropertyArgBase update)
        {
            Sender = sender;
            Update = update;
        }
    }
}

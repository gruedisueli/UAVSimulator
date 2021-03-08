using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Events
{
    public interface IUpdateElementArgs
    {
        UpdateElementType UpdateType { get; }
        string Guid { get; set; }
        UpdatePropertyArgBase Update { get; set; }
    }
}

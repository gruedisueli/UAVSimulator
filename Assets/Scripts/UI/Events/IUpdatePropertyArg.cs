using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Events
{
    public interface IUpdatePropertyArg<T>
    {
        UpdatePropertyType Type { get; }
        T Value { get; }
    }
}

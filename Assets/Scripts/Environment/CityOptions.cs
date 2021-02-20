using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// Class hold user-configurable city stats
    /// </summary>
    [Serializable]
    public class CityOptions
    {
        public string _name = "";
        public int _eastExt = 0;
        public int _westExt = 0;
        public int _northExt = 0;
        public int _southExt = 0;
        public int _population = 0;
        public int _jobs = 0;

        public CityOptions()
        {

        }
    }
}

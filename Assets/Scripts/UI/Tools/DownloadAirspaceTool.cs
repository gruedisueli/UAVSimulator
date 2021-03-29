using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Tools
{
    public class DownloadAirspaceTool : ToolBase
    {
        public EventHandler<System.EventArgs> OnDownloadAirspace;

        public void DownloadAirspace()
        {
            OnDownloadAirspace.Invoke(this, new System.EventArgs());
        }
    }
}

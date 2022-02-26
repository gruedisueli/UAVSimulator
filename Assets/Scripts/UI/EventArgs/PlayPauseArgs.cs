using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.EventArgs
{
    /// <summary>
    /// Sent by play/pause button.
    /// </summary>
    public class PlayPauseArgs
    {
        public bool IsPlaying { get; }

        public PlayPauseArgs(bool isPlaying)
        {
            IsPlaying = isPlaying;
        }
    }
}

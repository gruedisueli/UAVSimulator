using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts.UI.Tools
{
    public class OpenURL : MonoBehaviour
    {
        public string _url = "";

        public void GoToURL()
        {
            Application.OpenURL(_url);
        }
    }
}

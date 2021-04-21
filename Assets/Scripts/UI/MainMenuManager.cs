using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;

using Assets.Scripts.Environment;
using Assets.Scripts.Serialization;

namespace Assets.Scripts.UI
{
    public class MainMenuManager : MonoBehaviour
    {

        public void CreateNew()
        {
            EnvironManager.Instance.CreateNew();
            SceneManager.LoadScene(UISettings.FINDLOCATION_SCENEPATH, LoadSceneMode.Single);
        }

        public void LoadSaved()
        {
            EnvironManager.Instance.LoadSaved();
            SceneManager.LoadScene(UISettings.REGIONVIEW_SCENEPATH, LoadSceneMode.Single);
        }

        public void About()
        {

        }
    }
}

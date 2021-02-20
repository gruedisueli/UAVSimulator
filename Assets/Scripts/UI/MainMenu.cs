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
    public class MainMenu : MonoBehaviour
    {

        public void CreateNew()
        {
            EnvironController.Instance.CreateNew();
            SceneManager.LoadScene(UISettings.FINDLOCATION_SCENEPATH, LoadSceneMode.Single);
        }

        public void LoadSaved()
        {
            EnvironController.Instance.LoadSaved();
            SceneManager.LoadScene(UISettings.REGIONVIEW_SCENEPATH, LoadSceneMode.Single);
        }
    }
}

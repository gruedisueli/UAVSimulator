using System;
using System.Collections;
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
        public Canvas _canvas;

        public void CreateNew()
        {
            EnvironManager.Instance.CreateNew();

            StartCoroutine(LoadAsyncOperation(UISettings.FINDLOCATION_SCENEPATH));
        }

        public void LoadSaved(string name)
        {
            EnvironManager.Instance.LoadSaved(name);
            StartCoroutine(LoadAsyncOperation(UISettings.REGIONVIEW_SCENEPATH));
        }

        public void About()
        {

        }

        public void Quit()
        {
            Application.Quit();
        }

        IEnumerator LoadAsyncOperation(int scenePath)
        {
            var pG = Instantiate(EnvironManager.Instance.ProgressBarPrefab, _canvas.gameObject.transform);
            var rT = pG.transform as RectTransform;
            rT.anchoredPosition = new Vector2(0, -300);
            var progressBar = pG.GetComponent<ProgressBar>();
            progressBar.Init("LOADING");
            AsyncOperation loadScene = SceneManager.LoadSceneAsync(scenePath);
            while(loadScene.progress < 1)
            {
                progressBar.SetCompletion(loadScene.progress);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}

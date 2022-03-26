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
using Assets.Scripts.UI.Tools;

namespace Assets.Scripts.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        public Canvas _canvas;
        public ErrorDialog _errorDialog;

        //note: for stuff around loading saved json file, see https://pixeleuphoria.com/blog/index.php/2020/04/29/unity-webgl-upload-content/

        public void CreateNew()
        {
            EnvironManager.Instance.CreateNew();

            StartCoroutine(LoadAsyncOperation(UISettings.FINDLOCATION_SCENEPATH));
        }

        public void LoadSaved()
        {
            WebFileManager.BrowserTextUpload(".json", "FOA", "LoadDocumentString");
        }

        public void About()
        {

        }

        public void Quit()
        {
            Application.Quit();
        }

        public void LoadDocumentString(string str)
        {
            if (EnvironManager.Instance.LoadSaved(str))
            {
                StartCoroutine(LoadAsyncOperation(UISettings.REGIONVIEW_SCENEPATH));
            }
            else
            {
                _errorDialog.Activate();
            }
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

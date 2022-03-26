using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
public class WebFileManager : MonoBehaviour
{
    //https://pixeleuphoria.com/blog/index.php/2020/04/27/unity-webgl-download-content/
    [DllImport("__Internal")]
    public static extern void BrowserTextDownload(string filename, string textContent);

    //https://pixeleuphoria.com/blog/index.php/2020/04/29/unity-webgl-upload-content/
    [DllImport("__Internal")]
    public static extern string BrowserTextUpload(string extFilter, string gameObjName, string dataSinkFn);

    
}

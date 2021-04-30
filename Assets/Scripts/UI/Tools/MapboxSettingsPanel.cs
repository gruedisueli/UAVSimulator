using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assets.Scripts.UI.EventArgs;
using Assets.Scripts.Environment;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Tools
{
    public class MapboxSettingsPanel : PanelBase
    {
        protected Text _tokenTxt;
        protected ModifyFieldTool _modifyTool;

        public override void SetActive(bool isActive)
        {
            base.SetActive(isActive);
            _tokenTxt = GetComponentInChildren<Text>(true);
            if (_tokenTxt == null)
            {
                Debug.LogError("Could not find Text component in children of Mapbox Settings Panel");
            }
            _tokenTxt.text = "Access Token: " + EnvironManager.Instance.MapboxSettings.AccessToken;
            _modifyTool = GetComponentInChildren<ModifyFieldTool>(true);
            if (_modifyTool == null)
            {
                Debug.LogError("Could not find Modify Field Tool in children of Mapbox Settings Panel");
                return;
            }
            _modifyTool.OnElementModified += SetToken;
        }

        public void SetToken(IModifyElementArgs args)
        {
            var u = args.Update as ModifyStringPropertyArg;
            _tokenTxt.text = "Access Token: " + u.Value;
            EnvironManager.Instance.SetAccessToken(u.Value);
        }
    }
}

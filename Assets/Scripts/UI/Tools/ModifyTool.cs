using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.UI.Fields;
using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.UI.Tools
{
    public delegate void ElementModified(IModifyElementArgs args);

    /// <summary>
    /// A behavior that allows the user to modify existing simulation elements
    /// </summary>
    public abstract class ModifyTool : ToolBase
    {
        public event ElementModified OnElementModified;
        public ElementPropertyType _propertyType = ElementPropertyType.Unset;
        public VisibilityType _visibilityType = VisibilityType.Unset;

        protected bool _isVisibilityModifier = false;

        protected override void Awake()
        {
            base.Awake();

            if (_propertyType != ElementPropertyType.Unset && _visibilityType != VisibilityType.Unset)
            {
                Debug.LogError("Incorrect configuration of modification tool. Choose either a visiblity type or property type for modification");
                return;
            }

            _isVisibilityModifier = _propertyType != ElementPropertyType.Unset ? false : true;
        }

        private void Start()
        {
            Init();
        }

        /// <summary>
        /// Called when turning on the modiifer.
        /// </summary>
        public virtual void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        /// <summary>
        /// Called whenever a property is changed.
        /// </summary>
        public virtual void RegisterModification()
        {
            OnElementModified.Invoke(GatherInformation());
        }

        protected abstract void Init();
        protected abstract IModifyElementArgs GatherInformation();
    }
}

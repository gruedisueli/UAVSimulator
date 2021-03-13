using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Assets.Scripts.UI.EventArgs;

namespace Assets.Scripts.Environment
{
    [Serializable]
    public class RestrictionZoneCylStack : RestrictionZoneBase
    {
        [SerializeField]
        private string _type = "";
        public string Type
        {
            get
            {
                return _type;
            }
        }

        [SerializeField]
        private Vector3 _position = new Vector3();
        /// <summary>
        /// This is the position at the BOTTOM of the stack. Element positions are LOCAL positions relative to this parent.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        [SerializeField]
        private List<RestrictionZoneCyl> _elements = new List<RestrictionZoneCyl>();
        public List<RestrictionZoneCyl> Elements
        {
            get
            {
                return _elements;
            }
            set
            {
                _elements = value;
            }
        }

        public RestrictionZoneCylStack(Vector3 pos)
        {
            Position = pos;
        }

        public RestrictionZoneCylStack(RestrictionZoneCylStack rZ)
        {
            _type = rZ.Type;
            foreach(var e in rZ.Elements)
            {
                _elements.Add(new RestrictionZoneCyl(e));
            }
        }

        public override RestrictionZoneBase GetCopy()
        {
            return new RestrictionZoneCylStack(this);
        }

        public override void UpdateParams(ModifyPropertyArgBase args)
        {
            try
            {
                switch (args.Type)
                {
                    case ElementPropertyType.Position:
                        {
                            Position = (args as ModifyVector3PropertyArg).Value;
                            break;
                        }
                }
            }
            catch
            {
                Debug.LogError("Casting error in restriction zone property update");
                return;
            }
        }
    }
}

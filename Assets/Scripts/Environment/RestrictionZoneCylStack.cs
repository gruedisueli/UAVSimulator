using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

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

        public RestrictionZoneCylStack()
        {

        }

        public RestrictionZoneCylStack(RestrictionZoneCylStack rZ)
        {
            _type = rZ.Type;
            foreach(var e in rZ.Elements)
            {
                _elements.Add(new RestrictionZoneCyl(e));
            }
        }
    }
}

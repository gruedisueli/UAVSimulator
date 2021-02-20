using Mapbox.Unity.Map;

namespace Assets.Scripts.MapboxCustom
{
	using System;
	using UnityEngine;
	[Serializable]
	public class FOA_RangeAroundTransformTileProviderOptions : ExtentOptions
	{
		public Transform _targetTransform;
        public int _eastExt = 0;
        public int _westExt = 0;
        public int _northExt = 0;
        public int _southExt = 0;
        public int _visibleBuffer = 1;
        public int _disposeBuffer = 1;

		public override void SetOptions(ExtentOptions extentOptions)
		{
            FOA_RangeAroundTransformTileProviderOptions options = extentOptions as FOA_RangeAroundTransformTileProviderOptions;
			if (options != null)
			{
				SetOptions(options._targetTransform, options._eastExt, options._westExt, options._northExt, options._southExt, options._visibleBuffer, options._disposeBuffer);
			}
			else
			{
				Debug.LogError("ExtentOptions type mismatch : Using " + extentOptions.GetType() + " to set extent of type " + this.GetType());
			}
		}
		public void SetOptions(Transform tgtTransform = null, int eastExt = 0, int westExt = 0, int northExt = 0, int southExt = 0, int visibleBuffer = 1, int disposeRange = 1)
		{
			_targetTransform = tgtTransform;
            _eastExt = eastExt;
            _westExt = westExt;
            _northExt = northExt;
            _southExt = southExt;
			_visibleBuffer = visibleBuffer;
			_disposeBuffer = disposeRange;
		}
	}
}

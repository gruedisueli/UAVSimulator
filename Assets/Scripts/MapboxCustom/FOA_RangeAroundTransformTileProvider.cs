using System.Collections.Generic;
using Mapbox.Map;
using UnityEngine;

using Mapbox.Unity.Map.TileProviders;

namespace Assets.Scripts.MapboxCustom
{
    /// <summary>
    /// A special tile provider for our FOA project, based on the Mapbox RangeAroundTransformTileProvider.
    /// </summary>
    public class FOA_RangeAroundTransformTileProvider : AbstractTileProvider
	{
		[SerializeField] private FOA_RangeAroundTransformTileProviderOptions _rangeTileProviderOptions;

		private bool _initialized = false;
		private UnwrappedTileId _currentTile;
		private bool _waitingForTargetTransform = false;

		public override void OnInitialized()
		{
			if (Options != null)
			{
				_rangeTileProviderOptions = (FOA_RangeAroundTransformTileProviderOptions)Options;
			}
			else if (_rangeTileProviderOptions == null)
			{
				_rangeTileProviderOptions = new FOA_RangeAroundTransformTileProviderOptions();
			}

			if (_rangeTileProviderOptions._targetTransform == null)
			{
				Debug.LogError("TransformTileProvider: No location marker transform specified.");
				_waitingForTargetTransform = true;
			}
			else
			{
				_initialized = true;
			}
			_currentExtent.activeTiles = new HashSet<UnwrappedTileId>();
			_map.OnInitialized += UpdateTileExtent;
			_map.OnUpdated += UpdateTileExtent;
		}

		public override void UpdateTileExtent()
		{
			if (!_initialized) return;

			_currentExtent.activeTiles.Clear();
			_currentTile = TileCover.CoordinateToTileId(_map.WorldToGeoPosition(_rangeTileProviderOptions._targetTransform.localPosition), _map.AbsoluteZoom);

            //note: x values increase going east, and y values increase going south
			for (int x = _currentTile.X - _rangeTileProviderOptions._westExt; x <= (_currentTile.X + _rangeTileProviderOptions._eastExt); x++)
			{
				for (int y = _currentTile.Y - _rangeTileProviderOptions._northExt; y <= (_currentTile.Y + _rangeTileProviderOptions._southExt); y++)
				{
					_currentExtent.activeTiles.Add(new UnwrappedTileId(_map.AbsoluteZoom, x, y));
				}
			}
			OnExtentChanged();
		}

		public override void UpdateTileProvider()
		{
			if (_waitingForTargetTransform && !_initialized)
			{
				if (_rangeTileProviderOptions._targetTransform != null)
				{
					_initialized = true;
				}
			}

			if (_rangeTileProviderOptions != null && _rangeTileProviderOptions._targetTransform != null && _rangeTileProviderOptions._targetTransform.hasChanged)
			{
				UpdateTileExtent();
				_rangeTileProviderOptions._targetTransform.hasChanged = false;
			}
		}

		public override bool Cleanup(UnwrappedTileId tile)
		{
			bool dispose = false;
			dispose = tile.X > _currentTile.X + _rangeTileProviderOptions._disposeBuffer || tile.X < _currentTile.X - _rangeTileProviderOptions._disposeBuffer;
			dispose = dispose || tile.Y > _currentTile.Y + _rangeTileProviderOptions._disposeBuffer || tile.Y < _currentTile.Y - _rangeTileProviderOptions._disposeBuffer;


			return (dispose);
		}
	}
}
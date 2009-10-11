using MapsLibrary;
using System;
using System.Drawing;

namespace MapperTools.Pollicino
{
    public partial class Form_MapperToolMain : System.Windows.Forms.Form
    {
        protected CachedTilesMap map;
        protected LayeredMap lmap;
        protected LayerPoints trackpoints;
        protected LayerPoints waypoints;

        private void InitMaps()
        {
            lmap = new LayeredMap();
            map = new CachedTilesMapDL(options.Maps.TileCachePath, 
                                                SelectActiveMap(0), 
                                                required_buffers(true));
            lmap.addLayerOnTop(map);
            // Tracciato GPS
            trackpoints = new LayerPoints(map.mapsystem);
            waypoints = new LayerPoints(map.mapsystem);
            waypoints.SetDrawPointFunction(LayerPoints.DrawEmptySquare, new Pen(Color.Red));
            idx_layer_trkpnt = lmap.addLayerOnTop(trackpoints);
            idx_layer_waypnt = lmap.addLayerOnTop(waypoints);
        }

        private int idx_layer_trkpnt = 0, idx_layer_waypnt = 0;
        /// <summary>
        /// Indice nell'array options.Maps.ActiveTileMaps del MapSystem attualmente utilizzato da map.
        /// </summary>
        private int currentMapIdx;
        
        public void SetTrackVisibility(bool v)
        {
            lmap.setVisibility(idx_layer_trkpnt, v);
            lmap.setVisibility(idx_layer_waypnt, v);
        }

        public void RefreshActiveMapsList()
        {
            map.mapsystem = SelectActiveMap(0);
        }

        private TileMapSystem SelectActiveMap(int idx)
        {
            try
            {
                string newMapSysName = options.Maps.ActiveTileMaps[idx];
                TileMapSystem retval = (TileMapSystem)options.Maps.TileMaps[newMapSysName];
                currentMapIdx = idx;
                return retval;
            }
            catch (Exception e)
            {
                return OSMTileMapSystem.CreateOSMMapSystem(OSMTileMapSystem.ServerUrl_Mapnik);
            }
        }

        public void ShowNextMap()
        {
            int idx = currentMapIdx + 1;
            if (idx >= options.Maps.ActiveTileMaps.Count) idx = 0;
            map.mapsystem = SelectActiveMap(idx);
        }

        public void ShowPrevMap()
        {
            map.mapsystem = SelectActiveMap((currentMapIdx > 0 ? currentMapIdx : options.Maps.ActiveTileMaps.Count) - 1);
        }
    }
}


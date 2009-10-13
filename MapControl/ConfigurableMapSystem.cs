/*******************************************************************************
 *  Mappo! - A tool for gps mapping.
 *  Copyright (C) 2008  Marco Polci
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see http://www.gnu.org/licenses/gpl.html.
 * 
 ******************************************************************************/

using System;
using System.Text;
using System.Xml.Serialization;

namespace MapsLibrary
{
    /// <summary>
    /// TileMapSystem generico fatto per essere serializzato.
    /// </summary>
    [Serializable]
    //[XmlInclude(typeof(XYZMapSystem)), XmlInclude(typeof(QuadTileMapSystem))]
    public class ConfigurableMapSystem : TileMapSystem
    {
        private int _srvidx = 0;

        /// <summary>
        /// Elenco dei server da dove scaricare i tile. Verranno utilizzati secondo una politica di round robin.
        /// </summary>
        public string[] TileServers { get; set; }

        /// <summary>
        /// Protocollo da utilizzare nell'url dei tile. Es: HTTP, HTTPS, FTP.
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// Pattern da utilizzare per creare il path nell'url di un tile.
        /// Questa stringa sarà poi utilizzata nelle implementazioni di GetPath() come format string in una chiamata a String.Format().
        /// Tale stringa può contenere al suo interno le seguenti sottostringhe come parametri:
        /// <list type="table">
        /// <listheader><term>Parametro</term><description>Valore</description></listheader>
        /// <item><term>{0}</term><description>Livello di zoom del tile.</description></item>
        /// <item><term>{1}</term><description>Coordinata X del tile.</description></item>
        /// <item><term>{2}</term><description>Coordinata Y del tile.</description></item>
        /// <item><term>{3}</term><description>Rappresentazione come quadtile delle coordinate del tile. 
        /// Tale valore è non nullo solo se è impostata la proprietà QuadPattern.</description></item>
        /// </list>
        /// 
        /// Esempi (url - pattern):
        ///   http://tah.openstreetmap.org/Tiles/tile/12/3/17  - /Tiles/tile/{0}/{1}/{2}
        ///   http://mt0.google.com/vt/x=3&y=17&z=12           - /vt/x={1}&y={2}&z={0}
        /// </summary>
        public string UrlLocationPattern { get; set; }

        private string _qchars;
        /// <summary>
        /// Stringa di 4 caratteri ognuno identificante rispettivamente i quadranti alto sinistro, alto destro, basso sinistro, basso destro nei quali è suddiviso ogni tile.
        /// </summary>
        public string QuadPattern
        {
            get { return _qchars; }
            set
            {
                if (value != null && value.Length != 4)
                    throw new InvalidOperationException("The pattern must be null or a 4 chars length string");
                _qchars = value;
            }
        }

        #region Constructors 

        public ConfigurableMapSystem(string[] servers, string pattern)
        {
            Protocol = "http";
            TileServers = servers;
            UrlLocationPattern = pattern;

            //Uri srvuri = new Uri(protocol + "://" + servers[0]);
            //FIXME: sistemare l'impostazione di id
            //id = servers[0]; // +'_' + TileUrl(new TileNum(0, 0, 1)).GetHashCode().ToString("X8");
            identifier = servers[0];
            PixelZoomFactor = 8;
        }

        public ConfigurableMapSystem(string[] servers, string pathpattern, string quadpattern)
            : this(servers, pathpattern)
        {
            QuadPattern = quadpattern;
        }

        public ConfigurableMapSystem(string server, string pathpattern, string quadpattern)
            : this(new string[] { server }, pathpattern, quadpattern)
        { }

        public ConfigurableMapSystem(string server, string pathpattern)
            : this(new string[] { server }, pathpattern)
        { }

        public ConfigurableMapSystem()
        { }

        #endregion

        protected string getQuad(TileNum tn)
        {
            int X = tn.X;
            int Y = tn.Y;
            char[] qarray = new char[tn.uZoom];
            for (int i = (int)tn.uZoom - 1; i >= 0 ; i--)
            {
                int zx = X & 0x01;
                int zy = Y & 0x01;
                X >>= 1;
                Y >>= 1;
                qarray[i] = _qchars[zx + zy * 2];
            }
            return new string(qarray);
        }

        protected string getServer()
        {
            _srvidx = (_srvidx + 1) % TileServers.Length;
            return TileServers[_srvidx];
        }

        /// <summary>
        /// Restituisce il componente path dell'url di un tile.
        /// </summary>
        /// <param name="tn">Coordinate del tile.</param>
        protected string getPath(TileNum tn)
        {
            string quad = (_qchars != null) ? getQuad(tn) : null;
            return string.Format(UrlLocationPattern, tn.uZoom, tn.X, tn.Y, quad);
        }

        /// <summary>
        /// Restituisce l'URL dell'immagine del tile
        /// </summary>
        public override string TileUrl(TileNum tn)
        {
            return Protocol + "://" + getServer() + getPath(tn); 
        }
        
        //private uint maxzoom;
        //public override uint MaxZoom { get { return maxzoom; } }
        //private uint zoomf;
        //public override uint PixelZoomFactor { get { return zoomf; } }
        //private string id;
        //public override string identifier { get { return id; } }
        public override string identifier { get; set; }
        public override uint MaxZoom { get; set; }
        public override uint PixelZoomFactor { get; set; }
  
    }

    //[Serializable]
    //public class XYZMapSystem : ConfigurableMapSystem
    //{
    //    public XYZMapSystem(string protocol, string server, string pattern)
    //        : this(protocol, new string[] { server }, pattern)
    //    { }

    //    public XYZMapSystem(string protocol, string[] servers, string pattern)
    //        : base(protocol, servers, pattern)
    //    { }

    //    public XYZMapSystem()
    //    { }

    //}


    //[Serializable]
    //public class QuadTileMapSystem : ConfigurableMapSystem
    //{
    //    public QuadTileMapSystem(string protocol, string server, string pathpattern, string quadpattern)
    //        : this(protocol, new string[] { server }, pathpattern, quadpattern)
    //    { }

    //    public QuadTileMapSystem(string protocol, string[] servers, string pathpattern, string quadpattern)
    //        : base(protocol, servers, pathpattern)
    //    {
    //        QuadPattern = quadpattern;
    //    }

    //    public QuadTileMapSystem() 
    //    { }


    //    protected override string getPath(TileNum tn)
    //    {
    //        return string.Format(UrlLocationPattern, getQuad(tn));
    //    }
    //}
}

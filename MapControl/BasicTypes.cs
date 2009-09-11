/*******************************************************************************
 *  Pollicino - A tool for gps mapping.
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
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace MapsLibrary
{
    using PxType = Int32;
    using TileIdxType = Int32;

    public enum AreaIntersectionType
    {
        noItersection,
        partialIntersection,
        fullContains,
        fullContained,
    }

    public struct GeoPoint
    {
        public double dLat;
        public double dLon;

        public GeoPoint(double lat, double lon)
        {
            dLat = lat;
            dLon = lon;
        }

        public static GeoPoint middle(GeoPoint p1, GeoPoint p2)
        {
            return new GeoPoint((p1.dLat + p2.dLat) / 2, (p1.dLon + p2.dLon) / 2);
        }

        public override string ToString()
        {
            return "Lat: " + dLat.ToString("F7") + " Lon: " + dLon.ToString("F7");
        }

        /// <summary>
        /// Distance in meters, using the Harversine Formula (Great circle).
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Distance in meters</returns>
        /// <remarks>
        /// See:
        /// http://en.wikipedia.org/wiki/Great-circle_distance
        /// http://williams.best.vwh.net/avform.htm (Aviation Formulary)
        /// 
        /// Source code from SharpGPS http://www.codeplex.com/SharpGPS
        /// </remarks>
        public double Distance(GeoPoint other)
        {
            const double rad = Math.PI / 180;
            const double NauticalMile = 1852;

            double lon1 = rad * -dLon;
            double lat1 = rad * dLat;
            double lon2 = rad * -other.dLon;
            double lat2 = rad * other.dLat;

            double d = 2 * Math.Asin(Math.Sqrt(
                Math.Pow(Math.Sin((lat1 - lat2) / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin((lon1 - lon2) / 2), 2)
            ));
            return (double)(NauticalMile * 60 * d / rad);
        }

    }
    
    public struct GeoArea
    {

        public GeoPoint pMin;
        public GeoPoint pMax;

        public GeoArea(GeoPoint min, GeoPoint max)
        {
            double minLat, maxLat, minLon, maxLon;
            if (min.dLat <= max.dLat) {
                minLat = min.dLat;
                maxLat = max.dLat;
            } else {
                minLat = max.dLat;
                maxLat = min.dLat;
            }
            if (min.dLon <= max.dLon) {
                minLon = min.dLon;
                maxLon = max.dLon;
            } else {
                minLon = max.dLon;
                maxLon = min.dLon;
            }
            this.pMin = new GeoPoint(minLat, minLon);
            this.pMax = new GeoPoint(maxLat, maxLon);
        }

        public GeoPoint center
        {
            get
            {
                return GeoPoint.middle(pMin, pMax);
            }
        }

        public bool contains(GeoPoint point)
        {
            return (pMin.dLat <= point.dLat && point.dLat <= pMax.dLat) &&
                   (pMin.dLon <= point.dLon && point.dLon <= pMax.dLon);
        }

        public AreaIntersectionType testIntersection(GeoArea testarea)
        {
            if (pMin.dLat <= testarea.pMin.dLat)
            {
                if (pMax.dLat >= testarea.pMax.dLat)
                {   //pieno contenimento sulla latitudine
                    switch (testLonInt(testarea)) {
                        case AreaIntersectionType.fullContains:
                            return AreaIntersectionType.fullContains;
                        case AreaIntersectionType.noItersection:
                            return AreaIntersectionType.noItersection;
                        default:
                            return AreaIntersectionType.partialIntersection;
                    }
                } 
                else if (pMax.dLat >= testarea.pMin.dLat) 
                {   // intersezione sulla latitudine
                    return (testLonInt(testarea) == AreaIntersectionType.noItersection) ? 
                           AreaIntersectionType.noItersection : AreaIntersectionType.partialIntersection;
                } 
                else 
                { // nessuna intersezione sulla latitudine
                    return AreaIntersectionType.noItersection;
                }
            }
            else
            {
                if (pMax.dLat <= testarea.pMax.dLat)
                {   // pieno contenimento sulla latitudine da parte di testarea
                    switch (testLonInt(testarea)) {
                        case AreaIntersectionType.fullContained:
                            return AreaIntersectionType.fullContained;
                        case AreaIntersectionType.noItersection:
                            return AreaIntersectionType.noItersection;
                        default:
                            return AreaIntersectionType.partialIntersection;
                    }
                }
                else if (pMin.dLat <= testarea.pMax.dLat)
                {   // intersezione sulla latitudine
                    return (testLonInt(testarea) == AreaIntersectionType.noItersection) ? 
                           AreaIntersectionType.noItersection : AreaIntersectionType.partialIntersection;
                } 
                else 
                { // nessuna intersezione sulla latitudine
                    return AreaIntersectionType.noItersection;
                }
            }
        }

        private AreaIntersectionType testLonInt(GeoArea testarea)
        {
            if (pMin.dLon <= testarea.pMin.dLon)
            {
                if (pMax.dLon >= testarea.pMax.dLon)
                {   //pieno contenimento sulla longitudine
                    return AreaIntersectionType.fullContains;
                } 
                else if (pMax.dLon >= testarea.pMin.dLon)
                {   // intersezione sulla longitudine
                    return AreaIntersectionType.partialIntersection;
                } 
                else
                { // nessuna intersezione sulla longitudine
                    return AreaIntersectionType.noItersection;
                }
            }
            else
            {
                if (pMax.dLon <= testarea.pMax.dLon)
                {   // pieno contenimento sulla longitudine da parte di testarea
                            return AreaIntersectionType.fullContained;
                }
                else if (pMin.dLon <= testarea.pMax.dLon)
                {   // intersezione sulla longitudine
                    return AreaIntersectionType.partialIntersection;
                } 
                else
                { // nessuna intersezione sulla longitudine
                    return AreaIntersectionType.noItersection;
                }
            }
            
        }

        public override string ToString()
        {
            return "[(" + this.pMin.ToString() + ") (" + pMax.ToString() + ")]";
        }


    }

    /// <summary>
    /// Rappresentazione con numeri interi a 32 bit della proiezione delle coordinate geografiche
    /// </summary>
    public struct ProjectedGeoPoint
    {
        public Int32 nLat;
        public Int32 nLon;

        public ProjectedGeoPoint(Int32 lat, Int32 lon)
        {
            nLat = lat;
            nLon = lon;
        }

        public static ProjectedGeoPoint middle(ProjectedGeoPoint p1, ProjectedGeoPoint p2)
        {
            return new ProjectedGeoPoint((p1.nLat + p2.nLat) / 2, (p1.nLon + p2.nLon) / 2);
        }

        public override string ToString()
        {
            return "(nLat: " + nLat.ToString("X8") + " nLon: " + nLon.ToString("X8") + ")";
        }

        public static bool operator ==(ProjectedGeoPoint p1, ProjectedGeoPoint p2)
        {
            return p1.nLat == p2.nLat && p1.nLon == p2.nLon;
        }
        public static bool operator !=(ProjectedGeoPoint p1, ProjectedGeoPoint p2)
        {
            return p1.nLat != p2.nLat || p1.nLon != p2.nLon;
        }

        public static Int32 distanceXY(ProjectedGeoPoint p1, ProjectedGeoPoint p2)
        {
            return Math.Max(Math.Abs(p1.nLat - p2.nLat), Math.Abs(p1.nLon - p2.nLon));
        }

        public static ProjectedGeoPoint operator -(ProjectedGeoPoint a, ProjectedGeoPoint b)
        {
            return new ProjectedGeoPoint(a.nLat - b.nLat, a.nLon - b.nLon);
        }
        public static ProjectedGeoPoint operator +(ProjectedGeoPoint a, ProjectedGeoPoint b)
        {
            return new ProjectedGeoPoint(a.nLat + b.nLat, a.nLon + b.nLon);
        }
        public static ProjectedGeoPoint operator /(ProjectedGeoPoint num, int den)
        {
            return new ProjectedGeoPoint(num.nLat / den, num.nLon / den);
        }

    }

    public struct ProjectedGeoArea
    {

        public ProjectedGeoPoint pMin;
        public ProjectedGeoPoint pMax;

        public ProjectedGeoArea(ProjectedGeoPoint min, ProjectedGeoPoint max)
        {
            Int32 minLat, maxLat, minLon, maxLon;
            if (min.nLat <= max.nLat)
            {
                minLat = min.nLat;
                maxLat = max.nLat;
            }
            else
            {
                minLat = max.nLat;
                maxLat = min.nLat;
            }
            if (min.nLon <= max.nLon)
            {
                minLon = min.nLon;
                maxLon = max.nLon;
            }
            else
            {
                minLon = max.nLon;
                maxLon = min.nLon;
            }
            this.pMin = new ProjectedGeoPoint(minLat, minLon);
            this.pMax = new ProjectedGeoPoint(maxLat, maxLon);
        }

        public ProjectedGeoPoint center
        {
            get
            {
                return ProjectedGeoPoint.middle(pMin, pMax);
            }
        }

        public Int32 width
        {
            get
            {
                return pMax.nLon - pMin.nLon;
            }
        }

        public Int32 height
        {
            get
            {
                return pMax.nLat - pMin.nLat;
            }
        }

        /// <summary>
        /// </summary>
        public bool contains(ProjectedGeoPoint point)
        {
            return (pMin.nLat <= point.nLat && point.nLat <= pMax.nLat) &&
                   (pMin.nLon <= point.nLon && point.nLon <= pMax.nLon);
        }

        public static ProjectedGeoArea Intersection(ProjectedGeoArea a, ProjectedGeoArea b)
        {
            Int32 minlat = Math.Max(a.pMin.nLat, b.pMin.nLat),
                  minLon = Math.Max(a.pMin.nLon, b.pMin.nLon),
                  maxLat = Math.Min(a.pMax.nLat, b.pMax.nLat),
                  maxLon = Math.Min(a.pMax.nLon, b.pMax.nLon);
            if (minlat > maxLat || minLon > maxLon)
                return new ProjectedGeoArea(new ProjectedGeoPoint(0, 0), new ProjectedGeoPoint(0, 0));
            else
                return new ProjectedGeoArea(new ProjectedGeoPoint(minlat, minLon), new ProjectedGeoPoint(maxLat, maxLon));
        }

        public AreaIntersectionType testIntersection(ProjectedGeoArea testarea)
        {
            if (pMin.nLat <= testarea.pMin.nLat)
            {
                if (pMax.nLat >= testarea.pMax.nLat)
                {   //pieno contenimento sulla latitudine
                    switch (testLonInt(testarea))
                    {
                        case AreaIntersectionType.fullContains:
                            return AreaIntersectionType.fullContains;
                        case AreaIntersectionType.noItersection:
                            return AreaIntersectionType.noItersection;
                        default:
                            return AreaIntersectionType.partialIntersection;
                    }
                }
                else if (pMax.nLat >= testarea.pMin.nLat)
                {   // intersezione sulla latitudine
                    return (testLonInt(testarea) == AreaIntersectionType.noItersection) ?
                           AreaIntersectionType.noItersection : AreaIntersectionType.partialIntersection;
                }
                else
                { // nessuna intersezione sulla latitudine
                    return AreaIntersectionType.noItersection;
                }
            }
            else
            {
                if (pMax.nLat <= testarea.pMax.nLat)
                {   // pieno contenimento sulla latitudine da parte di testarea
                    switch (testLonInt(testarea))
                    {
                        case AreaIntersectionType.fullContained:
                            return AreaIntersectionType.fullContained;
                        case AreaIntersectionType.noItersection:
                            return AreaIntersectionType.noItersection;
                        default:
                            return AreaIntersectionType.partialIntersection;
                    }
                }
                else if (pMin.nLat <= testarea.pMax.nLat)
                {   // intersezione sulla latitudine
                    return (testLonInt(testarea) == AreaIntersectionType.noItersection) ?
                           AreaIntersectionType.noItersection : AreaIntersectionType.partialIntersection;
                }
                else
                { // nessuna intersezione sulla latitudine
                    return AreaIntersectionType.noItersection;
                }
            }
        }

        private AreaIntersectionType testLonInt(ProjectedGeoArea testarea)
        {
            if (pMin.nLon <= testarea.pMin.nLon)
            {
                if (pMax.nLon >= testarea.pMax.nLon)
                {   //pieno contenimento sulla longitudine
                    return AreaIntersectionType.fullContains;
                }
                else if (pMax.nLon >= testarea.pMin.nLon)
                {   // intersezione sulla longitudine
                    return AreaIntersectionType.partialIntersection;
                }
                else
                { // nessuna intersezione sulla longitudine
                    return AreaIntersectionType.noItersection;
                }
            }
            else
            {
                if (pMax.nLon <= testarea.pMax.nLon)
                {   // pieno contenimento sulla longitudine da parte di testarea
                    return AreaIntersectionType.fullContained;
                }
                else if (pMin.nLon <= testarea.pMax.nLon)
                {   // intersezione sulla longitudine
                    return AreaIntersectionType.partialIntersection;
                }
                else
                { // nessuna intersezione sulla longitudine
                    return AreaIntersectionType.noItersection;
                }
            }

        }

        public override string ToString()
        {
            return '(' + pMin.ToString() + ' ' + pMax.ToString() + ')';
        }

        /// <summary>
        /// Calcola la differenza fra l'area dell'oggetto e l'area indicata come parametro.
        /// </summary>
        public ProjectedGeoArea[] difference(ProjectedGeoArea area)
        {
            ArrayList zones = new ArrayList();
            
            if (area.pMax.nLat < this.pMin.nLat || area.pMin.nLat > this.pMax.nLat || area.pMax.nLon < this.pMin.nLon || area.pMin.nLon > this.pMax.nLon) {
                // nessuna intersezione
                zones.Add(this);
            } else {
                Int32 top = Math.Min(area.pMin.nLat, this.pMax.nLat),
                      bottom = Math.Max(area.pMax.nLat, this.pMin.nLat);
                if (area.pMin.nLat > pMin.nLat) {
                    // fascia superiore
                    zones.Add( new ProjectedGeoArea(pMin, new ProjectedGeoPoint(top, this.pMax.nLon)) );
                }
                if (area.pMax.nLat < pMax.nLat) {
                    // fascia inferiore
                    zones.Add( new ProjectedGeoArea(new ProjectedGeoPoint(bottom, pMin.nLon), this.pMax) );
                }
                if (area.pMin.nLon > this.pMin.nLon) {
                    // fascia di sinistra
                    zones.Add( new ProjectedGeoArea(new ProjectedGeoPoint(top, this.pMin.nLon), new ProjectedGeoPoint(bottom, Math.Min(area.pMin.nLon, this.pMax.nLon))) );
                }
                if (area.pMax.nLon < this.pMax.nLon) {
                    // fascia di destra
                    zones.Add( new ProjectedGeoArea(new ProjectedGeoPoint(top, Math.Max(area.pMax.nLon, this.pMin.nLon)), new ProjectedGeoPoint(bottom, this.pMax.nLon)) );
                }
            }
            ProjectedGeoArea[] retval = new ProjectedGeoArea[zones.Count];
            zones.CopyTo(retval);
            return retval;
        }

        public void translate(ProjectedGeoPoint delta)
        {
            this.pMin += delta;
            this.pMax += delta;
        }
    }

    public struct PxCoordinates
    {
        public PxType xpx;
        public PxType ypx;

        public PxCoordinates(PxType x, PxType y)
        {
            xpx = x;
            ypx = y;
        }

        public static PxCoordinates operator -(PxCoordinates a, PxCoordinates b)
        {
            return new PxCoordinates(a.xpx - b.xpx, a.ypx - b.ypx);
        }
        public static PxCoordinates operator +(PxCoordinates a, PxCoordinates b)
        {
            return new PxCoordinates(a.xpx + b.xpx, a.ypx + b.ypx);
        }
        public static PxCoordinates operator /(PxCoordinates n, int d)
        {
            return new PxCoordinates(n.xpx / d, n.ypx / d);
        }
        public static PxCoordinates operator *(PxCoordinates n, int m)
        {
            return new PxCoordinates(n.xpx * m, n.ypx * m);
        }

        public static explicit operator System.Drawing.Point(PxCoordinates p)
        {
            return new Point((int)p.xpx, (int)p.ypx);
        }

        public static implicit operator PxCoordinates(System.Drawing.Point p)
        {
            return new PxCoordinates(p.X, p.Y);
        }
        public override string ToString()
        {
            return "(x: " + this.xpx.ToString() + " y: " + this.ypx.ToString() + ")"; 
        }
    }

    public struct TileNum
    {
        public TileIdxType X, Y;
        public uint uZoom;

        public TileNum(TileIdxType x, TileIdxType y, uint zoom)
        {
            X = x; Y = y; uZoom = zoom;
        }

        public string ToString(char separator)
        {
            return uZoom.ToString() + separator + X.ToString() + separator + Y.ToString();
        }

        public override string ToString()
        {
            return this.ToString('/');
        }
/*
        public static bool operator ==(TileNum t1, TileNum t2)
        {
            return t1.uZoom == t2.uZoom && t1.X == t2.X && t1.Y == t2.Y;
        }
*/
    }
}

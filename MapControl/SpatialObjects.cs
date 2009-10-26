using System;
using System.Collections.Generic;

namespace MapsLibrary
{
    public class SORectArea : SpatialObject
    {
        public SORectArea(ProjectedGeoArea area)
        {
            _containingArea = area;
        }
        private ProjectedGeoArea _containingArea;
        public override ProjectedGeoArea ContainingArea { get { return _containingArea; } }

        public override bool Contains(ProjectedGeoPoint point)
        {
            return this.ContainingArea.contains(point);
        }
    }

    public class SOLine : SpatialObject
    {
        public ProjectedGeoPoint begin { get; private set; }
        public ProjectedGeoPoint end { get; private set; }
        public SOLine(ProjectedGeoPoint begin, ProjectedGeoPoint end)
        {
            this.begin = begin;
            this.end = end;
            _containingArea = new ProjectedGeoArea(begin, end);
        }
        private ProjectedGeoArea _containingArea;
        public override ProjectedGeoArea ContainingArea { get { return _containingArea; } }

        public override bool Contains(ProjectedGeoPoint point)
        {
            return false;
        }
    }

    public class SOCircularArea : SpatialObject
    {
        public ProjectedGeoPoint center { get; private set; }
        public double radius { get; private set; }

        /// <param name="center"></param>
        /// <param name="radius">Raggio in metri</param>
        /// <param name="mapsystem"></param>
        public SOCircularArea(ProjectedGeoPoint center, double radius, MercatorProjectionMapSystem mapsystem)
        {
            GeoPoint geocenter = mapsystem.CalcInverseProjection(center);
            GeoPoint gpDistRef = geocenter;
            const double lonRef = 1;
            gpDistRef.dLon += lonRef;
            double distRef = geocenter.Distance(gpDistRef);
            double lonDistance = radius / distRef * lonRef;
            const double latRef = 1;
            gpDistRef = geocenter;
            gpDistRef.dLat += latRef;
            distRef = geocenter.Distance(gpDistRef);
            double latDistance = radius / distRef * latRef;
            GeoPoint gpMin = new GeoPoint(geocenter.dLat - latDistance, geocenter.dLon - lonDistance);
            GeoPoint gpMax = new GeoPoint(geocenter.dLat + latDistance, geocenter.dLon + lonDistance);
            ProjectedGeoPoint pMin = mapsystem.CalcProjection(gpMin);
            ProjectedGeoPoint pMax = mapsystem.CalcProjection(gpMax);
            _containingArea = new ProjectedGeoArea(pMin, pMax);
            this.center = center;
            //msys = mapsystem;
        }

        private ProjectedGeoArea _containingArea;
        public override ProjectedGeoArea ContainingArea { get { return _containingArea; } }

        public override bool Contains(ProjectedGeoPoint point)
        {
            long c1 = center.nLat - point.nLat,
                 c2 = center.nLon - point.nLon;
            long rad = (_containingArea.pMax.nLon - _containingArea.pMin.nLon) / 2; ;
            c1 *= c1;
            c2 *= c2;
            rad *= rad;
            return rad >= (c1 + c2);
        }
    }
}
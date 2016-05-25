using System;
using System.Collections.Generic;
using System.Linq;

namespace RecipeParser
{
    // Algorithm from https://www.yzuzun.com/2015/07/dbscan-clustering-algorithm-and-c-implementation/
    public class DbscanClustering
    {
        private readonly Func<DatasetItem, DatasetItem, double> _metricFunc;

        public DbscanClustering(Func<DatasetItem, DatasetItem, double> metricFunc)
        {
            _metricFunc = metricFunc;
        }

        public void ComputeClusterDbscan(DatasetItem[] allPoints, double epsilon, int minPts, out HashSet<DatasetItem[]> clusters)
        {
            clusters = null;
            var allPointsDbscan = allPoints.Select(x => new DbscanPoint(x)).ToArray();
            var C = 0;
            for (int i = 0; i < allPointsDbscan.Length; i++)
            {
                var p = allPointsDbscan[i];
                if (p.IsVisited)
                    continue;
                p.IsVisited = true;

                DbscanPoint[] neighborPts = null;
                RegionQuery(allPointsDbscan, p.ClusterPoint, epsilon, out neighborPts);
                if (neighborPts.Length < minPts)
                    p.ClusterId = (int)ClusterIds.NOISE;
                else
                {
                    C++;
                    ExpandCluster(allPointsDbscan, p, neighborPts, C, epsilon, minPts);
                }
            }
            clusters = new HashSet<DatasetItem[]>(
                allPointsDbscan
                    .Where(x => x.ClusterId > 0)
                    .GroupBy(x => x.ClusterId)
                    .Select(x => x.Select(y => y.ClusterPoint).ToArray())
                );
        }

        private void ExpandCluster(DbscanPoint[] allPoints, DbscanPoint p, DbscanPoint[] neighborPts, int c, double epsilon, int minPts)
        {
            p.ClusterId = c;
            for (int i = 0; i < neighborPts.Length; i++)
            {
                var pn = neighborPts[i];
                if (!pn.IsVisited)
                {
                    pn.IsVisited = true;
                    DbscanPoint[] neighborPts2 = null;
                    RegionQuery(allPoints, pn.ClusterPoint, epsilon, out neighborPts2);
                    if (neighborPts2.Length >= minPts)
                    {
                        neighborPts = neighborPts.Union(neighborPts2).ToArray();
                    }
                }
                if (pn.ClusterId == (int)ClusterIds.UNCLASSIFIED)
                    pn.ClusterId = c;
            }
        }

        private void RegionQuery(DbscanPoint[] allPoints, DatasetItem p, double epsilon, out DbscanPoint[] neighborPts)
        {
            neighborPts = allPoints.Where(x => _metricFunc(p, x.ClusterPoint) <= epsilon).ToArray();
        }

        // Dbscan clustering identifiers
        public enum ClusterIds
        {
            UNCLASSIFIED = 0,
            NOISE = -1
        }

        // Point container for Dbscan clustering
        public class DbscanPoint
        {
            public bool IsVisited;
            public DatasetItem ClusterPoint;
            public int ClusterId;

            public DbscanPoint(DatasetItem x)
            {
                ClusterPoint = x;
                IsVisited = false;
                ClusterId = (int)ClusterIds.UNCLASSIFIED;
            }
        }

        // Each items in your dataset, preferences, features, points, etc.
        public class DatasetItem
        {
            public double X;
            public double Y;

            public DatasetItem(double x, double y)
            {
                X = x;
                Y = y;
            }
        }
    }
}
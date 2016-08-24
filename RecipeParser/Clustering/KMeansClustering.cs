using RecipeParser.RecipeJsonModel;
using System;
using System.Collections.Generic;

namespace RecipeParser
{
    public class KMeansClustering
    {

        private const float DISTANCE_COEFFICIENT = 0.125f; // 1/8 - we consider that we have maximum 8 columns
        private int distanceCriteria;

        public KMeansClustering(int width)
        {
            distanceCriteria = (int)(width * DISTANCE_COEFFICIENT);
        }

        /// <summary>
        /// Takes positions of each line and clusters them
        /// </summary>
        /// <param name="lines">Lines of text with bounds</param>
        /// <returns>Array of cluster numbers for lines of text</returns>
        public int[] Process(List<TextLine> lines)
        {
            const int restarts = 5, disttype = 2;

            // preparing an array of left margins for clustering
            double[,] xy = new double[lines.Count, 1];
            lines.ForEach(item => { xy[lines.IndexOf(item), 0] = item.Bounds.Left; });

            // processing one-dimensional clustering, it considers only x position of the text
            alglib.clusterizerstate s;
            alglib.clusterizercreate(out s);
            alglib.clusterizersetpoints(s, xy, disttype);
            alglib.clusterizersetkmeanslimits(s, restarts, 0);

            alglib.kmeansreport rep;
            int k = 2;
            do
            {
                alglib.clusterizerrunkmeans(s, k, out rep);
                k++;
            } while (IsCriteriaSatisfied(rep.c));

            if (k == 3)
            {
                alglib.clusterizerrunkmeans(s, 1, out rep);
                return PopulateArray(new int[rep.cidx.Length], (int) Math.Round(rep.c[0, 0]));
            }

            alglib.clusterizerrunkmeans(s, k - 2, out rep);
            
            int[] horisontalClusteringOut = GetClusterCenters(rep.cidx, rep.c);

            return horisontalClusteringOut;
        }

        /// <summary>
        /// Checks distance between centers
        /// </summary>
        /// <param name="rep">Centers of K-Means clusterting algorithm</param>
        /// <returns>True if the centers are far enough from each other, false otherwise</returns>
        private bool IsCriteriaSatisfied(double[,] centers)
        {
            double[] centersOneDim = ExtractVerticalArrayDimension(centers, 0);
            Array.Sort(centersOneDim);
            for (int i = 1; i < centersOneDim.Length; i++)
            {
                if (centersOneDim[i] - centersOneDim[i - 1] < distanceCriteria)
                    return false;
            }
            return true;
        }

        private int[] GetClusterCenters(int[] cidx, double[,] c)
        {
            int[] result = new int[cidx.Length];
            for (int i = 0; i < cidx.Length; i++)
            {
                result[i] = (int) Math.Round(c[cidx[i], 0]);
            }
            return result;
        }

        private T[] ExtractVerticalArrayDimension<T>(T[,] array, int dimension)
        {
            T[] newArray = new T[array.GetLength(dimension)];
            for (int i = 0; i < array.GetLength(dimension); i++)
            {
                newArray[i] = array[i, dimension];
            }
            return newArray;
        }

        /// <summary>
        /// Pupulates an array with specified value
        /// </summary>
        /// <typeparam name="T">Type of the array</typeparam>
        /// <param name="arr">Array to fill</param>
        /// <param name="value">Value to put</param>
        private T[] PopulateArray<T>(T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
            return arr;
        }
    }
}
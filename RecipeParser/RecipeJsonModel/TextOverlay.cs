﻿using System;
using System.Collections.Generic;

namespace RecipeParser.RecipeJsonModel
{
    public class TextOverlay
    {
        public List<TextLine> Lines { get; set; }
        public bool HasOverlay { get; set; }
        public string Message { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public void ComputeExtraFields()
        {
            int width = 0, height = 0;
            foreach (TextLine line in Lines)
            {
                line.ComputeExtraFields();

                int newWidth = line.Bounds.Left + line.Bounds.Width;
                int newHeight = line.Bounds.Top + line.Bounds.Height;

                if (newWidth > width)
                    width = newWidth;
                if (newHeight > height)
                    height = newHeight;

                Width = width;
                Height = height;
            }
        }

        public void Normalize()
        {
            // Processing horizontal clustering
            int[] clusters = new KMeansClustering(Width).Process(Lines);
            if (clusters.Length != Lines.Count) throw new Exception("Error during clustering, use debugging for more info.");
            for (int i = 0; i < Lines.Count; i++)
            {
                Lines[i].Bounds.Left = clusters[i];
            }
            
            int startLine = 0;
            double distanceCoefficient = 0.8;
            int allDistances = 0;
            int allHeights = 0;
            int allLeftPositions = 0;
            int maxWidth = 0;

            for (int i = 1; i < Lines.Count; i++)
            {
                TextLine line1 = Lines[i - 1];
                TextLine line2 = Lines[i];
                int line1BottomPoint = line1.Bounds.Top + line1.Bounds.Height;
                int line2TopPoint = line2.Bounds.Top;
                // new block of text found
                if ((line2TopPoint - line1BottomPoint) > line1.Bounds.Height * distanceCoefficient ||
                    (line2TopPoint - line1BottomPoint) < 0 ||
                    i == Lines.Count - 1)
                {
                    if (i - startLine > 2)
                    {
                        double averageDistancePrecise = allDistances / (i - startLine - 1);
                        int averageDistance = (int) Math.Round(averageDistancePrecise);

                        allHeights += Lines[startLine].Bounds.Height;
                        double averageHeightPrecise = allHeights / (i - startLine);
                        int averageHeight = (int) Math.Round(averageHeightPrecise);
                        Lines[startLine].Bounds.Height = averageHeight;
                        /*
                        if (line2.Bounds.Width > maxWidth)
                            maxWidth = line2.Bounds.Width;
                        Lines[startLine].Bounds.Width = maxWidth;

                        allLeftPositions += Lines[startLine].Bounds.Left;

                        double averageLeftPositionPrecise = allLeftPositions / (i - startLine);
                        int averageLeftPosition = (int)Math.Round(averageLeftPositionPrecise);
                        Lines[startLine].Bounds.Left = averageLeftPosition;
                        */
                        for (int j = startLine + 1; j < i; j++)
                        {
                            Lines[j].Bounds.Top = Lines[j - 1].Bounds.Top + Lines[j - 1].Bounds.Height + averageDistance;
                            Lines[j].Bounds.Height = averageHeight;
                            //Lines[j].Bounds.Width = maxWidth;
                            //Lines[j].Bounds.Left = averageLeftPosition;
                        }
                    }
                    startLine = i;
                    allDistances = 0;
                    allHeights = 0;
                    maxWidth = 0;
                    continue;
                }
                allDistances += (line2TopPoint - line1BottomPoint);
                allHeights += line2.Bounds.Height;
                if (line2.Bounds.Width > maxWidth)
                    maxWidth = line2.Bounds.Width;
                allLeftPositions += line2.Bounds.Left;
            }
        }
    }
}
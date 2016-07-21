using System;
using System.Collections.Generic;

namespace RecipeParser.RecipeJsonModel
{
    public class TextOverlay
    {
        public List<TextLine> Lines { get; set; }
        public bool HasOverlay { get; set; }
        public string Message { get; set; }
        public int Width { get; set; } // Extra field
        public int Height { get; set; } // Extra field

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

            // Processing vertical clustering
            FixDistancesAndHeight();

            FixTextLineOrder(clusters);
        }

        private void FixDistancesAndHeight()
        {
            int startLine = 0;
            int allDistances = 0;
            int allHeights = 0;

            for (int i = 1; i < Lines.Count; i++)
            {
                TextLine line1 = Lines[i - 1];
                TextLine line2 = Lines[i];

                // new block of text found
                if (isNewBlockOfText(line1, line2) || i == Lines.Count - 1)
                {
                    if (i - startLine > 2)
                    {
                        allHeights += Lines[startLine].Bounds.Height;

                        double averageDistanceDouble = allDistances / (i - startLine - 1);
                        double averageHeightDouble = allHeights / (i - startLine);
                        int averageDistance = (int)Math.Round(averageDistanceDouble);
                        int averageHeight = (int)Math.Round(averageHeightDouble);
                        
                        Lines[startLine].Bounds.Height = averageHeight;

                        for (int j = startLine + 1; j < i; j++)
                        {
                            Lines[j].Bounds.Top = Lines[j - 1].Bounds.Top + Lines[j - 1].Bounds.Height + averageDistance;
                            Lines[j].Bounds.Height = averageHeight;
                        }
                    }
                    startLine = i;
                    allDistances = 0;
                    allHeights = 0;
                    continue;
                }

                allDistances += line2.Bounds.Top - (line1.Bounds.Top + line1.Bounds.Height);
                allHeights += line2.Bounds.Height;
            }
        }

        private bool isNewBlockOfText(TextLine line1, TextLine line2)
        {
            int line1BottomPoint = line1.Bounds.Top + line1.Bounds.Height;
            int line2TopPoint = line2.Bounds.Top;
            double distanceCoefficient = 1.0;

            // another line is too far, consider a new block of text
            if ((line2TopPoint - line1BottomPoint) > line1.Bounds.Height * distanceCoefficient)
                return true;

            // in case if one line overlaps another one a little
            if ((line2TopPoint - line1BottomPoint) < (-0.5 * line2.Bounds.Height))
                return true;

            return false;
        }

        private void FixTextLineOrder(int[] clusters)
        {
            List<List<TextLine>> verticalBlocks = new List<List<TextLine>>();
            for (int i = 0; i < Lines.Count; i++)
            {
                bool found = false;
                foreach (List<TextLine> linesList in verticalBlocks)
                {
                    if (linesList[0].Bounds.Left == Lines[i].Bounds.Left)
                    {
                        linesList.Add(Lines[i]);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    List<TextLine> textLines = new List<TextLine>();
                    textLines.Add(Lines[i]);
                    verticalBlocks.Add(textLines);
                }
            }

            // Sorting horizontally and vertically
            List<TextLine> newLines = new List<TextLine>();

            verticalBlocks.Sort((line1, line2) =>
                line1[0].Bounds.Left > line2[0].Bounds.Left ? 1 :
                line1[0].Bounds.Left < line2[0].Bounds.Left ? -1 : 0);
            foreach (List<TextLine> linesList in verticalBlocks)
            {
                linesList.Sort((line1, line2) =>
                    line1.Bounds.Top > line2.Bounds.Top ? 1 :
                    line1.Bounds.Top < line2.Bounds.Top ? -1 : 0);
                newLines.AddRange(linesList);
            }

            Lines = newLines;

            bool verticalOrderFixed = false;
            while (!verticalOrderFixed)
            {
                verticalOrderFixed = true;
                for (int i = 0; i < Lines.Count; i++)
                {
                    for (int j = i + 1; j < Lines.Count; j++)
                    {
                        if (Lines[j].Bounds.Top < Lines[i].Bounds.Top &&
                            Lines[j].Bounds.Left < Lines[i].Bounds.Left + Lines[i].Bounds.Width)
                        {
                            TextLine tempLine = Lines[j];
                            Lines.Remove(tempLine);
                            Lines.Insert(i, tempLine);
                            verticalOrderFixed = false;
                        }
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace RecipeParser.RecipeJsonModel
{
    public class TextLine
    {
        public List<TextWord> Words { get; set; }
        public int MaxHeight { get; set; }
        public int MinTop { get; set; }
        public string Text { get; set; } // Extra field
        public BlockBounds Bounds { get; set; } // Extra field
        public bool Bold { get; set; } // Extra field

        public void ComputeExtraFields()
        {
            StringBuilder output = new StringBuilder(String.Empty);
            BlockBounds blockBounds = new BlockBounds();

            int left = int.MaxValue, width = 0;

            foreach (TextWord word in Words)
            {
                // composing text line from words
                output.Append(word.WordText);
                if (Words.IndexOf(word) < Words.Count - 1)
                {
                    output.Append(" ");
                }

                // computing boundaries
                width += word.Width;
                if (word.Left < left)
                    left = word.Left;
            }

            blockBounds.Left = left;
            blockBounds.Top = MinTop;
            blockBounds.Width = width;
            blockBounds.Height = MaxHeight;

            Text = output.ToString();
            Bounds = blockBounds;
        }
    }
}
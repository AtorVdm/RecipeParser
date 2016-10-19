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

            int boldWords = 0;
            for (int i = 0; i < Words.Count; i++)
            {
                TextWord word = Words[i];
                // composing text line from words
                if (word.Bold)
                {
                    if (i > 0 && Words[i - 1].Bold)
                    {
                        output.Remove(output.Length - 8, 7);
                    }
                    else
                    {
                        output.Append("<span data-ml=\"bold\">");
                    }
                }
                output.Append(word.WordText);
                if (word.Bold) output.Append("</span>");
                if (i < Words.Count - 1)
                {
                    output.Append(" ");
                }

                // computing boundaries
                width += word.Width;
                if (word.Left < left)
                    left = word.Left;

                if (word.Bold) boldWords++;
            }
            if (boldWords/5 == Words.Count/5)
            {
                output.Replace("<span data-ml=\"bold\">", String.Empty);
                output.Replace("</span>", String.Empty);
                Bold = true;
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
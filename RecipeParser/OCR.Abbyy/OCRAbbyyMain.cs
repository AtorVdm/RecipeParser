using RecipeParser.RecipeJsonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace RecipeParser.OCR.Abbyy
{
    public class OCRAbbyyMain
    {
        public Recipe ProcessAbbyyXML(XElement xdoc, string path)
        {
            XNamespace ad = "http://www.abbyy.com/FineReader_xml/FineReader10-schema-v1.xml";
            StringBuilder result = new StringBuilder();
            var page = xdoc.Descendants(ad + "page");
            int width = Int32.Parse(page.Attributes("width").First().Value);
            int height = Int32.Parse(page.Attributes("height").First().Value);
            result.Append(String.Format("<html>\n<body data-ml=\"0 0 {0} {1} block\">\n", width, height));
            Recipe recipe = new Recipe();
            recipe.OCRExitCode = 1;
            recipe.ParsedResults = new List<ParsedResult>();
            recipe.ParsedResults.Add(new ParsedResult());
            recipe.ParsedResults[0].TextOverlay = new TextOverlay();
            TextOverlay textOverlay = recipe.ParsedResults[0].TextOverlay;
            textOverlay.Width = width;
            textOverlay.Height = height;
            textOverlay.Lines = new List<TextLine>();
            foreach (XElement line in xdoc.Descendants(ad + "line"))
            {
                TextLine textLine = new TextLine();
                textLine.Words = new List<TextWord>();
                List<string> segmentLine = new List<string>();
                
                TextWord textWord = new TextWord();
                StringBuilder wordText = new StringBuilder();
                int isBold = 0;

                textWord.Left = Int32.Parse(line.Attribute("l").Value);

                int altHeight = 0;

                foreach (XElement character in line.Descendants(ad + "charParams"))
                {
                    if (character.LastNode.NodeType == System.Xml.XmlNodeType.Text)
                    {
                        String chr = ((XText)character.LastNode).Value;
                        if (Int32.Parse(character.Attribute("meanStrokeWidth").Value) > 80)
                        {
                            isBold++;
                        }
                        else
                        {
                            isBold--;
                        }
                        segmentLine.Add(chr);
                        wordText.Append(chr);
                    }
                    else
                    {
                        textLine.Words.Add(SetTextWord(textWord, isBold >= 0, wordText.ToString(), character.Attribute("l").Value));
                        
                        textWord = new TextWord();
                        wordText = new StringBuilder();
                        isBold = 0;

                        textWord.Left = Int32.Parse(character.Attribute("r").Value);
                        segmentLine.Add(" ");
                        int temp = Int32.Parse(character.Attribute("t").Value) - Int32.Parse(character.Attribute("b").Value);
                        altHeight += temp;
                        if (altHeight != temp)
                            altHeight = altHeight / 2;
                    }
                }
                
                textLine.Words.Add(SetTextWord(textWord, isBold >= 0, wordText.ToString(), line.Attribute("r").Value));

                List<string> segment = new List<string>();
                int left = Int32.Parse(line.Attribute("l").Value);
                int top = Int32.Parse(line.Attribute("t").Value);
                int right = Int32.Parse(line.Attribute("r").Value);
                int bottom = Int32.Parse(line.Attribute("b").Value);
                textLine.MaxHeight = bottom - top;
                textLine.MinTop = top;
                segment.Add(String.Format("\t<p data-ml=\"{0} {1} {2} {3} block>\">",
                    left, top, right - left, altHeight));
                segment.AddRange(segmentLine);
                segment.Add("</p>\n");
                string str = segment.Aggregate(new StringBuilder(),
                    (sb, i) => sb.Append(i),
                    sp => sp.ToString()
                );
                result.AppendLine(str);
                textOverlay.Lines.Add(textLine);
            }
            result.Append("</body>\n</html>");
            
            return recipe;
        }

        private TextWord SetTextWord(TextWord textWord, bool bold, string text, string right)
        {
            textWord.Bold = bold;
            textWord.WordText = text;
            textWord.Width = Int32.Parse(right) - textWord.Left;
            return textWord;
        }
    }
}
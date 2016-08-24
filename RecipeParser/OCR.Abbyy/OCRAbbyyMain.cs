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
        public string ProcessAbbyyXML(XElement xdoc, string path)
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
                int serifProbability = 0;
                TextWord textWord = new TextWord();
                StringBuilder wordText = new StringBuilder();
                textWord.Left = Int32.Parse(line.Attribute("l").Value);
                foreach (XElement character in line.Descendants(ad + "charParams"))
                {
                    if (character.LastNode.NodeType == System.Xml.XmlNodeType.Text)
                    {
                        String chr = ((XText)character.LastNode).Value;
                        if (Int32.Parse(character.Attribute("serifProbability").Value) > 80)
                        {
                            serifProbability++;
                        }
                        else
                        {
                            serifProbability--;
                        }
                        segmentLine.Add(chr);
                        wordText.Append(chr);
                    }
                    else
                    {
                        textWord.WordText = wordText.ToString();
                        textWord.Width = Int32.Parse(character.Attribute("l").Value) - textWord.Left;
                        textLine.Words.Add(textWord);
                        wordText = new StringBuilder();
                        textWord = new TextWord();
                        textWord.Left = Int32.Parse(character.Attribute("r").Value);
                        segmentLine.Add(" ");
                    }
                }
                textWord.WordText = wordText.ToString();
                textWord.Width = Int32.Parse(line.Attribute("r").Value) - textWord.Left;
                textLine.Words.Add(textWord);
                List<string> segment = new List<string>();
                int left = Int32.Parse(line.Attribute("l").Value);
                int top = Int32.Parse(line.Attribute("t").Value);
                int right = Int32.Parse(line.Attribute("r").Value);
                int bottom = Int32.Parse(line.Attribute("b").Value);
                textLine.MaxHeight = bottom - top;
                textLine.MinTop = top;
                textLine.Bold = serifProbability >= 0;
                segment.Add(String.Format("\t<p data-ml=\"{0} {1} {2} {3}{4}block>\">",
                    left, top, right - left, bottom - top, serifProbability >= 0 ? "bold " : String.Empty));
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

            string htmlOutput = RecipeHTMLConverter.ConvertRecipeToHTML(recipe);

            //File.WriteAllText(path + ".html", htmlOutput, swedishEncoding);
            //GenerateAreasPicture(recipe.ParsedResults[0].TextOverlay);
            return result.ToString();
        }
    }
}
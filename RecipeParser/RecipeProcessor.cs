using RecipeParser.RecipeJsonModel;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace RecipeParser
{
    public class RecipeProcessor
    {
        private Encoding swedishEncoding = Encoding.GetEncoding(1252);
        
        private OCR.Space.OCRSpaceMain ocrSpace = new OCR.Space.OCRSpaceMain();
        private OCR.Abbyy.OCRAbbyyMain ocrAbbyy = new OCR.Abbyy.OCRAbbyyMain();

        private string fileName;

        public string ProcessAbbyyXML(XElement xdoc, string path)
        {
            return ocrAbbyy.ProcessAbbyyXML(xdoc, path);
        }

        public void processUploadedFile(HttpPostedFile file)
        {
            fileName = String.Format(@"C:\test\{0}_{1}", file.FileName, DateTime.Now.ToString("MM-dd-hh-mm-ss"));

            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes(file.ContentLength);
            }

            string jsonObject = ocrSpace.processPicture(fileData, file.FileName);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Recipe recipe = serializer.Deserialize<Recipe>(jsonObject);

            if (recipe.OCRExitCode != 1) return;

            TextOverlay overlay = recipe.ParsedResults[0].TextOverlay;
            overlay.ComputeExtraFields();
            
            //string[] lines = recipe.ParsedResults[0].ParsedText.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            string htmlOutput = RecipeHTMLConverter.ConvertRecipeToHTML(recipe);

            File.WriteAllText(fileName + ".html", htmlOutput, swedishEncoding);

            GenerateAreasPicture(recipe.ParsedResults[0].TextOverlay);
        }

        private void GenerateAreasPicture(TextOverlay overlay)
        {
            Bitmap flag = new Bitmap(overlay.Width, overlay.Height);
            Graphics flagGraphics = Graphics.FromImage(flag);
            foreach (TextLine line in overlay.Lines)
            {
                flagGraphics.FillRectangle(Brushes.Black, line.Bounds.Left, line.Bounds.Top, line.Bounds.Width, line.Bounds.Height);
                flagGraphics.DrawString("Line " + overlay.Lines.IndexOf(line),
                    new Font(FontFamily.GenericSerif, 16),
                    Brushes.OrangeRed,
                    new PointF(line.Bounds.Left, line.Bounds.Top));
            }

            flag.Save(fileName + ".png");
        }
    }
}
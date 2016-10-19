using System;
using System.Text;
using System.Web.UI;
using System.Xml.Linq;
using System.Collections.Generic;
using RecipeParser.RecipeJsonModel;
using System.IO;

namespace RecipeParser
{
    public partial class _Default : Page
    {
        RecipeProcessor processor = new RecipeProcessor();
        private Encoding swedishEncoding = Encoding.GetEncoding(1252);

        protected void Page_Load(object sender, EventArgs e)
        {

        }
        
        protected void btnSubmitClick(object sender, EventArgs e)
        {
            SendAbbyyResponse();
            if (processor!=null) return;

            if (!uploadedFile.HasFiles)
            {
                errorLabel.Text = "File wasn't set!";
                return;
            }
            /*
            Bitmap bitmap;
            using (var ms = new MemoryStream(uploadedFile.FileBytes))
            {
                bitmap = new Bitmap(ms);
            }*/

            //outputTextBox.Text = processPicture(uploadedFile.FileBytes, uploadedFile.FileName);

            //string jsonObject = File.ReadAllText(@"C:\test\jsonTest.txt", swedishEncoding);
            foreach (var file in uploadedFile.PostedFiles)
            {
                processor.ProcessUploadedFile(file);
            }

            Response.Clear();
            Response.ClearHeaders();
            Response.AddHeader("Content-Type", "text/plain");
            Response.Write("Parsing done!");
            Response.Flush();
            Response.End();
        }

        private void SendAbbyyResponse()
        {
            /*
            JObject json = JObject.Parse(jsonObject);
            int time = (int)json["ProcessingTimeInMilliseconds"];
            int exitCode = (int)json["OCRExitCode"];
            bool hasError = (bool)json["IsErroredOnProcessing"];
            string errorMessage = (string)json["ErrorMessage"];*/

            List<string> pathes = new List<string>();
            pathes.Add(Server.MapPath("~/input/IMG_0030"));
            pathes.Add(Server.MapPath("~/input/IMG_0031"));
            pathes.Add(Server.MapPath("~/input/IMG_0032"));
            pathes.Add(Server.MapPath("~/input/IMG_20160411_120428"));
            pathes.Add(Server.MapPath("~/input/IMG_20160411_120615"));
            pathes.Add(Server.MapPath("~/input/IMG_20160411_120649"));

            string htmlOutput = String.Empty;
            Recipe recipe = null;
            foreach (string path in pathes)
            {
                XElement xdoc = XElement.Load(path + ".xml");
                recipe = processor.ProcessAbbyyXML(xdoc, path);
                htmlOutput = RecipeHTMLConverter.ConvertRecipeToHTML(recipe);
                processor.GenerateAreasPicture(recipe.ParsedResults[0].TextOverlay);
                File.WriteAllText(path + ".html", htmlOutput, swedishEncoding);
            }
            
            Response.Clear();
            Response.ClearHeaders();
            Response.AddHeader("Content-Type", "text/html; charset=" + swedishEncoding.EncodingName);
            Response.Write(htmlOutput);
            Response.Flush();
            Response.End();
        }
    }
}
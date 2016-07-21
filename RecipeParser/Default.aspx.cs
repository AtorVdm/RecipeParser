using RecipeParser.RecipeJsonModel;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web;

namespace RecipeParser
{
    public partial class _Default : Page
    {
        private const string OCR_API_URI_STRING = "https://api.ocr.space/parse/image";
        private Encoding swedishEncoding = Encoding.GetEncoding(1252);
        private string fileName;

        protected void Page_Load(object sender, EventArgs e)
        {
            /*
            JObject json = JObject.Parse(jsonObject);
            int time = (int)json["ProcessingTimeInMilliseconds"];
            int exitCode = (int)json["OCRExitCode"];
            bool hasError = (bool)json["IsErroredOnProcessing"];
            string errorMessage = (string)json["ErrorMessage"];*/
            //btnSubmitClick(null, null);
        }

        protected void btnSubmitClick(object sender, EventArgs e)
        {   
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
                processUploadedFile(file);
            }

            Response.Clear();
            Response.ClearHeaders();
            Response.AddHeader("Content-Type", "text/plain");
            Response.Write("Parsing done!");
            Response.Flush();
            Response.End();
        }

        private void processUploadedFile(HttpPostedFile file)
        {
            fileName = String.Format(@"C:\test\{0}_{1}", file.FileName, DateTime.Now.ToString("MM-dd-hh-mm-ss"));

            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes(file.ContentLength);
            }

            string jsonObject = processPicture(fileData, file.FileName);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Recipe recipe = serializer.Deserialize<Recipe>(jsonObject);

            if (recipe.OCRExitCode != 1) return;

            TextOverlay overlay = recipe.ParsedResults[0].TextOverlay;
            overlay.ComputeExtraFields();

            //GenerateAreasPicture(recipe.ParsedResults[0].TextOverlay);

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

        private string processPicture(byte[] pictureBytes, string name)
        {
            using (var client = new WebClient())
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent("c7e81b2fe088957"), "apikey");
                formData.Add(new StringContent("swe"), "language");
                formData.Add(new StringContent("true"), "isOverlayRequired");
                formData.Add(new ByteArrayContent(pictureBytes), "file", name);

                return SendMultipartFormData(formData).Result;
            }
        }

        private async Task<string> SendMultipartFormData(MultipartFormDataContent formData)
        {
            var httpClient = new HttpClient();
            var response = httpClient.PostAsync(new Uri(OCR_API_URI_STRING), formData).Result;
            return await response.Content.ReadAsStringAsync();
        }

        /*
        private string processPicture(Bitmap bitmap, string name)
        {
            using (var client = new WebClient())
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent("c7e81b2fe088957"), "apikey");
                formData.Add(new StringContent("swe"), "language");
                formData.Add(new StringContent("true"), "isOverlayRequired");
                formData.Add(new ByteArrayContent(ImageToByte(bitmap)), "file", name);

                return UploadRecipePicture(formData).Result;
            }
        }
        
        private byte[] ImageToByte(Bitmap img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                using (Bitmap myImage = new Bitmap(img))
                {
                    img.Dispose();

                    myImage.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    myImage.Dispose();

                    byteArray = stream.ToArray();
                }
            }
            return byteArray;
        }*/
    }
}
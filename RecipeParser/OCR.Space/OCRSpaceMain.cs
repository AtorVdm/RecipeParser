using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RecipeParser.OCR.Space
{
    public class OCRSpaceMain
    {
        private const string OCR_API_URI_STRING = "https://api.ocr.space/parse/image";

        public string ProcessPicture(byte[] pictureBytes, string name)
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
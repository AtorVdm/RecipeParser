using RecipeParser.RecipeJsonModel;
using RecipeParser.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace RecipeParser.Controllers
{
    public class RecipeController : ApiController
    {
        private const string MATLISTAN_API_URI_STRING = "http://api.test.matlistan.se/Recipes/Parse";

        [Route("api/recipe/{name}")]
        [HttpGet]
        public String Test(String name)
        {
            return "Hello " + (name == null? "World": name);
        }

        [Route("api/recipe/raw")]
        [HttpPost]
        public Recipe ParseImage()
        {
            byte[] image = Request.Content.ReadAsByteArrayAsync().Result;
            if (image == null)
                image = File.ReadAllBytes(@"C:\test\new\IMG_0030.jpg");
            OCR.Space.OCRSpaceMain ocrSpace = new OCR.Space.OCRSpaceMain();
            string jsonObject = ocrSpace.ProcessPicture(image, "TestImageName.jpg");

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Recipe recipe = serializer.Deserialize<Recipe>(jsonObject);

            if (recipe.OCRExitCode != 1) return null;

            TextOverlay overlay = recipe.ParsedResults[0].TextOverlay;
            overlay.ComputeExtraFields();

            return recipe;
        }

        [Route("api/recipe/ocr")]
        [HttpPost]
        public async Task<Recipe> OcrRecipe()
        {
            var multipartRequest = await Request.Content.ReadAsMultipartAsync();
            byte[] image = await multipartRequest.Contents[0].ReadAsByteArrayAsync();

            if (image == null)
                image = File.ReadAllBytes(@"C:\test\new\IMG_0030.jpg");
            OCR.Space.OCRSpaceMain ocrSpace = new OCR.Space.OCRSpaceMain();
            string jsonObject = ocrSpace.ProcessPicture(image, "TestImageName.jpg");
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Recipe recipe = serializer.Deserialize<Recipe>(jsonObject);
            return recipe;
        }

        [Route("api/recipe/parse")]
        [HttpPost]
        public async Task<string> ProcessRecipe()
        {
	        string jsonObject = await Request.Content.ReadAsStringAsync();
			//byte[] image = await Request.Content.ReadAsByteArrayAsync();

			//if (image == null)
			//	image = File.ReadAllBytes(@"C:\test\new\IMG_0030.jpg");
			//OCR.Space.OCRSpaceMain ocrSpace = new OCR.Space.OCRSpaceMain();
			//string jsonObject = ocrSpace.ProcessPicture(image, "TestImageName.jpg");

			JavaScriptSerializer serializer = new JavaScriptSerializer();
			Recipe recipe = serializer.Deserialize<Recipe>(jsonObject);

			if (recipe.OCRExitCode != 1) return null;

            TextOverlay overlay = recipe.ParsedResults[0].TextOverlay;
            overlay.ComputeExtraFields();

            string htmlOutput = RecipeHTMLConverter.ConvertRecipeToHTML(recipe);

            string jsonOutput = ProcessRecipe(Encoding.ASCII.GetBytes(htmlOutput));

            return jsonOutput;
        }

        [Route("api/recipe/test")]
        [HttpPost]
        public string Test()
        {
            //var result = await Request.Content.ReadAsMultipartAsync();
            string test = "\"Hello World!\"";
            return test;
        }

        public string ProcessRecipe(byte[] htmlPage)
        {
            using (var client = new WebClient())
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new ByteArrayContent(htmlPage), "data", "recipe.html");
                return SendMultipartFormData(formData).Result;
            }
        }

        private async Task<string> SendMultipartFormData(MultipartFormDataContent formData)
        {
            var httpClient = new HttpClient();
            var response = httpClient.PostAsync(new Uri(MATLISTAN_API_URI_STRING), formData).Result;
            return await response.Content.ReadAsStringAsync();
        }
    }
}

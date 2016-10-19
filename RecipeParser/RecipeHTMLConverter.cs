using RecipeParser.RecipeJsonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeParser
{
    class RecipeHTMLConverter
    {
        public static string ConvertRecipeToHTML(string[] lines)
        {
            StringBuilder output = new StringBuilder(String.Empty);
            output.AppendLine("<html>");
            output.AppendLine("<body>");
            foreach (string line in lines)
            {
                output.AppendLine(String.Format("\t<p>{0}</p>", line));
            }
            output.AppendLine("</body>");
            output.AppendLine("</html>");
            return output.ToString();
        }

        public static string ConvertRecipeToHTML(Recipe recipe)
        {
            TextOverlay overlay = recipe.ParsedResults[0].TextOverlay;
            overlay.ComputeExtraFields();
            overlay.Normalize();
            StringBuilder output = new StringBuilder(String.Empty);
            output.AppendLine("<html>");
            output.AppendLine(String.Format("<body data-ml=\"{0} {1} {2} {3} block\">", 0, 0, overlay.Width, overlay.Height));
            foreach (TextLine line in overlay.Lines)
            {
                output.AppendLine(String.Format("<p data-ml=\"{0} {1} {2} {3} {4}block\">{5}</p>",
                    line.Bounds.Left, line.Bounds.Top, line.Bounds.Width, line.Bounds.Height, line.Bold? "bold ": String.Empty, line.Text));
            }
            output.AppendLine("</body>");
            output.AppendLine("</html>");
            return output.ToString();
        }
    }
}

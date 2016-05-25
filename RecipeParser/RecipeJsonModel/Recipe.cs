using System.Collections.Generic;

namespace RecipeParser.RecipeJsonModel
{
    public class Recipe
    {
        public List<ParsedResult> ParsedResults { get; set; }
        public int OCRExitCode { get; set; }
        public bool IsErroredOnProcessing { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetails { get; set; }
        public int ProcessingTimeInMilliseconds { get; set; }
    }
}
namespace RecipeParser.RecipeJsonModel
{
    public class TextWord
    {
        public string WordText { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public bool Bold { get; set; } // Extra field
    }
}
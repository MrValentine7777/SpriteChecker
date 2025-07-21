using System.Text.Json.Serialization;

namespace SpriteChecker.Models
{
    public class SpriteInfo
    {
        public string Name { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? Description { get; set; }
        public string? Tag { get; set; }
    }

    public class SpriteAtlas
    {
        public string ImagePath { get; set; } = string.Empty;
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public int BitDepth { get; set; }
        public string Format { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public List<SpriteInfo> Sprites { get; set; } = new List<SpriteInfo>();
    }

    public class ImageMetadata
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int BitDepth { get; set; }
        public double DpiX { get; set; }
        public double DpiY { get; set; }
        public string Format { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public bool HasTransparency { get; set; }
        public string ColorProfile { get; set; } = string.Empty;
    }

    public class SelectionRectangle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsVisible { get; set; }
    }
}
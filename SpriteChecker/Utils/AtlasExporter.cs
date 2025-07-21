using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using SpriteChecker.Models;

namespace SpriteChecker.Utils
{
    public static class AtlasExporter
    {
        public static void ExportToJson(SpriteAtlas atlas, string filePath)
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(atlas, options);
            File.WriteAllText(filePath, json);
        }

        public static void ExportToXml(SpriteAtlas atlas, string filePath)
        {
            var root = new XElement("SpriteAtlas",
                new XAttribute("imagePath", atlas.ImagePath),
                new XAttribute("imageWidth", atlas.ImageWidth),
                new XAttribute("imageHeight", atlas.ImageHeight),
                new XAttribute("bitDepth", atlas.BitDepth),
                new XAttribute("format", atlas.Format),
                new XAttribute("createdDate", atlas.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")),
                
                new XElement("Sprites",
                    atlas.Sprites.Select(sprite =>
                        new XElement("Sprite",
                            new XAttribute("name", sprite.Name),
                            new XAttribute("x", sprite.X),
                            new XAttribute("y", sprite.Y),
                            new XAttribute("width", sprite.Width),
                            new XAttribute("height", sprite.Height),
                            sprite.Tag != null ? new XAttribute("tag", sprite.Tag) : null,
                            sprite.Description != null ? new XAttribute("description", sprite.Description) : null
                        )
                    )
                )
            );

            root.Save(filePath);
        }

        public static void ExportToCss(SpriteAtlas atlas, string filePath, string imageName)
        {
            var css = new StringBuilder();
            css.AppendLine($"/* Sprite Atlas CSS - Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss} */");
            css.AppendLine($"/* Image: {imageName} ({atlas.ImageWidth}x{atlas.ImageHeight}) */");
            css.AppendLine();
            
            css.AppendLine(".sprite {");
            css.AppendLine($"    background-image: url('{imageName}');");
            css.AppendLine("    background-repeat: no-repeat;");
            css.AppendLine("    display: inline-block;");
            css.AppendLine("}");
            css.AppendLine();

            foreach (var sprite in atlas.Sprites)
            {
                var className = SanitizeCssClassName(sprite.Name);
                css.AppendLine($".sprite-{className} {{");
                css.AppendLine($"    background-position: -{sprite.X}px -{sprite.Y}px;");
                css.AppendLine($"    width: {sprite.Width}px;");
                css.AppendLine($"    height: {sprite.Height}px;");
                if (!string.IsNullOrEmpty(sprite.Tag))
                {
                    css.AppendLine($"    /* Tag: {sprite.Tag} */");
                }
                css.AppendLine("}");
                css.AppendLine();
            }

            File.WriteAllText(filePath, css.ToString());
        }

        public static void ExportToUnityScript(SpriteAtlas atlas, string filePath, string className = "SpriteAtlas")
        {
            var cs = new StringBuilder();
            cs.AppendLine("using UnityEngine;");
            cs.AppendLine("using System.Collections.Generic;");
            cs.AppendLine();
            cs.AppendLine($"// Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            cs.AppendLine($"// Image: {Path.GetFileName(atlas.ImagePath)} ({atlas.ImageWidth}x{atlas.ImageHeight})");
            cs.AppendLine();
            cs.AppendLine("[System.Serializable]");
            cs.AppendLine("public class SpriteData");
            cs.AppendLine("{");
            cs.AppendLine("    public string name;");
            cs.AppendLine("    public Rect rect;");
            cs.AppendLine("    public string tag;");
            cs.AppendLine("}");
            cs.AppendLine();
            cs.AppendLine($"public class {className} : MonoBehaviour");
            cs.AppendLine("{");
            cs.AppendLine("    public Texture2D atlasTexture;");
            cs.AppendLine("    public List<SpriteData> sprites = new List<SpriteData>();");
            cs.AppendLine();
            cs.AppendLine("    void Start()");
            cs.AppendLine("    {");
            cs.AppendLine("        InitializeSprites();");
            cs.AppendLine("    }");
            cs.AppendLine();
            cs.AppendLine("    void InitializeSprites()");
            cs.AppendLine("    {");
            
            foreach (var sprite in atlas.Sprites)
            {
                var safeName = SanitizeVariableName(sprite.Name);
                cs.AppendLine($"        sprites.Add(new SpriteData {{");
                cs.AppendLine($"            name = \"{sprite.Name}\",");
                cs.AppendLine($"            rect = new Rect({sprite.X}, {sprite.Y}, {sprite.Width}, {sprite.Height}),");
                cs.AppendLine($"            tag = \"{sprite.Tag ?? ""}\"");
                cs.AppendLine($"        }});");
            }
            
            cs.AppendLine("    }");
            cs.AppendLine();
            cs.AppendLine("    public Sprite GetSprite(string spriteName)");
            cs.AppendLine("    {");
            cs.AppendLine("        var spriteData = sprites.Find(s => s.name == spriteName);");
            cs.AppendLine("        if (spriteData != null && atlasTexture != null)");
            cs.AppendLine("        {");
            cs.AppendLine("            return Sprite.Create(atlasTexture, spriteData.rect, Vector2.one * 0.5f);");
            cs.AppendLine("        }");
            cs.AppendLine("        return null;");
            cs.AppendLine("    }");
            cs.AppendLine("}");

            File.WriteAllText(filePath, cs.ToString());
        }

        private static string SanitizeCssClassName(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input.ToLower(), @"[^a-z0-9\-_]", "-");
        }

        private static string SanitizeVariableName(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, @"[^a-zA-Z0-9_]", "_");
        }
    }
}
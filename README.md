# Sprite Checker Tool

A comprehensive WPF application for game developers to analyze sprite sheets and texture atlases.

## Features

### Core Functionality
- **Image Loading**: Supports PNG, JPG, JPEG, BMP, GIF, and TIFF formats
- **Image Information**: Displays dimensions, bit depth, format, DPI, file size, and transparency info
- **Selection Tool**: Click and drag to select sprite regions
- **Sprite Management**: Add, edit, remove, and organize sprites with names and tags

### Quality of Life Features
- **Multiple Export Formats**: 
  - JSON atlas data
  - XML atlas data  
  - CSS sprite sheets
  - Unity C# scripts
- **Individual Sprite Export**: Export selected sprites as separate PNG files
- **Grid Overlay**: Configurable grid system for precise alignment
- **Zoom Controls**: Zoom in/out, fit to window, actual size
- **Visual Feedback**: 
  - Selection highlighting with red dashed border
  - Sprite boundaries with blue outlines
  - Selected sprite highlighting in yellow
  - Real-time mouse position display

### Advanced Features
- **Persistent Data**: Save and load sprite atlas configurations
- **Batch Operations**: Clear all sprites, bulk operations
- **Developer Tools**: 
  - CSS sprite generation with class names
  - Unity script generation with Rect data
  - XML export for game engines

## Usage

1. **Load an Image**: File ? Open Image or use the menu
2. **Select Sprite Areas**: Click and drag on the image to select regions
3. **Add Sprites**: Name your sprite and optionally add tags, then click "Add Sprite"
4. **Manage Sprites**: Use the sprite list to select, edit, or remove sprites
5. **Export Data**: Save your atlas data in various formats for use in your game engine

## Export Formats

### JSON Atlas Data
```json
{
  "imagePath": "spritesheet.png",
  "imageWidth": 512,
  "imageHeight": 512,
  "sprites": [
    {
      "name": "player_idle",
      "x": 0,
      "y": 0,
      "width": 32,
      "height": 32,
      "tag": "player"
    }
  ]
}
```

### CSS Sprite Sheet
```css
.sprite {
    background-image: url('spritesheet.png');
    background-repeat: no-repeat;
    display: inline-block;
}

.sprite-player-idle {
    background-position: -0px -0px;
    width: 32px;
    height: 32px;
}
```

### Unity C# Script
```csharp
public class SpriteAtlas : MonoBehaviour
{
    public Texture2D atlasTexture;
    public List<SpriteData> sprites = new List<SpriteData>();
    
    public Sprite GetSprite(string spriteName) { /* ... */ }
}
```

## Keyboard Shortcuts
- **Ctrl+O**: Open Image
- **Ctrl+S**: Save Atlas Data
- **Delete**: Remove Selected Sprite
- **F**: Zoom to Fit
- **1**: Actual Size
- **+**: Zoom In
- **-**: Zoom Out

## Tips for Game Developers
- Use consistent naming conventions for sprites (e.g., "character_action_frame")
- Leverage tags to organize related sprites (e.g., "ui", "character", "environment")
- Export to CSS for web games, Unity scripts for Unity projects
- Use the grid overlay to ensure pixel-perfect alignment
- Save atlas data frequently to preserve your work

Built with .NET 8 and WPF for Windows desktop environments.
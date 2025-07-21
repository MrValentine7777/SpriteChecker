# Sprite Checker Tool

A comprehensive WPF application for game developers to analyze sprite sheets and texture atlases.

## Features

### Core Functionality
- **Image Loading**: Supports PNG, JPG, JPEG, BMP, GIF, and TIFF formats
- **Image Information**: Displays dimensions, bit depth, format, DPI, file size, and transparency info
- **Selection Tool**: Click and drag to select sprite regions
- **Sprite Management**: Add, edit, remove, and organize sprites with names and tags

### Navigation & Viewing
- **Advanced Zoom Controls**: 
  - **CTRL + Mouse Wheel**: Smooth zooming in and out at mouse cursor position
  - Menu zoom options: Zoom in/out, fit to window, actual size, reset view
  - Zoom range: 10% to 1000% with smooth scaling
- **Pan & Scroll**:
  - **Right-click + Drag**: Pan/scroll around the image by grabbing and moving
  - Standard scrollbars for precise navigation
  - Visual cursor feedback during panning (hand cursor)
- **View Management**: Reset view to return to default zoom and position

### Quality of Life Features
- **Multiple Export Formats**: 
  - JSON atlas data
  - XML atlas data  
  - CSS sprite sheets
  - Unity C# scripts
- **Individual Sprite Export**: Export selected sprites as separate PNG files
- **Grid Overlay**: Configurable grid system for precise alignment
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

### Basic Operations
1. **Load an Image**: File ? Open Image or use the menu
2. **Navigate the Image**: 
   - Hold **CTRL + Mouse Wheel** to zoom in/out at cursor position
   - **Right-click + Drag** to pan around large images
   - Use **View** menu for specific zoom options (Fit, Actual Size, Reset)
3. **Select Sprite Areas**: Left-click and drag on the image to select regions
4. **Add Sprites**: Name your sprite and optionally add tags, then click "Add Sprite"
5. **Manage Sprites**: Use the sprite list to select, edit, or remove sprites
6. **Export Data**: Save your atlas data in various formats for use in your game engine

### Navigation Controls
- **CTRL + Mouse Wheel**: Zoom in/out at mouse position
- **Right-click + Drag**: Pan/scroll the image
- **Left-click + Drag**: Create sprite selections
- **Right-click** (without drag): Clear current selection

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

## Controls Reference

### Mouse Controls
- **Left Click + Drag**: Create sprite selection
- **Right Click + Drag**: Pan/scroll the image
- **Right Click**: Clear selection (when not dragging)
- **CTRL + Mouse Wheel**: Zoom in/out at cursor position

### Menu Shortcuts
- **File ? Open Image**: Load sprite sheet image
- **File ? Save Atlas Data**: Export atlas configuration
- **View ? Zoom In/Out**: Manual zoom controls
- **View ? Zoom to Fit**: Fit entire image in view
- **View ? Actual Size**: Reset to 100% zoom
- **View ? Reset View**: Reset both zoom and pan to defaults

### Keyboard Shortcuts (Future)
- **Ctrl+O**: Open Image
- **Ctrl+S**: Save Atlas Data
- **Delete**: Remove Selected Sprite
- **F**: Zoom to Fit
- **1**: Actual Size
- **0**: Reset View
- **+**: Zoom In
- **-**: Zoom Out

## Tips for Game Developers
- **Navigation**: Use CTRL+Wheel for precise zooming at specific areas, then right-drag to reposition
- **Workflow**: Zoom to fit when starting, then zoom in on sections for detailed sprite selection
- **Organization**: Use consistent naming conventions for sprites (e.g., "character_action_frame")
- **Tagging**: Leverage tags to organize related sprites (e.g., "ui", "character", "environment")
- **Export Formats**: Use CSS for web games, Unity scripts for Unity projects
- **Grid Alignment**: Use the grid overlay to ensure pixel-perfect alignment
- **Data Persistence**: Save atlas data frequently to preserve your work

## Technical Features
- **Smooth Zooming**: Zoom range from 10% to 1000% with proper scaling
- **Smart Panning**: Right-click drag for intuitive image navigation
- **Transform Preservation**: Zoom and pan states maintained during sprite operations
- **Visual Feedback**: Cursor changes and visual indicators for different interaction modes
- **Performance Optimized**: Efficient rendering with WPF transforms

Built with .NET 8 and WPF for Windows desktop environments.
# Sprite Checker Tool

A comprehensive WPF application for game developers to analyze sprite sheets and texture atlases.

## Features

### Core Functionality
- **Image Loading**: Supports PNG, JPG, JPEG, BMP, GIF, and TIFF formats
- **Image Information**: Displays dimensions, bit depth, format, DPI, file size, and transparency info
- **Selection Tool**: Click and drag to select sprite regions OR manually enter precise coordinates
- **Manual Coordinate Editing**: Direct numerical input for pixel-perfect sprite positioning
- **Sprite Management**: Add, edit, remove, and organize sprites with names and tags
- **Ghost Sprites**: Visual reference overlays that remain on the image after sprite creation

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

### Enhanced Selection System
- **Dual Selection Methods**:
  - **Visual Selection**: Traditional left-click and drag on the image
  - **Manual Input**: Direct coordinate entry in text boxes for pixel-perfect accuracy
- **Real-time Updates**: Selection rectangle updates instantly when editing coordinates
- **Coordinate Validation**: Automatic bounds checking to prevent invalid selections
- **Smart Clamping**: Coordinates automatically adjusted to stay within image boundaries

### Ghost Sprite System
- **Visual Reference**: 40% opacity blue overlays remain on image after sprite creation
- **Persistent Display**: Ghost sprites provide context for subsequent sprite placement
- **Zoom Compatibility**: Ghost sprites scale and position correctly with zoom/pan operations
- **Automatic Management**: Cleared when removing all sprites or starting fresh

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
  - Ghost sprite overlays in semi-transparent blue
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
3. **Select Sprite Areas**: 
   - **Visual Method**: Left-click and drag on the image to select regions
   - **Manual Method**: Enter precise X, Y, Width, Height values in the text boxes
4. **Add Sprites**: Name your sprite and optionally add tags, then click "Add Sprite"
5. **Manage Sprites**: Use the sprite list to select, edit, or remove sprites
6. **Export Data**: Save your atlas data in various formats for use in your game engine

### Manual Coordinate Editing
The **Current Selection** panel now features editable text boxes for precise control:

#### Selection Coordinates
- **X**: Horizontal position (0 to image width)
- **Y**: Vertical position (0 to image height)  
- **Width**: Selection width (1 to remaining horizontal space)
- **Height**: Selection height (1 to remaining vertical space)

#### Usage Tips
- **Direct Entry**: Type coordinates directly for pixel-perfect positioning
- **Real-time Preview**: Selection rectangle updates instantly as you type
- **Auto-validation**: Invalid values are automatically corrected to valid ranges
- **Workflow Integration**: Combine with visual selection - drag roughly, then fine-tune with numbers

### Ghost Sprite Reference System
After adding sprites, semi-transparent blue overlays remain visible:

#### Benefits
- **Spatial Context**: See where existing sprites are located while creating new ones
- **Overlap Detection**: Easily identify overlapping or adjacent sprite areas
- **Visual Reference**: Maintain awareness of overall sprite layout
- **Layout Planning**: Plan sprite placement relative to existing sprites

#### Management
- **Automatic Creation**: Ghost sprites appear automatically when adding sprites
- **Persistent Display**: Remain visible during all editing operations
- **Zoom Responsive**: Scale and position correctly at all zoom levels
- **Bulk Clearing**: Removed when clearing all sprites from the atlas

### Navigation Controls
- **CTRL + Mouse Wheel**: Zoom in/out at mouse position
- **Right-click + Drag**: Pan/scroll the image
- **Left-click + Drag**: Create sprite selections (visual method)
- **Right-click** (without drag): Clear current selection
- **Text Box Editing**: Type coordinates for precise manual selection

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
- **Left Click + Drag**: Create sprite selection (visual method)
- **Right Click + Drag**: Pan/scroll the image
- **Right Click**: Clear selection (when not dragging)
- **CTRL + Mouse Wheel**: Zoom in/out at cursor position

### Manual Input Controls
- **Coordinate Text Boxes**: Direct numerical input for X, Y, Width, Height
- **Real-time Updates**: Selection rectangle updates as you type
- **Tab Navigation**: Move between coordinate fields efficiently
- **Enter/Click Away**: Confirm coordinate changes

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

## Workflow Examples

### Precision Sprite Creation
1. **Rough Selection**: Left-click drag to approximately select the sprite area
2. **Fine-tune Coordinates**: Use text boxes to adjust X=100, Y=200, Width=32, Height=32
3. **Verify Visually**: Confirm the red selection rectangle covers the exact pixels needed
4. **Add Sprite**: Click "Add Sprite" to create with ghost overlay for reference

### Grid-based Sprite Atlases
1. **Enable Grid**: Tools ? Grid Settings, set to your sprite size (e.g., 32x32)
2. **Manual Positioning**: Use coordinate text boxes with grid-aligned values (0, 32, 64, 96...)
3. **Systematic Creation**: Create sprites at X=0,Y=0; X=32,Y=0; X=64,Y=0; etc.
4. **Visual Verification**: Ghost sprites help verify grid alignment and spacing

### Complex Sprite Layouts
1. **Visual Overview**: Start with zoom-to-fit to see entire sprite sheet
2. **Rough Placement**: Use left-click drag for approximate selections
3. **Precision Tuning**: Switch to manual coordinate entry for exact boundaries
4. **Reference Checking**: Use ghost sprites to avoid overlaps and maintain spacing

## Tips for Game Developers
- **Dual Method Workflow**: Combine visual selection with manual coordinate entry for best results
- **Navigation**: Use CTRL+Wheel for precise zooming at specific areas, then right-drag to reposition
- **Ghost Reference**: Leverage ghost sprites to maintain spatial awareness while creating complex atlases
- **Precision Work**: Use manual coordinate entry for pixel-perfect alignment and consistent sizing
- **Grid Systems**: Enable grid overlay and use manual coordinates for systematic sprite creation
- **Organization**: Use consistent naming conventions for sprites (e.g., "character_action_frame")
- **Tagging**: Leverage tags to organize related sprites (e.g., "ui", "character", "environment")
- **Export Formats**: Use CSS for web games, Unity scripts for Unity projects
- **Data Persistence**: Save atlas data frequently to preserve your work

## Technical Features
- **Smooth Zooming**: Zoom range from 10% to 1000% with proper scaling
- **Smart Panning**: Right-click drag for intuitive image navigation
- **Manual Precision**: Direct coordinate input with real-time visual feedback
- **Ghost Sprite System**: Persistent visual references with 40% opacity overlays
- **Transform Preservation**: Zoom and pan states maintained during sprite operations
- **Coordinate Validation**: Automatic bounds checking and value clamping
- **Dual Input Methods**: Visual selection and numerical input work seamlessly together
- **Visual Feedback**: Cursor changes and visual indicators for different interaction modes
- **Performance Optimized**: Efficient rendering with WPF transforms

Built with .NET 8 and WPF for Windows desktop environments.
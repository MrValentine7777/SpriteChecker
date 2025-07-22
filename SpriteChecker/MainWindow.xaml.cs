using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using SpriteChecker.Models;
using SpriteChecker.Controls;
using SpriteChecker.Utils;
using ImageMetadata = SpriteChecker.Models.ImageMetadata;
using Path = System.IO.Path;

namespace SpriteChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SpriteAtlas _currentAtlas = new SpriteAtlas();
        private ImageMetadata? _currentImageMetadata;
        private SelectionRectangle _currentSelection = new SelectionRectangle();
        private bool _isSelecting = false;
        private Point _selectionStartPoint;
        private Rectangle? _selectionRectangle;
        private string? _currentImagePath;

        // Grid settings
        private bool _showGrid = false;
        private int _gridSizeX = 32;
        private int _gridSizeY = 32;
        private Color _gridColor = Colors.Gray;
        private double _gridOpacity = 0.5;

        // Zoom and pan settings
        private double _zoomFactor = 1.0;
        private ScaleTransform _scaleTransform = new ScaleTransform();
        private TranslateTransform _translateTransform = new TranslateTransform();
        private TransformGroup _transformGroup = new TransformGroup();
        
        // Pan/scroll settings for right-click drag
        private bool _isPanning = false;
        private Point _panStartPoint;
        private Point _panOriginalOffset;

        // Zoom constraints
        private const double MinZoom = 0.1;
        private const double MaxZoom = 10.0;

        // Manual editing flags
        private bool _updatingTextBoxesProgrammatically = false;
        private List<Rectangle> _ghostSprites = new List<Rectangle>();

        public MainWindow()
        {
            InitializeComponent();
            
            // Set up transform group for zoom and pan
            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_translateTransform);
            ImageContainer.RenderTransform = _transformGroup;
            
            // Enable mouse wheel events on the scroll viewer
            ImageScrollViewer.PreviewMouseWheel += ImageScrollViewer_PreviewMouseWheel;
            
            // Enable right-click dragging for panning
            ImageScrollViewer.MouseRightButtonDown += ImageScrollViewer_MouseRightButtonDown;
            ImageScrollViewer.MouseRightButtonUp += ImageScrollViewer_MouseRightButtonUp;
            ImageScrollViewer.MouseMove += ImageScrollViewer_MouseMove;
            
            // Also add left-click events to ScrollViewer to ensure they reach the image
            ImageScrollViewer.PreviewMouseLeftButtonDown += ImageScrollViewer_PreviewMouseLeftButtonDown;
            ImageScrollViewer.PreviewMouseMove += ImageScrollViewer_PreviewMouseMove;
            ImageScrollViewer.PreviewMouseLeftButtonUp += ImageScrollViewer_PreviewMouseLeftButtonUp;
            
            UpdateUI();
        }

        #region File Operations

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Image File",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadImage(openFileDialog.FileName);
            }
        }

        private void LoadImage(string imagePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                SpriteImage.Source = bitmap;
                _currentImagePath = imagePath;

                // Extract metadata
                _currentImageMetadata = ExtractImageMetadata(bitmap, imagePath);
                
                // Update atlas info
                _currentAtlas = new SpriteAtlas
                {
                    ImagePath = imagePath,
                    ImageWidth = bitmap.PixelWidth,
                    ImageHeight = bitmap.PixelHeight,
                    BitDepth = _currentImageMetadata.BitDepth,
                    Format = _currentImageMetadata.Format
                };

                // Reset zoom and pan when loading new image
                ResetZoomAndPan();
                
                UpdateImageInfo();
                ClearSelection();
                DrawGrid();
                StatusText.Text = $"Loaded: {Path.GetFileName(imagePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ImageMetadata ExtractImageMetadata(BitmapSource bitmap, string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var metadata = new ImageMetadata
            {
                Width = bitmap.PixelWidth,
                Height = bitmap.PixelHeight,
                DpiX = bitmap.DpiX,
                DpiY = bitmap.DpiY,
                Format = bitmap.Format.ToString(),
                FileSizeBytes = fileInfo.Length,
                BitDepth = bitmap.Format.BitsPerPixel,
                HasTransparency = bitmap.Format.ToString().Contains("Alpha") || bitmap.Format.ToString().Contains("Bgra")
            };

            return metadata;
        }

        private void SaveAtlas_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAtlas.Sprites.Count == 0)
            {
                MessageBox.Show("No sprites defined to save.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save Atlas Data",
                Filter = "JSON files (*.json)|*.json|XML files (*.xml)|*.xml|CSS Sprite (*.css)|*.css|Unity Script (*.cs)|*.cs|All files (*.*)|*.*",
                DefaultExt = "json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var extension = Path.GetExtension(saveFileDialog.FileName).ToLower();
                    
                    switch (extension)
                    {
                        case ".json":
                            AtlasExporter.ExportToJson(_currentAtlas, saveFileDialog.FileName);
                            break;
                        case ".xml":
                            AtlasExporter.ExportToXml(_currentAtlas, saveFileDialog.FileName);
                            break;
                        case ".css":
                            var imageName = _currentImagePath != null ? Path.GetFileName(_currentImagePath) : "sprite-atlas.png";
                            AtlasExporter.ExportToCss(_currentAtlas, saveFileDialog.FileName, imageName);
                            break;
                        case ".cs":
                            AtlasExporter.ExportToUnityScript(_currentAtlas, saveFileDialog.FileName);
                            break;
                        default:
                            AtlasExporter.ExportToJson(_currentAtlas, saveFileDialog.FileName);
                            break;
                    }
                    
                    StatusText.Text = $"Atlas data saved to {Path.GetFileName(saveFileDialog.FileName)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving atlas data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadAtlas_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Load Atlas Data",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(openFileDialog.FileName);
                    var atlas = JsonSerializer.Deserialize<SpriteAtlas>(json);
                    
                    if (atlas != null)
                    {
                        _currentAtlas = atlas;
                        SpriteListBox.ItemsSource = _currentAtlas.Sprites;
                        
                        // Try to load the referenced image
                        if (File.Exists(_currentAtlas.ImagePath))
                        {
                            LoadImage(_currentAtlas.ImagePath);
                        }
                        else
                        {
                            MessageBox.Show("Referenced image file not found. Please load the image manually.", 
                                          "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        
                        DrawAllSprites();
                        StatusText.Text = $"Atlas data loaded: {_currentAtlas.Sprites.Count} sprites";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading atlas data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportSprite_Click(object sender, RoutedEventArgs e)
        {
            if (SpriteListBox.SelectedItem is not SpriteInfo selectedSprite)
            {
                MessageBox.Show("Please select a sprite to export.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SpriteImage.Source is not BitmapSource bitmap)
            {
                MessageBox.Show("No image loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Title = "Export Sprite",
                Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*",
                DefaultExt = "png",
                FileName = selectedSprite.Name + ".png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var croppedBitmap = new CroppedBitmap(bitmap, 
                        new Int32Rect(selectedSprite.X, selectedSprite.Y, selectedSprite.Width, selectedSprite.Height));
                    
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
                    
                    using var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create);
                    encoder.Save(fileStream);
                    
                    StatusText.Text = $"Sprite '{selectedSprite.Name}' exported successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting sprite: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region View Operations

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomAt(1.25, new Point(ImageScrollViewer.ActualWidth / 2, ImageScrollViewer.ActualHeight / 2));
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomAt(0.8, new Point(ImageScrollViewer.ActualWidth / 2, ImageScrollViewer.ActualHeight / 2));
        }

        private void ZoomToFit_Click(object sender, RoutedEventArgs e)
        {
            if (SpriteImage.Source != null)
            {
                var containerWidth = ImageScrollViewer.ActualWidth;
                var containerHeight = ImageScrollViewer.ActualHeight;
                var imageWidth = SpriteImage.Source.Width;
                var imageHeight = SpriteImage.Source.Height;

                var scaleX = containerWidth / imageWidth;
                var scaleY = containerHeight / imageHeight;
                var newZoomFactor = Math.Min(scaleX, scaleY) * 0.9; // 90% to leave some margin

                // Reset transforms and apply new zoom
                _translateTransform.X = 0;
                _translateTransform.Y = 0;
                _zoomFactor = Math.Max(MinZoom, Math.Min(MaxZoom, newZoomFactor));
                UpdateZoom();
            }
        }

        private void ActualSize_Click(object sender, RoutedEventArgs e)
        {
            // Reset transforms and set to actual size
            _translateTransform.X = 0;
            _translateTransform.Y = 0;
            _zoomFactor = 1.0;
            UpdateZoom();
        }

        private void ResetView_Click(object sender, RoutedEventArgs e)
        {
            ResetZoomAndPan();
        }

        private void ResetZoomAndPan()
        {
            _zoomFactor = 1.0;
            _translateTransform.X = 0;
            _translateTransform.Y = 0;
            UpdateZoom(); // This will call DrawGrid() automatically
        }

        private void ZoomAt(double zoomDelta, Point centerPoint)
        {
            var newZoomFactor = _zoomFactor * zoomDelta;
            newZoomFactor = Math.Max(MinZoom, Math.Min(MaxZoom, newZoomFactor));

            if (Math.Abs(newZoomFactor - _zoomFactor) < 0.001)
                return; // No change needed

            // Calculate the point relative to the ImageContainer
            // centerPoint is relative to ImageScrollViewer, so we need to transform it to ImageContainer coordinates
            var relativePoint = centerPoint;
            
            try
            {
                // Transform the mouse position from ScrollViewer coordinates to ImageContainer coordinates
                var transform = ImageScrollViewer.TransformToDescendant(ImageContainer);
                relativePoint = transform.Transform(centerPoint);
            }
            catch (InvalidOperationException)
            {
                // If transform fails, use the centerPoint as-is (fallback behavior)
                // This can happen if the visual tree isn't fully constructed yet
                relativePoint = centerPoint;
            }
            
            // Apply zoom
            var previousZoom = _zoomFactor;
            _zoomFactor = newZoomFactor;
            
            // Adjust translation to keep the zoom point centered
            var deltaZoom = _zoomFactor / previousZoom;
            _translateTransform.X = (deltaZoom * (_translateTransform.X + relativePoint.X)) - relativePoint.X;
            _translateTransform.Y = (deltaZoom * (_translateTransform.Y + relativePoint.Y)) - relativePoint.Y;

            UpdateZoom();
        }

        private void UpdateZoom()
        {
            _scaleTransform.ScaleX = _zoomFactor;
            _scaleTransform.ScaleY = _zoomFactor;
            UpdateZoomText();
            
            // Redraw grid when zoom changes to maintain proper line thickness
            DrawGrid();
            
            // Redraw sprites and selection to maintain proper visual scaling
            DrawAllSprites();
            UpdateGhostSprites();
            if (_currentSelection.IsVisible)
            {
                DrawSelectionRectangle();
            }
        }

        private void UpdateZoomText()
        {
            ZoomText.Text = $"{_zoomFactor:P0}";
        }

        #endregion

        #region Zoom and Pan Event Handlers

        private void ImageScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Only zoom when Ctrl is held down
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true; // Prevent normal scrolling
                
                var zoomDelta = e.Delta > 0 ? 1.1 : 0.9;
                var mousePosition = e.GetPosition(ImageScrollViewer);
                
                ZoomAt(zoomDelta, mousePosition);
            }
            // If Ctrl is not held, allow normal scrolling behavior
        }

        private void ImageScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only handle if we're clicking on the image area and not panning
            if (SpriteImage.Source == null || _isPanning) return;

            // Get the position relative to the image
            var imagePosition = e.GetPosition(SpriteImage);
            
            // Check if click is within image bounds
            if (imagePosition.X >= 0 && imagePosition.Y >= 0 && 
                imagePosition.X <= SpriteImage.Source.Width && imagePosition.Y <= SpriteImage.Source.Height)
            {
                _isSelecting = true;
                _selectionStartPoint = ClampToImageBounds(imagePosition);
                ImageScrollViewer.CaptureMouse(); // Capture on ScrollViewer instead of Image
                
                ClearSelection();
                e.Handled = true; // Prevent further processing
            }
        }

        private void ImageScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (SpriteImage.Source != null)
            {
                // Get position relative to the SpriteImage (which accounts for transforms)
                var position = e.GetPosition(SpriteImage);
                
                // Clamp the position to image bounds for display
                var clampedPosition = ClampToImageBounds(position);
                
                MousePositionText.Text = $"X: {(int)clampedPosition.X}, Y: {(int)clampedPosition.Y}";

                if (_isSelecting && e.LeftButton == MouseButtonState.Pressed && !_isPanning)
                {
                    UpdateSelection(position);
                    e.Handled = true; // Mark event as handled during selection
                }
            }
        }

        private void ImageScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;
                ImageScrollViewer.ReleaseMouseCapture();
                
                var endPoint = e.GetPosition(SpriteImage);
                FinalizeSelection(endPoint);
                e.Handled = true; // Mark event as handled
            }
        }

        private void ImageScrollViewer_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SpriteImage.Source == null) return;

            _isPanning = true;
            _panStartPoint = e.GetPosition(ImageScrollViewer);
            _panOriginalOffset = new Point(_translateTransform.X, _translateTransform.Y);
            
            ImageScrollViewer.CaptureMouse();
            ImageScrollViewer.Cursor = Cursors.Hand;
            
            e.Handled = true; // Prevent context menu from appearing
        }

        private void ImageScrollViewer_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                ImageScrollViewer.ReleaseMouseCapture();
                ImageScrollViewer.Cursor = Cursors.Arrow;
                e.Handled = true;
            }
        }

        private void ImageScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning && e.RightButton == MouseButtonState.Pressed)
            {
                var currentPoint = e.GetPosition(ImageScrollViewer);
                var deltaX = currentPoint.X - _panStartPoint.X;
                var deltaY = currentPoint.Y - _panStartPoint.Y;

                _translateTransform.X = _panOriginalOffset.X + deltaX;
                _translateTransform.Y = _panOriginalOffset.Y + deltaY;
                
                // No need to redraw grid during panning since it moves with the transform
                e.Handled = true;
            }
        }

        #endregion

        #region Grid Operations

        private void DrawGrid()
        {
            GridCanvas.Children.Clear();

            if (!_showGrid || SpriteImage.Source == null) return;

            var imageWidth = SpriteImage.Source.Width;
            var imageHeight = SpriteImage.Source.Height;

            var gridBrush = new SolidColorBrush(_gridColor) { Opacity = _gridOpacity };
            
            // Calculate line thickness based on zoom level to maintain visual consistency
            // At normal zoom (1.0), use thickness of 1. As zoom decreases, increase thickness
            // so grid lines remain visible
            var lineThickness = Math.Max(0.5, 1.0 / _zoomFactor);

            // Draw vertical lines starting from x=0 (image origin)
            for (int x = 0; x <= imageWidth; x += _gridSizeX)
            {
                var line = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = imageHeight,
                    Stroke = gridBrush,
                    StrokeThickness = lineThickness
                };
                GridCanvas.Children.Add(line);
            }

            // Draw horizontal lines starting from y=0 (image origin)
            for (int y = 0; y <= imageHeight; y += _gridSizeY)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = imageWidth,
                    Y2 = y,
                    Stroke = gridBrush,
                    StrokeThickness = lineThickness
                };
                GridCanvas.Children.Add(line);
            }
        }

        #endregion

        #region Mouse Events

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // This event is now handled by ScrollViewer preview events
            // Keep this as fallback but don't do anything to avoid conflicts
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            // This event is now handled by ScrollViewer preview events
            // Keep this as fallback but don't do anything to avoid conflicts
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // This event is now handled by ScrollViewer preview events
            // Keep this as fallback but don't do anything to avoid conflicts
        }

        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only clear selection if we're not panning
            if (!_isPanning)
            {
                ClearSelection();
            }
            // Don't handle this event - let it bubble up for pan functionality
        }

        #endregion

        #region Selection Management

        private void UpdateSelection(Point currentPoint)
        {
            // Clamp both points to image bounds
            var clampedCurrentPoint = ClampToImageBounds(currentPoint);
            var clampedStartPoint = ClampToImageBounds(_selectionStartPoint);

            var left = Math.Min(clampedStartPoint.X, clampedCurrentPoint.X);
            var top = Math.Min(clampedStartPoint.Y, clampedCurrentPoint.Y);
            var width = Math.Abs(clampedCurrentPoint.X - clampedStartPoint.X);
            var height = Math.Abs(clampedCurrentPoint.Y - clampedStartPoint.Y);

            _currentSelection.X = left;
            _currentSelection.Y = top;
            _currentSelection.Width = width;
            _currentSelection.Height = height;
            _currentSelection.IsVisible = true;

            DrawSelectionRectangle();
            UpdateSelectionInfo();
        }

        private void FinalizeSelection(Point endPoint)
        {
            if (_currentSelection.Width > 5 && _currentSelection.Height > 5) // Minimum size threshold
            {
                AddSpriteButton.IsEnabled = true;
                SpriteNameTextBox.Text = $"Sprite_{_currentAtlas.Sprites.Count + 1}";
            }
            else
            {
                ClearSelection();
            }
        }

        private void ClearSelection()
        {
            _currentSelection.IsVisible = false;
            
            // Clear selection rectangle but keep ghost sprites
            var itemsToRemove = SelectionCanvas.Children.OfType<Rectangle>()
                .Where(r => r.Tag?.ToString() != "ghost" && 
                           r.Tag?.ToString() != "sprite" && 
                           r.Tag?.ToString() != "highlight").ToList();
            
            foreach (var item in itemsToRemove)
            {
                SelectionCanvas.Children.Remove(item);
            }

            AddSpriteButton.IsEnabled = false;
            SpriteNameTextBox.Text = "";
            SpriteTagTextBox.Text = "";
            UpdateSelectionInfo();
        }

        private void DrawSelectionRectangle()
        {
            SelectionCanvas.Children.Clear();

            if (_currentSelection.IsVisible)
            {
                // Calculate stroke thickness based on zoom level for consistent visibility
                var strokeThickness = Math.Max(1.0, 2.0 / _zoomFactor);
                
                _selectionRectangle = new Rectangle
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = strokeThickness,
                    StrokeDashArray = new DoubleCollection { 5 / _zoomFactor, 5 / _zoomFactor }, // Scale dash pattern with zoom
                    Fill = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0)),
                    Width = _currentSelection.Width,
                    Height = _currentSelection.Height
                };

                Canvas.SetLeft(_selectionRectangle, _currentSelection.X);
                Canvas.SetTop(_selectionRectangle, _currentSelection.Y);
                SelectionCanvas.Children.Add(_selectionRectangle);
            }
        }

        #endregion

        #region Sprite Management

        private void AddSprite_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentSelection.IsVisible || string.IsNullOrWhiteSpace(SpriteNameTextBox.Text))
                return;

            var sprite = new SpriteInfo
            {
                Name = SpriteNameTextBox.Text.Trim(),
                X = (int)_currentSelection.X,
                Y = (int)_currentSelection.Y,
                Width = (int)_currentSelection.Width,
                Height = (int)_currentSelection.Height,
                Tag = SpriteTagTextBox.Text.Trim()
            };

            _currentAtlas.Sprites.Add(sprite);
            SpriteListBox.ItemsSource = null;
            SpriteListBox.ItemsSource = _currentAtlas.Sprites;

            // Create ghost sprite before clearing selection
            CreateGhostSprite(sprite);

            DrawAllSprites();
            ClearSelection();
            StatusText.Text = $"Added sprite: {sprite.Name}";
        }

        private void RemoveSprite_Click(object sender, RoutedEventArgs e)
        {
            if (SpriteListBox.SelectedItem is SpriteInfo selectedSprite)
            {
                _currentAtlas.Sprites.Remove(selectedSprite);
                SpriteListBox.ItemsSource = null;
                SpriteListBox.ItemsSource = _currentAtlas.Sprites;
                DrawAllSprites();
                StatusText.Text = $"Removed sprite: {selectedSprite.Name}";
            }
        }

        private void EditSprite_Click(object sender, RoutedEventArgs e)
        {
            if (SpriteListBox.SelectedItem is SpriteInfo selectedSprite)
            {
                // Create selection from sprite bounds
                _currentSelection.X = selectedSprite.X;
                _currentSelection.Y = selectedSprite.Y;
                _currentSelection.Width = selectedSprite.Width;
                _currentSelection.Height = selectedSprite.Height;
                _currentSelection.IsVisible = true;

                SpriteNameTextBox.Text = selectedSprite.Name;
                SpriteTagTextBox.Text = selectedSprite.Tag ?? "";
                AddSpriteButton.IsEnabled = true;

                DrawSelectionRectangle();
                UpdateSelectionInfo();

                // Remove the sprite temporarily so it can be re-added with new values
                _currentAtlas.Sprites.Remove(selectedSprite);
                SpriteListBox.ItemsSource = null;
                SpriteListBox.ItemsSource = _currentAtlas.Sprites;
                DrawAllSprites();
            }
        }

        private void SpriteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveSpriteButton.IsEnabled = SpriteListBox.SelectedItem != null;
            EditSpriteButton.IsEnabled = SpriteListBox.SelectedItem != null;

            if (SpriteListBox.SelectedItem is SpriteInfo selectedSprite)
            {
                HighlightSprite(selectedSprite);
            }
        }

        private void HighlightSprite(SpriteInfo sprite)
        {
            // Clear existing highlights
            var highlightsToRemove = SelectionCanvas.Children.OfType<Rectangle>()
                .Where(r => r.Tag?.ToString() == "highlight").ToList();
            
            foreach (var highlight in highlightsToRemove)
            {
                SelectionCanvas.Children.Remove(highlight);
            }

            // Calculate stroke thickness based on zoom level for consistent visibility
            var strokeThickness = Math.Max(2.0, 3.0 / _zoomFactor);

            // Add new highlight
            var highlightRect = new Rectangle
            {
                Stroke = Brushes.Yellow,
                StrokeThickness = strokeThickness,
                Fill = new SolidColorBrush(Color.FromArgb(50, 255, 255, 0)),
                Width = sprite.Width,
                Height = sprite.Height,
                Tag = "highlight"
            };

            Canvas.SetLeft(highlightRect, sprite.X);
            Canvas.SetTop(highlightRect, sprite.Y);
            SelectionCanvas.Children.Add(highlightRect);
        }

        private void DrawAllSprites()
        {
            // Clear existing sprite rectangles (but keep selection and highlights)
            var spritesToRemove = SelectionCanvas.Children.OfType<Rectangle>()
                .Where(r => r.Tag?.ToString() == "sprite").ToList();
            var labelsToRemove = SelectionCanvas.Children.OfType<TextBlock>()
                .Where(r => r.Tag?.ToString() == "sprite").ToList();
            
            foreach (var sprite in spritesToRemove)
            {
                SelectionCanvas.Children.Remove(sprite);
            }
            foreach (var label in labelsToRemove)
            {
                SelectionCanvas.Children.Remove(label);
            }

            if (_currentAtlas.Sprites.Count == 0) return;

            foreach (var sprite in _currentAtlas.Sprites)
            {
                var spriteRect = new Rectangle
                {
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1 / _zoomFactor,
                    Fill = Brushes.Transparent,
                    Width = sprite.Width,
                    Height = sprite.Height,
                    Tag = "sprite"
                };

                var zoomFactor = _zoomFactor;

                // Adjust position considering the current translation and zoom
                var adjustedX = (sprite.X) * zoomFactor + _translateTransform.X;
                var adjustedY = (sprite.Y) * zoomFactor + _translateTransform.Y;

                Canvas.SetLeft(spriteRect, adjustedX);
                Canvas.SetTop(spriteRect, adjustedY);

                SelectionCanvas.Children.Add(spriteRect);

                // Draw sprite name label
                var label = new TextBlock
                {
                    Text = sprite.Name,
                    Foreground = Brushes.White,
                    Background = Brushes.Black,
                    Tag = "sprite",
                    Padding = new Thickness(2),
                    FontSize = 12 / zoomFactor
                };

                // Adjust label position to be at the sprite's position
                Canvas.SetLeft(label, adjustedX);
                Canvas.SetTop(label, adjustedY + sprite.Height * zoomFactor);

                SelectionCanvas.Children.Add(label);
            }
        }

        private void CreateGhostSprite(SpriteInfo sprite)
        {
            // Remove existing ghost sprites
            foreach (var ghost in _ghostSprites)
            {
                SelectionCanvas.Children.Remove(ghost);
            }
            _ghostSprites.Clear();

            if (sprite == null) return;

            var ghostSprite = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Color.FromArgb(120, 255, 0, 0)),
                Width = sprite.Width,
                Height = sprite.Height,
                Tag = "ghost"
            };

            Canvas.SetLeft(ghostSprite, sprite.X);
            Canvas.SetTop(ghostSprite, sprite.Y);

            SelectionCanvas.Children.Add(ghostSprite);
            _ghostSprites.Add(ghostSprite);
        }

        private void UpdateGhostSprites()
        {
            // Update existing ghost sprites to match sprite positions
            foreach (var sprite in _currentAtlas.Sprites)
            {
                var ghost = _ghostSprites.FirstOrDefault(g => g.Tag?.ToString() == "ghost" && 
                                                             Canvas.GetLeft(g) == sprite.X &&
                                                             Canvas.GetTop(g) == sprite.Y);
                
                if (ghost != null)
                {
                    // Update position
                    var zoomFactor = _zoomFactor;
                    var adjustedX = (sprite.X) * zoomFactor + _translateTransform.X;
                    var adjustedY = (sprite.Y) * zoomFactor + _translateTransform.Y;

                    Canvas.SetLeft(ghost, adjustedX);
                    Canvas.SetTop(ghost, adjustedY);
                }
            }
        }

        private void ClearSprites_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear all sprites?", "Confirm", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _currentAtlas.Sprites.Clear();
                SpriteListBox.ItemsSource = null;
                SpriteListBox.ItemsSource = _currentAtlas.Sprites;
                ClearGhostSprites(); // Clear ghost sprites when clearing all sprites
                DrawAllSprites();
                StatusText.Text = "All sprites cleared";
            }
        }

        private void ClearGhostSprites()
        {
            // Remove existing ghost sprites
            foreach (var ghost in _ghostSprites)
            {
                SelectionCanvas.Children.Remove(ghost);
            }
            _ghostSprites.Clear();
        }

        #endregion

        #region Tool Operations

        private void GridSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new GridSettingsDialog
            {
                Owner = this,
                ShowGrid = _showGrid,
                GridSizeX = _gridSizeX,
                GridSizeY = _gridSizeY,
                GridColor = _gridColor,
                GridOpacity = _gridOpacity
            };

            if (dialog.ShowDialog() == true)
            {
                _showGrid = dialog.ShowGrid;
                _gridSizeX = dialog.GridSizeX;
                _gridSizeY = dialog.GridSizeY;
                _gridColor = dialog.GridColor;
                _gridOpacity = dialog.GridOpacity;
                
                DrawGrid();
                StatusText.Text = "Grid settings updated";
            }
        }

        #endregion

        #region UI Updates

        private void UpdateImageInfo()
        {
            if (_currentImageMetadata != null && _currentImagePath != null)
            {
                ImagePathText.Text = Path.GetFileName(_currentImagePath);
                DimensionsText.Text = $"{_currentImageMetadata.Width} x {_currentImageMetadata.Height}";
                BitDepthText.Text = $"{_currentImageMetadata.BitDepth} bits";
                FormatText.Text = _currentImageMetadata.Format;
                DpiText.Text = $"{_currentImageMetadata.DpiX:F0} x {_currentImageMetadata.DpiY:F0}";
                FileSizeText.Text = FormatFileSize(_currentImageMetadata.FileSizeBytes);
                TransparencyText.Text = _currentImageMetadata.HasTransparency ? "Yes" : "No";
            }
            else
            {
                ImagePathText.Text = "No image loaded";
                DimensionsText.Text = "-";
                BitDepthText.Text = "-";
                FormatText.Text = "-";
                DpiText.Text = "-";
                FileSizeText.Text = "-";
                TransparencyText.Text = "-";
            }
        }

        private void UpdateSelectionInfo()
        {
            UpdateSelectionTextBoxes();
        }

        private void UpdateUI()
        {
            UpdateImageInfo();
            UpdateSelectionInfo();
            UpdateZoomText();
        }

        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }

        #endregion

        #region Manual Selection Editing

        private void SelectionCoordinate_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Don't process if we're updating programmatically
            if (_updatingTextBoxesProgrammatically) return;

            // Only process if we have a current selection
            if (!_currentSelection.IsVisible) return;

            var textBox = sender as TextBox;
            if (textBox?.Tag == null) return;

            var coordinate = textBox.Tag.ToString();
            
            // Try to parse the value
            if (!int.TryParse(textBox.Text, out int value))
            {
                // Invalid input, revert to current value
                UpdateSelectionTextBoxes();
                return;
            }

            // Ensure value is within reasonable bounds
            if (SpriteImage.Source != null)
            {
                switch (coordinate)
                {
                    case "X":
                        value = Math.Max(0, Math.Min(value, (int)SpriteImage.Source.Width - 1));
                        _currentSelection.X = value;
                        break;
                    case "Y":
                        value = Math.Max(0, Math.Min(value, (int)SpriteImage.Source.Height - 1));
                        _currentSelection.Y = value;
                        break;
                    case "Width":
                        value = Math.Max(1, Math.Min(value, (int)SpriteImage.Source.Width - (int)_currentSelection.X));
                        _currentSelection.Width = value;
                        break;
                    case "Height":
                        value = Math.Max(1, Math.Min(value, (int)SpriteImage.Source.Height - (int)_currentSelection.Y));
                        _currentSelection.Height = value;
                        break;
                }
            }

            // Update the visual selection
            DrawSelectionRectangle();
            
            // Update text boxes with clamped values (without triggering events)
            UpdateSelectionTextBoxes();
            
            // Enable Add Sprite button if selection is valid
            if (_currentSelection.Width > 0 && _currentSelection.Height > 0)
            {
                AddSpriteButton.IsEnabled = true;
                if (string.IsNullOrWhiteSpace(SpriteNameTextBox.Text))
                {
                    SpriteNameTextBox.Text = $"Sprite_{_currentAtlas.Sprites.Count + 1}";
                }
            }
        }

        private void UpdateSelectionTextBoxes()
        {
            _updatingTextBoxesProgrammatically = true;

            if (_currentSelection.IsVisible)
            {
                SelectionXTextBox.Text = ((int)_currentSelection.X).ToString();
                SelectionYTextBox.Text = ((int)_currentSelection.Y).ToString();
                SelectionWidthTextBox.Text = ((int)_currentSelection.Width).ToString();
                SelectionHeightTextBox.Text = ((int)_currentSelection.Height).ToString();
            }
            else
            {
                SelectionXTextBox.Text = "";
                SelectionYTextBox.Text = "";
                SelectionWidthTextBox.Text = "";
                SelectionHeightTextBox.Text = "";
            }

            _updatingTextBoxesProgrammatically = false;
        }

        #endregion

        #region Coordinate Transformation Helpers

        /// <summary>
        /// Converts a point from ScrollViewer coordinates to Image pixel coordinates
        /// </summary>
        private Point TransformScrollViewerToImage(Point scrollViewerPoint)
        {
            try
            {
                // Transform from ScrollViewer to Image coordinates
                var imagePoint = ImageScrollViewer.TransformToDescendant(SpriteImage).Transform(scrollViewerPoint);
                
                // Clamp to image bounds
                if (SpriteImage.Source != null)
                {
                    imagePoint.X = Math.Max(0, Math.Min(imagePoint.X, SpriteImage.Source.Width));
                    imagePoint.Y = Math.Max(0, Math.Min(imagePoint.Y, SpriteImage.Source.Height));
                }
                
                return imagePoint;
            }
            catch (InvalidOperationException)
            {
                // Fallback: return the point as-is if transform fails
                return scrollViewerPoint;
            }
        }

        /// <summary>
        /// Ensures a point is within image bounds
        /// </summary>
        private Point ClampToImageBounds(Point point)
        {
            if (SpriteImage.Source == null) return point;
            
            return new Point(
                Math.Max(0, Math.Min(point.X, SpriteImage.Source.Width)),
                Math.Max(0, Math.Min(point.Y, SpriteImage.Source.Height))
            );
        }

        #endregion
    }
}
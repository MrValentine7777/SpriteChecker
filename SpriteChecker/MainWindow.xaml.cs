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

        // Zoom settings
        private double _zoomFactor = 1.0;
        private ScaleTransform _scaleTransform = new ScaleTransform();

        public MainWindow()
        {
            InitializeComponent();
            
            // Set up zoom transform
            ImageContainer.RenderTransform = _scaleTransform;
            
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
            _zoomFactor *= 1.25;
            UpdateZoom();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            _zoomFactor *= 0.8;
            UpdateZoom();
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
                _zoomFactor = Math.Min(scaleX, scaleY) * 0.9; // 90% to leave some margin

                UpdateZoom();
            }
        }

        private void ActualSize_Click(object sender, RoutedEventArgs e)
        {
            _zoomFactor = 1.0;
            UpdateZoom();
        }

        private void UpdateZoom()
        {
            _scaleTransform.ScaleX = _zoomFactor;
            _scaleTransform.ScaleY = _zoomFactor;
            UpdateZoomText();
        }

        private void UpdateZoomText()
        {
            ZoomText.Text = $"{_zoomFactor:P0}";
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

            // Draw vertical lines
            for (int x = 0; x <= imageWidth; x += _gridSizeX)
            {
                var line = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = imageHeight,
                    Stroke = gridBrush,
                    StrokeThickness = 1
                };
                GridCanvas.Children.Add(line);
            }

            // Draw horizontal lines
            for (int y = 0; y <= imageHeight; y += _gridSizeY)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = imageWidth,
                    Y2 = y,
                    Stroke = gridBrush,
                    StrokeThickness = 1
                };
                GridCanvas.Children.Add(line);
            }
        }

        #endregion

        #region Mouse Events

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SpriteImage.Source == null) return;

            _isSelecting = true;
            _selectionStartPoint = e.GetPosition(SpriteImage);
            SpriteImage.CaptureMouse();

            ClearSelection();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(SpriteImage);
            MousePositionText.Text = $"X: {(int)position.X}, Y: {(int)position.Y}";

            if (_isSelecting && e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateSelection(position);
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;
                SpriteImage.ReleaseMouseCapture();
                
                var endPoint = e.GetPosition(SpriteImage);
                FinalizeSelection(endPoint);
            }
        }

        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Clear selection on right click
            ClearSelection();
        }

        #endregion

        #region Selection Management

        private void UpdateSelection(Point currentPoint)
        {
            var left = Math.Min(_selectionStartPoint.X, currentPoint.X);
            var top = Math.Min(_selectionStartPoint.Y, currentPoint.Y);
            var width = Math.Abs(currentPoint.X - _selectionStartPoint.X);
            var height = Math.Abs(currentPoint.Y - _selectionStartPoint.Y);

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
            SelectionCanvas.Children.Clear();
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
                _selectionRectangle = new Rectangle
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection { 5, 5 },
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

            // Add new highlight
            var highlightRect = new Rectangle
            {
                Stroke = Brushes.Yellow,
                StrokeThickness = 3,
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

            // Draw all sprites
            foreach (var sprite in _currentAtlas.Sprites)
            {
                var rect = new Rectangle
                {
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1,
                    Fill = new SolidColorBrush(Color.FromArgb(20, 0, 0, 255)),
                    Width = sprite.Width,
                    Height = sprite.Height,
                    Tag = "sprite"
                };

                Canvas.SetLeft(rect, sprite.X);
                Canvas.SetTop(rect, sprite.Y);
                SelectionCanvas.Children.Add(rect);

                // Add sprite name label
                var label = new TextBlock
                {
                    Text = sprite.Name,
                    Foreground = Brushes.Blue,
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Background = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
                    Tag = "sprite"
                };

                Canvas.SetLeft(label, sprite.X);
                Canvas.SetTop(label, sprite.Y - 15);
                SelectionCanvas.Children.Add(label);
            }
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

        private void ClearSprites_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear all sprites?", "Confirm", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _currentAtlas.Sprites.Clear();
                SpriteListBox.ItemsSource = null;
                SpriteListBox.ItemsSource = _currentAtlas.Sprites;
                DrawAllSprites();
                StatusText.Text = "All sprites cleared";
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
            if (_currentSelection.IsVisible)
            {
                SelectionXText.Text = ((int)_currentSelection.X).ToString();
                SelectionYText.Text = ((int)_currentSelection.Y).ToString();
                SelectionWidthText.Text = ((int)_currentSelection.Width).ToString();
                SelectionHeightText.Text = ((int)_currentSelection.Height).ToString();
            }
            else
            {
                SelectionXText.Text = "-";
                SelectionYText.Text = "-";
                SelectionWidthText.Text = "-";
                SelectionHeightText.Text = "-";
            }
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
    }
}
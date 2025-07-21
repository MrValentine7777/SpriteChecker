using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpriteChecker.Controls
{
    public partial class GridSettingsDialog : Window
    {
        public bool ShowGrid { get; set; }
        public int GridSizeX { get; set; } = 32;
        public int GridSizeY { get; set; } = 32;
        public Color GridColor { get; set; } = Colors.Gray;
        public double GridOpacity { get; set; } = 0.5;

        public GridSettingsDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
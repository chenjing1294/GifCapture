using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GifCapture.Models;
using GifCapture.ViewModels;

namespace GifCapture.Windows
{
    public partial class RecordBarWindow : Window
    {
        private readonly int _width = 200;
        private readonly int _height = 30;

        public RecordBarWindow(MainViewModel mainViewModel, Rectangle rectangle)
        {
            this.DataContext = mainViewModel;
            InitializeComponent();
            Rectangle screen = SystemInformation.VirtualScreen;
            int left = (int) ((rectangle.X + rectangle.Width / 2) / Dpi.X - _width / 2);
            int top = (int) ((rectangle.Y + rectangle.Height) / Dpi.Y + 10);
            if (top > screen.Height - _height)
            {
                top = screen.Height - _height;
            }

            if (left > screen.Width - _width)
            {
                left = screen.Width / 2 - _width / 2;
            }

            this.Top = top;
            this.Left = left;
            this.Width = _width;
            this.Height = _height;
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.StopRecord_OnClick(null, null);
            this.Close();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
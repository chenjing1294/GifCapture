using System.Windows;
using System.Windows.Controls;

namespace GifCapture.Controls
{
    public partial class Magnifier : UserControl
    {
        public Magnifier()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(nameof(Target), typeof(UIElement), typeof(Magnifier));


        public UIElement Target
        {
            get => (UIElement) GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        public void UpdateViewBox(Point point, Size size)
        {
            if (PART_VisualBrush != null)
            {
                PART_VisualBrush.Viewbox = new Rect(point, size);
            }
        }

        public void UpdatePositionTextBlock(Point point, Size size)
        {
            PositionTextBlock.Text = $"X,Y={point.X},{point.Y} WxH={size.Width}x{size.Height}";
        }

        public void HideRectangle()
        {
            Rectangle1.Visibility = Visibility.Hidden;
            Rectangle2.Visibility = Visibility.Hidden;
            Rectangle3.Visibility = Visibility.Hidden;
        }

        public void ShowRectangle()
        {
            Rectangle1.Visibility = Visibility.Visible;
            Rectangle2.Visibility = Visibility.Visible;
            Rectangle3.Visibility = Visibility.Visible;
        }
    }
}
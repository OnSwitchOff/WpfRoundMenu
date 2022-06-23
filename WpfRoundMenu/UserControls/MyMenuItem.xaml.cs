using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfRoundMenu.UserControls
{
    /// <summary>
    /// Interaction logic for MenuItem.xaml
    /// </summary>
    public partial class MyMenuItem : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MyMenuItem), new PropertyMetadata(OnTitlePropertyChanged));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set
            {
                SetValue(TitleProperty, value);
            }
        }


        public MyMenuItem()
        {
            InitializeComponent();
        }

        static void OnTitlePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {

            MyMenuItem ctrl = obj as MyMenuItem;
            ctrl.MainGrid.Children.Clear();

            if (String.IsNullOrEmpty(ctrl.Title))
                return;

            foreach (Char ch in ctrl.Title)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = ch.ToString();
                textBlock.FontSize = 14;
                ctrl.MainGrid.Children.Add(textBlock);
            }

            ctrl.OrientTextOnPath();
        }

        void OrientTextOnPath()
        {
            double FONTSIZE = 14;

            PathGeometry path = PG.GetFlattenedPathGeometry();

            double length = 0.0;

            foreach (PathFigure pf in path.Figures)
            {
                Point start = pf.StartPoint;

                foreach (var seg in pf.Segments)
                {
                    if (seg.GetType() == typeof(PolyLineSegment))
                    {
                        foreach (Point point in (seg as PolyLineSegment).Points)
                        {
                            length += Distance(start, point);
                            start = point;
                        }
                    }
                    if (seg.GetType() == typeof(LineSegment))
                    {
                        Point point = (seg as LineSegment).Point;
                        length += Distance(start, point) ;
                        start = point;                        
                    }

                }
            }

            double pathLength = length;
            double textLength = 0;

            foreach (UIElement child in MainGrid.Children)
            {
                child.Measure(new Size(Double.PositiveInfinity,
                    Double.PositiveInfinity));
                textLength += child.DesiredSize.Width;
            }

            if (pathLength == 0 || textLength == 0)
                return;

            double scalingFactor = pathLength / textLength;
            PathGeometry pathGeometry = PG;
            double baseline =
                scalingFactor * FONTSIZE * FontFamily.Baseline;
            double progress = 0;

            foreach (UIElement child in MainGrid.Children)
            {
                double width = scalingFactor * child.DesiredSize.Width;
                progress += width / 2 / pathLength;
                Point point, tangent;

                pathGeometry.GetPointAtFractionLength(progress,
                    out point, out tangent);

                TransformGroup transformGroup = new TransformGroup();

                transformGroup.Children.Add(
                    new ScaleTransform(scalingFactor, scalingFactor));
                transformGroup.Children.Add(
                    new RotateTransform(Math.Atan2(tangent.Y, tangent.X)
                    * 180 / Math.PI, width / 2, baseline));
                transformGroup.Children.Add(
                    new TranslateTransform(point.X - width / 2,
                    point.Y - baseline));

                child.RenderTransform = transformGroup;
                progress += width / 2 / pathLength;
            }
            
        }
        private static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}

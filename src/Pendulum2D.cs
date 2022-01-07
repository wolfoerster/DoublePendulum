//******************************************************************************************
// Copyright © 2016 - 2022 Wolfgang Foerster (wolfoerster@gmx.de)
//
// This file is part of the DoublePendulum project which can be found on github.com
//
// DoublePendulum is free software: you can redistribute it and/or modify it under the terms 
// of the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.
// 
// DoublePendulum is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
//******************************************************************************************

namespace DoublePendulum
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using WFTools3D;

    class Pendulum2D : Canvas
    {
        private readonly Line line1 = new Line();
        private readonly Line line2 = new Line();
        private readonly Ellipse axis = new Ellipse();
        private readonly Ellipse weight1 = new Ellipse();
        private readonly Ellipse weight2 = new Ellipse();
        private readonly Ellipse omega1 = new Ellipse();
        private readonly Ellipse omega2 = new Ellipse();
        private readonly Brush hot = Brushes.Red;
        private readonly Brush cold = Brushes.Green;
        private readonly Brush warm1 = Brushes.Khaki;
        private readonly Brush warm2 = Brushes.DarkKhaki;
        private readonly TextBox textBox = new TextBox { IsReadOnly = true, Width = 70, Margin = new Thickness(4, 4, 0, 0) };
        private readonly CheckBox checkBox = new CheckBox { Content = "Show Ω", Margin = new Thickness(4, 280, 0, 0), ToolTip = "Show/adjust angular velocities/accelerations" };
        private readonly Button startButton = new Button { Content = " Start ", Margin = new Thickness(258, 273, 0, 0), Visibility = Visibility.Collapsed };
        private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
        private Ellipse hotElli; // one of the above ellipses that the mouse is over
        private Point position1; // the position of the first weight
        private Vector? delta;

        public Pendulum2D()
        {
            ShowOmega = false;
            SizeChanged += MySizeChanged;

            axis.Fill = line1.Stroke = line2.Stroke = cold;

            line1.StrokeStartLineCap = line2.StrokeStartLineCap =
                line1.StrokeEndLineCap = line2.StrokeEndLineCap = PenLineCap.Round;

            weight1.Fill = weight2.Fill = warm1;
            omega1.Fill = omega2.Fill = warm2;

            weight1.ToolTip = weight2.ToolTip = "Click and drag to initial angle";
            omega1.ToolTip = omega2.ToolTip = "Click and drag to initial angular velocity";

            Children.Add(line1);
            Children.Add(line2);
            Children.Add(axis);
            Children.Add(weight1);
            Children.Add(weight2);
            Children.Add(omega1);
            Children.Add(omega2);

            Children.Add(textBox);
            Children.Add(checkBox);
            Children.Add(startButton);

            checkBox.Click += ShowOmegaClick;
            startButton.Click += StartButtonClick;

            timer.Interval = TimeSpan.FromSeconds(0.2);
            timer.Tick += Timer_Tick;
        }

        public void Clear()
        {
            startButton.Visibility = Visibility.Collapsed;
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            startButton.Visibility = Visibility.Collapsed;
            StartSim?.Invoke(this, EventArgs.Empty);
        }

        private void ShowOmegaClick(object sender, RoutedEventArgs e)
        {
            ShowOmega = (sender as CheckBox).IsChecked.Value;
            Update();
        }

        public event EventHandler BeginDrag;

        public event EventHandler IsDragging;

        public event EventHandler StartSim;

        public bool ShowOmega { get; set; }

        public bool IsBusy { get; set; }

        /// <summary>
        /// Make the second weight look hot for a short period. 
        /// </summary>
        public void NewPoincarePoint()
        {
            if (timer.IsEnabled)
                return;

            hotElli = weight2;
            hotElli.Fill = hot;
            timer.Start();
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            CoolDownHotElli();
        }

        #region Drawing

        void MySizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update();
        }

        public void Update()
        {
            var pendulum = App.SelectedPendulum;
            double radius = Radius;
            double length = Length;

            Point p0 = Center;

            Point p1 = p0;
            p1.X += length * Math.Sin(pendulum.Q1);
            p1.Y += length * Math.Cos(pendulum.Q1);
            position1 = p1;

            Point p2 = p1;
            p2.X += length * Math.Sin(pendulum.Q2);
            p2.Y += length * Math.Cos(pendulum.Q2);

            AdjustLine(line1, p0, p1, radius);
            AdjustLine(line2, p1, p2, radius);
            AdjustEllipse(axis, p0, radius / 2);
            AdjustEllipse(weight1, p1, radius);
            AdjustEllipse(weight2, p2, radius);

            radius = ShowOmega ? radius / 3 : 0;
            Point p3 = Center;
            p3.X += length * Math.Sin(pendulum.Q1 + pendulum.W1);
            p3.Y += length * Math.Cos(pendulum.Q1 + pendulum.W1);
            AdjustEllipse(omega1, p3, radius);

            Point p4 = position1;
            p4.X += length * Math.Sin(pendulum.Q2 + pendulum.W2);
            p4.Y += length * Math.Cos(pendulum.Q2 + pendulum.W2);
            AdjustEllipse(omega2, p4, radius);

            InvalidateVisual();
        }

        double Size => Math.Min(ActualHeight, ActualWidth);

        double Radius => Size / 30;

        double Length => (Size / 2 - Radius) / 2.2;

        private void AdjustEllipse(Ellipse ellipse, Point position, double radius)
        {
            ellipse.Width = ellipse.Height = 2 * radius;
            Canvas.SetTop(ellipse, position.Y - radius);
            Canvas.SetLeft(ellipse, position.X - radius);
        }

        private void AdjustLine(Line line, Point p1, Point p2, double radius)
        {
            line.StrokeThickness = radius / 2;
            line.X1 = p1.X;
            line.Y1 = p1.Y;
            line.X2 = p2.X;
            line.Y2 = p2.Y;
        }

        #endregion Drawing

        #region Overrides

        protected override void OnRender(DrawingContext dc)
        {
            var pendulum = App.SelectedPendulum;

            if (Background != null)
            {
                var rect = new Rect(new Size(ActualWidth, ActualHeight));
                dc.DrawGeometry(Background, null, new RectangleGeometry(rect));
            }

            textBox.Text = $"E0: {pendulum.E0.ToStringInv()}";

            if (!ShowOmega)
                return;

            double thickness = line1.StrokeThickness * 0.667;

            Pen pen = new Pen(cold, thickness);
            DrawArc(dc, pendulum.Q1, pendulum.W1, Center, pen);
            DrawArc(dc, pendulum.Q2, pendulum.W2, position1, pen);

            pen = new Pen(hot, thickness);
            DrawArc(dc, pendulum.Q1, pendulum.A1, Center, pen);
            DrawArc(dc, pendulum.Q2, pendulum.A2, position1, pen);
        }

        private void DrawArc(DrawingContext dc, double q, double w, Point origin, Pen pen)
        {
            double start = q - MathUtils.PIo2;
            double stop = start + w;
            Geometry arc = ArcGeometry.Create(origin, Length, start, stop, pen.Thickness);
            dc.DrawGeometry(null, pen, arc);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (hotElli != null)
            {
                double x = Canvas.GetLeft(hotElli);
                double y = Canvas.GetTop(hotElli);
                double r = hotElli.Width * 0.5;
                delta = new Point(x + r, y + r) - e.GetPosition(this);
                CaptureMouse();
                BeginDrag?.Invoke(this, new EventArgs());
                startButton.Visibility = Visibility.Visible;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            delta = null;
            ReleaseMouseCapture(); // that calls OnMouseLeave() if mouse is outside of hotElli which then sets hotElli to null
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            Cursor = null;
            CoolDownHotElli();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsBusy)
                return;

            if (delta == null)
                CheckForHotElli(e);

            if (hotElli == null)
            {
                Cursor = null;
            }
            else
            {
                Cursor = Cursors.Hand;
                if (e.LeftButton == MouseButtonState.Pressed && delta.HasValue)
                {
                    MoveHotElli(e.GetPosition(this) + delta.Value);
                }
            }
        }

        #endregion Overrides

        #region Helpers

        /// <summary>
        /// Gets the center of this element.
        /// </summary>
        private Point Center
        {
            get { return new Point(ActualWidth / 2, ActualHeight / 2); }
        }

        private void CoolDownHotElli()
        {
            if (hotElli != null)
            {
                hotElli.Fill = (hotElli == weight1 || hotElli == weight2) ? warm1 : warm2;
                hotElli = null;
            }
        }

        private void CheckForHotElli(MouseEventArgs e)
        {
            Ellipse eli = GetEllipse(e);

            if (eli == null)
            {
                CoolDownHotElli();
                return;
            }

            if (eli == hotElli)
                return;

            CoolDownHotElli();

            hotElli = eli;
            hotElli.Fill = hot;
        }

        private Ellipse GetEllipse(MouseEventArgs e)
        {
            bool IsHit(Ellipse eli)
            {
                Point pt = e.GetPosition(eli);
                return VisualTreeHelper.HitTest(eli, pt) != null;
            }

            if (IsHit(omega2))
                return omega2;

            if (IsHit(omega1))
                return omega1;

            if (IsHit(weight2))
                return weight2;

            if (IsHit(weight1))
                return weight1;

            return null;
        }

        private void MoveHotElli(Point pt)
        {
            var pendulum = App.SelectedPendulum;

            double q1 = pendulum.Q1;
            double q2 = pendulum.Q2;
            double w1 = pendulum.W1;
            double w2 = pendulum.W2;

            if (!ShowOmega)
                w1 = w2 = 0;

            if (hotElli == weight1)
            {
                Vector v = pt - Center;
                q1 = Math.Atan2(v.X, v.Y);
            }
            else if (hotElli == weight2)
            {
                Vector v = pt - position1;
                q2 = Math.Atan2(v.X, v.Y);
            }
            else if (hotElli == omega1)
            {
                Vector v = pt - Center;
                v = Rotate(v, q1 - MathUtils.PIo2);
                w1 = Math.Atan2(-v.Y, v.X);
            }
            else if (hotElli == omega2)
            {
                Vector v = pt - position1;
                v = Rotate(v, q2 - MathUtils.PIo2);
                w2 = Math.Atan2(-v.Y, v.X);
            }

            pendulum.Init(q1, q2, w1, w2);
            Update();

            IsDragging?.Invoke(this, new EventArgs());
        }

        Vector Rotate(Vector v, double angle)
        {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);
            return new Vector(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
        }

        #endregion Helpers
    }
}

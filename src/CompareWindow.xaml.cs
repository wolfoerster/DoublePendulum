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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;
    using WFTools3D;
    using PendulumModel = WFTools3D.Pendulum;

    /// <summary>
    /// Interaction logic for CompareWindow.xaml
    /// </summary>
    public partial class CompareWindow : Window
    {
        private readonly List<Pendulator> pendulators = new List<Pendulator>();
        private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
        private readonly Helper helper;

        public CompareWindow(List<Pendulum> pendulums)
        {
            InitializeComponent();
            SetSizeAndPosition();

            helper = new Helper(pendulators);
            scene2D.Child = helper;

            var eps = 0.0;
            var brush1 = Resources["wood1"] as ImageBrush;
            var brush2 = Resources["wood2"] as ImageBrush;
            var brush = brush1;

            foreach (var pendulum in pendulums)
            {
                var clone = new Pendulum();
                clone.Init(pendulum.E0, pendulum.Q10, pendulum.L10);

                var pendulator = new Pendulator(clone);
                pendulators.Add(pendulator);

                brush = brush == brush2 ? brush1 : brush2;
                var model = new PendulumModel(brush, brush2) { Position = new Point3D(eps, eps, eps - 1) };
                model.Update(clone.Q1, clone.Q2);
                scene3D.Models.Add(model);

                eps += 0.001;
            }

            scene3D.Camera.Position = new Point3D(5, 0, 2);
            scene3D.Camera.LookAtOrigin();

            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;

            tb.Text = pendulums[0].dT.ToStringInv("e3");

            SwitchMode("2D");
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }

        private void SetSizeAndPosition()
        {
            var mainWindow = Application.Current.MainWindow;
            Top = mainWindow.Top;
            Height = mainWindow.Height;
            Width = Height - 30;
            Left = mainWindow.Left + (mainWindow.Width - Width) * 0.5;
        }

        private void OnMouseUpDt(object sender, MouseButtonEventArgs e)
        {
            var dt = double.Parse(tb.Text, CultureInfo.InvariantCulture);
            dt *= e.ChangedButton == MouseButton.Left ? 2 : 0.5;
            tb.Text = dt.ToStringInv("e3");

            foreach (var pendulator in pendulators)
            {
                var pendulum = pendulator.Pendulum;
                pendulum.dT = dt;
            }
        }

        private void OnButtonInit(object sender, RoutedEventArgs e)
        {
            var dt = double.Parse(tb.Text, CultureInfo.InvariantCulture);

            foreach (var pendulator in pendulators)
            {
                var pendulum = pendulator.Pendulum;
                pendulum.Init(pendulum.E0, pendulum.Q10, pendulum.L10);
                pendulum.dT = dt;
            }

            Update();
        }

        private void Update()
        {
            helper.InvalidateVisual();

            for (int i = 0; i < pendulators.Count; i++)
            {
                var pendulum = pendulators[i].Pendulum;
                var model = scene3D.Models[i] as PendulumModel;
                model.Update(pendulum.Q1, pendulum.Q2);
            }
        }

        private void OnButton2D3D(object sender, RoutedEventArgs e)
        {
            var mode = (sender as Button).Content as string;
            SwitchMode(mode);
        }

        private void OnButtonStartStop(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                btnStartStop.Content = "Start";

                foreach (var pendulator in pendulators)
                    pendulator.Stop();
            }
            else
            {
                foreach (var pendulator in pendulators)
                    pendulator.Start();

                timer.Start();
                btnStartStop.Content = "Stop";
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        private void SwitchMode(string mode)
        {
            if (mode == "2D")
            {
                scene3D.Visibility = Visibility.Hidden;
                scene2D.Visibility = Visibility.Visible;
                btn2D3D.Content = "3D";
            }
            else
            {
                scene3D.Visibility = Visibility.Visible;
                scene2D.Visibility = Visibility.Hidden;
                btn2D3D.Content = "2D";
            }
        }

        private class Helper : FrameworkElement
        {
            private readonly Color[] colors = new[] { Colors.Orange, Colors.SkyBlue, Colors.Turquoise, Colors.LawnGreen };
            private readonly List<Pendulator> pendulators;
            private double length;
            private Point center;

            public Helper(List<Pendulator> pendulators)
            {
                this.pendulators = pendulators;
            }

            protected override void OnRender(DrawingContext dc)
            {
                length = Math.Min(ActualWidth, ActualHeight) * 0.2;
                center = new Point(ActualWidth * 0.5, ActualHeight * 0.5);

                for (int i = 0; i < pendulators.Count; i++)
                {
                    var color = colors[i % colors.Length];
                    DrawPendulum(dc, pendulators[i].Pendulum, color);
                }
            }

            private void DrawPendulum(DrawingContext dc, Pendulum pendulum, Color color)
            {
                var brush = new SolidColorBrush(color);
                brush.Opacity = 0.6;
                var pen = new Pen(brush, 12);

                Point p1 = center;
                p1.X += length * Math.Sin(pendulum.Q1);
                p1.Y += length * Math.Cos(pendulum.Q1);

                Point p2 = p1;
                p2.X += length * Math.Sin(pendulum.Q2);
                p2.Y += length * Math.Cos(pendulum.Q2);

                dc.DrawLine(pen, center, p1);
                dc.DrawLine(pen, p1, p2);
            }
        }
    }
}

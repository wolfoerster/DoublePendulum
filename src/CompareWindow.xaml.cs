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
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;
    using PendulumModel = WFTools3D.Pendulum;

    /// <summary>
    /// Interaction logic for CompareWindow.xaml
    /// </summary>
    public partial class CompareWindow : Window
    {
        private readonly List<Pendulator> pendulators = new List<Pendulator>();
        private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);

        public CompareWindow(List<Pendulum> pendulums)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var eps = 0.0;
            var brush1 = Resources["wood1"] as ImageBrush;
            var brush2 = Resources["wood2"] as ImageBrush;

            foreach (var pendulum in pendulums)
            {
                var clone = new Pendulum();
                clone.Init(pendulum.E0, pendulum.Q10, pendulum.L10);

                var pendulator = new Pendulator(clone);
                var model = new PendulumModel(brush1, brush2) { Position = new Point3D(eps, eps, -1) };
                model.Update(clone.Q1, clone.Q2);

                pendulators.Add(pendulator);
                scene.Models.Add(model);

                eps += 0.001;
            }

            scene.Camera.Position = new Point3D(5, 0, 2);
            scene.Camera.LookAtOrigin();

            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;

            tb.Text = pendulums[0].dT.ToString("e3");
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }

        private void OnMouseUpDt(object sender, MouseButtonEventArgs e)
        {
            var dt = double.Parse(tb.Text);
            dt *= e.ChangedButton == MouseButton.Left ? 2 : 0.5;
            tb.Text = dt.ToString("e3");
            foreach (var pendulator in pendulators)
            {
                var pendulum = pendulator.Pendulum;
                pendulum.dT = dt;
            }
        }

        private void OnButtonInit(object sender, RoutedEventArgs e)
        {
            var dt = double.Parse(tb.Text);
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
            for (int i = 0; i < pendulators.Count; i++)
            {
                var pendulum = pendulators[i].Pendulum;
                var model = scene.Models[i] as PendulumModel;
                model.Update(pendulum.Q1, pendulum.Q2);
            }
        }

        private void OnButtonStart(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();

                foreach (var pendulator in pendulators)
                    pendulator.Stop();

                return;
            }

            foreach (var pendulator in pendulators)
                pendulator.Start();

            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Update();
        }
    }
}

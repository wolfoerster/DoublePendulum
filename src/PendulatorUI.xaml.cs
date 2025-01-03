﻿//******************************************************************************************
// Copyright © 2016 - 2024 Wolfgang Foerster (wolfoerster@gmx.de)
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
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using WFTools3D;

    /// <summary>
    /// Interaction logic for PendulatorControl.xaml
    /// </summary>
    public partial class PendulatorUI : UserControl
    {
        private readonly Brush oldBrush;
        private readonly Brush redBrush;
        private readonly Brush grnBrush;
        private readonly Action<PendulatorUI, string> callback;

        public PendulatorUI(Pendulator pendulator, Action<PendulatorUI, string> callback)
        {
            InitializeComponent();
            Pendulator = pendulator;
            Pendulator.Finished = Pendulator_Finished;

            Update();
            this.callback = callback;

            redBrush = new SolidColorBrush(Color.FromRgb(255, 100, 100)) { Opacity = 0.99 };
            redBrush.Freeze();

            grnBrush = new SolidColorBrush(Color.FromRgb(100, 255, 100)) { Opacity = 0.99 };
            grnBrush.Freeze();

            oldBrush = btnStartStop.Background;
        }

        private void ChangeId(object sender, MouseButtonEventArgs e)
        {
            NotifyCaller();
        }

        public Pendulator Pendulator { get; }

        public Pendulum Pendulum => Pendulator.Pendulum;

        public void Start()
        {
#if false
            not clear what this is supposed to do!
            anyway it leads to a different start position when starting a new simulation from the Poincare2D with ctrl button down
            if (WFUtils.IsCtrlDown())
            {
                var pendulum = Pendulator.Pendulum;
                var dT = pendulum.dT;
                pendulum.Init(pendulum.Q10, pendulum.Q20, pendulum.W10, pendulum.W20);
                pendulum.dT = dT;
                NotifyCaller("OnCheckBoxClicked");
            }
#endif
            Pendulator.Start();
            btnStartStop.Content = "Stop";
            btnStartStop.ToolTip = "Stop simulation";
            btnStartStop.Background = redBrush;
            btnSave.Background = grnBrush;
            btnSave.IsEnabled = false;
            NotifyCaller();
        }

        public void Stop()
        {
            Pendulator.Stop();
            btnStartStop.Content = "Start";
            btnStartStop.ToolTip = "Start/continue simulation";
            btnStartStop.Background = oldBrush;
            NotifyCaller();
        }

        public void Update()
        {
            cbH.IsChecked = Pendulum.IsHighlighted;
            cbM.IsChecked = Pendulum.IsMuted;
            cbS.IsChecked = Pendulum.IsSoloed;
            tbId.Text = $"{Pendulum.Id:D3}";
            tbdE.Text = $"dE: {Pendulum.dE.ToStringInv("f3")}%";
            tbdT.Text = $"dT: {Pendulum.dT.ToStringInv("e3")}";
            tbCount.Text = $"#Pts: {Pendulum.PoincarePoints.Count}";
            cbColor.SelectedColor = Pendulum.PoincareColor;

            var elapsed = TimeSpan.FromSeconds(Pendulum.SimulationTime);
            tbT.Text = elapsed.ToString("hh\\:mm\\:ss");

            // stop simulation after 4 minutes
            if (Pendulator.IsBusy && Pendulator.Elapsed >= 240)
            {
                Stop();
            }
        }

        private void NotifyCaller([CallerMemberName] string methodName = null)
        {
            callback?.Invoke(this, methodName);
        }

        private void OnMouseUpDt(object sender, MouseButtonEventArgs e)
        {
            Pendulum.dT *= e.ChangedButton == MouseButton.Left ? 2 : 0.5;
            Update();
            NotifyCaller();
        }

        private void OnCheckBoxClicked(object sender, RoutedEventArgs e)
        {
            var who = (sender as CheckBox).Content as string;

            if (who == "H")
            {
                Pendulum.IsHighlighted ^= true;
            }
            else if (who == "M")
            {
                Pendulum.IsMuted ^= true;
                if (Pendulum.IsMuted)
                    Pendulum.IsSoloed = false;
            }
            else
            {
                Pendulum.IsSoloed ^= true;
                if (Pendulum.IsSoloed)
                    Pendulum.IsMuted = false;
            }

            Update();
            NotifyCaller();
        }

        private void Pendulator_Finished()
        {
            btnSave.IsEnabled = true;
        }

        private void OnButtonSave(object sender, RoutedEventArgs e)
        {
            btnSave.Background = oldBrush;
            NotifyCaller();
        }

        private void ColorChanged(object sender, SelectionChangedEventArgs e)
        {
            Pendulum.PoincareColor = cbColor.SelectedColor;
            NotifyCaller();
        }

        private void OnButtonStartStop(object sender, RoutedEventArgs e)
        {
            if (Pendulator.IsBusy)
                Stop();
            else
                Start();
        }
    }
}

//******************************************************************************************
// Copyright © 2016 Wolfgang Foerster (wolfoerster@gmx.de)
//
// This file is part of the EquationOfTime project which can be found on github.com
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
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using WFTools3D;

namespace DoublePendulum
{
	/// <summary>
	/// Interaction logic for MainView.xaml
	/// </summary>
	public partial class MainView : UserControl
	{
		public MainView()
		{
			InitializeComponent();

			simulator = new Simulator();
			simulator.Data.Init(-0.41, -0.842, 0, 0);

			simulator.NewPoincarePoint += NewPoincarePoint;
			UpdateText();

			pendulum3d = new Pendulum3D(GetBrush(1), GetBrush(2));
			pendulum3d.Position = new Point3D(0, 0, -1.5);
			pendulum3d.Data = simulator.Data;
			pendulum3d.Update();

			scene.Models.Children.Add(pendulum3d);
			scene.Camera.Position = new Point3D(4, -2, 3);
			scene.Camera.LookAtOrigin();
			FocusManager.SetFocusedElement(this, scene);

			pendulum2d.Data = simulator.Data;
			pendulum2d.UserDragged += UserDragged;

			poincare2d.Data = simulator.Data;
			poincare2d.MouseRightButtonUp += Poincare2DMouseRightButtonUp;

			colorPicker.MouseLeftButtonUp += ColorPickerMouseLeftButtonUp;

			timer = new DispatcherTimer(DispatcherPriority.Render);
			timer.Interval = TimeSpan.FromMilliseconds(30);
			timer.Tick += TimerTick;
		}
		Simulator simulator;
		Pendulum3D pendulum3d;
		DispatcherTimer timer;

		void TimerTick(object sender, EventArgs e)
		{
			pendulum2d.Update();
			pendulum3d.Update();
			if (++count % 33 == 0)
				UpdateText();
		}
		ulong count;

		void UserDragged(object sender, EventArgs e)
		{
			poincare2d.Clear();
			pendulum3d.Update();
			UpdateText();
		}

		void UpdateText()
		{
			textBox1.Text = string.Format("E0 = {0:G3}", simulator.Data.E0);
			textBox2.Text = string.Format("dE = {0:F3}", simulator.Data.dE);
		}

		ImageBrush GetBrush(int i)
		{
			return Resources["wood" + i.ToString()] as ImageBrush;
		}

		void OnStartStopClicked(object sender, RoutedEventArgs e)
		{
			if (simulator.IsBusy)
				Stop();
			else
				Start();
		}

		void Stop()
		{
			timer.Stop();
			simulator.Stop();
			pendulum2d.IsBusy = false;
			startStopButton.Content = "Start";
		}

		void Start()
		{
			pendulum2d.IsBusy = true;
			startStopButton.Content = "Stop";
			timer.Start();
			simulator.Start();
		}

		void NewPoincarePoint(object sender, EventArgs e)
		{
			pendulum2d.NewPoincarePoint();
			poincare2d.NewPoincarePoint();
		}

		void Poincare2DMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (simulator.IsBusy)
			{
				Stop();
				return;
			}

			if (Helpers.IsCtrlDown())
			{
				simulator.Data.Points.Clear();
				poincare2d.Redraw();
				return;
			}

			if (simulator.Data.Points.Count > 0)
				poincare2d.PushData();

			Point pt = poincare2d.GetCoordinates(e.GetPosition(poincare2d));
			if (simulator.Data.Init(pt.X, pt.Y))
				Start();
		}

		void ColorPickerMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			simulator.Data.Color = (sender as ColorPicker).Color;
			poincare2d.Redraw();
		}

		void OnGravityClicked(object sender, RoutedEventArgs e)
		{
			simulator.Data.Gravity = !simulator.Data.Gravity;
		}

		void OnOmegaClicked(object sender, RoutedEventArgs e)
		{
			pendulum2d.ShowOmegas = !pendulum2d.ShowOmegas;
			pendulum2d.Update();
		}

		void OnSlowDownClicked(object sender, RoutedEventArgs e)
		{
			simulator.Data.dT *= 0.5;
		}

		void OnSpeedUpClicked(object sender, RoutedEventArgs e)
		{
			simulator.Data.dT *= 2.0;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Key == Key.Enter)
				OnStartStopClicked(null, null);
		}
	}
}

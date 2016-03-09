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

			pendelator = new Simulator();
			pendelator.Data.Init(-0.4, -0.7, 0, 0);
			pendelator.NewPoincarePoint += NewPoincarePoint;
			textBox.Text = string.Format("E0 = {0:G3}", Data.E0);

			pendulum3d = new Pendulum3D(GetBrush(1), GetBrush(2));
			pendulum3d.Position = new Point3D(0, 0, -1.5);
			pendulum3d.Data = Data;
			pendulum3d.Update();

			scene.Models.Children.Add(pendulum3d);
			scene.Camera.Position = new Point3D(4, -2, 3);
			scene.Camera.LookAtOrigin();
			FocusManager.SetFocusedElement(this, scene);

			pendulum2d.Data = Data;
			pendulum2d.UserDragged += UserDragged;

			poincare2d.Data = Data;
			poincare2d.MouseLeftButtonUp += Poincare2DMouseLeftButtonUp;

			timer = new DispatcherTimer(DispatcherPriority.Render);
			timer.Interval = TimeSpan.FromMilliseconds(30);
			timer.Tick += TimerTick;
		}
		Simulator pendelator;
		Pendulum3D pendulum3d;
		DispatcherTimer timer;

		DataModel Data
		{
			get { return pendelator.Data; }
		}

		void TimerTick(object sender, EventArgs e)
		{
			pendulum2d.Update();
			pendulum3d.Update();
		}

		void UserDragged(object sender, EventArgs e)
		{
			poincare2d.Clear();
			pendulum3d.Update();
			textBox.Text = string.Format("E0 = {0:G3}", Data.E0);
		}

		ImageBrush GetBrush(int i)
		{
			return Resources["wood" + i.ToString()] as ImageBrush;
		}

		void OnStartStopClicked(object sender, RoutedEventArgs e)
		{
			if (pendelator.IsBusy)
				Stop();
			else
				Start();
		}

		void Stop()
		{
			timer.Stop();
			pendelator.Stop();
			pendulum2d.IsBusy = false;
			startStopButton.Content = "Start";
		}

		void Start()
		{
			timer.Start();
			pendulum2d.IsBusy = true;
			startStopButton.Content = "Stop";
			pendelator.Start();
		}

		void NewPoincarePoint(object sender, EventArgs e)
		{
			pendulum2d.NewPoincarePoint();
			poincare2d.NewPoincarePoint();
		}

		void Poincare2DMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (pendelator.IsBusy)
			{
				Stop();
				return;
			}

			if (Data.Points.Count > 0)
				poincare2d.PushData();

			Point pt = poincare2d.GetCoordinates(e.GetPosition(poincare2d));
			if (Data.Init(pt.X, pt.Y))
				Start();
		}

		void OnGravityClicked(object sender, RoutedEventArgs e)
		{
			Data.Gravity = !Data.Gravity;
		}

		void OnOmegaClicked(object sender, RoutedEventArgs e)
		{
			pendulum2d.ShowOmegas = !pendulum2d.ShowOmegas;
			pendulum2d.Update();
		}

		void OnSlowDownClicked(object sender, RoutedEventArgs e)
		{
			Data.dT *= 0.5;
		}

		void OnSpeedUpClicked(object sender, RoutedEventArgs e)
		{
			Data.dT *= 2.0;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Key == Key.Enter)
				OnStartStopClicked(null, null);
		}
	}
}

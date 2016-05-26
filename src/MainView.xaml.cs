//******************************************************************************************
// Copyright © 2016 Wolfgang Foerster (wolfoerster@gmx.de)
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
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Windows.Controls.Primitives;
using System.Globalization;
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

			pendulum3d = new PendulumModel3D(GetBrush(1), GetBrush(2));
			pendulum3d.Position = new Point3D(0, 0, -1.5);
			pendulum3d.Data = simulator.Data;
			pendulum3d.Update();
			scene.Models.Add(pendulum3d);

			xyPlane = new Disk { ScaleX = 2, ScaleY = 2 };
			xyPlane.EmissiveMaterial.Brush = xyPlane.SpecularMaterial.Brush = null;
			Brush brush = new SolidColorBrush(Color.FromArgb(160, 0, 22, 22));
			xyPlane.DiffuseMaterial.Brush = brush;
			xyPlane.BackMaterial = xyPlane.Material;

			trajectory = new Trajectory(simulator.Data);

			scene.Camera.Position = new Point3D(4, -2, 3);
			scene.Camera.LookAtOrigin();
			scene.Camera.Scale = 0.1;
			FocusManager.SetFocusedElement(this, scene);

			pendulum2d.Data = simulator.Data;
			pendulum2d.UserDragged += UserDragged;

			poincare2d.Data = simulator.Data;
			poincare2d.MouseRightButtonUp += Poincare2DMouseRightButtonUp;

			colorPicker.MouseLeftButtonUp += ColorPickerMouseLeftButtonUp;

			timer = new DispatcherTimer(DispatcherPriority.Render);
			timer.Interval = TimeSpan.FromMilliseconds(30);
			timer.Tick += TimerTick;

			InitializeDirectories();
		}
		Simulator simulator;
		PendulumModel3D pendulum3d;
		Trajectory trajectory;
		Primitive3D xyPlane;
		DispatcherTimer timer;

		void TimerTick(object sender, EventArgs e)
		{
			pendulum2d.Update();
			pendulum3d.Update();
			trajectory.Update();

			string msg = checker.GetResult(simulator.Count * 1000);
			DateTime t1 = DateTime.Now;
			if ((t1 - t0).TotalSeconds > 1)
			{
				t0 = t1;
				UpdateText();
				Application.Current.MainWindow.Title = string.Format("Double Pendulum ({0})", msg);
			}
		}
		DateTime t0;
		PerformanceChecker checker = new PerformanceChecker();

		void UserDragged(object sender, EventArgs e)
		{
			trajectory.Clear();
			poincare2d.Clear();
			pendulum3d.Update();
			energyList.SelectedIndex = -1;

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

		void OnViewClicked(object sender, RoutedEventArgs e)
		{
			tb0.IsChecked = tb1.IsChecked = tb2.IsChecked = tb3.IsChecked = tb4.IsChecked = false;
			ToggleButton tb = (sender as ToggleButton);
			tb.IsChecked = true;

			scene.Models.Clear();
			if (tb.Name == "tb0")
			{
				trajectory.Mode = 0;
				scene.Models.Add(pendulum3d);
			}
			else
			{
				scene.Models.Add(new AxisModel(1));
				scene.Models.Add(trajectory);
				scene.Models.Add(xyPlane);
				trajectory.Mode = int.Parse(tb.Name.Substring(2));
			}
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
			checker.Reset();
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

			if (WFUtils.IsCtrlDown())
			{
				simulator.Data.PoincarePoints.Clear();
				poincare2d.Redraw();
				return;
			}

			if (simulator.Data.PoincarePoints.Count > 0)
				poincare2d.CloneData();

			Point pt = poincare2d.PixelToData(e.GetPosition(poincare2d));
			if (simulator.Data.Init(pt.X, pt.Y))
			{
				trajectory.Clear();
				Start();
			}
		}

		void ColorPickerMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			simulator.Data.Color = (sender as ColorPicker).Color;
			poincare2d.Redraw();
		}

		void OnReflectClicked(object sender, RoutedEventArgs e)
		{
			poincare2d.DoReflect = (sender as ToggleButton).IsChecked == true;
			poincare2d.Redraw();
		}

		void OnHighlightClicked(object sender, RoutedEventArgs e)
		{
			poincare2d.DoHighlight = (sender as ToggleButton).IsChecked == true;
			poincare2d.Redraw();
		}

		void OnGravityClicked(object sender, RoutedEventArgs e)
		{
			simulator.Data.Gravity = !simulator.Data.Gravity;
			simulator.Data.E0 = simulator.Data.CalculateEnergy();
			UpdateText();
		}

		void OnVelosClicked(object sender, RoutedEventArgs e)
		{
			pendulum2d.ShowVelos = !pendulum2d.ShowVelos;
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

		#region Directories

		void InitializeDirectories()
		{
			baseDirectory = AppDomain.CurrentDomain.BaseDirectory + "Data\\"; 
			if (!Directory.Exists(baseDirectory))
				return;

			string[] names = Directory.GetDirectories(baseDirectory);
			if (names.Length == 0)
				return;

			List<string> directories = new List<string>();
			foreach (var name in names)
				directories.Add(name.Substring(name.LastIndexOf("\\") + 1));

			directories.Sort((string s1, string s2) => 
			{
				double d1 = double.Parse(s1, CultureInfo.InvariantCulture);
				double d2 = double.Parse(s2, CultureInfo.InvariantCulture);
				return d1.CompareTo(d2);
			});

			energyList.ItemsSource = directories;
		}
		string baseDirectory;

		void EnergyListSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (energyList.SelectedItem != null)
			{
				Dispatcher.BeginInvoke((Action)(() =>
					ShowDirectory(baseDirectory + energyList.SelectedItem)), DispatcherPriority.Background);
			}
		}

		void ShowDirectory(string dir)
		{
			if (simulator.IsBusy)
				Stop();

			bool firstTime = true;
			string[] names = Directory.GetFiles(dir, "e*.*");
			foreach (string name in names)
			{
				if (simulator.Data.Read(name))
				{
					if (firstTime)
					{
						UpdateText();
						firstTime = false;
						poincare2d.Clear();
					}
					poincare2d.CloneData();
				}
			}

			poincare2d.Redraw();
			trajectory.Clear();
		}

		#endregion Directories
	}
}

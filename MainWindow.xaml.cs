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
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;

namespace DoublePendulum
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			#region Initial size and position

			Loaded += MeLoaded;
			Closing += MeClosing;

			Top = Properties.Settings.Default.Top;
			Left = Properties.Settings.Default.Left;
			Width = Properties.Settings.Default.Width;
			Height = Properties.Settings.Default.Height;

			doMaximize = false;
			WindowState = WindowState.Normal;

			Screen screen = Utils.GetScreenByName(Properties.Settings.Default.ScreenName);
			if (screen == null)
			{
				screen = Utils.GetPrimaryScreen();
				Top = screen.WorkArea.Top;
				Left = screen.WorkArea.Left + 90;
				Width = screen.WorkArea.Width - 250;
				Height = screen.WorkArea.Height;
			}
			else
			{
				doMaximize = Properties.Settings.Default.WindowState == 2;
				if (doMaximize)
				{
					Top = screen.WorkArea.Top + 1;
					Left = screen.WorkArea.Left + 1;
					Width = screen.WorkArea.Width - 2;
					Height = screen.WorkArea.Height - 2;
				}
			}

			#endregion Initial size and position
		}
		bool doMaximize;

		void MeLoaded(object sender, RoutedEventArgs e)
		{
			if (doMaximize)
				WindowState = WindowState.Maximized;
		}

		void MeClosing(object sender, CancelEventArgs e)
		{
			Properties.Settings.Default.Top = Top;
			Properties.Settings.Default.Left = Left;
			Properties.Settings.Default.Width = Width;
			Properties.Settings.Default.Height = Height;
			Properties.Settings.Default.WindowState = (int)WindowState;
			Properties.Settings.Default.ScreenName = Utils.GetScreenByPixel(Left, Top).Name;
			Properties.Settings.Default.Save();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Key == Key.Escape)
				Close();
		}
	}
}

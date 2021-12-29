//******************************************************************************************
// Copyright © 2016 - 2022 Wolfgang Foerster (wolfoerster@gmx.de)
//
// This file is part of the DoublePendulum project which can be found on github.com.
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
    using System.Windows;
    using System.Windows.Input;
    using System.ComponentModel;
    using WFTools3D;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MeLoaded;
            Closing += MeClosing;
            RestoreSizeAndPosition();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
                Close();
        }

        private void MeLoaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.IsMaximized)
                this.WindowState = WindowState.Maximized;
        }

        private void MeClosing(object sender, CancelEventArgs e)
        {
            if (controlCenter.IsBusy)
            {
                controlCenter.Shutdown();
                e.Cancel = true;
            }

            StoreSizeAndPosition();
        }

        private void RestoreSizeAndPosition()
        {
            var name = Properties.Settings.Default.ScreenName;
            var screen = Screen.LookUpByName(name);
            if (screen == null)
            {
                this.Top = 0;
                this.Width = 1415;
                this.Height = 1050;
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                return;
            }

            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            this.Width = Properties.Settings.Default.Width;
            this.Height = Properties.Settings.Default.Height;
            this.WindowState = (WindowState)Properties.Settings.Default.WindowState;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private void StoreSizeAndPosition()
        {
            Properties.Settings.Default.IsMaximized = this.WindowState == WindowState.Maximized;

            if (this.WindowState != WindowState.Normal)
                this.WindowState = WindowState.Normal;

            var pt = new Point(this.Left, this.Top);
            var screen = Screen.LookUpByPixel(pt.ToPixel(this));
            Properties.Settings.Default.ScreenName = screen?.Name;

            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Width = this.Width;
            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Save();
        }
    }
}

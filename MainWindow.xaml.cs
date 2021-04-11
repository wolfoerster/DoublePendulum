//******************************************************************************************
// Copyright © 2016 - 2021 Wolfgang Foerster (wolfoerster@gmx.de)
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
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using WFTools3D;

namespace DoublePendulum
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool doMaximize;

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
            if (doMaximize) WindowState = WindowState.Maximized;
        }

        private void MeClosing(object sender, CancelEventArgs e)
        {
            StoreSizeAndPosition();
        }

        private void RestoreSizeAndPosition()
        {
            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            this.Width = Properties.Settings.Default.Width;
            this.Height = Properties.Settings.Default.Height;

            this.doMaximize = false;
            this.WindowState = WindowState.Normal;

            var name = Properties.Settings.Default.ScreenName;
            var screen = WFUtils.GetScreenByName(name);

            if (screen == null)
            {
                var leftMargin = 80;
                var rightMargin = 150;

                screen = WFUtils.GetPrimaryScreen();
                this.Top = screen.WorkArea.Top;
                this.Left = screen.WorkArea.Left + leftMargin;
                this.Width = screen.WorkArea.Width - leftMargin - rightMargin;
                this.Height = screen.WorkArea.Height;
            }
            else
            {
                this.doMaximize = Properties.Settings.Default.WindowState == 2;
            }
        }

        private void StoreSizeAndPosition()
        {
            Properties.Settings.Default.WindowState = (int)this.WindowState;

            if (this.WindowState != WindowState.Normal)
                this.WindowState = WindowState.Normal;

            var screen = WFUtils.GetScreenByPixel(this.Left, this.Top);
            Properties.Settings.Default.ScreenName = screen?.Name;

            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Width = this.Width;
            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Save();
        }
    }
}

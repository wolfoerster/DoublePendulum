﻿//******************************************************************************************
// Copyright © 2016 - 2024 Wolfgang Foerster (wolfoerster@gmx.de)
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
    using System;

    public partial class MainWindow : Window
    {
        private readonly Properties.Settings settings = Properties.Settings.Default;

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
            if (settings.IsMaximized)
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
            var name = settings.ScreenName;
            var screen = Screen.LookUpByName(name);

            if (screen == null || Keyboard.IsKeyToggled(Key.CapsLock))
            {
                screen = Screen.LookUpPrimary();
                var area = screen.WorkArea;
                var topLeft = area.TopLeft.ToDip(this);
                var bottomRight = area.BottomRight.ToDip(this);
                var width = bottomRight.X - topLeft.X;
                var height = bottomRight.Y - topLeft.Y;

                this.Top = topLeft.Y;
                this.Height = height;
                this.Width = Math.Min(width, height + 370);
                this.Left = (width - this.Width) * 0.5;
            }
            else
            {
                this.Top = settings.Top;
                this.Left = settings.Left;
                this.Width = settings.Width;
                this.Height = settings.Height;
            }

            this.WindowState = WindowState.Normal;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private void StoreSizeAndPosition()
        {
            settings.IsMaximized = this.WindowState == WindowState.Maximized;

            if (this.WindowState != WindowState.Normal)
                this.WindowState = WindowState.Normal;

            var pt = new Point(this.Left, this.Top);
            var screen = Screen.LookUpByPixel(pt.ToPixel(this));
            settings.ScreenName = screen?.Name;

            settings.Top = this.Top;
            settings.Left = this.Left;
            settings.Width = this.Width;
            settings.Height = this.Height;

            settings.MirrorQ = ControlCenter.MirrorQ;
            settings.MirrorL = ControlCenter.MirrorL;

            settings.Save();
        }
    }
}

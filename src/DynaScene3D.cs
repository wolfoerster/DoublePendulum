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
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using WFTools3D;

    public class DynaScene3D : Scene3D
    {
        private int mode;
        private Vector mouseMove;
        private Point prevPosition;
        private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);

        public DynaScene3D()
        {
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            timer.Stop();
            mode = 0;
            timer.Start();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            timer.Stop();

            if (mouseMove.X != 0 || mouseMove.Y != 0)
            {
                double dx = Math.Abs(mouseMove.X);
                double dy = Math.Abs(mouseMove.Y);
                mode = dx > 2 * dy ? 1 : dy > 2 * dx ? 2 : 3;
                mouseMove *= 0.2;
                timer.Start();
            }
            else
            {
                mouseMove = new Vector(0, 0);
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            timer.Stop();

            var dx = 0.4;
            var leftClick = Mouse.GetPosition(this).X < ActualWidth * 0.5;
            mouseMove.X += leftClick ? dx : -dx;

            mode = 1;
            timer.Start();
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            switch (mode)
            {
                case 0:
                    Point position = Mouse.GetPosition(this);
                    mouseMove = prevPosition - position;
                    prevPosition = position;
                    return;

                case 1:
                    Camera.Rotate(Math3D.UnitZ, mouseMove.X);
                    return;

                case 2:
                    Camera.Rotate(Camera.RightDirection, mouseMove.Y);
                    return;

                case 3:
                    Camera.Rotate(Camera.UpDirection, (mouseMove.X + mouseMove.Y) * 0.5);
                    return;
            }
        }
    }
}

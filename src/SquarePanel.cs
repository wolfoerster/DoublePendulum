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
    using System.Windows;
    using System.Windows.Controls;

    public class SquarePanel : Panel
    {
        private Rect rect0, rect1;

        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.RegisterAttached("Location", typeof(int), typeof(SquarePanel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public static int GetLocation(DependencyObject obj)
        {
            return (int)obj.GetValue(LocationProperty);
        }

        public static void SetLocation(DependencyObject obj, int value)
        {
            obj.SetValue(LocationProperty, value);
        }

        protected override Size MeasureOverride(Size finalSize)
        {
            var length = finalSize.Width < finalSize.Height ? finalSize.Width : finalSize.Height;

            rect0 = new Rect(0, 0, length, length);

            rect1 = finalSize.Width < finalSize.Height
                ? new Rect(0, length, finalSize.Width, finalSize.Height - length)
                : new Rect(length, 0, finalSize.Width - length, finalSize.Height);

            foreach (FrameworkElement child in base.InternalChildren)
            {
                if (child != null)
                {
                    var rect = GetLocation(child) == 0 ? rect0 : rect1;
                    child.Measure(rect.Size);
                }
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (FrameworkElement child in base.InternalChildren)
            {
                if (child != null)
                {
                    var rect = GetLocation(child) == 0 ? rect0 : rect1;
                    child.Arrange(rect);
                }
            }

            return finalSize;
        }
    }
}

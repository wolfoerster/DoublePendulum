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
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Controls;
    using System.Collections.Generic;

    public class ColorBox : ComboBox
    {
        private readonly List<Color> AvailableColors = new List<Color>();

        public ColorBox()
        {
            Add(255, 255, 255);
            Add(192, 192, 192);
            Add(0, 0, 0);
            Add(255, 127, 127);
            Add(255, 0, 0);
            Add(165, 0, 0);
            Add(127, 255, 127);
            Add(0, 255, 0);
            Add(0, 165, 0);
            Add(127, 127, 255);
            Add(0, 0, 255);
            Add(0, 0, 165);
            Add(255, 255, 127);
            Add(255, 255, 0);
            Add(165, 165, 0);
            Add(255, 127, 255);
            Add(255, 0, 255);
            Add(165, 0, 165);
            Add(127, 255, 255);
            Add(0, 255, 255);
            Add(0, 165, 165);

            SelectionChanged += MeSelectionChanged;
            SelectedIndex = 0;
        }

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorBox),
                new PropertyMetadata(Colors.White, OnSelectedColorChanged));

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorBox = d as ColorBox;
            var newColor = (Color)e.NewValue;

            for (int i = 0; i < colorBox.AvailableColors.Count; i++)
            {
                if (colorBox.AvailableColors[i] == newColor)
                {
                    colorBox.SelectedIndex = i;
                    return;
                }
            }

            colorBox.Add(newColor);
            colorBox.SelectedIndex = colorBox.AvailableColors.Count - 1;
        }

        private void MeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedColor = SelectedIndex < 0 ? Colors.Transparent : AvailableColors[SelectedIndex];
        }

        private void Add(byte r, byte g, byte b)
        {
            Add(Color.FromRgb(r, g, b));
        }

        private void Add(Color color)
        {
            Rectangle rc = new Rectangle
            {
                Width = 15,
                Height = 15,
                Fill = new SolidColorBrush(color),
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };
            Items.Add(rc);
            AvailableColors.Add(color);
        }
    }
}

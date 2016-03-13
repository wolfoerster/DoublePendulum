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
using System.Windows.Media;

namespace DoublePendulum
{
	public class ColorPicker : FrameworkElement
	{
		protected override void OnRender(DrawingContext dc)
		{
			int id = 0, m = 2;

			for (int iy = 0; iy < 4; iy++)
			{
				for (int ix = 0, max = 4 / (iy + 1); ix < max; ix++)
				{
					Rect rect = new Rect(m + ix * size, m + iy * size, size - m, size - m);
					dc.DrawRectangle(brushes[id++], null, rect);
				}
			}
		}
		int size = 16;
		static SolidColorBrush[] brushes = new SolidColorBrush[] { 
			Brushes.White, Brushes.Yellow, Brushes.Orange, Brushes.Red, 
			Brushes.Cyan, Brushes.LawnGreen, 
			Brushes.CornflowerBlue, 
			Brushes.Magenta };

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			Point pt = e.GetPosition(this);
			int ix = (int)(pt.X / size);
			int iy = (int)(pt.Y / size);
			int id = iy == 0 ? ix : iy == 1 ? ix + 4 : iy + 4;
			Color = brushes[id].Color;
		}

		public Color Color;
	}
}

//******************************************************************************************
// Copyright © 2016 - 2024 Wolfgang Foerster (wolfoerster@gmx.de)
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
    using System.Diagnostics;
    using System.Windows.Media;
    using WFTools3D;

    public class ColorTransform
    {
        private LinearTransform tR, tG, tB;

        public void Init(double lowValue, double highValue, Color lowColor, Color highColor)
        {
            tR = new LinearTransform(lowValue, highValue, lowColor.R, highColor.R);
            tG = new LinearTransform(lowValue, highValue, lowColor.G, highColor.G);
            tB = new LinearTransform(lowValue, highValue, lowColor.B, highColor.B);
        }

        public Color GetColor(double t)
        {
            byte r = (byte)(tR.Transform(t) + 0.5);
            byte g = (byte)(tG.Transform(t) + 0.5);
            byte b = (byte)(tB.Transform(t) + 0.5);
            return Color.FromRgb(r, g, b);
        }
    }
}

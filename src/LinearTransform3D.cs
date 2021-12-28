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
    using System.Windows.Media.Media3D;
    using WFTools3D;

    public class LinearTransform3D
    {
        LinearTransform tx, ty, tz;

        public void Init(double xmax, double ymax, double zmax)
        {
            tx = CreateTransform(xmax);
            ty = CreateTransform(ymax);
            tz = CreateTransform(zmax);
        }

        public void Clear()
        {
            tx = ty = tz = null;
        }

        public bool IsEmpty => tx == null;

        LinearTransform CreateTransform(double dataValue)
        {
            return new LinearTransform(-dataValue, dataValue, -1, 1);
        }

        public Point3D Transform(double x, double y, double z)
        {
            return new Point3D(tx.Transform(x), ty.Transform(y), tz.Transform(z));
        }
    }
}

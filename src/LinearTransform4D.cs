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

    public class LinearTransform4D
    {
        private readonly LinearTransform3D t3d = new();
        private readonly LinearTransform ttc = new();

        public void Init(double xmax, double ymax, double zmax, double cmax)
        {
            t3d.Init(xmax, ymax, zmax);
            ttc.Init(-cmax, cmax, 0, 1);
        }

        public (Point3D Point, double TexCoord) Transform(double x, double y, double z, double c)
            => (t3d.Transform(x, y, z), ttc.Transform(c));
    }
}

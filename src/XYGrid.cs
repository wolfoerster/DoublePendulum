//******************************************************************************************
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
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using WFTools3D;

    public class XYGrid : Primitive3D
    {
        private readonly double size;

        public XYGrid(double size)
        {
            var brush = new SolidColorBrush(Color.FromRgb(223, 223, 223)) { Opacity = 0.5 };
            brush.Freeze();

            DiffuseMaterial.Brush = brush;
            EmissiveMaterial.Brush = brush;
            SpecularMaterial.Brush = null;
            BackMaterial = Material;

            this.size = size;
            InitMesh();

            Children.Add(CreateXYPlane(size + 0.01));
        }

        protected override MeshGeometry3D CreateMesh()
        {
            if (size < 1e-12)
                return null;

            var ds = 0.1;
            var th = 0.003;
            var s = -size - th * 0.5;
            var longX = Math3D.UnitX * 2 * size;
            var longY = Math3D.UnitY * 2 * size;
            var shortX = Math3D.UnitX * th;
            var shortY = Math3D.UnitY * th;
            var mesh = new MeshGeometry3D();

            while (s < size)
            {
                MeshUtils.AddTriangles(mesh, 1, new(-size, s, 0), longX, shortY, 0, 1, true);
                MeshUtils.AddTriangles(mesh, 1, new(s, -size, 0), shortX, longY, 0, 1, true);
                s += ds;
            }

            return mesh;
        }

        private static Square CreateXYPlane(double size)
        {
            var brush = new SolidColorBrush(Color.FromRgb(23, 23, 23)) { Opacity = 0.4 };
            brush.Freeze();

            var xyPlane = new Square() { ScaleX = size, ScaleY = size, Position = new(0, 0, -0.001) };
            xyPlane.DiffuseMaterial.Brush = brush;
            xyPlane.EmissiveMaterial.Brush = brush;
            xyPlane.SpecularMaterial.Brush = null;
            xyPlane.BackMaterial = xyPlane.Material;
            return xyPlane;
        }
    }
}

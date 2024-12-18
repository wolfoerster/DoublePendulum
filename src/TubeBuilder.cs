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
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using System.Collections.Generic;
    using WFTools3D;
    using System.Linq;

    public class TubeBuilder
    {
        private readonly List<Point> section = [];
        private readonly List<Point3D> path = [];
        private readonly List<Vector3D> normals = [];
        private readonly List<Point3D> positions = [];
        private readonly List<double> tcoords = [];
        private readonly List<int> indices = [];
        private Vector3D v;

        public TubeBuilder(double radius = 0.02, int divisions = 6)
        {
            //--- setup the cross section of the tube
            for (int i = 0; i < divisions; i++)
            {
                double phi = i * MathUtils.PIx2 / divisions;
                section.Add(new Point(radius * Math.Cos(phi), radius * Math.Sin(phi)));
            }
        }

        public void Clear()
        {
            path.Clear();
            normals.Clear();
            positions.Clear();
            tcoords.Clear();
            indices.Clear();
        }

        public MeshGeometry3D CreateMesh()
        {
            if (indices.Count == 0)
                return null;
#if false
            // doesn't work, because underlying lists are changed during execution
            return new MeshGeometry3D
            {
                TriangleIndices = new Int32Collection(indices),
                Positions = new Point3DCollection(positions),
                Normals = new Vector3DCollection(normals),
            };
#else
            return new MeshGeometry3D
            {
                TriangleIndices = new Int32Collection(indices.PickUp()),
                Positions = new Point3DCollection(positions.PickUp()),
                Normals = new Vector3DCollection(normals.PickUp()),
                TextureCoordinates = new PointCollection(tcoords.PickUp().Select(x => new Point(x, 0))),
            };
#endif
        }

        public void AddPoint(Point3D point, double tcoord)
        {
            tcoord = CheckTextureCoordinate(tcoord);

            path.Add(point);

            if (path.Count > 1)
            {
                // calc positions for the last but one section:
                var i = path.Count - 2;
                AddPositions(i, tcoord);

                if (path.Count > 2)
                {
                    // add triangles from section i-1 to section i
                    AddTriangles(i);
                }
            }
        }

        private void AddPositions(int i, double tcoord)
        {
            var prev = i == 0 ? path[i] : path[i - 1];
            var next = path[i + 1];
            var diff = next - prev;

            if (i == 0)
            {
                var startDirection = path[1] - path[0];
                v = startDirection.FindAnyPerpendicular();
            }

            var u = v.Cross(diff);
            u.Normalize();

            v = diff.Cross(u);
            v.Normalize();

            // add positions and normals for section i:
            for (int j = 0; j < section.Count; j++)
            {
                var n = section[j].X * u + section[j].Y * v;
                positions.Add(path[i] + n);
                tcoords.Add(tcoord);

                n.Normalize();
                normals.Add(n);
            }
        }

        private void AddTriangles(int i)
        {
            for (int j = 1; j <= section.Count; j++)
            {
                var i11 = i * section.Count + j;
                var i10 = i11 - 1;
                int i01 = i11 - section.Count;
                int i00 = i01 - 1;

                if (j == section.Count)
                {
                    i11 -= section.Count;
                    i01 -= section.Count;
                }

                AddTriangleIndices(i00, i01, i11);
                AddTriangleIndices(i11, i10, i00);
            }
        }

        private void AddTriangleIndices(int i, int j, int k)
        {
            indices.Add(i);
            indices.Add(j);
            indices.Add(k);
        }

        private static double CheckTextureCoordinate(double tcoord)
        {
            var eps = 1e-12;

            if (tcoord < -eps || tcoord > 1 + eps)
                throw new ArgumentOutOfRangeException(nameof(tcoord));

            return Math.Max(0, Math.Min(1, tcoord));
        }
    }
}

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
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using WFTools3D;

    public class TubeBuilder
    {
        private readonly List<Point> section = [];
        private readonly List<Point3D> path = [];
        private readonly List<Vector3D> normals = [];
        private readonly List<Point3D> positions = [];
        private readonly List<double> tcoords = [];
        private readonly List<int> indices = [];
        private int segmentStart = 0;
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
            segmentStart = 0;
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
            if (path.Count > 0)
            {
                var dist = (path[^1] - point).Length;
                if (dist < 1e-2) // no need to take every point
                    return;

                if (dist > 1.5) // a rotation occurred (angle crossed ± 180°)
                {
                    // finish the current segment
                    var i = path.Count - 1;
                    AddPositions(i, tcoord, AddPositionsMode.Last);
                    AddTriangles(i);
                    // and store the begin of a new one
                    segmentStart = path.Count;
                }
            }

            path.Add(point);

            var numPoints = path.Count - segmentStart;
            if (numPoints > 1)
            {
                if (numPoints == 2)
                {
                    // find a normal for the initial direction
                    var startDirection = path[segmentStart + 1] - path[segmentStart];
                    v = startDirection.FindAnyPerpendicular();
                }

                // calc positions for the last but one section:
                var i = path.Count - 2;
                var mode = i == segmentStart ? AddPositionsMode.First : AddPositionsMode.Normal;
                AddPositions(i, tcoord, mode);

                if (numPoints > 2)
                {
                    // add triangles from section i-1 to section i
                    AddTriangles(i);
                }
            }
        }

        private void AddPositions(int i, double tcoord, AddPositionsMode mode)
        {
            var d = GetDiff(i, mode);
            var u = v.Cross(d);
            v = d.Cross(u);
            v.Normalize();
            u.Normalize();

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

        private Vector3D GetDiff(int i, AddPositionsMode mode)
        {
            if (mode == AddPositionsMode.First)
                return path[i + 1] - path[i];

            if (mode == AddPositionsMode.Last)
                return path[i] - path[i - 1];

            var v0 = path[i - 1] - path[i];
            var v1 = path[i + 1] - path[i];
            v0.Normalize();
            v1.Normalize();
            return v1 - v0;
        }

        private enum AddPositionsMode
        {
            Normal,
            First,
            Last,
        }
    }
}

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

    public class TubeBuilder
    {
        private readonly List<Point> section = [];
        private readonly List<Point3D> path = [];
        private readonly List<Vector3D> normals = [];
        private readonly List<Point3D> positions = [];
        private readonly List<int> indices = [];
        private readonly List<int> marker = [];
        private Vector3D v;

        public TubeBuilder(double radius = 0.01, int divisions = 6)
        {
            //--- setup the cross section of the tube
            for (int i = 0; i < divisions; i++)
            {
                double phi = i * MathUtils.PIx2 / divisions;
                section.Add(new Point(radius * Math.Cos(phi), radius * Math.Sin(phi)));
            }

            Clear();
        }

        public void Clear()
        {
            path.Clear();
            marker.Clear();
            indices.Clear();
            normals.Clear();
            positions.Clear();
        }

        public MeshGeometry3D CreateMesh()
        {
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
            };
#endif
        }

        public void AddPoint(Point3D point)
        {
            var newSegment = false;

            if (path.Count > 0)
            {
                var dist = (path[path.Count - 1] - point).Length;
                if (dist > 1)
                {
                    newSegment = true;
                }
            }

            AddToPath(point, newSegment);
        }

        private void AddToPath(Point3D point, bool newSegment)
        {
            if (newSegment)
            {
                // add end cap
                var i = path.Count - 1;
                AddPositions(i, false, true);
                AddTriangles(i);
                marker.Add(path.Count);
            }

            path.Add(point);

            var segStart = marker.Count > 0 ? marker[^1] : 0;
            var numPoints = path.Count - segStart;

            if (numPoints > 1)
            {
                if (numPoints == 2) // initialize v, a vector perpendicular to the segment's start direction
                {
                    var startDirection = path[segStart + 1] - path[segStart];
                    v = startDirection.FindAnyPerpendicular();
                }

                // calc positions for the last but one section:
                var i = path.Count - 2;
                AddPositions(i, i == segStart);

                if (numPoints > 2)
                {
                    // add triangles from section i-1 to section i
                    AddTriangles(i);
                }
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

        private void AddPositions(int i, bool isFirstSection = false, bool isLastSection = false)
        {
            var prev = isFirstSection ? path[i] : path[i - 1];
            var next = isLastSection ? path[i] : path[i + 1];
            var diff = next - prev;

            var u = v.Cross(diff);
            u.Normalize();

            v = diff.Cross(u);
            v.Normalize();

            // add positions and normals for section i:
            for (int j = 0; j < section.Count; j++)
            {
                var n = section[j].X * u + section[j].Y * v;
                positions.Add(path[i] + n);

                n.Normalize();
                normals.Add(n);
            }
        }

        private void AddTriangleIndices(int i, int j, int k)
        {
            indices.Add(i);
            indices.Add(j);
            indices.Add(k);
        }
    }
}

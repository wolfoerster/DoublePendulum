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
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using WFTools3D;

    public class Poincare3D : Object3D
    {
        private static readonly LinearTransform3D dataTransform = new LinearTransform3D();

        public static bool MirrorQ;

        public static bool MirrorL;

        public void Clear()
        {
            Children.Clear();
            dataTransform.Clear();
        }

        public void Redraw(Pendulum pendulum)
        {
            var child = Children.FirstOrDefault(o => (o as PointsModel).PendulumId == pendulum.Id);
            if (child != null)
                Children.Remove(child);

            AddPointsModel(pendulum);
        }

        public void Redraw()
        {
            Clear();

            if (dataTransform.IsEmpty)
            {
                var pendulum = App.Pendulums.Count > 0 ? App.Pendulums[0] : App.SelectedPendulum;
                dataTransform.Init(pendulum.Q1Max, pendulum.L1Max, pendulum.L2Max);
            }

            var soloed = false;
            foreach (var pendulum in App.Pendulums)
            {
                if (pendulum.IsSoloed)
                {
                    soloed = true;
                    AddPointsModel(pendulum);
                }
            }

            if (!soloed)
            {
                foreach (var pendulum in App.Pendulums)
                    AddPointsModel(pendulum);
            }
        }

        private void AddPointsModel(Pendulum pendulum)
        {
            if (!pendulum.IsMuted)
            {
                var model = new PointsModel(pendulum);
                Children.Add(model);
            }
        }

        public void NewPoincarePoint(Pendulum pendulum)
        {
            if (!pendulum.IsMuted)
            {
                Redraw(pendulum);
            }
        }

        private class PointsModel : Primitive3D
        {
            private readonly Pendulum pendulum;
            private Point3DCollection positions;
            private Int32Collection trindices;
            private int trindex;

            public PointsModel(Pendulum pendulum)
            {
                this.pendulum = pendulum;
                InitMesh();

                DiffuseMaterial.Brush = new SolidColorBrush(pendulum.PoincareColor);
                DiffuseMaterial.Brush.Freeze();
                DiffuseMaterial.Freeze();

                DiffuseMaterial material = new DiffuseMaterial(Brushes.Gray);
                BackMaterial = new MaterialGroup();
                BackMaterial.Children.Add(material);
                BackMaterial.Freeze();
            }

            public int PendulumId => pendulum.Id;

            protected override MeshGeometry3D CreateMesh()
            {
                if (pendulum == null || pendulum.PoincarePoints.Count == 0)
                    return null;

                int nPositions = 12 * pendulum.PoincarePoints.Count;
                trindices = new Int32Collection(nPositions);
                positions = new Point3DCollection(nPositions);
                trindex = 0;

                // do not use foreach because pendulum.PoincarePoints might change during loop execution
                var nPoints = pendulum.PoincarePoints.Count;

                for (int n = 0; n < nPoints; n++)
                {
                    var pt = pendulum.PoincarePoints[n];
                    AddTriangles(pt.Q1, pt.L1, pt.L2);

                    if (MirrorQ)
                    {
                        AddTriangles(-pt.Q1, pt.L1, pt.L2);

                        if (MirrorL)
                        {
                            AddTriangles(pt.Q1, -pt.L1, -pt.L2);
                            AddTriangles(-pt.Q1, -pt.L1, -pt.L2);
                        }
                    }
                    else if (MirrorL)
                    {
                        AddTriangles(pt.Q1, -pt.L1, -pt.L2);
                    }
                }

                return new MeshGeometry3D
                {
                    TriangleIndices = trindices,
                    Positions = positions
                };
            }

            private void AddTriangles(double x, double y, double z)
            {
                bool doSquare = false;
                Point3D p0 = dataTransform.Transform(x, y, z);
                GetPoints(ref p0, out var p1, out var p2, out var p3, doSquare);

                if (doSquare)
                {
                    AddTriangle(p0, p1, p2);
                    AddTriangle(p3, p2, p1);
                }
                else //--- tetraeder
                {
#if false
                    not showing the bottom triangle makes it faster

                    if (MirrorL)
                    {
                        //--- add bottom triangle facing to the inside
                        AddTriangle(p1, p3, p2);
                    }
                    else
                    {
                        //--- add bottom triangle facing to the outside
                        AddTriangle(p1, p2, p3);
                    }
#endif

                    //--- all other triangles facing to the outside
                    AddTriangle(p0, p2, p1);
                    AddTriangle(p0, p1, p3);
                    AddTriangle(p0, p3, p2);
                }
            }

            private void AddTriangle(Point3D p, Point3D q, Point3D r)
            {
                positions.Add(p);
                trindices.Add(trindex++);

                positions.Add(q);
                trindices.Add(trindex++);

                positions.Add(r);
                trindices.Add(trindex++);
            }

            private void GetPoints(ref Point3D p0, out Point3D p1, out Point3D p2, out Point3D p3, bool doSquare)
            {
                Point3DS p0s = new Point3DS(p0);
                Quaternion q = Math3D.RotationZ(p0s.Phi, false) * Math3D.RotationY(p0s.Theta, false);
                Vector3D vx = q.Transform(Math3D.UnitX);
                Vector3D vy = q.Transform(Math3D.UnitY);

                double length = pendulum.IsHighlighted ? 0.015 : 0.005;
                vx *= length;
                vy *= length;

                if (doSquare)
                {
                    p0 -= (vx + vy) * 0.5;
                    p1 = p0 + vx;
                    p2 = p0 + vy;
                    p3 = p0 + vx + vy;
                }
                else //--- tetraeder
                {
                    p1 = p0 + vy;
                    p2 = p0 + vx * 0.87 - vy * 0.5;
                    p3 = p0 - vx * 0.87 - vy * 0.5;

                    p0s.Length += length;
                    p0 = p0s.ToCartesian();
                }
            }
        }
    }
}

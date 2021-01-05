//******************************************************************************************
// Copyright © 2016 Wolfgang Foerster (wolfoerster@gmx.de)
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
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Threading.Tasks;
using WFTools3D;

namespace DoublePendulum
{
    public class Trajectory : Primitive3D
    {
        protected override MeshGeometry3D CreateMesh()
        {
            return null;
        }

        public Trajectory(PendulumData data)
            : base(8)
        {
            Data = data;

            //--- setup the material brush
            LinearGradientBrush brush = new LinearGradientBrush(new GradientStopCollection
            {
                new GradientStop(Colors.Red, 0),
                new GradientStop(Colors.Green, 0.1),
                new GradientStop(Colors.Blue, 0.2),
                new GradientStop(Colors.Goldenrod, 0.3),
                new GradientStop(Colors.Cyan, 0.4),
                new GradientStop(Colors.Magenta, 0.5),
                new GradientStop(Colors.Yellow, 0.6),
                new GradientStop(Colors.Firebrick, 0.7),
                new GradientStop(Colors.LimeGreen, 0.8),
                new GradientStop(Colors.LightBlue, 0.9),
                new GradientStop(Colors.Orange, 1)
            }, 0);
            brush.Freeze();
            DiffuseMaterial.Brush = brush;

            //--- setup the cross section of the tube
            double radius = 0.01;
            for (int id = 0; id < divisions; id++)
            {
                double phi = id * MathUtils.PIx2 / divisions;
                Section.Add(new Point(radius * Math.Cos(phi), radius * Math.Sin(phi)));
            }
        }
        PendulumData Data;
        List<Point> Section = new List<Point>();
        List<Point3D> Points = new List<Point3D>();
        LinearTransform3D T = new LinearTransform3D();

        public int Mode
        {
            get { return mode; }
            set { mode = value; Clear(); }
        }
        private int mode;

        /// <summary>
        /// Clear everything and initialize transformations.
        /// </summary>
        public void Clear()
        {
            Points.Clear();
            positions.Clear();
            normals.Clear();
            textureCoordinates.Clear();
            triangleIndices.Clear();
            startPathIndex = 0;
            Mesh = new MeshGeometry3D();

            switch (mode)
            {
                case 1: T.Init(Data.Q1Max, Data.L1Max, Data.Q2Max); break;
                case 2: T.Init(Data.Q1Max, Data.L1Max, Data.L2Max); break;
                case 3: T.Init(Data.Q2Max, Data.L2Max, Data.Q1Max); break;
                case 4: T.Init(Data.Q2Max, Data.L2Max, Data.L1Max); break;
            }
        }
        int startPathIndex = 0;
        List<Point3D> positions = new List<Point3D>();
        List<Vector3D> normals = new List<Vector3D>();
        List<Point> textureCoordinates = new List<Point>();
        List<int> triangleIndices = new List<int>();

        /// <summary>
        /// Adds the current state and updates the mesh asynchronously.
        /// </summary>
        async public void Update()
        {
            switch (mode)
            {
                case 0: return;
                case 1: Points.Add(T.Transform(Data.Q1, Data.L1, Data.Q2)); break;
                case 2: Points.Add(T.Transform(Data.Q1, Data.L1, Data.L2)); break;
                case 3: Points.Add(T.Transform(Data.Q2, Data.L2, Data.Q1)); break;
                case 4: Points.Add(T.Transform(Data.Q2, Data.L2, Data.L1)); break;
            }

            if (busy || Points.Count % 4 != 3)
                return;

            busy = true;
            await Task.Run(() =>
            {
                try
                {
                    AddTube(Points);
                }
                catch
                {
                    Clear();
                }
            });

            Mesh.Positions = new Point3DCollection(positions);
            Mesh.Normals = new Vector3DCollection(normals);
            Mesh.TextureCoordinates = new PointCollection(textureCoordinates);
            Mesh.TriangleIndices = new Int32Collection(triangleIndices);
            busy = false;
        }
        bool busy;

        //--- based on MeshBuilder.AddTube() from https://github.com/helix-toolkit
        private void AddTube(List<Point3D> path)
        {
            int pathCount = path.Count;
            int sectionCount = Section.Count;

            //--- if this is called for the first time, find a vector v which is perpendicular to the direction of the first path segment
            if (startPathIndex == 0)
                v = FindAnyPerpendicular(path[1] - path[0]);

            //--- calculate positions and normals
            int stopPathIndex = pathCount - 1;
            for (int i = startPathIndex; i < stopPathIndex; i++)
            {
                //--- calculate direction from previous to next point in path
                Vector3D forward = path[i + 1] - path[i == 0 ? 0 : i - 1];

                //--- find vector u which is perpendicular to this direction and v (note that v already is perpendicular to last direction)
                Vector3D u = v.Cross(forward);
                u.Normalize();

                //--- recalc v
                v = forward.Cross(u);
                v.Normalize();

                for (int j = 0; j < sectionCount; j++)
                {
                    Vector3D n = Section[j].X * u + Section[j].Y * v;
                    Point3D pt = path[i] + n;
                    positions.Add(pt);

                    n.Normalize();
                    normals.Add(n);
                }
            }

            //--- set triangle indices
            for (int i = startPathIndex > 0 ? startPathIndex : 1; i < stopPathIndex; i++)
            {
                for (int j = 1; j <= sectionCount; j++)
                {
                    int i11 = i * sectionCount + j;
                    int i10 = i11 - 1;
                    int i01 = i11 - sectionCount;
                    int i00 = i01 - 1;
                    if (j == sectionCount)//section is closed!
                    {
                        i11 -= sectionCount;
                        i01 -= sectionCount;
                    }
                    AddTriangleIndices(i00, i01, i11);
                    AddTriangleIndices(i11, i10, i00);
                }
            }

            //--- recalculate all texture coordinates
            textureCoordinates = new List<Point>();
            for (int i = 0; i < pathCount; i++)
            {
                for (int j = 0; j < sectionCount; j++)
                {
                    double tx = i / (pathCount - 1.0);
                    double ty = j / (sectionCount - 1.0);
                    textureCoordinates.Add(new Point(tx, ty));
                }
            }

            startPathIndex = stopPathIndex;
        }
        Vector3D v;

        Vector3D FindAnyPerpendicular(Vector3D direction)
        {
            direction.Normalize();

            Vector3D result = direction.Cross(Math3D.UnitX);
            if (result.LengthSquared < 1e-3)
                result = direction.Cross(Math3D.UnitY);

            return result;
        }

        void AddTriangleIndices(int i, int j, int k)
        {
            triangleIndices.Add(i);
            triangleIndices.Add(j);
            triangleIndices.Add(k);
        }
    }

    internal class LinearTransform3D
    {
        public void Init(double xmax, double ymax, double zmax)
        {
            tx = CreateTransform(xmax, 1);
            ty = CreateTransform(ymax, 1);
            tz = CreateTransform(zmax, 1);
        }
        LinearTransform tx, ty, tz;

        LinearTransform CreateTransform(double dataValue, double worldValue)
        {
            return new LinearTransform(-dataValue, dataValue, -worldValue, worldValue);
        }

        public Point3D Transform(double x, double y, double z)
        {
            return new Point3D(tx.Transform(x), ty.Transform(y), tz.Transform(z));
        }
    }
}
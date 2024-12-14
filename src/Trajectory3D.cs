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
#if !false
    using System.Collections.Generic;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using WFTools3D;

    public class Trajectory3D : Object3D
    {
        private readonly List<Record> records = new List<Record>();
        private readonly Transformation T = new Transformation();
        private int mode;

        public int Mode
        {
            get { return mode; }
            set
            {
                if (mode != value)
                {
                    mode = value;
                    Clear();
                }
            }
        }

        public void Clear()
        {
            var pendulum = App.SelectedPendulum;
            if (pendulum == null)
                return;

            Children.Clear();
            records.Clear();
            switch (mode)
            {
                case 1: T.Init(pendulum.Q1Max, pendulum.L1Max, pendulum.Q2Max, pendulum.L2Max); break;
                case 2: T.Init(pendulum.Q1Max, pendulum.L1Max, pendulum.L2Max, pendulum.Q2Max); break;
                case 3: T.Init(pendulum.Q2Max, pendulum.L2Max, pendulum.Q1Max, pendulum.L1Max); break;
                case 4: T.Init(pendulum.Q2Max, pendulum.L2Max, pendulum.L1Max, pendulum.Q1Max); break;
                default: return;
            }
        }

        public void NewTrajectoryPoint(double q1, double q2, double l1, double l2)
        {
            Record record;
            switch (mode)
            {
                case 1: record = T.Transform(q1, l1, q2, l2); break;
                case 2: record = T.Transform(q1, l1, l2, q2); break;
                case 3: record = T.Transform(q1, q2, l2, l1); break;
                case 4: record = T.Transform(l1, q2, l2, q1); break;
                default: return;
            }

            records.Add(record);
            Dispatcher.Invoke(() => OnNewPoint(), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void OnNewPoint()
        {
            var i = records.Count - 1;
            if (i < 1)
                return;

            var cyl = new Cylinder(6)
            {
                Radius = 0.02,
                From = records[i - 1].Point,
                To = records[i].Point
            };

            cyl.DiffuseMaterial.Brush = new SolidColorBrush(records[i].Color);
            cyl.DiffuseMaterial.Brush.Freeze();
            Children.Add(cyl);
        }

        public void Update()
        {
        }

        private class Transformation
        {
            private readonly LinearTransform3D t3d = new LinearTransform3D();
            private readonly ColorTransform tc = new ColorTransform();

            public void Init(double xmax, double ymax, double zmax, double cmax)
            {
                t3d.Init(xmax, ymax, zmax);
                tc.Init(-cmax, cmax, Color.FromRgb(255, 0, 0), Color.FromRgb(0, 255, 0));
            }

            public Record Transform(double x, double y, double z, double c)
            {
                return new Record(t3d.Transform(x, y, z), tc.GetColor(c));
            }
        }

        private class Record
        {
            public Record(Point3D point, Color color)
            {
                Color = color;
                Point = point;
            }

            public Point3D Point { get; set; }
            
            public Color Color { get; set; }
        }
    }
#else
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using WFTools3D;

    public class Trajectory3D : Primitive3D
    {
        private readonly TubeBuilder builder = new TubeBuilder(0.02, 4);
        private readonly LinearTransform3D T = new LinearTransform3D();
        private int count;
        private int mode;

        public Trajectory3D()
        {
        }

        public int Mode
        {
            get { return mode; }
            set
            {
                if (mode != value)
                {
                    mode = value;
                    Clear();
                }
            }
        }

        public void Clear()
        {
            builder.Clear();

            var pendulum = App.SelectedPendulum;
            if (pendulum == null)
                return;

            switch (mode)
            {
                case 1: T.Init(pendulum.Q1Max, pendulum.L1Max, pendulum.Q2Max); break;
                case 2: T.Init(pendulum.Q1Max, pendulum.L1Max, pendulum.L2Max); break;
                case 3: T.Init(pendulum.Q2Max, pendulum.L2Max, pendulum.Q1Max); break;
                case 4: T.Init(pendulum.Q2Max, pendulum.L2Max, pendulum.L1Max); break;
                default: return;
            }

            DiffuseMaterial.Brush = new SolidColorBrush(pendulum.PoincareColor);
            DiffuseMaterial.Brush.Freeze();
        }

        public void NewTrajectoryPoint(double q1, double q2, double l1, double l2)
        {
            Point3D point;
            switch (mode)
            {
                case 1: point = T.Transform(q1, l1, q2); break;
                case 2: point = T.Transform(q1, l1, l2); break;
                case 3: point = T.Transform(q2, l2, q1); break;
                case 4: point = T.Transform(q2, l2, l1); break;
                default: return;
            }

            builder.AddPoint(point);
        }

        public void Update()
        {
            if (++count == 4)
            {
                count = 0;
                InitMesh();
            }
        }

        protected override MeshGeometry3D CreateMesh()
        {
            return builder.CreateMesh();
        }
    }
#endif
}

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
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using WFTools3D;

    public class Trajectory3D : Primitive3D
    {
        private readonly TubeBuilder builder = new(0.02, 4);
        private readonly LinearTransform3D T = new();
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
            InitMesh();

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
            InitMesh();
        }

        protected override MeshGeometry3D CreateMesh()
        {
            return builder.CreateMesh();
        }
    }
}

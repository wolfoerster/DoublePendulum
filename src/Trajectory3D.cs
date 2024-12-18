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
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using WFTools3D;

    public class Trajectory3D : Primitive3D
    {
        private readonly TubeBuilder[] builder = new TubeBuilder[4];
        private readonly LinearTransform4D[] T = new LinearTransform4D[4];
        private int mode;
        private bool doListen;

        public Trajectory3D()
        {
            DiffuseMaterial.Brush = new LinearGradientBrush(Colors.Magenta, Colors.Cyan, 0);
            DiffuseMaterial.Brush.Freeze();
        }

        public int Mode
        {
            get => mode;
            set
            {
                if (mode != value)
                {
                    mode = value;
                    InitMesh();
                }
            }
        }

        public bool DoListen 
        {
            get => doListen;
            set
            {
                if (doListen != value)
                {
                    doListen = value;
                    Clear();
                }
            }
        }

        public void Clear()
        {
            foreach (var b in builder)
                b.Clear();

            InitMesh();

            var pendulum = App.SelectedPendulum;
            if (pendulum == null)
                return;

            T[0].Init(pendulum.Q1Max, pendulum.L1Max, pendulum.Q2Max, pendulum.L2Max);
            T[1].Init(pendulum.Q1Max, pendulum.L1Max, pendulum.L2Max, pendulum.Q2Max);
            T[2].Init(pendulum.Q2Max, pendulum.L2Max, pendulum.Q1Max, pendulum.L1Max);
            T[3].Init(pendulum.Q2Max, pendulum.L2Max, pendulum.L1Max, pendulum.Q1Max);
        }

        public void NewTrajectoryPoint(double q1, double q2, double l1, double l2)
        {
            if (!doListen)
                return;

            var (point, tc) = T[0].Transform(q1, l1, q2, l2);
            builder[0].AddPoint(point, tc);

            (point, tc) = T[1].Transform(q1, l1, l2, q2);
            builder[1].AddPoint(point, tc);

            (point, tc) = T[2].Transform(q2, l2, q1, l1);
            builder[2].AddPoint(point, tc);

            (point, tc) = T[3].Transform(q2, l2, l1, q1);
            builder[3].AddPoint(point, tc);
        }

        public void Update()
        {
            InitMesh();
        }

        protected override MeshGeometry3D CreateMesh()
        {
            EnsureBuilder();
            return builder[mode].CreateMesh();
        }

        private void EnsureBuilder()
        {
            if (builder[0] == null)
            {
                for (int i = 0; i < 4; i++)
                {
                    builder[i] = new TubeBuilder();
                    T[i] = new LinearTransform4D();
                }
            }
        }
    }
}

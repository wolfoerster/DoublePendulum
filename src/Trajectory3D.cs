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
        private int count;

        public Trajectory3D()
        {
            DiffuseMaterial.Brush = new LinearGradientBrush(Colors.Cyan, Colors.Magenta, 0);
            DiffuseMaterial.Brush.Freeze();
            BackMaterial = Material;
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

        public void Init()
        {
            foreach (var b in builder)
                b.Clear();

            InitMesh();

            var q1Max = App.SelectedPendulum.Q1Max;
            var q2Max = App.SelectedPendulum.Q2Max;
            var l1Max = App.SelectedPendulum.L1Max;
            var l2Max = App.SelectedPendulum.L2Max;

            T[0].Init(q1Max, l1Max, q2Max, l2Max);
            T[1].Init(q1Max, l1Max, l2Max, q2Max);
            T[2].Init(q2Max, l2Max, q1Max, l1Max);
            T[3].Init(q2Max, l2Max, l1Max, q1Max);
        }

        public void NewTrajectoryPoint(double q1, double q2, double l1, double l2)
        {
            if (Pendulum.IsFixed)
                q2 = l2 = 0;

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
            if (++count == 2)
            {
                count = 0;
                InitMesh();
            }
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

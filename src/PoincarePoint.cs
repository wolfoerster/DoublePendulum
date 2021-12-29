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
    using System;
    using WFTools3D;

    public class PoincarePoint
    {
        /// <summary>
        /// Poincare condition: q2 = 0 and w2 > 0.
        /// </summary>
        public PoincarePoint(double q1, double w1, double w2)
        {
            Q1 = q1;
            W1 = w1;
            W2 = w2;

            double cos = Math.Cos(q1);
            L1 = 2 * w1 + w2 * cos;
            L2 = w2 + w1 * cos;
        }

        public double Q1 { get; private set; }
        public double W1 { get; private set; }
        public double W2 { get; private set; }
        public double L1 { get; private set; }
        public double L2 { get; private set; }

        public double E
        {
            get
            {
                double cos = Math.Cos(Q1);
                //double eKin = w1 * w1 + w2 * w2 / 2.0 + w1 * w2 * Math.Cos(q1 - q2);
                double eKin = W1 * W1 + W2 * W2 / 2.0 + W1 * W2 * cos;
                //double ePot = 3 - 2 * Math.Cos(q1) - Math.Cos(q2);
                double ePot = 2 - 2 * cos;
                return eKin + ePot;
            }
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", Q1.ToStringInv(), W1.ToStringInv(), W2.ToStringInv());
        }
    }
}

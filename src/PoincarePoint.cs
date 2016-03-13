//******************************************************************************************
// Copyright © 2016 Wolfgang Foerster (wolfoerster@gmx.de)
//
// This file is part of the EquationOfTime project which can be found on github.com
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

namespace DoublePendulum
{
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
			L1 = 2 * W1 + W2 * cos;
			L2 = W2 + W1 * cos;
		}

		public double Q1 { get; private set; }
		public double W1 { get; private set; }
		public double W2 { get; private set; }
		public double L1 { get; private set; }
		public double L2 { get; private set; }

		public override string ToString()
		{
			return string.Format("{0},{1},{2}", Q1.ToString("G3"), W1.ToString("G3"), W2.ToString("G3"));
		}
	}
}

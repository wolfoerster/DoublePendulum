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
using System.Windows.Media;
using System.Collections.Generic;
using WFTools3D;

namespace DoublePendulum
{
	public class PendulumData
	{
		#region Public Properties

		public List<PoincarePoint> Points = new List<PoincarePoint>();

		public event EventHandler NewPoincarePoint;

		public bool Gravity
		{
			get { return gravity; }
			set { gravity = value; }
		}
		bool gravity = true;

		public double E0
		{
			get { return e0; }
			set
			{
				e0 = value;
				de = 0;

				l1max = 2.0 * Math.Sqrt(e0);
				l2max = Math.Sqrt(2.0 * e0);
				q1max = (e0 >= 4 || !gravity) ? Math.PI : q1max = Math.Acos(1.0 - e0 / 2.0);
				q2max = (e0 >= 2 || !gravity) ? Math.PI : q2max = Math.Acos(1.0 - e0);

				//--- give an estimation for dt
				dt = 2e-6 / (Math.Sqrt(e0) + 1);
			}
		}
		double e0;

		public double Q1
		{
			get { return q1; }
		}
		double q1;

		public double Q2
		{
			get { return q2; }
		}
		double q2;

		public double W1
		{
			get { return w1; }
		}
		double w1;

		public double W2
		{
			get { return w2; }
		}
		double w2;

		public double A1
		{
			get { return a1; }
		}
		double a1;

		public double A2
		{
			get { return a2; }
		}
		double a2;

		public double L1
		{
			get { return 2.0 * w1 + w2 * Math.Cos(q1 - q2); }
		}

		public double L2
		{
			get { return w2 + w1 * Math.Cos(q1 - q2); }
		}

		public double Q1Max
		{
			get { return q1max; }
		}
		double q1max;

		public double Q2Max
		{
			get { return q2max; }
		}
		double q2max;

		public double L1Max
		{
			get { return l1max; }
		}
		double l1max;

		public double L10
		{
			get { return l10; }
		}
		double l10;

		public double L20
		{
			get { return l20; }
		}
		double l20;

		public double L2Max
		{
			get { return l2max; }
		}
		double l2max;

		public double dT
		{
			get { return dt; }
			set
			{
				if (value > -1e3 && value < 1e-3)
					dt = value;
			}
		}
		double dt = 1e-6;

		public double dE
		{
			get { return de; }
		}
		double de;

		public Color Color = Colors.White;

		#endregion Public Properties

		public PendulumData Clone()
		{
			PendulumData data = MemberwiseClone() as PendulumData;
			data.Points = new List<PoincarePoint>(Points);
			return data;
		}

		public void Init(double q01, double q02, double w01, double w02)
		{
			q1 = q10 = q01;
			q2 = q20 = q2old = q02;
			w1 = w10 = w01;
			w2 = w20 = w02;
			a1 = a2 = 0;

			double cos = Math.Cos(q1 - q2);
			l10 = 2.0 * w1 + w2 * cos;
			l20 = w2 + w1 * cos;

			E0 = CalculateEnergy();
			Points.Clear();
		}
		double q10, q20, w10, w20, q2old;

		public bool Init(double e00, double q01, double l01)
		{
			E0 = e00;
			return Init(q01, l01);
		}

		public bool Init(double q01, double l01)
		{
			//--- E0 is already set!
			de = 0;
			Points.Clear();

			q1 = q10 = q01;
			q2 = q20 = 0;//--- Poincare condition!
			q2old = -0.1;
			l10 = l01;

			double cos = Math.Cos(q1 - q2);
			double b = 2.0 - cos * cos;

			l20 = CalculateL2(cos, b);
			if (double.IsNaN(l20))
				return false;

			w2 = w20 = (2.0 * l20 - l10 * cos) / b;
			w1 = w10 = (l10 - l20 * cos) / b;
			return true;
		}

		double CalculateL2(double cos, double b)
		{
			/***
			 * In folgenden Einheiten 
			 *		Zeit in sqrt(l / g), l = Laenge der Pendel, g = Erdbeschleunigung
			 *		Energie in m*g*l
			 *		Drehimpuls in m*l*sqrt(l*g)
			 * wird die Lagrange-Funktion
			 * 
			 * L = eKin - ePot = w1^2 + w2^2/2 + w1*w2*cos - (3 - 2*cos(q1) - cos(q2)), mit cos = cos(q1 - q2)
			 * 
			 * Daraus folgt fuer die Drehimpulse
			 * 
			 * L1 = dL/dw1 = 2w1 + w2*cos
			 * L2 = dL/dw2 = w2 + w1*cos
			 * 
			 * bzw. fuer die Winkelgeschwindigkeiten
			 * 
			 * w1 = (L1 - L2*cos) / b, mit b = 2 - cos^2
			 * w2 = (2L2 - L1*cos) / b
			 * 
			 * Fuer die Hamiltonfunktion gilt
			 * 
			 * H = L1*w1 + L2*w2 - L
			 * 
			 * Alles eingesetzt ergibt
			 * 
			 * H = (L1^2/2 + L2^2 - L1*L2*cos)/b + 3 - 2*cos(q1) - cos(q2)
			 * 
			 * In unserem Fall entspricht die Hamiltonfunktion der Gesamtenergie des Systems.
			 * Diese ist aber E0 und bleibt unveraendert! Also gilt
			 * 
			 * E0 = [L1^2/2 + L2^2 - L1*L2*cos] / b + ePot, oder
			 * 
			 * bE0 = L1^2/2 + L2^2 - L1*L2*cos + b*ePot, oder
			 * 
			 * 0 = L1^2/2 + L2^2 - L1*L2*cos + b(ePot - E0), oder
			 * 
			 * L2^2 - (L1*cos)L2 + [b(ePot - E0) + L1^2/2] = 0 (quadratische Gleichung fuer L2)
			 * 
			 * Daraus laesst sich L2 bestimmen gemaess der quadratischen Normalform: 
			 * 
			 * x^2 + px + q = 0 ==> x = -p/2 +/- sqrt((p/2)^2 - q)
			 * 
			 ***/

			double ePot = 3.0 - 2.0 * Math.Cos(q1) - Math.Cos(q2);
			double p = l10 * cos / 2.0;//ist eigentlich schon -p/2
			double q = b * (ePot - e0) + l10 * l10 / 2.0;

			double arg = p * p - q;
			if (arg < -eps)
				return double.NaN;

			double result = arg < eps ? p : p + Math.Sqrt(arg);//p - sqrt() fuehrt zu einer anderen Bahn
			return result;
		}
		double eps = 1e-12;

		public double CalculateEnergy()
		{
			double eKin = w1 * w1 + w2 * w2 / 2.0 + w1 * w2 * Math.Cos(q1 - q2);

			if (!gravity)
				return eKin;

			double ePot = 3.0 - 2.0 * Math.Cos(q1) - Math.Cos(q2);
			return eKin + ePot;
		}

		public void Move(int numSteps)
		{
			double A12, Det, B0, B1, B2;

			for (int i = 0; i < numSteps; i++)
			{
				//--- first check for Poincare condition
				if (q2 >= 0 && q2old < 0 && q2 <= MathUtils.PIo2)
				{
					//--- move back in time to when q2 was 0
					double t = -Math.Abs(q2 * dt / (q2 - q2old));
					Points.Add(new PoincarePoint(q1 + w1 * t, w1 + a1 * t, w2 + a2 * t));
					if (NewPoincarePoint != null)
						NewPoincarePoint(this, new EventArgs());
				}
				q2old = q2;

				//--- then move the pendulum
				A12 = Math.Cos(q1 - q2);
				Det = 2.0 - A12 * A12;
				B0 = Math.Sin(q1 - q2);
				B1 = -B0 * w2 * w2;
				B2 = B0 * w1 * w1;

				if (gravity)
				{
					B1 -= 2 * Math.Sin(q1);
					B2 -= Math.Sin(q2);
				}

				a1 = (B1 - B2 * A12) / Det;
				w1 += a1 * dt;
				q1 += w1 * dt;
				q1 = MathUtils.NormalizeAngle(q1);

				a2 = (B2 * 2.0 - B1 * A12) / Det;
				w2 += a2 * dt;
				q2 += w2 * dt;
				q2 = MathUtils.NormalizeAngle(q2);
			}

			if (++count > 4000)//--- check energy every now and then
			{
				count = 0;
				double e = CalculateEnergy();
				de = (e - e0) / e0 * 100.0;
			}
		}
		int count;
	}
}

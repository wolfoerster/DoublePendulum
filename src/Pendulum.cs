﻿//******************************************************************************************
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
#pragma warning disable IDE1006 // Naming Styles

namespace DoublePendulum
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Media;
    using WFTools3D;

    public class Pendulum
    {
        private const byte ModiFlag = 0x01; // indicates pendulum is modified
        private const byte MuteFlag = 0x02; // indicates pendulum is muted
        private const byte SoloFlag = 0x04; // indicates pendulum is soloed
        private const byte HighFlag = 0x08; // indicates pendulum is highlighted

        private bool gravity = true;
        private int id = 999;
        private byte flags;
        private Color poincareColor = Colors.White;
        private double q1, q10, q1max, q2, q20, q2max, q2old, w1, w10, w2, w20, a1, a2, l1, l10, l1max, l2, l20, l2max, e0, de, dt, time;

        public Pendulum()
        {
        }

        public Pendulum(string fileName)
        {
            Read(fileName);
        }

        public List<PoincarePoint> PoincarePoints { get; private set; } = new List<PoincarePoint>();

        public Action<Pendulum> NewPoincarePoint;

        public Action<double, double, double, double> NewTrajectoryPoint;

        public void UpdateTrajectory()
        {
            NewTrajectoryPoint?.Invoke(q1, q2, L1, L2);
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public Color PoincareColor
        {
            get { return poincareColor; }
            set { poincareColor = value; }
        }

        public bool Gravity
        {
            get { return gravity; }
            set { gravity = value; }
        }

        public double dT
        {
            get { return dt; }
            set { dt = value; }
        }

        public double Q1 => q1;

        public double Q10 => q10;

        public double Q1Max => q1max;

        public double Q2 => q2;

        public double Q2Max => q2max;

        public double W1 => w1;

        public double W2 => w2;

        public double A1 => a1;

        public double A2 => a2;

        public double L1 => 2.0 * w1 + w2 * Math.Cos(q1 - q2);

        public double L2 => w2 + w1 * Math.Cos(q1 - q2);

        public double L10 => l10;

        public double L1Max => l1max;

        public double L2Max => l2max;

        public double E0 => e0;

        public double dE => de;

        public double SimulationTime // in s
        {
            // Einfaches Pendel der Laenge l: w = Sqrt(g / l), T = 2PI / w
            // ==> fuer T = 1 sec ist l = 24.85 cm.
            // Da die Zeiten in Einheiten von Sqrt(l / g) gemessen
            // werden, ist die Einheit der Zeit fuer das Doppelpendel
            // mit l = 12.425 in Sekunden
            get { return time * 0.1125396; }
        }

        public bool IsModified
        {
            get => GetFlag(ModiFlag);
            set => SetFlag(ModiFlag, value);
        }

        public bool IsMuted
        {
            get => GetFlag(MuteFlag);
            set => SetFlag(MuteFlag, value);
        }

        public bool IsSoloed
        {
            get => GetFlag(SoloFlag);
            set => SetFlag(SoloFlag, value);
        }

        public bool IsHighlighted
        {
            get => GetFlag(HighFlag);
            set => SetFlag(HighFlag, value);
        }

        public void Init(double q10, double q20, double w10, double w20)
        {
            q1 = this.q10 = q10;
            q2 = this.q20 = q2old = q20;
            w1 = this.w10 = w10;
            w2 = this.w20 = w20;
            a1 = a2 = 0;

            double cos = Math.Cos(q10 - q20);
            l10 = 2.0 * w10 + w20 * cos;
            l20 = w20 + w10 * cos;

            e0 = CalculateEnergy();
            SetEnergy(e0);
            ResetMovement();
        }

        public bool Init(double e0, double q10 = 0, double l10 = 0)
        {
            SetEnergy(e0);
            q1 = this.q10 = q10;
            l1 = this.l10 = l10;

            //--- Poincare condition!
            q2 = q20 = 0;
            q2old = -0.1;

            double cos = Math.Cos(q1 - q2);
            double b = 2.0 - cos * cos;
            l2 = l20 = CalculateL2(cos, b);

            if (double.IsNaN(l2))
                return false;

            w2 = (2.0 * l2 - l1 * cos) / b;

            if (w2 < 0)
                return false;

            w1 = (l1 - l2 * cos) / b;

            ResetMovement();
            return true;
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
                    //--- q(0) = q2, q(-dt) = q2old, q(t) = 0 ==>
                    double t = -Math.Abs(q2 * dt / (q2 - q2old));
                    PoincarePoints.Add(new PoincarePoint(q1 + w1 * t, w1 + a1 * t, w2 + a2 * t));
                    NewPoincarePoint?.Invoke(this);
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

            time += numSteps * dt;
        }

        public double CalculateEnergy()
        {
            double eKin = w1 * w1 + w2 * w2 / 2.0 + w1 * w2 * Math.Cos(q1 - q2);

            if (!gravity)
                return eKin;

            double ePot = 3.0 - 2.0 * Math.Cos(q1) - Math.Cos(q2);
            return eKin + ePot;
        }

        public void CheckEnergy()
        {
            double e1 = CalculateEnergy();
            de = (e1 - e0) / e0 * 100.0;
        }

        private double CalculateL2(double cos, double b)
        {
            /***
            * In folgenden Einheiten
            * 
            *       Zeit in sqrt(l / g), l = Laenge der Pendel, g = Erdbeschleunigung
            *       Energie in m*g*l
            *       Drehimpuls in m*l*sqrt(l*g)
            *       
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

            var ePot = 3.0 - 2.0 * Math.Cos(q1) - Math.Cos(q2);
            var p = l1 * cos * 0.5; // ist eigentlich schon -p/2
            var q = b * (ePot - e0) + l1 * l1 * 0.5;
            var arg = p * p - q;

            if (arg < -1e-13)
                return double.NaN;

            if (arg < 0)
                return p;

            var sqrt = Math.Sqrt(arg);
            return p + sqrt; // p - sqrt fuehrt zu negativem w2 und ist damit kein Poincarepunkt
        }

        private void SetEnergy(double energy)
        {
            e0 = energy;
            l1max = 2.0 * Math.Sqrt(e0);
            l2max = Math.Sqrt(2.0 * e0);
            q1max = (e0 >= 4 || !gravity) ? Math.PI : Math.Acos(1.0 - e0 / 2.0);
            q2max = (e0 >= 2 || !gravity) ? Math.PI : Math.Acos(1.0 - e0);
        }

        private void ResetMovement()
        {
            de = 0;
            dt = 2e-6 / (Math.Sqrt(e0) + 1);
            dt = dt.RoundSignificantDigits(2);
            time = 0;
            PoincarePoints.Clear();
        }

        private bool GetFlag(byte flag)
        {
            return (flags & flag) > 0;
        }

        private void SetFlag(byte flag, bool value)
        {
            if (value)
                flags |= flag;
            else
                flags &= unchecked((byte)~flag);
        }

        public bool Read(string fileName)
        {
            try
            {
                using (var reader = new BinaryReader(File.OpenRead(fileName)))
                {
                    byte version = reader.ReadByte();
                    if (version < 101 || version > 105)
                        return false;

                    if (version < 105)
                    {
                        reader.ReadBytes(3);
                        id = reader.ReadInt32();
                        reader.ReadBytes(12);
                    }
                    else
                    {
                        id = reader.ReadInt32();
                    }

                    q10 = reader.ReadDouble();
                    q20 = reader.ReadDouble();
                    w10 = reader.ReadDouble();
                    w20 = reader.ReadDouble();
                    l10 = reader.ReadDouble();
                    l20 = reader.ReadDouble();
                    q1 = reader.ReadDouble();
                    q2 = reader.ReadDouble();
                    w1 = reader.ReadDouble();
                    w2 = reader.ReadDouble();
                    a1 = reader.ReadDouble();
                    a2 = reader.ReadDouble();
                    time = reader.ReadDouble();
                    q2max = reader.ReadDouble();
                    e0 = reader.ReadDouble();
                    de = reader.ReadDouble();
                    dt = reader.ReadDouble();
                    q1max = reader.ReadDouble();
                    q2old = reader.ReadDouble();
                    l1max = reader.ReadDouble();
                    l2max = reader.ReadDouble();
                    byte red = reader.ReadByte();
                    byte green = reader.ReadByte();
                    byte blue = reader.ReadByte();
                    byte alpha = reader.ReadByte();
                    flags = reader.ReadByte();

                    if (version == 104)
                    {
                        reader.ReadBytes(32);
                    }

                    SetEnergy(e0);
                    PoincareColor = Color.FromRgb(red, green, blue);
                    PoincarePoints = new List<PoincarePoint>();

                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        double q = reader.ReadDouble();
                        double w = reader.ReadDouble();
                        double v = reader.ReadDouble();
                        PoincarePoints.Add(new PoincarePoint(q, w, v));
                    }
                }

                return true;
            }
            catch
            {
            }

            return false;
        }

        public bool Write(string fileName)
        {
            try
            {
                byte version = 105;
                using (var stream = File.Create(fileName))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(version);
                    writer.Write(id);
                    writer.Write(q10);
                    writer.Write(q20);
                    writer.Write(w10);
                    writer.Write(w20);
                    writer.Write(l10);
                    writer.Write(l20);
                    writer.Write(q1);
                    writer.Write(q2);
                    writer.Write(w1);
                    writer.Write(w2);
                    writer.Write(a1);
                    writer.Write(a2);
                    writer.Write(time);
                    writer.Write(q2max);
                    writer.Write(e0);
                    writer.Write(de);
                    writer.Write(dt);
                    writer.Write(q1max);
                    writer.Write(q2old);
                    writer.Write(l1max);
                    writer.Write(l2max);
                    writer.Write(PoincareColor.R);
                    writer.Write(PoincareColor.G);
                    writer.Write(PoincareColor.B);
                    writer.Write(PoincareColor.A);
                    writer.Write(flags);

                    foreach (var pp in PoincarePoints)
                    {
                        writer.Write(pp.Q1);
                        writer.Write(pp.W1);
                        writer.Write(pp.W2);
                    }
                }

                return true;
            }
            catch
            {
            }

            return false;
        }
    }
}

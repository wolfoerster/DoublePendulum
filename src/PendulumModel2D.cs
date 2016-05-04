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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;
using WFTools3D;

namespace DoublePendulum
{
	class PendulumModel2D : Canvas
	{
		public PendulumModel2D()
		{
			ShowVelos = false;
			SizeChanged += MySizeChanged;

			axis.Fill = line1.Stroke = line2.Stroke = cold;

			line1.StrokeStartLineCap = line2.StrokeStartLineCap =
				line1.StrokeEndLineCap = line2.StrokeEndLineCap = PenLineCap.Round;

			weight1.Fill = weight2.Fill = warm1;
			omega1.Fill = omega2.Fill = warm2;

			Children.Add(line1);
			Children.Add(line2);
			Children.Add(axis);
			Children.Add(weight1);
			Children.Add(weight2);
			Children.Add(omega1);
			Children.Add(omega2);

			timer.Interval = TimeSpan.FromMilliseconds(120);
			timer.Tick += timer_Tick;
		}

		/// <summary>
		/// Make the second weight look hot for a short period. 
		/// Will be called from outside if the Poincare condition is true.
		/// </summary>
		public void NewPoincarePoint()
		{
			if (timer.IsEnabled)
				return;

			hotElli = weight2;
			hotElli.Fill = hot;
			timer.Start();
		}

		void timer_Tick(object sender, EventArgs e)
		{
			timer.Stop();
			CoolDownHotElli();
		}

		#region Private Fields

		Line line1 = new Line();
		Line line2 = new Line();
		Ellipse axis = new Ellipse();
		Ellipse weight1 = new Ellipse();
		Ellipse weight2 = new Ellipse();
		Ellipse omega1 = new Ellipse();
		Ellipse omega2 = new Ellipse();
		Ellipse hotElli;//one of the above ellipses that the mouse is over
		Point position1;//the position of the first weight
		Brush hot = Brushes.Red;
		Brush cold = Brushes.Green;
		Brush warm1 = Brushes.Khaki;
		Brush warm2 = Brushes.DarkKhaki;
		DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);

		#endregion Private Fields

		#region Public Properties

		public PendulumData Data { get; set; }

		public bool ShowVelos { get; set; }

		public bool IsBusy { get; set; }

		public event EventHandler UserDragged;

		#endregion Public Properties

		#region Drawing

		void MySizeChanged(object sender, SizeChangedEventArgs e)
		{
			Update();
		}

		public void Update()
		{
			double radius = Radius;
			double length = Length;

			Point p0 = Center;

			Point p1 = p0;
			p1.X += length * Math.Sin(Data.Q1);
			p1.Y += length * Math.Cos(Data.Q1);
			position1 = p1;

			Point p2 = p1;
			p2.X += length * Math.Sin(Data.Q2);
			p2.Y += length * Math.Cos(Data.Q2);

			AdjustLine(line1, p0, p1, radius);
			AdjustLine(line2, p1, p2, radius);
			AdjustEllipse(axis, p0, radius / 2);
			AdjustEllipse(weight1, p1, radius);
			AdjustEllipse(weight2, p2, radius);

			radius = ShowVelos ? radius / 3 : 0;
			Point p3 = Center;
			p3.X += length * Math.Sin(Data.Q1 + Data.W1);
			p3.Y += length * Math.Cos(Data.Q1 + Data.W1);
			AdjustEllipse(omega1, p3, radius);

			Point p4 = position1;
			p4.X += length * Math.Sin(Data.Q2 + Data.W2);
			p4.Y += length * Math.Cos(Data.Q2 + Data.W2);
			AdjustEllipse(omega2, p4, radius);

			InvalidateVisual();
		}

		double Size
		{
			get
			{
				double size = Math.Min(ActualHeight, ActualWidth);
				return size;
			}
		}

		double Radius
		{
			get
			{
				double radius = Size / 30;
				return radius;
			}
		}

		double Length
		{
			get
			{
				double length = (Size / 2 - Radius) / 2;
				return length;
			}
		}

		private void AdjustEllipse(Ellipse ellipse, Point position, double radius)
		{
			ellipse.Width = ellipse.Height = 2 * radius;
			Canvas.SetTop(ellipse, position.Y - radius);
			Canvas.SetLeft(ellipse, position.X - radius);
		}

		private void AdjustLine(Line line, Point p1, Point p2, double radius)
		{
			line.StrokeThickness = radius / 2;
			line.X1 = p1.X;
			line.Y1 = p1.Y;
			line.X2 = p2.X;
			line.Y2 = p2.Y;
		}

		#endregion Drawing

		#region Overrides

		protected override void OnRender(DrawingContext dc)
		{
			Rect rect = new Rect(0, 0, RenderSize.Width, RenderSize.Height);
			dc.DrawRectangle(Brushes.Black, null, rect);

			if (!ShowVelos)
				return;

			double thickness = line1.StrokeThickness * 0.667;

			Pen pen = new Pen(cold, thickness);
			DrawArc(dc, Data.Q1, Data.W1, Center, pen);
			DrawArc(dc, Data.Q2, Data.W2, position1, pen);

			pen = new Pen(hot, thickness);
			double fac = MathUtils.PIo2 / Data.L1Max;
			bool showA = true;
			double a1 = showA ? Data.A1 : Data.L1 * fac;
			double a2 = showA ? Data.A2 : Data.L2 * fac;
			DrawArc(dc, Data.Q1, a1, Center, pen);
			DrawArc(dc, Data.Q2, a2, position1, pen);
		}

		private void DrawArc(DrawingContext dc, double q, double w, Point origin, Pen pen)
		{
			double start = q - MathUtils.PIo2;
			double stop = start + w;
			Geometry arc = ArcGeometry.Create(origin, Length, start, stop, pen.Thickness);
			dc.DrawGeometry(null, pen, arc);
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (hotElli != null)
			{
				double x = Canvas.GetLeft(hotElli);
				double y = Canvas.GetTop(hotElli);
				double r = hotElli.Width * 0.5;
				delta = new Point(x + r, y + r) - e.GetPosition(this);
				CaptureMouse();
			}
		}
		Vector delta = new Vector(double.NaN, 0);

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			delta.X = double.NaN;
			ReleaseMouseCapture();
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			Cursor = null;
			CoolDownHotElli();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (IsBusy)
				return;

			if (double.IsNaN(delta.X))
				CheckForHotElli(e);

			if (hotElli == null)
			{
				Cursor = null;
			}
			else
			{
				Cursor = Cursors.Hand;
				if (e.LeftButton == MouseButtonState.Pressed)
					MoveHotElli(e.GetPosition(this) + delta);
			}
		}

		#endregion Overrides

		#region Helpers

		/// <summary>
		/// Gets the center of this element.
		/// </summary>
		private Point Center
		{
			get { return new Point(ActualWidth / 2, ActualHeight / 2); }
		}

		private void CoolDownHotElli()
		{
			if (hotElli != null)
			{
				hotElli.Fill = (hotElli == weight1 || hotElli == weight2) ? warm1 : warm2;
				hotElli = null;
			}
		}

		private void CheckForHotElli(MouseEventArgs e)
		{
			Ellipse eli = GetEllipse(e);

			if (eli == null)
			{
				CoolDownHotElli();
				return;
			}

			if (eli == hotElli)
				return;

			CoolDownHotElli();

			hotElli = eli;
			hotElli.Fill = hot;
		}

		private Ellipse GetEllipse(MouseEventArgs e)
		{
			if (IsHit(e, omega2))
				return omega2;

			if (IsHit(e, omega1))
				return omega1;

			if (IsHit(e, weight2))
				return weight2;

			if (IsHit(e, weight1))
				return weight1;

			return null;
		}

		private bool IsHit(MouseEventArgs e, Ellipse eli)
		{
			Point pt = e.GetPosition(eli);
			return VisualTreeHelper.HitTest(eli, pt) != null;
		}

		private void MoveHotElli(Point pt)
		{
			double q1 = Data.Q1;
			double q2 = Data.Q2;
			double w1 = Data.W1;
			double w2 = Data.W2;

			if (!ShowVelos)
				w1 = w2 = 0;

			if (hotElli == weight1)
			{
				Vector v = pt - Center;
				q1 = Math.Atan2(v.X, v.Y);
			}
			else if (hotElli == weight2)
			{
				Vector v = pt - position1;
				q2 = Math.Atan2(v.X, v.Y);
			}
			else if (hotElli == omega1)
			{
				Vector v = pt - Center;
				v = Rotate(v, q1 - MathUtils.PIo2);
				w1 = Math.Atan2(-v.Y, v.X);
			}
			else if (hotElli == omega2)
			{
				Vector v = pt - position1;
				v = Rotate(v, q2 - MathUtils.PIo2);
				w2 = Math.Atan2(-v.Y, v.X);
			}

			Data.Init(q1, q2, w1, w2);
			Update();

			if (UserDragged != null)
				UserDragged(this, new EventArgs());
		}

		Vector Rotate(Vector v, double angle)
		{
			double sin = Math.Sin(angle);
			double cos = Math.Cos(angle);
			return new Vector(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
		}

		#endregion Helpers
	}
}

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

namespace DoublePendulum
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using WFTools3D;

    internal class Poincare2D : Border
    {
        private readonly ZoomStack zoomStack = new ZoomStack();
        private readonly Image image = new Image();
        private readonly bool hideChaos = false;
        private WriteableBitmap bitmap;

        public Poincare2D()
        {
            Child = image;
            image.Stretch = Stretch.None;
            SizeChanged += MySizeChanged;
            RenderOptions.SetEdgeMode(image, EdgeMode.Aliased);
        }

        void MySizeChanged(object sender, SizeChangedEventArgs e)
        {
            var dpi = VisualTreeHelper.GetDpi(this);
            int width = (int)(ActualWidth * dpi.DpiScaleX);
            int height = (int)(ActualHeight * dpi.DpiScaleY);

            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            //no! bitmap = new WriteableBitmap(width, height, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32, null);
            image.Source = bitmap;

            zoomStack.Init(width, height);
            Redraw();
        }

        public static bool MirrorQ = false;

        public void Redraw()
        {
            if (bitmap == null)
                return;

            if (zoomStack.IsEmpty)
            {
                var pendulum = App.Pendulums.Count > 0 ? App.Pendulums[0] : App.SelectedPendulum;
                zoomStack.Zoom(-pendulum.Q1Max, pendulum.Q1Max, -pendulum.L1Max, pendulum.L1Max);
            }

            bitmap.Lock();
            bitmap.Clear();

            var soloed = false;
            foreach (var pendulum in App.Pendulums)
            {
                if (pendulum.IsSoloed)
                {
                    soloed = true;
                    ShowData(pendulum);
                }
            }

            if (!soloed)
            {
                foreach (var pendulum in App.Pendulums)
                    ShowData(pendulum);
            }

            bitmap.Unlock();
        }

        public void Clear()
        {
            zoomStack.Clear();

            if (bitmap != null)
            {
                bitmap.Lock();
                bitmap.Clear();
                bitmap.Unlock();
            }
        }

        void ShowData(Pendulum pendulum)
        {
            if (bitmap == null || pendulum.IsMuted || pendulum.PoincarePoints.Count == 0)
                return;

            if (hideChaos && pendulum.PoincareColor == Colors.Cyan)
                return;

            bitmap.Lock();

            // do not use foreach because pendulum.PoincarePoints might change during loop execution
            var nPoints = pendulum.PoincarePoints.Count;

            for (int n = 0; n < nPoints; n++)
            {
                AddPoint(pendulum.PoincarePoints[n], pendulum.PoincareColor, pendulum.IsHighlighted);
            }

            bitmap.Unlock();
        }

        public void NewPoincarePoint(Pendulum pendulum)
        {
            if (!pendulum.IsMuted)
            {
                var pt = pendulum.PoincarePoints[pendulum.PoincarePoints.Count - 1];
                AddPoint(pt, pendulum.PoincareColor, pendulum.IsHighlighted);
            }
        }

        void AddPoint(PoincarePoint pt, Color color, bool isHighlighted)
        {
            bitmap.Lock();

            AddPoint(pt.Q1, pt.L1, color, isHighlighted);

            if (MirrorQ)
                AddPoint(-pt.Q1, pt.L1, color, isHighlighted);

            bitmap.Unlock();
        }

        void AddPoint(double q1, double l1, Color color, bool isHighlighted)
        {
            var pt = zoomStack.DataToPixel(new Point(q1, l1));

            int x = (int)Math.Round(pt.X);
            int y = (int)Math.Round(pt.Y);

            if (x < 0 || y < 0 || x >= bitmap.PixelWidth || y >= bitmap.PixelHeight)
                return;

            bitmap.SetPixel(x, y, color);

            if (isHighlighted)
                bitmap.DrawRectangle(x - 1, y - 1, x + 1, y + 1, color);
        }

        public Point GetCoordinates()
        {
            Point pt = Mouse.GetPosition(this).ToPixel(this);
            return zoomStack.PixelToData(pt);
        }

        public int GetNearestPendulumIndex(Point ptMouse, out int poincareIndex)
        {
            poincareIndex = -1;
            var pendulumIndex = -1;
            var minDist = double.PositiveInfinity;

            for (int i = 0; i < App.Pendulums.Count; i++)
            {
                var pendulum = App.Pendulums[i];
                for (int j = 0; j < pendulum.PoincarePoints.Count; j++)
                {
                    PoincarePoint pt = pendulum.PoincarePoints[j];
                    double l1 = pt.L1;

                    Vector v = ptMouse - new Point(pt.Q1, l1);
                    double d = v.LengthSquared;
                    if (d < minDist)
                    {
                        minDist = d;
                        pendulumIndex = i;
                        poincareIndex = j;
                    }

                    if (MirrorQ)
                    {
                        v = ptMouse - new Point(-pt.Q1, l1);
                        d = v.LengthSquared;
                        if (d < minDist)
                        {
                            minDist = d;
                            pendulumIndex = i;
                            poincareIndex = j;
                        }
                    }
                }
            }

            return pendulumIndex;
        }

        #region Zooming

        private Point mouseDown = new Point(double.NaN, 0);
        private OverlayRect ovr;
        private bool IsZooming => ovr != null && !double.IsNaN(mouseDown.X);

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            CaptureMouse();
            mouseDown = e.GetPosition(this);
            ovr = new OverlayRect(this, mouseDown);
            AdornerLayer.GetAdornerLayer(this).Add(ovr);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsZooming)
            {
                var mode = e.RightButton == MouseButtonState.Pressed;
                mouseDown = ovr.HandleMouseMove(e.GetPosition(this), mode);
            }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            if (IsZooming)
            {
                e.Handled = true;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsZooming)
            {
                AdornerLayer.GetAdornerLayer(this).Remove(ovr);
                ovr = null;

                e.Handled = true;
                ReleaseMouseCapture();
                Point mouseUp = e.GetPosition(this);

                if (mouseUp.X == mouseDown.X && mouseUp.Y == mouseDown.Y)
                {
                    e.Handled = false; // might be handled in main window
                }
                else if (mouseUp.X > mouseDown.X && mouseUp.Y > mouseDown.Y)
                {
                    var topLeft = mouseDown.ToPixel(this);
                    var bottomRight = mouseUp.ToPixel(this);
                    zoomStack.Zoom(topLeft, bottomRight);
                    Redraw();
                }
                else
                {
                    zoomStack.Unzoom((mouseDown - mouseUp).Length < 200);
                    Redraw();
                }

                mouseDown.X = double.NaN;
            }
        }

        private sealed class OverlayRect : Adorner
        {
            private readonly Brush fill;
            private Point p1;
            private Point p2;

            public OverlayRect(UIElement adornedElement, Point pt)
                : base(adornedElement)
            {
                p1 = p2 = pt;
                fill = Brushes.White.Clone();
                fill.Opacity = 0.5;
            }

            public Point HandleMouseMove(Point pt, bool mode)
            {
                if (mode)
                    p1 += pt - p2;

                p2 = pt;
                InvalidateVisual();
                return p1;
            }

            protected override void OnRender(DrawingContext dc)
            {
                if (p2.X > p1.X && p2.Y > p1.Y)
                    dc.DrawRectangle(fill, null, new Rect(p1, p2));
            }
        }

        class ZoomStack
        {
            private readonly List<LinearTransform> tx = new List<LinearTransform>();
            private readonly List<LinearTransform> ty = new List<LinearTransform>();
            private int lastColumn;
            private int lastRow;

            public void Init(int width, int height)
            {
                double xmin = double.NaN, xmax = 0, ymin = 0, ymax = 0;

                if (tx.Count > 0)
                {
                    int i = tx.Count - 1;
                    xmin = tx[i].BackTransform(0);
                    xmax = tx[i].BackTransform(lastColumn);
                    ymin = ty[i].BackTransform(lastRow);
                    ymax = ty[i].BackTransform(0);
                }

                tx.Clear();
                ty.Clear();

                lastColumn = width - 1;
                lastRow = height - 1;

                if (!double.IsNaN(xmin))
                {
                    Zoom(xmin, xmax, ymin, ymax);
                }
            }

            public void Clear()
            {
                tx.Clear();
                ty.Clear();
            }

            public bool IsEmpty => tx.Count == 0;

            public void Zoom(Point topLeft, Point bottomRight)
            {
                var pt1 = PixelToData(topLeft);
                var pt2 = PixelToData(bottomRight);
                Zoom(pt1.X, pt2.X, pt2.Y, pt1.Y);
            }

            public void Zoom(double xmin, double xmax, double ymin, double ymax)
            {
                tx.Add(new LinearTransform(xmin, xmax, 0, lastColumn));
                ty.Add(new LinearTransform(ymin, ymax, lastRow, 0));
            }

            public void Unzoom(bool singleStep)
            {
                while (tx.Count > 1)
                {
                    tx.RemoveAt(tx.Count - 1);
                    ty.RemoveAt(ty.Count - 1);

                    if (singleStep)
                        break;
                }
            }

            public Point DataToPixel(Point pt)
            {
                int i = tx.Count - 1;
                return new Point(tx[i].Transform(pt.X), ty[i].Transform(pt.Y));
            }

            public Point PixelToData(Point pt)
            {
                int i = tx.Count - 1;
                return new Point(tx[i].BackTransform(pt.X), ty[i].BackTransform(pt.Y));
            }
        }

        #endregion Zooming
    }
}
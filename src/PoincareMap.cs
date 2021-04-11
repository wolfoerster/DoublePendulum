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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using WFTools3D;

namespace DoublePendulum
{
    public class PoincareMap : Border
    {
        public PoincareMap()
        {
            Background = Brushes.Black;
            Child = image = new Image();
            image.Stretch = Stretch.None;
            SizeChanged += MySizeChanged;
            RenderOptions.SetEdgeMode(image, EdgeMode.Aliased);
        }
        Image image;
        List<PendulumData> dataList = new List<PendulumData>();

        /// <summary>
        /// Gets or sets the current simulation data.
        /// </summary>
        public PendulumData Data { get; set; }

        /// <summary>
        /// Copies the current simulation data to the data list.
        /// </summary>
        public void CloneData()
        {
            dataList.Add(Data.Clone());
        }

        /// <summary>
        /// If true, points are reflected across the L1 axis.
        /// </summary>
        public bool DoReflect = true;

        /// <summary>
        /// If true, points for the current simulation are highlighted.
        /// </summary>
        public bool DoHighlight = false;

        /// <summary>
        /// Scaling factors for the conversion of dips to pixels.
        /// </summary>
        private double dpiScaleX, dpiScaleY;

        public int PixelWidth => bitmap.PixelWidth;

        public int PixelHeight => bitmap.PixelHeight;

        /// <summary>
        /// Called when the size has changed.
        /// </summary>
        void MySizeChanged(object sender, SizeChangedEventArgs e)
        {
            Point dpi = WFUtils.GetResolution(this);
            dpiScaleX = dpi.X / 96.0;
            dpiScaleY = dpi.Y / 96.0;

            int width = (int)(ActualWidth * dpiScaleX);
            int height = (int)(ActualHeight * dpiScaleY);

            bitmap = new WriteableBitmap(width, height, dpi.X, dpi.Y, PixelFormats.Pbgra32, null);
            image.Source = bitmap;

            pixelMapper.Init(this);
            Redraw();
        }
        WriteableBitmap bitmap;
        PixelMapper pixelMapper = new PixelMapper();

        public void Clear()
        {
            pixelMapper.Init(this);
            dataList.Clear();
            bitmap.Clear();
        }

        public void Redraw()
        {
            bitmap.Lock();
            bitmap.Clear();

            foreach (var data in dataList)
                ShowData(data, false);

            ShowData(Data, DoHighlight);

            bitmap.Unlock();
        }

        void ShowData(PendulumData data, bool bigPoints)
        {
            if (data.PoincarePoints.Count == 0)
                return;

            bitmap.Lock();

            foreach (var point in data.PoincarePoints)
                AddPoint(point, data.Color, bigPoints);

            bitmap.Unlock();
        }

        public void NewPoincarePoint()
        {
            AddPoint(Data.PoincarePoints[Data.PoincarePoints.Count - 1], Data.Color, DoHighlight);
        }

        void AddPoint(PoincarePoint pp, Color color, bool bigPoint)
        {
            bitmap.Lock();
            AddPoint(pp.Q1, pp.L1, color, bigPoint);

            if (DoReflect)
                AddPoint(-pp.Q1, pp.L1, color, bigPoint);

            bitmap.Unlock();
        }

        void AddPoint(double q, double l, Color color, bool bigPoint)
        {
            Point pt = pixelMapper.DataToPixel(new Point(q, l));
            int x = (int)Math.Round(pt.X);
            int y = (int)Math.Round(pt.Y);
            if (bigPoint)
                bitmap.FillRectangle(x - 1, y - 1, x + 2, y + 2, color);
            else
                bitmap.DrawRectangle(x, y, x + 1, y + 1, color);
        }

        public Point DipToData(Point pt)
        {
            var pixel = DipToPixel(pt);
            return pixelMapper.PixelToData(pixel);
        }

        private Point DipToPixel(Point pointInDip)
        {
            var pt = pointInDip;
            pt.X *= dpiScaleX;
            pt.Y *= dpiScaleY;
            return pt;
        }

        private Point PixelToDip(Point pointInPixel)
        {
            var pt = pointInPixel;
            pt.X /= dpiScaleX;
            pt.Y /= dpiScaleY;
            return pt;
        }

        #region Zooming

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            CaptureMouse();
            mouseDown = e.GetPosition(this);
            ovr = new OverlayRect(this, mouseDown);
            AdornerLayer.GetAdornerLayer(this).Add(ovr);
        }
        Point mouseDown = new Point(-1, 0);
        OverlayRect ovr;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mouseDown.X > -1 && ovr != null)
                ovr.MoveTo(e.GetPosition(this));
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (mouseDown.X > -1)
            {
                AdornerLayer.GetAdornerLayer(this).Remove(ovr);
                ovr = null;

                e.Handled = true;
                ReleaseMouseCapture();
                Point mouseUp = e.GetPosition(this);

                if (mouseUp.X == mouseDown.X && mouseUp.Y == mouseDown.Y)
                {
                    e.Handled = false;//--- might be handled in the main view
                }
                else if (mouseUp.X > mouseDown.X && mouseUp.Y > mouseDown.Y)
                {
                    var pixelDown = DipToPixel(mouseDown);
                    var pixelUp = DipToPixel(mouseUp);
                    pixelMapper.Zoom(this, pixelDown, pixelUp);
                    Redraw();
                }
                else
                {
                    pixelMapper.Unzoom((mouseDown - mouseUp).Length < 200);
                    Redraw();
                }

                mouseDown.X = -1;
            }
        }

        class OverlayRect : Adorner
        {
            public OverlayRect(UIElement adornedElement, Point pt)
                : base(adornedElement)
            {
                p1 = p2 = pt;
                fill = Brushes.White.Clone();
                fill.Opacity = 0.5;
            }
            Brush fill;
            Point p1, p2;

            public void MoveTo(Point pt)
            {
                p2 = pt;
                InvalidateVisual();
            }

            protected override void OnRender(DrawingContext dc)
            {
                if (p2.X > p1.X && p2.Y > p1.Y)
                    dc.DrawRectangle(fill, null, new Rect(p1, p2));
            }
        }

        class PixelMapper
        {
            List<LinearTransform> tx = new List<LinearTransform>();
            List<LinearTransform> ty = new List<LinearTransform>();

            public void Init(PoincareMap map)
            {
                tx.Clear();
                ty.Clear();
                Zoom(map, -map.Data.Q1Max, map.Data.Q1Max, map.Data.L1Max, -map.Data.L1Max);
            }

            public void Zoom(PoincareMap map, Point pixel1, Point pixel2)
            {
                var data1 = PixelToData(pixel1);
                var data2 = PixelToData(pixel2);
                Zoom(map, data1.X, data2.X, data1.Y, data2.Y);
            }

            private void Zoom(PoincareMap map, double dataX1, double dataX2, double dataY1, double dataY2)
            {
                tx.Add(new LinearTransform(dataX1, dataX2, 0, map.PixelWidth - 1));
                ty.Add(new LinearTransform(dataY1, dataY2, 0, map.PixelHeight - 1));
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

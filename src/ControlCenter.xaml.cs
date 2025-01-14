﻿//******************************************************************************************
// Copyright © 2016 - 2024 Wolfgang Foerster (wolfoerster@gmx.de)
//
// This file is part of the DoublePendulum project which can be found on github.com.
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;
    using WFTools3D;

    public partial class ControlCenter : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private readonly DispatcherTimer Timer = new DispatcherTimer(DispatcherPriority.Render);
        private readonly Pendulum3D pendulum3D;
        private readonly Poincare3D poincare3D = new Poincare3D();
        private readonly Trajectory3D trajectory3D = new Trajectory3D();
        private readonly string dataDirectory = AppDomain.CurrentDomain.BaseDirectory + "Data\\";
        private PendulatorUI selectedPendulatorUI;
        private string selectedEnergy;
        private DateTime t0;
        private Color lastUsedColor = Colors.White;

        public ControlCenter()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MeLoaded;

            MirrorQ = Properties.Settings.Default.MirrorQ;
            mirrorQ.IsChecked = MirrorQ;

            MirrorL = Properties.Settings.Default.MirrorL;
            mirrorL.IsChecked = MirrorL;

            pendulum2D.BeginDrag += Pendulum2D_BeginDrag;
            pendulum2D.IsDragging += Pendulum2D_IsDragging;
            pendulum2D.StartSim += Pendulum2D_StartSim;

            poincare2D.MouseMove += Poincare2D_MouseMove;
            poincare2D.MouseLeftButtonUp += Poincare2D_MouseLeftButtonUp;
            poincare2D.MouseRightButtonUp += Poincare2D_MouseRightButtonUp;
            poincare2D.MouseRightButtonDown += Poincare2D_MouseRightButtonDown;

            var brush1 = Resources["wood1"] as ImageBrush;
            var brush2 = Resources["wood2"] as ImageBrush;
            pendulum3D = new Pendulum3D(brush1, brush2) { Position = new Point3D(0, 0, -1) };
            pendulum3D.Rotation1 = Math3D.RotationZ(-90);

            scene.Lighting.AmbientLight.Color = Color.FromRgb(64, 64, 64);
            scene.Lighting.DirectionalLight1.Direction = new Vector3D(1, 1, -1);
            scene.Lighting.DirectionalLight2.Direction = new Vector3D(-1, -1, 0);

            scene.Camera.Position = new Point3D(-2, -4, 3);
            scene.Camera.LookAt(new Point3D(0, 0, 0));
            scene.Camera.Scale = 0.1;
            scene.MouseLeftButtonUp += Scene_MouseLeftButtonUp;

            Timer.Interval = TimeSpan.FromMilliseconds(30);
            Timer.Tick += Timer_Tick;

            SwitchView("view0");
        }

        public static bool MirrorQ { get; set; }

        public static bool MirrorL { get; set; }

        public ObservableCollection<string> Energies { get; } = new ObservableCollection<string>();

        public string SelectedEnergy
        {
            get => selectedEnergy;
            set
            {
                if (IsBusy)
                {
                    MessageBox.Show("Stop running simulations first!", "Cannot change energy");
                    return;
                }

                if (selectedEnergy != value)
                {
                    selectedEnergy = value;
                    FirePropertyChanged();
                    OnEnergyChanged();
                }
            }
        }

        public ObservableCollection<PendulatorUI> PendulatorUIs { get; } = new ObservableCollection<PendulatorUI>();

        public PendulatorUI SelectedPendulatorUI
        {
            get => selectedPendulatorUI;
            set
            {
                if (selectedPendulatorUI != value)
                {
                    selectedPendulatorUI = value;
                    FirePropertyChanged();
                    OnPendulatorChanged();
                }
            }
        }

        public bool IsBusy => PendulatorUIs.Any(ui => ui.Pendulator.IsBusy);

        public void Shutdown()
        {
            foreach (var pendulatorUI in PendulatorUIs)
            {
                if (pendulatorUI.Pendulator.IsBusy)
                {
                    pendulatorUI.Stop();
                }
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            CheckKey(e.Key);
        }

        private void MeLoaded(object sender, RoutedEventArgs e)
        {
            ReadDataDirectory();
            lbUIs.Focus();
        }

        private void OnButtonSwitchMirror(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton).Name == "mirrorQ")
            {
                MirrorQ ^= true;
                poincare2D.Redraw();
                poincare3D.Redraw();
            }
            else
            {
                MirrorL ^= true;
                poincare3D.Redraw();
            }
        }

        private void OnButtonSwitchMode(object sender, RoutedEventArgs e)
        {
            mode0.IsChecked = mode1.IsChecked = mode2.IsChecked = mode3.IsChecked = false;
            ToggleButton tb = (sender as ToggleButton);
            tb.IsChecked = true;

            trajectory3D.Mode = int.Parse(tb.Name.Substring(4));
        }

        private void OnButtonSwitchView(object sender, RoutedEventArgs e)
        {
            view0.IsChecked = view1.IsChecked = view2.IsChecked = view3.IsChecked = false;
            ToggleButton tb = (sender as ToggleButton);
            tb.IsChecked = true;

            SwitchView(tb.Name);
        }

        private void SwitchView(string tbName)
        {
            App.SelectedPendulum.NewTrajectoryPoint = null;
            modePanel.Visibility = Visibility.Hidden;
            mirrorQ.Visibility = Visibility.Hidden;
            mirrorL.Visibility = Visibility.Hidden;
            scene.Models.Clear();

            if (tbName == "view0")
            {
                poincare2D.Visibility = Visibility.Visible;
                mirrorQ.Visibility = Visibility.Visible;
                scene.Visibility = Visibility.Hidden;
                poincare2D.Redraw();
            }
            else
            {
                poincare2D.Visibility = Visibility.Hidden;

                if (tbName == "view1")
                {
                    mirrorQ.Visibility = Visibility.Visible;
                    mirrorL.Visibility = Visibility.Visible;
                    scene.Models.Add(new AxisModel(2, 0.01, 8));
                    scene.Models.Add(poincare3D);
                    poincare3D.Redraw();
                }
                else if (tbName == "view2")
                {
                    scene.Models.Add(pendulum3D);
                    pendulum3D.Update();
                }
                else
                {
                    modePanel.Visibility = Visibility.Visible;
                    scene.Models.Add(new AxisModel(2, 0.01, 8));
                    scene.Models.Add(trajectory3D);
                    scene.Models.Add(new XYGrid(2));
                    trajectory3D.Init();
                    App.SelectedPendulum.NewTrajectoryPoint = trajectory3D.NewTrajectoryPoint;
                }

                scene.Visibility = Visibility.Visible;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (selectedPendulatorUI != null && selectedPendulatorUI.Pendulator.IsBusy)
            {
                pendulum2D.Update();

                if (scene.Models.Contains(pendulum3D))
                    pendulum3D.Update();

                else if (scene.Models.Contains(trajectory3D))
                    trajectory3D.Update();
            }

            //--- if all pendulators are stopped, pendulator uis need to be updated
            var updateUIs = !IsBusy;

            //--- also after each second uis need to be updated
            var t1 = DateTime.UtcNow;
            if ((int)(t1 - t0).TotalMilliseconds > 980)
            {
                t0 = t1;
                updateUIs = true;
            }

            if (updateUIs)
            {
                var calcsPerSecond = 0;

                foreach (var pendulatorUI in PendulatorUIs)
                {
                    if (pendulatorUI.Pendulator.IsBusy)
                    {
                        pendulatorUI.Update();
                        calcsPerSecond += pendulatorUI.Pendulator.CalcsPerSecond;
                    }
                }

                Title = $"Double Pendulum, {calcsPerSecond / 1000000} calcs/μs";
            }

            //--- if all pendulators are stopped, stop the timer, too
            if (!IsBusy)
            {
                Timer.Stop();
                pendulum3D.Update();
                pendulum2D.Update();
                pendulum2D.IsBusy = false;
            }
        }

        private string Title
        {
            set
            {
                if (Application.Current.MainWindow != null)
                    Application.Current.MainWindow.Title = value;
            }
        }

        private void Pendulum2D_BeginDrag(object sender, EventArgs e)
        {
            // store current pendulum
            var pendulum = App.SelectedPendulum;

            SelectedEnergy = null;
            SelectedPendulatorUI = null;

            // restore current pendulum
            App.SelectedPendulum = pendulum;
            pendulum2D.Update();
            pendulum3D.Update();
        }

        private void Pendulum2D_StartSim(object sender, EventArgs e)
        {
            var pendulum = App.SelectedPendulum;
            AdaptTimeStep(pendulum);
            pendulum.PoincareColor = lastUsedColor;
            SelectedEnergy = AddEnergy(pendulum.E0.ToStringInv());
            StartPendulum(pendulum);
        }

        private void Pendulum2D_IsDragging(object sender, EventArgs e)
        {
            var pendulum = App.SelectedPendulum;
            pendulum3D.Update(pendulum.Q1, pendulum.Q2);
        }

        private void Poincare2D_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedPendulatorUI = null;
            if (selectedEnergy != null)
            {
                var pendulum = new Pendulum { Id = PrepareId, PoincareColor = lastUsedColor };
                pendulum.Init(Double(selectedEnergy));
                App.SelectedPendulum = pendulum;
                PrepareNewSimulation();
                pendulum2D.Update();
            }
        }

        private const int PrepareId = int.MaxValue;

        private void Poincare2D_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.RightButton == MouseButtonState.Pressed && PrepareNewSimulation())
            {
                pendulum2D.Update();
            }
        }

        private void Poincare2D_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedEnergy == null || selectedPendulatorUI != null)
                return;

            if (PrepareNewSimulation())
            {
                var pendulum = App.SelectedPendulum;
                AdaptTimeStep(pendulum);
                StartPendulum(pendulum);
            }
        }

        private void AdaptTimeStep(Pendulum pendulum)
        {
            var isShiftDown = WFUtils.IsShiftDown();
            var isCtrlDown = WFUtils.IsCtrlDown();
            var isAltDown = WFUtils.IsAltDown();

            if (isCtrlDown && isShiftDown && isAltDown)
                pendulum.dT *= 0.125;
            else if (isCtrlDown && isShiftDown)
                pendulum.dT *= 0.25;
            else if (isCtrlDown)
                pendulum.dT *= 0.5;
            else if (isShiftDown)
                pendulum.dT *= 2.0;
        }

        private bool PrepareNewSimulation()
        {
            var pendulum = App.SelectedPendulum;
            if (pendulum.Id != PrepareId)
                return false;

            var pt = poincare2D.GetCoordinates();
            if (pt.X < -pendulum.Q1Max)
            {
                pt.X = -pendulum.Q1Max;
                pt.Y = 0;
            }
            else if (pt.X > pendulum.Q1Max)
            {
                pt.X = pendulum.Q1Max;
                pt.Y = 0;
            }
            else if (pt.Y < -pendulum.L1Max)
            {
                pt.X = 0;
                pt.Y = -pendulum.L1Max;
            }
            else if (pt.Y > pendulum.L1Max)
            {
                pt.X = 0;
                pt.Y = pendulum.L1Max;
            }

            return pendulum.Init(Double(selectedEnergy), pt.X, pt.Y);
        }

        private void StartPendulum(Pendulum pendulum)
        {
            pendulum.Id = PendulatorUIs.Count > 0 ? PendulatorUIs[0].Pendulum.Id + 1 : 0;

            var pendulator = new Pendulator(pendulum);
            var pendulatorUI = new PendulatorUI(pendulator, UICallback);

            AddPendulatorUI(pendulatorUI);

            SelectedPendulatorUI = pendulatorUI;

            poincare2D.Redraw();

            pendulatorUI.Start();
        }

        private void Poincare2D_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.RightButton == MouseButtonState.Pressed)
                return;

            var pt = poincare2D.GetCoordinates();
            var index = poincare2D.GetNearestPendulumIndex(pt, out var _);
            SelectPendulum(index);
        }

        private void Scene_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (scene.Models.Contains(poincare3D))
            {
                var ptScene = e.GetPosition(scene);
                var id = poincare3D.FindHitPendulumId(ptScene);
                if (id > -1)
                {
                    var pendulum = App.Pendulums.FirstOrDefault(p => p.Id == id);
                    var index = App.Pendulums.IndexOf(pendulum);
                    SelectPendulum(index);
                }
            }
        }

        private void SelectPendulum(int index)
        {
            if (index > -1)
            {
                SelectedPendulatorUI = PendulatorUIs[index];
                lastUsedColor = SelectedPendulatorUI.Pendulum.PoincareColor;
                lbUIs.Focus();
            }
        }

        private void ReadDataDirectory()
        {
            if (!Directory.Exists(dataDirectory))
            {
                SelectedEnergy = AddEnergy("0.8");
                return;
            }

            foreach (var dir in Directory.GetDirectories(dataDirectory))
            {
                AddEnergy(Path.GetFileName(dir));
            }

            if (Energies.Count > 0)
                SelectedEnergy = Energies[0];
            else
                SelectedEnergy = AddEnergy("0.8");
        }

        private string AddEnergy(string s)
        {
            // lowest energy first!
            var energy = Double(s);

            for (int i = 0; i < Energies.Count; i++)
            {
                var e = Double(Energies[i]);

                if (Math.Abs(energy - e) <= 1e-12)
                    return Energies[i];

                if (energy < e)
                {
                    Energies.Insert(i, s);
                    return s;
                }
            }

            Energies.Add(s);
            return s;
        }

        private void OnButtonNewEnergy(object sender, RoutedEventArgs e)
        {
            var energy = Double(tbNewEnergy.Text);
            if (energy > 0)
            {
                SelectedEnergy = AddEnergy(energy.ToStringInv());
            }
        }

        private void OnEnergyChanged()
        {
            pendulum2D.Clear();
            poincare2D.Clear();
            poincare3D.Clear();
            PendulatorUIs.Clear();
            App.Pendulums.Clear();

            tbNewEnergy.Text = selectedEnergy;

            if (selectedEnergy == null)
            {
                App.SelectedPendulum.Init(0);
                pendulum2D.Update();
                pendulum3D.Update();
                return;
            }

            var energy = Double(selectedEnergy);
            var dir = Path.Combine(dataDirectory, selectedEnergy);

            if (Directory.Exists(dir))
            {
                var pendulums = CreatePendulums(Directory.GetFiles(dir));
                foreach (var pendulum in pendulums)
                {
                    energy = pendulum.E0;
                    var pendulator = new Pendulator(pendulum);
                    var pendulatorUI = new PendulatorUI(pendulator, UICallback);
                    AddPendulatorUI(pendulatorUI);
                }
            }

#if QuickHack
            Doing so ends up in a 'wrong' pendulum (q2 is 0) when doing a 'Start' in Pendulum2D
            To reproduce: move the second bob straight up (q2 is definitely != 0) and press 'Start'
            => the second bob will start at q2 = 0
            App.SelectedPendulum.Init(energy);
            pendulum2D.Update();
#endif
            poincare2D.Init(energy);
            poincare3D.Redraw();
        }

        private IEnumerable<Pendulum> CreatePendulums(string[] fileNames)
        {
            var tasks = new List<Task>();
            var pendulums = new ConcurrentBag<Pendulum>();

            foreach (var fileName in fileNames)
            {
                var task = Task.Run(() => pendulums.Add(new Pendulum(fileName)));
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            return pendulums;
        }

        private void AddPendulatorUI(PendulatorUI pendulatorUI)
        {
            var pendulum = pendulatorUI.Pendulum;
            pendulum.NewPoincarePoint = NewPoincarePoint;

            // highest id first!
            for (int i = 0; i < PendulatorUIs.Count; i++)
            {
                if (PendulatorUIs[i].Pendulum.Id < pendulum.Id)
                {
                    App.Pendulums.Insert(i, pendulum);
                    PendulatorUIs.Insert(i, pendulatorUI);
                    return;
                }
            }

            App.Pendulums.Add(pendulum);
            PendulatorUIs.Add(pendulatorUI);
        }

        private void NewPoincarePoint(Pendulum pendulum)
        {
            this.Dispatch(() =>
            {
                if (pendulum == App.SelectedPendulum)
                    pendulum2D.NewPoincarePoint();

                if (poincare2D.Visibility == Visibility.Visible)
                    poincare2D.NewPoincarePoint(pendulum);

                if (scene.Models.Contains(poincare3D))
                    poincare3D.NewPoincarePoint(pendulum);
            });
        }

        private void OnPendulatorChanged()
        {
            lbUIs.ScrollIntoView(selectedPendulatorUI);
            var callback = App.SelectedPendulum.NewTrajectoryPoint;
            App.SelectedPendulum.NewTrajectoryPoint = null;
            App.SelectedPendulum = selectedPendulatorUI?.Pendulum ?? new Pendulum();

            pendulum2D.Update();
            pendulum3D.Update();
            trajectory3D.Init();
            App.SelectedPendulum.NewTrajectoryPoint = callback;
        }

        private void UICallback(PendulatorUI pendulatorUI, string methodName)
        {
            var pendulum = pendulatorUI.Pendulum;

            switch (methodName)
            {
                case "ColorChanged":
                    lastUsedColor = pendulum.PoincareColor;
                    poincare2D.Redraw();
                    poincare3D.Redraw(pendulum);
                    break;

                case "OnCheckBoxClicked":
                    poincare2D.Redraw();
                    poincare3D.Redraw();
                    break;

                case "OnButtonSave":
                    Save(pendulum);
                    break;

                case "Start":
                    OnStarted();
                    break;

                case "ChangeId":
                    ChangeId(pendulum);
                    break;
            }
        }

        private void ChangeId(Pendulum pendulum)
        {
            for (int i = 0; i < PendulatorUIs.Count; i++)
            {
                if (PendulatorUIs[i].Pendulum == pendulum)
                {
                    if (i < PendulatorUIs.Count - 1)
                    {
                        var prevId = PendulatorUIs[i + 1].Pendulum.Id;
                        if (prevId < pendulum.Id - 1)
                        {
                            if (PendulatorUIs[i].Pendulator.IsBusy)
                            {
                                MessageBox.Show("Stop simulation first!");
                                return;
                            }

                            DeleteFile(pendulum.Id);
                            pendulum.Id = prevId + 1;
                            Save(pendulum);
                            PendulatorUIs[i].Update();
                        }
                    }

                    return;
                }
            }
        }

        private void CheckKey(Key key)
        {
            if (key == Key.C)
            {
                ComparePendulums();
                return;
            }

            if (selectedPendulatorUI == null)
                return;

            if (key == Key.Space)
            {
                selectedPendulatorUI.Stop();
            }
            else if (key == Key.Delete)
            {
                var ok = MessageBox.Show("Really delete this simulation?", "Delete Simulation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ok == MessageBoxResult.Yes)
                    DeleteSelected();
            }
            else if (key == Key.H || key == Key.M || key == Key.S)
            {
                if (key == Key.H)
                {
                    selectedPendulatorUI.Pendulum.IsHighlighted ^= true;
                    selectedPendulatorUI.cbH.IsChecked = selectedPendulatorUI.Pendulum.IsHighlighted;
                }
                else if (key == Key.M)
                {
                    selectedPendulatorUI.Pendulum.IsMuted ^= true;
                    selectedPendulatorUI.cbM.IsChecked = selectedPendulatorUI.Pendulum.IsMuted;
                }
                else if (key == Key.S)
                {
                    selectedPendulatorUI.Pendulum.IsSoloed ^= true;
                    selectedPendulatorUI.cbS.IsChecked = selectedPendulatorUI.Pendulum.IsSoloed;
                }

                poincare2D.Redraw();
                poincare3D.Redraw();
            }
        }

        private void DeleteSelected()
        {
            var pendulum = selectedPendulatorUI.Pendulum;
            DeleteFile(pendulum.Id);

            App.Pendulums.Remove(pendulum);
            PendulatorUIs.Remove(selectedPendulatorUI);
            poincare2D.Redraw();
            poincare3D.Redraw();
        }

        private void DeleteFile(int pendulumId)
        {
            var directory = Path.Combine(dataDirectory, selectedEnergy);
            var fileName = Path.Combine(directory, $"{selectedEnergy}.{pendulumId:D3}");

            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        private void Save(Pendulum pendulum)
        {
            var directory = Path.Combine(dataDirectory, selectedEnergy);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var fileName = Path.Combine(directory, $"{selectedEnergy}.{pendulum.Id:D3}");
            pendulum.Write(fileName);
        }

        private void OnStarted()
        {
            if (!Timer.IsEnabled)
            {
                t0 = DateTime.UtcNow;
                pendulum2D.IsBusy = true;
                Timer.Start();
            }
        }

        private static double Double(string s)
        {
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                return value;

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return value;

            return 0;
        }

        private void ComparePendulums()
        {
            var pendulums = App.Pendulums.Where(o => o.IsHighlighted).ToList();
            if (pendulums.Count < 2)
            {
                MessageBox.Show("Need at least two highlighted pendulums!");
                return;
            }

            var win = new CompareWindow(pendulums);
            win.Show();
        }
    }
}

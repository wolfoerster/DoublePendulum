//******************************************************************************************
// Copyright © 2016 - 2022 Wolfgang Foerster (wolfoerster@gmx.de)
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private readonly DispatcherTimer Timer = new DispatcherTimer(DispatcherPriority.Render);
        private readonly Pendulum3D pendulum3D;
        private readonly Poincare3D poincare3D = new Poincare3D();
        private readonly Trajectory3D trajectory3D = new Trajectory3D();
        private readonly string dataDirectory;
        private PendulatorUI selectedPendulatorUI;
        private string selectedEnergy;
        private DateTime t0;
        private Color lastUsedColor = Colors.White;

        public ControlCenter()
        {
            dataDirectory = AppDomain.CurrentDomain.BaseDirectory + "Data\\";
            //RenameDirs();
            InitializeComponent();
            DataContext = this;
            Loaded += MeLoaded;

            pendulum2D.BeginDrag += Pendulum2D_BeginDrag;
            pendulum2D.IsDragging += Pendulum2D_IsDragging;
            pendulum2D.StartSim += Pendulum2D_StartSim;

            poincare2D.MouseLeftButtonUp += Poincare2D_MouseLeftButtonUp;
            poincare2D.MouseRightButtonUp += Poincare2D_MouseRightButtonUp;

            var brush1 = Resources["wood1"] as ImageBrush;
            var brush2 = Resources["wood2"] as ImageBrush;
            pendulum3D = new Pendulum3D(brush1, brush2) { Position = new Point3D(0, 0, -1) };
            pendulum3D.Rotation1 = Math3D.RotationZ(-90);

            byte b = 64;
            scene.Lighting.AmbientLight.Color = Color.FromRgb(b, b, b);
            scene.Lighting.DirectionalLight1.Direction = new Vector3D(1, 1, -1);
            scene.Lighting.DirectionalLight2.Direction = new Vector3D(-1, -1, 0);

            scene.Camera.Position = new Point3D(-2, -4, 3);
            scene.Camera.LookAt(new Point3D(0, 0, 0));
            scene.Camera.Scale = 0.1;

            Timer.Interval = TimeSpan.FromMilliseconds(30);
            Timer.Tick += Timer_Tick;

            SwitchView("view0");
        }

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
                    OnPropertyChanged();
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
                    if (App.SelectedPendulum != null)
                        App.SelectedPendulum.NewTrajectoryPoint = null;

                    if (value?.Pendulum != null)
                        App.SelectedPendulum = value?.Pendulum;

                    selectedPendulatorUI = value;
                    lbUIs.ScrollIntoView(value);
                    OnPropertyChanged();
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
                Poincare2D.MirrorQ ^= true;
                poincare2D.Redraw();

                Poincare3D.MirrorQ ^= true;
                poincare3D.Redraw();
            }
            else
            {
                Poincare3D.MirrorL ^= true;
                poincare3D.Redraw();
            }
        }

        private void OnButtonSwitchMode(object sender, RoutedEventArgs e)
        {
            mode1.IsChecked = mode2.IsChecked = mode3.IsChecked = mode4.IsChecked = false;
            ToggleButton tb = (sender as ToggleButton);
            tb.IsChecked = true;

            SwitchMode(tb.Name);
        }

        private void SwitchMode(string tbName)
        {
            trajectory3D.Mode = int.Parse(tbName.Substring(4));
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
            trajectory3D.Mode = 0;
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
                    scene.Models.Add(new AxisModel(2));
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
                    scene.Models.Add(new AxisModel(2));
                    scene.Models.Add(trajectory3D);
                    trajectory3D.Mode = 1;
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

                if (scene.Models.Contains(trajectory3D))
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

                Title = $"Double Pendulum, {calcsPerSecond / 1000} cpms";
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
            SelectedEnergy = null;
            SelectedPendulatorUI = null;
        }

        private void Pendulum2D_StartSim(object sender, EventArgs e)
        {
            var pendulum = App.SelectedPendulum;
            pendulum.PoincareColor = lastUsedColor;
            SelectedEnergy = AddEnergy(pendulum.E0.ToStringInv());
            StartPendulum(pendulum);
        }

        private void Pendulum2D_IsDragging(object sender, EventArgs e)
        {
            var pendulum = App.SelectedPendulum;
            pendulum3D.Update(pendulum.Q1, pendulum.Q2);
        }

        private void Poincare2D_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedEnergy == null)
                return;

            var e0 = Double(selectedEnergy);
            var pt = poincare2D.GetCoordinates();
            var pendulum = new Pendulum { PoincareColor = lastUsedColor };
            pendulum.Init(e0);

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

            if (pendulum.Init(e0, pt.X, pt.Y))
            {
                if (WFUtils.IsShiftDown()) pendulum.dT *= 2.0;
                if (WFUtils.IsCtrlDown()) pendulum.dT *= 0.5;
                StartPendulum(pendulum);
            }
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
            var pt = poincare2D.GetCoordinates();
            var index = poincare2D.GetNearestPendulumIndex(pt, out var _);
            if (index > -1)
            {
                SelectedPendulatorUI = PendulatorUIs[index];
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
                return;

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
            Doing so ends up in a 'wrong' pendulum (q2 is 0) when doing a 'Start' in pendulum2D
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
            if (selectedPendulatorUI == null)
                return;

            pendulum2D.Update();
            pendulum3D.Update();

            App.SelectedPendulum.NewTrajectoryPoint = trajectory3D.NewTrajectoryPoint;
            trajectory3D.Clear();
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

        private void RenameDirs()
        {
            foreach (var dir in Directory.GetDirectories(dataDirectory))
            {
                RenameDir(dir);
            }
        }

        private void RenameDir(string dir)
        {
            foreach (var subDir in Directory.GetDirectories(dir))
            {
                RenameDir(subDir);
            }

            bool Convert(string oldName, out string newName)
            {
                newName = oldName;
                var odir = Path.GetDirectoryName(oldName);
                var name = Path.GetFileName(oldName);

                if (name[0] != 'E' || name.IndexOf('P') < 0)
                    return false;

                name = name.Substring(1).Replace('P', '.');
                newName = Path.Combine(odir, name);
                return true;
            }

            foreach (var file in Directory.GetFiles(dir))
            {
                if (Convert(file, out var newFile))
                    File.Move(file, newFile);
            }

            if (Convert(dir, out var newDir))
                Directory.Move(dir, newDir);
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

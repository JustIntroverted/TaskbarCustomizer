using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using TaskbarCustomizer.Helpers;
using TaskbarCustomizer.TaskSettings;

namespace TaskbarCustomizer {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private bool _running = false;
        private BackgroundWorker _bgWorker;
        private System.Windows.Forms.NotifyIcon _trayIcon;

        private TaskbarElement _taskbar;
        private TaskbarElement _startButton;
        private TaskbarElement _startMenu;
        private TaskbarElement _cortanaButton;
        private TaskbarElement _cortanaSearchMenu;
        private TaskbarElement _mainAppContainer;
        private TaskbarElement _trayIconContainer;
        private TaskbarElement _showDesktopButton;

        private Window _dummyTaskbar;

        private int _taskBarWidth => (int)SliderTaskWidth.Value;

        private Settings _settings = new Settings();

        public MainWindow() {
            InitializeComponent();

            // set up the tray icon
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.Icon = new System.Drawing.Icon("Resources\\icon.ico");
            _trayIcon.Text = this.Title;
            _trayIcon.Visible = true;
            _trayIcon.DoubleClick +=
                delegate (object sender, EventArgs args) {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };

            // grab the handles of everything we'll be tweaking
            _taskbar = new TaskbarElement("Shell_TrayWnd");
            _startButton = new TaskbarElement(_taskbar, "Start", 1);
            _startMenu = new TaskbarElement("Windows.UI.Core.CoreWindow", "Start");
            _cortanaButton = new TaskbarElement(_taskbar, "TrayButton", 1);
            _cortanaSearchMenu = new TaskbarElement("Windows.UI.Core.CoreWindow", "Cortana");
            _mainAppContainer = new TaskbarElement(_taskbar, "ReBarWindow32", 1);
            _trayIconContainer = new TaskbarElement(_taskbar, "TrayNotifyWnd", 1);
            _showDesktopButton = new TaskbarElement(_trayIconContainer, "TrayShowDesktopButtonWClass", 1);

            // set up sliders
            SliderTaskWidth.Maximum = _taskbar.Width;

            // create an instance of a dummy taskbar with some settings
            _dummyTaskbar = new Window();
            _dummyTaskbar.WindowState = WindowState.Normal;
            _dummyTaskbar.WindowStyle = WindowStyle.None;
            _dummyTaskbar.ResizeMode = ResizeMode.NoResize;
            _dummyTaskbar.Width = _taskBarWidth;
            _dummyTaskbar.Height = _taskbar.Height;
            _dummyTaskbar.Top = _taskbar.Top;
            _dummyTaskbar.Left = (_taskbar.Width / 2) - _dummyTaskbar.Width / 2;
            _dummyTaskbar.AllowsTransparency = true;
            _dummyTaskbar.Background = new SolidColorBrush(Color.FromArgb((byte)SliderTaskOpacity.Value, 0, 0, 0));
            _dummyTaskbar.ShowInTaskbar = false;
            _dummyTaskbar.Hide();

            // set up background worker
            _bgWorker = new BackgroundWorker {
                WorkerSupportsCancellation = true
            };

            _bgWorker.DoWork += _bgWorker_DoWork;
        }

        private void _bgWorker_DoWork(object sender, DoWorkEventArgs e) {
            while (_running) {
                Dispatcher.Invoke(() => {
                    if (_running)
                        ApplyStyle();
                });

                System.Threading.Thread.Sleep(100);
            }

            Dispatcher.Invoke(() => {
                ResetStyle();
            });
        }

        public void CreateSettings() {
            Setting setting = new Setting {
                SettingName = "width",
                SettingValue = _taskbar.Width.ToString()
            };
            _settings.AddSetting(setting);

            setting = new Setting {
                SettingName = "opacity",
                SettingValue = SliderTaskOpacity.Value.ToString()
            };
            _settings.AddSetting(setting);

            setting = new Setting {
                SettingName = "showtaskbar",
                SettingValue = ChkTaskbarVisible.IsChecked.ToString()
            };
            _settings.AddSetting(setting);

            setting = new Setting {
                SettingName = "hidestartbutton",
                SettingValue = ChkHideStart.IsChecked.ToString()
            };
            _settings.AddSetting(setting);

            setting = new Setting {
                SettingName = "hideshowdesktopbutton",
                SettingValue = ChkHideShowDesk.IsChecked.ToString()
            };
            _settings.AddSetting(setting);

            _settings.SaveSettings();
        }

        private void ApplyStyle() {
            // make taskbar transparent
            if (ChkTaskbarVisible.IsChecked == true) {
                // show the taskbar
                _taskbar.AccentPolicy.AccentState = Utility.AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
                _taskbar.ApplyAccentPolicy();
            } else {
                // hide the taskbar
                _taskbar.AccentPolicy.AccentState = Utility.AccentState.ACCENT_INVALID_STATE;
                _taskbar.ApplyAccentPolicy();
            }

            // make sure the dummy taskbar maintains position
            _dummyTaskbar.Top = _taskbar.Top;
            _dummyTaskbar.Width = _taskBarWidth;
            _dummyTaskbar.Left = (_taskbar.Width / 2) - _dummyTaskbar.Width / 2;
            _dummyTaskbar.Height = _taskbar.Height;

            // get the offsets of buttons that may or may not be visible
            int offset = (_startButton.IsElementVisible() ? _startButton.Width : 0) +
                         (_cortanaButton.IsElementVisible() ? _cortanaButton.Width : 0);

            // resize the app container and then move it into position
            _mainAppContainer.ResizeElement((int)_dummyTaskbar.Width - _trayIconContainer.Width - offset);
            _mainAppContainer.MoveElement((int)_dummyTaskbar.Left + offset);

            // move the start button into position
            if (_startButton.IsElementVisible())
                _startButton.MoveElement((int)_dummyTaskbar.Left);

            // show or hide the show desktopbutton
            if (ChkHideShowDesk.IsChecked == true)
                _showDesktopButton.HideElement();
            else
                _showDesktopButton.ShowElement();

            // move the cortana button into position
            if (_cortanaButton.IsElementVisible())
                _cortanaButton.MoveElement((int)_dummyTaskbar.Left + offset - _cortanaButton.Width);

            // move the start menu into the correct position
            if (_dummyTaskbar.Top == 0) {
                _startMenu.MoveElement((int)_dummyTaskbar.Left, (int)_dummyTaskbar.Height);
                _cortanaSearchMenu.MoveElement((int)_dummyTaskbar.Left, (int)_dummyTaskbar.Height);
            } else {
                _startMenu.MoveElement((int)_dummyTaskbar.Left);
                _cortanaSearchMenu.MoveElement((int)_dummyTaskbar.Left, (int)_dummyTaskbar.Top - (_cortanaSearchMenu.Height));
            }

            // move the tray icon container into position
            _trayIconContainer.MoveElement((int)_dummyTaskbar.Left + (int)_dummyTaskbar.Width - _trayIconContainer.Width);
        }

        private void ResetStyle() {
            // get the offsets of buttons that may or may not be visible
            int offset = (_startButton.IsElementVisible() ? _startButton.Width : 0) +
                         (_cortanaButton.IsElementVisible() ? _cortanaButton.Width : 0);

            // fix the taskbar opacity
            //TODO (justin): get accent state before application is activated, then set it back to what it was
            _taskbar.AccentPolicy.AccentState = Utility.AccentState.ACCENT_DISABLED;
            _taskbar.ApplyAccentPolicy();

            // return things back to normal
            _startButton.ShowElement();
            _startButton.MoveElement(0);

            // move the start menu into the correct position
            if (_taskbar.Top == 0) {
                _startMenu.MoveElement(0);
                _cortanaSearchMenu.MoveElement(0, (int)_dummyTaskbar.Height);
            } else {
                _startMenu.MoveElement(0);
                _cortanaSearchMenu.MoveElement(0, (int)_taskbar.Top - (_cortanaSearchMenu.Height));
            }

            if (_cortanaButton.IsElementVisible())
                _cortanaButton.MoveElement(offset - _cortanaButton.Width);

            _showDesktopButton.ShowElement();
            _mainAppContainer.MoveElement(offset);
            _mainAppContainer.ResizeElement(_taskbar.Width - _trayIconContainer.Width - offset);
            _trayIconContainer.MoveElement(_taskbar.Width - _trayIconContainer.Width);

            // get rid of the system tray icon
            _trayIcon.Dispose();
        }

        #region window events

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (!File.Exists("settings.txt")) {
                CreateSettings();
            } else {
                _settings.LoadSettings();

                SliderTaskWidth.Value = Convert.ToInt16(_settings.FindSetting("width").SettingValue);
                SliderTaskOpacity.Value = Convert.ToByte(_settings.FindSetting("opacity").SettingValue);
                ChkTaskbarVisible.IsChecked = Convert.ToBoolean(_settings.FindSetting("showtaskbar").SettingValue);
                ChkHideStart.IsChecked = Convert.ToBoolean(_settings.FindSetting("hidestartbutton").SettingValue);
                ChkHideShowDesk.IsChecked = Convert.ToBoolean(_settings.FindSetting("hideshowdesktopbutton").SettingValue);
            }

            _dummyTaskbar?.Show();

            this.Focus();
        }

        private void Window_StateChanged(object sender, EventArgs e) {
            if (this.WindowState == WindowState.Minimized) {
                this.Hide();

                return;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            // stop the background worker
            _running = false;

            _settings.SaveSettings();

            // kill the dummy taskbar
            if (_dummyTaskbar != null)
                _dummyTaskbar.Close();

            ResetStyle();
        }

        #endregion window events

        #region control events

        private void ChkHideStart_Checked(object sender, RoutedEventArgs e) {
            if (_running)
                _startButton.HideElement();
        }

        private void ChkHideStart_Unchecked(object sender, RoutedEventArgs e) {
            if (_running)
                _startButton.ShowElement();
        }

        private void ChkHideShowDesk_Checked(object sender, RoutedEventArgs e) {
            if (_running)
                _showDesktopButton.HideElement();
        }

        private void ChkHideShowDesk_Unchecked(object sender, RoutedEventArgs e) {
            if (_running)
                _showDesktopButton.ShowElement();
        }

        private void SliderTaskOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (_running)
                _dummyTaskbar.Background = new SolidColorBrush(Color.FromArgb((byte)SliderTaskOpacity.Value, 0, 0, 0));
        }

        private void SliderTaskWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (_running)
                ApplyStyle();
        }

        private void ChkTaskbarVisible_Click(object sender, RoutedEventArgs e) {
            if (!_running) return;

            if (ChkTaskbarVisible.IsChecked == true) {
                // show the taskbar
                _taskbar.AccentPolicy.AccentState = Utility.AccentState.ACCENT_DISABLED;
                _taskbar.ApplyAccentPolicy();
            } else {
                //    // hide the taskbar
                _taskbar.AccentPolicy.AccentState = Utility.AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
                _taskbar.ApplyAccentPolicy();
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e) {
            if (!_bgWorker.IsBusy) {
                _running = true;
                _bgWorker.RunWorkerAsync();
            }

            BtnStart.Visibility = Visibility.Hidden;
            BtnStop.Visibility = Visibility.Visible;
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e) {
            _running = false;

            BtnStart.Visibility = Visibility.Visible;
            BtnStop.Visibility = Visibility.Hidden;
        }

        #endregion control events
    }
}
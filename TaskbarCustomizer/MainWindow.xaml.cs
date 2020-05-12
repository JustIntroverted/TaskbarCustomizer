using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using TaskbarCustomizer.Helpers;
using TaskbarCustomizer.TaskSettings;
using TaskbarCustomizer.Taskbars.Elements;

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
        private TaskbarElement _volumeContainer;
        private TaskbarElement _showDesktopButton;
        
        private Window _dummyTaskbar;

        private int _taskBarWidth => (int)SliderTaskWidth.Value;

        private Settings _settings = new Settings();

        public MainWindow() {
            InitializeComponent();
        }

        private void _bgWorker_DoWork(object sender, DoWorkEventArgs e) {
            while (_running) {
                Dispatcher.Invoke(() => {
                    if (_running && _taskbar.GetHandle() != IntPtr.Zero)
                        try {
                            ApplyStyle();
                            //Utility.SetWindowPos((IntPtr)0x00001e24, 100, 100, 0, 0, 0, Utility.SWP_NOSIZE | Utility.SWP_NOACTIVATE);
                        } catch (Exception ex) {
                            using (StreamWriter sw = new StreamWriter("debug.txt")) {
                                sw.WriteLine(ex);
                            }
                        }
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
                SettingValue = _taskbar.GetWidth().ToString()
            };
            _settings.AddSetting(setting);

            setting = new Setting {
                SettingName = "opacity",
                SettingValue = SliderTaskOpacity.Value.ToString()
            };
            _settings.AddSetting(setting);

            setting = new Setting {
                SettingName = "showtaskbar",
                SettingValue = CheckTaskbarVisible.IsChecked.ToString()
            };
            _settings.AddSetting(setting);

            setting = new Setting {
                SettingName = "hidestartbutton",
                SettingValue = CheckHideStart.IsChecked.ToString()
            };
            _settings.AddSetting(setting);

            setting = new Setting {
                SettingName = "hideshowdesktopbutton",
                SettingValue = CheckHideShowDesk.IsChecked.ToString()
            };
            _settings.AddSetting(setting);

            setting = new Setting {
                SettingName = "autostart",
                SettingValue = CheckAutoStart.IsChecked.ToString()
            };
            _settings.AddSetting(setting);

            setting = new Setting {
                SettingName = "launchwithwindows",
                SettingValue = CheckLaunchWithWindows.IsChecked.ToString()
            };
            _settings.AddSetting(setting);

            _settings.SaveSettings();
        }

        private void ApplyStyle() {
            // make taskbar transparent
            if (CheckTaskbarVisible.IsChecked == true) {
                // show the taskbar
                _taskbar.AccentPolicy.AccentState = Utility.AccentState.ACCENT_DISABLED;
                _taskbar.ApplyAccentPolicy();
            } else {
                // hide the taskbar
                _taskbar.AccentPolicy.AccentState = Utility.AccentState.ACCENT_INVALID_STATE;
                _taskbar.ApplyAccentPolicy();
            }

            // make sure the dummy taskbar maintains position
            _dummyTaskbar.Top = _taskbar.GetTop();
            _dummyTaskbar.Width = _taskBarWidth;
            _dummyTaskbar.Left = (_taskbar.GetWidth() / 2) - _dummyTaskbar.Width / 2;
            _dummyTaskbar.Height = _taskbar.GetHeight();
            _dummyTaskbar.Background = new SolidColorBrush(Color.FromArgb((byte)SliderTaskOpacity.Value, 0, 0, 0));

            // get the offsets of buttons that may or may not be visible
            int offset = (_startButton.IsElementVisible() ? _startButton.GetWidth() : 0) +
                         (_cortanaButton.IsElementVisible() ? _cortanaButton.GetWidth() : 0);

            // resize the app container and then move it into position
            _mainAppContainer.ResizeElement((int)_dummyTaskbar.Width - _trayIconContainer.GetWidth() - offset);
            _mainAppContainer.MoveElement((int)_dummyTaskbar.Left + offset);

            // move the start button into position
            if (_startButton.IsElementVisible())
                _startButton.MoveElement((int)_dummyTaskbar.Left);

            // show or hide the show desktopbutton
            if (CheckHideShowDesk.IsChecked == true)
                _showDesktopButton.HideElement();
            else
                _showDesktopButton.ShowElement();

            // move the cortana button into position
            if (_cortanaButton.IsElementVisible())
                _cortanaButton.MoveElement((int)_dummyTaskbar.Left + offset - _cortanaButton.GetWidth());

            // move the start menu into the correct position
            if (_dummyTaskbar.Top == 0) {
                _startMenu.MoveElement((int)_dummyTaskbar.Left, (int)_dummyTaskbar.Height);
                _cortanaSearchMenu.MoveElement((int)_dummyTaskbar.Left, (int)_dummyTaskbar.Height);
            } else {
                _startMenu.MoveElement((int)_dummyTaskbar.Left);
                _cortanaSearchMenu.MoveElement((int)_dummyTaskbar.Left, (int)_dummyTaskbar.Top - (_cortanaSearchMenu.GetHeight()));
            }

            // move the tray icon container into position
            _trayIconContainer.MoveElement((int)_dummyTaskbar.Left + (int)_dummyTaskbar.Width - _trayIconContainer.GetWidth());
        }

        private void ResetStyle() {
            // get the offsets of buttons that may or may not be visible
            int offset = (_startButton.IsElementVisible() ? _startButton.GetWidth() : 0) +
                         (_cortanaButton.IsElementVisible() ? _cortanaButton.GetWidth() : 0);

            // fix the taskbar opacity
            //TODO (justin): get accent state before application is activated, then set it back to what it was
            _taskbar.AccentPolicy.AccentState = Utility.AccentState.ACCENT_DISABLED;
            _taskbar.ApplyAccentPolicy();

            // return things back to normal
            _startButton.ShowElement();
            _startButton.MoveElement(0);

            // move the start menu into the correct position
            if (_taskbar.GetTop() == 0) {
                _startMenu.MoveElement(0);
                _cortanaSearchMenu.MoveElement(0, (int)_dummyTaskbar.Height);
            } else {
                _startMenu.MoveElement(0);
                _cortanaSearchMenu.MoveElement(0, (int)_taskbar.GetTop() - (_cortanaSearchMenu.GetHeight()));
            }

            if (_cortanaButton.IsElementVisible())
                _cortanaButton.MoveElement(offset - _cortanaButton.GetWidth());

            _showDesktopButton.ShowElement();
            _mainAppContainer.MoveElement(offset);
            _mainAppContainer.ResizeElement(_taskbar.GetWidth() - _trayIconContainer.GetWidth() - offset);
            _trayIconContainer.MoveElement(_taskbar.GetWidth() - _trayIconContainer.GetWidth());

            // get rid of the system tray icon
            _trayIcon.Dispose();
        }

        #region window events

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            try {
                // set up the tray icon
                System.Windows.Forms.NotifyIcon notifyIcon = _trayIcon = new System.Windows.Forms.NotifyIcon {
                    Icon = new System.Drawing.Icon("Resources\\icon.ico"),
                    Text = Title,
                    Visible = true
                };
                _trayIcon.DoubleClick +=
                    delegate (object sender2, EventArgs args) {
                        Show();
                        WindowState = WindowState.Normal;
                    };

                // grab the handles of everything we'll be tweaking
                _taskbar = new TaskbarElement("Shell_TrayWnd");
                _startButton = new TaskbarElement(_taskbar, "Start", 1);
                _startMenu = new TaskbarElement("Windows.UI.Core.CoreWindow", "Start");
                _cortanaButton = new TaskbarElement(_taskbar, "TrayButton", 1);
                _cortanaSearchMenu = new TaskbarElement("Windows.UI.Core.CoreWindow", "Cortana");
                _mainAppContainer = new TaskbarElement(_taskbar, "ReBarWindow32", 1);
                _trayIconContainer = new TaskbarElement(_taskbar, "TrayNotifyWnd", 1);
                //_volumeContainer = new TaskbarElement(_taskbar, "")
                _showDesktopButton = new TaskbarElement(_trayIconContainer, "TrayShowDesktopButtonWClass", 1);

                // set up sliders
                SliderTaskWidth.Maximum = _taskbar.GetWidth();

                //if (CheckAutoStart.IsChecked == true) {
                // create an instance of a dummy taskbar with some settings
                _dummyTaskbar = new Window();
                _dummyTaskbar.WindowState = WindowState.Normal;
                _dummyTaskbar.WindowStyle = WindowStyle.None;
                _dummyTaskbar.ResizeMode = ResizeMode.NoResize;
                _dummyTaskbar.Width = _taskBarWidth;
                _dummyTaskbar.Height = _taskbar.GetHeight();
                _dummyTaskbar.Top = _taskbar.GetTop();
                _dummyTaskbar.Left = (_taskbar.GetWidth() / 2) - _dummyTaskbar.Width / 2;
                _dummyTaskbar.AllowsTransparency = true;
                _dummyTaskbar.Background = new SolidColorBrush(Color.FromArgb((byte)SliderTaskOpacity.Value, 0, 0, 0));
                _dummyTaskbar.ShowInTaskbar = false;
                _dummyTaskbar.Hide();
                //}
            } catch (Exception ex) {
                using (StreamWriter sw = new StreamWriter("debug.txt")) {
                    sw.WriteLine(ex);
                }
            }

            if (!File.Exists("settings.txt")) {
                _settings.LoadSettings();
                CreateSettings();
            } else {
                _settings.LoadSettings();

                //TODO (justin): add some error checking here
                SliderTaskWidth.Value = Convert.ToInt16(_settings.FindSetting("width").SettingValue);
                SliderTaskOpacity.Value = Convert.ToByte(_settings.FindSetting("opacity").SettingValue);
                CheckTaskbarVisible.IsChecked = Convert.ToBoolean(_settings.FindSetting("showtaskbar").SettingValue);
                CheckHideStart.IsChecked = Convert.ToBoolean(_settings.FindSetting("hidestartbutton").SettingValue);
                CheckHideShowDesk.IsChecked = Convert.ToBoolean(_settings.FindSetting("hideshowdesktopbutton").SettingValue);
                CheckAutoStart.IsChecked = Convert.ToBoolean(_settings.FindSetting("autostart").SettingValue);
                CheckLaunchWithWindows.IsChecked = Convert.ToBoolean(_settings.FindSetting("launchwithwindows").SettingValue);

                //_dummyTaskbar.Background = new SolidColorBrush(Color.FromArgb((byte)SliderTaskOpacity.Value, 0, 0, 0));
            }

            _dummyTaskbar?.Show();

            Focus();

            // set up background worker
            _bgWorker = new BackgroundWorker {
                WorkerSupportsCancellation = true
            };
            _bgWorker.DoWork += _bgWorker_DoWork;


            // TODO: create method for the button click action
            //       because this is stupid
            if (CheckAutoStart.IsChecked == true)
                ButtonStart_Click(null, null);
        }

        private void Window_StateChanged(object sender, EventArgs e) {
            if (WindowState == WindowState.Minimized) {
                Hide();

                return;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            // stop the background worker
            _running = false;

            _settings.UpdateSetting(new Setting("width", ((int)SliderTaskWidth.Value).ToString()));
            _settings.UpdateSetting(new Setting("opacity", ((int)SliderTaskOpacity.Value).ToString()));
            _settings.UpdateSetting(new Setting("showtaskbar", CheckTaskbarVisible.IsChecked.ToString()));
            _settings.UpdateSetting(new Setting("hidestartbutton", CheckHideStart.IsChecked.ToString()));
            _settings.UpdateSetting(new Setting("hideshowdesktopbutton", CheckHideShowDesk.IsChecked.ToString()));
            _settings.UpdateSetting(new Setting("autostart", CheckAutoStart.IsChecked.ToString()));
            _settings.UpdateSetting(new Setting("launchwithwindows", CheckLaunchWithWindows.IsChecked.ToString()));

            _settings.SaveSettings();

            if (CheckLaunchWithWindows.IsChecked == true) {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)) {
                    key.SetValue("TaskbarCustomizer", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
                }
            } else {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)) {
                    key.DeleteValue("TaskbarCustomizer", false);
                }
            }

            // kill the dummy taskbar
            if (_dummyTaskbar != null)
                _dummyTaskbar.Close();

            ResetStyle();
        }

        #endregion window events

        #region control events

        private void CheckHideStart_Checked(object sender, RoutedEventArgs e) {
            if (_running)
                _startButton.HideElement();
        }

        private void CheckHideStart_Unchecked(object sender, RoutedEventArgs e) {
            if (_running)
                _startButton.ShowElement();
        }

        private void CheckHideShowDesk_Checked(object sender, RoutedEventArgs e) {
            if (_running)
                _showDesktopButton.HideElement();
        }

        private void CheckHideShowDesk_Unchecked(object sender, RoutedEventArgs e) {
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

        private void CheckTaskbarVisible_Click(object sender, RoutedEventArgs e) {
            if (!_running) return;

            if (CheckTaskbarVisible.IsChecked == true) {
                // show the taskbar
                _taskbar.AccentPolicy.AccentState = Utility.AccentState.ACCENT_DISABLED;
                _taskbar.ApplyAccentPolicy();
            } else {
                // hide the taskbar
                _taskbar.AccentPolicy.AccentState = Utility.AccentState.ACCENT_INVALID_STATE;
                _taskbar.ApplyAccentPolicy();
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e) {
            if (!_bgWorker.IsBusy && !_running) {
                _running = true;
                _bgWorker.RunWorkerAsync();
                ButtonStart.Content = "Stop";
            } else {
                _running = false;
                ButtonStart.Content = "Start";
            }
        }

        #endregion control events

        private void CheckLaunchWithWindows_Checked(object sender, RoutedEventArgs e) {
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)) {
                key.SetValue("TaskbarCustomizer", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            }
        }

        private void CheckLaunchWithWindows_Unchecked(object sender, RoutedEventArgs e) {
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)) {
                key.DeleteValue("TaskbarCustomizer", false);
            }
        }
    }
}
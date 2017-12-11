using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using TaskbarCustomizer.Helpers;

namespace TaskbarCustomizer {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
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

        private int _taskBarWidth => (int)sliderTaskWidth.Value;

        private IntPtr _winHook;
        private Utility.WinEventDelegate procDelegate;

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
            sliderTaskWidth.Maximum = _taskbar.Width;

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
            _dummyTaskbar.Background = new SolidColorBrush(Color.FromArgb((byte)sliderTaskOpacity.Value, 0, 0, 0));
            _dummyTaskbar.ShowInTaskbar = false;
            _dummyTaskbar.Hide();

            // create wineventhook
            procDelegate = new Utility.WinEventDelegate(WinEventProc);
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            if (eventType == 0x0008 || eventType == 0x0009 || eventType == 0x0005 || eventType == 0x0003 || eventType == 0x0010 || eventType == 0x0011 || eventType == 0x8005) {
                applyStyle();
                System.Threading.Thread.Sleep(80);
            }

            Debug.WriteLine(eventType);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if (msg == Utility.WM_DWMCOLORIZATIONCOLORCHANGED) {
                System.Threading.Thread.Sleep(10);

                // make taskbar transparent
                _taskbar.AccentPolicy.AccentState = Helpers.Utility.AccentState.ACCENT_INVALID_STATE;
                _taskbar.ApplyAccentPolicy();
            }

            return IntPtr.Zero;
        }

        private void applyStyle() {
            // make taskbar transparent
            _taskbar.AccentPolicy.AccentState = Helpers.Utility.AccentState.ACCENT_INVALID_STATE;
            _taskbar.ApplyAccentPolicy();

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

            // move the cortana button into position
            if (_cortanaButton.IsElementVisible())
                _cortanaButton.MoveElement((int)_dummyTaskbar.Left + offset - _cortanaButton.Width);

            // move the start menu into the correct position
            _startMenu.MoveElement((int)_dummyTaskbar.Left, (int)_dummyTaskbar.Height);
            _cortanaSearchMenu.MoveElement((int)_dummyTaskbar.Left, (int)_dummyTaskbar.Height);
            //_networkMenu.MoveElement((int)_dummyTaskbar.Left + (int)_dummyTaskbar.Width - _networkMenu.Width, (int)_dummyTaskbar.Height);

            // move the tray icon container into position
            _trayIconContainer.MoveElement((int)_dummyTaskbar.Left + (int)_dummyTaskbar.Width - _trayIconContainer.Width);

            //System.Threading.Thread.Sleep(10);
        }

        private void chkHideStart_Checked(object sender, RoutedEventArgs e) {
            _startButton.HideElement();
        }

        private void chkHideStart_Unchecked(object sender, RoutedEventArgs e) {
            _startButton.ShowElement();
        }

        private void chkHideShowDesk_Checked(object sender, RoutedEventArgs e) {
            _showDesktopButton.HideElement();
        }

        private void chkHideShowDesk_Unchecked(object sender, RoutedEventArgs e) {
            _showDesktopButton.ShowElement();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            _dummyTaskbar.Show();

            this.Focus();

            applyStyle();

            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
            HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
            mainWindowSrc.AddHook(WndProc);

            _winHook = Utility.SetWinEventHook(Utility.EVENT_MIN, Utility.EVENT_MAX, IntPtr.Zero, procDelegate, 0, 0, Utility.WINEVENT_OUTOFCONTEXT);

            sliderTaskWidth.Value = sliderTaskWidth.Maximum - (sliderTaskWidth.Maximum / 2);
            sliderTaskOpacity.Value = sliderTaskOpacity.Maximum / 2;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            // get the offsets of buttons that may or may not be visible
            int offset = (_startButton.IsElementVisible() ? _startButton.Width : 0) +
                         (_cortanaButton.IsElementVisible() ? _cortanaButton.Width : 0);

            Utility.UnhookWinEvent(_winHook);

            // fix the taskbar
            _taskbar.AccentPolicy.AccentState = Helpers.Utility.AccentState.ACCENT_DISABLED;
            _taskbar.ApplyAccentPolicy();

            if (_dummyTaskbar != null)
                _dummyTaskbar.Close();

            // return things back to normal
            _startButton.ShowElement();
            _startButton.MoveElement(0);
            _startMenu.MoveElement(0);

            if (_cortanaButton.IsElementVisible())
                _cortanaButton.MoveElement(offset - _cortanaButton.Width);

            _showDesktopButton.ShowElement();
            _mainAppContainer.MoveElement(offset);
            _mainAppContainer.ResizeElement(_taskbar.Width - _trayIconContainer.Width - offset);
            _trayIconContainer.MoveElement(_taskbar.Width - _trayIconContainer.Width);

            // get rid of the system tray icon
            _trayIcon.Dispose();
        }

        private void sliderTaskOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            _dummyTaskbar.Background = new SolidColorBrush(Color.FromArgb((byte)sliderTaskOpacity.Value, 0, 0, 0));
        }

        private void sliderTaskWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (this.Visibility == Visibility.Visible)
                applyStyle();
        }

        private void Window_StateChanged(object sender, EventArgs e) {
            if (this.WindowState == WindowState.Minimized) {
                this.Hide();

                return;
            }
        }
    }
}
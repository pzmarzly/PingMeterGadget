using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace PingMeterGadget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Helper fields and methods

        #region Colors definition
        /// <summary>This color is used when ping is less than 80 ms</summary>
        Brush COLOR_OK = Brushes.ForestGreen;
        /// <summary>This color is used when ping is between 80 and 150 ms</summary>
        Brush COLOR_WARN = Brushes.Gold;
        /// <summary>This color is used when client is not connected to a remote server/summary>
        Brush COLOR_IDLE = Brushes.Black;
        /// <summary>This color is used when ping is above 150 ms</summary>
        Brush COLOR_ERR = Brushes.Red;
        #endregion

        #region label:c_ping manipulation
        /// <summary>
        /// Resets c_ping label. Called on configuration
        /// </summary>
        void UpdatePing()
        {
            UpdatePing("--", COLOR_IDLE);
        }

        /// <summary>
        /// Chooses font color and update ping value. Called on ping reply
        /// </summary>
        /// <param name="ping">Ping to display</param>
        void UpdatePing(long ping)
        {
            Brush b;
            if (ping < ProgramConfig.limitOK) b = COLOR_OK;
            else if (ping < ProgramConfig.limitWarn) b = COLOR_WARN;
            else b = COLOR_ERR;
            UpdatePing(ping.ToString(), b);
        }

        /// <summary>
        /// Internal: Manipulates the c_ping element
        /// </summary>
        /// <param name="ping">Text to display</param>
        /// <param name="brush">Text color</param>
        void UpdatePing(string ping, Brush brush)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                c_ping.Content = ping;
                c_ping.Foreground = brush;
            }));
        }
        #endregion

        #region label:c_status manipulation
        /// <summary>A variable that holds copy of c_status.Content. It's used to prevent calling Dispatcher.</summary>
        string curr_status = "";
        /// <summary>
        /// Updates c_status
        /// </summary>
        /// <param name="text">Text</param>
        void UpdateStatus(string text)
        {
            if (text != curr_status)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                    c_status.Content = text;
                }));
                curr_status = text;
            }
        }
        #endregion

        #endregion

        #region Application flow

        #region GUI thread

        /// <summary>Stores config and allows file saving.</summary>
        UserConfigHelper cfgHelper;
        /// <summary>Second thread, pings server.</summary>
        Thread worker;
        /// <summary>Pauses second thread execution.</summary>
        bool pauseLoop = false;
        /// <summary>Current instance ID's, may be dispalyed.</summary>
        string instanceId;
        /// <summary>Current executable's name, prefix in configuration files' names.</summary>
        string exeName;

        /// <summary>Initializes window, asks for config and launches loop.</summary>
        private void InitializeApp()
        {
            Application.Current.Exit += ApplicationExit;
            c_closebtn.Click += CloseBtn;
            UpdatePing();

            exeName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            instanceId = Process.GetProcessesByName(exeName).Count().ToString();
            cfgHelper = new UserConfigHelper(exeName, instanceId);
            if (cfgHelper.cfg.eachTimeConfig)
            {
                DoConfig();
            }
            else
            {
                while (cfgHelper.cfg.firstConfig)
                {
                    DoConfig();
                }
            }

            ParseConfig();

            Left = cfgHelper.cfg.lastX;
            Top = cfgHelper.cfg.lastY;

            c_settingsbtn.Click += SettingsBtn;
            c_clonebtn.Click += CloneBtn;

            worker = new Thread(Loop);
            worker.Start();
        }

        /// <summary>Called on exit. Saves window position and exits application.</summary>
        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            cfgHelper.cfg.lastX = Left;
            cfgHelper.cfg.lastY = Top;
            cfgHelper.Save();
            pauseLoop = true;
            worker.Abort();
            Thread.Sleep(ProgramConfig.timeExit);
        }

        /// <summary>Calls CfgDialos and saves results, doesn't update window.</summary>
        private void DoConfig()
        {
            UpdatePing();
            UpdateStatus("Config...");
            CfgDialog dialog = new CfgDialog(cfgHelper.cfg);
            if (dialog.ShowDialog() == true)
            {
                cfgHelper.cfg = dialog.NewConfig;
                cfgHelper.cfg.firstConfig = false;
                cfgHelper.Save();
            }
        }

        /// <summary>Updates window with config got by DoConfig(). Also part of initialization.</summary>
        private void ParseConfig()
        {
            c_closebtn.IsCancel = cfgHelper.cfg.escapeToExit;
            c_instance.Content = cfgHelper.cfg.showId ? "#" + instanceId : "";
            Topmost = cfgHelper.cfg.alwaysOnTop;
            // check value to prevent crash
            if (cfgHelper.cfg.opacity > 100)
            {
                cfgHelper.cfg.opacity = 100;
                cfgHelper.Save();
            }
            else if (cfgHelper.cfg.opacity < 1)
            {
                cfgHelper.cfg.opacity = 10;
                cfgHelper.Save();
            }
            Opacity = (double)cfgHelper.cfg.opacity / 100;
        }

        #endregion

        #region Second thread

        /// <summary>Main loop, pings.</summary>
        private void Loop()
        {
            Ping pinger = new Ping();
            while (true)
            {
                int timeToSleep = cfgHelper.cfg.timeNormal;
                if (!pauseLoop)
                {
                    UpdateStatus(cfgHelper.cfg.favServer);
                    try
                    {
                        PingReply r = pinger.Send(cfgHelper.cfg.favServer);
                        if (!pauseLoop)
                        {
                            if (r.Status == IPStatus.Success)
                            {
                                if (r.RoundtripTime > ProgramConfig.limitSlow) timeToSleep = cfgHelper.cfg.timeHigh;
                                UpdatePing(r.RoundtripTime);
                            }
                            else
                            {
                                UpdatePing(r.Status.ToString(), COLOR_ERR);
                                timeToSleep = ProgramConfig.timeError;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        if (!pauseLoop) UpdatePing("Exception!", COLOR_ERR);
                        timeToSleep = ProgramConfig.timeException;
                    }
                }
                Thread.Sleep(timeToSleep);
            }
        }

        #endregion

        #region WPF init

        /// <summary>Constructor, calls InitializeApp().</summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializeApp();
        }

        /// <summary>Allows dragging.</summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        #endregion

        #endregion

        #region Buttons and context menu

        /// <summary>Fired when + button is pressed. Launches new instance of application.</summary>
        private void CloneBtn(object sender, RoutedEventArgs e)
        {
            Process.Start(Assembly.GetEntryAssembly().Location);
            Thread.Sleep(ProgramConfig.timeClone);
        }

        /// <summary>Fired when close button is pressed or when user chooses to close the window from context menu. Closes window.</summary>
        private void CloseBtn(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>Fired when settings button is pressed. Pauses the loop and launches options dialog.</summary>
        private void SettingsBtn(object sender, RoutedEventArgs e)
        {
            pauseLoop = true;
            DoConfig();
            ParseConfig();
            pauseLoop = false;
        }

        /// <summary>Fired when user chooses to close all windows from context menu. Closes all windows.</summary>
        private void MenuItem_KillAll(object sender, RoutedEventArgs e)
        {
            Process[] pr;
            while ((pr = Process.GetProcessesByName(exeName)).Count() > 1)
            {
                foreach (Process p in pr)
                {
                    p.CloseMainWindow();
                }
                Thread.Sleep(50);
            }
            Close();
        }

        #endregion
    }
}

using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace PingMeterGadget
{
    /// <summary>
    /// Interaction logic for CfgDialog.xaml
    /// </summary>
    public partial class CfgDialog : Window
    {
        /// <summary>Object from savefile</summary>
        UserConfig cfg;

        /// <summary>
        /// Creates window and fills it with old values.
        /// </summary>
        /// <param name="old">Object with old values.</param>
        public CfgDialog(UserConfig old)
        {
            cfg = old;
            InitializeComponent();

            s_favServer.Text = cfg.favServer;
            if (cfg.eachTimeConfig)   s_eachTime.IsChecked = true;
            if (cfg.escapeToExit)     s_escape.IsChecked = true;
            if (cfg.showId)           s_instanceId.IsChecked = true;
            if (cfg.rememberLocation) s_rememberPos.IsChecked = true;
            if (cfg.alwaysOnTop)      s_onTop.IsChecked = true;
            s_opacity.Text = cfg.opacity.ToString();
            s_timeNormal.Text = cfg.timeNormal.ToString();
            s_timeHigh.Text = cfg.timeHigh.ToString();

            btn_save.Click += DoSave;
            btn_cancel.Click += DontSave;
        }

        /// <summary>New UserConfig object with user-provided values.</summary>
        public UserConfig NewConfig
        {
            get
            {
                cfg.favServer = s_favServer.Text;
                cfg.eachTimeConfig   = s_eachTime.IsChecked == true;
                cfg.escapeToExit     = s_escape.IsChecked == true;
                cfg.showId           = s_instanceId.IsChecked == true;
                cfg.rememberLocation = s_rememberPos.IsChecked == true;
                cfg.alwaysOnTop      = s_onTop.IsChecked == true;
                int tmp6;
                if (TryParse(s_opacity.Text, out tmp6, 1, 100)) cfg.opacity = tmp6;
                int tmp7;
                if (TryParse(s_timeNormal.Text, out tmp7, 10)) cfg.timeNormal = tmp7;
                int tmp8;
                if (TryParse(s_timeHigh.Text, out tmp8, 10)) cfg.timeHigh = tmp8;

                return cfg;
            }
        }

        /// <summary>
        /// Parses int from string and validates it.
        /// </summary>
        /// <param name="text">String containing number</param>
        /// <param name="parsed"></param>
        /// <param name="min">Lowest correct number, by default 0</param>
        /// <param name="max">Highest correct number, by default int.MaxValue</param>
        /// <returns></returns>
        private bool TryParse(string text, out int parsed, int min = 0, int max = int.MaxValue)
        {
            if (text.Length <= max.ToString().Length)
            {
                int tmp;
                if (int.TryParse(text, out tmp))
                {
                    tmp = System.Math.Abs(tmp);
                    if (tmp >= min && tmp <= max)
                    {
                        parsed = tmp;
                        return true;
                    }
                }
            }
            parsed = 99;
            return false;
        }

        /// <summary>Called when Save button is pressed.</summary>
        private void DoSave(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>Called when Cancel button is pressed.</summary>
        private void DontSave(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>Opens hyperlink in browser.</summary>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Versioning;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Velopack;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UpdateManagerMultipleVersion _um;
        private UpdateInfo _update;

        public MainWindow()
        {
            InitializeComponent();

            string updateUrl = SampleHelper.GetReleasesDir(); // replace with your update url
            _um = new UpdateManagerMultipleVersion(updateUrl, logger: App.Log);
            
            TextLog.Text = App.Log.ToString();
            App.Log.LogUpdated += LogUpdated;
            UpdateStatus();
        }

        private async void BtnCheckUpdateClick(object sender, RoutedEventArgs e)
        {
            Working();
            try
            {
                // ConfigureAwait(true) so that UpdateStatus() is called on the UI thread
                _update = await _um.CheckForUpdatesVersionAsync(new SemanticVersion(1,6,5)).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                App.Log.LogError(ex, "Error checking for updates");
            }
            UpdateStatus();
        }

        private async void BtnDownloadUpdateClick(object sender, RoutedEventArgs e)
        {
            Working();
            try
            {
                // ConfigureAwait(true) so that UpdateStatus() is called on the UI thread
                await _um.DownloadUpdatesAsync(_update, Progress).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                App.Log.LogError(ex, "Error downloading updates");
            }
            UpdateStatus();
        }

        private void BtnRestartApplyClick(object sender, RoutedEventArgs e)
        {
            _um.ApplyUpdatesAndRestart(_update);
        }

        private void LogUpdated(object sender, LogUpdatedEventArgs e)
        {
            // logs can be sent from other threads
            this.Dispatcher.InvokeAsync(() => {
                TextLog.Text = e.Text;
                ScrollLog.ScrollToEnd();
            });
        }

        private void Progress(int percent)
        {
            // progress can be sent from other threads
            this.Dispatcher.InvokeAsync(() => {
                TextStatus.Text = $"Downloading ({percent}%)...";
            });
        }

        private void Working()
        {
            App.Log.LogInformation("");
            BtnCheckUpdate.IsEnabled = false;
            BtnDownloadUpdate.IsEnabled = false;
            BtnRestartApply.IsEnabled = false;
            TextStatus.Text = "Working...";
        }

        private void UpdateStatus()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Velopack version: {VelopackRuntimeInfo.VelopackNugetVersion}");
         
            sb.AppendLine($"This app version: {(_um.IsInstalled ? _um.CurrentVersion?.ToString() : "(n/a - not installed)")}");

            if (_update != null)
            {
                sb.AppendLine($"Update available: {_update.TargetFullRelease.Version}");
                BtnDownloadUpdate.IsEnabled = true;
            }
            else
            {
                BtnDownloadUpdate.IsEnabled = false;
            }

            if (_um.UpdatePendingRestart != null)
            {
                sb.AppendLine("Update ready, pending restart to install");
                BtnRestartApply.IsEnabled = true;
            }
            else
            {
                BtnRestartApply.IsEnabled = false;
            }

            TextStatus.Text = sb.ToString();
            BtnCheckUpdate.IsEnabled = true;
        }
    }
}
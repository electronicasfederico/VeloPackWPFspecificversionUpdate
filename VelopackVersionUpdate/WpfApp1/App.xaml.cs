using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Velopack;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MemoryLogger Log { get; private set; }

        // Since WPF has an "automatic" Program.Main, we need to create our own.
        // In order for this to work, you must also add the following to your .csproj:
        // <StartupObject>CSharpWpf.App</StartupObject>
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                // Logging is essential for debugging! Ideally you should write it to a file.
                Log = new MemoryLogger();

                // It's important to Run() the VelopackApp as early as possible in app startup.
                VelopackApp.Build()
                    .WithFirstRun((v) => { /* Your first run code here */ })
                    .Run(Log);

                // We can now launch the WPF application as normal.
                var app = new App();
                app.InitializeComponent();
                app.Run();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unhandled exception: " + ex.ToString());
            }
        }

    }

}

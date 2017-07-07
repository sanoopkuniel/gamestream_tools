using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        static string launcherConfig = baseDir + "\\launcher.json";

        public MainWindow()
        {
            InitializeComponent();

            this.Visibility = Visibility.Hidden;

            //Check commandline for steam args
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                string strArgs = String.Join(" ", args.Skip(1)).Trim();

                JObject config = JObject.Parse(File.ReadAllText(launcherConfig));

                //strArgs = "steam://rungameid/208650";
                

                if (strArgs.StartsWith("steam://rungameid/"))
                {
                    string gameId = strArgs.Replace("steam://rungameid/", "");
                    if (config[gameId] != null)
                    {
                        gameLabel.Content= config[gameId]["DisplayName"].ToString();
                        this.Show();


                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1)); //Sleep for 1s

                        if (!String.IsNullOrWhiteSpace(config[gameId]["CommandLine"].ToString()))
                        {
                            var process = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = config[gameId]["CommandLine"].ToString(),
                                    Arguments = config[gameId]["CommandLineArgs"].ToString()
                                }
                            };
                            process.Start();
                            process.WaitForExit();
                        } else
                        {
                            MessageBox.Show("\"CommandLine\" property for gameId " + gameId + " is empty, update launcher.json and try again");
                        }

                    } else
                    {
                        //Start steam

                        string steamExe = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamExe", null);
                        if (steamExe != null)
                        {
                            //Start Steam
                            var process = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = steamExe,
                                    Arguments = strArgs
                                }
                            };
                            process.Start();
                        } else
                        {
                            gameLabel.Content = "Unable to find steam, exiting..";
                            this.Show();

                            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1)); //Sleep for 1s
                        }
                    }
                }

            }
            Application.Current.Shutdown();
        }
    }
}

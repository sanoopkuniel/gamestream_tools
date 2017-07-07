using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using RegistryUtils;
using System;
using System.Collections.Generic;
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

namespace SteamProtoAssist
{
    public partial class MainWindow : Window
    {

        static string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        RegistryMonitor monitor = new RegistryMonitor(RegistryHive.ClassesRoot, @"steam\shell\open\command");

        public MainWindow()
        {
            InitializeComponent();
            this.Hide();

            //string[] args = Environment.GetCommandLineArgs();


            // Register registry listener
            monitor.RegChanged += new EventHandler(OnRegChanged);
            monitor.Start();
        }

        private void OnRegChanged(object sender, EventArgs e)
        {
            // Reset protocol handler
            string launcherPath = baseDir + "\\launcher.exe";
            Util.RegisterProtocol("steam", launcherPath);
        }

        private void btnRegisterProto_Click(object sender, RoutedEventArgs e)
        {
            string launcherPath = baseDir + "\\launcher.exe";
            Util.RegisterProtocol("steam", launcherPath);
            MessageBox.Show("Registered steam:// to " + launcherPath);
        }

        private void btnParseGameStream_Click(object sender, RoutedEventArgs e)
        {
            string gameStreamPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\NVIDIA\\NvBackend\\StreamingAssetsData";
            string[] streamingSettings = Directory.GetFiles(gameStreamPath, "StreamingSettings.json", SearchOption.AllDirectories);



            JObject launcherJson = new JObject();


            foreach (string settingsFile in streamingSettings)
            {

                    JObject game = JObject.Parse(File.ReadAllText(settingsFile));
                    IEnumerable<JToken> steamGame = game.SelectTokens("$.GameData[?(@.SteamAppID > 1)].StreamingDisplayName");

                    foreach (JToken item in steamGame)
                    {
                        string steamID = (String)item.Parent.Parent["SteamAppID"];
                        string steamGameDisplayName = (String)item;
                        JObject gameObj = new JObject(
                                    new JProperty("DisplayName", steamGameDisplayName),
                                    new JProperty("CommandLine", ""),
                                    new JProperty("CommandLineArgs", "")
                                    );
                    try
                    {
                        launcherJson.Add(new JProperty(steamID, gameObj));
                    }
                    catch { }
                    }

            }
            System.IO.File.WriteAllText(baseDir + "\\launcher.json", launcherJson.ToString());
            MessageBox.Show("Launcher Config Generated: " + baseDir + "\\launcher.json");
        }

        private void ctxSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        private void ctxQuit_Click(object sender, RoutedEventArgs e)
        {
            //monitor.Stop();
            Application.Current.Shutdown();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();

            //base.OnClosing(e);
        }

        private void btnAutoStart_Click(object sender, RoutedEventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    rk.SetValue("SteamAssist", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            MessageBox.Show("AutoStart enabled");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using TentLibrary;

namespace TentTestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Functions asyncFunctions;

        public MainWindow()
        {
            InitializeComponent();

            asyncFunctions = new Functions();

            asyncFunctions.GetProfileCompletedHandler += new GetProfileCompletedEventHandler(getProfileCompleted);
            asyncFunctions.GetServersCompletedHandler += new GetServersCompletedEventHandler(getServersCompleted);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> servers = Functions.GetServers(Functions.GetProfile("https://jedlimke.tent.is/"));

                foreach (string s in servers)
                {
                    textBlock1.Text = textBlock1.Text + "\n" + s;
                }
            }
            catch (Exception ex)
            {
                textBlock1.Text = textBlock1.Text + "\nEXCEPTION: " + ex.Message;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            asyncFunctions.GetProfileAsync("https://jedlimke.tent.is/");
        }

        private void getProfileCompleted(object sender, GetProfileCompletedEventArgs e)
        {
            textBlock1.Text = textBlock1.Text + "\nASYNC PROFILE: " + e.Profile;

            // Retrieve the servers once the profile has completed.
            asyncFunctions.GetServersAsync(e.Profile);
        }

        private void getServersCompleted(object sender, GetServersCompletedEventArgs e)
        {
            RegistrationData rd = new RegistrationData();

            rd.Name = "FooApp";
            rd.Description = "Does amazing foos with your data";
            rd.Url = "http://example.com";
            rd.Icon = "http://example.com/icon.png";
            rd.RedirectUris = new string[]{ "https://app.example.com/tent/callback" };
            rd.Scopes = new Scopes();
            rd.Scopes.WriteProfile = "Uses an app profile section to describe foos";
            rd.Scopes.ReadFollowings = "Calculates foos based on your followings";

            foreach (string s in e.Servers)
            {
                textBlock1.Text = textBlock1.Text + "\nASYNC SERVER: " + s;

                textBlock1.Text = textBlock1.Text + "\nBOOM: " + Functions.RegisterApplication(s, rd);
            }
        }
    }
}

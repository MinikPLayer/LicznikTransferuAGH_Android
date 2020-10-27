using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Javax.Crypto;

namespace LTAndroid.Resources.layout
{
    public class SettingsScreen : Android.Support.V4.App.Fragment
    {
        Button saveButton;
        EditText emailField;
        EditText passwordField;
        EditText serverIpField;
        EditText serverPortField;

        public static string username = "";
        public static string password = "";
        public static string serverIP = "minik.ml";
        public static int serverPort = 7154;

        public static string errorDialog = "";

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            
            if(errorDialog.Length > 0)
            {
                MainActivity.singleton.ShowDialog("Błąd", errorDialog, Context);
                errorDialog = "";
            }


        }

        static string GetConfigPath()
        {
            return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "config.ini");
        }

        public static void SaveConfig(string path)
        {
            string data = "version: " + MainActivity.version;
            data += "\nusername: " + username;
            data += "\npassword: " + password;
            data += "\nip: " + serverIP;
            data += "\nport: " + serverPort;


            //File.WriteAllText(path, data);
            byte[] bytes = Crypto.Encrypt(data);
            File.WriteAllBytes(path, bytes);
        }

        public static void SaveConfig()
        {
            SaveConfig(GetConfigPath());
        }

        public static void ReadConfigFile()
        {
            var file = GetConfigPath();
            if (!File.Exists(file))
            {
                SaveConfig(file);
            }

            byte[] bytes = File.ReadAllBytes(file);
            string plainText = "";
            try
            {
                plainText = Crypto.Decrypt(bytes);
            }
            catch (Exception e)
            {
                errorDialog = "Błąd odszyfrowywania pliku konfiguracyjnego";
                MainActivity.singleton.ShowSettingsScreen();
                return;
            }

            string[] lines = plainText.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string[] data = lines[i].Split(": ");
                if (data.Length != 2)
                {
                    errorDialog = "Błąd przetwarzania pliku konfiguracji";
                    MainActivity.singleton.ShowSettingsScreen();
                    return;
                }

                if (data[0] == "username")
                {
                    username = data[1];
                }
                else if (data[0] == "password")
                {
                    password = data[1];
                }
                else if (data[0] == "version")
                {
                    MainActivity.version = data[1];
                }
                else if(data[0] == "ip")
                {
                    serverIP = data[1];
                }
                else if(data[0] == "port")
                {
                    bool result = int.TryParse(data[1], out int port);
                    if(!result)
                    {
                        errorDialog = "Blad przetwarzania pliku konfiguracji - niepoprawny port";
                        MainActivity.singleton.ShowSettingsScreen();
                        return;
                    }
                    serverPort = port;
                }
                else
                {
                    //Log("Blad przetwarzania pliku konfiguracji - nie znaleziono tagu \"" + data[0] + "\"");
                    //MainActivity.singleton.ShowDialog("Error", "Blad przetwarzania pliku konfiguracji - nie znaleziono tagu \"" + data[0] + "\"", Context);
                    errorDialog = "Blad przetwarzania pliku konfiguracji - nie znaleziono tagu \"" + data[0] + "\"";
                    MainActivity.singleton.ShowSettingsScreen();
                    return;
                }
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            View view = inflater.Inflate(Resource.Layout.settings_screen, container, false);

            saveButton = view.FindViewById<Button>(Resource.Id.saveButton);
            emailField = view.FindViewById<EditText>(Resource.Id.email_input);
            passwordField = view.FindViewById<EditText>(Resource.Id.password_input);
            serverIpField = view.FindViewById<EditText>(Resource.Id.serverip_input);
            serverPortField = view.FindViewById<EditText>(Resource.Id.serverport_input);
            saveButton.Click += SaveButton_Click;

            emailField.Text = username;
            passwordField.Text = password;
            serverIpField.Text = serverIP;
            serverPortField.Text = serverPort.ToString();

            return view;
        }

        void SaveButtonClicked(object sender, DialogClickEventArgs e)
        {
            MainActivity.singleton.CloseAllTabs();
            MainActivity.singleton.ShowLimitsScreen();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            username = emailField.Text;
            password = passwordField.Text;
            serverIP = serverIpField.Text;
            serverPort = int.Parse(serverPortField.Text);

            SaveConfig();
            Console.WriteLine(username + ":" + password);

            AlertDialog.Builder dialog = new AlertDialog.Builder(Context);
            dialog.SetMessage("Pomyślnie zapisano ustawienia");
            dialog.SetTitle("Sukces!");
            dialog.SetPositiveButton("OK", SaveButtonClicked);
            dialog.SetCancelable(true);
            dialog.Create().Show();
        }
    }
}
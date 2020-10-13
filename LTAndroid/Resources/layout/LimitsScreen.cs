using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using Network;
using System.ComponentModel;
using Android.Media;
using System.Drawing.Imaging;
using Network.Packets;
using System.Threading;
using Android.Graphics;
using System.IO;

namespace LTAndroid.Resources.layout
{
    public class LimitsScreen : Android.Support.V4.App.Fragment
    {
        public struct DownloadLimits
        {
            public int download;
            public int upload;
            public int downloadLimit;
            public int uploadLimit;
            public int cost;

            public DownloadLimits(int download, int upload, int downloadLimit, int uploadLimit, int cost)
            {
                this.download = download;
                this.upload = upload;
                this.downloadLimit = downloadLimit;
                this.uploadLimit = uploadLimit;
                this.cost = cost;
            }

            public override string ToString()
            {
                return ToString(';');
            }

            public string ToString(char separator)
            {
                return download.ToString() + separator + upload.ToString() + separator + downloadLimit.ToString() + separator + uploadLimit.ToString() + separator + cost.ToString();
            }

            public static DownloadLimits EMPTY = new DownloadLimits() { cost = -1 };
        }

        ProgressBar downloadLimitBar;
        ProgressBar uploadLimitBar;
        ProgressBar downloadingBar;

        TextView downloadLimitText;
        TextView uploadLimitText;
        TextView costText;

        DownloadLimits limits;

        void Log(object data)
        {
            //textMessage.Text += data.ToString() + "\n";
            Console.WriteLine(data);
        }

        

        async Task IncrementLimitBar(ProgressBar bar, TextView text, int limit, int max)
        {
            //limit = 100000;

            //float step = (limit * 100f / max) / 100f;
            int end = (int)(limit * 100f / max);
            for (int i = -90; i < 90; i += 2)
            {
                float v = end * (MathF.Sin(i * MathF.PI / 180f) + 1) / 2f;

                bar.SetProgress((int)v, false);
                text.Text = (v * max / 100f / 1024.0).ToString("0.00") + "GB / " + (max / 1024.0).ToString("0.00") + "GB";

                await Task.Delay(1);
            }

            bar.SetProgress((int)(limit * 100f / max), false);
            text.Text = (limit / 1024.0).ToString("0.00") + "GB / " + (max / 1024.0).ToString("0.00") + "GB";
        }

        async Task IncrementCostText(TextView text, int limit)
        {
            for(int i = -90;i<90;i += 2)
            {
                float v = limit * (MathF.Sin(i * MathF.PI / 180f) + 1) / 2f;

                text.Text = "Koszt: " + ((int)v).ToString() + "%";

                await Task.Delay(1);
            }

            text.Text = "Koszt: " + limit.ToString() + "%";
        }

        async Task UpdateLimits()
        {

            ConnectionResult result = ConnectionResult.Timeout;

            TcpConnection connection = ConnectionFactory.CreateTcpConnection("minik.ml", 7154, out result);//ConnectionFactory.CreateTcpConnection("10.0.0.102", 7154, out result);
            connection.ConnectionClosed += Connection_ConnectionClosed;
            Log(result);
            //SettingsScreen.username = "tomeckimichal01@gmail.com";
            //SettingsScreen.password = "Misiek200111";
            string data = SettingsScreen.username + ";" + SettingsScreen.password; //"tomeckimichal01@gmail.com;Misiek200111";
            Console.WriteLine("\n\n\nData: " + data + "\n\n\n");
            bool done = false;


            connection.RegisterRawDataHandler("getLimit", (a /* RawData */ , b /* Connection */) =>
            {
                try
                {
                    string r = System.Text.Encoding.UTF8.GetString(a.Data);
                    string[] limitsDt = r.Split(';');

                    if(limitsDt.Length == 2 && limitsDt[0] == "BL")
                    {
                        SettingsScreen.errorDialog = "Błędny login lub hasło";
                        MainActivity.singleton.ShowSettingsScreen();
                        done = true;
                        return;
                    }
                    /*dl = int.Parse(limits[0]);
                    ul = int.Parse(limits[1]);
                    dlT = int.Parse(limits[2]);
                    ulT = int.Parse(limits[3]);
                    cost = int.Parse(limits[4]);*/

                    limits = new DownloadLimits(int.Parse(limitsDt[0]), int.Parse(limitsDt[1]), int.Parse(limitsDt[2]), int.Parse(limitsDt[3]), int.Parse(limitsDt[4]));
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                }
                done = true;
            });
            connection.SendRawData("getLimit", System.Text.Encoding.UTF8.GetBytes(data));

            while (!done)
            {
                await Task.Delay(100);
            }

            DisplayLimits();

            connection.ConnectionClosed -= Connection_ConnectionClosed;
        }

        async void DisplayLimits(int delay = 0)
        {
            if(delay > 0)
                await Task.Delay(delay);

            try
            {
                Log("dl: " + limits.download.ToString() + ", dlT: " + limits.downloadLimit.ToString());
                IncrementLimitBar(downloadLimitBar, downloadLimitText, limits.download, limits.downloadLimit);
                IncrementLimitBar(uploadLimitBar, uploadLimitText, limits.upload, limits.uploadLimit);


                downloadingBar.Visibility = ViewStates.Invisible;

                //costText.Text = "Koszt: " + limits.cost.ToString() + "%";
                IncrementCostText(costText, limits.cost);
                
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }


        private void Connection_ConnectionClosed(Network.Enums.CloseReason arg1, Connection arg2)
        {
            Log("Connection lost");
        }

        void UpdateCredentials(object sender, EventArgs e)
        {
            MainActivity.singleton.ShowSettingsScreen();
            initialized = false;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            // Create your fragment here
            if(!initialized)
            {
                SettingsScreen.ReadConfigFile();
            }

            if(SettingsScreen.username.Length == 0)
            {
                //MainActivity.singleton.ShowDialog("Brak danych", "Brak zapisanych informacji logowania, wpisz je na następnym ekranie");
                AlertDialog.Builder dialog = new AlertDialog.Builder(Context);
                dialog.SetMessage("Brak zapisanych informacji logowania, wpisz je na następnym ekranie");
                dialog.SetTitle("Brak danych");
                dialog.SetPositiveButton("OK", UpdateCredentials);
                dialog.SetCancelable(true);
                dialog.Create().Show();
                //MainActivity.singleton.ShowSettingsScreen();
                
                return;
            }

            if (!initialized)
            {
                UpdateLimits();
            }
            else
            {
                DisplayLimits(10);
            }

        }

        bool initialized = false;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            
            View v = inflater.Inflate(Resource.Layout.limits_screen, container, false);
            if (v == null)
            {
                throw new Exception("It's null");
            }

            downloadLimitBar = v.FindViewById<ProgressBar>(Resource.Id.downloadProgressBar);
            uploadLimitBar = v.FindViewById<ProgressBar>(Resource.Id.uploadProgressBar);
            downloadingBar = v.FindViewById<ProgressBar>(Resource.Id.downloadingBar);
            downloadLimitText = v.FindViewById<TextView>(Resource.Id.downloadLimitText);
            uploadLimitText = v.FindViewById<TextView>(Resource.Id.uploadLimitText);
            costText = v.FindViewById<TextView>(Resource.Id.costText);

            initialized = true;
            return v;
        }
    }
}
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;

using Android.Util;

using System;

using Network;
using System.ComponentModel;
using Android.Media;
using System.Drawing.Imaging;
using System.Text;
using Network.Packets;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;

using System.IO;
using LTAndroid.Resources.layout;

namespace LTAndroid
{

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        public static TextView textMessage;
        LimitsScreen limitsScr = null;
        SettingsScreen settingsScr = null;


        public static string version = "1";

        public static MainActivity singleton;

        public void CloseAllTabs()
        {
            limitsScr = null;
            settingsScr = null;
        }

        void Log(object data)
        {
            textMessage.Text += data.ToString() + "\n";
        }

        public void ShowDialog(string title, string message, Android.Content.Context context)
        {
            Android.Support.V7.App.AlertDialog.Builder dialog = new Android.Support.V7.App.AlertDialog.Builder(context);
            dialog.SetMessage(message);
            dialog.SetTitle(title);
            dialog.SetPositiveButton("OK", (a,b) => { });
            dialog.SetCancelable(true);
            dialog.Create().Show();
        }

        void ShowInitScreen()
        {
            //var ft = SupportFragmentManager.BeginTransaction();
            //ft.Add(Resource.Id.fLayout, new fragment1(), "fragment1");
            //ft.Commit();
            //SetContentView(Resource.Layout.settings_screen);
        }

        public void ShowSettingsScreen()
        {
            if(settingsScr == null)
            {
                settingsScr = new SettingsScreen();
            }

            var transcation = SupportFragmentManager.BeginTransaction();
            transcation.Replace(Resource.Id.fragmentLayout, settingsScr, "Settings");
            transcation.Commit();
        }


        public void ShowLimitsScreen()
        {
            if(limitsScr == null)
            {
                limitsScr = new LimitsScreen();
            }

            var transcation = SupportFragmentManager.BeginTransaction();
            transcation.Replace(Resource.Id.fragmentLayout, limitsScr, "Limits");
            transcation.Commit();
        }

        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            textMessage = FindViewById<TextView>(Resource.Id.textView1);

            singleton = this;

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);

            ShowLimitsScreen();
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.navigation_home:
                    //SetContentView(Resource.Layout.activity_main);
                    ShowLimitsScreen();
                    return true;
                case Resource.Id.navigation_settings:
                    //SetContentView(Resource.Layout.settings_screen);
                    ShowSettingsScreen();
                    return true;
            }
            return false;
        }
    }
}


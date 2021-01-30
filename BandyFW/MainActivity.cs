using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System;
using System.Net;
using System.Net.Http;
using Xamarin.Essentials;
using Android.Preferences;
using Android.Content;

namespace BandyFW
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			SetContentView(Resource.Layout.activity_main);
        }

        protected override void OnResume()
        {
            base.OnResume();
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();

            Button submit_button = FindViewById<Button>(Resource.Id.submit_button);
            Button remember_button = FindViewById<Button>(Resource.Id.remember_button);
            Button restore_button = FindViewById<Button>(Resource.Id.restore_button);
            Button reset_button = FindViewById<Button>(Resource.Id.reset_button);
            EditText model_text = FindViewById<EditText>(Resource.Id.model_text);
            EditText production_text = FindViewById<EditText>(Resource.Id.production_text);
            EditText app_name_text = FindViewById<EditText>(Resource.Id.app_name_text);
            EditText app_version_text = FindViewById<EditText>(Resource.Id.app_version_text);
            EditText response_text = FindViewById<EditText>(Resource.Id.response_text);

            submit_button.Click += async delegate
            {
                var current = Connectivity.NetworkAccess;
                if (current == Xamarin.Essentials.NetworkAccess.Internet)
                {
                    if (model_text.Text == "" || model_text.Text == "" || model_text.Text == "" || model_text.Text == "")
                    {
                        Toast.MakeText(this, "You should to input all required values!", ToastLength.Long).Show();
                    }
                    else
                    {
                        HttpClient client = new HttpClient();
                        HttpRequestMessage request = new HttpRequestMessage();

                        UriBuilder uriBuilder = new UriBuilder
                        {
                            Scheme = "https",
                            Host = "api-mifit-ru.huami.com",
                            Path = "devices/ALL/hasNewVersion",
                            Query = "productId=0&vendorSource=0&resourceVersion=0&firmwareFlag=0&vendorId=0&resourceFlag=0&productionSource=" + production_text.Text + "&userid=0&userId=0&deviceSource=" + model_text.Text + "&fontVersion=0&fontFlag=0&appVersion=" + app_version_text.Text + "&appid=0&callid=0&channel=0&country=0&cv=0&device=0&deviceType=ALL&device_type=0&firmwareVersion=0&hardwareVersion=0&lang=0&support8Bytes=true&timezone=0&v=0",
                        };
                        Uri URL = uriBuilder.Uri;
                        String stringUri;
                        stringUri = URL.ToString();

                        request.RequestUri = new Uri(stringUri);
                        request.Method = HttpMethod.Get;

                        request.Headers.Add("hm-privacy-diagnostics", "0");
                        request.Headers.Add("country", "0");
                        request.Headers.Add("appplatform", "android_phone");
                        request.Headers.Add("hm-privacy-ceip", "0");
                        request.Headers.Add("X-Request-Id", "0");
                        request.Headers.Add("timezone", "0");
                        request.Headers.Add("channel", "0");
                        request.Headers.Add("User-Agent", "0");
                        request.Headers.Add("cv", "0");
                        request.Headers.Add("appname", app_name_text.Text);
                        request.Headers.Add("v", "0");
                        request.Headers.Add("apptoken", "0");
                        request.Headers.Add("lang", "0");
                        request.Headers.Add("Host", "api-mifit-ru.huami.com");
                        request.Headers.Add("Connection", "0");
                        request.Headers.Add("Accept-Encoding", "0");

                        HttpResponseMessage response = await client.SendAsync(request);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            HttpContent responseContent = response.Content;

                            var json = await responseContent.ReadAsStringAsync();
                            response_text.Text = json;

                            //For firmware_json.cs
                            //var data = JsonConvert.DeserializeObject<Firmware_json>(json);
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this, "Enable network!", ToastLength.Short).Show();
                }
            };

            remember_button.Click += delegate
            {
                if (model_text.Text == "" && production_text.Text == "" && app_name_text.Text == "" && app_version_text.Text == "" && response_text.Text == "")
                {
                    RunOnUiThread(() => Toast.MakeText(this, "There are no values for saving!", ToastLength.Short).Show());
                }
                else
                {
                    editor.PutString("model_code", model_text.Text);
                    editor.PutString("production_code", production_text.Text);
                    editor.PutString("app_name", app_name_text.Text);
                    editor.PutString("app_version", app_version_text.Text);
                    editor.PutString("response", response_text.Text);
                    editor.Apply();
                    RunOnUiThread(() => Toast.MakeText(this, "Remember success", ToastLength.Short).Show());
                }
            };

            restore_button.Click += delegate
            {
                if (prefs.GetString("model_code", null) == "" && prefs.GetString("production_code", null) == "" && prefs.GetString("app_name", null) == "" && prefs.GetString("app_version", null) == "" && prefs.GetString("response", null) == "")
                {
                    RunOnUiThread(() => Toast.MakeText(this, "There are no values in memory!", ToastLength.Short).Show());
                }
                else {
                    model_text.Text = prefs.GetString("model_code", null);
                    production_text.Text = prefs.GetString("production_code", null);
                    app_name_text.Text = prefs.GetString("app_name", null);
                    app_version_text.Text = prefs.GetString("app_version", null);
                    response_text.Text = prefs.GetString("response", null);
                    RunOnUiThread(() => Toast.MakeText(this, "Restore success", ToastLength.Short).Show());
                }
            };

            reset_button.Click += delegate
            {
                model_text.Text = "";
                production_text.Text = "";
                app_name_text.Text = "";
                app_version_text.Text = "";
                response_text.Text = "";

                editor.PutString("model_code", model_text.Text);
                editor.PutString("production_code", production_text.Text);
                editor.PutString("app_name", app_name_text.Text);
                editor.PutString("app_version", app_version_text.Text);
                editor.PutString("response", response_text.Text);
                editor.Apply();

                RunOnUiThread(() => Toast.MakeText(this, "Reset success", ToastLength.Short).Show());
            };
        }
    }
}

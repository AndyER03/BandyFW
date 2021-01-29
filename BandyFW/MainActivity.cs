using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System;
using System.Net.Http;

namespace BandyFW
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Button button = FindViewById<Button>(Resource.Id.button1);
            Button button2 = FindViewById<Button>(Resource.Id.button2);
            Button button3 = FindViewById<Button>(Resource.Id.button3);
            EditText edittext = FindViewById<EditText>(Resource.Id.edittext1);
            EditText edittext2 = FindViewById<EditText>(Resource.Id.edittext2);

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://api-mifit-ru.huami.com/devices/ALL/hasNewVersion?productId=0&vendorSource=0&resourceVersion=0&firmwareFlag=0&vendorId=0&resourceFlag=0&productionSource=257&userid=0&userId=0&deviceSource=25&fontVersion=0&fontFlag=0&appVersion=4.8.1_50392&appid=0&callid=0&channel=0&country=0&cv=0&device=0&deviceType=ALL&device_type=0&firmwareVersion=0&hardwareVersion=0&lang=0&support8Bytes=0&timezone=0&v=0");
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
            request.Headers.Add("appname", "com.xiaomi.hm.health");
            request.Headers.Add("v", "0");
            request.Headers.Add("apptoken", "0");
            request.Headers.Add("lang", "0");
            request.Headers.Add("Host", "api-mifit-ru.huami.com");
            request.Headers.Add("Connection", "0");
            request.Headers.Add("Accept-Encoding", "0");

            HttpResponseMessage response = await client.SendAsync(request);
            HttpContent responseContent = response.Content;
            var json = await responseContent.ReadAsStringAsync();

            //string hostname = "https://api-mifit-ru.huami.com/devices/ALL/hasNewVersion";
            //var web = new WebClient();
            //var url = $"{hostname}productId=0&vendorSource=0&resourceVersion=0&firmwareFlag=0&vendorId=0&resourceFlag=0&productionSource=257&userid=0&userId=0&deviceSource=25&fontVersion=0&fontFlag=0&appVersion=4.8.1_50392&appid=0&callid=0&channel=0&country=0&cv=0&device=0&deviceType=ALL&device_type=0&firmwareVersion=0&hardwareVersion=0&lang=0&support8Bytes=0&timezone=0&v=0";
            //var responseString = web.DownloadString(url);

            button.Click += delegate
            {
				if (edittext.Text == "") {
                    button.Text = "Click";
                    Toast.MakeText(this, "Enter value", ToastLength.Short).Show();
                }
                else
				    button.Text = edittext.Text;

            };

            button2.Click += delegate
            {
                button.Text = "Click";
                edittext.Text = "";
            };

            button3.Click += delegate
            {
                edittext2.Text = json;
                Toast.MakeText(this, "Created by AndyER03", ToastLength.Short).Show();
            };
        }
    }
}

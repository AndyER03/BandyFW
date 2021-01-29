using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

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

            Button button = FindViewById<Button>(Resource.Id.button1);
            Button button2 = FindViewById<Button>(Resource.Id.button2);
            Button button3 = FindViewById<Button>(Resource.Id.button3);
            EditText edittext = FindViewById<EditText>(Resource.Id.edittext1);

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
                Toast.MakeText(this, "Created by AndyER03", ToastLength.Short).Show();
            };
        }
    }
}

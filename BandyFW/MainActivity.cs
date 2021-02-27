using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Support.V7.App;
using Android.Widget;
using BandyFW.Model;
using Newtonsoft.Json;
using Plugin.Clipboard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using Xamarin.Essentials;

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

			ListView response_listview = FindViewById<ListView>(Resource.Id.response_listview);

			Button submit_button = FindViewById<Button>(Resource.Id.submit_button);
			Button copy_MD5_button = FindViewById<Button>(Resource.Id.copy_MD5_button);
			Button download_button = FindViewById<Button>(Resource.Id.download_button);
			Button reset_button = FindViewById<Button>(Resource.Id.reset_button);
			Button clear_response_button = FindViewById<Button>(Resource.Id.clear_response_button);
			EditText model_text = FindViewById<EditText>(Resource.Id.model_text);
			EditText production_text = FindViewById<EditText>(Resource.Id.production_text);
			LinearLayout device_layout = FindViewById<LinearLayout>(Resource.Id.device_layout);

			var app_radio = FindViewById<RadioGroup>(Resource.Id.app_radio);
			RadioButton radio_zepp = FindViewById<RadioButton>(Resource.Id.radio_zepp);
			RadioButton radio_mifit = FindViewById<RadioButton>(Resource.Id.radio_mifit);

			CheckBox play_postfix_checkbox = FindViewById<CheckBox>(Resource.Id.play_postfix_checkbox);

			EditText app_name_text = FindViewById<EditText>(Resource.Id.app_name_text);
			EditText app_version_number_text = FindViewById<EditText>(Resource.Id.app_version_number_text);
			EditText app_version_build_text = FindViewById<EditText>(Resource.Id.app_version_build_text);

			LinearLayout response_text_layout = FindViewById<LinearLayout>(Resource.Id.response_text_layout);
			EditText response_text = FindViewById<EditText>(Resource.Id.response_text);

			Spinner spinner = FindViewById<Spinner>(Resource.Id.device_model_spinner);
			var items = new List<string>() {
				GetString(Resource.String.device_name_manual),
				GetString(Resource.String.device_name_york),
				GetString(Resource.String.device_name_chaohu),
				GetString(Resource.String.device_name_chaohulite),
				GetString(Resource.String.device_name_comow),
				GetString(Resource.String.device_name_lisbon),
				GetString(Resource.String.device_name_lisbonw),
				GetString(Resource.String.device_name_hawk),
				GetString(Resource.String.device_name_hawkw),
				GetString(Resource.String.device_name_falcon),
				GetString(Resource.String.device_name_falconw),
				GetString(Resource.String.device_name_falconl),
				GetString(Resource.String.device_name_newton_china),
				GetString(Resource.String.device_name_newton_global),
				GetString(Resource.String.device_name_nessw),
				GetString(Resource.String.device_name_kestrel),
				GetString(Resource.String.device_name_kesrtelw),
				GetString(Resource.String.device_name_tokyo),
				GetString(Resource.String.device_name_comol),
				GetString(Resource.String.device_name_vulture),
				GetString(Resource.String.device_name_pyh),
				GetString(Resource.String.device_name_venice_china),
				GetString(Resource.String.device_name_venicew),
				GetString(Resource.String.device_name_cinco_nfc),
				GetString(Resource.String.device_name_cinco_global),
				GetString(Resource.String.device_name_kongming_china),
				GetString(Resource.String.device_name_kongming_china_nfc),
				GetString(Resource.String.device_name_corsica),
				GetString(Resource.String.device_name_corsicaw),
				GetString(Resource.String.device_name_onyx),
				GetString(Resource.String.device_name_onyxw),
				GetString(Resource.String.device_name_osprey),
				GetString(Resource.String.device_name_ospreyw)
			};
			var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, items);
			spinner.Adapter = adapter;
			spinner.ItemSelected += Spinner_ItemSelected;
			spinner.SaveEnabled = true;

			app_name_text.Focusable = false;

			int spinner_position = prefs.GetInt("spinner_position", 0);
			spinner.SetSelection(spinner_position);

			string zepp_name = "com.huami.midong";
			string mifit_name = "com.xiaomi.hm.health";

			app_radio.CheckedChange += (s, e) =>
			{
				if (radio_zepp.Checked)
				{
					app_name_text.Text = zepp_name;
					play_postfix_checkbox.Enabled = true;
					Clear_response();

					editor.PutString("mifit_app_version_number", app_version_number_text.Text);
					editor.PutString("mifit_app_version_build", app_version_build_text.Text);
					editor.Apply();

					if (prefs.GetBoolean("play_postfix_checkbox", false) == true)
					{
						play_postfix_checkbox.Checked = true;
					}

					app_version_number_text.Text = prefs.GetString("zepp_app_version_number", null);
					app_version_build_text.Text = prefs.GetString("zepp_app_version_build", null);

				}
				else if (radio_mifit.Checked)
				{
					app_name_text.Text = mifit_name;
					Clear_response();

					if (play_postfix_checkbox.Checked)
					{
						editor.PutBoolean("play_postfix_checkbox", true);
						editor.Apply();
					}
					else
					{
						editor.PutBoolean("play_postfix_checkbox", false);
						editor.Apply();
					}
					editor.PutString("zepp_app_version_number", app_version_number_text.Text);
					editor.PutString("zepp_app_version_build", app_version_build_text.Text);
					editor.Apply();

					play_postfix_checkbox.Enabled = false;
					play_postfix_checkbox.Checked = false;

					app_version_number_text.Text = prefs.GetString("mifit_app_version_number", null);
					app_version_build_text.Text = prefs.GetString("mifit_app_version_build", null);
				}
			};

			var play_postfix = "";
			play_postfix_checkbox.CheckedChange += (s, e) =>
			{
				if (play_postfix_checkbox.Checked)
				{
					play_postfix = "-play";

				}
				else
				{
					play_postfix = "";
				}
			};

			//Submit button logics (Get request + change request field)
			submit_button.Click += async delegate
			{
				var current = Connectivity.NetworkAccess;
				if (current == Xamarin.Essentials.NetworkAccess.Internet)
				{
					if (model_text.Text == "" || production_text.Text == "" || app_name_text.Text == GetString(Resource.String.shoose_app) || app_version_number_text.Text == "" || app_version_build_text.Text == "")
					{
						Toast.MakeText(this, Resource.String.input_all_values, ToastLength.Long).Show();
					}
					else
					{
						HttpClient client = new HttpClient();
						HttpRequestMessage request = new HttpRequestMessage();

						string request_host = "api-mifit-ru.huami.com";

						UriBuilder uriBuilder = new UriBuilder
						{
							Scheme = "https",
							Host = request_host,
							Path = "devices/ALL/hasNewVersion",
							Query = "productId=71&vendorSource=1&resourceVersion=0&firmwareFlag=0&vendorId=0&resourceFlag=0&productionSource=" + production_text.Text + "&userid=0&userId=0&deviceSource=" + model_text.Text + "&fontVersion=0&fontFlag=0&appVersion=" + app_version_number_text.Text + play_postfix + "_" + app_version_build_text.Text + "&appid=0&callid=0&channel=0&country=0&cv=0&device=0&deviceType=ALL&device_type=0&firmwareVersion=0&hardwareVersion=0&lang=0&support8Bytes=true&timezone=0&v=0",
						};
						Uri URL = uriBuilder.Uri;
						String stringUri;
						stringUri = URL.ToString();

						request.RequestUri = new Uri(stringUri);
						request.Method = HttpMethod.Get;

						request.Headers.Add("hm-privacy-diagnostics", "false");
						request.Headers.Add("country", "CH");
						request.Headers.Add("appplatform", "android_phone");
						request.Headers.Add("hm-privacy-ceip", "0");
						request.Headers.Add("X-Request-Id", "0");
						request.Headers.Add("timezone", "0");
						request.Headers.Add("channel", "play");
						request.Headers.Add("User-Agent", "0");
						request.Headers.Add("cv", "0");
						request.Headers.Add("appname", app_name_text.Text);
						request.Headers.Add("v", "0");
						request.Headers.Add("apptoken", "0");
						request.Headers.Add("lang", "zh_CH");
						request.Headers.Add("Host", request_host);
						request.Headers.Add("Connection", "Keep-Alive");
						request.Headers.Add("Accept-Encoding", "0");

						HttpResponseMessage response = await client.SendAsync(request);
						if (response.StatusCode == HttpStatusCode.OK)
						{
							HttpContent responseContent = response.Content;

							var server_response = await responseContent.ReadAsStringAsync();
							//response_text.Text = server_response;

							Response content = JsonConvert.DeserializeObject<Response>(server_response);
							string log = "";
							if (content.changeLog != null)
							{
								log = "Click to show log";
							}
							else
							{
								log = "Log not founded";
							}

							ObservableCollection<string> data = new ObservableCollection<string>
							{
								"Firmware version: " + content.firmwareVersion + "\n" + "MD5: " + content.firmwareMd5,
								"Resource version: " + content.resourceVersion.ToString() + "\n" + "MD5: " + content.resourceMd5,
								"Font version: " + content.fontVersion.ToString() + "\n" + "MD5: " + content.fontMd5,
								"Languages: " + content.lang,
								log
								//content.deviceType,
								//content.deviceSource,
								//content.firmwareLength,
								//content.firmwareFlag,
								//content.fontLength,
								//content.fontFlag,
								//content.resourceFlag,
								//content.resourceLength,
								//content.productionSource,
								//content.changeLog,
								//content.upgradeType,
								//content.buildTime,
								//content.ignore,
								//content.support8Bytes,
								//content.downloadBackupPaths
							};

							adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, data);
							response_listview.TextFilterEnabled = false;

							if (content.buildTime != 0)
							{
								response_listview.Visibility = Android.Views.ViewStates.Visible;
								response_listview.Adapter = adapter;
								clear_response_button.Visibility = Android.Views.ViewStates.Visible;
							}
							else
							{
								response_listview.Visibility = Android.Views.ViewStates.Gone;
								clear_response_button.Visibility = Android.Views.ViewStates.Gone;
								Toast.MakeText(Application, "Firmware not found", ToastLength.Short).Show();
							}


							response_listview.ItemClick += delegate (object sender, AdapterView.ItemClickEventArgs args)
							{
								//Toast.MakeText(Application, ((TextView)args.View).Text, ToastLength.Short).Show();


								if (args.Position.ToString() == "0")
								{
									response_text.Text = content.firmwareVersion + " : " + content.firmwareMd5;
									editor.PutString("content_MD5", content.firmwareMd5);
									editor.PutString("content_URL", content.firmwareUrl);
									editor.Apply();

									copy_MD5_button.Visibility = Android.Views.ViewStates.Visible;
									download_button.Visibility = Android.Views.ViewStates.Visible;
									clear_response_button.Visibility = Android.Views.ViewStates.Visible;
									response_text_layout.Visibility = Android.Views.ViewStates.Visible;
									response_text.Visibility = Android.Views.ViewStates.Visible;
								}
								else if (args.Position.ToString() == "1")
								{
									if (content.resourceVersion != 0)
									{
										response_text.Text = content.resourceVersion + " : " + content.resourceMd5;
										editor.PutString("content_MD5", content.resourceMd5);
										editor.PutString("content_URL", content.resourceUrl);
										editor.Apply();

										copy_MD5_button.Visibility = Android.Views.ViewStates.Visible;
										download_button.Visibility = Android.Views.ViewStates.Visible;
										clear_response_button.Visibility = Android.Views.ViewStates.Visible;
										response_text_layout.Visibility = Android.Views.ViewStates.Visible;
										response_text.Visibility = Android.Views.ViewStates.Visible;
									}
									else
									{
										response_text.Text = GetString(Resource.String.not_available);
										editor.PutString("content_MD5", "");
										editor.PutString("content_URL", "");
										editor.Apply();

										copy_MD5_button.Visibility = Android.Views.ViewStates.Gone;
										download_button.Visibility = Android.Views.ViewStates.Gone;
										clear_response_button.Visibility = Android.Views.ViewStates.Visible;
										response_text_layout.Visibility = Android.Views.ViewStates.Visible;
										response_text.Visibility = Android.Views.ViewStates.Visible;
									}
								}
								else if (args.Position.ToString() == "2")
								{
									response_text.Text = content.fontVersion + " : " + content.fontMd5;
									editor.PutString("content_MD5", content.fontMd5);
									editor.PutString("content_URL", content.fontUrl);
									editor.Apply();

									copy_MD5_button.Visibility = Android.Views.ViewStates.Visible;
									download_button.Visibility = Android.Views.ViewStates.Visible;
									clear_response_button.Visibility = Android.Views.ViewStates.Visible;
									response_text_layout.Visibility = Android.Views.ViewStates.Visible;
									response_text.Visibility = Android.Views.ViewStates.Visible;
								}
								else if (args.Position.ToString() == "3")
								{
									response_text.Text = content.lang;
									editor.PutString("content_MD5", "");
									editor.PutString("content_URL", "");
									editor.Apply();

									copy_MD5_button.Visibility = Android.Views.ViewStates.Gone;
									download_button.Visibility = Android.Views.ViewStates.Gone;
									clear_response_button.Visibility = Android.Views.ViewStates.Visible;
									response_text_layout.Visibility = Android.Views.ViewStates.Visible;
									response_text.Visibility = Android.Views.ViewStates.Visible;
								}
								else if (args.Position.ToString() == "4")
								{
									response_text.Text = content.changeLog;
									editor.PutString("content_MD5", "");
									editor.PutString("content_URL", "");
									editor.Apply();

									copy_MD5_button.Visibility = Android.Views.ViewStates.Gone;
									download_button.Visibility = Android.Views.ViewStates.Gone;
									clear_response_button.Visibility = Android.Views.ViewStates.Visible;
									response_text_layout.Visibility = Android.Views.ViewStates.Visible;
									response_text.Visibility = Android.Views.ViewStates.Visible;
								}
								//response_text.Text = ((TextView)args.View).Text;
							};
						}
					}
				}
				else
				{
					Toast.MakeText(this, "Enable network!", ToastLength.Short).Show();
				}
			};

			//copy MD5 button logics
			copy_MD5_button.Click += delegate
			{
				if (prefs.GetString("content_MD5", null) != "")
				{
					CrossClipboard.Current.SetText(prefs.GetString("content_MD5", null));
					Toast.MakeText(this, Resource.String.md5_copied, ToastLength.Short).Show();
				}
			};

			//Download button logics
			download_button.Click += async delegate
			{
				if (prefs.GetString("content_URL", null) != "")
				{
					await Browser.OpenAsync(new Uri(prefs.GetString("content_URL", null)), BrowserLaunchMode.SystemPreferred);
					Toast.MakeText(this, Resource.String.downloading, ToastLength.Short).Show();
				}
			};

			//Reset button logics
			reset_button.Click += delegate
			{
				model_text.Enabled = true;
				production_text.Enabled = true;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = true;
				play_postfix_checkbox.Enabled = true;
				device_layout.Visibility = Android.Views.ViewStates.Gone;
				response_listview.Visibility = Android.Views.ViewStates.Gone;

				spinner.SetSelection(0);
				model_text.Text = "";
				production_text.Text = "";
				radio_zepp.Checked = false;
				radio_mifit.Checked = false;
				app_name_text.Text = GetString(Resource.String.shoose_app);
				play_postfix_checkbox.Checked = false;
				app_version_number_text.Text = "";
				app_version_build_text.Text = "";
				response_text.Text = "";
				copy_MD5_button.Visibility = Android.Views.ViewStates.Gone;
				download_button.Visibility = Android.Views.ViewStates.Gone;

				editor.PutString("content_MD5", "");
				editor.PutString("content_URL", "");
				editor.PutString("zepp_app_version_number", app_version_number_text.Text);
				editor.PutString("zepp_app_version_build", app_version_build_text.Text);
				editor.PutString("mifit_app_version_number", app_version_number_text.Text);
				editor.PutString("mifit_app_version_build", app_version_build_text.Text);
				editor.PutBoolean("play_postfix_checkbox", false);
				editor.PutString("response", response_text.Text);
				editor.Apply();

				RunOnUiThread(() => Toast.MakeText(this, Resource.String.reset_success, ToastLength.Short).Show());
			};

			//Clear button logics
			clear_response_button.Click += delegate
			{
				Clear_response();
			};

			//App name field click logics
			app_name_text.Click += delegate
			{
				if (radio_zepp.Checked || radio_mifit.Checked)
				{
					RunOnUiThread(() => Toast.MakeText(this, app_name_text.Text, ToastLength.Short).Show());
				}
				else
				{
					app_name_text.SetText(Resource.String.shoose_app);
				}
			};
		}

		public void Clear_response()
		{
			Button copy_MD5_button = FindViewById<Button>(Resource.Id.copy_MD5_button);
			Button download_button = FindViewById<Button>(Resource.Id.download_button);
			Button clear_response_button = FindViewById<Button>(Resource.Id.clear_response_button);
			EditText response_text = FindViewById<EditText>(Resource.Id.response_text);
			ListView response_listview = FindViewById<ListView>(Resource.Id.response_listview);
			LinearLayout response_text_layout = FindViewById<LinearLayout>(Resource.Id.response_text_layout);

			copy_MD5_button.Visibility = Android.Views.ViewStates.Gone;
			download_button.Visibility = Android.Views.ViewStates.Gone;
			response_listview.Visibility = Android.Views.ViewStates.Gone;
			clear_response_button.Visibility = Android.Views.ViewStates.Gone;
			response_text_layout.Visibility = Android.Views.ViewStates.Gone;
			response_text.Visibility = Android.Views.ViewStates.Gone;
			response_text.Text = "";
		}

		public void Spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
		{
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
			ISharedPreferencesEditor editor = prefs.Edit();

			Spinner spinner = FindViewById<Spinner>(Resource.Id.device_model_spinner);

			EditText model_text = FindViewById<EditText>(Resource.Id.model_text);
			EditText production_text = FindViewById<EditText>(Resource.Id.production_text);
			EditText app_name_text = FindViewById<EditText>(Resource.Id.app_name_text);
			RadioButton radio_zepp = FindViewById<RadioButton>(Resource.Id.radio_zepp);
			RadioButton radio_mifit = FindViewById<RadioButton>(Resource.Id.radio_mifit);
			CheckBox play_postfix_checkbox = FindViewById<CheckBox>(Resource.Id.play_postfix_checkbox);
			LinearLayout device_layout = FindViewById<LinearLayout>(Resource.Id.device_layout);

			if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_manual))
			{
				model_text.Enabled = true;
				production_text.Enabled = true;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = true;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Visible;
				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Visible;
				radio_mifit.Visibility = Android.Views.ViewStates.Visible;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_chaohu))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = true;
				play_postfix_checkbox.Enabled = false;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Visible;
				radio_mifit.Visibility = Android.Views.ViewStates.Visible;

				model_text.Text = "12";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_cinco_nfc))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = true;
				play_postfix_checkbox.Enabled = false;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Visible;
				radio_mifit.Visibility = Android.Views.ViewStates.Visible;

				model_text.Text = "24";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_pyh))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = true;
				play_postfix_checkbox.Enabled = false;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Visible;
				radio_mifit.Visibility = Android.Views.ViewStates.Visible;

				model_text.Text = "30";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_kongming_china_nfc))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = false;
				radio_mifit.Enabled = true;
				play_postfix_checkbox.Enabled = false;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Gone;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "58";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_cinco_global))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = true;
				play_postfix_checkbox.Enabled = false;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Visible;
				radio_mifit.Visibility = Android.Views.ViewStates.Visible;

				model_text.Text = "25";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_chaohulite))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = false;
				radio_mifit.Enabled = true;
				play_postfix_checkbox.Enabled = false;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Gone;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "42";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_kongming_china))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = false;
				radio_mifit.Enabled = true;
				play_postfix_checkbox.Enabled = false;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Gone;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "59";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.mifit_app_package_name);
				radio_mifit.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_falcon))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "35";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_falconw))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "36";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_hawk))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "37";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_kestrel))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "40";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_vulture))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "50";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_venice_china))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "53";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_osprey))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "56";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_onyx))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "57";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_corsica))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "61";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_tokyo))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "62";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_lisbon))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "63";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_lisbonw))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "64";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_newton_china))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "67";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_york))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "73";
				production_text.Text = "256";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_hawkw))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "38";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_kesrtelw))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				model_text.Text = "41";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_comol))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "68";
				production_text.Text = "258";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_comow))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "69";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_venicew))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "71";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_ospreyw))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "76";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_newton_global))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "78";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_onyxw))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "81";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_corsicaw))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "82";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_nessw))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "92";
				production_text.Text = "257";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}
			else if ((string)spinner.GetItemAtPosition(e.Position) == GetString(Resource.String.device_name_falconl))
			{
				model_text.Enabled = false;
				production_text.Enabled = false;
				radio_zepp.Enabled = true;
				radio_mifit.Enabled = false;
				play_postfix_checkbox.Enabled = true;
				play_postfix_checkbox.Checked = false;
				Clear_response();

				device_layout.Visibility = Android.Views.ViewStates.Gone;

				play_postfix_checkbox.Visibility = Android.Views.ViewStates.Visible;
				radio_zepp.Visibility = Android.Views.ViewStates.Gone;
				radio_mifit.Visibility = Android.Views.ViewStates.Gone;

				model_text.Text = "46";
				production_text.Text = "258";
				app_name_text.Text = GetString(Resource.String.zepp_app_package_name);
				radio_zepp.Checked = true;
			}

			editor.PutInt("spinner_position", (int)spinner.GetItemIdAtPosition(e.Position));
			editor.Apply();
		}
	}
}

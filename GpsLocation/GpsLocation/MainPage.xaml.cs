using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace GpsLocation
{
    public partial class MainPage : ContentPage
    {
        List<LocationInfo> locationInfos = new List<LocationInfo>();

        private string SaveKey = "GpsLocation";

        public MainPage()
        {
            InitializeComponent();

            string saveData = GetPreferences(this.SaveKey);

            if (saveData != null)
            {
               this.locationInfos = JsonConvert.DeserializeObject<List<LocationInfo>>(saveData);

                foreach (var locationInfo in this.locationInfos)
                {
                    AddLocationData(locationInfo);
                }
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            GetLocation();
        }

        private async void GetLocation()
        {
            string result = await DisplayPromptAsync("위치이름", "위치이름을 지정해주세요");

            if (result != null)
            {
                try
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        this.indicator.IsVisible = true;
                    }));
                    
                    var location = await Geolocation.GetLocationAsync();
                    
                    if (location != null)
                    {
                        Device.BeginInvokeOnMainThread(new Action(() =>
                        {
                            this.indicator.IsVisible = false;
                        }));

                        LocationInfo locationInfo = new LocationInfo();
                        locationInfo.Name = result;
                        locationInfo.Latitude = location.Latitude.ToString();
                        locationInfo.Longitude = location.Longitude.ToString();

                        AddLocationData(locationInfo);

                        locationInfos.Add(locationInfo);

                        SetPreferences(this.SaveKey, JsonConvert.SerializeObject(locationInfos));

                        Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    }
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    // Handle not supported on device exception
                }
                catch (FeatureNotEnabledException fneEx)
                {
                    // Handle not enabled on device exception
                }
                catch (PermissionException pEx)
                {
                    // Handle permission exception
                }
                catch (Exception ex)
                {
                    // Unable to get location
                }
            }
        }

        private void ClearButton_Clicked(object sender, EventArgs e)
        {
            SetPreferences(this.SaveKey, null);
            this.locationStackLayout.Children.Clear();
        }


        private void AddLocationData(LocationInfo locationInfo)
        {
            StackLayout stackLayout = new StackLayout() { Orientation = StackOrientation.Horizontal, HeightRequest = 50 };
            Label nameLabel = new Label() { TextColor = Color.Black, Text = locationInfo.Name, WidthRequest = 70, Margin = new Thickness(10, 0, 0, 0), VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
            Label latLabel = new Label() { TextColor = Color.Black, Text = "Latitude:" + locationInfo.Latitude, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
            Label lonLabel = new Label() { TextColor = Color.Black, Text = "Longitude:" + locationInfo.Longitude, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
            stackLayout.Children.Add(nameLabel);
            stackLayout.Children.Add(latLabel);
            stackLayout.Children.Add(lonLabel);

            this.locationStackLayout.Children.Add(stackLayout);
        }

        #region SetPreferences
        /// <summary>
        /// 값을 저장합니다.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetPreferences(string key, string value)
        {
            Preferences.Set(key, value);
        }
        #endregion

        #region GetPreferences
        /// <summary>
        /// 저장한 값을 가져옵니다.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetPreferences(string key, string defaultValue = null)
        {
            return Preferences.Get(key, defaultValue);
        }
        #endregion
    }

    public class LocationInfo
    {
        public string Name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}

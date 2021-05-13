﻿using Newtonsoft.Json;

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

            if (locationInfos.Where(c => c.Name == result).Any())
            {
                await DisplayAlert("이름중복", "위치 이름이 중복됩니다.", "예");
                return;
            }

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

        private async void ClearButton_Clicked(object sender, EventArgs e)
        {
            bool isYes = await DisplayAlert("삭제확인", "모두 제거 하시겠습니까?", "예", "아니오");
            if (isYes)
            {
                SetPreferences(this.SaveKey, null);
                this.locationStackLayout.Children.Clear();
            }
        }

        private void AddLocationData(LocationInfo locationInfo)
        {
            StackLayout stackLayout = new StackLayout() { Orientation = StackOrientation.Horizontal, HeightRequest = 50};
            Label nameLabel = new Label() { TextColor = Color.Black, Text = locationInfo.Name, WidthRequest = 70, Margin = new Thickness(10, 0, 0, 0), VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
            Label latLabel = new Label() { TextColor = Color.Black, Text = "Lat: " + locationInfo.Latitude, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
            Label lonLabel = new Label() { TextColor = Color.Black, Text = "Lon: " + locationInfo.Longitude, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
            stackLayout.Children.Add(nameLabel);
            stackLayout.Children.Add(latLabel);
            stackLayout.Children.Add(lonLabel);
            Button deleteButton = new Button() 
            { 
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 40, HeightRequest = 40, Text = "x", Margin = new Thickness(0, 0, 5, 0), 
                BackgroundColor = Color.White, BorderColor = Color.Black, TextColor = Color.Black, BorderWidth = 0, ClassId= locationInfo.Name
            };
            deleteButton.Clicked += DeleteButton_Clicked;
            stackLayout.Children.Add(deleteButton);
            this.locationStackLayout.Children.Add(stackLayout);
        }

        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            string name = ((Button)sender).ClassId;

            bool isYes = await DisplayAlert("삭제확인", "'" + name + "' 항목을 제거 하시겠습니까?", "예", "아니오");
            if (isYes)
            {
                var deleteitem = locationInfos.Where(c => c.Name == name).FirstOrDefault();
                if (deleteitem != null)
                {
                    locationInfos.Remove(deleteitem);

                    SetPreferences(this.SaveKey, JsonConvert.SerializeObject(locationInfos));

                    this.locationStackLayout.Children.Clear();
                    foreach (var locationInfo in this.locationInfos)
                    {
                        AddLocationData(locationInfo);
                    }
                }
            }
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

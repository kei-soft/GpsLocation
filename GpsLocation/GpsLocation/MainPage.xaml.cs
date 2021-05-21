using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace GpsLocation
{
    public partial class MainPage : ContentPage
    {
        #region Field
        /// <summary>
        /// 위치정보 리스트입니다.
        /// </summary>
        List<LocationInfo> locationInfos = new List<LocationInfo>();

        /// <summary>
        /// 저장 정보 키입니다.
        /// </summary>
        private string SaveKey = "GpsLocation";
        #endregion

        #region MainPage
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
        #endregion

        // Events
        #region GetButton_Clicked
        private void GetButton_Clicked(object sender, EventArgs e)
        {
            GetLocation();
        }
        #endregion
        #region DeleteButton_Clicked
        private void DeleteButton_Clicked(object sender, EventArgs e)
        {
            string name = ((Button)sender).ClassId;
            DeleteAction(name);
        }
        #endregion
        #region ClearButton_Clicked
        private async void ClearButton_Clicked(object sender, EventArgs e)
        {
            bool isYes = await DisplayAlert("삭제확인", "모두 제거 하시겠습니까?", "예", "아니오");
            if (isYes)
            {
                SetPreferences(this.SaveKey, null);
                this.locationStackLayout.Children.Clear();
            }
        }
        #endregion

        // Methods
        #region GetLocation
        /// <summary>
        /// 현재 위치 정보를 가져옵니다.
        /// </summary>
        private async void GetLocation()
        {
            string result = await DisplayPromptAsync("Location Name", "Input Location Name");

            if (locationInfos.Where(c => c.Name == result).Any())
            {
                await DisplayAlert("Location Name Duplicate", "Duplicate location name.", "OK");
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
        #endregion
        #region AddLocationData
        /// <summary>
        /// 위치 정보를 추가합니다.
        /// </summary>
        /// <param name="locationInfo"></param>
        private void AddLocationData(LocationInfo locationInfo)
        {
            StackLayout stackLayout = new StackLayout() { Orientation = StackOrientation.Horizontal, HeightRequest = 50};
            
            Image editImage = new Image()
            {
                Source = "EditImage.png",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 20,
                HeightRequest = 20,
                Margin = new Thickness(0, 0, 5, 0),
                BackgroundColor = Color.White,
                ClassId = locationInfo.Name
            };

            TapGestureRecognizer editTapGestureRecognizer = new TapGestureRecognizer();
            editTapGestureRecognizer.Tapped += async (s, e) =>
            {
                string name = ((Image)s).ClassId;

                string result = await DisplayPromptAsync("Edit Location Name", "Input Location Name", initialValue: name);

                if (result != null && result != name)
                {
                    if (locationInfos.Where(c => c.Name == result).Any())
                    {
                        await DisplayAlert("Location Name Duplicate", "Duplicate Location Name.", "OK");
                        return;
                    }
                    else
                    {
                        var editItem = locationInfos.Where(c => c.Name == name).FirstOrDefault();

                        if (editItem != null)
                        {
                            editItem.Name = result;
                            SetPreferences(this.SaveKey, JsonConvert.SerializeObject(locationInfos));

                            this.locationStackLayout.Children.Clear();
                            foreach (var item in this.locationInfos)
                            {
                                AddLocationData(item);
                            }
                        }
                    }
                }
            };

            editImage.GestureRecognizers.Add(editTapGestureRecognizer);

            TapGestureRecognizer copyTapGestureRecognizer = new TapGestureRecognizer();
            copyTapGestureRecognizer.Tapped += (s, e) =>
            {
                string clipdata = $"{locationInfo.Name} = lat : {locationInfo.Latitude}, lon : {locationInfo.Latitude}";
                Clipboard.SetTextAsync(clipdata);
                DisplayAlert("Copy Complete", clipdata, "OK");
            };

            Label nameLabel = new Label() { TextColor = Color.Black, Text = locationInfo.Name, WidthRequest = 70, Margin = new Thickness(10, 0, 0, 0), VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, FontSize = 14, FontAttributes = FontAttributes.Bold };
            Label latLabel = new Label() { TextColor = Color.Black, Text = "Lat: " + locationInfo.Latitude, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, FontSize = 15 };
            Label lonLabel = new Label() { TextColor = Color.Black, Text = "Lon: " + locationInfo.Longitude, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, FontSize = 15 };
            nameLabel.GestureRecognizers.Add(copyTapGestureRecognizer);
            latLabel.GestureRecognizers.Add(copyTapGestureRecognizer);
            lonLabel.GestureRecognizers.Add(copyTapGestureRecognizer);

            stackLayout.Children.Add(nameLabel);
            stackLayout.Children.Add(editImage);
            stackLayout.Children.Add(latLabel);
            stackLayout.Children.Add(lonLabel);

            Image image = new Image()
            {
                Source = "DeleteImage.png",
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 20,
                HeightRequest = 20,
                Margin = new Thickness(0, 0, 5, 0),
                BackgroundColor = Color.White,
                ClassId = locationInfo.Name
            };

            TapGestureRecognizer deleteTapGestureRecognizer = new TapGestureRecognizer();
            deleteTapGestureRecognizer.Tapped += (s,e) =>
            {
                string name = ((Image)s).ClassId;
                DeleteAction(name);
            };

            image.GestureRecognizers.Add(deleteTapGestureRecognizer);

            //Button deleteButton = new Button() 
            //{ 
            //    HorizontalOptions = LayoutOptions.EndAndExpand,
            //    VerticalOptions = LayoutOptions.Center,
            //    WidthRequest = 40, HeightRequest = 40, Text = "x", Margin = new Thickness(0, 0, 5, 0), 
            //    BackgroundColor = Color.White, BorderColor = Color.Black, TextColor = Color.Black, BorderWidth = 0, ClassId= locationInfo.Name
            //};
            //deleteButton.Clicked += DeleteButton_Clicked;
            stackLayout.Children.Add(image);
            this.locationStackLayout.Children.Add(stackLayout);
        }

        #endregion
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

        #region DeleteAction
        /// <summary>
        /// 아이템을 삭제합니다.
        /// </summary>
        /// <param name="name"></param>
        private async void DeleteAction(string name)
        {
            bool isYes = await DisplayAlert("Delete", "'" + name + "' Do you want to delete?", "Yes", "No");
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
        #endregion
    }

    #region LocationInfo Class
    /// <summary>
    /// 위치정보 데이터입니다.
    /// </summary>
    public class LocationInfo
    {
        /// <summary>
        /// 위치정보이름
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 경도
        /// </summary>
        public string Latitude { get; set; }
        /// <summary>
        /// 위도
        /// </summary>
        public string Longitude { get; set; }
    }
    #endregion
}

using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Weatherio.Core;
using Weatherio.Models;

namespace Weatherio {
    public partial class MainWindow : Window {
        private const string UnableUpdateMessage = "Unable to update weather";
        private const string UpdatingMessage = "Weather is updating...";

        private const string Celsius = "°C";
        private const string Speed = "m/s";
        private const string PressureHeight = "mm";

        private bool _isUpdating;

        public MainWindow() {
            InitializeComponent();
            var ni = new System.Windows.Forms.NotifyIcon {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                    Assembly.GetEntryAssembly().ManifestModule.Name),
                Visible = true,
                Text = Assembly.GetEntryAssembly().GetName().Name
            };
            ni.DoubleClick +=
                delegate {
                    Show();
                    WindowState = WindowState.Normal;
                };
        }

        protected override void OnStateChanged(EventArgs e) {
            if (WindowState == WindowState.Minimized)
                Hide();

            base.OnStateChanged(e);
        }

        private async void MainWindow_OnActivated(object sender, EventArgs e) {
            if (_isUpdating)
                return;
            _isUpdating = true;
            await UpdateWeather();
            _isUpdating = false;
        }

        private async Task UpdateWeather() {
            BeginUpdate();
            var weatherInfo = await GeoWeatherHelper.GetWeather();
            if (weatherInfo == null) {
                CancelUpdate();
                return;
            }

            SetWeatherValues(weatherInfo);
            EndUpdate();
        }

        private void BeginUpdate() {
            UpdatingLabel.Content = UpdatingMessage;
            UpdatingLabel.Visibility = Visibility.Visible;
            WeatherInfoPanel.Opacity = 0.5;
        }

        private void CancelUpdate() {
            UpdatingLabel.Content = UnableUpdateMessage;
        }

        private void EndUpdate() {
            UpdatingLabel.Visibility = Visibility.Hidden;
            WeatherInfoPanel.Opacity = 1;
        }

        private void SetWeatherValues(WeatherInfo weatherInfo) {
            SetTemp(weatherInfo.Temp);
            SetFeel(weatherInfo.FeelsLike);
            SetCondition(weatherInfo.Condition);
            SetWater(weatherInfo.TempWater);
            SetFeel(weatherInfo.FeelsLike);
            SetWind(weatherInfo.WindSpeed);
            SetPressure(weatherInfo.Pressure);
        }

        private void SetTemp(string temp) {
            TempLabel.Content = temp + Celsius;
        }

        private void SetWind(string wind) {
            WindLabel.Content = wind + Speed;
        }

        private void SetFeel(string feel) {
            FeelLabel.Content = feel + Celsius;
        }

        private void SetWater(string water) {
            WaterLabel.Content = water + Celsius;
        }

        private void SetCondition(string condtion) {
            ConditionLabel.Content = condtion.Replace("_", " ");
        }

        private void SetPressure(string pressure) {
            PressureLabel.Content = pressure + PressureHeight;
        }
    }
}
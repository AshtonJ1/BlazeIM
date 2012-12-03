using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Net;
using Newtonsoft.Json;

namespace BlazeGames.IM.Client
{
    internal class WeatherApi
    {
        WebClient ApiConnection;
        public WeatherData Data;

        string CurrentCity;
        System.Timers.Timer UpdateTimer;

        public event EventHandler<WeatherData> WeatherDataUpdated;
        
        public WeatherApi(string City)
        {
            CurrentCity = City;

            ApiConnection = new WebClient();
            ApiConnection.DownloadStringCompleted += ApiConnection_DownloadStringCompleted;
            ApiConnection.DownloadStringAsync(new Uri("http://openweathermap.org/data/2.1/find/name?q=" + City));

            UpdateTimer = new System.Timers.Timer(300000);
            UpdateTimer.Start();
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        public void ChangeCity(string City)
        {
            CurrentCity = City;

            if (ApiConnection.IsBusy)
                ApiConnection.CancelAsync();
            ApiConnection.DownloadStringAsync(new Uri("http://openweathermap.org/data/2.1/find/name?q=" + City));
        }

        public void StartAutoUpdate()
        {
            UpdateTimer.Start();
        }

        public void StopAutoUpdate()
        {
            UpdateTimer.Stop();
        }

        void ApiConnection_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                Data = JsonConvert.DeserializeObject<WeatherData>(e.Result);
                if (WeatherDataUpdated != null)
                    WeatherDataUpdated(this, Data);
            }
            catch { }
        }

        void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(ApiConnection.IsBusy)
                ApiConnection.CancelAsync();
            ApiConnection.DownloadStringAsync(new Uri("http://openweathermap.org/data/2.1/find/name?q=" + CurrentCity));
        }
    }

    internal class WeatherData : EventArgs
    {
        public string message { get; set; }
        public string type { get; set; }
        public string calctime { get; set; }
        public string units { get; set; }

        public int cod { get; set; }
        public int count { get; set; }

        public WeatherDataCityList[] list { get; set; }
    }

    internal class WeatherDataCityList
    {
        public int id { get; set; }
        public int dt { get; set; }
        public string name { get; set; }
        public string date { get; set; }
        public string url { get; set; }
        public WeatherDataCityCoordinates coord { get; set; }
        public WeatherDataCityTemperature main { get; set; }
        public WeatherDataCityWind wind { get; set; }
        public WeatherDataCityClouds clouds { get; set; }
        public WeatherDataCityWeather[] weather { get; set; }
        public WeatherDataCityInfo sys { get; set; }
    }

    internal class WeatherDataCityCoordinates
    {
        public decimal lat { get; set; }
        public decimal lon { get; set; }
    }

    internal class WeatherDataCityTemperature
    {
        public decimal temp { get; set; }
        public decimal temp_C { get { return temp - 273.15M; } }
        public decimal temp_F { get { return ((9.0M / 5.0M) * temp_C) + 32; } }
        public decimal temp_min { get; set; }
        public decimal temp_min_C { get { return temp_min - 273.15M; } }
        public decimal temp_min_F { get { return ((9.0M / 5.0M) * temp_min_C) + 32; } }
        public decimal temp_max { get; set; }
        public decimal temp_max_C { get { return temp_max - 273.15M; } }
        public decimal temp_max_F { get { return ((9.0M / 5.0M) * temp_max_C) + 32; } }
        public int pressure { get; set; }
        public int humidity { get; set; }
    }

    internal class WeatherDataCityWind
    {
        public decimal speed { get; set; }
        public int deg { get; set; }
    }

    internal class WeatherDataCityClouds
    {
        public int all { get; set; }
    }

    internal class WeatherDataCityWeather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
        public WeatherDataCondition Condition { get { return (WeatherDataCondition)id; } }
    }

    internal class WeatherDataCityInfo
    {
        public string country { get; set; }
        public int population { get; set; }
    }

    internal enum WeatherDataCondition
    {
        ThunderstormLightRain = 200,
        ThunderstormRain = 201,
        ThunderstormHeavyRain = 202,
        LightThunderstorm = 210,
        Thunderstorm = 211,
        HeavyThunderstorm = 212,
        RaggedThunderstorm = 221,
        ThunderstormLightDrizzle = 230,
        ThunderstormDrizzle = 231,
        ThunderstormHeavyDrizzle = 232,

        LightDrizzle = 300,
        Drizzle = 301,
        HeavyDrizzle = 302,
        LightDrizzleRain = 310,
        DrizzleRain = 311,
        HeavyDrizzleRain = 312,
        DrizzleShower = 321,

        LightRain = 500,
        ModerateRain = 501,
        HeavyRain = 502,
        VeryHeavyRain = 503,
        ExtremeRain = 504,
        FreezingRain = 511,
        LightRainShower = 520,
        RainShower = 521,
        HeavyRainShower = 522,

        LightSnow = 600,
        Snow = 601,
        HeavySnow = 602,
        Sleet = 611,
        SnowShower = 621,

        Mist = 701,
        Smoke = 711,
        Haze = 721,
        Dust = 731,
        Fog = 741,

        Clear = 800,
        FewClouds = 801,
        ScatteredClouds = 802,
        BrokenClouds = 803,
        Overcast = 804,

        ExtremeTornado = 900,
        ExtremeTropicalStorm = 901,
        ExtremeHurricane = 902,
        ExtremeCold = 903,
        ExtremeHot = 904,
        ExtremeWindy = 905,
        ExtremeHail = 906
    }
}

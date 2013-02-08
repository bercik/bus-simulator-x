using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Testy_mapy
{
    class EnvironmentSimulation
    {
        public int hour { get; private set; } // godzina
        public int minute { get; private set; } // minuta

        private double time = 0.0f; // czas który upłynął od ostatnio dodanej minuty

        // zachod slonca:
        private bool sunset; // czy wystepuje efekt zachodu slonca
        private bool setSunset; // czy wylosowano juz czy ma nastapic efekt zachodu slonca

        // deszcz:
        private bool rain; // czy pada deszcz
        private int weatherChangeTimer; // ile czasu uplynelo od zmiany pogody (w minutach)
        private int weatherChangeTime; // ile czasu ma trwac dana pogoda (w minutach)

        // global light
        private Vector4 globalLightColor;

        public EnvironmentSimulation()
        {
            hour = 0;
            minute = 0;

            setSunset = false;
            sunset = false;
            rain = false;
            weatherChangeTime = 0;
            weatherChangeTimer = 0;
        }

        public EnvironmentSimulation(int minute, int hour) : this()
        {
            SetTime(minute, hour);
        }

        public void Update(TimeSpan frameInterval, ref ParticlesLogic particlesLogic)
        {
            time += frameInterval.TotalSeconds;

            if (time > GameParams.timeBeetwenOneMinute)
            {
                AddMinute();
                time -= GameParams.timeBeetwenOneMinute;

                SetSunset();
                SetRain(ref particlesLogic);
            }
        }

        public string GetCurrentTime()
        {
            return hour.ToString("00") + ":" + minute.ToString("00");
        }

        public void SetTime(int minute, int hour)
        {
            this.minute = minute;
            this.hour = hour;

            SetGlobalLightColor();
        }

        public Vector4 GetGlobalLightColor()
        {
            return globalLightColor;
        }

        public bool EnableLampadaire()
        {
            if (globalLightColor.Y < GameParams.lampadaireGlobalLightToTurnOnTheLights)
                return true;
            else
                return false;
        }

        private void SetGlobalLightColor()
        {
            float red = 1.0f, greenBlue = 1.0f;

            float fHour = hour + (minute * 0.0166f);

            if (sunset)
            {
                if (fHour >= 18 && fHour <= 22.5f)
                    red = -0.177f * fHour + 4.2f;
                else if (fHour > 22.5f || fHour < 3)
                    red = 0.2f;

                if (fHour >= 18 && fHour <= 21)
                    greenBlue = -0.266f * fHour + 5.8f;
                else if (fHour > 21 || fHour < 3)
                    greenBlue = 0.2f;
            }
            else if (!sunset)
            {
                if (fHour >= 18 && fHour <= 21)
                    red = greenBlue = -0.266f * fHour + 5.8f;
                else if (fHour > 21 || fHour < 3)
                    red = greenBlue = 0.2f;
            }

            if (fHour >= 3 && fHour <= 6)
            {
                red = greenBlue = 0.266f * fHour - 0.6f;
            }
            else if (fHour > 6 && fHour < 18)
            {
                red = greenBlue = 1.0f;
            }

            globalLightColor = new Vector4(red, greenBlue, greenBlue, 1);
        }

        private void SetSunset()
        {
            if (hour == 17 && !setSunset)
            {
                if (rain)
                {
                    sunset = false;
                }
                else
                {
                    if (Helper.random.NextDouble() < GameParams.sunsetProbability)
                        sunset = true;
                    else
                        sunset = false;
                }

                setSunset = true;
            }
            if (hour == 18)
            {
                setSunset = false;
            }
        }

        private void SetRain(ref ParticlesLogic particlesLogic)
        {
            if (!(sunset && hour >= 18 && hour <= 22) && weatherChangeTimer > weatherChangeTime)
            {
                // ustawiamy liczniki:
                weatherChangeTimer = 0;
                weatherChangeTime = Helper.random.Next(GameParams.minWeatherChangeTime, GameParams.maxWeatherChangeTime);

                // losujemy pogode:
                if (Helper.random.NextDouble() < GameParams.rainProbability)
                {
                    rain = true;
                    particlesLogic.EnableRain();
                }
                else
                {
                    rain = false;
                    particlesLogic.DisableRain();
                }
            }
        }

        private void AddMinute()
        {
            ++minute;
            ++weatherChangeTimer;

            if (minute > 59)
            {
                minute -= 60;
                ++hour;

                if (hour > 23)
                {
                    hour -= 24;
                }
            }

            SetGlobalLightColor();
        }
    }
}

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

        public EnvironmentSimulation()
        {
            hour = 0;
            minute = 0;
        }

        public EnvironmentSimulation(int minute, int hour)
        {
            SetTime(minute, hour);
        }

        public void Update(TimeSpan frameInterval)
        {
            time += frameInterval.TotalSeconds;

            if (time > GameParams.timeBeetwenOneMinute)
            {
                AddMinute();
                time -= GameParams.timeBeetwenOneMinute;
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
        }

        public Vector4 GetGlobalLightColor()
        {
            float radians = MathHelper.ToRadians((hour + minute * 0.01666f) * 15);
            float value = (float)Math.Abs(Math.Sin(radians / 2));

            if (value < 0.1f)
                value = 0.1f;

            return new Vector4(value, value, value, 1);
        }

        private void AddMinute()
        {
            ++minute;

            if (minute > 59)
            {
                minute -= 60;
                ++hour;

                if (hour > 23)
                {
                    hour -= 24;
                }
            }
        }
    }
}

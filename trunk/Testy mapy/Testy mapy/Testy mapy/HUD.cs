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
    struct InformationForHud
    {
        public float busSpeed; // predkosc autobusu
        public float busRpm; // obroty silnika autobusu
        public int busGear; // bieg autobusu
    }

    class HUD
    {
        // zmienne do ustawiania:
        float speedometerScale = 1.0f; // skala predkosciomierza (rozmiar tekstury jest mnozony przez skale)
        float tachometerScale = 1.0f; // skala obrotomierza (rozmiar tekstury jest mnozony przez skale)
        float tipScaleForSpeedometer = 0.65f; // skala wskazówki dla predkosciomierza
        float gearFrameScale = 1.0f; // skala ramki dla biegow
        Vector2 tipPosForSpeedometer = new Vector2(75, 75); // pozycja wzgledna wskazowki na predkosciomierzu

        // speedometer:
        Texture2D speedometerTexture;
        Rectangle speedometerRect;
        float[] steps = new float[] { 0.009f, -0.1f, 0.08f, 0.07f, 0.0f, 0.02f, 0.0f, 0.01f, 0.01f, 0.005f, 0.0f, 0.005f, 0.0f, 0.005f }; // mnozniki dla kolejnych "faz" predkosci

        // tachometer:
        Texture2D tachometerTexture;
        Rectangle tachometerRect;

        // tip:
        Texture2D tipTexture;
        Rectangle tipRectForSpeedometer;
        Vector2 tipOrigin = new Vector2(8, 99);

        // gearFrame:
        Texture2D gearFrameTexture;
        Rectangle gearFrameRect;
        SpriteFont gearFont;

        public HUD()
        {

        }

        public void LoadContent(ContentManager content)
        {
            // zmienne pomocnicze:
            Point point;
            Size size;
            int actualPosX = 0; // aktualna pozycja X elementuInterfejsu

            // tekstura wskazówki:
            tipTexture = content.Load<Texture2D>("HUD/tip");

            // tachometer:
            tachometerTexture = content.Load<Texture2D>("HUD/tachometer");
            size = new Size((int)(tachometerTexture.Width * tachometerScale), (int)(tachometerTexture.Height * tachometerScale));
            point = new Point(actualPosX, (int)(Helper.screenSize.Y - size.Height));
            tachometerRect = new Rectangle(point.X, point.Y, size.Width, size.Height);
            actualPosX += size.Width;

            // speedometer:
            speedometerTexture = content.Load<Texture2D>("HUD/speedometer");
            size = new Size((int)(speedometerTexture.Width * speedometerScale), (int)(speedometerTexture.Height * speedometerScale));
            point = new Point(actualPosX, (int)(Helper.screenSize.Y - size.Height));
            speedometerRect = new Rectangle(point.X, point.Y, size.Width, size.Height);
            actualPosX += size.Width;

            tipPosForSpeedometer *= speedometerScale;
            tipPosForSpeedometer += new Vector2(point.X, point.Y);
            tipScaleForSpeedometer *= speedometerScale;
            size = new Size((int)(tipTexture.Width * tipScaleForSpeedometer), (int)(tipTexture.Height * tipScaleForSpeedometer));
            tipRectForSpeedometer = new Rectangle((int)tipPosForSpeedometer.X, (int)tipPosForSpeedometer.Y, size.Width, size.Height);

            // ramka dla biegów:
            gearFont = content.Load<SpriteFont>("fonts/gearFont");

            gearFrameTexture = content.Load<Texture2D>("HUD/gear_frame");
            size = new Size((int)(gearFrameTexture.Width * gearFrameScale), (int)(gearFrameTexture.Height * gearFrameScale));
            point = new Point(actualPosX, (int)(Helper.screenSize.Y - size.Height));
            gearFrameRect = new Rectangle(point.X, point.Y, size.Width, size.Height);
            actualPosX += size.Width;
        }

        public void Draw(SpriteBatch spriteBatch, InformationForHud infoForHud)
        {
            // tachometer:
            spriteBatch.Draw(tachometerTexture, tachometerRect, Color.White);
            
            // speedometer:
            spriteBatch.Draw(speedometerTexture, speedometerRect, Color.White);

            float rotation = GetSpeedometerTipRotation(infoForHud.busSpeed);
            spriteBatch.Draw(tipTexture, tipRectForSpeedometer, null, Color.White, MathHelper.ToRadians(rotation), tipOrigin, SpriteEffects.None, 1.0f);

            // gear frame:
            spriteBatch.Draw(gearFrameTexture, gearFrameRect, Color.White);
            string gearName = GetGearName(infoForHud.busGear);
            Vector2 gearPos = GetGearPos(gearName);
            spriteBatch.DrawString(gearFont, gearName, gearPos, Color.White);
        }

        protected float GetSpeedometerTipRotation(float busSpeed)
        {
            busSpeed = Math.Abs(busSpeed);

            float multipler = 2.77f; // mnoznik predkosci autobusu uzywany do ustawienie rotacji wskazowki predkosciomierza
            float step = 5.0f; // co ile ma nastepowac krok

            for (int i = 0; i < steps.Length; ++i)
            {
                if (busSpeed - step * i > 0.0f)
                {
                    multipler += (((busSpeed - step * i > step) ? step : busSpeed - step * i) * steps[i]);
                }
                else
                {
                    break;
                }
            }

            return 180.0f + (busSpeed * multipler);
        }

        protected string GetGearName(int gear)
        {
            if (gear == 0)
                return "R";
            else
                return gear.ToString();
        }

        protected Vector2 GetGearPos(string gear)
        {
            Vector2 gearSize = gearFont.MeasureString(gear);

            float x = gearFrameRect.X + ((gearFrameRect.Width / 2) - (gearSize.X / 2));
            float y = gearFrameRect.Y + ((gearFrameRect.Height / 2) - (gearSize.Y / 2));

            return new Vector2(x, y);
        }
    }
}

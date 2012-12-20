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
    enum PedestrianState { GettingIn, GettingOff, Nothing } // stan pieszych (wsiadają, wysiadają, nic nie robią)

    struct InformationForHud
    {
        public float busSpeed; // predkosc autobusu
        public float busRpm; // obroty silnika autobusu
        public int busGear; // bieg autobusu

        public Vector2 busPosition;     // pozycja autobusu
        public Vector2 busStopPosition; // pozycja przystanku

        public PedestrianState pedestrianState; // stan pieszych
        public int numberOfPedestriansInTheBus; // liczba pieszych w autobusie
        public bool doorOpen; // czy drzwi sa otwarte

    }

    class HUD
    {
        // zmienne do ustawiania:
        float speedometerScale = 1.0f; // skala predkosciomierza (rozmiar tekstury jest mnozony przez skale)
        float tachometerScale = 1.0f; // skala obrotomierza (rozmiar tekstury jest mnozony przez skale)
        float tipScaleForSpeedometer = 0.65f; // skala wskazówki dla predkosciomierza
        float gearFrameScale = 1.0f; // skala ramki dla biegow
        float rightGuiScale = 1.0f; // skala prawego GUI (wyświetlającego stan drzwi, pieszych i ich ilość)
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
        
        // minimap !!! DO USUNIECIA !!! (chwilowo - pogladowo):
        Texture2D minimapTexture;
        Rectangle minimapRect;

        // arrow
        Texture2D directionArrowTexture;
        Vector2 directionArrowTextureOrigin, directionArrowSize = new Vector2(100, 30);

        // door state:
        Texture2D doorOpenTexture;
        Texture2D doorCloseTexture;
        Rectangle doorStateRect;

        // pedestrian state:
        Texture2D pedestriansGettingIn;
        Texture2D pedestriansGettingOff;
        Texture2D pedestriansNothing;
        Rectangle pedestriansStateRect;

        // pedestrians in bus frame:
        Texture2D pedestriansInBusFrameTexture;
        Rectangle pedestriansInBusFrameRect;
        SpriteFont pedestriansInBusFont;

        public HUD()
        {

        }

        public void LoadContent(ContentManager content)
        {
            // zmienne pomocnicze:
            Point point;
            Size size;
            int actualPosX = 0; // aktualna pozycja X elementu Interfejsu
            int actualPosY = 0; // aktualna pozycja Y elementu Interfejsu

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

            // strzalka kierunku
            directionArrowTexture = content.Load<Texture2D>("HUD/direction_arrow");
            directionArrowTextureOrigin = new Vector2(directionArrowTexture.Width / 2, directionArrowTexture.Height / 2);

            // minimapa:
            minimapTexture = content.Load<Texture2D>("HUD/minimap");
            size = new Size((int)(minimapTexture.Width), (int)(minimapTexture.Height));
            actualPosX = (int)(Helper.screenSize.X - size.Width); // ustawiamy HUD od prawej krawedzi ekranu
            point = new Point(actualPosX, (int)(Helper.screenSize.Y - size.Height));
            minimapRect = new Rectangle(point.X, point.Y, size.Width, size.Height);

            // door state:
            doorCloseTexture = content.Load<Texture2D>("HUD/door_close");
            doorOpenTexture = content.Load<Texture2D>("HUD/door_open");
            size = new Size((int)(doorOpenTexture.Width * rightGuiScale), (int)(doorOpenTexture.Height * rightGuiScale));
            actualPosX -= size.Width;
            actualPosY = (int)(Helper.screenSize.Y - size.Height);
            point = new Point(actualPosX, actualPosY);
            doorStateRect = new Rectangle(point.X, point.Y, size.Width, size.Height);

            // pedestrian state:
            pedestriansGettingIn = content.Load<Texture2D>("HUD/pedestrians_getting_in");
            pedestriansGettingOff = content.Load<Texture2D>("HUD/pedestrians_getting_off");
            pedestriansNothing = content.Load<Texture2D>("HUD/pedestrians_nothing");
            size = new Size((int)(pedestriansNothing.Width * rightGuiScale), (int)(pedestriansNothing.Height * rightGuiScale));
            actualPosY -= size.Height;
            point = new Point(actualPosX, actualPosY);
            pedestriansStateRect = new Rectangle(point.X, point.Y, size.Width, size.Height);

            // number of pedestrians in bus
            pedestriansInBusFont = content.Load<SpriteFont>("fonts/pedestriansInBusFont");

            pedestriansInBusFrameTexture = content.Load<Texture2D>("HUD/pedestrians_in_bus_frame");
            size = new Size((int)(pedestriansInBusFrameTexture.Width * rightGuiScale), (int)(pedestriansInBusFrameTexture.Height * rightGuiScale));
            actualPosY -= size.Height;
            point = new Point(actualPosX, actualPosY);
            pedestriansInBusFrameRect = new Rectangle(point.X, point.Y, size.Width, size.Height);
        }

        public void Draw(SpriteBatch spriteBatch, InformationForHud infoForHud)
        {
            // zmienna pomocnicza:
            Vector2 textPos;

            // tachometer:
            spriteBatch.Draw(tachometerTexture, tachometerRect, Color.White);
            
            // speedometer:
            spriteBatch.Draw(speedometerTexture, speedometerRect, Color.White);

            float rotation = GetSpeedometerTipRotation(infoForHud.busSpeed);
            spriteBatch.Draw(tipTexture, tipRectForSpeedometer, null, Color.White, MathHelper.ToRadians(rotation), tipOrigin, SpriteEffects.None, 1.0f);

            // gear frame:
            spriteBatch.Draw(gearFrameTexture, gearFrameRect, Color.White);
            string gearName = GetGearName(infoForHud.busGear);
            textPos = GetTextPos(gearName, gearFont, gearFrameRect);
            spriteBatch.DrawString(gearFont, gearName, textPos, Color.White);

            // minimap:
            spriteBatch.Draw(minimapTexture, minimapRect, Color.White);

            // door state:
            Texture2D doorStateTexture;

            if (infoForHud.doorOpen)
            {
                doorStateTexture = doorOpenTexture;
            }
            else
            {
                doorStateTexture = doorCloseTexture;
            }

            spriteBatch.Draw(doorStateTexture, doorStateRect, Color.White);

            // pedestrian state:
            Texture2D pedestriansStateTexture;

            switch (infoForHud.pedestrianState)
            {
                case PedestrianState.GettingOff:
                    pedestriansStateTexture = pedestriansGettingOff;
                    break;
                case PedestrianState.GettingIn:
                    pedestriansStateTexture = pedestriansGettingIn;
                    break;
                case PedestrianState.Nothing:
                    pedestriansStateTexture = pedestriansNothing;
                    break;
                default:
                    pedestriansStateTexture = pedestriansNothing;
                    break;
            }

            spriteBatch.Draw(pedestriansStateTexture, pedestriansStateRect, Color.White);

            // number of pedestrians in bus:
            spriteBatch.Draw(pedestriansInBusFrameTexture, pedestriansInBusFrameRect, Color.White);
            string numberOfPedestriansInBus = infoForHud.numberOfPedestriansInTheBus.ToString();
            textPos = GetTextPos(numberOfPedestriansInBus, pedestriansInBusFont, pedestriansInBusFrameRect);
            spriteBatch.DrawString(pedestriansInBusFont, numberOfPedestriansInBus, textPos, Color.White);

            // strzalka
            Object arrow = GetArrow(infoForHud.busPosition, infoForHud.busStopPosition);
            Rectangle rect = Helper.CalculateScaleRectangle(arrow.pos, arrow.size);
            spriteBatch.Draw(directionArrowTexture, rect, null, Color.White, arrow.rotate, directionArrowTextureOrigin, SpriteEffects.None, 1);
        }

        protected Object GetArrow(Vector2 busPosition, Vector2 busStopPosition)
        {
            int arrowDistance = 150;
            Vector2 arrowPosition = Helper.screenOrigin;
            float arrowDirection = Helper.CalculateDirection(busPosition, busStopPosition);

            arrowPosition.X += (arrowDistance * (float)Math.Sin(MathHelper.ToRadians(arrowDirection)));
            arrowPosition.Y -= (arrowDistance * (float)Math.Cos(MathHelper.ToRadians(arrowDirection)));

            Object arrow = new Object("", arrowPosition, directionArrowSize, MathHelper.ToRadians(arrowDirection));
            
            return arrow;
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

        protected Vector2 GetTextPos(string gear, SpriteFont font, Rectangle rect)
        {
            Vector2 gearSize = font.MeasureString(gear);

            float x = rect.X + ((rect.Width / 2) - (gearSize.X / 2));
            float y = rect.Y + ((rect.Height / 2) - (gearSize.Y / 2));

            return new Vector2(x, y);
        }
    }
}

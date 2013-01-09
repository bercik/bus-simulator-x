using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    enum AreaType { village = 1, suburbs = 2, town = 3 } // typ obszaru (wieś, przedmieścia, miasto)

    class Area
    {
        Rectangle[] areaRectangles; // prostokaty obszarowe
        public string name { get; private set; } // nazwa obszaru
        public AreaType areaType { get; private set; } // typ obszaru

        public Area(string name, AreaType areaType, Rectangle[] areaRectangles)
        {
            this.name = name;
            this.areaType = areaType;
            this.areaRectangles = areaRectangles;
        }

        public bool ContainPoint(Vector2 point)
        {
            Point p = new Point((int)point.X, (int)point.Y);

            for (int i = 0; i < areaRectangles.Length; ++i)
            {
                if (areaRectangles[i].Contains(p))
                {
                    return true;
                }
            }

            return false;
        }
    }

    class AreasLogic
    {
        List<Area> areas; // lista wszystkich obszarów
        int actualAreaIndex = -1; // indeks aktualnego obszaru

        float lastUpdateTime = 100.0f; // ostatni czas update
        const int updateTime = 100; // co ile nalezy wykonac update (w milisekundach)

        float currentWaitingTime = 0.0f; // aktualny czas oczekiwania na sprawdzanie obszarów, po zmienieniu obszaru
        const int waitTime = 4000; // czas oczekiwania na sprawdzanie obszarów, po zmienieniu obszaru (w milisekundach)
        bool showText = false; // czy pokazywać tekst nazwy zmienionego obszaru
        float transparency = 0.0f; // przeźroczystość tekstu
        bool hiding = false; // czy ukrywamy tekst

        Color[] textColors; // kolory tekstu dla różnych typów obszaru

        public AreasLogic()
        {
            areas = new List<Area>();
            textColors = new Color[]{ Color.YellowGreen, Color.Yellow, Color.Red };
        }

        /// <summary>
        /// Metoda update. Zwraca typ obszaru w jakim aktualnie się znajdujemy
        /// </summary>
        /// <param name="framesInterval"></param>
        /// <returns></returns>
        public void Update(TimeSpan framesInterval, ref TrafficLogic trafficLogic)
        {
            if (lastUpdateTime > updateTime)
            {
                lastUpdateTime = 0.0f;

                if (CheckIsChangeAreaType())
                {
                    ChangeTraffic(ref trafficLogic);
                }
            }
            else
            {
                lastUpdateTime += (float)framesInterval.TotalMilliseconds;

                if (actualAreaIndex == -1)
                {
                    return;
                }
            }
        }

        private void ChangeTraffic(ref TrafficLogic trafficLogic)
        {
            switch (areas[actualAreaIndex].areaType)
            {
                case AreaType.village:
                    trafficLogic.SetTrafficDensityLow();
                    break;
                case AreaType.suburbs:
                    trafficLogic.SetTrafficDensityMedium();
                    break;
                case AreaType.town:
                    trafficLogic.SetTrafficDensityHigh();
                    break;
            }
        }

        public void GetTextAndColorToShow(TimeSpan framesInterval, out string text, out Color color)
        {
            if (showText)
            {
                text = areas[actualAreaIndex].name;
                Color c = textColors[(int)areas[actualAreaIndex].areaType - 1]; // wybieramy kolor na podstawie typu obszaru
                color = new Color((byte)c.R, (byte)c.G, (byte)c.B, (byte)transparency);

                currentWaitingTime += (float)framesInterval.TotalMilliseconds;
                UpdateTransparency((float)framesInterval.TotalMilliseconds);

                if (currentWaitingTime > waitTime)
                {
                    showText = false;
                    currentWaitingTime = 0.0f;
                    transparency = 0.0f;
                    hiding = false;
                }
            }
            else
            {
                text = "";
                color = Color.White;
            }
        }

        private void UpdateTransparency(float milisecondsInterval)
        {
            if (!hiding && (waitTime - currentWaitingTime) <= 1020)
                hiding = true;

            if (transparency < 255.0f && !hiding)
            {
                transparency += milisecondsInterval / 4;

                if (transparency > 255.0f)
                    transparency = 255.0f;
            }
            else if (transparency > 0.0f && hiding)
            {
                transparency -= milisecondsInterval / 4;

                if (transparency < 0.0f)
                    transparency = 0.0f;
            }
        }

        private bool CheckIsChangeAreaType()
        {
            // jezeli nie pokazujemy aktualnie tekstu o zmienieniu obszaru (jezeli tak to czekamy az ten tekst zniknie i wtedy wznawiamy sprawdzanie)
            if (!showText)
            {
                if (actualAreaIndex != -1 && areas[actualAreaIndex].ContainPoint(Helper.mapPos))
                {
                    return false;
                }
                for (int i = 0; i < areas.Count; ++i)
                {
                    if (i == actualAreaIndex)
                    {
                        continue;
                    }
                    else if (areas[i].ContainPoint(Helper.mapPos))
                    {
                        // tutaj ma się znaleźć metoda która wyświetla na ekranie informacje o wjechaniu do nowej strefy!
                        showText = true;

                        if (actualAreaIndex == -1 || (areas[actualAreaIndex].areaType != areas[i].areaType))
                        {
                            actualAreaIndex = i;
                            return true;
                        }
                        else
                        {
                            actualAreaIndex = i;
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        // laduje obszary z pliku
        public void LoadAreas(ref StreamReader sr)
        {
            areas.Clear();

            string s_object = sr.ReadLine(); // !!! ta linia to komentarz (DO USUNIECIA) !!!

            while ((s_object = sr.ReadLine()) != null && s_object[0] != '*')
            {
                AddArea(s_object);
            }
        }

        private void AddArea(string s_object)
        {
            string[] split = s_object.Split(';');

            string name = split[0];
            AreaType areaType = (AreaType)Int32.Parse(split[1]);
            int numberOfAreaRectangles = Int32.Parse(split[2]);

            // wczytywanie obszarów
            Rectangle[] areaRectangles = new Rectangle[numberOfAreaRectangles];
            int i = 3; // licznik pozycjii w split
            int j = 0; // licznik pozycjii w areaRectangles
            while (j < numberOfAreaRectangles)
            {
                int x = Int32.Parse(split[i]);
                int y = Int32.Parse(split[i + 1]);
                int width = Int32.Parse(split[i + 2]);
                int height = Int32.Parse(split[i + 3]);
                
                areaRectangles[j] = new Rectangle(x, y, width, height);

                i += 4;
                j += 1;
            }

            AddArea(name, areaType, areaRectangles);
        }

        private void AddArea(string name, AreaType areaType, Rectangle[] areaRectangles)
        {
            areas.Add(new Area(name, areaType, areaRectangles));
        }
    }
}

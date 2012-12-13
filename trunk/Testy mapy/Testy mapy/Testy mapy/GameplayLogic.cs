using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;

namespace Testy_mapy
{
    class GameplayLogic
    {
        /// <summary>
        /// This is the bus stop.
        /// </summary>
        protected class BusStop
        {
            public BusStop(int id, Vector2 stopArea, Vector2 waitingArea, Vector2 sign)
            {
                this.id = id;
                this.stopArea = stopArea;
                this.waitingArea = waitingArea;
                this.sign = sign;
            }

            public int id;
            public Vector2 stopArea;
            public Vector2 waitingArea;
            public Vector2 sign;
        }

        protected List<BusStop> busStops = new List<BusStop>();
        protected List<Int32> busStopsOrder;
        protected SettingsHandling settingsHandling = new SettingsHandling();

        /// <summary>
        /// Load map file.
        /// </summary>
        public void LoadMapFile(string fileName)
        {
            busStops = settingsHandling.LoadMap(fileName);
        }

        /// <summary>
        /// Load bus stops order file.
        /// </summary>
        public void LoadOrderFile(string fileName)
        {
            busStopsOrder = settingsHandling.LoadOrder(fileName);
        }

        protected class SettingsHandling
        {
            /// <summary>
            /// Check if the text is a comment (begins with //).
            /// </summary>
            private bool TextIsComment(string text)
            {
                if (text[0] == '/' && text[1] == '/')
                    return true;
                else
                    return false;
            }

            /// <summary>
            /// Load bus stops.
            /// </summary>
            public List<BusStop> LoadMap(string filePath)
            {
                filePath = "maps/" + filePath;
                List<BusStop> tempBusStops = new List<BusStop>();

                if (File.Exists(filePath))
                {
                    // Przygotuj StreamReader.
                    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    Encoding encoding = Encoding.GetEncoding("Windows-1250");
                    StreamReader streamReader = new StreamReader(fileStream, encoding);

                    // Zmienna pozwalająca na ustalenie czy czas rozpocząć odczytywanie.
                    bool beginningEncountered = false;

                    while (true)
                    {
                        // Odczytaj nową linię.
                        string line = streamReader.ReadLine();

                        if (line == null)
                        {
                            break;
                        }

                        // Jeśli rozpoczęto odczyt.
                        if (beginningEncountered)
                        {
                            // Jeśli należy zakończyć odczyt, wyjdź z pętli (koniec tekstu v koniec sekcji bus_stops).
                            if (line[0] == '*')
                            {
                                break;
                            }

                            // Jeśli odczytano tekst niebędący komentarzem.
                            if (!TextIsComment(line))
                            {
                                string[] split = line.Split(new char[] { ';' });
                                BusStop busStop = new BusStop(int.Parse(split[0]), new Vector2(float.Parse(split[1]), float.Parse(split[2])), new Vector2(float.Parse(split[3]), float.Parse(split[4])), new Vector2(float.Parse(split[5]), float.Parse(split[6])));

                                tempBusStops.Add(busStop);
                            }
                        }

                        if (line == "*busstops*")
                        {
                            beginningEncountered = true;
                        }
                    }
                }

                return tempBusStops;
            }

            /// <summary>
            /// Load the order of the bus stops.
            /// </summary>
            public List<Int32> LoadOrder(string filePath)
            {
                filePath = "maps/" + filePath;
                List<Int32> tempOrder = new List<Int32>();

                if (File.Exists(filePath))
                {
                    // Przygotuj StreamReader.
                    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    Encoding encoding = Encoding.GetEncoding("Windows-1250");
                    StreamReader streamReader = new StreamReader(fileStream, encoding);

                    string line;

                    // Przeliteruj przez wszystkie linie przypisując je do line.
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        // Jeśli odczytano tekst niebędący komentarzem.
                        if (!TextIsComment(line))
                        {
                            tempOrder.Add(Int32.Parse(line));
                        }
                    }
                }

                return tempOrder;
            }
        }

    }
}
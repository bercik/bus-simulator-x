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
        protected class BusStop
        {
            public BusStop(int id, Vector2 busArea, Vector2 waitingArea, Vector2 sign)
            {
                this.id = id;
                this.busArea = busArea;
                this.waitingArea = waitingArea;
                this.sign = sign;
            }

            public int id;
            public Vector2 busArea;
            public Vector2 waitingArea;
            public Vector2 sign;
        }

        protected List<BusStop> busStops = new List<BusStop>();
        protected SettingsHandling settingsHandling = new SettingsHandling();

        public bool LoadMapFile(string fileName)
        {
            return settingsHandling.LoadMap(fileName);
        }

        protected class SettingsHandling
        {
            public void Save(string file, List<BusStop> busStops)
            {
             
            }

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

            public bool LoadMap(string filePath)
            {
                filePath = "maps/" + filePath;

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

                            }
                        }

                        if (line == "*busStops*")
                        {
                            beginningEncountered = true;
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}
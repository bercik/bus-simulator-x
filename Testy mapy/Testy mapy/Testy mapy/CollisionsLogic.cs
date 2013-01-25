using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testy_mapy
{
    class CollisionsLogic
    {
        /// <summary>
        /// Checks if the objects are close enough to be checked.
        /// </summary>
        private bool ShouldBeChecked(Vector2 center1, Vector2 center2, Vector2 size1, Vector2 size2)
        {
            float minDistance = (float)Math.Sqrt(Math.Pow(size1.X, 2) + Math.Pow(size1.Y, 2)) + (float)Math.Sqrt(Math.Pow(size2.X, 2) + Math.Pow(size2.Y, 2));
            if (Helper.CalculateDistance(center1, center2) < minDistance + 1)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Prepare the rectangle using the 4 points given.
        /// </summary>
        private MyRectangle PrepareRectangle(Vector2[] collisionPoints)
        {
            return new MyRectangle(collisionPoints[3], collisionPoints[2], collisionPoints[1], collisionPoints[0]);
        }

        /// <summary>
        /// Main logic.
        /// </summary>
        public void HandleCollisions(TrafficLogic trafficLogic, BusLogic busLogic, GameplayLogic gameplayLogic)
        {
            Vector2[] busCollisionPoints = busLogic.GetCollisionPoints();
            MyRectangle busCollisionBox = PrepareRectangle(busCollisionPoints);

            foreach (TrafficLogic.Vehicle vehicle in trafficLogic.vehicles)
            {
                Vector2[] vehicleCollisionPoints = vehicle.GetCollisionPoints();
                MyRectangle vehicleCollisionBox = PrepareRectangle(vehicleCollisionPoints);

                foreach (TrafficLogic.Vehicle subVehicle in trafficLogic.vehicles)
                {
                    // Sprawdź pojazd - pojazd.
                    if (subVehicle != vehicle && ShouldBeChecked(vehicle.GetVehiclePosition(), subVehicle.GetVehiclePosition(), vehicle.GetVehicleSize(), subVehicle.GetVehicleSize()))
                    {
                        Vector2[] subVehicleCollisionPoints = subVehicle.GetCollisionPoints();
                        MyRectangle subVehicleCollisionBox = PrepareRectangle(subVehicleCollisionPoints);

                        // Sprawdź czy punkty kolizji subPojazdu są w collision boxie pojazdu
                        foreach (Vector2 point in vehicleCollisionPoints)
                        {
                            if (Helper.IsInside(point, subVehicleCollisionBox))
                            {
                                vehicle.Collision();
                                subVehicle.Collision();
                            }
                        }

                        // Sprawdź czy punkty kolizji pojazdu są w collision boxie subPojazdu
                        foreach (Vector2 point in subVehicleCollisionPoints)
                        {
                            if (Helper.IsInside(point, vehicleCollisionBox))
                            {
                                vehicle.Collision();
                                subVehicle.Collision();
                            }
                        }
                    }
                }

                // Sprawdź pojazd - autobus.
                if (ShouldBeChecked(vehicle.GetVehiclePosition(), busLogic.GetBusPosition(), vehicle.GetVehicleSize(), busLogic.GetSize()))
                {
                    foreach (Vector2 point in vehicleCollisionPoints)
                    {
                        if (Helper.IsInside(point, busCollisionBox))
                        {
                            vehicle.Collision();
                            busLogic.Collision();
                        }
                    }

                    foreach (Vector2 point in busCollisionPoints)
                    {
                        if (Helper.IsInside(point, vehicleCollisionBox))
                        {
                            vehicle.Collision();
                            busLogic.Collision();
                        }
                    }
                }
            }

            // Sprawdź autobus - piesi z gameplayLogic.
            foreach (GameplayLogic.BusStop busStop in gameplayLogic.busStops)
            {
                foreach (GameplayLogic.BusStop.Pedestrian pedestrian in busStop.pedestrians)
                {
                    if (pedestrian.collisionActive && !pedestrian.collision)
                    {
                        Vector2[] pedestrianCollisionPoints = pedestrian.GetCollisionPoints();
                        MyRectangle pedestrianCollisionBox = PrepareRectangle(pedestrianCollisionPoints);

                        // Sprawdź czy punkty kolizji subPojazdu są w collision boxie pieszego
                        foreach (Vector2 point in pedestrianCollisionPoints)
                        {
                            if (Helper.IsInside(point, busCollisionBox))
                            {
                                pedestrian.Collision();
                            }
                        }

                        // Sprawdź czy punkty kolizji pieszego są w collision boxie subPojazdu
                        foreach (Vector2 point in busCollisionPoints)
                        {
                            if (Helper.IsInside(point, pedestrianCollisionBox))
                            {
                                pedestrian.Collision();
                            }
                        }
                    }
                }

                foreach (GameplayLogic.BusStop.Pedestrian pedestrian in busStop.pedestriansWhoGotOff)
                {
                    if (pedestrian.collisionActive)
                    {
                        Vector2[] pedestrianCollisionPoints = pedestrian.GetCollisionPoints();
                        MyRectangle pedestrianCollisionBox = PrepareRectangle(pedestrianCollisionPoints);

                        // Sprawdź czy punkty kolizji subPojazdu są w collision boxie pojazdu
                        foreach (Vector2 point in pedestrianCollisionPoints)
                        {
                            if (Helper.IsInside(point, busCollisionBox))
                            {
                                pedestrian.Collision();
                            }
                        }

                        // Sprawdź czy punkty kolizji pojazdu są w collision boxie subPojazdu
                        foreach (Vector2 point in busCollisionPoints)
                        {
                            if (Helper.IsInside(point, pedestrianCollisionBox))
                            {
                                pedestrian.Collision();
                            }
                        }
                    }
                }
            }
        }
    }
}

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
        /// Main logic.
        /// </summary>
        public void HandleCollisions(TrafficLogic trafficLogic, BusLogic busLogic)
        {
            foreach (TrafficLogic.Vehicle vehicle in trafficLogic.vehicles)
            {
                Vector2[] vehicleCollisionPoints = vehicle.GetCollisionPoints();
                MyRectangle vehicleCollisionBox = new MyRectangle(vehicleCollisionPoints[3], vehicleCollisionPoints[2], vehicleCollisionPoints[1], vehicleCollisionPoints[0]);

                foreach (TrafficLogic.Vehicle subVehicle in trafficLogic.vehicles)
                {
                    // Sprawdź pojazd - pojazd.
                    if (subVehicle != vehicle && ShouldBeChecked(vehicle.GetVehiclePosition(), subVehicle.GetVehiclePosition(), vehicle.GetVehicleSize(), subVehicle.GetVehicleSize()))
                    {
                        Vector2[] subVehicleCollisionPoints = subVehicle.GetCollisionPoints();
                        MyRectangle subVehicleCollisionBox = new MyRectangle(subVehicleCollisionPoints[3], subVehicleCollisionPoints[2], subVehicleCollisionPoints[1], subVehicleCollisionPoints[0]);

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
                    Vector2[] busCollisionPoints = busLogic.GetCollisionPoints();
                    MyRectangle busCollisionBox = new MyRectangle(busCollisionPoints[3], busCollisionPoints[2], busCollisionPoints[1], busCollisionPoints[0]);

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
        }
    }
}

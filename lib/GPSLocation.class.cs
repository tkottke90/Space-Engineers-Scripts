using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class GPSlocation
        {
            private System.Text.RegularExpressions.Regex StoreFormat = new System.Text.RegularExpressions.Regex(@"([a-zA-Z\s0-9:{}$]*)");

            public string name;
            public Vector3D gps;
            public int fitness = 0;
            public int fitnessType = 0;
            public Dictionary<string, string> customInfo = new Dictionary<string, string>();

            public string eventLog = "";

            public GPSlocation(string newName, Vector3D newGPS)
            {
                name = newName;
                gps = newGPS;
            }

            public GPSlocation(string storedGPS)
            {

                // "<Origin^{X:0 Y:0 Z:0}^0^OriginType:Stationary$OriginComm:none>"

                System.Text.RegularExpressions.MatchCollection matches = StoreFormat.Matches(storedGPS); //storeGPS.Split('^');
                string[] attr = new string[matches.Count];
                matches.CopyTo(attr, 0);

                // Name
                name = attr[0];

                // GPS
                gps = recoverGPS(attr[1]);

                // Fitness
                int fit; bool fitCheck = Int32.TryParse(attr[2], out fit);
                if (fitCheck) { fitness = fit; } else { fitness = 0; }

                // Custom Info
                if (attr.Length == 2)
                {
                    string[] customAttr = attr[3].Split('$');
                    foreach (string str in customAttr)
                    {
                        str.Trim(' ');
                        if (str.Length > 3 || str != "")
                        {
                            //string strTest = str.Trim(new Char[]'>');
                            string[] temp = str.Split(':');
                            try
                            {
                                customInfo.Add(temp[0], temp[1]);
                            }
                            catch (Exception e)
                            {
                                eventLog += String.Format("Error Adding: {3}\r\n \tKey: {0}\r\n \tValue: {1}\r\n \r\n Stack Trace:\r\n{2}\r\n", temp[0], "value", e.ToString(), str);
                            }
                        }
                    }
                }
            }

            public MyWaypointInfo convertToWaypoint()
            {
                return new MyWaypointInfo(name, gps);
            }

            public Vector3D recoverGPS(string waypoint)
            {
                waypoint = waypoint.Trim(new Char[] { '{', '}' });
                string[] coord = waypoint.Split(' ');

                double x = double.Parse(coord[0].Split(':')[1]);
                double y = double.Parse(coord[1].Split(':')[1]);
                double z = double.Parse(coord[2].Split(':')[1]);

                return new Vector3D(x, y, z);
            }

            public bool checkNear(Vector3D gps2)
            {
                double deltaX = (gps.X > gps2.X) ? gps.X - gps2.X : gps2.X - gps.X;
                double deltaY = (gps.Y > gps2.Y) ? gps.Y - gps2.Y : gps2.Y - gps.Y;
                double deltaZ = (gps.Z > gps2.Z) ? gps.Z - gps2.Z : gps2.Z - gps.Z;

                if (deltaX < 200 || deltaY < 200 || deltaZ < 200) { return false; } else { return true; }
            }

            public string getCustomInfo(string infoName)
            {
                string output;
                return customInfo.TryGetValue(infoName, out output) ? output : null;
            }

            public bool setCustomInfo(string infoName, string newValue, bool createNew)
            {
                if (createNew)
                {
                    customInfo[infoName] = newValue;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override string ToString()
            {
                string custom = "";
                if (customInfo.Count != 0)
                {
                    foreach (KeyValuePair<string, string> item in customInfo)
                    {
                        custom += String.Format("{0}:{1}$", item.Key, item.Value);
                    }
                    custom = custom.TrimEnd('$');
                }
                else { custom = "0"; }

                string rtnString = String.Format("<{0}^{1}^{2}^{3}>", name, gps.ToString(), fitness, custom);
                return rtnString;
            }
        }

        public class Asteriod : GPSlocation {
            public Asteriod(string newName, Vector3D newGPS) : base(newName, newGPS) { }

            public Asteriod(string storedGPS) : base(storedGPS) { }


        }

        public class ShipStation : GPSlocation
        {


            public ShipStation(string newName, Vector3D newGPS) : base(newName, newGPS) { }

            public ShipStation(string storedGPS) : base(storedGPS) { }


        }
    }
}

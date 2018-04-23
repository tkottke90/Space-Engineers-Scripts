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
        public class MyData
        {
            public string name = "";
            Dictionary<string, string> data = new Dictionary<string, string>();

            System.Text.RegularExpressions.Regex approvedChars = new System.Text.RegularExpressions.Regex(@"([\sa-zA-Z0-9\-\%] *)");

            public MyData(string packetData, bool newData = true)
            {
                if (newData) {
                    this.name = packetData;
                } else {
                    string[] unpackedData = packetData.Split('\n');
                    name = unpackedData[0].Replace(">", "").Replace("<", "");

                    for(int i = 1; i < unpackedData.Length; i ++)
                    {
                        string[] kvpair = unpackedData[i].Split(':');
                        data.Add(kvpair[0], kvpair[1]);
                    }
                }

            }

            public bool AddData(string id, string value)
            {
                if (!data.ContainsKey(id)) {
                    data.Add(id, value);
                    return true;
                } else { return false; }
            }

            public bool EditData(string id, string value)
            {
                if (data.ContainsKey(id))
                {
                    data[id] = value;
                    return true;
                } else { return false; }
            }

            public bool GetData(string id, out string value)
            {
                return data.TryGetValue(id, out value);
            }

            public bool DeleteData(string id)
            {
                return data.Remove(id);
            }

            //public string[] Parse(string) {
                
            //}

            override
            public string ToString()
            {
                StringBuilder output = new StringBuilder();

                // Add Data Name:
                output.AppendFormat("<{0}>\n", name);

                foreach (string s in data.Keys)
                {
                    output.AppendFormat("{0}: {1}\n", s, data[s]);
                }

                output.AppendLine();
                return output.ToString();
            }
        }
    }
}


/*
 * 
 *  MyData MD = new MyData("test");

    MD.AddData("Value1", "An Error Occurred");
    MD.AddData("Value2", "No It Has Not");

    MyData GPSMarker = new MyData("GPSLocation");

    GPSMarker.AddData("Name", "Asteriod");
    GPSMarker.AddData("X", "100");
    GPSMarker.AddData("Y", "200");
    GPSMarker.AddData("Z", "0");

    string output = MD.ToString() + GPSMarker.ToString();

    Echo(MD.ToString() + GPSMarker.ToString());

            
            
    System.Text.RegularExpressions.Regex dataFlag = new System.Text.RegularExpressions.Regex(@"(<[a-zA-Z0-9]*>)([\sa-zA-Z0-9\-\:\n]*)");
    System.Text.RegularExpressions.MatchCollection dataPackets = dataFlag.Matches(output);

    Me.CustomData = "";

    for (int i = 0; i < dataPackets.Count; i++)
    {
        // Me.CustomData += String.Format("{0}) {1}", i, dataPackets[i].Value);
        string[] dataBits = dataPackets[i].Value.Split('\n');

        Me.CustomData += String.Format("Name: {0}\n", dataBits[0]);
        for (int j = 1; j < dataBits.Length; j++) { }
                
    }
 * 
 * 
 */

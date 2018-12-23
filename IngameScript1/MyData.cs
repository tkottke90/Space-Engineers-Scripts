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
            public string name;
            public Dictionary<string, string> data;


            public MyData(string name)
            {
                this.name = name;
                this.data = new Dictionary<string, string>();
            }

            public MyData(string name, Dictionary<string, string> newData)
            {
                this.name = name;
                this.data = newData;
            }

            public void AddData(string key, string value)
            {
                this.data.Add(key, value);
            }

            public void UpdateData(string key, string newValue)
            {
                this.data[key] = newValue;
            }

            public void DeleteData(string key)
            {
                this.data.Remove(key);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"<{this.name}>");
                foreach (string s in this.data.Keys)
                {
                    sb.AppendFormat("<{0}>{1}</{0}>", s, this.data[s]);
                }
                sb.Append($"</{this.name}>");

                return sb.ToString();
            }

            public static System.Text.RegularExpressions.Regex XMLTag = new System.Text.RegularExpressions.Regex(@"(<(?'tag'[a-zA-Z0-9-]*?)\s*(?'attr'[a-zA-Z0-9=]*?)>)(?'text'.*?)(</\k'tag'>)");


            public static Dictionary<string, string> ReadDataProperties(System.Text.RegularExpressions.MatchCollection matches) {
                Dictionary<string, string> props = new Dictionary<string, string>();
                for (int i = 0; i < matches.Count; i++)
                {
                    props.Add(matches[i].Groups["tag"].Value, matches[i].Groups["text"].Value);
                }
                return props;
            }

            public static List<MyData> ParseData(string storage)
            {
                List<MyData> d = new List<MyData>();

                System.Text.RegularExpressions.MatchCollection matches = XMLTag.Matches(storage);

                for(int i = 0; i < matches.Count; i++)
                {
                    // Get Match
                    System.Text.RegularExpressions.Match topElem = matches[i];
                    // Create new MyData object
                    d.Add(new MyData(topElem.Groups["tag"].Value, ReadDataProperties(XMLTag.Matches(topElem.Groups["text"].Value))));
                }

                return d;
            }

            {

            }

            public static bool FindDataInstance(string storage, string name, out MyData returnData)
            {
                returnData = ParseData(storage).Find((data) => data.name == name);

                return returnData != null;
            }

            public static bool FindDataWithProperty(string storage, string property, out List<MyData> returnData) 
            {
                returnData = ParseData(storage).Find(data => data.data.ContainsKey(property));

                return returnData.Count > 0;
            }

            public static bool UpdateDataInstance(string storage, MyData newData, out string updatedStorage)
            {
                try
                {
                    List<MyData> existingData = ParseData(storage);

                    if (existingData.Exists(data => data.name == newData.name))
                    {
                        existingData.RemoveAt(existingData.FindIndex(data => data.name == newData.name));
                        existingData.Add(newData);
                        updatedStorage = string.Join("", existingData);
                        return true;
                    } else
                    {
                        updatedStorage = string.Join("", existingData);
                        return false;
                    }
                } catch (Exception e)
                {
                    updatedStorage = string.Join("", ParseData(storage));
                    return false;
                }

            }

            
            public static bool DeleteDataInstance(string storage, string dataName, )
            
        }
    }
}

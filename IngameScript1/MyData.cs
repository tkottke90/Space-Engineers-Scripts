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
                sb.AppendLine("{" + this.name + "}");
                foreach (string s in this.data.Keys)
                {
                    sb.AppendFormat("<{0}:{1}>", s, this.data[s]).AppendLine();
                }


                return sb.ToString();
            }

            public static List<MyData> ParseStorage(string stringifiedData)
            {
                List<MyData> d = new List<MyData>();

                System.Text.RegularExpressions.Regex element = new System.Text.RegularExpressions.Regex(@"\{([a-zA-Z0-9_-]*)\}");
                System.Text.RegularExpressions.Regex dataPoint = new System.Text.RegularExpressions.Regex(@"<([0-9a-zA-z-_:\[\]\@\.\s]*)>");

                MyData newDat = new MyData("Init");

                foreach (string s in stringifiedData.Split('\n'))
                {
                    if (element.IsMatch(s))
                    {
                        d.Add(newDat);
                        newDat = new MyData(s.Replace("[", "").Replace("]", ""));
                    }
                    else if (dataPoint.IsMatch(s))
                    {
                        string key = s.Split(':')[0].Replace("<", "");
                        string value = s.Split(':')[1].Replace(">", "");
                        newDat.AddData(key, value);
                    }
                }
                return d;
            }

            public static bool FindDataInstance(string storage, string name, out MyData returnData)
            {
                returnData = null;

                System.Text.RegularExpressions.Regex element = new System.Text.RegularExpressions.Regex(@"\{" + name + @"\}");
                System.Text.RegularExpressions.Regex dataPoint = new System.Text.RegularExpressions.Regex(@"<([0-9a-zA-z-_:\[\]\@\.\s]*)>");
                bool matchFound = false;


                string[] splitStorage = storage.Split('\n');
                for (int i = 0; i < splitStorage.Length; i++)
                {
                    if (matchFound && dataPoint.IsMatch(splitStorage[i]))
                    {

                        string key = splitStorage[i].Split(':')[0].Replace("<", "");
                        string value = splitStorage[i].Split(':')[1].Replace(">", "");
                        returnData.AddData(key, value);
                        continue;
                    }

                    if (!matchFound && element.IsMatch(splitStorage[i]))
                    {
                        returnData = new MyData(splitStorage[i].Replace("{", "").Replace("}", ""));
                        matchFound = !matchFound;
                    }

                }

                return matchFound;
            }

            public static List<MyData> FindDataWithProperty(string storage, string propertyName)
            {
                List<MyData> d = new List<MyData>();

                System.Text.RegularExpressions.Regex element = new System.Text.RegularExpressions.Regex(@"\{([a-zA-Z0-9_-]*)\}");
                System.Text.RegularExpressions.Regex dataPoint = new System.Text.RegularExpressions.Regex(@"<([0-9a-zA-z-_:\[\]\@\.\s]*)>");

                MyData newDat = new MyData("Init");

                foreach (string s in storage.Split('\n'))
                {
                    if (element.IsMatch(s))
                    {
                        if (newDat.data.ContainsKey(propertyName))
                        {
                            d.Add(newDat);
                        }
                        newDat = new MyData(s.Replace("{", "").Replace("}", ""));
                    }
                    else if (dataPoint.IsMatch(s))
                    {
                        string key = s.Split(':')[0].Replace("<", "");
                        string value = s.Split(':')[1].Replace(">", "");
                        newDat.AddData(key, value);
                    }
                }

                foreach (MyData md in d)
                {
                    if (!md.data.ContainsKey(propertyName))
                    {
                        d.Remove(md);
                    }
                }
                return d;
            }

            public static string Pop(string storage, string name, out MyData returnData)
            {
                returnData = null;

                System.Text.RegularExpressions.Regex element = new System.Text.RegularExpressions.Regex(@"\{" + name + @"\}");
                System.Text.RegularExpressions.Regex dataPoint = new System.Text.RegularExpressions.Regex(@"<([0-9a-zA-z-_:\[\]\@\.\s]*)>");
                bool matchFound = false;

                List<string> splitStorage = storage.Split('\n').ToList();
                List<string> resultList = new List<string>();
                foreach (string s in splitStorage)
                {
                    if (matchFound && dataPoint.IsMatch(s))
                    {

                        string key = s.Split(':')[0].Replace("<", "");
                        string value = s.Split(':')[1].Replace(">", "");
                        returnData.AddData(key, value);
                        continue;
                    }

                    if (!matchFound && element.IsMatch(s))
                    {

                        returnData = new MyData(s.Replace("{", "").Replace("}", ""));
                        matchFound = !matchFound;
                        continue;
                    }

                    resultList.Add(s);
                }

                return string.Join("\n", resultList);
            }

            public static bool ExistsInStorage(string storage, string name)
            {
                System.Text.RegularExpressions.Regex element = new System.Text.RegularExpressions.Regex(@"\{" + name + @"\}");
                foreach (string s in storage.Split('\n'))
                {
                    if (element.IsMatch(s))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}

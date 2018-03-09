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
        public class MyLogger
        {
            Dictionary<string, StringBuilder> logs = new Dictionary<string, StringBuilder>();

            public bool AddLog(string logName)
            {
                if (!logs.ContainsKey(logName))
                {
                    logs.Add(logName, new StringBuilder());
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void WriteLog(string logName, string logData)
            {
                logs[logName].AppendLine(logData);
            }

            public string ReadLog(string logName)
            {
                return logs[logName].ToString();
            }

            public void ClearLog(string logName)
            {
                logs[logName].Clear();
            }

            public int LogSize(string logName)
            {
                return logs[logName].Length;
            }
        }
    }
}

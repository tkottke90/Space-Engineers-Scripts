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
        public class ScanGyro
        {
            Program _program;

            public enum SpinDirection {
                Pitch,
                Yaw,
                Roll,
            }
            public IMyGyro activeGyro { get; set; } 

            public ScanGyro(Program p) { _program = p; }

            public bool Add(IMyGyro gyro)
            {
                activeGyro = gyro;
                return activeGyro != null;
            }

            public bool setSpin(IMyGyro gyro, string direct, float speed)
            {
                try
                {
                    gyro.SetValue("Pitch", 0.0f);
                    gyro.SetValue("Yaw", 0.0f);
                    gyro.SetValue("Roll", 0.0f);

                    gyro.SetValue(direct, speed);
                    return true;
                }
                catch (Exception e)
                {
                    _program.debugSB.AppendLine("Set Spin Error:").AppendLine($"Exception: {e}\n---");
                    return false;
                }


                
            }
        }
    }

    

}

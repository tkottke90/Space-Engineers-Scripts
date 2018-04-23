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
        public class AIModule
        {
            private enum Goals {
                StayAlive,
                StayAwake,
                CollectResources
            };

            Program _program;
            bool MANUAL_FLIGHT = false;

            Dictionary<Goals, GoapGoal> MyGoals = new Dictionary<Goals, GoapGoal>() {
                { Goals.StayAwake, new GoapGoal(0.7f) },
                { Goals.CollectResources, new GoapGoal(0.5f) }
            };

            GoapPlanner MyPlanner = new GoapPlanner();
            Queue<GoapAction> plan = new Queue<GoapAction>();

            public AIModule(Program p, bool flightMode = false)
            {
                _program = p;
                MANUAL_FLIGHT = flightMode;

                MyGoals.Add(Goals.StayAlive, new StayAliveGoal(p));
            }




            // Uses GOAP Methodology
            /*
             *      World State Variables:
             *          - HasPower
             *          - NoDamage
             *          -  
             *          
             *      States:
             *          
             * 
             *      Goals:
             *          1) Stay Alive
             *              Relivent function:
             *                  If (Block Damage > 45%) 
             *                      Releventcy Value = Damage - 45% / 55%
             *          2) Stay Awake
             *              Relivent Function:
             *                  If (Power Available < 35% && Power Available > 17.5%)
             *                      Relivency Value = (Power Availalbe - 17.5% / 17.5%) * 50%
             *                  Else If (Power Available < 17.5%)
             *                      Relivency Value = ((Power Available - 17.5% / 17.5%) * 50%) - 50%
             *          3) Collect Resources
             *              Relevent Function:
             *                  If (Inventory Space Available > 50% && Known Asteriod Count >= 1)
             *                      Relevency Value = 
             *                          (Inventory Space Available Above 50% / 50%) * Inventory Space Weight (0.6 By Default) +
             *                          (Known Asteriod Count/Known Asteriod Count) * Known Asteriod Weight (0.4 by Default)              
             *          
             *      Actions:
             *          1) Goto
             *          2) Scan
             *          3) 
             *          
             *          
             *      Select Goal:
             *          StayAlive?: 
             */
        }

        public abstract class GoapGoal
        {
            public Program _program;
            public virtual float relivency_mod { get; set; } = 0.0f;

            public abstract bool CheckRelivency();
        }

        public class StayAliveGoal : GoapGoal {

            float blockDamage = 0.0f;

            public StayAliveGoal(Program p) {
                this.relivency_mod = 0.8f;
                _program = p;
            }

            public override bool CheckRelivency()
            {
                List<IMyTerminalBlock> tblocks = new List<IMyTerminalBlock>();
                _program.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(tblocks);

                float health = 0.0f;

                foreach(IMyTerminalBlock b in tblocks)
                {
                    IMySlimBlock slim = b.CubeGrid.GetCubeBlock(b.Position);
                    health += (slim.BuildIntegrity - slim.CurrentDamage) / slim.MaxIntegrity;
                }

                health = health / tblocks.Count;
                return false;
            }
        }



        public class GoapAction { }

        public class GoapPlanner { }
    }
}


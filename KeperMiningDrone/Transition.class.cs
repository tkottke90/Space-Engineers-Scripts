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

        public enum ProgramTransitions
        {
            boot,
            error,
            refresh,
            reboot,
            done,
            nav,
            scan,
            drill
        }


        public abstract class Transition {
            public Program _program;
            public virtual bool action() { return false; }
            public virtual bool action(string message) { return false; }
        }


        public class ErrorTransition : Transition {
            public ErrorTransition(Program p) { _program = p; }

            public override bool action(string message)
            {
                try
                {
                    _program.debugSB.AppendLine("Error: " + message);
                    _program._CurrentState = _program.states[ProgramStates.Error];
                    _program._CurrentState.Init();
                    return true;
                }
                catch (Exception e) { _program.debugSB.AppendLine("Exception in Error Transition: " + e.ToString()); return false; }
            }
        }

        public class BootTransition : Transition {
            public BootTransition(Program p) { _program = p;  }

            public override bool action()
            {
                try
                {
                    _program._CurrentState = _program.states[ProgramStates.Idle];
                    _program._CurrentState.Init();
                    return true;
                } catch (Exception e) { _program.debugSB.AppendLine("Exception in Boot Transition: " + e.ToString()); return false; }
            }
        }

        public class RebootTransition : Transition
        {
            public override bool action()
            {
                try
                {
                    _program._CurrentState = _program.states[ProgramStates.Startup];
                    _program._CurrentState.Init();
                    return true;
                }
                catch (Exception e) { _program.debugSB.AppendLine("Exception in Rebppt Transition: " + e.ToString()); _program.transitions[ProgramTransitions.error].action(); return false; }
            }
        }
    }
}

// --- N00bKeper's Dock Bay Display Manager Script ---
/*
 *   R e a d m e
 *   -----------
 * 
 *   Script designed to manage docking bays in a base using Sensors, LCD Panels, and Connectors.  Using the Cross and Arrow
 *   images, it provides a quick way to check if a ship is docked in a bay.
 *    
 *   To setup script:
 *   
 *      1) Build PB on base, station, or large ship with docking ports for smaller ships/rovers
 *      2) Build LCD Panels around bay so they are visible to engineers when they enter the docking space
 *      3) Setup one of the following sensor configurations:
 *          3a) Setup a sensor that encompasses the size of the bay
 *              ** Note that if the ship connects to a connector, it is concidered part of the grid it is attached to which means that a small
 *              ship attached to a large grid is no longer sensed by a sensor looking for small ships
 *          3b) Setup a connector in the bay that the ship can connect to
 *              ** Note that without a senor a ship could sit in a bay without being connected to the connector
 *          3c) Setup 1 sensor and 1 connector in the bay.  Configure the the sensor as in 3a.
 *      4) Assign Sensor, Connector, and any LCDs related to this bay to a unique group
 *      5) Add that group name to the list in the configuration section
 *          
 */

// =======================================================================================
//                                                                            --- Configuration ---
// =======================================================================================

// --- Block Groups ---
// =======================================================================================
// Enter names of block groups to be handed by script
// Example: string[] groups = { "DockingBay1", "TruckBayA" };
string[] groups = { "Truck Bay 1 Status", "Truck Bay 2 Status" };

// =======================================================================================

// DO NOT EDIT BELOW THIS LINE!!
// =======================================================================================
//                                                                            --- Script ---
// =======================================================================================

List<DockBayGroup> myGroups = new List<DockBayGroup>();


StringBuilder errors = new StringBuilder();
StringBuilder notify = new StringBuilder();

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    getBlockGroups();

    Me.CustomName = "Programmable Block [DBM]";
}

[Flags]
public enum Test {
    off = 0,
    on = 1,
    ready = 2
}

public void Main(string argument, UpdateType updateSource)
{

    foreach (DockBayGroup g in myGroups)
    {
        g.update();
    }

    Echo(DrawApp());
}

void getBlockGroups()
{
    foreach (string group in groups)
    {
        IMySensorBlock s = null;
        IMyShipConnector c = null;
        List<IMyTextPanel> t = new List<IMyTextPanel>();

        IMyBlockGroup bGroup = GridTerminalSystem.GetBlockGroupWithName(group);
        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
        bGroup.GetBlocks(blocks);

        foreach (IMyTerminalBlock b in blocks)
        {
            string[] blockName = b.GetType().ToString().Split('.');
            switch (blockName[blockName.Length - 1])
            {
                case "MyTextPanel":
                    t.Add((IMyTextPanel)b);
                    break;
                case "MySensorBlock":
                    s = (IMySensorBlock)b;
                    break;
                case "MyShipConnector":
                    c = (IMyShipConnector)b;
                    break;
            }
        }

        if (s != null && c != null)
        {
            myGroups.Add(new DockBayGroup(s, c, t));
        } else if (s != null)
        {
            myGroups.Add(new DockBayGroup(s, t));
        } else if (c != null)
        {
            myGroups.Add(new DockBayGroup(c, t));
        } else
        {
            errors.AppendLine("No Connector/Sensor in Block Group: " + group);
        }

    }
}


public string DrawApp()
{
    StringBuilder sb = new StringBuilder();

    sb.AppendLine("--- Docking Bay Manager ---").AppendLine();

    if (errors.Length > 0)
    {
        sb.AppendLine(errors.ToString()).AppendLine();
    }

    sb.AppendLine("Bays Managed: " + myGroups.Count);


    return sb.ToString();
}





public class DockBayGroup
{
    IMySensorBlock sensor;
    IMyShipConnector connector;
    List<IMyTextPanel> LCDPanels;

    [Flags]
    public enum States {
        none = 0,
        sensor = 1,
        connector = 2,
    }

    bool prevState = false;
    public States state = States.none;

    public DockBayGroup(IMySensorBlock s, List<IMyTextPanel> p) {
        sensor = s;
        LCDPanels = p;
        Setup();
    }

    public DockBayGroup(IMyShipConnector c, List<IMyTextPanel> p) {
        connector = c;
        LCDPanels = p;
        Setup();
    }

    public DockBayGroup(IMySensorBlock s, IMyShipConnector c, List<IMyTextPanel> p) {
        sensor = s;
        connector = c;
        LCDPanels = p;
        Setup();
    }

    private void Setup() {
        foreach (IMyTextPanel t in LCDPanels)
        {
            t.ClearImagesFromSelection();
            t.SetShowOnScreen(0);
        }
    }

    public void update() {

        if ( sensor != null && sensor.IsActive)
        {
            state |= States.sensor;
        } else
        {
            state &= ~States.sensor;
        }

        if ( connector != null && connector.Status == MyShipConnectorStatus.Connected)
        {
            state |= States.connector;
        } else
        {
            state &= ~States.connector;
        }

        bool currentState = (state & States.sensor) != 0 || (state & States.connector) != 0;

        if (prevState != currentState)
        {
            DrawLCD(currentState);

        }

        prevState = currentState;

    }

    public void DrawLCD(bool occupied) {
        if (occupied)
        {
            foreach(IMyTextPanel t in LCDPanels)
            {
                t.AddImageToSelection("Cross");
                t.RemoveImageFromSelection("Arrow");

            }
        } else
        {
            foreach (IMyTextPanel t in LCDPanels)
            {
                t.AddImageToSelection("Arrow");
                t.RemoveImageFromSelection("Cross");
            }
        }
    }
}

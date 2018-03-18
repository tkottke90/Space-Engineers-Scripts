/*
 *   R e a d m e
 *   -----------
 * 
 *   Script designed to manage docking bays in a base using Sensors, LCD Panels, and Connectors.  Using the Cross and Arrow
 *   images, it provides a quick way to check if a ship is docked in a bay.
 *    
 *   To setup script:
 *   
 *      1) Build PB on base, station, or large ship with docking ports for smaller scripts
 *      2) Build LCD Panels around bay so they are visible to engineers when they enter the docking space
 *      3) Setup one of the following sensor configurations:
 *          3a) Setup a sensor that encompasses the size of the bay
 *              ** Note that if the ship connects to a connector, it is concidered part of the grid it is attached to which means that a small
 *              ship attached to a large grid is no longer sensed by a sensor looking for small ships
 *          3b) Setup a connector in the bay that the ship can connect to
 *              ** Note that without a senor a ship could sit in a bay without being connected to the connector
 *          3c) Setup 1 sensor and 1 connector in the bay.  Configure the the sensor as in 3a.
 *      4) Assign Sensor, Connector, and any LCDs related to this bay to a group
 *      5) Add that group name to the list in the configuration section
 *          
 */
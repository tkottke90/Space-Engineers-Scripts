# Dock Status Script

Script designed to manage docking bays and connectors with a LCD Display that will show a green arrow when the space is available and a red cross when the space is occupied.



### Setup Steps:
  1) Build PB on base, station, or large ship with docking ports for smaller ships/rovers
  2) Build LCD Panels around bay so they are visible to engineers when they enter the docking space
  3) Setup one of the following sensor configurations:
  - Setup a sensor that encompasses the size of the bay.  Note that if the ship connects to a connector, it is concidered part of the grid it is attached to which means that a small ship attached to a large grid is no longer sensed by a sensor looking for small ships
  - Setup a connector in the bay that the ship can connect to. Note that without a senor a ship could sit in a bay without being connected to the connector
  - Setup 1 sensor and 1 connector in the bay.  Configure the the sensor as in 3a.
  4) Assign Sensor, Connector, and any LCDs related to this bay to a unique group
  5) Add that group name to the list in the configuration section
  
### Images

Open Dock:
![Open Dock](https://github.com/tkottke90/Space-Engineers-Scripts/blob/master/projects/DockStatusScript/2018-03-18-13-04-29-583.png)

Occupied Dock:
![Occupied Dock](https://github.com/tkottke90/Space-Engineers-Scripts/blob/master/projects/DockStatusScript/2018-03-18-13-04-00-190.png)

Connected to Dock:
![Connected to Dock](https://github.com/tkottke90/Space-Engineers-Scripts/blob/master/projects/DockStatusScript/2018-03-18-13-05-13-196.png)

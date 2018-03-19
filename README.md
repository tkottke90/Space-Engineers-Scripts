# Space Engineers Scripts
 
 Space Engineers is a game developed by the Keen Software House.  This repo contains a collection of scripts and utility classes designed to be used with the ingame programming system.
 
You can find what I am currently working in the [mdk branch](https://github.com/tkottke90/Space-Engineers-Scripts/tree/mdk)
 
 ## Disclamer
 I am a self taught engineer and as I develop my skills I am still learning all the syntatic and semantic standards for languages.  You may see inconsistancies between my code and standards (such as Types should use pascel case not camel case).  I hope to increase my proficiency I am hoping to continue to produce better and cleaner code.
 
 ## Completed Scripts
 - [Dock Status Manager](https://github.com/tkottke90/Space-Engineers-Scripts/tree/master/projects/DockStatusScript)
    - Script designed to display occupancy status of docking bay
 
 ## Current Utility Classes
 - [GPSLocation](https://github.com/tkottke90/Space-Engineers-Scripts/blob/master/lib/GPSLocation.class.cs)
    - This class is designed to assist the programable block with managing GPS locations by providing a concrete yet flexable object, as well as a way to save and recover the object as a string.
  - [LCDGroup](https://github.com/tkottke90/Space-Engineers-Scripts/blob/master/lib/LCDGroup.class.cs)
    - This class is designed to provide an overlay for a group of LCD Panels.  This allows for consistancy across multiple panels that fall into this same LCD Group.
  - [CameraGroup](https://github.com/tkottke90/Space-Engineers-Scripts/blob/master/lib/CameraGroup.class.cs)
    - This class is designed to handle a group of cameras in conjunction with the Raycast feature.  This class **requires** the GPSLocation class as it uses that class to record found entities in the game.

## Currently Working On:
  - Mining Drone AI
  - Interior Light Color Manager

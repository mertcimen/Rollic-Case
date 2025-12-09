# Rollic-Case
 Level Storage

All levels in this project are stored as ScriptableObject-based LevelData assets.
Each LevelData contains:
Grid configuration
Placed blocks
Placed shredders
Level time
Cell states
This makes every level fully serializable and easily editable.


 Editing a Level

To edit any existing LevelData:

1. Open the Level Editor Scene

Navigate to:

Main/ Scenes / LevelEditorScene  and open it.

Level Library (Levels Container)
All created LevelData assets must be assigned into the project’s:
Levels (ScriptableObject)


This container acts as the level database and is used by the game to load levels sequentially or by index.

If a LevelData asset is not added inside Levels, 
the game will not be able to access it.


2. Select the Level Data Editor object

In the Hierarchy, click on:
LevelDataEditor. 
This object contains the custom editor tool used to modify LevelData assets.

3. Assign or Create a LevelData asset

In the inspector:

Assign an existing LevelData asset, or

Click “Create New LevelData” to generate a new one.

4. Edit the Level Using the Grid Editor

The custom inspector gives you:

Grid editing mode (enable/disable cells)

Block placement mode

Shredder placement mode

Rotation, color, and move direction settings

Ability to inspect & delete placed items

All changes are saved directly to the LevelData asset.

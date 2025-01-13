# Custom Item Mod

![screenshot](screenshot.png)

This is a mod for Derail Valley that let's mod developers add custom items to the game. The items will be purchaseable in the shops of the valley.   
This mod doesn't do anything by itself! You have to create a custom item [like this one](https://git.tostiman.com/tostiman/dv_blahaj).

## How to install

1. install Unity Mod Manager(UMM)
2. download this mods zip from the 'Releases' tab and throw it into UMM

## How to create your own custom item

Follow the instructions [here](https://git.tostiman.com/tostiman/dv_blahaj).

## Item guide

This section is all about best practices for making an item.  The tips in this section will help you get to a working item in the minimum time.

### Item Prefab

When building your prefab, it's important to establish the prefab's origin at the bottom front of your model.  This is usually done by offsetting the model as a child of the prefab, though can be done by ensuring that the exported model has it's origin at the bottom front.
The prefab should be set up so that it's front is along the **-Z** axis and the top along the **+Y** axis.  The prefab should include a box collider or mesh collider to enable interaction.  Box collider is preferred unless it's excessively sized relative to the actual mesh.

### Shelf Prefab

The shelf prefab is an optional prefab.  If provided, this is what will be placed on the shelf.  It can include multiple copies of your item, or a creative arrangement.  It should be oriented with the front along the **+Z** axis and the top along the **+Y** axis. It should have a single box collider that wraps the entire visible portion of the prefab.

### Icons

Icons are simply png files (keep them reasonably sized, they only show up around 300x300 pixels, maybe a bit bigger on 4k screens...).  In Unity they must be marked as sprites (after importing the png, select it and in the properties window on the right change from texture to sprite).

### Shelf Size

The shelf size is determined in one of two ways in order of priority:

1. ShelfBounds json value - this is an XYZ Vector3.  Specifically X determines the width of the shelf space needed, and Y determines the height needed.  Z is not currently used.
1. ShelfPrefab box collider size (only used if a shelf prefab is provided)

If the size cannot be determined in one of those ways, a default shelf space of .35 x .3 meters will be assumed.

### JSON Fields

**Required Fields:**

- Name - string : This is the name that will be displayed in game
- Description - string: This will be displayed when hovering over the item to purchase it.
- Amount - integer: This is the number of items available to purchase
- Price - integer: This is how much the item will cost in career mode (affected by career settings) 
- AssetBundleName - string: This is the filename of the built asset bundle
- PrefabPath - string: This is the path within the asset bundle to get to your item prefab (this should be listed in your asset bundle manifest and end in .prefab)
- IconStandardPath - string: This is the path within the asset bundle to the icon asset for inventory view.
- IconDroppedPath - string: This is the path within the asset bundle to the icon asset for locked slots in inventory where the item is missing. 

**Optional Fields:**
- ShelfPrefabPath - string: This is the path within the asset bundle to get to your shelf prefab if provided.  If missing, the item prefab will be used rotated 180 degrees.
- ShelfBounds - Vector3: This is the width, height, and depth (x, y, z) of the item on the shelf
- ShelfScale - Vector3: A scaling factor to apply to the item placed on the shelf (either the shelf prefab or the item prefab) to help it fit.  Scaling is non-uniform (unless x,y,and z match), and a full vector3 is required.
- ShelfRotation - Vector3: Rotation in Euler angles (Roll Pitch Yaw) in degrees to apply to the shelf item.  If provided, this will override the automatic rotation applied to item prefab if no shelf prefab is provided.
- PreviewRotation - Vector3: This rotation is applied to the item when placing it in the world using the "place" command (R by default on keyboard)

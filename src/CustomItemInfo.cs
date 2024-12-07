using System;
using UnityEngine;

namespace custom_item_mod;

[Serializable]
public class CustomItemInfo
{
	// Required info fields
	public string Name;
	public string Description;
	public int Amount;
	public int Price;
	
	// Required asset references
	public string AssetBundleName;
	public string PrefabPath;
	public string IconStandardPath;
	public string IconDroppedPath;

	// Optional Asset References
    public string ShelfPrefabPath;

	//Optional info fields
    public Vector3 ShelfBounds;
	public Vector3 ShelfScale;
	public Vector3 ShelfRotation;
	public Vector3 PreviewRotation;
}
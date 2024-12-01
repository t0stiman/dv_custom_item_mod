﻿using System;
using UnityEngine;

namespace custom_item_mod;

[Serializable]
public class CustomItemInfo
{
	public string Name;
	public string Description;
	public int Amount;
	public int Price;
	
	public string AssetBundleName;
	public string PrefabPath;
	public string ShelfPrefabPath;
	public string IconStandardPath;
	public string IconDroppedPath;
	public Vector3 ShelfBounds;
	public Vector3 ShelfScale;
	public Vector3 ShelfRotation;
}
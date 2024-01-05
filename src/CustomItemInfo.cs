using System;

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
	public string IconStandardPath;
	public string IconDroppedPath;
}
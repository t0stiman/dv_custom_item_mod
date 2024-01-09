using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityModManagerNet;

namespace custom_item_mod;

//the code in this file is based on code from Insprill's Mapify mod
public static class ItemModsFinder
{
	public const string ITEM_INFO_FILE = "item_info.json";

	public static List<CustomItem> CustomItems = new();
	
	//store the file paths of bundles so we can check if they are already loaded
	private static List<(string, AssetBundle)> loadedAssetBundles = new();

	/// <summary>
	///     name -> (info, mod, directory)
	/// </summary>
	private static readonly Dictionary<string, (CustomItemInfo, UnityModManager.ModEntry, string)> itemsIDK = new();
	
	public static void Setup()
	{
		UnityModManager.toggleModsListen += ToggleModsListen;
		foreach (var entry in UnityModManager.modEntries)
		{
			FindCustomItemsInMod(entry);
		}
	}
	
	private static void ToggleModsListen(UnityModManager.ModEntry aModEntry, bool modEnabled)
	{
		if (modEnabled)
		{
			Main.Log($"New mod enabled ({aModEntry.Info.DisplayName}), checking for custom items...");
			FindCustomItemsInMod(aModEntry);
		}
	} 
	
	private static void FindCustomItemsInMod(UnityModManager.ModEntry aModEntry)
	{
		// search the subdirectories of the mod directory
		foreach (string modSubDirectory in Directory.GetDirectories(aModEntry.Path))
		{
			string itemInfoPath = GetModAsset(ITEM_INFO_FILE, modSubDirectory);
			if (!File.Exists(itemInfoPath)) {
				continue;
			}

			var itemInfo = JsonUtility.FromJson<CustomItemInfo>(File.ReadAllText(itemInfoPath));
			
			//an item with this name is already loaded?
			if (itemsIDK.TryGetValue(itemInfo.Name, out (CustomItemInfo, UnityModManager.ModEntry, string) existingItem))
			{
				if (existingItem.Item2 != aModEntry)
				{
					Main.Error(
						$"Skipping item from '{aModEntry.Info.DisplayName}' in '{modSubDirectory}' due to duplicate name '{itemInfo.Name}' (Already added by '{existingItem.Item2.Info.DisplayName}')");
				}

				continue;
			}

			Main.Log($"Found custom item '{itemInfo.Name}' from '{aModEntry.Info.DisplayName}' in '{modSubDirectory}'");

			try
			{
				SetupItem(itemInfo, modSubDirectory);
			}
			catch (Exception theException)
			{
				Main.Error($"Exception while loading item: {theException}" );
				continue;
			}
			
			itemsIDK.Add(itemInfo.Name, (itemInfo, aModEntry, modSubDirectory));
		}
	}
	
	public static string GetModAsset(string fileName, string modDir)
	{
		return Path.Combine(modDir, fileName);
	}
	
	private static void SetupItem(CustomItemInfo itemInfo, string itemDirectory)
	{
		var assBundle = LoadAssetbundle(itemInfo, itemDirectory);
		if(assBundle == null){ return; }

		bool success = true;
		var prefab = assBundle.LoadAsset<GameObject>(itemInfo.PrefabPath);
		if (prefab == null)
		{
			Main.Error($"Failed to load {nameof(prefab)} from item {itemInfo.Name} at {itemInfo.PrefabPath}");
			success = false;
		}
		
		var iconStandard = assBundle.LoadAsset<Sprite>(itemInfo.IconStandardPath);
		if (iconStandard == null)
		{
			Main.Error($"Failed to load {nameof(iconStandard)} from item {itemInfo.Name} at {itemInfo.IconStandardPath}");
			success = false;
		}
		
		var iconDropped = assBundle.LoadAsset<Sprite>(itemInfo.IconDroppedPath);
		if (iconDropped == null)
		{
			Main.Error($"Failed to load {nameof(iconDropped)} from item {itemInfo.Name} at {itemInfo.IconDroppedPath}");
			success = false;
		}

		if (success)
		{
			CustomItems.Add(new CustomItem(
				itemInfo.Name,
				itemInfo.Description,
				prefab,
				itemInfo.Amount,
				itemInfo.Price,
				iconStandard,
				iconDropped
				//todo implement more parameters
			));
		}
	}

	private static AssetBundle LoadAssetbundle(CustomItemInfo itemInfo, string itemDirectory)
	{
		var bundlePath = Path.GetFullPath(Path.Combine(itemDirectory, itemInfo.AssetBundleName));

		//is already loaded?
		foreach (var idk in loadedAssetBundles)
		{
			if (idk.Item1 == bundlePath && idk.Item2 != null)
			{
				return idk.Item2;
			}
		}

		//load it
		var assBundle = AssetBundle.LoadFromFile(bundlePath);
		if (assBundle == null)
		{
			Main.Error($"Failed to load the assetbundle from item '{itemInfo.Name}' at '{bundlePath}'");
			return null;
		}
		
		loadedAssetBundles.Add((bundlePath, assBundle));
		return assBundle;
	}

	public static void Unload()
	{
		foreach (var assBundle in  AssetBundle.GetAllLoadedAssetBundles())
		{
			assBundle.Unload(true);
		}
	}
}
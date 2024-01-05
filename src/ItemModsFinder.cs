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
	private static List<AssetBundle> loadedAssetBundles = new();

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
			return;
		}

		// UnloadMod(aModEntry);
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
	
	private static string GetModAsset(string fileName, string modDir)
	{
		return Path.Combine(modDir, fileName);
	}
	
	private static void SetupItem(CustomItemInfo itemInfo, string itemDirectory)
	{
		var assBundle = AssetBundle.LoadFromFile(Path.Combine(itemDirectory, itemInfo.AssetBundleName));
		
		if (assBundle == null)
		{
			Main.Error($"Failed to load the assetbundle from item {itemInfo.Name}");
			return;
		}

		loadedAssetBundles.Add(assBundle);

		var prefab = assBundle.LoadAsset<GameObject>(itemInfo.PrefabPath);
		var iconStandard = assBundle.LoadAsset<Sprite>(itemInfo.IconStandardPath);
		var iconDropped = assBundle.LoadAsset<Sprite>(itemInfo.IconDroppedPath);

		if (prefab == null || iconStandard == null || iconDropped == null)
		{
			Main.Error($"Failed to load one or more assets from item {itemInfo.Name}");
			return;
		}

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

	public static void Unload()
	{
		foreach (var assBundle in loadedAssetBundles)
		{
			assBundle.Unload(true);
		}
	}
}
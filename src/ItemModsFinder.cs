﻿using DV.CashRegister;
using DV.Shops;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

namespace custom_item_mod;

//the code in this file is based on code from Insprill's Mapify mod
public static class ItemModsFinder
{
	public const string ITEM_INFO_FILE = "item_info.json";

	public static readonly List<CustomItem> CustomItems = new();
	
	//store the file paths of bundles so we can check if they are already loaded
	private static readonly List<(string, AssetBundle)> loadedAssetBundles = new();

	/// <summary>
	///     name -> (info, mod, directory)
	/// </summary>
	private static readonly Dictionary<string, (CustomItemInfo, UnityModManager.ModEntry, string)> itemsIDK = new();
	
	public static void InitializeItems()
	{
		Main.Log("Initializing items");
		foreach (var me in UnityModManager.modEntries)
		{
			if (me.Enabled)
			{
				SetupCustomItems(me);
			}
		}
		Main.Log("Done initializing items");
	}
	
	private static void SetupCustomItems(UnityModManager.ModEntry aModEntry)
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

			Main.Log("sanity check");
			Main.Log($"Found custom item '{itemInfo.Name}' from '{aModEntry.Info.DisplayName}' in '{modSubDirectory}'");

			try
			{
				var item = SetupItem(itemInfo, modSubDirectory);
				if (item != null)
				{
					CustomItems.Add(item);
				}
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
	
	private static CustomItem SetupItem(CustomItemInfo itemInfo, string itemDirectory)
	{
		var assBundle = LoadAssetbundle(itemInfo, itemDirectory);
		if(assBundle == null){ return null; }

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
			var item = new CustomItem(
				itemInfo,
				prefab,
				iconStandard,
				iconDropped
				//todo implement more parameters?
			);
			return item;
		}
		return null;
	}

	private static AssetBundle LoadAssetbundle(CustomItemInfo itemInfo, string itemDirectory)
	{
		var bundlePath = Path.GetFullPath(Path.Combine(itemDirectory, itemInfo.AssetBundleName));

		//is already loaded?
		foreach (var loadedAssBundle in loadedAssetBundles)
		{
			if (loadedAssBundle.Item1 == bundlePath && loadedAssBundle.Item2 != null)
			{
				return loadedAssBundle.Item2;
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

    internal static void FinalizeItems(GlobalShopController instance)
    {
		foreach (var item in CustomItems)
		{
			item.FinishInitialization(instance);
		}
		instance.initialAmounts.Clear();
		instance.InitializeShopData();
		foreach(var item in CustomItems)
		{
			foreach(var shop in instance.globalShopList)
			item.AddToShop(shop);
		}
    }
}
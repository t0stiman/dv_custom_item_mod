using System;
using System.Linq;
using DV.Shops;
using HarmonyLib;
using I2.Loc;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace custom_item_mod.Patches;

[HarmonyPatch(typeof(Shop))]
[HarmonyPatch(nameof(Shop.Awake))]
public class Shop_Awake_Patch
{
	private static void Postfix(ref Shop __instance)
	{
		if (__instance.scanItemResourceModules == null || __instance.scanItemResourceModules.Length == 0)
		{
			Main.Error($"{nameof(Shop_Awake_Patch)}: {nameof(__instance.scanItemResourceModules)} not init yet");
			return;
		}

		foreach (var item in ItemModsFinder.CustomItems)
		{
			AddItemToShop(item, ref __instance);
		}
	}

	private static void AddItemToShop(CustomItem anItem, ref Shop aShop)
	{
		var existingScanModuleObject = aShop.scanItemResourceModules[0].gameObject;
		var newScanModuleObject =
			Object.Instantiate(existingScanModuleObject, existingScanModuleObject.transform.parent);

		newScanModuleObject.name = $"{existingScanModuleObject.name}({anItem.ItemSpec.LocalizedName})";

		var module = newScanModuleObject.GetComponent<ScanItemCashRegisterModule>();
		module.sellingItemSpec = anItem.ItemSpec;
		// Main.Log($"item spec set on {newScanModuleObject.name}");

		//name
		{
			var nameObj = newScanModuleObject.transform.Find("Texts/Name").gameObject;
			var tmp = nameObj.GetComponent<TextMeshPro>();
			tmp.text = anItem.Name;
		}

		//description
		{
			var descriptionObj = newScanModuleObject.transform.Find("Texts/Description").gameObject;
			var tmp = descriptionObj.GetComponent<TextMeshPro>();
			tmp.text = anItem.Description;
		}

		//price
		{
			var priceObj = newScanModuleObject.transform.Find("Texts/Price").gameObject;
			var tmp = priceObj.GetComponent<TextMeshPro>();
			tmp.text = anItem.PriceText;
		}
		
		aShop.scanItemResourceModules = aShop.scanItemResourceModules.Append(module).ToArray();
		aShop.cashRegister.registerModules = aShop.cashRegister.registerModules.Append(module).ToArray();
	}
}
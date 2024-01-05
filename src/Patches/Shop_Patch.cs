using DV.Shops;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace custom_item_mod.Patches;

[HarmonyPatch(typeof(Shop))]
[HarmonyPatch(nameof(Shop.Awake))]
public class Shop_Awake_Patch
{
	private static void Postfix(ref Shop __instance)
	{
		var scanModules = __instance.scanItemResourceModules;
		if (scanModules == null || scanModules.Length == 0)
		{
			Main.Error($"Shop_Awake_Patch: scanItemResourceModules not init yet");
			return;
		}
		
		var existingScanModuleObject = scanModules[0].gameObject;
		for (var i = 0; i < ItemModsFinder.CustomItems.Count; i++)
		{
			AddItemToShop(ItemModsFinder.CustomItems[i], ref __instance, existingScanModuleObject, i);
		}
	}

	private static void AddItemToShop(CustomItem anItem, ref Shop aShop, GameObject existingScanModuleObject, int itemNumber)
	{
		var newScanModuleObject =
			Object.Instantiate(existingScanModuleObject, existingScanModuleObject.transform.parent);

		//todo
		newScanModuleObject.transform.localPosition -= new Vector3((itemNumber+1)*0.5f, 0, 0);

		newScanModuleObject.name = $"{existingScanModuleObject.name}({anItem.ItemSpec.LocalizedName})";

		var module = newScanModuleObject.GetComponent<ScanItemCashRegisterModule>();
		module.sellingItemSpec = anItem.ItemSpec;
		Main.Log($"item spec set on {newScanModuleObject.name}");

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

		//todo waarom werkt dit niet
		// scanModules.AddItem(module);
		// __instance.cashRegister.registerModules.AddItem(module);
		
		aShop.scanItemResourceModules = new[] { module };
		aShop.cashRegister.registerModules = new[] { module };
	}
}
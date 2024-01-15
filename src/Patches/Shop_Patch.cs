using System;
using System.Collections.Generic;
using System.Linq;
using DV.Shops;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace custom_item_mod.Patches;

[HarmonyPatch(typeof(Shop))]
[HarmonyPatch(nameof(Shop.Awake))]
public class Shop_Awake_Patch
{
	// i hope you like spaghetti (code)
	private static void Postfix(ref Shop __instance)
	{
		var scanModules = __instance.scanItemResourceModules;
		if (scanModules == null || scanModules.Length == 0)
		{
			Main.Error($"{nameof(Shop_Awake_Patch)}: {nameof(__instance.scanItemResourceModules)} not init yet");
			return;
		}
		
		var scanModuleTransforms = __instance.scanItemResourceModules.Select(something => something.transform).ToArray();

		float minimumX = scanModuleTransforms.Min(something => something.localPosition.x);
		float maximumY = scanModuleTransforms.Max(something => something.localPosition.y);
		
		var ordenedModuleTransforms = new List<List<Transform>>();

		foreach (var scanModTrans in scanModuleTransforms)
		{
			bool placedInRow = false;
			
			foreach (var row in ordenedModuleTransforms)
			{
				if (NearlyEqual(scanModTrans.localPosition.y, row[0].localPosition.y, 0.1f))
				{
					row.Add(scanModTrans);
					placedInRow = true;
					break;
				}
			}

			//doesn't match any existing row, add a new one
			if (!placedInRow)
			{
				ordenedModuleTransforms.Add(new List<Transform> { scanModTrans });
			}
		}

		var offset = new Vector2();
		try
		{
			offset.y = Math.Abs(ordenedModuleTransforms[0][0].localPosition.y -
			                    ordenedModuleTransforms[1][0].localPosition.y);
			offset.x = Math.Abs(ordenedModuleTransforms[0][0].localPosition.x -
			                    ordenedModuleTransforms[0][1].localPosition.x);
		}
		catch (IndexOutOfRangeException e)
		{
			e.ToString(); //get rid of the 'unused variable' warning
			
			Main.Warning($"Shop {__instance.gameObject.name} has less then 2 rows/cols? Assuming offsets");
			offset.y = 0.288f;
			offset.x = 0.6f;
		}

		foreach (var item in ItemModsFinder.CustomItems)
		{
			AddItemToShop(item, ref __instance);
		}

		{
			var extraItemsPerRow = 2;  //fit more items in a row
			int itemsPerRow = ordenedModuleTransforms.Max(x => x.Count) + extraItemsPerRow;
			var startPos = new Vector3(minimumX - extraItemsPerRow*offset.x/2.0f, maximumY, scanModuleTransforms[0].localPosition.z);

			//not sure if its necessary to do this again to include the custom items, but lets be safe
			scanModuleTransforms = __instance.scanItemResourceModules.Select(something => something.transform).ToArray();
			
			int row = 0;
			int column = 0;
			
			foreach (var t in scanModuleTransforms)
			{
				
				t.localPosition = new Vector3(startPos.x + column * offset.x, startPos.y - row * offset.y,
					startPos.z);
				
				column++;
				if (column >= itemsPerRow)
				{
					column = 0;
					row++;
				}
			}

			// recenter the signs
			var oldRowCount = ordenedModuleTransforms.Count;
			var rowCountDelta = row+1 - oldRowCount;
			var yMovePls = rowCountDelta * offset.y / 2.0f;
			
			foreach (var idk in __instance.scanItemResourceModules)
			{
				idk.gameObject.transform.localPosition += new Vector3(0, yMovePls, 0);
			}
		}
	}

	private static bool NearlyEqual(float a, float b, float margin)
	{
		return a==b || ( a < b + margin && a > b - margin);
	}

	private static void AddItemToShop(CustomItem anItem, ref Shop aShop)
	{
		var existingScanModuleObject = aShop.scanItemResourceModules[0].gameObject;
		var newScanModuleObject =
			Object.Instantiate(existingScanModuleObject, existingScanModuleObject.transform.parent);

		newScanModuleObject.name = $"{existingScanModuleObject.name}({anItem.ItemSpec.LocalizedName})";

		var module = newScanModuleObject.GetComponent<ScanItemCashRegisterModule>();
		module.sellingItemSpec = anItem.ItemSpec;

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
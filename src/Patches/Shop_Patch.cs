using System;
using System.Collections.Generic;
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
		var scanModules = __instance.scanItemResourceModules;
		if (scanModules == null || scanModules.Length == 0)
		{
			Main.Error($"{nameof(Shop_Awake_Patch)}: {nameof(__instance.scanItemResourceModules)} not init yet");
			return;
		}

		var scanModuleTransforms = scanModules.Select(something => something.transform).ToArray();
		
		// ==========================================

		float minimumX = scanModuleTransforms.Min(something => something.localPosition.x);
		float maximumY = scanModuleTransforms.Max(something => something.localPosition.y);
		
		// ==========================================
		
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

		float verticalOffset;
		float horizontalOffset;
		if (ordenedModuleTransforms.Count < 2)
		{
			Main.Warning($"Shop {__instance.gameObject.name} has less then 2 rows? Assuming offsets");
			verticalOffset = 0.288f;
			horizontalOffset = 0.6f;
		}
		else
		{
			verticalOffset = Math.Abs(ordenedModuleTransforms[0][0].localPosition.y -
			                              ordenedModuleTransforms[1][0].localPosition.y);
			horizontalOffset = Math.Abs(ordenedModuleTransforms[0][0].localPosition.x -
			                                ordenedModuleTransforms[0][1].localPosition.x);
		}

		// ==========================================
		
		{
			int itemsPerRow = ordenedModuleTransforms.Max(x => x.Count);
			var startPos = new Vector2(minimumX, maximumY);
			
			int row = 1;
			int column = 0;

			for (var i = 0; i < ItemModsFinder.CustomItems.Count; i++)
			{
				var localPosition = new Vector3(startPos.x + column * horizontalOffset, startPos.y + row * verticalOffset,
					scanModuleTransforms[0].localPosition.z);
				AddItemToShop(ItemModsFinder.CustomItems[i], ref __instance, scanModules[0].gameObject, localPosition);

				column++;
				if (column >= itemsPerRow)
				{
					column = 0;
					row++;
				}
			}
		}
		
		
	}

	private static bool NearlyEqual(float a, float b, float margin)
	{
		return a==b || ( a < b + margin && a > b - margin);
	}

	private static void AddItemToShop(CustomItem anItem, ref Shop aShop, GameObject existingScanModuleObject, Vector3 localPosition)
	{
		var newScanModuleObject =
			Object.Instantiate(existingScanModuleObject, existingScanModuleObject.transform.parent);

		//todo better sign positioning system
		newScanModuleObject.transform.localPosition = localPosition;

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
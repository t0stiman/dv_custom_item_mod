using System;
using System.Collections.Generic;
using System.Linq;
using DV.CabControls;
using DV.CabControls.NonVR;
using DV.CabControls.Spec;
using DV.Interaction;
using HarmonyLib;
using UnityEngine;

namespace custom_item_mod.Patches;

[HarmonyPatch(typeof(ItemBase))]
[HarmonyPatch(nameof(ItemBase.Awake))]
public class ItemBase_Awake_Patch
{
	private static void Prefix(ref ItemBase __instance)
	{
		var inventoryItemSpec = __instance.GetComponent<InventoryItemSpec>();
		if (inventoryItemSpec is null)
		{
			Main.Error($"{nameof(ItemBase_Awake_Patch)}: inventoryItemSpec is null");
			return;
		}
		
		//custom items only
		if (!inventoryItemSpec.localizationKeyName.StartsWith(Main.MyModEntry.Info.Id))
		{
			return;
		}
		
		Main.Log(nameof(ItemBase_Awake_Patch));
		
		var itemComponent = __instance.GetComponent<Item>();
		itemComponent.colliderGameObjects = __instance.gameObject.GetComponentsInChildren<Collider>()
			.Select(collider => collider.gameObject).ToArray();
	}
}
using System.Linq;
using DV.CabControls;
using DV.CabControls.Spec;
using HarmonyLib;
using UnityEngine;

namespace custom_item_mod.Patches;

[HarmonyPatch(typeof(ItemBase))]
[HarmonyPatch(nameof(ItemBase.Awake))]
public class ItemBase_Patch
{
	private static void Prefix(ref ItemBase __instance)
	{
		var inventoryItemSpec = __instance.GetComponent<InventoryItemSpec>();
		if (inventoryItemSpec is null)
		{
			Main.Error($"{nameof(ItemBase_Patch)}: inventoryItemSpec is null");
			return;
		}
		
		//custom items only
		if (!inventoryItemSpec.localizationKeyName.StartsWith(Main.MyModEntry.Info.Id))
		{
			return;
		}
		
		var itemComponent = __instance.GetComponent<Item>();
		if (itemComponent.colliderGameObjects.Length == 0)
		{
			itemComponent.colliderGameObjects = __instance.gameObject.GetComponentsInChildren<Collider>()
				.Select(collider => collider.gameObject).ToArray();
		}
	}
}
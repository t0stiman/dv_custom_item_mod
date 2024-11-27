using DV.Shops;
using HarmonyLib;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace custom_item_mod.Patches;

[HarmonyPatch(typeof(GlobalShopController))]
[HarmonyPatch(nameof(GlobalShopController.Awake))]
public class GlobalShopController_Awake_Patch
{
	private static void Prefix(ref GlobalShopController __instance)
	{
		foreach (var item in ItemModsFinder.CustomItems)
		{
			__instance.shopItemsData.Add(item.ShopData);
		}
	}
}

[HarmonyPatch(typeof(ShelfPlacer))]
[HarmonyPatch(nameof(ShelfPlacer.TryPlaceOnAnyShelf))]
public class ShelfPlacer_TryPlaceOnAnyShelf_Patch
{
	private static bool Prefix(ref ShelfPlacer __instance, ShelfItem item, ref bool __result)
	{
		Main.Log(nameof(ShelfPlacer_TryPlaceOnAnyShelf_Patch));
		Main.Log($"item: {item.name}");
		
		var num = Random.Range(0, __instance.shelves.Length);
		
		for (var index = 0; index < __instance.shelves.Length; ++index)
		{
			var shelf = __instance.shelves[(index + num) % __instance.shelves.Length];
			
			if (!(item.Depth <= (double)shelf.depth)) continue;
			
			if (item.Width > (double) shelf.length)
				Debug.LogError("Found item that's longer than the shelf length: " + item.name, __instance.gameObject);
			
			if (!shelf.TryGetEmptySpace(item.Width, out var space)) continue;
			
			item.transform.position = Vector3.Lerp(shelf.leftEnd.position, shelf.rightEnd.position, Mathf.InverseLerp(0.0f, shelf.length, (float) ((space.x + (double) space.y) / 2.0)));
			item.transform.rotation = shelf.leftEnd.rotation;
			
			Main.Log($"placed {item.name} on a shelve");
			
			__result = true;
			return false;
		}
		
		__result = false;
		return false;
	}
}


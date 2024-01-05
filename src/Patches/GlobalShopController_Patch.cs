using DV.Shops;
using HarmonyLib;

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
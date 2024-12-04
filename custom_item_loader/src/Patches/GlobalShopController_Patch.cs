using DV.Shops;
using HarmonyLib;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace custom_item_mod.Patches;

[HarmonyPatch(typeof(GlobalShopController))]
public class GlobalShopController_Awake_Patch
{
	[HarmonyPatch(nameof(GlobalShopController.Awake))]
	[HarmonyPrefix]
	private static void Prefix()
	{
		ItemModsFinder.InitializeItems();
	}

	[HarmonyPatch(nameof(GlobalShopController.Awake))]
	[HarmonyPostfix]
	private static void Postfix(GlobalShopController __instance)
	{
		ItemModsFinder.FinalizeItems(__instance);
	}
}


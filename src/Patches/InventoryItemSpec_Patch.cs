using System;
using DV.Localization;
using HarmonyLib;

namespace custom_item_mod.Patches;

/// <summary>
/// IDK why but a NullReferenceException occurs without this patch. Pasted the exception at the bottom of this file.
/// </summary>
//[HarmonyPatch(typeof(InventoryItemSpec))]
//[HarmonyPatch(nameof(InventoryItemSpec.LocalizedName), MethodType.Getter)]
//public class InventoryItemSpec_Patch
//{
//	private static Exception Finalizer(Exception __exception, ref InventoryItemSpec __instance, ref string __result)
//	{
//		if(__instance.itemPrefabName.StartsWith(Main.MyModEntry.Info.Id) &&
//		   __exception.GetType() == typeof(NullReferenceException))
//		{
//			__result = LocalizationAPI.L(__instance.localizationKeyName);
//			return null;
//		}
		
//		return __exception;
//	}
//}

/*
NullReferenceException
  at (wrapper managed-to-native) UnityEngine.Component.GetComponentFastPath(UnityEngine.Component,System.Type,intptr)
  at UnityEngine.Component.GetComponent[T] () [0x00021] in <85d1d3e7744a4a47b5f51883bf40bba2>:0 
  at (wrapper dynamic-method) InventoryItemSpec.InventoryItemSpec.get_LocalizedName_Patch0(InventoryItemSpec)
  at DV.Shops.ShopScanner.UpdateText () [0x00052] in <dada6c6fa778466988cb5a66bbff0984>:0 
  at DV.Shops.ShopScanner.Update () [0x00113] in <dada6c6fa778466988cb5a66bbff0984>:0 
*/
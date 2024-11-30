using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace custom_item_mod.Patches;

/// <summary>
/// DV loads items by sending InventoryItemSpec.itemPrefabName to Resources.Load. This patch intercepts our custom item and makes sure it loads.
/// </summary>
[HarmonyPatch]
public class Resources_Load_Patch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return AccessTools.GetTypesFromAssembly(Assembly.GetAssembly(typeof(Resources)))
			.SelectMany(type => type.GetMethods())
			.Where(method => method.ReturnType == typeof(Object) &&
			                 method.Name == nameof(Resources.Load) &&
			                 method.GetParameters().Length == 1 &&
			                 method.GetParameters()[0].ParameterType == typeof(string))
			.Cast<MethodBase>();
	}
	
	private static bool Prefix(MethodBase __originalMethod, string path, ref Object __result)
	{
		//Main.Log($"Possibly patching asset loading for {path}");
		foreach (var item in ItemModsFinder.CustomItems)
		{
			if (path != item.ItemSpec.itemPrefabName) continue;

			Main.Log($"Patched asset loading for {path}");
			__result = item.ItemPrefab;
			return false; //skip original
		}

		return true;
	}
}
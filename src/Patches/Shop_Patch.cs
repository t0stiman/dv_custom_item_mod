using System.Linq;
using DV.Shops;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace custom_item_mod.Patches;

[HarmonyPatch(typeof(Shop))]
[HarmonyPatch(nameof(Shop.Awake))]
public class Shop_Awake_Patch
{
	private static void Postfix(ref Shop __instance)
	{
		foreach (var item in ItemModsFinder.CustomItems)
		{
			AddItemToShop(item, ref __instance);
		}
	}

	private static void AddItemToShop(CustomItem anItem, ref Shop aShop)
	{
		var soldOnlyAt = anItem.ShopData.soldOnlyAt;
		if (soldOnlyAt.Any() && !soldOnlyAt.Contains(aShop)) return;

		Main.Log($"{nameof(AddItemToShop)} {anItem.Name} {aShop.name}");

		var existingScanModuleObject = aShop.scanItemResourceModules[0].gameObject;
		var newScanModuleObject =
			Object.Instantiate(existingScanModuleObject, existingScanModuleObject.transform.parent);

		newScanModuleObject.name = $"{anItem.ItemSpec.LocalizedName}_ShelfItem";

		var module = newScanModuleObject.GetComponent<ScanItemCashRegisterModule>();
		module.sellingItemSpec = anItem.ItemSpec;

		var shelfItem = newScanModuleObject.GetComponent<ShelfItem>();
		shelfItem.SetValues(anItem.ShopData.shelfItem);

		aShop.scanItemResourceModules = aShop.scanItemResourceModules.Append(module).ToArray();
		aShop.cashRegister.registerModules = aShop.cashRegister.registerModules.Append(module).ToArray();

		foreach (var child in newScanModuleObject.transform.GetChildren().Where(child => child.name.Contains("- preview")))
		{
			Main.Log($"destroying {child.name}");
			Object.Destroy(child.gameObject);
		}
	}
}
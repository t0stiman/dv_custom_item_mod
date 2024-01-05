using HarmonyLib;
using I2.Loc;

namespace custom_item_mod.Patches;

[HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTranslation))]
public static class LocalizationManager_GetTranslation_Patch
{
	private static bool Prefix(ref string __result, string Term)
	{
		if (!Term.StartsWith(Main.MyModEntry.Info.Id)) {
			return true;
		}

		foreach (var item in ItemModsFinder.CustomItems)
		{
			if (Term == item.ItemSpec.localizationKeyName)
			{
				__result = item.Name;
				return false;
			} 
			if (Term == item.ItemSpec.localizationKeyDescription)
			{
				__result = item.Description;
				return false;
			}
		}
		
		__result = "< missing translation >";

		return false;
	}
}

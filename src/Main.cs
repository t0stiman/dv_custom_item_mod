using System;
using System.Reflection;
using UnityModManagerNet;
using HarmonyLib;

namespace custom_item_mod
{
	[EnableReloading]
	static class Main
	{
		public static UnityModManager.ModEntry MyModEntry;
		private static Harmony myHarmony;

		//================================================================

		private static bool Load(UnityModManager.ModEntry modEntry)
		{
			try
			{
				MyModEntry = modEntry;
				
				modEntry.OnToggle = OnToggle;
				modEntry.OnUnload = OnUnload;

				ItemModsFinder.Setup();

				myHarmony = new Harmony(MyModEntry.Info.Id);
				myHarmony.PatchAll(Assembly.GetExecutingAssembly());
			}
			catch (Exception ex)
			{
				MyModEntry.Logger.LogException($"Failed to load {MyModEntry.Info.DisplayName}:", ex);
				myHarmony?.UnpatchAll(MyModEntry.Info.Id);
				return false;
			}
			
			modEntry.Logger.Log("loaded");

			return true;
		}

		private static bool OnUnload(UnityModManager.ModEntry modEntry)
		{
			myHarmony?.UnpatchAll(MyModEntry.Info.Id);
			ItemModsFinder.Unload();
			return true;
		}

		private static bool OnToggle(UnityModManager.ModEntry modEntry, bool enabled) 
		{
			string msg = enabled ? "hello!" : "goodbye!";
			modEntry.Logger.Log(msg);

			return true;
		}
		
		// Logger Commands
		public static void Log(string message)
		{
			MyModEntry.Logger.Log(message);
		}

		public static void Warning(string message)
		{
			MyModEntry.Logger.Warning(message);
		}

		public static void Error(string message)
		{
			MyModEntry.Logger.Error(message);
		}
	}
}
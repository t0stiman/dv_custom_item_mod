using System;
using System.Collections;
using DV.CashRegister;
using HarmonyLib;
using I2.Loc;
using UnityEngine;

namespace custom_item_mod.Patches;

[HarmonyPatch(typeof(CashRegisterWithModules))]
[HarmonyPatch(nameof(CashRegisterWithModules.OnEnable))]
public class CashRegisterWithModules_UpdateText_Patch
{
	private static void Postfix(ref CashRegisterWithModules __instance)
	{
		var shops = __instance.transform.GetComponentsInParent<Shop>();
		if (shops.Length <= 0)
		{
			return; //not all CashRegisterWithModules belong to a shop
		}

		__instance.StartCoroutine(OrganizeSigns(shops[0]));
	}
	
	private static IEnumerator OrganizeSigns(Shop aShop)
	{
		yield return new WaitForSeconds(2);
		
		Main.Log($"{nameof(OrganizeSigns)} shop: {aShop.gameObject.GetPath()}");
		
		var baseObjTransform = aShop.scanItemResourceModules[0].transform;
		var baseObjPos = baseObjTransform.position;

		var distanceLeft = GetDistanceToWall(baseObjPos, -baseObjTransform.right);
		var distanceRight = GetDistanceToWall(baseObjPos, baseObjTransform.right);
		
		var distanceUp = GetDistanceToWall(baseObjPos, baseObjTransform.up);
		var distanceDown = GetDistanceToWall(baseObjPos, -baseObjTransform.up);

		if (distanceLeft < 0 || distanceRight < 0 || distanceDown < 0 || distanceUp < 0)
		{
			yield break;
		}
		
		var wallWidth = distanceLeft + distanceRight;
		var wallHeight = distanceUp + distanceDown;
		
		
		int row = 0;
		int column = 0;
		
		var targetOffset = new Vector2 (0.6f, 0.288f); // its what the vanilla shops use
		int itemsPerRow = (int)Math.Floor(wallWidth / targetOffset.x) - 1;
		int itemsPerColumn = (int)Math.Floor(wallHeight / targetOffset.x) - 1;
		var realOffset = new Vector2 (wallWidth / (itemsPerRow+1), wallHeight / (itemsPerColumn+1));
		var baseObjLocalPos = baseObjTransform.localPosition;
		var topLeftLocalPos = new Vector3(
			baseObjLocalPos.x-distanceLeft+realOffset.x,
			baseObjLocalPos.y+distanceUp-realOffset.y,
			baseObjLocalPos.z);
		
		foreach (var scanMod in aShop.scanItemResourceModules)
		{
			scanMod.transform.localPosition = new Vector3(topLeftLocalPos.x + column * realOffset.x, topLeftLocalPos.y + row * realOffset.y, topLeftLocalPos.z );
			
			column++;
			if (column >= itemsPerRow)
			{
				column = 0;
				row++;
			}
		}
	} 
	
	private static float GetDistanceToWall(Vector3 origin, Vector3 direction)
	{
		var raycastHits = Physics.RaycastAll(origin, direction, 100f, ~0); // ~0 makes it hit all layers
		foreach (var hit in raycastHits)
		{
			if (hit.transform.gameObject.name == "Interior")
				// if(hit.transform.gameObject.GetPath().EndsWith("Shop/Colliders/Interior"))
			{
				Main.Log($"{hit.transform.GetPath()}");
				return hit.distance;
			}
		}
		
		Main.Error($"{nameof(Shop_Awake_Patch)}: raycast {direction} failed. hits:");
		foreach (var hit in raycastHits)
		{
			Main.Log(hit.transform.gameObject.GetPath());
		}

		return -1;
	}
	
	// private static bool NearlyEqual(float a, float b, float margin)
	// {
	// 	return a==b || ( a < b + margin && a > b - margin);
	// }
}
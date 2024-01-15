using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace custom_item_mod;

public static class Extensions
{
	public static List<Transform> GetChildren(this Transform myTransform)
	{
		var returno = new List<Transform>(myTransform.childCount);
		
		for (int i = 0; i < myTransform.childCount; i++)
		{
			returno.Add(myTransform.GetChild(i));
		}

		return returno;
	}
	
	/// <summary>
	/// Add a component of type TComponent to gameObject if it doesn't already have it
	/// </summary>
	public static void AddComponentIfNotHave<TComponent>(this GameObject gameObject)
	where TComponent : Component
	{
		if (gameObject.GetComponent<TComponent>() == null) {
			gameObject.AddComponent<TComponent>();
		}
	}
	
	public static void SetLayerIncludingChildren(this GameObject aGameObject, int layerNumber)
	{
		aGameObject.transform.SetLayerIncludingChildren(layerNumber);
	}

	public static void SetLayerIncludingChildren(this Transform aTransform, int layerNumber)
	{
		aTransform.gameObject.layer = layerNumber;
		foreach (var transformChild in aTransform.GetChildren())
		{
			transformChild.SetLayerIncludingChildren(layerNumber);
		}
	}

	public static string GetPath(this GameObject gameObject)
	{
		return string.Join("/", gameObject.GetComponentsInParent<Transform>().Select(t => t.name).Reverse().ToArray());
	}

	public static void Start(this Shop shop)
	{
		Main.Log("Shop.Start");
	}
}
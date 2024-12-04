using System.Collections.Generic;
using DV.Shops;
using Unity.Mathematics;
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

	public static void SetValues(this ShelfItem this_, ShelfItem other)
	{
		this_.size = other.size;
		this_.height = other.height;
		
		if (!this_.TryGetComponent(typeof(BoxCollider), out Component boxCollider)) return;
		
		//regenerate the collider
		var boxColliderComp = (BoxCollider)boxCollider;
		
		// Values copied from ShelfItem.Awake
		boxColliderComp.center = new float3(0.0f, this_.height * 0.5f, this_.size.y * -0.5f);
		boxColliderComp.size = new float3(this_.size.x, this_.height, this_.size.y);
	}
}
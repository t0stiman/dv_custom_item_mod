﻿using System;
using System.Collections.Generic;
using System.Linq;
using DV.CabControls;
using DV.CabControls.Spec;
using DV.CashRegister;
using DV.Interaction;
using DV.Shops;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace custom_item_mod;

public class CustomItem
{
	public string Name;
	public string Description;

	public GameObject ItemPrefab { get; private set; }

	public InventoryItemSpec ItemSpec => ShopData.item;
	public ShopItemData ShopData { get; private set; }

	public GameObject ProvidedPrefab { get; private set; }
	
	
	/// <param name="itemInfo">The parsed item info json</param>
	/// <param name="unityPrefab">the prefab of the item as extracted from the asset bundle</param>
	/// <param name="iconStandard">the inventory icon</param>
	/// <param name="iconDropped">this icon will be shown in your inventory when you've dropped the item. For vanilla game items, this is a blue (#72A2B3) silhouette of the item.</param>
	/// <param name="previewRotation">TODO what is this</param>
	/// <param name="soldOnlyAt">item will only be available at these shops</param>
	/// <param name="previewBounds">TODO IDK what this is</param>
	/// <param name="careerOnly">if true, the item can only be purchased it career mode</param>
	/// <param name="immuneToDumpster">if true, item won't be destroyed by dumpsters (I think)</param>
	/// <param name="isEssential">if true, the item will leave a ghost in your inventory when you drop it and the inventory slot will be reserved for this item</param>
	public CustomItem(
		CustomItemInfo itemInfo,
		GameObject unityPrefab,
		Sprite iconStandard,
		Sprite iconDropped,
		Vector3 previewRotation = default,
		List<Shop> soldOnlyAt = default,
		bool careerOnly = false,
		bool immuneToDumpster = true,
		bool isEssential = false 
	)
	{
		// C# pls
		if (soldOnlyAt == default(List<Shop>)) { soldOnlyAt = new List<Shop>(); }
		Main.Log($"Instantiating {itemInfo.Name}");
		
		Name = itemInfo.Name;
		Description = itemInfo.Description;
		ItemPrefab = CreateItemPrefab(unityPrefab);
		ProvidedPrefab = unityPrefab;

		Main.Log($"Loaded prefabs for {itemInfo.Name}");

		var previewBounds = itemInfo.PreviewBounds;
		Main.Log($"Preview bounds: {previewBounds}");
		if (previewBounds == default)
		{
			previewBounds = new Vector3(0.35f, 0.3f, 0.2f);
		}
		if (previewRotation == default) { previewRotation = Vector3.zero; }

		var itemSpec = SetupItemSpec(immuneToDumpster, isEssential, iconStandard, iconDropped, previewBounds, previewRotation);
		Main.Log($"Built item spec for {itemInfo.Name}");

		var shelfObject = new GameObject();
		shelfObject.SetActive(false);
		shelfObject.transform.parent = ItemPrefab.transform.parent;
		shelfObject.name = $"{ItemPrefab.name}_ShelfItem";
		var shelfItemComponent = shelfObject.AddComponent<ShelfItem>();
		shelfItemComponent.size = new Vector2(previewBounds.x, previewBounds.y);

		Main.Log($"Built shelf object for {itemInfo.Name}");
		
		ShopData = new ShopItemData
		{
			item = itemSpec,
			shelfItem = shelfItemComponent,
			amount = itemInfo.Amount,
			basePrice = itemInfo.Price,
			careerOnly = careerOnly,
			soldOnlyAt = soldOnlyAt
		};
	}

	/// <summary>
	/// Called to handle initialization steps that depend on GlobalShopController
	/// </summary>
	/// <param name="shopController"></param>
	public void FinishInitialization(GlobalShopController shopController)
	{
		// Create the scan tag for purchasing
		var tag = Object.Instantiate(shopController.scanItemShelfPrefab.transform.Find("ScanItem"));
		tag.parent = ShopData.shelfItem.transform;
		tag.name = "ScanItem";

		// Add a preview object to show on the shelf
		var shopSample = UnityEngine.Object.Instantiate(ProvidedPrefab);
		shopSample.name = $"{ProvidedPrefab.name} - preview";
		shopSample.transform.parent = ShopData.shelfItem.transform;
		// this probably needs to be adjusted for other items - we may want to make this something in the json?
		// TODO: figure out a sane way to calculate this.
		shopSample.transform.localPosition = new Vector3(0f, 0f, -0.1f);

		// Add the ScanItemCashRegisterModule to the shelf item
		var module = ShopData.shelfItem.gameObject.AddComponent<ScanItemCashRegisterModule>();
		module.sellingItemSpec = ShopData.item;
		module.itemNameText = ShopData.shelfItem.transform.Find("ScanItem/Texts/Name").gameObject.GetComponent<TextMeshPro>();
		module.itemPriceText = ShopData.shelfItem.transform.Find("ScanItem/Texts/Price").gameObject.GetComponent<TextMeshPro>();

		// Add the shopItemData to the GlobalShopController - we'll later have the shopcontroller recalculate it's available stock
		shopController.shopItemsData.Add(ShopData);
	}

	/// <summary>
	/// Add a shop item to a specific shop - this should be called once per shop item and shop - it will decide if that shop is disallowed or not
	/// </summary>
	/// <param name="shop"></param>
	public void AddToShop(Shop shop)
	{
		if (ShopData.soldOnlyAt != default(List<Shop>) && ShopData.soldOnlyAt.Count > 0 && !ShopData.soldOnlyAt.Contains(shop))
		{
			// We're not supposed to add this item to this shop.
			return;
		}

		Main.Log($"Adding {Name} to {shop.name}");

		// Create a new shelf item from the shelf item prefab
		var shelfItem = UnityEngine.Object.Instantiate(ShopData.shelfItem);
		shelfItem.name = ShopData.shelfItem.name;
		shelfItem.transform.parent = shop.transform.Find("ScanItemAnchor");
		Main.Log($"{Name} shop item created");

		Main.Log($"{Name} scanModule initialized");
		// add scanItemCashRegisterModule to the shop and the register
		var module = shelfItem.GetComponent<ScanItemCashRegisterModule>();
		shop.scanItemResourceModules = shop.scanItemResourceModules.AddItem(module).ToArray();
		var register = shop.GetComponentInChildren<CashRegisterWithModules>();
		register.registerModules = register.registerModules.AddItem(module).ToArray();
		shelfItem.gameObject.SetActive(true);

		Main.Log($"{Name} added to {shop.name}");
	}

	private GameObject CreateItemPrefab(GameObject unityPrefab)
	{
		// root object is just a container object - it'll never be returned to anything else
		// however it allows it's child to look "active" while being not active
		var rootObject = new GameObject();
		rootObject.name = $"{unityPrefab.name}_root";
		rootObject.SetActive(false);

		// create the item object itself - using the prefab provided by the custom item author
		var item = Object.Instantiate(unityPrefab);
		item.name = unityPrefab.name;
		item.transform.parent = rootObject.transform;

		//this makes sure the item collides with the right stuff
		item.SetLayerIncludingChildren(LayerMask.NameToLayer("World_Item"));

		// these components are standard requirements for every item but cannot be added in Unity due to them being in Assembly and not a custom dll
		var itemSpec = item.AddComponent<DV.CabControls.Spec.Item>();
		item.AddComponent<DV.Items.TrainItemActivityHandlerOverride>();
		item.AddComponent<ItemSaveData>();
		item.AddComponent<ShopRestocker>();

		//TODO: Provide a way for item authors to declare which colliders are standard interaction colliders and which are not - needed for gadget support
		itemSpec.colliderGameObjects = (from c in item.GetComponentsInChildren<Collider>()
									   select c.gameObject).ToArray();
		return item;
	}

	private InventoryItemSpec SetupItemSpec(
		bool immuneToDumpster,
		bool isEssential,
		Sprite iconStandard,
		Sprite iconDropped,
		Vector3 previewBounds,
		Vector3 previewRotation
		)
	{
		var itemSpec = ItemPrefab.AddComponent<InventoryItemSpec>();
		
		var itemKey = $"{Main.MyModEntry.Info.Id}/{Name}";
		//name of the item
		itemSpec.localizationKeyName = $"{itemKey}/name";
		//description of the item
		itemSpec.localizationKeyDescription = $"{itemKey}/description";
		
		itemSpec.immuneToDumpster = immuneToDumpster;
		itemSpec.isEssential = isEssential;
			
		//In the base game, this is the path to the Prefab passed to Resources.Load.
		//In our case, we will put the mod id in there so we know its our item in GlobalShopController_InstantiatePurchasedItem_Patch
		itemSpec.itemPrefabName = $"{itemKey}/prefab";
			
		//this is the old black and white inventory icon from DV Overhauled. Not used anymore but i'm gonna set it anyway just in case.
		itemSpec.itemIconSprite = iconStandard;
		
		//this is the currently used inventory icon
		itemSpec.itemIconSpriteStandard = iconStandard;
		//this icon will be shown in your inventory when you've dropped the item. For vanilla game items, this is a blue (#72A2B3) silhouette of the item.
		itemSpec.itemIconSpriteDropped = iconDropped;
			
		//this preview is shown when you hold R to place an item
		itemSpec.previewPrefab = ItemPrefab;
		itemSpec.previewBounds = new Bounds(Vector3.zero, previewBounds);
		itemSpec.previewRotation = previewRotation;
		
		return itemSpec;
	}
}

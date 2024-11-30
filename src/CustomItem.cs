using System;
using System.Collections.Generic;
using System.Linq;
using DV.CabControls.Spec;
using DV.CashRegister;
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
	// public ShelfItem ShelfComponent => ShopData.shelfItem;
	public ShopItemData ShopData { get; private set; }

	public GameObject ProvidedPrefab { get; private set; }
	
	
	/// <param name="name">name of the item</param>
	/// <param name="description">description of the item (shown in the shops)</param>
	/// <param name="itemPrefab">the prefab of the item</param>
	/// <param name="amount">How many of this item the player can buy. After they buy this amount, the item will be out of stock.</param>
	/// <param name="price">price in DV dollars</param>
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
		Bounds previewBounds = default,
		Vector3 previewRotation = default,
		List<Shop> soldOnlyAt = default,
		bool careerOnly = false,
		bool immuneToDumpster = true,
		bool isEssential = false 
	)
	{
		// C# pls
		if (soldOnlyAt == default(List<Shop>)) { soldOnlyAt = new List<Shop>(); }
		if (previewBounds == default(Bounds)) {
			previewBounds = new Bounds(Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f));
		}
		if (previewRotation == default) { previewRotation = Vector3.zero; }

		Main.Log($"Instantiating {itemInfo.Name}");
		
		Name = itemInfo.Name;
		Description = itemInfo.Description;
		ItemPrefab = CreateItemPrefab(unityPrefab);
		ProvidedPrefab = unityPrefab;

		Main.Log($"Loaded prefabs for {itemInfo.Name}");

		var itemSpec = SetupItemSpec(immuneToDumpster, isEssential, iconStandard, iconDropped, previewBounds, previewRotation);
        Main.Log($"Built item spec for {itemInfo.Name}");

		var shelfObject = new GameObject();
		shelfObject.SetActive(false);
		shelfObject.name = $"{ItemPrefab.name}_ShelfItem";
		shelfObject.SetActive(false);
		var shelfItemComponent = shelfObject.AddComponent<ShelfItem>();

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

	public void FinishInitialization(GlobalShopController shopController)
	{
		var tag = Object.Instantiate(shopController.scanItemShelfPrefab.transform.Find("ScanItem"));
		tag.parent = ShopData.shelfItem.transform;
		tag.name = "ScanItem";
		shopController.shopItemsData.Add(ShopData);
    }
    public void AddToShop(Shop shop)
	{
        if (ShopData.soldOnlyAt != default(List<Shop>) && ShopData.soldOnlyAt.Count > 0 && !ShopData.soldOnlyAt.Contains(shop))
        {
            return;
        }
        Main.Log($"Adding {Name} to {shop.name}");
        var shopItem = UnityEngine.Object.Instantiate(ShopData.shelfItem);
        shopItem.name = ShopData.shelfItem.name;
        shopItem.transform.parent = shop.transform.Find("ScanItemAnchor");
        var shopSample = UnityEngine.Object.Instantiate(ProvidedPrefab);
        shopSample.name = $"{ProvidedPrefab.name} - preview";
        shopSample.transform.parent = shopItem.transform;
        shopSample.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        Main.Log($"{Name} shop item created");
        var module = shopItem.gameObject.AddComponent<ScanItemCashRegisterModule>();
        module.sellingItemSpec = ShopData.item;
        module.itemNameText = shopItem.transform.Find("ScanItem/Texts/Name").gameObject.GetComponent<TextMeshPro>();
        module.itemPriceText = shopItem.transform.Find("ScanItem/Texts/Price").gameObject.GetComponent<TextMeshPro>();
        Main.Log($"{Name} scanModule initialized");
        shop.scanItemResourceModules = shop.scanItemResourceModules.AddItem(module).ToArray();
        var register = shop.GetComponentInChildren<CashRegisterWithModules>();
        register.registerModules = register.registerModules.AddItem(module).ToArray();
        shopItem.gameObject.SetActive(true);
        Main.Log($"{Name} added to {shop.name}");
    }

    private GameObject CreateItemPrefab(GameObject unityPrefab)
    {
        var item = Object.Instantiate(unityPrefab);
		item.name = unityPrefab.name;
		item.SetActive(false);
		Object.DontDestroyOnLoad(item);
        //this makes sure the item collides with the right stuff
        item.SetLayerIncludingChildren(LayerMask.NameToLayer("World_Item"));

        item.AddComponent<DV.CabControls.Spec.Item>();
        item.AddComponent<DV.Items.TrainItemActivityHandlerOverride>();
        item.AddComponent<ItemSaveData>();
        item.AddComponent<ShopRestocker>();
		return item;
    }

    private InventoryItemSpec SetupItemSpec(
		bool immuneToDumpster,
		bool isEssential,
		Sprite iconStandard,
		Sprite iconDropped,
		Bounds previewBounds,
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
		itemSpec.previewBounds = previewBounds;
		itemSpec.previewRotation = previewRotation;
		
		return itemSpec;
	}
}

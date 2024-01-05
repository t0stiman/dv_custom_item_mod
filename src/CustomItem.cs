using System;
using System.Collections.Generic;
using DV.Shops;
using UnityEngine;

namespace custom_item_mod;

public class CustomItem
{
	/// <param name="name">name of the item</param>
	/// <param name="description">description of the item (shown in the shops)</param>
	/// <param name="itemPrefab">the prefab of the item</param>
	/// <param name="amount">How many of this item the player can buy. If they buy this amount, the item will be out of stock.</param>
	/// <param name="price">price in DV dollars</param>
	/// <param name="iconStandard">the inventory icon</param>
	/// <param name="iconDropped">this icon will be shown in your inventory when you've dropped the item. For vanilla game items, this is a blue (#72A2B3) silhouette of the item.</param>
	/// <param name="previewPrefab">this object is shown when you hold R to place the item</param>
	/// <param name="soldOnlyAt">item will only be available at these shops</param>
	/// <param name="previewBounds">todo idk what this is</param>
	/// <param name="careerOnly">if true, the item can only be purchased it career mode</param>
	/// <param name="immuneToDumpster">if true, item won't be destroyed by dumpsters (I think)</param>
	/// <param name="isEssential">if true, the item will leave a ghost in your inventory when you drop it and the inventory slot will be reserved for this item</param>
	public CustomItem(
		string name,
		string description,
		GameObject itemPrefab,
		int amount,
		int price,
		Sprite iconStandard,
		Sprite iconDropped,
		GameObject previewPrefab = default,
		List<Shop> soldOnlyAt = default,
		Bounds previewBounds = default,
		bool careerOnly = false,
		bool immuneToDumpster = true,
		bool isEssential = false 
	)
	{
		// C# pls
		if (previewPrefab == default(GameObject)) { previewPrefab = itemPrefab; }
		if (soldOnlyAt == default(List<Shop>)) { soldOnlyAt = new List<Shop>(); }
		if (previewBounds == default(Bounds)) {
			previewBounds = new Bounds(Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f));
		}
		
		Name = name;
		Description = description;
		ItemPrefab = itemPrefab;
		//this makes sure the item collides with the right stuff
		ItemPrefab.SetLayerIncludingChildren(LayerMask.NameToLayer("World_Item"));

		SetupItemSpec(immuneToDumpster, isEssential, iconStandard, iconDropped, previewPrefab, previewBounds);
		SetupShopData(amount, price, careerOnly, soldOnlyAt);
		SetupOtherComponents();
	}

	public string Name;
	public string Description;
	// price of the item in dollars
	public int Price => (int)Math.Round(ShopData.basePrice);
	// price as a string 
	public string PriceText => "$"+Price;

	public GameObject ItemPrefab { get; private set; }

	public InventoryItemSpec ItemSpec { get; private set; }
	public ShopItemData ShopData { get; private set; }

	private void SetupItemSpec(
		bool immuneToDumpster,
		bool isEssential,
		Sprite iconStandard,
		Sprite iconDropped,
		GameObject previewPrefab,
		Bounds previewBounds
		)
	{
		ItemSpec = ItemPrefab.AddComponent<InventoryItemSpec>();
		
		var itemKey = $"{Main.MyModEntry.Info.Id}/{Name}";
		//name of the item
		ItemSpec.localizationKeyName = $"{itemKey}/name";
		//description of the item
		ItemSpec.localizationKeyDescription = $"{itemKey}/description";
		
		ItemSpec.immuneToDumpster = immuneToDumpster;
		ItemSpec.isEssential = isEssential;
			
		//In the base game, this is the path to the Prefab passed to Resources.Load.
		//In our case, we will put the mod id in there so we know its our item in GlobalShopController_InstantiatePurchasedItem_Patch
		ItemSpec.itemPrefabName = $"{itemKey}/prefab_name";
			
		//this is the old black and white inventory icon from DV Overhauled. Not used anymore but i'm gonna set it anyway just in case.
		ItemSpec.itemIconSprite = iconStandard;
		
		//this is the currently used inventory icon
		ItemSpec.itemIconSpriteStandard = iconStandard;
		//this icon will be shown in your inventory when you've dropped the item. For vanilla game items, this is a blue (#72A2B3) silhouette of the item.
		ItemSpec.itemIconSpriteDropped = iconDropped;
			
		//this preview is shown when you hold R to place an item
		ItemSpec.previewPrefab = previewPrefab;
		ItemSpec.previewBounds = previewBounds;
	}
	
	private void SetupShopData(int amount, int price, bool careerOnly, List<Shop> soldOnlyAt)
	{
		ShopData = new ShopItemData();
			
		ShopData.item = ItemSpec;
		ShopData.amount = amount;
		ShopData.basePrice = price;
		ShopData.careerOnly = careerOnly;
		ShopData.soldOnlyAt = soldOnlyAt;
	}
	
	private void SetupOtherComponents()
	{
		ItemPrefab.AddComponentIfNotHave<DV.CabControls.Spec.Item>();
		//im just going to use the defaults. Will probably work, right? 
		
		ItemPrefab.AddComponentIfNotHave<DV.Items.TrainItemActivityHandlerOverride>();

		// ItemSaveData (adds the item to your save file, maybe?)
		ItemPrefab.AddComponentIfNotHave<ItemSaveData>();
		
		ItemPrefab.AddComponentIfNotHave<ShopRestocker>();
	}
}
using System;
using System.Collections.Generic;
using DV.Shops;
using UnityEngine;
using Object = UnityEngine.Object;

namespace custom_item_mod;

public class CustomItem
{
	public string Name;
	public string Description;

	private readonly GameObject ItemPrefab;
	public GameObject WorldObject {get; private set;}

	public InventoryItemSpec ItemSpec => ShopData.item;
	// public ShelfItem ShelfComponent => ShopData.shelfItem;
	public ShopItemData ShopData { get; private set; }
	
	
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
		string name,
		string description,
		GameObject itemPrefab,
		int amount,
		int price,
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
		
		Name = name;
		Description = description;
		ItemPrefab = itemPrefab;
		
		WorldObject = Object.Instantiate(itemPrefab);
		WorldObject.name = itemPrefab.name;
		
		var itemSpec = SetupItemSpec(immuneToDumpster, isEssential, iconStandard, iconDropped, previewBounds, previewRotation);
		
		//this makes sure the item collides with the right stuff
		WorldObject.SetLayerIncludingChildren(LayerMask.NameToLayer("World_Item"));
		
		WorldObject.AddComponent<DV.CabControls.Spec.Item>();
		WorldObject.AddComponent<DV.Items.TrainItemActivityHandlerOverride>();
		WorldObject.AddComponent<ItemSaveData>();
		WorldObject.AddComponent<ShopRestocker>();

		var shelfItemComponent = WorldObject.AddComponent<ShelfItem>();
		
		// WorldObject.SetActive(false);
		
		ShopData = new ShopItemData
		{
			item = itemSpec,
			shelfItem = shelfItemComponent,
			amount = amount,
			basePrice = price,
			careerOnly = careerOnly,
			soldOnlyAt = soldOnlyAt
		};
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
		var itemSpec = WorldObject.AddComponent<InventoryItemSpec>();
		
		var itemKey = $"{Main.MyModEntry.Info.Id}/{Name}";
		//name of the item
		itemSpec.localizationKeyName = $"{itemKey}/name";
		//description of the item
		itemSpec.localizationKeyDescription = $"{itemKey}/description";
		
		itemSpec.immuneToDumpster = immuneToDumpster;
		itemSpec.isEssential = isEssential;
			
		//In the base game, this is the path to the Prefab passed to Resources.Load.
		//In our case, we will put the mod id in there so we know its our item in GlobalShopController_InstantiatePurchasedItem_Patch
		itemSpec.itemPrefabName = $"{itemKey}/prefab_name";
			
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

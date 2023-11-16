using System.Collections.Generic;
using Kobold;
using UnityEngine;
using UnityEngine.UI;

public class InventoryFilterBarBehaviour : MonoBehaviour
{
	public BaseInventoryView inventory;

	public InventoryFilterBarButton toggledButton;

	public Dictionary<string, GameObject> categoryButtons = new Dictionary<string, GameObject>();

	public static Dictionary<string, string> categoryImageMap = new Dictionary<string, string>
	{
		{ "*", "Text/asterisk.png" },
		{ "Light Source", "Items/sw_torch_lit.png" },
		{ "Melee Weapon", "Items/sw_sword.bmp" },
		{ "Miscellaneous", "Items/sw_hookah.bmp" },
		{ "Food", "Items/sw_loaf.bmp" },
		{ "Corpse", "Items/sw_sword.bmp" },
		{ "Plant", "Items/sw_splat1.bmp" },
		{ "Missile Weapon", "Items/sw_revolver.bmp" },
		{ "Projectile", "Items/sw_sword.bmp" },
		{ "Ammo", "Items/sw_ammo.bmp" },
		{ "Armor", "Items/sw_plate_mail.bmp" },
		{ "Shields", "Items/sw_shield1.bmp" },
		{ "Grenades", "Items/sw_grenade.bmp" },
		{ "Creature", "Creatures/sw_snapjaw.bmp" },
		{ "Applicators", "Items/sw_spray.bmp" },
		{ "Energy Cell", "Items/sw_ammo.bmp" },
		{ "Natural Weapon", "Items/sw_sword.bmp" },
		{ "Natural Missile Weapon", "Items/sw_sword.bmp" },
		{ "Natural Armor", "Items/sw_sword.bmp" },
		{ "Meds", "Items/sw_hit.bmp" },
		{ "Tonics", "Items/sw_injector.bmp" },
		{ "Water Container", "Items/sw_bag.bmp" },
		{ "Books", "Items/sw_book1.bmp" },
		{ "Tools", "Tiles/sw_box.bmp" },
		{ "Artifacts", "Items/sw_goggles.bmp" },
		{ "Clothes", "Items/sw_leather_armor.bmp" },
		{ "Trade Goods", "Items/sw_necklace.bmp" },
		{ "Quest Items", "Items/sw_key.bmp" },
		{ "Data Disk", "Items/sw_disk2.bmp" },
		{ "Scrap", "Items/sw_gadget.bmp" },
		{ "Trinket", "Items/sw_gadget.bmp" }
	};

	public void Clear()
	{
		foreach (KeyValuePair<string, GameObject> categoryButton in categoryButtons)
		{
			PooledPrefabManager.Return(categoryButton.Value);
		}
		categoryButtons.Clear();
	}

	public void AddCategory(string category)
	{
		if (!categoryButtons.ContainsKey(category))
		{
			categoryButtons.Add(category, PooledPrefabManager.Instantiate("InventoryFilterBarButton"));
			categoryButtons[category].GetComponent<InventoryFilterBarButton>().filterBar = this;
			categoryButtons[category].transform.SetParent(base.transform);
			categoryButtons[category].transform.localScale = new Vector3(1f, 1f, 1f);
			categoryButtons[category].GetComponent<InventoryFilterBarButton>().category = category;
			string text = null;
			if (categoryImageMap.ContainsKey(category))
			{
				text = categoryImageMap[category];
			}
			if (text != null)
			{
				categoryButtons[category].transform.Find("3CImage").GetComponent<Image>().sprite = SpriteManager.GetUnitySprite(text);
			}
			categoryButtons[category].transform.SetSiblingIndex(categoryButtons.Count);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}

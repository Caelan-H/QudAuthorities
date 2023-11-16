using System;
using System.Collections.Generic;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class GenericInventoryRestocker : IPart
{
	public long LastRestockTick;

	public long RestockFrequency = 6000L;

	public int Chance = 71;

	public override bool SameAs(IPart p)
	{
		GenericInventoryRestocker genericInventoryRestocker = p as GenericInventoryRestocker;
		if (genericInventoryRestocker.LastRestockTick != LastRestockTick)
		{
			return false;
		}
		if (genericInventoryRestocker.RestockFrequency != RestockFrequency)
		{
			return false;
		}
		if (genericInventoryRestocker.Chance != Chance)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (E.Context != "Initialization" && E.ReplacementObject == null)
		{
			PerformStock();
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (LastRestockTick == 0L)
		{
			LastRestockTick = TurnNumber;
			return;
		}
		long num = TurnNumber - LastRestockTick;
		if (num >= RestockFrequency && IComponent<GameObject>.ThePlayer != null && IComponent<GameObject>.ThePlayer.InSameZone(ParentObject))
		{
			LastRestockTick = TurnNumber;
			if (!ParentObject.IsPlayerControlled() && !ParentObject.WasPlayer() && (Chance * (num / RestockFrequency)).in100())
			{
				PerformRestock();
			}
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "MadeHero");
		base.Register(Object);
	}

	public static Action<GameObject> GetCraftmarkApplication(GameObject who)
	{
		string MarkString = who.GetStringProperty("MakersMark");
		if (!string.IsNullOrEmpty(MarkString))
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("This item bears the mark of ").Append(who.DisplayNameOnlyStripped).Append(".");
			string propertyOrTag = who.GetPropertyOrTag("HeroGenericInventoryBasicBestowalChances");
			string propertyOrTag2 = who.GetPropertyOrTag("HeroGenericInventoryBasicBestowalPercentage");
			int BasicBestowalChances = ((!string.IsNullOrEmpty(propertyOrTag)) ? Convert.ToInt32(propertyOrTag) : 0);
			int BasicBestowalPercentage = ((!string.IsNullOrEmpty(propertyOrTag2)) ? Convert.ToInt32(propertyOrTag2) : 0);
			string MarkDesc = stringBuilder.ToString();
			return delegate(GameObject obj)
			{
				if (!obj.HasTag("AlwaysStack"))
				{
					int num = 5;
					obj.AddPart(new MakersMark(MarkString, MarkDesc));
					for (int i = 0; i < BasicBestowalChances; i++)
					{
						if (!BasicBestowalPercentage.in100())
						{
							break;
						}
						if (RelicGenerator.ApplyBasicBestowal(obj))
						{
							num += 30;
						}
					}
					obj.RequirePart<Commerce>().Value += num;
				}
			};
		}
		return null;
	}

	public void PerformStock(bool Restock = false, bool Silent = false)
	{
		Inventory inventory = ParentObject.Inventory;
		string context = (Restock ? "Restock" : "Stock");
		Action<GameObject> craftmarkApplication = GetCraftmarkApplication(ParentObject);
		if (!Restock)
		{
			foreach (GameObject @object in inventory.Objects)
			{
				if (!@object.HasPropertyOrTag("norestock") && !@object.HasProperty("_stock"))
				{
					@object.SetIntProperty("norestock", 1);
				}
			}
		}
		int intProperty = ParentObject.GetIntProperty("InventoryTier", ZoneManager.zoneGenerationContextTier);
		string propertyOrTag = ParentObject.GetPropertyOrTag("GenericInventoryRestockerPopulationTable");
		List<GameObject> list = new List<GameObject>(inventory.Objects);
		if (!string.IsNullOrEmpty(propertyOrTag))
		{
			ParentObject.EquipFromPopulationTable(propertyOrTag, intProperty, craftmarkApplication, context);
		}
		if (ParentObject.GetIntProperty("Hero") > 0)
		{
			string propertyOrTag2 = ParentObject.GetPropertyOrTag("HeroGenericInventoryRestockerPopulationTable");
			if (!string.IsNullOrEmpty(propertyOrTag2))
			{
				ParentObject.EquipFromPopulationTable(propertyOrTag2, intProperty, craftmarkApplication, context);
			}
		}
		bool flag = false;
		foreach (GameObject object2 in inventory.Objects)
		{
			if (!list.Contains(object2))
			{
				object2.SetIntProperty("_stock", 1);
				flag = true;
			}
		}
		if (flag && Restock && !Silent && IComponent<GameObject>.ThePlayer.InSameZone(ParentObject))
		{
			IComponent<GameObject>.AddPlayerMessage("{{G|" + ParentObject.The + ParentObject.ShortDisplayName + " " + ParentObject.GetVerb("have", PrependSpace: false) + " restocked " + ParentObject.its + " inventory!}}");
		}
	}

	public void PerformRestock(bool Silent = false)
	{
		Inventory inventory = ParentObject.Inventory;
		List<GameObject> list = Event.NewGameObjectList();
		list.AddRange(inventory.Objects);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject = list[i];
			if (gameObject.HasProperty("_stock") && !gameObject.HasPropertyOrTag("norestock") && !gameObject.IsImportant())
			{
				inventory.RemoveObject(gameObject);
				gameObject.Obliterate();
			}
		}
		PerformStock(Restock: true, Silent);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "MadeHero")
		{
			PerformRestock(Silent: true);
		}
		return base.FireEvent(E);
	}
}

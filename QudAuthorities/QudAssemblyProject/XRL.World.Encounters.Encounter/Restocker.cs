using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class Restocker : IPart
{
	public List<string> WaresBuilders = new List<string>();

	public long NextRestockTick;

	public long RestockFrequency = 6000L;

	public Restocker()
	{
	}

	public Restocker(string Builder)
		: this()
	{
		RequireWaresBuilder(Builder);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (NextRestockTick == 0L)
		{
			NextRestockTick = XRLCore.CurrentTurn + RestockFrequency + Stat.Random((int)RestockFrequency / 2, (int)RestockFrequency);
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (TurnNumber < NextRestockTick || IComponent<GameObject>.ThePlayer == null || !IComponent<GameObject>.ThePlayer.InSameZone(ParentObject))
		{
			return;
		}
		NextRestockTick = TurnNumber + RestockFrequency;
		if (ParentObject.IsPlayerControlled() || ParentObject.WasPlayer())
		{
			return;
		}
		Inventory inventory = ParentObject.Inventory;
		List<GameObject> list = Event.NewGameObjectList();
		list.AddRange(inventory.Objects);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject = list[i];
			if (gameObject.HasProperty("_stock") && !gameObject.HasPropertyOrTag("norestock") && !gameObject.HasPropertyOrTag("QuestItem"))
			{
				inventory.RemoveObject(gameObject);
				gameObject.Obliterate();
			}
		}
		foreach (string waresBuilder in WaresBuilders)
		{
			Type type = ModManager.ResolveType("XRL.World.Encounters.EncounterObjectBuilders." + waresBuilder);
			if (type != null)
			{
				object obj = Activator.CreateInstance(type);
				MethodInfo method = type.GetMethod("BuildObject");
				if (method != null && !(bool)method.Invoke(obj, new object[2] { ParentObject, "Restock" }))
				{
					return;
				}
			}
			Debug.Log(ParentObject.DebugName + " restocking " + waresBuilder);
		}
		if (IComponent<GameObject>.ThePlayer != null && IComponent<GameObject>.ThePlayer.InSameZone(ParentObject))
		{
			IComponent<GameObject>.AddPlayerMessage("{{G|" + ParentObject.A + ParentObject.BaseDisplayName + " " + ParentObject.GetVerb("have", PrependSpace: false) + " restocked " + ParentObject.its + " inventory!}}");
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void RequireWaresBuilder(string Builder)
	{
		if (!WaresBuilders.Contains(Builder))
		{
			WaresBuilders.Add(Builder);
		}
	}
}

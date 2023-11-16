using System;
using System.Collections.Generic;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class Leader : IPart
{
	public bool bCreated;

	public string popTable = "";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		SetupLeader(builtKnown: true);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			SetupLeader();
		}
		return base.FireEvent(E);
	}

	private void SetupLeader(bool builtKnown = false)
	{
		try
		{
			if (bCreated || (!builtKnown && !ParentObject.CurrentCell.ParentZone.Built))
			{
				return;
			}
			bCreated = true;
			_ = ParentObject.pPhysics;
			List<string> list = new List<string>();
			for (int i = 0; i < 12; i++)
			{
				foreach (PopulationResult item in PopulationManager.Generate(popTable))
				{
					list.Add(item.Blueprint);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				GameObject gameObject = GameObjectFactory.Factory.CreateObject(list[j]);
				(gameObject.GetPart("Brain") as Brain).PartyLeader = ParentObject;
				Cell connectedSpawnLocation = ParentObject.GetCurrentCell().GetConnectedSpawnLocation();
				if (connectedSpawnLocation != null)
				{
					XRLCore.Core.Game.ActionManager.AddActiveObject(gameObject);
					connectedSpawnLocation.AddObject(gameObject);
				}
			}
		}
		catch
		{
		}
	}
}

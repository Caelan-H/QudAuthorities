using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Clairvoyance : BaseMutation
{
	public bool RealityDistortionBased = true;

	public string Sound = "clairvoyance";

	public new Guid ActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	public List<VisCell> Cells = new List<VisCell>(64);

	public Clairvoyance()
	{
		DisplayName = "Clairvoyance";
		Type = "Mental";
	}

	public override void SaveData(SerializationWriter Writer)
	{
		Writer.Write(Cells.Count);
		foreach (VisCell cell in Cells)
		{
			Writer.Write(cell.C.ParentZone.ZoneID);
			Writer.Write(cell.C.X);
			Writer.Write(cell.C.Y);
			Writer.Write(cell.Turns);
		}
		base.SaveData(Writer);
	}

	public override void LoadData(SerializationReader Reader)
	{
		int num = Reader.ReadInt32();
		Cells.Clear();
		for (int i = 0; i < num; i++)
		{
			string zoneID = Reader.ReadString();
			int x = Reader.ReadInt32();
			int y = Reader.ReadInt32();
			int turns = Reader.ReadInt32();
			Cell cell = XRLCore.Core.Game.ZoneManager.GetZone(zoneID).GetCell(x, y);
			Cells.Add(new VisCell(cell, turns));
		}
		base.LoadData(Reader);
	}

	public override string GetDescription()
	{
		return "You briefly gain vision of a nearby area.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		if (Level < 15)
		{
			text = text + "Vision radius: {{rules|" + (3 + Level) + "}}\n";
		}
		if (Level >= 15)
		{
			text += "Vision radius: {{rules|whole map}}\n";
		}
		text = text + "Vision duration: {{rules|" + (Level + 19) + "}} rounds.\n";
		return text + "Cooldown: 100";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID && ID != EndTurnEvent.ID)
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("stars", 1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		foreach (VisCell cell in Cells)
		{
			cell.C.ParentZone.AddLight(cell.C.X, cell.C.Y, 0, LightLevel.Omniscient);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		List<VisCell> list = null;
		foreach (VisCell cell in Cells)
		{
			cell.Turns--;
			if (cell.Turns <= 0)
			{
				if (list == null)
				{
					list = new List<VisCell>();
				}
				list.Add(cell);
			}
		}
		if (list != null)
		{
			foreach (VisCell item in list)
			{
				Cells.Remove(item);
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandClairvoyance");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandClairvoyance")
		{
			if (RealityDistortionBased && !ParentObject.IsRealityDistortionUsable())
			{
				RealityStabilized.ShowGenericInterdictMessage(ParentObject);
				return true;
			}
			int radius = 3 + base.Level;
			List<Cell> list;
			if (base.Level < 15)
			{
				list = PickCircle(radius, 80, bLocked: false, AllowVis.Any);
				if (list == null)
				{
					return true;
				}
				if (list.Count == 0)
				{
					return true;
				}
				if (RealityDistortionBased)
				{
					Event e = Event.New("InitiateRealityDistortionTransit", "Object", ParentObject, "Mutation", this, "Cell", list[0]);
					if (!ParentObject.FireEvent(e, E) || !list[0].FireEvent(e, E))
					{
						return true;
					}
				}
			}
			else
			{
				list = ParentObject.CurrentCell.ParentZone.GetCells();
				if (RealityDistortionBased && !ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this)))
				{
					return true;
				}
			}
			IComponent<GameObject>.PlayUISound(Sound);
			CooldownMyActivatedAbility(ActivatedAbilityID, 101);
			UseEnergy(1000, "Mental Mutation");
			Predicate<Cell> predicate;
			if (RealityDistortionBased)
			{
				Event CheckAccess = Event.New("CheckRealityDistortionAccessibility");
				predicate = (Cell C) => C != null && !C.ParentZone.IsWorldMap() && C.FireEvent(CheckAccess);
			}
			else
			{
				predicate = (Cell C) => C != null && !C.ParentZone.IsWorldMap();
			}
			foreach (Cell item in list)
			{
				if (predicate(item))
				{
					Cells.Add(new VisCell(item, 19 + base.Level));
					item.SetExplored();
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Clairvoyance", "CommandClairvoyance", "Mental Mutation", null, "+", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

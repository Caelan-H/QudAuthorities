using System;
using ConsoleLib.Console;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class StrideMason : IPoweredPart
{
	public const int BP_COST_DIVISOR = 500;

	public string Source;

	public bool DynamicCharge;

	/// <summary>Copy the appearance of the sourced object, not just its blueprint.</summary>
	public bool Imitate;

	public string Blueprint = "Sandstone";

	public string DisplayName;

	public string Description;

	public Renderable Renderable;

	[NonSerialized]
	public int BlueprintChargeUse;

	public StrideMason()
	{
		ChargeUse = 10;
		WorksOnEquipper = true;
		Reset();
	}

	public void Reset()
	{
		BlueprintChargeUse = -1;
		DisplayName = null;
		Description = null;
		Renderable = null;
	}

	public bool IsReady(bool UseCharge = false)
	{
		if (Blueprint != null)
		{
			return IsReady(UseCharge, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, GetChargeUse(), UseChargeIfUnpowered: false, 0L);
		}
		return false;
	}

	public bool IsValidCell(Cell C)
	{
		if (!C.OnWorldMap())
		{
			return C.IsEmpty();
		}
		return false;
	}

	public bool IsValidWall(GameObject Wall)
	{
		if (Wall != null && Wall.Blueprint != Blueprint && Wall.IsWall() && !Wall.HasTag("Plant") && !Wall.HasTag("PlantLike") && !Wall.IsTemporary && Wall.pPhysics.IsReal)
		{
			return !Wall.HasPart("Forcefield");
		}
		return false;
	}

	public bool IsImitable(GameObject Wall)
	{
		if (Wall.pRender != null && !Wall.HasProperName && (Wall.HasIntProperty("ForceMutableSave") || !Wall.HasTag("Immutable")))
		{
			return !Wall.HasTagOrProperty("QuestItem");
		}
		return false;
	}

	public void ApplyRenderable(GameObject Wall)
	{
		Render pRender = Wall.pRender;
		pRender.DisplayName = DisplayName ?? pRender.DisplayName;
		pRender.Tile = Renderable.Tile ?? pRender.Tile;
		pRender.RenderString = Renderable.RenderString ?? pRender.RenderString;
		pRender.ColorString = Renderable.ColorString ?? pRender.ColorString;
		pRender.TileColor = Renderable.TileColor ?? pRender.TileColor;
		if (Renderable.DetailColor != 0)
		{
			pRender.DetailColor = Renderable.DetailColor.ToString();
		}
		if (Wall.GetPart("Description") is Description description)
		{
			description.Short = Description ?? description._Short;
		}
		if (Wall.HasTag("Immutable"))
		{
			Wall.SetIntProperty("ForceMutableSave", 1);
		}
	}

	public int GetChargeUse()
	{
		if (!DynamicCharge)
		{
			return ChargeUse;
		}
		if (BlueprintChargeUse == -1 && GameObjectFactory.Factory.Blueprints.TryGetValue(Blueprint, out var value))
		{
			BlueprintChargeUse = value.Stat("AV", 1) * value.Stat("Hitpoints", 1) / 500;
		}
		return Math.Max(ChargeUse, BlueprintChargeUse);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "LeftCell");
		if (Source == "Look")
		{
			E.Actor.RegisterPartEvent(this, "LookedAt");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "LeftCell");
		E.Actor.UnregisterPartEvent(this, "LookedAt");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LeftCell")
		{
			if (E.GetParameter("Cell") is Cell cell && IsValidCell(cell) && IsReady(UseCharge: true))
			{
				GameObject gameObject = GameObjectFactory.Factory.CreateObject(Blueprint);
				if (Imitate && Renderable != null)
				{
					ApplyRenderable(gameObject);
				}
				Phase.carryOver(ParentObject.Equipped, cell.AddObject(gameObject));
				cell.SetReachable(State: false);
			}
		}
		else if (E.ID == "LookedAt" && Source == "Look")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (IsValidWall(gameObjectParameter) && IsReady())
			{
				Reset();
				Blueprint = gameObjectParameter.Blueprint;
				if (Imitate && IsImitable(gameObjectParameter))
				{
					DisplayName = gameObjectParameter.pRender.DisplayName;
					Description = gameObjectParameter.GetPart<Description>()?._Short;
					Renderable = new Renderable(gameObjectParameter.pRender);
				}
			}
		}
		return base.FireEvent(E);
	}
}

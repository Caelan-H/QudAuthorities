using System;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class PluckablePolyp : IPart
{
	public bool Plucked;

	public long PluckTime;

	public long RegrowTime = 3600L;

	public int CacheChance = 500;

	public string RevealObject = "PolypCache";

	public string OldTile;

	public string OldDisplayName;

	[FieldSaveVersion(228)]
	public string PluckSounds = "pluck1,pluck2";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AutoexploreObjectEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (!Plucked && CanBePlucked() && E.Object?.pBrain != null && !E.Object.HasPropertyOrTag("Polypwalking"))
		{
			Pluck(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!Plucked && CanBePlucked())
		{
			E.Want = true;
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return Plucked;
	}

	public override bool WantTenTurnTick()
	{
		return Plucked;
	}

	public override bool WantHundredTurnTick()
	{
		return Plucked;
	}

	public override void TurnTick(long TurnNumber)
	{
		CheckUnpluck();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		CheckUnpluck();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		CheckUnpluck();
	}

	public void Pluck(GameObject who)
	{
		if (Plucked || !CanBePlucked())
		{
			return;
		}
		IComponent<GameObject>.XDidY(who, "pluck", "a coral polyp off the strut and" + who.GetVerb("toss") + " it aside");
		OldTile = ParentObject.pRender.Tile;
		OldDisplayName = ParentObject.pRender.DisplayName;
		Plucked = true;
		PluckTime = The.Game.Turns;
		if (!string.IsNullOrEmpty(PluckSounds) && ParentObject.IsAudible(IComponent<GameObject>.ThePlayer))
		{
			PlayWorldSound(PluckSounds.CachedCommaExpansion().GetRandomElement(), 0.3f);
		}
		if (Stat.Random(1, CacheChance) <= 1)
		{
			if (IComponent<GameObject>.Visible(who) && AutoAct.IsInterruptable())
			{
				AutoAct.Interrupt();
			}
			if (IComponent<GameObject>.Visible(who))
			{
				CombatJuice.playPrefabAnimation(who, "Particles/CoralCachePluck");
			}
			GameObject obj = ParentObject.CurrentCell.AddObject(RevealObject);
			IComponent<GameObject>.XDidYToZ(who, "reveal", obj, null, null, null, who, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, who.IsPlayer());
		}
		else if (IComponent<GameObject>.Visible(who))
		{
			CombatJuice.playPrefabAnimation(who, "Particles/CoralPluck");
		}
		ParentObject.pRender.ColorString = "&C";
		ParentObject.pRender.Tile = ParentObject.pRender.Tile.Replace("_both_", "_metal_");
		ParentObject.pRender.DisplayName = ParentObject.pRender.DisplayName.Replace(" with coral growth", "");
	}

	public void Unpluck()
	{
		ParentObject.pRender.Tile = OldTile;
		ParentObject.pRender.DisplayName = OldDisplayName;
		Plucked = false;
		ParentObject.CurrentCell.GetObjects("PolypCache").ForEach(delegate(GameObject o)
		{
			o.Obliterate();
		});
	}

	public void CheckUnpluck()
	{
		if (Plucked && The.Game.Turns - PluckTime >= RegrowTime)
		{
			Unpluck();
		}
	}

	public bool CanBePlucked()
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		if (currentZone.Z > 10)
		{
			return false;
		}
		Cell cell = currentZone.GetCell(0, 0);
		if (cell != null && cell.HasObject("NoPolypPluckingWidget"))
		{
			return false;
		}
		return true;
	}
}

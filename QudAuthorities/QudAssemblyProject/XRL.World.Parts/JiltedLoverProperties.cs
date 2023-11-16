using System;

namespace XRL.World.Parts;

[Serializable]
public class JiltedLoverProperties : IPart
{
	public string Color = "g";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID)
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone != null && ParentObject.hasid)
		{
			currentZone.FireEvent(Event.New("CheckStuck", "Invalidate", ParentObject.id));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		ZoneCheckStuck();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		ZoneCheckStuck();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		GameObject firstObjectWithTag = E.Cell.GetFirstObjectWithTag("Wall");
		string[] array = ParentObject.pRender.ColorString.Split('^');
		string text = ((array != null) ? array[0] : null);
		if (firstObjectWithTag != null)
		{
			ParentObject.pRender.ColorString = text + "^" + firstObjectWithTag.pRender.GetForegroundColor();
		}
		else
		{
			ParentObject.pRender.ColorString = text;
		}
		ParentObject.pRender.TileColor = ParentObject.pRender.ColorString;
		return base.HandleEvent(E);
	}

	public void ZoneCheckStuck()
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone != null && ParentObject.hasid)
		{
			currentZone.FireEvent(Event.New("CheckStuck"));
		}
	}
}

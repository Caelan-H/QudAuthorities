using System;

namespace XRL.World.Parts;

[Serializable]
public class Hangable : IPart
{
	public bool SameCellSupportSufficient = true;

	public int SupportPointsRequired = 2;

	public string HangingTile;

	public bool Hanging;

	[NonSerialized]
	private static Event eStartHanging = new ImmutableEvent("StartHanging");

	[NonSerialized]
	private static Event eStopHanging = new ImmutableEvent("StopHanging");

	public override bool SameAs(IPart p)
	{
		Hangable hangable = p as Hangable;
		if (hangable.SameCellSupportSufficient != SameCellSupportSufficient)
		{
			return false;
		}
		if (hangable.SupportPointsRequired != SupportPointsRequired)
		{
			return false;
		}
		if (hangable.HangingTile != HangingTile)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != GetDisplayNameEvent.ID && ID != LeftCellEvent.ID)
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		CheckHanging();
		if (Hanging)
		{
			E.AddTag("{{y|[hanging]}}");
		}
		else if (ParentObject.CurrentCell != null)
		{
			E.AddTag("{{y|[on ground]}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		CheckHanging();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		Hanging = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		CheckHanging();
		return base.HandleEvent(E);
	}

	public bool HangingSupportAvailable()
	{
		if (ParentObject.pPhysics == null)
		{
			return false;
		}
		Cell cell = ParentObject.pPhysics.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		int num = 0;
		if (cell.HasObjectWithPropertyOrTag("HangingSupport"))
		{
			if (SameCellSupportSufficient || SupportPointsRequired <= 1)
			{
				return true;
			}
			num++;
		}
		cell.ForeachCardinalAdjacentLocalCell(delegate(Cell OC)
		{
			if (OC.HasObjectWithIntProperty("Wall") || OC.HasObjectWithPropertyOrTag("HangingSupport"))
			{
				num++;
				if (num >= SupportPointsRequired)
				{
					return false;
				}
			}
			return true;
		});
		return num >= SupportPointsRequired;
	}

	public void CheckHanging()
	{
		bool hanging = Hanging;
		Hanging = HangingSupportAvailable();
		if (Hanging && !hanging)
		{
			ParentObject.FireEvent(eStartHanging);
		}
		else if (hanging && !Hanging)
		{
			ParentObject.FireEvent(eStopHanging);
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (Hanging && !string.IsNullOrEmpty(HangingTile))
		{
			E.Tile = HangingTile;
		}
		return base.Render(E);
	}
}

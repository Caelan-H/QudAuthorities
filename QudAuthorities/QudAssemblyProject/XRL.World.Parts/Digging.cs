using System;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Digging : IPart
{
	public const string SUPPORT_TYPE = "Digging";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Initialize()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Dig", "CommandDig", "Maneuvers", "Automatically dig from your present position to a destination location.", "â");
		base.Initialize();
	}

	public override void Remove()
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		base.Remove();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == NeedPartSupportEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(NeedPartSupportEvent E)
	{
		if (E.Type == "Digging" && !PartSupportEvent.Check(E, this))
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandDig");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandDig")
		{
			if (ParentObject.OnWorldMap())
			{
				ParentObject.Fail("You cannot dig on the world map.");
			}
			else
			{
				Cell cell = ParentObject.CurrentCell;
				Cell cell2 = PickDestinationCell(9999999, AllowVis.Any, Locked: false, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Dig to where?");
				if (cell2 != null && cell != null)
				{
					AutoAct.Setting = "d" + cell.X + "," + cell.Y + "," + cell2.X + "," + cell2.Y;
					ParentObject.ForfeitTurn();
				}
			}
		}
		return base.FireEvent(E);
	}
}

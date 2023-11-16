using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Serenity : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public Serenity()
	{
		DisplayName = "Serenity";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("travel", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandSerenity");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandSerenity")
		{
			string zoneID = "SerenityWorld.40.12.1.1.10";
			The.ZoneManager.SetActiveZone(zoneID);
			Cell cell = null;
			for (int num = 23; num >= 0; num--)
			{
				for (int num2 = 40; num2 >= 0; num2--)
				{
					Cell cell2 = The.ZoneManager.ActiveZone.GetCell(num2, num);
					if (cell2.IsReachable() && cell2.IsEmpty())
					{
						cell = cell2;
						break;
					}
					Cell cell3 = The.ZoneManager.ActiveZone.GetCell(40 - num2, num);
					if (cell3.IsReachable() && cell3.IsEmpty())
					{
						cell = cell3;
						break;
					}
					if (cell != null)
					{
						break;
					}
				}
				if (cell != null)
				{
					ParentObject.SystemMoveTo(cell);
					The.ZoneManager.ProcessGoToPartyLeader();
					CooldownMyActivatedAbility(ActivatedAbilityID, Math.Max(300 - 20 * base.Level, 5));
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
		ActivatedAbilityID = AddMyActivatedAbility("Serenity", "CommandSerenity", "Mental Mutation", null, "\u0017");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

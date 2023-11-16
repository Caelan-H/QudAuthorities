using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class FrostWebs : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public FrostWebs()
	{
		DisplayName = "Frost Webs";
		Type = "Physical";
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
		E.Add("ice", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "ApplyStuck");
		Object.RegisterPartEvent(this, "CommandFrostWebs");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You fill a nearby area with frosty webs.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("" + "Cooldown: 30 rounds\n", "Range: 12\n"), "Area: 3x3\n");
	}

	public void FrostWeb(List<Cell> Cells)
	{
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		foreach (Cell Cell in Cells)
		{
			if (80.in100())
			{
				GameObject gameObject = GameObject.create("FrostWeb");
				Cell.AddObject(gameObject);
				gameObject.DotPuff("&C");
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyStuck")
		{
			return false;
		}
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 12 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandFrostWebs");
			}
		}
		else if (E.ID == "CommandFrostWebs")
		{
			List<Cell> list = PickCircle(1, 12, bLocked: false, AllowVis.OnlyVisible);
			if (list == null)
			{
				return false;
			}
			foreach (Cell item in list)
			{
				if (item.DistanceTo(ParentObject) > 13)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("That is out of range! (12 squares)");
					}
					return false;
				}
			}
			if (list == null)
			{
				return false;
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, 30);
			UseEnergy(1000, "Physical Mutation Frost Webs");
			FrostWeb(list);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Knit Frosty Webs", "CommandFrostWebs", "Mental Mutation", null, "#");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

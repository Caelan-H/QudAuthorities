using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Invisibility : BaseMutation
{
	public Invisibility()
	{
		DisplayName = "Invisibility";
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
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
		E.Add("glass", 1);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "You cannot be seen.";
	}

	public override string GetLevelText(int Level)
	{
		return GetDescription();
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CustomRender");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CustomRender")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell == null || cell.CurrentLightLevel == LightLevel.Darkvision || cell.CurrentLightLevel == LightLevel.Dimvision || cell.CurrentLightLevel == LightLevel.Interpolight || cell.CurrentLightLevel == LightLevel.Omniscient)
			{
				ParentObject.pRender.Visible = true;
			}
			else
			{
				ParentObject.pRender.Visible = false;
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
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}

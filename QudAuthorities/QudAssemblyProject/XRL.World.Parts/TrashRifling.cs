using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class TrashRifling : IPart
{
	public const string SUPPORT_TYPE = "TrashRifling";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Initialize()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Rifle through Trash", "CommandToggleTrashRifling", "Skill", "Toggle to enable or disable rifling through trash.", "%", null, Toggleable: true, DefaultToggleState: true);
		base.Initialize();
	}

	public override void Remove()
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		base.Remove();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIBoredEvent.ID && ID != CommandEvent.ID && ID != AutoexploreObjectEvent.ID)
		{
			return ID == NeedPartSupportEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && ParentObject.pBrain != null && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			List<GameObject> list = Event.NewGameObjectList();
			foreach (GameObject item in cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, 8, "Garbage", ParentObject))
			{
				if (ParentObject.HasLOSTo(item))
				{
					list.Add(item);
				}
			}
			if (list.Count > 1)
			{
				list.Sort((GameObject a, GameObject b) => ParentObject.DistanceTo(a).CompareTo(ParentObject.DistanceTo(b)));
			}
			GameObject randomElement = list.GetRandomElement();
			if (randomElement != null)
			{
				Cell cell2 = randomElement.CurrentCell;
				if (cell2 != null)
				{
					ParentObject.pBrain.PushGoal(new MoveTo(cell2, careful: false, overridesCombat: false, 0, wandering: false, global: false, juggernaut: false, 3));
				}
			}
			else
			{
				ParentObject.pBrain.Think("I can't find any trash.");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandToggleTrashRifling")
		{
			ToggleMyActivatedAbility(ActivatedAbilityID);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(NeedPartSupportEvent E)
	{
		if (E.Type == "TrashRifling" && !PartSupportEvent.Check(E, this))
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if ((!E.Want || E.FromAdjacent == null) && E.Item.HasPart("Garbage") && Options.AutoexploreAutopickups && IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			E.Want = true;
			E.FromAdjacent = "RifleThroughGarbage";
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}

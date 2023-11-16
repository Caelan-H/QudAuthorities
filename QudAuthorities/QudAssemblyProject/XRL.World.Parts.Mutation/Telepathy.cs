using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Telepathy : BaseMutation
{
	public bool RealityDistortionBased;

	public new Guid ActivatedAbilityID = Guid.Empty;

	public Telepathy()
	{
		DisplayName = "Telepathy";
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandTelepathy");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return string.Concat("" + "You may communicate with others through the psychic aether.\n\n", "Chat with anyone in vision\nTakes you much less time to issue orders to companions");
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandTelepathy")
		{
			if (RealityDistortionBased && !ParentObject.IsRealityDistortionUsable())
			{
				RealityStabilized.ShowGenericInterdictMessage(ParentObject);
				return false;
			}
			Cell cell = PickDestinationCell(80, AllowVis.OnlyExplored);
			if (cell != null)
			{
				if (RealityDistortionBased)
				{
					Event e = Event.New("InitiateRealityDistortionTransit", "Object", ParentObject, "Mutation", this, "Cell", cell);
					if (!ParentObject.FireEvent(e, E) || !cell.FireEvent(e, E))
					{
						return false;
					}
				}
				cell.ForeachObjectWithPart("ConversationScript", delegate(GameObject GO)
				{
					GO.GetPart<ConversationScript>().AttemptConversation(Silent: false, true);
				});
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
		ActivatedAbilityID = AddMyActivatedAbility("Telepathy", "CommandTelepathy", "Mental Mutation", null, "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

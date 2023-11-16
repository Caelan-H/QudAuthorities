using System.Collections.Generic;
using System.Linq;
using Qud.API;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.QuestManagers;

namespace XRL.World.Conversations.Parts;

public class PaxInfectLimb : IConversationPart
{
	public bool IfQuestActive;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != IsElementVisibleEvent.ID && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnterElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsElementVisibleEvent E)
	{
		if (IfQuestActive)
		{
			if (The.Player.HasObjectEquipped("PaxInfection"))
			{
				return false;
			}
			Quest quest = The.Game.Quests.Values.FirstOrDefault((Quest x) => x.Manager is SpreadPax);
			if (quest == null || The.Game.HasFinishedQuest(quest.ID))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{g|[Select limb to infect]}}";
		return false;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		List<BodyPart> parts = The.Player.Body.GetParts();
		if (!FungalSporeInfection.ChooseLimbForInfection(parts, "Klanq", out var Target, out var Name))
		{
			return false;
		}
		if (!InfectLimb(parts, Target, Name) || Target.Equipped?.Blueprint != "PaxInfection")
		{
			Popup.Show("The limb rejects the infection!");
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool InfectLimb(List<BodyPart> Parts, BodyPart Target, string Name)
	{
		if (Target.Equipped != null && !Target.TryUnequip())
		{
			return false;
		}
		GameObject gameObject = GameObjectFactory.Factory.CreateObject("PaxInfection");
		gameObject.GetPart<Armor>().AV = ((!(Target.Type == "Body")) ? 1 : 3);
		if (Target.Type == "Hand")
		{
			MeleeWeapon part = gameObject.GetPart<MeleeWeapon>();
			part.BaseDamage = "1d4";
			part.Skill = "Cudgel";
			part.PenBonus = 0;
			part.MaxStrengthBonus = 4;
		}
		if (Target.SupportsDependent != null)
		{
			foreach (BodyPart Part in Parts)
			{
				if (Part != Target && !(Part.DependsOn != Target.SupportsDependent) && FungalSporeInfection.BodyPartSuitableForFungalInfection(Part))
				{
					gameObject.UsesSlots = Target.Type + "," + Part.Type;
					break;
				}
			}
		}
		if (!Target.Equip(gameObject, 0, Silent: true, ForDeepCopy: false, Forced: false, SemiForced: true))
		{
			gameObject.Destroy();
			return false;
		}
		JournalAPI.AddAccomplishment("You contracted " + gameObject.DisplayNameOnly + " on your " + Name + ", endearing " + The.Player.itself + " to fungi across Qud.", "In a show of unprecedented solidarity with fungi, =name= deigned to contract " + gameObject.DisplayNameOnly + " on " + The.Player.GetPronounProvider().PossessiveAdjective + " " + Name + ".", "general", JournalAccomplishment.MuralCategory.BodyExperienceNeutral, JournalAccomplishment.MuralWeight.Medium, null, -1L);
		Popup.Show("You've contracted " + gameObject.DisplayNameOnly + " on your " + Name + ".");
		return true;
	}
}

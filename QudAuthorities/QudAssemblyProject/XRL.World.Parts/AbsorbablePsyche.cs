using System;
using Qud.API;
using XRL.Language;
using XRL.UI;
using XRL.World.Encounters;

namespace XRL.World.Parts;

[Serializable]
public class AbsorbablePsyche : IPart
{
	public static readonly int ABSORB_CHANCE = 10;

	public int EgoBonus = 1;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != KilledEvent.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(KilledEvent E)
	{
		if (E.Dying.GetPsychicGlimmer() >= PsychicManager.GLIMMER_FLOOR)
		{
			if (ParentObject.GetPrimaryFaction() == "Seekers")
			{
				E.Reason = "You were resorbed into the Mass Mind.";
				E.ThirdPersonReason = E.Dying.It + E.Dying.GetVerb("were") + " @@resorbed into the Mass Mind.";
			}
			else if (ABSORB_CHANCE.in100())
			{
				E.Reason = "Your psyche exploded, and its psionic bits were encoded on the holographic boundary surrounding the psyche of " + Grammar.MakePossessive(ParentObject.BaseDisplayName) + ".";
				E.ThirdPersonReason = E.Dying.Its + " psyche exploded, and its psionic bits were encoded on the holographic boundary surrounding the psyche of " + Grammar.MakePossessive(ParentObject.BaseDisplayName) + ".";
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (EgoBonus > 0)
		{
			GameObject killer = E.Killer;
			if (killer != null && killer.IsPlayer())
			{
				int egoBonus = EgoBonus;
				EgoBonus = 0;
				if (ABSORB_CHANCE.in100())
				{
					if (Popup.ShowYesNo("At the moment of victory, your swelling ego curves the psychic aether and causes the psyche of " + ParentObject.BaseDisplayName + " to collide with your own. As the weaker of the two, its binding energy is exceeded and it explodes. Would you like to encode its psionic bits on the holographic boundary of your own psyche?\n\n(+1 Ego permanently)") == DialogResult.Yes)
					{
						IComponent<GameObject>.ThePlayer.GetStat("Ego").BaseValue += egoBonus;
						Popup.Show("You encode the psyche of " + ParentObject.BaseDisplayName + " and gain +{{C|1}} {{Y|Ego}}!");
						JournalAPI.AddAccomplishment("You slew " + ParentObject.BaseDisplayName + " and encoded their psyche's psionic bits on the holographic boundary of your own psyche.", "After a climactic battle of wills, =name= slew " + ParentObject.the + ParentObject.BaseDisplayName + " and absorbed " + ParentObject.its + " psyche, thickening toward Godhood.", "general", JournalAccomplishment.MuralCategory.Slays, JournalAccomplishment.MuralWeight.High, null, -1L);
						AchievementManager.SetAchievement("ACH_ABSORB_PSYCHE");
					}
					else
					{
						Popup.Show("You pause as the psyche of " + ParentObject.BaseDisplayName + " radiates into nothingness.");
						JournalAPI.AddAccomplishment("You slew " + ParentObject.BaseDisplayName + " and watched their psyche radiate into nothingness.", "After a climactic battle of wills, =name= slew " + ParentObject.the + ParentObject.BaseDisplayName + " and watched " + ParentObject.its + " psyche radiate into nothingness.", "general", JournalAccomplishment.MuralCategory.Slays, JournalAccomplishment.MuralWeight.Medium, null, -1L);
					}
				}
				else
				{
					JournalAPI.AddAccomplishment("You slew " + ParentObject.BaseDisplayName + ".", "After a climactic battle of wills, =name= slew " + ParentObject.the + ParentObject.BaseDisplayName + ".", "general", JournalAccomplishment.MuralCategory.Slays, JournalAccomplishment.MuralWeight.Medium, null, -1L);
				}
			}
		}
		return base.HandleEvent(E);
	}
}

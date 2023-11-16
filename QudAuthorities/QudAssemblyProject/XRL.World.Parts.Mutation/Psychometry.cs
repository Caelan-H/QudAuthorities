using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.UI;
using XRL.World.Tinkering;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Psychometry : BaseMutation
{
	public bool RealityDistortionBased = true;

	public new Guid ActivatedAbilityID = Guid.Empty;

	public List<string> LearnedBlueprints;

	public Psychometry()
	{
		DisplayName = "Psychometry";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetItemElementsEvent.ID && ID != GetRitualSifrahSetupEvent.ID && ID != GetTinkeringBonusEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == OwnerGetInventoryActionsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTinkeringBonusEvent E)
	{
		if (E.Type == "Examine" || E.Type == "ReverseEngineerTurns" || E.Type == "Hacking")
		{
			if (UsePsychometry(E.Actor))
			{
				E.Bonus++;
				E.PsychometryApplied = true;
			}
			else
			{
				if (E.Actor.IsInGraveyard() || E.Actor.HasEffect("Stun"))
				{
					return false;
				}
				if (Popup.ShowYesNo("Do you want to continue despite being unable to use Psychometry?") != 0)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetRitualSifrahSetupEvent E)
	{
		if (E.Type == "Item Naming")
		{
			if (UsePsychometry(E.Actor))
			{
				E.PsychometryApplied = true;
			}
			else if (E.Actor.IsInGraveyard() || E.Actor.HasEffect("Stun"))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
	{
		if (E.Object.GetEpistemicStatus() != 2)
		{
			if (E.Actor.GetConfusion() <= 0 && !Options.SifrahExamine && E.Object.GetPart("Examiner") is Examiner examiner && examiner.Complexity > 0)
			{
				E.AddAction("Psychometry", "read history with Psychometry", "Psychometry", null, 'i', FireOnActor: true);
			}
		}
		else if (ParentObject.HasPart("Tinkering"))
		{
			string text = (E.Object.GetPart("TinkerItem") as TinkerItem)?.ActiveBlueprint ?? E.Object.Blueprint;
			if (!LearnedBlueprints.Contains(text))
			{
				bool flag = false;
				foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
				{
					if (tinkerRecipe.Blueprint == text)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					flag = false;
					foreach (TinkerData knownRecipe in TinkerData.KnownRecipes)
					{
						if (knownRecipe.Blueprint == text)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						E.AddAction("Psychometry", "read early history with Psychometry", "Psychometry", null, 'i', FireOnActor: true);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Psychometry")
		{
			if (!Activate(E.Item))
			{
				return true;
			}
			Examiner examiner = E.Item.GetPart("Examiner") as Examiner;
			TinkerItem tinkerItem = E.Item.GetPart("TinkerItem") as TinkerItem;
			if (!E.Item.Understood())
			{
				if (Options.SifrahExamine)
				{
					InventoryActionEvent.Check(E.Item, E.Actor, E.Item, "Examine");
					return true;
				}
				if (examiner.Complexity > GetIdentifiableComplexity())
				{
					Popup.ShowFail(E.Item.IndicativeProximal + (E.Item.IsPlural ? " artifacts" : " artifact") + E.Item.Is + " too complex for you to decipher " + E.Item.its + " function.");
					return true;
				}
				examiner.MakeUnderstood();
				Popup.ShowFail("You flush with understanding of the " + (E.Item.IsPlural ? "artifacts'" : "artifact's") + " past and determine " + E.Item.them + " to be " + E.Item.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				UseEnergy(1000, "Mental Mutation");
			}
			else
			{
				if (Options.SifrahReverseEngineer && tinkerItem != null && tinkerItem.CanDisassemble)
				{
					if (E.Actor.HasSkill("Tinkering_ReverseEngineer"))
					{
						Popup.ShowFail("You must disassemble " + E.Item.t() + " in order to unlock " + E.Item.its + " secrets.");
					}
					else
					{
						Popup.ShowFail("You must learn the way of the Reverse Engineer and disassemble " + E.Item.t() + " in order to unlock " + E.Item.its + " secrets.");
					}
					return true;
				}
				if (examiner != null && examiner.Complexity > GetLearnableComplexity())
				{
					Popup.ShowFail(E.Item.IndicativeProximal + (E.Item.IsPlural ? " artifacts" : " artifact") + E.Item.Is + " too complex for you to decipher " + E.Item.its + " method of construction.");
					return true;
				}
				string text = tinkerItem?.ActiveBlueprint ?? E.Item.Blueprint;
				LearnedBlueprints.Add(text);
				foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
				{
					if (tinkerRecipe.Blueprint == text)
					{
						GameObject gameObject = GameObject.createSample(tinkerRecipe.Blueprint);
						gameObject.MakeUnderstood();
						try
						{
							tinkerRecipe.DisplayName = gameObject.DisplayNameOnlyDirect;
							Popup.Show("You abide the memory of " + ((E.Item.Count > 1) ? E.Item.a : E.Item.the) + Grammar.MakePossessive(E.Item.ShortDisplayNameSingle) + " creation. You learn to build " + (gameObject.IsPlural ? gameObject.ShortDisplayNameSingle : Grammar.Pluralize(gameObject.ShortDisplayNameSingle)) + ".");
							TinkerData.KnownRecipes.Add(tinkerRecipe);
						}
						finally
						{
							gameObject.Obliterate();
						}
						return true;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("scholarship", 1);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandPsychometryMenu");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandPsychometryMenu")
		{
			Popup.ShowFail("Select an artifact and select 'psychometry' from the popup menu to use Psychometry.");
		}
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		return "You read the history of artifacts by touching them, learning what they do and how they were made.";
	}

	public static int GetIdentifiableComplexity(int Level)
	{
		return 4 + Level / 2;
	}

	public int GetIdentifiableComplexity()
	{
		return GetIdentifiableComplexity(base.Level);
	}

	public static int GetLearnableComplexity(int Level)
	{
		return 2 + (Level - 1) / 2;
	}

	public int GetLearnableComplexity()
	{
		return GetLearnableComplexity(base.Level);
	}

	public override string GetLevelText(int Level)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (Options.SifrahExamine)
		{
			stringBuilder.Compound("Grants an extra turn and a non-resource-consuming option to use when examining and reverse engineering artifacts.");
		}
		else
		{
			stringBuilder.Compound("Unerringly identify artifacts up to complexity tier {{rules|", "\n").Append(GetIdentifiableComplexity(Level)).Append("}}.");
		}
		stringBuilder.Compound("Learn how to construct identified artifacts up to complexity tier {{rules|", "\n").Append(GetLearnableComplexity(Level)).Append("}} (must have the appropriate Tinker power).");
		stringBuilder.Compound("You may open security doors by touching them.", "\n");
		return stringBuilder.ToString();
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = GO.AddActivatedAbility("Psychometry", "CommandPsychometryMenu", "Mental Mutation", null, "\a", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		if (LearnedBlueprints == null)
		{
			LearnedBlueprints = new List<string>();
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}

	public bool Advisable(GameObject obj)
	{
		if (!GameObject.validate(ref obj))
		{
			return false;
		}
		if (!RealityDistortionBased)
		{
			return true;
		}
		if (!ParentObject.FireEvent("CheckRealityDistortionAdvisability"))
		{
			return false;
		}
		if (!(obj.GetCurrentCell() ?? ParentObject.GetCurrentCell()).FireEvent("CheckRealityDistortionAdvisability"))
		{
			return false;
		}
		return true;
	}

	public bool Activate(GameObject obj)
	{
		if (!GameObject.validate(ref obj))
		{
			return false;
		}
		if (ParentObject.GetConfusion() > 0)
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("You strain to part the veil of time in order to use psychometry on " + obj.t() + ", but you are too confused.");
			}
			return false;
		}
		if (!IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID))
		{
			if (ParentObject.IsPlayer() && IsMyActivatedAbilityCoolingDown(ActivatedAbilityID))
			{
				Popup.ShowFail("You strain to part the veil of time in order to use psychometry on " + obj.t() + ", but your psyche is too exhausted.");
			}
			return false;
		}
		if (!RealityDistortionBased)
		{
			return true;
		}
		Cell cell = obj.GetCurrentCell() ?? ParentObject.GetCurrentCell();
		Event @event = Event.New("InitiateRealityDistortionTransit");
		@event.SetParameter("Object", ParentObject);
		@event.SetParameter("Mutation", this);
		@event.SetParameter("Cell", cell);
		@event.SetParameter("Purpose", "use psychometry on " + obj.t());
		if (!ParentObject.FireEvent(@event))
		{
			return false;
		}
		if (!cell.FireEvent(@event))
		{
			return false;
		}
		return true;
	}
}

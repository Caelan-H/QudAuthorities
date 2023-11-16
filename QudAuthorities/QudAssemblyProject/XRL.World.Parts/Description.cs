using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Description : IPart
{
	public string Mark = "";

	public string _Short = "A really ugly specimen.";

	[NonSerialized]
	private List<string> FeatureItems = new List<string>();

	[NonSerialized]
	private List<string> EquipItems = new List<string>();

	public string Short
	{
		get
		{
			return GetShortDescription();
		}
		set
		{
			if (value.Contains("~"))
			{
				_Short = value.Split('~')[0];
			}
			else
			{
				_Short = value;
			}
		}
	}

	public string Long
	{
		get
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			GetLongDescription(stringBuilder);
			return stringBuilder.ToString();
		}
	}

	public string GetShortDescription(bool AsIfKnown = false, bool NoConfusion = false, string Context = null)
	{
		if (!NoConfusion && The.Core.ConfusionLevel > 0)
		{
			return "???";
		}
		IShortDescriptionEvent shortDescriptionEvent = null;
		IShortDescriptionEvent oE = null;
		switch (AsIfKnown ? 2 : ParentObject.GetEpistemicStatus())
		{
		case 0:
			shortDescriptionEvent = GetUnknownShortDescriptionEvent.FromPool(ParentObject, ParentObject.GetPart<Examiner>()?.UnknownDescription, Context, AsIfKnown);
			if (IComponent<GameObject>.ThePlayer != null)
			{
				oE = OwnerGetUnknownShortDescriptionEvent.FromPool();
			}
			break;
		case 1:
			shortDescriptionEvent = GetUnknownShortDescriptionEvent.FromPool(ParentObject, ParentObject.GetPart<Examiner>()?.AlternateDescription, Context, AsIfKnown);
			if (IComponent<GameObject>.ThePlayer != null)
			{
				oE = OwnerGetUnknownShortDescriptionEvent.FromPool();
			}
			break;
		default:
			shortDescriptionEvent = GetShortDescriptionEvent.FromPool(ParentObject, _Short, Context, AsIfKnown);
			if (IComponent<GameObject>.ThePlayer != null)
			{
				oE = OwnerGetShortDescriptionEvent.FromPool();
			}
			break;
		}
		return GetDescription(shortDescriptionEvent, oE);
	}

	private string GetDescription(IShortDescriptionEvent E, IShortDescriptionEvent OE)
	{
		E.Process(ParentObject);
		if (OE != null && IComponent<GameObject>.ThePlayer != null)
		{
			OE.Process(IComponent<GameObject>.ThePlayer, E);
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (E.Prefix.Length > 0)
		{
			stringBuilder.Append(E.Prefix);
			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '\n')
			{
				stringBuilder.Append('\n');
			}
		}
		stringBuilder.Append(E.Base);
		if (E.Infix.Length > 0)
		{
			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '\n')
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append(E.Infix);
		}
		if (E.Postfix.Length > 0)
		{
			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '\n')
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append(E.Postfix);
		}
		if (!string.IsNullOrEmpty(Mark))
		{
			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '\n')
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append('\n').Append(Mark);
		}
		if (ParentObject.pPhysics != null && ParentObject.pPhysics.Takeable)
		{
			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '\n')
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append("\n{{K|Weight: ").Append(ParentObject.Weight).Append(" lbs.}}");
		}
		return GameText.VariableReplace(stringBuilder, ParentObject);
	}

	public void GetLongDescription(StringBuilder SB)
	{
		SB.Append(Short);
		if (!ParentObject.HasProperty("HideCon"))
		{
			Body body = ParentObject.Body;
			if (body != null)
			{
				FeatureItems.Clear();
				EquipItems.Clear();
				List<GameObject> list = Event.NewGameObjectList();
				_ = SB.Length;
				foreach (BodyPart item in body.LoopParts())
				{
					if (item.Equipped != null && !item.Equipped.HasPropertyOrTag("SuppressInLookDisplay") && !list.Contains(item.Equipped))
					{
						if (item.Equipped.HasPropertyOrTag("ShowAsPhysicalFeature") || (item.Equipped.HasPropertyOrTag("VisibleAsDefaultBehavior") && !item.Equipped.HasPropertyOrTag("UndesireableWeapon")))
						{
							FeatureItems.Add(item.Equipped.ShortDisplayName);
						}
						else
						{
							EquipItems.Add(item.Equipped.ShortDisplayName);
						}
						list.Add(item.Equipped);
					}
					if (item.Cybernetics != null && !item.Cybernetics.HasPropertyOrTag("SuppressInLookDisplay") && !list.Contains(item.Cybernetics))
					{
						EquipItems.Add(item.Cybernetics.ShortDisplayName);
						list.Add(item.Cybernetics);
					}
					if (item.DefaultBehavior != null && !item.DefaultBehavior.HasPropertyOrTag("SuppressInLookDisplay") && !list.Contains(item.DefaultBehavior) && (item.DefaultBehavior.HasPropertyOrTag("ShowAsPhysicalFeature") || (item.DefaultBehavior.HasPropertyOrTag("VisibleAsDefaultBehavior") && !item.DefaultBehavior.HasPropertyOrTag("UndesireableWeapon"))))
					{
						FeatureItems.Add(item.DefaultBehavior.ShortDisplayName);
						list.Add(item.DefaultBehavior);
					}
				}
				GetExtraPhysicalFeaturesEvent.Send(ParentObject, FeatureItems);
				if (FeatureItems.Count > 0)
				{
					SB.Append("\n\nPhysical features: ").Append(FeatureItems[0]);
					int i = 1;
					for (int count = FeatureItems.Count; i < count; i++)
					{
						SB.Append(", ").Append(FeatureItems[i]);
					}
				}
				if (EquipItems.Count > 0)
				{
					if (FeatureItems.Count <= 0)
					{
						SB.Append('\n');
					}
					SB.Append("\nEquipped: ").Append(EquipItems[0]);
					int j = 1;
					for (int count2 = EquipItems.Count; j < count2; j++)
					{
						SB.Append(", ").Append(EquipItems[j]);
					}
				}
			}
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (ParentObject.Effects != null)
		{
			foreach (Effect effect in ParentObject.Effects)
			{
				if (effect.SuppressInLookDisplay())
				{
					continue;
				}
				string description = effect.GetDescription();
				if (!string.IsNullOrEmpty(description))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(description);
				}
			}
		}
		ParentObject.FireEvent(Event.New("GetEffectsBlock", "Block", stringBuilder));
		if (stringBuilder.Length > 0)
		{
			SB.Append("\n\n").Append(stringBuilder);
		}
	}

	public string GetFeelingDescription(GameObject who = null)
	{
		if (who == null)
		{
			who = IComponent<GameObject>.ThePlayer;
			if (who == null)
			{
				return null;
			}
		}
		if (ParentObject.HasProperty("HideCon"))
		{
			return null;
		}
		Brain pBrain = ParentObject.pBrain;
		if (pBrain == null)
		{
			return null;
		}
		return pBrain.GetOpinion(who) switch
		{
			Brain.CreatureOpinion.allied => "{{G|Friendly}}", 
			Brain.CreatureOpinion.hostile => "{{R|Hostile}}", 
			_ => "Neutral", 
		};
	}

	public string GetDifficultyDescription(GameObject who = null)
	{
		return DifficultyEvaluation.GetDifficultyDescription(ParentObject, who);
	}

	public override bool SameAs(IPart p)
	{
		Description description = p as Description;
		if (description.Mark != Mark)
		{
			return false;
		}
		if (description._Short != _Short)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Look", "look", "Look", null, 'l', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true);
		if (ParentObject.Understood() && ParentObject.HasStringProperty("Story"))
		{
			E.AddAction("Recall Story", "recall story", "ReadStory", null, 's', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Look")
		{
			StringBuilder stringBuilder = Event.NewStringBuilder().Append(ParentObject.DisplayName).Append("\n\n")
				.Append(Long)
				.Append("\n\n")
				.Append(Strings.WoundLevel(ParentObject));
			string difficultyDescription = GetDifficultyDescription(E.Actor);
			if (!string.IsNullOrEmpty(difficultyDescription))
			{
				stringBuilder.Append('\n').Append(difficultyDescription);
			}
			string feelingDescription = GetFeelingDescription(E.Actor);
			if (!string.IsNullOrEmpty(feelingDescription))
			{
				stringBuilder.Append(string.IsNullOrEmpty(difficultyDescription) ? "\n" : ", ").Append(feelingDescription);
			}
			if (ParentObject.HasStringProperty("Story") && ParentObject.Understood())
			{
				if (Popup.ShowBlock(stringBuilder.ToString(), "[press {{W|space}} or recall {{W|s}}tory]", Capitalize: false, MuteBackground: true, ParentObject.RenderForUI(), centerIcon: false, rightIcon: true, LogMessage: false) == Keys.S)
				{
					BookUI.ShowBook(ParentObject.Property["Story"]);
				}
			}
			else
			{
				Popup.ShowBlockSpace(stringBuilder.ToString(), "[press {{W|space}}]", Capitalize: false, MuteBackground: true, ParentObject.RenderForUI(), centerIcon: false, rightIcon: true, LogMessage: false);
			}
			ParentObject.FireEvent(Event.New("AfterLookedAt", "Looker", E.Actor));
		}
		else if (E.Command == "ReadStory")
		{
			BookUI.ShowBook(ParentObject.GetStringProperty("Story"));
		}
		return base.HandleEvent(E);
	}
}

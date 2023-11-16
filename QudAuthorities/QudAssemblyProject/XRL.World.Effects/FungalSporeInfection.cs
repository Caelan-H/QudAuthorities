using System;
using System.Collections.Generic;
using System.Linq;
using Qud.API;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class FungalSporeInfection : Effect
{
	public string InfectionObject = "LuminousInfection";

	public bool Fake;

	public int TurnsLeft;

	public int Damage = 2;

	public GameObject Owner;

	public bool bSpawned;

	public FungalSporeInfection()
	{
		base.DisplayName = "{{w|itchy skin}}";
		base.Duration = 1;
	}

	public FungalSporeInfection(int Duration, string Infection)
		: this()
	{
		TurnsLeft = Duration;
		InfectionObject = Infection;
	}

	public override int GetEffectType()
	{
		return 117456900;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Skin flakes and itches. Is something growing there?";
	}

	public override bool Apply(GameObject Object)
	{
		return Object.FireEvent("ApplyFungalSporeInfection");
	}

	public override void Remove(GameObject Object)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GeneralAmnestyEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Owner = null;
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ApplyFungalSporeInfection");
		Object.RegisterEffectEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "ApplyFungalSporeInfection");
		Object.UnregisterEffectEvent(this, "EndTurn");
		base.Unregister(Object);
	}

	public static bool BodyPartSuitableForFungalInfection(BodyPart Part)
	{
		if (Part.Abstract)
		{
			return false;
		}
		if (Part.Extrinsic)
		{
			return false;
		}
		if (!Part.Contact)
		{
			return false;
		}
		if (Part.Category != 1 && Part.Category != 2 && Part.Category != 3 && Part.Category != 4 && Part.Category != 5 && Part.Category != 9 && Part.Category != 12 && Part.Category != 13 && Part.Category != 14 && Part.Category != 16)
		{
			return false;
		}
		if (Part.Equipped != null)
		{
			if (Part.Equipped == Part.Cybernetics)
			{
				return false;
			}
			if (!Part.Equipped.FireEvent(Event.New("CanBeUnequipped", "SemiForced", 1)))
			{
				return false;
			}
		}
		return true;
	}

	public static bool BodyPartPreferableForFungalInfection(BodyPart Part)
	{
		if (!BodyPartSuitableForFungalInfection(Part))
		{
			return false;
		}
		return Part.Type == "Fungal Outcrop";
	}

	public static bool ChooseLimbForInfection(string FungusName, out BodyPart Target, out string Name)
	{
		return ChooseLimbForInfection(The.Player.Body.GetParts(), FungusName, out Target, out Name);
	}

	public static bool ChooseLimbForInfection(List<BodyPart> Parts, string FungusName, out BodyPart Target, out string Name)
	{
		Target = null;
		Name = null;
		List<BodyPart> infectable = Parts.Where(BodyPartPreferableForFungalInfection).ToList();
		infectable.AddRange(Parts.Where((BodyPart x) => !infectable.Contains(x) && BodyPartSuitableForFungalInfection(x)));
		if (infectable.Count == 0)
		{
			Popup.Show("You have no infectable body parts.");
			return false;
		}
		string[] array = infectable.Select((BodyPart x) => x.GetOrdinalName()).ToArray();
		int num = Popup.ShowOptionList("Choose a limb to infect with " + FungusName + ".", array, null, 1, null, 75, RespectOptionNewlines: false, AllowEscape: true);
		if (num < 0)
		{
			return false;
		}
		Target = infectable[num];
		Name = array[num];
		return true;
	}

	public static bool ApplyFungalInfection(GameObject Object, string InfectionBlueprint, BodyPart SelectedPart = null)
	{
		if (Object.HasTagOrProperty("ImmuneToFungus"))
		{
			return false;
		}
		if (InfectionBlueprint == "PaxInfection" && (Object.IsPlayer() || Object.HasIntProperty("HasPax")))
		{
			return true;
		}
		Body body = Object.Body;
		if (body == null)
		{
			return true;
		}
		List<BodyPart> list;
		if (SelectedPart == null)
		{
			list = body.GetParts();
			list.ShuffleInPlace();
		}
		else
		{
			list = new List<BodyPart>();
			list.Add(SelectedPart);
		}
		BodyPart bodyPart = null;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (BodyPartPreferableForFungalInfection(list[i]) && (list[i].Equipped == null || list[i].TryUnequip(Silent: false, SemiForced: true)))
			{
				bodyPart = list[i];
				break;
			}
		}
		if (bodyPart == null)
		{
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				if (BodyPartSuitableForFungalInfection(list[j]) && (list[j].Equipped == null || list[j].TryUnequip(Silent: false, SemiForced: true)))
				{
					bodyPart = list[j];
					break;
				}
			}
		}
		if (bodyPart != null)
		{
			GameObject gameObject = GameObject.create(InfectionBlueprint);
			string text = Laterality.LateralityAdjective(bodyPart.Laterality);
			gameObject.UsesSlots = (string.IsNullOrEmpty(text) ? bodyPart.VariantType : (text + " " + bodyPart.VariantType));
			if (bodyPart.SupportsDependent != null)
			{
				foreach (BodyPart part2 in Object.Body.GetParts())
				{
					if (part2 != bodyPart && part2.DependsOn == bodyPart.SupportsDependent && BodyPartSuitableForFungalInfection(part2))
					{
						GameObject equipped = part2.Equipped;
						if (equipped == null || !equipped.HasPropertyOrTag("FungalInfection"))
						{
							string text2 = Laterality.LateralityAdjective(part2.Laterality);
							gameObject.UsesSlots = gameObject.UsesSlots + "," + (string.IsNullOrEmpty(text2) ? part2.VariantType : (text2 + " " + part2.VariantType));
						}
						break;
					}
				}
			}
			if (bodyPart.Type == "Hand")
			{
				MeleeWeapon part = gameObject.GetPart<MeleeWeapon>();
				if (InfectionBlueprint == "WaxInfection")
				{
					part.BaseDamage = "2d3";
					part.Skill = "Cudgel";
					part.PenBonus = 0;
					part.MaxStrengthBonus = 9999;
				}
				else
				{
					part.BaseDamage = "1d4";
					part.Skill = "Cudgel";
					part.PenBonus = 0;
					part.MaxStrengthBonus = 9999;
				}
			}
			else if (bodyPart.Type == "Body")
			{
				if (InfectionBlueprint == "WaxInfection")
				{
					gameObject.GetPart<Armor>().AV = 6;
					gameObject.GetPart<Armor>().DV = -12;
					gameObject.GetPart<Armor>().SpeedPenalty = 4;
				}
				else
				{
					gameObject.GetPart<Armor>().AV = 3;
				}
			}
			else if (bodyPart.Type == "Feet" || bodyPart.Type == "Head")
			{
				if (InfectionBlueprint == "WaxInfection")
				{
					gameObject.GetPart<Armor>().AV = 3;
					gameObject.GetPart<Armor>().DV = -3;
					gameObject.GetPart<Armor>().SpeedPenalty = 2;
				}
				else
				{
					gameObject.GetPart<Armor>().AV = 1;
				}
			}
			else if (bodyPart.Type == "Hand" || bodyPart.Type == "Hands")
			{
				if (InfectionBlueprint == "WaxInfection")
				{
					gameObject.GetPart<Armor>().AV = 2;
					gameObject.GetPart<Armor>().DV = -2;
					gameObject.GetPart<Armor>().SpeedPenalty = 2;
				}
				else
				{
					gameObject.GetPart<Armor>().AV = 1;
				}
			}
			else if (InfectionBlueprint == "WaxInfection")
			{
				gameObject.GetPart<Armor>().AV = 2;
				gameObject.GetPart<Armor>().DV = -4;
				gameObject.GetPart<Armor>().SpeedPenalty = 2;
			}
			else
			{
				gameObject.GetPart<Armor>().AV = 1;
			}
			if (bodyPart.Equip(gameObject, 0, Silent: true, ForDeepCopy: false, Forced: false, SemiForced: true))
			{
				if (Object.IsPlayer())
				{
					Object.pPhysics.PlayWorldSound("FungalInfectionAcquired");
					JournalAPI.AddAccomplishment("You contracted " + gameObject.DisplayNameOnly + " on your " + bodyPart.GetOrdinalName() + ", endearing " + Object.itself + " to fungi across Qud.", "Bless the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", when =name= cemented a historic alliance with fungi by contracting " + gameObject.ShortDisplayName + " on " + The.Player.GetPronounProvider().PossessiveAdjective + " " + bodyPart.GetOrdinalName() + "!", "general", JournalAccomplishment.MuralCategory.BodyExperienceNeutral, JournalAccomplishment.MuralWeight.Medium, null, -1L);
					Popup.Show("You've contracted " + gameObject.DisplayNameOnly + " on your " + bodyPart.GetOrdinalName() + ".");
				}
				if (InfectionBlueprint == "PaxInfection")
				{
					Object.SetIntProperty("HasPax", 1);
				}
				return true;
			}
			gameObject.Destroy();
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			GameObject.validate(ref Owner);
			if (base.Object != null && !base.Object.FireEvent("ApplySpores"))
			{
				base.Duration = 0;
				return true;
			}
			if (TurnsLeft % 300 == 0 && !bSpawned && base.Object != null && base.Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your skin itches.");
			}
			if (TurnsLeft > 0)
			{
				TurnsLeft--;
			}
			if (TurnsLeft <= 0 && !bSpawned && base.Object != null)
			{
				base.Duration = 0;
				bSpawned = true;
				if (!Fake)
				{
					ApplyFungalInfection(base.Object, InfectionObject);
				}
			}
		}
		else if (E.ID == "ApplyFungalSporeInfection")
		{
			return false;
		}
		return true;
	}
}

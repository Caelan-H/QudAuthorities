using System;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Carapace : BaseDefaultEquipmentMutation
{
	public int ACModifier;

	public int DVModifier;

	public int ResistanceMod;

	public new Guid ActivatedAbilityID = Guid.Empty;

	public bool Tight;

	public int TightFactor;

	public GameObject CarapaceObject;

	public int bodyID = int.MinValue;

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Carapace obj = base.DeepCopy(Parent, MapInv) as Carapace;
		obj.CarapaceObject = null;
		return obj;
	}

	public Carapace()
	{
		DisplayName = "Carapace";
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetEnergyCostEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (Tight && (E.Type == null || (!E.Type.Contains("Pass") && !E.Type.Contains("Mental") && !E.Type.Contains("Carapace"))))
		{
			Loosen();
			if (ParentObject.IsPlayer())
			{
				Popup.Show("Your carapace loosens. Your AV decreases by {{R|" + ACModifier + "}}.");
			}
			else
			{
				IComponent<GameObject>.EmitMessage(ParentObject, Grammar.MakePossessive(ParentObject.T()) + " carapace loosens.");
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetDefensiveMutationList");
		Object.RegisterPartEvent(this, "BeginMove");
		Object.RegisterPartEvent(this, "CommandTightenCarapace");
		Object.RegisterPartEvent(this, "IsMobile");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You are protected by a durable carapace.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("+{{C|" + (3 + (int)Math.Floor((decimal)(Level / 2))) + "}} AV\n", "-2 DV\n"), "+{{C|", (5 + 5 * Level).ToString(), "}} Heat Resistance\n"), "+{{C|", (5 + 5 * Level).ToString(), "}} Cold Resistance\n"), "You may tighten your carapace to receive double the AV bonus at a -2 DV penalty as long as you remain still.\n"), "Cannot wear armor.\n"), "+400 reputation with {{w|tortoises}}");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetDefensiveMutationList")
		{
			int high = Math.Max(ParentObject.baseHitpoints - E.GetIntParameter("Distance"), 1);
			if (!Tight && ACModifier >= 1 && ParentObject.HasStat("Hitpoints") && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && Stat.Random(0, high) > ParentObject.hitpoints)
			{
				E.AddAICommand("CommandTightenCarapace");
			}
		}
		else if (E.ID == "CommandTightenCarapace")
		{
			Loosen();
			int aCModifier = ACModifier;
			if (aCModifier < 1)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You fail to tighten your carapace.");
				}
				return false;
			}
			UseEnergy(1000, "Physical Mutation Tighten Carapace");
			Tighten();
			The.Core.RenderBase();
			if (ParentObject.IsPlayer())
			{
				Popup.Show("You tighten your carapace. Your AV increases by {{G|" + aCModifier + "}}.");
			}
			else
			{
				DidX("tighten", ParentObject.its + " carapace", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
			}
		}
		else if (E.ID == "BeginMove" && Tight && !E.HasFlag("Forced") && E.GetStringParameter("Type") != "Teleporting")
		{
			Loosen();
			if (ParentObject.IsPlayer())
			{
				Popup.Show("Your carapace loosens. Your AV decreases by {{R|" + ACModifier + "}}.");
			}
			else
			{
				IComponent<GameObject>.EmitMessage(ParentObject, Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName) + " carapace loosens.");
			}
		}
		return base.FireEvent(E);
	}

	public void Tighten()
	{
		if (!Tight)
		{
			Tight = true;
			TightFactor = ACModifier;
			ParentObject.Statistics["AV"].Bonus += TightFactor;
			ParentObject.Statistics["DV"].Penalty += 2;
		}
	}

	public void Loosen()
	{
		if (Tight)
		{
			ParentObject.Statistics["AV"].Bonus -= TightFactor;
			ParentObject.Statistics["DV"].Penalty -= 2;
			Tight = false;
			TightFactor = 0;
		}
	}

	public override bool ChangeLevel(int NewLevel)
	{
		Loosen();
		ACModifier = 3 + (int)Math.Floor((decimal)(base.Level / 2));
		DVModifier = -2;
		if (ResistanceMod > 0)
		{
			if (ParentObject.HasStat("HeatResistance"))
			{
				ParentObject.GetStat("HeatResistance").Bonus -= ResistanceMod;
			}
			if (ParentObject.HasStat("ColdResistance"))
			{
				ParentObject.GetStat("ColdResistance").Bonus -= ResistanceMod;
			}
			ResistanceMod = 0;
		}
		ResistanceMod = 5 + 5 * base.Level;
		if (ParentObject.HasStat("HeatResistance"))
		{
			ParentObject.GetStat("HeatResistance").Bonus += ResistanceMod;
		}
		if (ParentObject.HasStat("ColdResistance"))
		{
			ParentObject.GetStat("ColdResistance").Bonus += ResistanceMod;
		}
		if (CarapaceObject != null)
		{
			Armor part = CarapaceObject.GetPart<Armor>();
			part.AV = ACModifier;
			part.DV = DVModifier;
		}
		return base.ChangeLevel(NewLevel);
	}

	public override void OnRegenerateDefaultEquipment(Body body)
	{
		AddCarapaceTo(body.GetPartByID(bodyID));
	}

	public void AddCarapaceTo(BodyPart body)
	{
		if (body != null)
		{
			if (CarapaceObject == null)
			{
				CarapaceObject = GameObjectFactory.Factory.CreateObject("Carapace");
			}
			if (body.Equipped != CarapaceObject)
			{
				body.ForceUnequip(Silent: true);
				body.ParentBody.ParentObject.ForceEquipObject(CarapaceObject, body, Silent: true, 0);
			}
		}
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		Body body = GO.Body;
		if (body != null)
		{
			BodyPart body2 = body.GetBody();
			bodyID = body2.ID;
			AddCarapaceTo(body2);
			ActivatedAbilityID = AddMyActivatedAbility("Tighten Carapace", "CommandTightenCarapace", "Physical Mutation", null, "ï");
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		Loosen();
		if (ResistanceMod > 0)
		{
			if (ParentObject.HasStat("HeatResistance"))
			{
				ParentObject.GetStat("HeatResistance").Bonus -= ResistanceMod;
			}
			if (ParentObject.HasStat("ColdResistance"))
			{
				ParentObject.GetStat("ColdResistance").Bonus -= ResistanceMod;
			}
			ResistanceMod = 0;
		}
		CleanUpMutationEquipment(GO, ref CarapaceObject);
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

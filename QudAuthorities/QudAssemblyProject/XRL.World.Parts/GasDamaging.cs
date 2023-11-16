using System;
using System.Collections.Generic;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class GasDamaging : IPart
{
	public string GasType = "Damaging";

	public string Noun = "damaging gas";

	public string MessageColor = "&R";

	public string DamageAttributes = "Gas";

	public string TargetPart;

	public string TargetTag;

	public string TargetTagValue;

	public string TargetEquippedPart;

	public string TargetEquippedTag;

	public string TargetEquippedTagValue;

	public string ExcludeTag;

	public int TargetBodyPartCategoryCode;

	public bool AffectEquipment;

	public bool AffectCybernetics;

	public bool Respiratory;

	public int CreatureDamageDivisor = 200;

	public string TargetBodyPartCategory
	{
		get
		{
			if (TargetBodyPartCategoryCode == 0)
			{
				return null;
			}
			return BodyPartCategory.GetName(TargetBodyPartCategoryCode);
		}
		set
		{
			if (value == null)
			{
				TargetBodyPartCategoryCode = 0;
			}
			else
			{
				TargetBodyPartCategoryCode = BodyPartCategory.GetCode(value);
			}
		}
	}

	public override bool SameAs(IPart p)
	{
		GasDamaging gasDamaging = p as GasDamaging;
		if (gasDamaging.GasType != GasType)
		{
			return false;
		}
		if (gasDamaging.Noun != Noun)
		{
			return false;
		}
		if (gasDamaging.MessageColor != MessageColor)
		{
			return false;
		}
		if (gasDamaging.DamageAttributes != DamageAttributes)
		{
			return false;
		}
		if (gasDamaging.TargetPart != TargetPart)
		{
			return false;
		}
		if (gasDamaging.TargetTag != TargetTag)
		{
			return false;
		}
		if (gasDamaging.ExcludeTag != ExcludeTag)
		{
			return false;
		}
		if (gasDamaging.TargetTagValue != TargetTagValue)
		{
			return false;
		}
		if (gasDamaging.TargetEquippedPart != TargetEquippedPart)
		{
			return false;
		}
		if (gasDamaging.TargetEquippedTag != TargetEquippedTag)
		{
			return false;
		}
		if (gasDamaging.TargetEquippedTagValue != TargetEquippedTagValue)
		{
			return false;
		}
		if (gasDamaging.TargetBodyPartCategoryCode != TargetBodyPartCategoryCode)
		{
			return false;
		}
		if (gasDamaging.AffectEquipment != AffectEquipment)
		{
			return false;
		}
		if (gasDamaging.AffectCybernetics != AffectCybernetics)
		{
			return false;
		}
		if (gasDamaging.Respiratory != Respiratory)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetNavigationWeightEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (TargetPart == "Extradimensional")
		{
			E.MinWeight(2);
		}
		else if (E.Smart)
		{
			E.Uncacheable = true;
			if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && Match(E.Actor))
			{
				Gas gas = ParentObject.GetPart("Gas") as Gas;
				int num = gas.Level * 10;
				int value = (Respiratory ? GetRespiratoryAgentPerformanceEvent.GetFor(E.Actor, ParentObject, gas) : gas.Density);
				int num2 = StepValue(value) + num;
				if (E.Actor != null && !string.IsNullOrEmpty(DamageAttributes) && DamageAttributes != "Gas")
				{
					Damage damage = new Damage(0);
					damage.AddAttributes(DamageAttributes);
					if (damage.IsHeatDamage())
					{
						int num3 = E.Actor.Stat("HeatResistance");
						if (num3 != 0)
						{
							num2 = num2 * (100 - num3) / 100;
						}
					}
					if (damage.IsColdDamage())
					{
						int num4 = E.Actor.Stat("ColdResistance");
						if (num4 != 0)
						{
							num2 = num2 * (100 - num4) / 100;
						}
					}
					if (damage.IsElectricDamage())
					{
						int num5 = E.Actor.Stat("ElectricResistance");
						if (num5 != 0)
						{
							num2 = num2 * (100 - num5) / 100;
						}
					}
					if (damage.IsAcidDamage())
					{
						int num6 = E.Actor.Stat("AcidResistance");
						if (num6 != 0)
						{
							num2 = num2 * (100 - num6) / 100;
						}
					}
				}
				if (num2 > 0)
				{
					E.MinWeight(num2, Math.Min(10 + num, 85));
				}
			}
		}
		else
		{
			E.MinWeight(5);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Type != "Thrown")
		{
			ApplyGasToOthers(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		ProcessDamagingGasBehavior();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		ProcessDamagingGasBehavior();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		ProcessDamagingGasBehavior();
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "DensityChange");
		if (DamageAttributes.Contains("AffectGas"))
		{
			Object.RegisterPartEvent(this, "GasPressureIn");
			Object.RegisterPartEvent(this, "GasPressureOut");
		}
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GasPressureIn" || E.ID == "GasPressureOut")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (Match(gameObjectParameter))
			{
				ApplyDamagingGas(gameObjectParameter);
			}
		}
		else if (E.ID == "DensityChange" && StepValue(E.GetIntParameter("OldValue")) != StepValue(E.GetIntParameter("NewValue")))
		{
			FlushNavigationCaches();
		}
		return base.FireEvent(E);
	}

	private void ProcessDamagingGasBehavior()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && cell.Objects.Count > 1 && !cell.IsGraveyard())
		{
			List<GameObject> list = Event.NewGameObjectList();
			list.AddRange(cell.Objects);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				ApplyGasToOthers(list[i]);
			}
		}
	}

	private bool MatchInner(GameObject GO)
	{
		if (GO == null)
		{
			return false;
		}
		if (Respiratory && !GO.Respires)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(ExcludeTag) && GO.HasTag(ExcludeTag))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(TargetPart) && GO.HasPart(TargetPart))
		{
			return true;
		}
		Body body = null;
		if (TargetBodyPartCategoryCode != 0)
		{
			if (body == null)
			{
				body = GO.Body;
			}
			if (body != null && body.AnyCategoryParts(TargetBodyPartCategoryCode))
			{
				return true;
			}
		}
		if (!string.IsNullOrEmpty(TargetTag) && GO.HasTagOrProperty(TargetTag) && (string.IsNullOrEmpty(TargetTagValue) || GO.GetTagOrStringProperty(TargetTag) == TargetTagValue))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(TargetEquippedPart) || !string.IsNullOrEmpty(TargetEquippedTag))
		{
			if (body == null)
			{
				body = GO.Body;
			}
			if (body != null)
			{
				foreach (BodyPart part in body.GetParts())
				{
					if (part.Equipped != null)
					{
						if (!string.IsNullOrEmpty(TargetEquippedPart) && part.Equipped.HasPart(TargetEquippedPart))
						{
							return true;
						}
						if (!string.IsNullOrEmpty(TargetEquippedTag) && part.Equipped.HasTag(TargetEquippedTag) && (string.IsNullOrEmpty(TargetEquippedTagValue) || part.Equipped.GetTag(TargetEquippedTag) == TargetEquippedTagValue))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public bool Match(GameObject GO)
	{
		if (MatchInner(GO))
		{
			return GO.PhaseMatches(ParentObject);
		}
		return false;
	}

	public bool ApplyDamagingGasWithResult(GameObject GO)
	{
		if (GO.IsInvalid() || GO.IsInGraveyard())
		{
			return false;
		}
		Gas gas = ParentObject.GetPart("Gas") as Gas;
		if (!CheckGasCanAffectEvent.Check(GO, ParentObject, gas))
		{
			return false;
		}
		if (!Match(GO))
		{
			return false;
		}
		int num = (Respiratory ? GetRespiratoryAgentPerformanceEvent.GetFor(GO, ParentObject, gas) : gas.Density);
		if (num <= 0)
		{
			return false;
		}
		int num2 = 0;
		num2 = ((!GO.HasPart("Combat") || GO.HasPropertyOrTag("GasDamageAsIfInanimate")) ? ((int)Math.Ceiling((0.75f * (float)gas.Level + 0.25f) * (float)num)) : ((int)Math.Ceiling((decimal)(num * gas.Level) / (decimal)CreatureDamageDivisor)));
		if (num2 == 0)
		{
			num2 = 1;
		}
		GameObject creator = gas.Creator;
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("from ");
		if (ParentObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: false, BaseOnly: true) != Noun)
		{
			stringBuilder.Append("%t ");
		}
		else
		{
			stringBuilder.Append("the ");
		}
		if (!string.IsNullOrEmpty(MessageColor))
		{
			stringBuilder.Append("{{").Append(MessageColor).Append('|');
		}
		stringBuilder.Append(Noun);
		if (!string.IsNullOrEmpty(MessageColor))
		{
			stringBuilder.Append("}}");
		}
		stringBuilder.Append('!');
		return GO.TakeDamage(num2, Attributes: DamageAttributes, Message: stringBuilder.ToString(), DeathReason: null, ThirdPersonDeathReason: null, Owner: creator, Attacker: null, Source: ParentObject, Perspective: null, Accidental: false, Environmental: false, Indirect: true);
	}

	public void ApplyDamagingGas(GameObject GO)
	{
		ApplyDamagingGasWithResult(GO);
	}

	private void ApplyGasToOthers(GameObject GO)
	{
		if (GO == ParentObject)
		{
			return;
		}
		ApplyDamagingGas(GO);
		if (AffectEquipment || AffectCybernetics)
		{
			List<GameObject> list = Event.NewGameObjectList();
			if (AffectEquipment)
			{
				GO.GetEquippedObjects(list);
			}
			if (AffectCybernetics)
			{
				GO.GetInstalledCybernetics(list);
			}
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				ApplyDamagingGas(list[i]);
			}
		}
	}
}

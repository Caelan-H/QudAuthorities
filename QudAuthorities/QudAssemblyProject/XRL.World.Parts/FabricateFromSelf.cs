using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class FabricateFromSelf : IPoweredPart
{
	public string FabricateBlueprint = "PhysicalObject";

	public string BatchSize = "1d6";

	public string HitpointsPer = "2d4";

	public string Cooldown = "5d10";

	public string UsesLiquid;

	public string FabricateVerb = "fabricate";

	public string FabricateAlternateSource;

	public int EnergyCost = 1000;

	public int HitpointsThreshold;

	public int AIHitpointsThreshold;

	public int LiquidPerBatch = 1;

	public bool AIUseForAmmo;

	public bool AIUseOffensively;

	public bool AIUseDefensively;

	public bool AIUsePassively;

	public bool AIUseForThrowing;

	public bool LiquidAutoTriggers;

	public bool LiquidMustBePure;

	public Guid ActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private string _AbilityDescription;

	[NonSerialized]
	private GameObject _Sample;

	private GameObject Sample
	{
		get
		{
			if (_Sample == null)
			{
				_Sample = GameObject.createSample(FabricateBlueprint);
			}
			return _Sample;
		}
	}

	private string AbilityDescription
	{
		get
		{
			if (_AbilityDescription == null)
			{
				_AbilityDescription = "Fabricate " + Grammar.Pluralize(Sample.GetDisplayName(int.MaxValue, null, null, AsIfKnown: true, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: true));
			}
			return _AbilityDescription;
		}
	}

	private string FabricationDescription => Grammar.Pluralize(Sample.ShortDisplayName);

	public FabricateFromSelf()
	{
		ChargeUse = 1000;
		IsBootSensitive = false;
		IsEMPSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		FabricateFromSelf fabricateFromSelf = p as FabricateFromSelf;
		if (fabricateFromSelf.FabricateBlueprint != FabricateBlueprint)
		{
			return false;
		}
		if (fabricateFromSelf.BatchSize != BatchSize)
		{
			return false;
		}
		if (fabricateFromSelf.HitpointsPer != HitpointsPer)
		{
			return false;
		}
		if (fabricateFromSelf.Cooldown != Cooldown)
		{
			return false;
		}
		if (fabricateFromSelf.UsesLiquid != UsesLiquid)
		{
			return false;
		}
		if (fabricateFromSelf.FabricateVerb != FabricateVerb)
		{
			return false;
		}
		if (fabricateFromSelf.FabricateAlternateSource != FabricateAlternateSource)
		{
			return false;
		}
		if (fabricateFromSelf.EnergyCost != EnergyCost)
		{
			return false;
		}
		if (fabricateFromSelf.HitpointsThreshold != HitpointsThreshold)
		{
			return false;
		}
		if (fabricateFromSelf.AIHitpointsThreshold != AIHitpointsThreshold)
		{
			return false;
		}
		if (fabricateFromSelf.LiquidPerBatch != LiquidPerBatch)
		{
			return false;
		}
		if (fabricateFromSelf.AIUseForAmmo != AIUseForAmmo)
		{
			return false;
		}
		if (fabricateFromSelf.AIUseOffensively != AIUseOffensively)
		{
			return false;
		}
		if (fabricateFromSelf.AIUseDefensively != AIUseDefensively)
		{
			return false;
		}
		if (fabricateFromSelf.AIUsePassively != AIUsePassively)
		{
			return false;
		}
		if (fabricateFromSelf.AIUseForThrowing != AIUseForThrowing)
		{
			return false;
		}
		if (fabricateFromSelf.LiquidAutoTriggers != LiquidAutoTriggers)
		{
			return false;
		}
		if (fabricateFromSelf.LiquidMustBePure != LiquidMustBePure)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return LiquidAutoTriggers;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Activate(Automatic: true);
		}
	}

	public override void Register(GameObject Object)
	{
		ActivatedAbilityID = AddMyActivatedAbility(AbilityDescription, "ActivateFabricateFromSelf", "Tinkering", null, "ö");
		Object.RegisterPartEvent(this, "AIGetDefensiveMutationList");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "AIGetPassiveMutationList");
		Object.RegisterPartEvent(this, "ActivateFabricateFromSelf");
		base.Register(Object);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (!string.IsNullOrEmpty(UsesLiquid))
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume.Volume <= 0)
			{
				return true;
			}
			if (LiquidMustBePure)
			{
				if (!liquidVolume.IsPureLiquid(UsesLiquid))
				{
					return true;
				}
			}
			else if (!liquidVolume.ContainsLiquid(UsesLiquid))
			{
				return true;
			}
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "ProcessInputMissing";
	}

	private bool Activate(bool Automatic = false)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (ParentObject.IsPlayer() && !Automatic)
			{
				Popup.ShowFail("Nothing happens.");
			}
			return false;
		}
		if (HitpointsThreshold > 0 && ParentObject.Stat("Hitpoints") < HitpointsThreshold)
		{
			if (ParentObject.IsPlayer() && !Automatic)
			{
				Popup.ShowFail("Your health is too weak to do that.");
			}
			return false;
		}
		int num = BatchSize.RollCached();
		if (num < 1)
		{
			if (ParentObject.IsPlayer() && !Automatic)
			{
				Popup.ShowFail("Nothing happens.");
			}
			return false;
		}
		if (!string.IsNullOrEmpty(UsesLiquid) && LiquidPerBatch > 0 && !ConsumeLiquid(UsesLiquid, LiquidPerBatch))
		{
			if (ParentObject.IsPlayer() && !Automatic)
			{
				Popup.ShowFail("Nothing happens.");
			}
			return false;
		}
		GameObject gameObject = GameObject.createUnmodified(FabricateBlueprint);
		if (num > 1 && gameObject.HasPart("Stacker") && gameObject.CanGenerateStacked())
		{
			gameObject.GetPart<Stacker>().StackCount = num;
			ParentObject.ReceiveObject(gameObject);
		}
		else
		{
			ParentObject.ReceiveObject(gameObject);
			for (int i = 1; i < num; i++)
			{
				gameObject = GameObject.createUnmodified(FabricateBlueprint);
				ParentObject.ReceiveObject(gameObject);
			}
		}
		if (ParentObject.IsPlayer())
		{
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You " + FabricateVerb + " " + ((num == 1) ? (gameObject.a + gameObject.ShortDisplayName) : (Grammar.Cardinal(num) + " " + FabricationDescription)) + " from " + (string.IsNullOrEmpty(FabricateAlternateSource) ? "the substance of your body" : FabricateAlternateSource) + ".");
			}
		}
		else
		{
			if (Visible())
			{
				IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb(FabricateVerb) + " " + ((num == 1) ? (gameObject.a + gameObject.ShortDisplayName) : (Grammar.Cardinal(num) + " " + FabricationDescription)) + " from " + (string.IsNullOrEmpty(FabricateAlternateSource) ? ("the substance of " + ParentObject.its + " body") : FabricateAlternateSource) + ".");
			}
			if (AIUseForThrowing)
			{
				BodyPart firstBodyPart = ParentObject.GetFirstBodyPart("Thrown Weapon");
				if (firstBodyPart != null && firstBodyPart.Equipped == null)
				{
					ParentObject.FireEvent(Event.New("CommandEquipObject", "Object", gameObject, "BodyPart", firstBodyPart));
				}
			}
		}
		int num2 = 0;
		for (int j = 0; j < num; j++)
		{
			num2 += HitpointsPer.RollCached();
		}
		if (num2 > 0)
		{
			ParentObject.TakeDamage(num2, Owner: ParentObject, Message: "from using " + (string.IsNullOrEmpty(FabricateAlternateSource) ? ((ParentObject.IsPlayer() ? "your" : ParentObject.its) + " body") : FabricateAlternateSource) + " as raw materials.", Attributes: "Fabrication");
		}
		if (!string.IsNullOrEmpty(Cooldown))
		{
			int num3 = Cooldown.RollCached();
			if (num3 > 0)
			{
				CooldownMyActivatedAbility(ActivatedAbilityID, num3);
			}
		}
		ConsumeCharge();
		if (EnergyCost > 0)
		{
			ParentObject.UseEnergy(EnergyCost, "Physical Ability Fabricate");
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ActivateFabricateFromSelf")
		{
			if (!Activate())
			{
				return false;
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			if ((AIUseOffensively || AIUseForAmmo || AIUseForThrowing) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && (AIHitpointsThreshold <= 0 || ParentObject.Stat("Hitpoints") >= AIHitpointsThreshold) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				bool flag = false;
				if (!flag && AIUseOffensively)
				{
					flag = true;
				}
				if (!flag && AIUseForAmmo)
				{
					List<GameObject> missileWeapons = ParentObject.GetMissileWeapons();
					if (missileWeapons != null && missileWeapons.Count > 0)
					{
						bool flag2 = false;
						int num = 0;
						foreach (GameObject item in missileWeapons)
						{
							if (item.GetPart("MagazineAmmoLoader") is MagazineAmmoLoader magazineAmmoLoader && !string.IsNullOrEmpty(magazineAmmoLoader.AmmoPart) && Sample.HasPart(magazineAmmoLoader.AmmoPart))
							{
								flag2 = true;
								int num2 = ((magazineAmmoLoader.Ammo == null) ? 100 : (100 - magazineAmmoLoader.Ammo.Count * 100 / magazineAmmoLoader.MaxAmmo));
								if (num2 > num)
								{
									num = num2;
								}
							}
						}
						if (flag2 && num > 0)
						{
							flag = true;
						}
					}
				}
				if (!flag && AIUseForThrowing)
				{
					BodyPart firstBodyPart = ParentObject.GetFirstBodyPart("Thrown Weapon");
					if (firstBodyPart != null && firstBodyPart.Equipped == null)
					{
						flag = true;
					}
				}
				if (flag)
				{
					E.AddAICommand("ActivateFabricateFromSelf");
				}
			}
		}
		else if (E.ID == "AIGetDefensiveMutationList")
		{
			if (AIUseDefensively && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && (AIHitpointsThreshold <= 0 || ParentObject.Stat("Hitpoints") >= AIHitpointsThreshold) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				E.AddAICommand("ActivateFabricateFromSelf");
			}
		}
		else if (E.ID == "AIGetPassiveMutationList" && AIUsePassively && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && (AIHitpointsThreshold <= 0 || ParentObject.Stat("Hitpoints") >= AIHitpointsThreshold) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.AddAICommand("ActivateFabricateFromSelf");
		}
		return base.FireEvent(E);
	}
}

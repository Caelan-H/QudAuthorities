using System;

namespace XRL.World.Parts;

[Serializable]
public class DamageContents : IPoweredPart
{
	public string Damage = "1d6";

	public string DamageAttributes = "Crushing";

	public string CountLimit;

	[NonSerialized]
	private int ActiveCountLimit = -1;

	[NonSerialized]
	private bool AnyDamage;

	[NonSerialized]
	private int ActiveTurns = 1;

	public DamageContents()
	{
		ChargeUse = 200;
		WorksOnInventory = true;
	}

	public override bool SameAs(IPart p)
	{
		DamageContents damageContents = p as DamageContents;
		if (damageContents.Damage != Damage)
		{
			return false;
		}
		if (damageContents.DamageAttributes != DamageAttributes)
		{
			return false;
		}
		if (damageContents.CountLimit != CountLimit)
		{
			return false;
		}
		return base.SameAs(p);
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
		CheckDamageContents();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		CheckDamageContents(10);
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		CheckDamageContents(100);
	}

	private bool DoDamageContentsItem(GameObject obj)
	{
		if (ActiveCountLimit != -1 && ActiveCountLimit-- <= 0)
		{
			return false;
		}
		if (obj.TakeDamage(Damage.RollCached(), Attributes: DamageAttributes, Message: "from " + ParentObject.t() + ".", DeathReason: null, ThirdPersonDeathReason: null, Owner: null, Attacker: null, Source: ParentObject, Perspective: ParentObject, Accidental: false, Environmental: false, Indirect: false, ShowUninvolved: false, ShowForInanimate: true))
		{
			AnyDamage = true;
		}
		return true;
	}

	public bool DoDamageContents(int Turns = 1)
	{
		if (!string.IsNullOrEmpty(CountLimit))
		{
			ActiveCountLimit = CountLimit.RollCached() * Turns;
		}
		else
		{
			ActiveCountLimit = -1;
		}
		for (int i = 0; i < Turns; i++)
		{
			AnyDamage = false;
			ForeachActivePartSubjectWhile(DoDamageContentsItem, MayMoveAddOrDestroy: true);
			if (!AnyDamage)
			{
				break;
			}
			ConsumeCharge(Turns);
		}
		return AnyDamage;
	}

	public bool CheckDamageContents(int Turns = 1)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			DoDamageContents(Turns);
		}
		return AnyDamage;
	}
}

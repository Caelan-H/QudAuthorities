using System;
using System.Text;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Effects;

/// This class is not used in the base game.
[Serializable]
public class CardiacArrest : Effect
{
	public int HeartsStopped = 1;

	public int Progress;

	public int IllFactor = 5;

	public string SaveAttribute = "Toughness,Willpower";

	public int SaveTarget = 30;

	public string SaveVs = "CardiacArrest CardiacArrestRecovery";

	public CardiacArrest()
	{
		base.Duration = 1;
		base.DisplayName = "{{W|cardiac arrest}}";
	}

	public CardiacArrest(int HeartsStopped)
		: this()
	{
		this.HeartsStopped = HeartsStopped;
	}

	public override int GetEffectType()
	{
		return 100663312;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("-1 to every attribute per turn (current: " + -Progress + ").");
		if (HeartsStopped < base.Object.GetHeartCount())
		{
			stringBuilder.Compound("Unable to progress until all hearts have stopped (current:", "\n").Append(HeartsStopped).Append(").");
		}
		stringBuilder.Compound("Dies when any attribute reaches zero.", "\n").Compound("Doesn't regenerate hit points.", "\n").Compound("Healing effects are only half as effective.", "\n");
		if (!string.IsNullOrEmpty(SaveAttribute))
		{
			stringBuilder.Compound("Difficulty ", "\n").Append(SaveTarget).Append(' ')
				.Append(SaveAttribute.Replace(",", "/"))
				.Append(" save each turn to recover.");
		}
		stringBuilder.Compound("Electrical damage has a percentage chance to cause recovery equal to the damage taken.", "\n");
		if (IllFactor > 0)
		{
			stringBuilder.Compound("Will become ill upon recovery for 5 turns per attribute penalty point.", "\n");
		}
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		int heartCount = Object.GetHeartCount();
		if (heartCount < 1)
		{
			return false;
		}
		if (Object.GetEffect("CardiacArrest") is CardiacArrest cardiacArrest)
		{
			if (cardiacArrest.HeartsStopped < heartCount)
			{
				DidX("go", "into another cardiac arrest", "!", null, null, Object, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, Object.IsPlayer());
				cardiacArrest.HeartsStopped += HeartsStopped;
			}
			return false;
		}
		DidX("go", "into cardiac arrest", "!", null, null, Object, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, Object.IsPlayer());
		return true;
	}

	public override void Remove(GameObject Object)
	{
		base.StatShifter.RemoveStatShifts();
		if (Object.IsPlayer())
		{
			if (HeartsStopped > 1)
			{
				Popup.Show("{{G|Your hearts restart!}}");
			}
			else
			{
				Popup.Show("{{G|Your heart restarts!}}");
			}
		}
		else
		{
			DidX("look", "less stricken", null, null, Object);
		}
		int num = Progress * IllFactor;
		if (num > 0)
		{
			Object.ApplyEffect(new Ill(num, "You feel shaken and infirm."));
		}
		base.Remove(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (XRLCore.CurrentFrame > 45)
		{
			E.Tile = null;
			E.RenderString = "û";
			E.ColorString = "&W^k";
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID)
		{
			return ID == TookDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (E.Damage.IsElectricDamage() && E.Damage.Amount.in100())
		{
			base.Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		CardiacArrestProgress();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "Recuperating");
		Object.RegisterEffectEvent(this, "Regenerating");
		Object.RegisterEffectEvent(this, "Healing");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "Recuperating");
		Object.UnregisterEffectEvent(this, "Regenerating");
		Object.UnregisterEffectEvent(this, "Healing");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Healing")
		{
			E.SetParameter("Amount", E.GetIntParameter("Amount") / 2);
		}
		else
		{
			if (E.ID == "Regenerating")
			{
				E.SetParameter("Amount", 0);
				return false;
			}
			if (E.ID == "Recuperating")
			{
				base.Object.RemoveEffect(this);
			}
		}
		return base.FireEvent(E);
	}

	public void CardiacArrestProgress()
	{
		int heartCount = base.Object.GetHeartCount();
		bool flag = HeartsStopped >= heartCount;
		if (flag)
		{
			Progress++;
			foreach (string attribute in Statistic.Attributes)
			{
				if (base.Object.Stat(attribute) <= 1)
				{
					base.Object.Die(null, "cardiac arrest", "Your " + ((heartCount > 1) ? "hearts" : "heart") + " stopped.", base.Object.Its + " " + ((heartCount > 1) ? "hearts" : "heart") + " stopped.");
					return;
				}
				base.StatShifter.SetStatShift(attribute, -Progress);
			}
		}
		if (!string.IsNullOrEmpty(SaveAttribute) && base.Object.MakeSave(SaveAttribute, SaveTarget, null, null, SaveVs, IgnoreNaturals: false, IgnoreNatural1: false, !flag))
		{
			base.Object.RemoveEffect(this);
		}
	}
}

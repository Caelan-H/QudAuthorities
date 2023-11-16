using System;
using Qud.API;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Beguiled : Effect
{
	public GameObject Beguiler;

	public int Level;

	public int LevelApplied;

	public bool Independent;

	public Beguiled()
	{
		base.DisplayName = "{{m|beguiled}}";
		base.Duration = 1;
	}

	public Beguiled(GameObject Beguiler)
		: this()
	{
		this.Beguiler = Beguiler;
	}

	public Beguiled(GameObject Beguiler, int Level)
		: this(Beguiler)
	{
		this.Level = Level;
	}

	public Beguiled(GameObject Beguiler, int Level, bool Independent)
		: this(Beguiler, Level)
	{
		this.Independent = Independent;
	}

	public override int GetEffectType()
	{
		return 2;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		string text = "Charmed by another creature into following them.";
		if (Level != 0)
		{
			text = text + "\n+" + Level * 5 + " hit points.";
		}
		return text;
	}

	public override string GetDescription()
	{
		return "{{m|beguiled}}";
	}

	public override bool Apply(GameObject Object)
	{
		if (!GameObject.validate(ref Beguiler))
		{
			return false;
		}
		if (Object.pBrain == null)
		{
			return false;
		}
		if (!Object.FireEvent("CanApplyBeguile"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyBeguile"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Beguile", this))
		{
			return false;
		}
		IComponent<GameObject>.XDidYToZ(Object, "ogle", Beguiler, "lovingly", null, null, Beguiler);
		if (Beguiler.IsPlayer() && !Beguiler.HasEffect("Dominated"))
		{
			JournalAPI.AddAccomplishment(Object.A + Object.DisplayNameOnly + " ogled you lovingly after you employed your charm.", "The storied eroticism of =name= became intimately known to " + Object.a + Object.DisplayNameOnly + ".", "general", JournalAccomplishment.MuralCategory.Trysts, JournalAccomplishment.MuralWeight.Medium, null, -1L);
		}
		Object.Heartspray();
		Beguiling.SyncTarget(Beguiler, Object, Independent);
		if (Object.pBrain.GetFeeling(Beguiler) < 0)
		{
			Object.pBrain.SetFeeling(Beguiler, 0);
		}
		Object.pBrain.BecomeCompanionOf(Beguiler);
		ApplyBeguilement();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (GameObject.validate(ref Beguiler) && Object.PartyLeader == Beguiler && !Beguiler.SupportsFollower(Object))
		{
			Object.pBrain.PartyLeader = null;
			Object.pBrain.Goals.Clear();
			DidXToY("lose", "interest in", Beguiler, null, null, null, null, Beguiler);
		}
		UnapplyBeguilement();
		Beguiler = null;
		Object.UpdateVisibleStatusColor();
		base.Remove(Object);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeginTakeAction");
		base.Unregister(Object);
	}

	public Beguiling GetMutation()
	{
		if (!GameObject.validate(ref Beguiler))
		{
			return null;
		}
		if (!(Beguiler.GetPart("Beguiling") is Beguiling beguiling))
		{
			return null;
		}
		if (beguiling.RealityDistortionBased && !base.Object.FireEvent("CheckRealityDistortionUsability"))
		{
			return null;
		}
		return beguiling;
	}

	public void ApplyBeguilement()
	{
		int num = Level - LevelApplied;
		if (base.Object.HasStat("Hitpoints"))
		{
			base.Object.Statistics["Hitpoints"].BaseValue += 5 * num;
		}
		if (base.Object.pBrain != null && GameObject.validate(ref Beguiler))
		{
			base.Object.pBrain.AdjustFeeling(Beguiler, num * 2);
		}
		LevelApplied = Level;
	}

	public void UnapplyBeguilement()
	{
		if (LevelApplied != 0)
		{
			if (base.Object.HasStat("Hitpoints"))
			{
				base.Object.Statistics["Hitpoints"].BaseValue -= 5 * LevelApplied;
			}
			if (base.Object.pBrain != null && GameObject.validate(ref Beguiler))
			{
				base.Object.pBrain.AdjustFeeling(Beguiler, LevelApplied * -2);
			}
			LevelApplied = 0;
		}
	}

	public void SyncToMutation()
	{
		if (!Independent)
		{
			Beguiling mutation = GetMutation();
			if (mutation == null)
			{
				base.Duration = 0;
			}
			else if (mutation.Level != Level)
			{
				Level = mutation.Level;
				ApplyBeguilement();
			}
		}
	}

	public bool IsSupported()
	{
		if (GameObject.validate(ref Beguiler))
		{
			return Beguiler.SupportsFollower(base.Object, 2);
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (!IsSupported())
			{
				base.Duration = 0;
			}
			else
			{
				SyncToMutation();
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyBeguilement();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyBeguilement();
		}
		return base.FireEvent(E);
	}
}

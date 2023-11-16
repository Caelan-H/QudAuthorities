using System;
using XRL.Language;
using XRL.Rules;
using XRL.Wish;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
[HasWishCommand]
public class Nosebleed : Bleeding
{
	public bool Hemorrhage;

	public Nosebleed()
	{
		base.DisplayName = "{{r|nosebleed}}";
		base.Duration = 1;
		Internal = true;
	}

	public Nosebleed(string Damage, int SaveTarget = 20, GameObject Owner = null, bool Stack = true)
		: this()
	{
		base.Damage = Damage;
		base.SaveTarget = SaveTarget;
		base.Owner = Owner;
		base.Stack = Stack;
	}

	public override bool Apply(GameObject Object)
	{
		if (Stack && Object.HasEffect("Nosebleed"))
		{
			Nosebleed nosebleed = Object.GetEffect("Nosebleed") as Nosebleed;
			if (nosebleed.SaveTarget > SaveTarget)
			{
				SaveTarget = nosebleed.SaveTarget;
			}
			if (Stat.RollMin(nosebleed.Damage) * 2 + Stat.RollMax(nosebleed.Damage) < Stat.RollMin(Damage) * 2 + Stat.RollMax(Damage))
			{
				nosebleed.Damage = Damage;
			}
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyBleeding", "Effect", this)))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Bleeding", this))
		{
			return false;
		}
		if (!Object.CanHaveNosebleed())
		{
			if (Object.pBrain == null)
			{
				return false;
			}
			base.DisplayName = "{{r|hemorrhaging}}";
			Hemorrhage = true;
		}
		StartMessage(Object);
		return true;
	}

	public override void StartMessage(GameObject Object)
	{
		if (Hemorrhage)
		{
			base.StartMessage(Object);
			return;
		}
		char color = ColorCoding.ConsequentialColorChar(null, Object);
		IComponent<GameObject>.AddPlayerMessage(Object.Poss("nose begins bleeding."), color);
	}

	public override void StopMessage(GameObject Object)
	{
		if (Hemorrhage)
		{
			base.StartMessage(Object);
			return;
		}
		char color = ColorCoding.ConsequentialColorChar(Object);
		IComponent<GameObject>.AddPlayerMessage(Object.Poss("nose stops bleeding."), color);
	}

	public override string DamageMessage()
	{
		if (Hemorrhage)
		{
			return base.DamageMessage();
		}
		return "from " + Grammar.A(base.DisplayNameStripped) + ".";
	}
}

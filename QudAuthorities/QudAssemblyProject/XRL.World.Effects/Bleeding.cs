using System;
using XRL.Core;
using XRL.Rules;
using XRL.Wish;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
[HasWishCommand]
public class Bleeding : Effect
{
	public string Damage = "1";

	public int SaveTarget = 20;

	public GameObject Owner;

	public bool Stack = true;

	public bool Internal;

	public Bleeding()
	{
		base.DisplayName = "{{r|bleeding}}";
		base.Duration = 1;
	}

	public Bleeding(string Damage, int SaveTarget = 20, GameObject Owner = null, bool Stack = true)
		: this()
	{
		this.Damage = Damage;
		this.SaveTarget = SaveTarget;
		this.Owner = Owner;
		this.Stack = Stack;
	}

	public override int GetEffectType()
	{
		return 117440528;
	}

	public override void SaveData(SerializationWriter Writer)
	{
		GameObject.validate(ref Owner);
		base.SaveData(Writer);
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return base.DisplayName;
	}

	public override string GetDetails()
	{
		return Damage + " damage per turn.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.GetIntProperty("Bleeds") <= 0)
		{
			return false;
		}
		if (Stack && Object.HasEffect("Bleeding"))
		{
			Bleeding bleeding = Object.GetEffect("Bleeding") as Bleeding;
			if (bleeding.SaveTarget > SaveTarget)
			{
				SaveTarget = bleeding.SaveTarget;
			}
			if (Stat.RollMin(bleeding.Damage) * 2 + Stat.RollMax(bleeding.Damage) < Stat.RollMin(Damage) * 2 + Stat.RollMax(Damage))
			{
				bleeding.Damage = Damage;
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
		SyncVersion();
		StartMessage(Object);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
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
		Object.RegisterEffectEvent(this, "EndTurn");
		Object.RegisterEffectEvent(this, "Recuperating");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "EndTurn");
		Object.UnregisterEffectEvent(this, "Recuperating");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		_ = base.Object.pRender;
		int num = XRLCore.CurrentFrame % 60;
		if (num > 45 && num < 55)
		{
			E.RenderString = "\u0003";
			E.ColorString = "&r^K";
			return false;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			GameObject.validate(ref Owner);
			if (base.Duration > 0 && !RecoveryChance())
			{
				base.Object.TakeDamage(Damage.RollCached(), Attacker: Owner, Message: DamageMessage(), Attributes: "Bleeding Unavoidable");
				bool flag = false;
				Cell cell = base.Object.CurrentCell;
				if (!Internal && cell != null && !cell.OnWorldMap())
				{
					if (50.in100())
					{
						foreach (GameObject item in cell.GetObjectsWithPartReadonly("Render"))
						{
							LiquidVolume liquidVolume = item.LiquidVolume;
							if (liquidVolume != null && liquidVolume.IsOpenVolume())
							{
								LiquidVolume liquidVolume2 = new LiquidVolume();
								liquidVolume2.InitialLiquid = base.Object.GetBleedLiquid();
								liquidVolume2.Volume = 2;
								liquidVolume.MixWith(liquidVolume2);
								flag = true;
							}
							else
							{
								item.MakeBloody(base.Object.GetBleedLiquid(), Stat.Random(1, 3));
							}
						}
					}
					if (!flag && 5.in100())
					{
						GameObject gameObject = GameObject.create("BloodSplash");
						gameObject.LiquidVolume.InitialLiquid = base.Object.GetBleedLiquid();
						cell.AddObject(gameObject);
					}
				}
			}
		}
		else if (E.ID == "Recuperating")
		{
			base.Duration = 0;
			StopMessage(base.Object);
		}
		return base.FireEvent(E);
	}

	public bool RecoveryChance()
	{
		if (base.Object.MakeSave("Toughness", SaveTarget, null, null, "Bleeding"))
		{
			base.Duration = 0;
			return true;
		}
		SaveTarget--;
		return false;
	}

	public void SyncVersion()
	{
		base.DisplayName = GetVersionOfBleedingForLiquidSpecification();
	}

	public virtual void StartMessage(GameObject Object)
	{
		DidX("begin", base.DisplayNameStripped, "!", null, null, Object);
	}

	public virtual void StopMessage(GameObject Object)
	{
		DidX("stop", base.DisplayNameStripped, null, null, Object);
	}

	public virtual string DamageMessage()
	{
		return "from " + base.DisplayNameStripped + ".";
	}

	public static string GetVersionOfBleedingForLiquidSpecification(string liquid)
	{
		if (!liquid.Contains(","))
		{
			if (!liquid.Contains("-"))
			{
				return LiquidVolume.getLiquid(liquid).ColoredCirculatoryLossTerm;
			}
			if (liquid.EndsWith("-1000"))
			{
				return LiquidVolume.getLiquid(liquid.Substring(0, liquid.Length - 5)).ColoredCirculatoryLossTerm;
			}
		}
		return new LiquidVolume(liquid, 1).GetColoredCirculatoryLossTerm();
	}

	public string GetVersionOfBleedingForLiquidSpecification()
	{
		return GetVersionOfBleedingForLiquidSpecification(base.Object.GetBleedLiquid());
	}

	[WishCommand(null, null, Command = "bleed")]
	public void HandleBleedWish()
	{
		IComponent<GameObject>.ThePlayer.ApplyEffect(new Bleeding("1d2-1"));
	}
}

using System;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class HolographicBleeding : Effect
{
	public string Damage = "1";

	public int SaveTarget = 20;

	public GameObject Owner;

	public bool Stack = true;

	public HolographicBleeding()
	{
		base.DisplayName = "{{r|bleeding}}";
		base.Duration = 1;
	}

	public HolographicBleeding(string Damage, int SaveTarget, GameObject Owner, bool Stack)
		: this()
	{
		this.Damage = Damage;
		this.SaveTarget = SaveTarget;
		this.Owner = Owner;
		this.Stack = Stack;
	}

	public override int GetEffectType()
	{
		return 117440576;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override void SaveData(SerializationWriter Writer)
	{
		GameObject.validate(ref Owner);
		base.SaveData(Writer);
	}

	public override string GetDescription()
	{
		return "{{r|bleeding}}";
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
		if (Stack && Object.HasEffect("HolographicBleeding"))
		{
			HolographicBleeding holographicBleeding = (HolographicBleeding)Object.GetEffect("HolographicBleeding");
			if (holographicBleeding.SaveTarget > SaveTarget)
			{
				SaveTarget = holographicBleeding.SaveTarget;
			}
			if (Stat.RollMin(holographicBleeding.Damage) * 2 + Stat.RollMax(holographicBleeding.Damage) < Stat.RollMin(Damage) * 2 + Stat.RollMax(Damage))
			{
				holographicBleeding.Damage = Damage;
			}
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyHolographicBleeding", "Effect", this)))
		{
			return false;
		}
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You begin bleeding!", 'r');
		}
		else if (Visible())
		{
			IComponent<GameObject>.AddPlayerMessage(Object.The + Object.ShortDisplayName + Object.GetVerb("begin") + " acting like " + Object.itis + " bleeding.");
		}
		return true;
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
			if (base.Duration > 0)
			{
				if (base.Object.MakeSave("Intelligence", SaveTarget, null, null, "Hologram Illusion"))
				{
					base.Duration = 0;
					if (base.Object.IsPlayer())
					{
						if (base.Object.GetIntProperty("Analgesia") > 0)
						{
							IComponent<GameObject>.AddPlayerMessage("You realize your wound is an illusion.", 'g');
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage("You realize your wound is an illusion, and the pain suddenly stops.", 'g');
						}
					}
					else if (Visible())
					{
						IComponent<GameObject>.AddPlayerMessage(base.Object.The + base.Object.ShortDisplayName + base.Object.GetVerb("stop") + " acting like " + base.Object.itis + " bleeding.");
					}
				}
				else
				{
					base.Object.TakeDamage(Damage.RollCached(), "from bleeding.", "Bleeding Illusion Unavoidable", null, null, null, Owner, null, null, Accidental: false, Environmental: false, Indirect: true);
					SaveTarget--;
					bool flag = false;
					Cell cell = base.Object.CurrentCell;
					if (cell != null && !cell.OnWorldMap())
					{
						if (50.in100())
						{
							foreach (GameObject item in cell.GetObjectsWithPartReadonly("Render"))
							{
								item.MakeBloody("blood", Stat.Random(1, 3));
							}
						}
						if (!flag && 5.in100())
						{
							GameObject gameObject = GameObject.create("BloodSplash");
							gameObject.LiquidVolume.InitialLiquid = base.Object.GetPropertyOrTag("BleedLiquid", "blood-1000");
							gameObject.AddPart(new XRL.World.Parts.Temporary(25));
							cell.AddObject(gameObject);
						}
					}
				}
			}
		}
		else if (E.ID == "Recuperating")
		{
			base.Duration = 0;
			if (base.Object.IsPlayer())
			{
				if (base.Object.GetIntProperty("Analgesia") > 0)
				{
					IComponent<GameObject>.AddPlayerMessage("You realize your wound is an illusion.", 'g');
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage("You realize your wound is an illusion, and the pain suddenly stops.");
				}
			}
			else if (Visible())
			{
				IComponent<GameObject>.AddPlayerMessage(base.Object.The + base.Object.ShortDisplayName + base.Object.GetVerb("stop") + " acting like " + base.Object.itis + " bleeding.");
			}
		}
		return base.FireEvent(E);
	}
}

using System;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Language;

namespace XRL.World.Effects;

[Serializable]
public class LatchedOnto : Effect
{
	public int SaveTarget;

	public string SaveStat = "Strength";

	public string SaveDifficultyStat = "Strength";

	public GameObject LatchedOnWeapon;

	public LatchedOnto()
	{
		base.DisplayName = "{{r|latched onto}}";
		base.Duration = 9;
	}

	public LatchedOnto(GameObject LatchedOnWeapon)
		: this()
	{
		this.LatchedOnWeapon = LatchedOnWeapon;
	}

	public LatchedOnto(GameObject LatchedOnWeapon, int SaveTarget)
		: this(LatchedOnWeapon)
	{
		this.SaveTarget = SaveTarget;
	}

	public LatchedOnto(GameObject LatchedOnWeapon, int SaveTarget, string SaveStat)
		: this(LatchedOnWeapon, SaveTarget)
	{
		this.SaveStat = SaveStat;
	}

	public LatchedOnto(GameObject LatchedOnWeapon, int SaveTarget, string SaveStat, string SaveDifficultyStat)
		: this(LatchedOnWeapon, SaveTarget, SaveStat)
	{
		this.SaveDifficultyStat = SaveDifficultyStat;
	}

	public LatchedOnto(GameObject LatchedOnWeapon, int SaveTarget, string SaveStat, string SaveDifficultyStat, int Duration)
		: this(LatchedOnWeapon, SaveTarget, SaveStat, SaveDifficultyStat)
	{
		base.Duration = Duration;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 33554464;
	}

	public override string GetDetails()
	{
		string text = WhatSubjectIsHeldBy(NullOkay: true, Indefinite: true);
		if (text == null)
		{
			return "Has been latched onto.\nCan't move without breaking free first.";
		}
		return "Has been latched onto by " + text + ".\nCan't move without breaking free first.";
	}

	public override bool Apply(GameObject Object)
	{
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
			return ID == BeginTakeActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0 && (!base.Object.FireEvent("BeforeGrabbed") || base.Object.MakeSave(SaveStat, SaveTarget, LatchedOnWeapon?.Equipped, SaveDifficultyStat, "LatchOn Continue Grab Restraint Escape", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, LatchedOnWeapon)))
		{
			base.Object.ParticleText("*broke free*", IComponent<GameObject>.ConsequentialColorChar(base.Object));
			string text = WhatSubjectIsHeldBy(NullOkay: false, Indefinite: false, LatchedOnWeapon.pPhysics.Equipped.IsPlayer());
			DidX("break", "free from " + text, "!", null, base.Object);
			base.Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeginMove");
		Object.RegisterEffectEvent(this, "Juked");
		Object.RegisterEffectEvent(this, "IsMobile");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeginMove");
		Object.UnregisterEffectEvent(this, "Juked");
		Object.UnregisterEffectEvent(this, "IsMobile");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0 && XRLCore.CurrentFrame > 35 && XRLCore.CurrentFrame < 45)
		{
			E.Tile = null;
			E.RenderString = "X";
			E.ColorString = "&R";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile")
		{
			return false;
		}
		if (E.ID == "Juked")
		{
			base.Object.RemoveEffect(this);
		}
		else if (E.ID == "BeginMove" && E.GetGameObjectParameter("Dragging") != LatchedOnWeapon && E.GetParameter("DestinationCell") is Cell cell)
		{
			foreach (GameObject item in cell.LoopObjectsWithPart("Brain"))
			{
				if (item.pBrain.IsHostileTowards(base.Object) && item.HasPart("Combat"))
				{
					return true;
				}
			}
			if (E.HasParameter("Teleporting"))
			{
				base.Object.ParticleText("*broke free*", IComponent<GameObject>.ConsequentialColorChar(base.Object));
				base.Duration = 0;
			}
			else if (base.Duration > 0)
			{
				string text = WhatSubjectIsHeldBy(NullOkay: false, Indefinite: false, LatchedOnWeapon.pPhysics.Equipped.IsPlayer());
				DidX("are", "held in place by " + text, "!", null, null, base.Object);
				base.Object.UseEnergy(1000, "Movement Failure");
				return false;
			}
		}
		return base.FireEvent(E);
	}

	public string WhatSubjectIsHeldBy(bool NullOkay = false, bool Indefinite = false, bool SecondPerson = false)
	{
		if (LatchedOnWeapon != null && LatchedOnWeapon.IsNowhere())
		{
			LatchedOnWeapon = null;
		}
		if (LatchedOnWeapon == null)
		{
			if (!NullOkay)
			{
				return "being latched onto";
			}
			return null;
		}
		if (LatchedOnWeapon.HasProperName || LatchedOnWeapon.Equipped == null)
		{
			return (Indefinite ? LatchedOnWeapon.a : LatchedOnWeapon.the) + LatchedOnWeapon.ShortDisplayName;
		}
		return (SecondPerson ? "your" : Grammar.MakePossessive((Indefinite ? LatchedOnWeapon.Equipped.a : LatchedOnWeapon.Equipped.the) + LatchedOnWeapon.Equipped.ShortDisplayName)) + " " + LatchedOnWeapon.ShortDisplayName;
	}

	public override void Expired()
	{
		if (LatchedOnWeapon == null)
		{
			return;
		}
		string text = WhatSubjectIsHeldBy(NullOkay: true);
		if (text != null)
		{
			string text2 = IComponent<GameObject>.ConsequentialColor(base.Object);
			if (base.Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage(text2 + ColorUtility.CapitalizeExceptFormatting(text) + text2 + LatchedOnWeapon.GetVerb("release") + " you.");
			}
			else if (base.Object.IsVisible())
			{
				IComponent<GameObject>.AddPlayerMessage(text2 + ColorUtility.CapitalizeExceptFormatting(text) + text2 + LatchedOnWeapon.GetVerb("release") + " " + base.Object.the + base.Object.ShortDisplayName + text2 + ".");
			}
		}
	}
}

using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public abstract class ICamouflage : IActivePart
{
	public int Level = 2;

	public string Description;

	public string _EffectClass;

	[NonSerialized]
	private Type EffectType;

	public string EffectClass
	{
		get
		{
			return _EffectClass;
		}
		set
		{
			_EffectClass = value;
			EffectType = ModManager.ResolveType("XRL.World.Effects." + _EffectClass);
		}
	}

	public ICamouflage()
	{
		WorksOnWearer = true;
	}

	public override bool SameAs(IPart p)
	{
		ICamouflage camouflage = p as ICamouflage;
		if (camouflage.Level != Level)
		{
			return false;
		}
		if (camouflage._EffectClass != _EffectClass)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && (ID != GetShortDescriptionEvent.ID || string.IsNullOrEmpty(Description)))
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		string text = Description;
		if (text.Contains("=level="))
		{
			text = text.Replace("=level=", Level.ToString());
		}
		E.Postfix.AppendRules(text, base.AddStatusSummary);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "EnteredCell");
		CheckCamouflage();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "EnteredCell");
		CheckCamouflage(ForceRemove: true);
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
		CheckCamouflage();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		CheckCamouflage();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		CheckCamouflage();
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			CheckCamouflage();
		}
		return base.FireEvent(E);
	}

	private ICamouflageEffect FindEffect(GameObject who)
	{
		return who.GetEffectByClassName(EffectClass) as ICamouflageEffect;
	}

	private void CheckCamouflage(GameObject who)
	{
		ICamouflageEffect camouflageEffect = FindEffect(who);
		bool flag = camouflageEffect != null;
		if (!flag)
		{
			camouflageEffect = (ICamouflageEffect)Activator.CreateInstance(EffectType);
		}
		if (who.CurrentCell != null && who.CurrentCell.HasObjectOtherThan(camouflageEffect.EnablesCamouflage, who))
		{
			if (!flag)
			{
				who.ApplyEffect(camouflageEffect);
			}
			camouflageEffect.SetContribution(ParentObject, Level);
		}
		else if (flag)
		{
			camouflageEffect.RemoveContribution(ParentObject);
		}
	}

	private void RemoveCamouflage(GameObject who)
	{
		ICamouflageEffect camouflageEffect = FindEffect(who);
		if (camouflageEffect != null)
		{
			who.RemoveEffect(camouflageEffect);
		}
	}

	private void CheckCamouflage(bool ForceRemove = false)
	{
		if (ForceRemove)
		{
			ForeachActivePartSubject(RemoveCamouflage);
		}
		else
		{
			ForeachActivePartSubject(CheckCamouflage);
		}
	}
}

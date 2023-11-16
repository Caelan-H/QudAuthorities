using System;
using System.Text;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class HornsProperties : IPart
{
	public int HornLevel;

	public override bool SameAs(IPart p)
	{
		if ((p as HornsProperties).HornLevel != HornLevel)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDebugInternalsEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "HornLevel", HornLevel);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		GetBleedingPerformance(out var Damage, out var SaveTarget);
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("On penetration, this weapon causes bleeding: ").Append(Damage).Append(" damage per round, save difficulty ")
			.Append(SaveTarget);
		E.Postfix.AppendRules(stringBuilder.ToString());
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "QueryWeaponSecondaryAttackChance");
		Object.RegisterPartEvent(this, "WeaponDealDamage");
		Object.RegisterPartEvent(this, "RollMeleeToHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "RollMeleeToHit")
		{
			E.SetParameter("Result", E.GetIntParameter("Result") + GetHornLevel());
		}
		else if (E.ID == "WeaponDealDamage")
		{
			if (E.GetIntParameter("Penetrations") > 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
				if (gameObjectParameter != null)
				{
					GameObject gameObjectParameter2 = E.GetGameObjectParameter("Attacker");
					GetBleedingPerformance(out var Damage, out var SaveTarget);
					gameObjectParameter.ApplyEffect(new Bleeding(Damage, SaveTarget, gameObjectParameter2));
				}
			}
		}
		else if (E.ID == "QueryWeaponSecondaryAttackChance")
		{
			if ((E.GetStringParameter("Properties", "") ?? "").Contains("Charging"))
			{
				E.SetParameter("Chance", 100);
			}
			else
			{
				E.SetParameter("Chance", 20);
			}
		}
		return base.FireEvent(E);
	}

	public void GetBleedingPerformance(out string Damage, out int SaveTarget)
	{
		int hornLevel = GetHornLevel();
		Damage = "1";
		if (hornLevel > 3)
		{
			Damage = "1d2";
			int num = (hornLevel - 4) / 3;
			if (num > 0)
			{
				Damage = Damage + "+" + num;
			}
		}
		SaveTarget = 20 + 3 * hornLevel;
	}

	public int GetHornLevel()
	{
		int result = 1;
		if (HornLevel != 0)
		{
			result = HornLevel;
		}
		else if (ParentObject?.Equipped?.GetPart("Mutations") is Mutations mutations && mutations.GetMutation("Horns") is Horns horns)
		{
			result = horns.Level;
		}
		return result;
	}
}

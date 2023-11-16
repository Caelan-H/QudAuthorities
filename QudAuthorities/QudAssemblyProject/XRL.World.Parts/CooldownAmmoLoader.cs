using System;
using System.Text;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class CooldownAmmoLoader : IPart
{
	public string Cooldown = "2d6";

	public string ProjectileObject;

	public bool Readout;

	private int CurrentCooldown;

	public long LastFireTimeTick;

	public override bool SameAs(IPart p)
	{
		CooldownAmmoLoader cooldownAmmoLoader = p as CooldownAmmoLoader;
		if (cooldownAmmoLoader.Cooldown != Cooldown)
		{
			return false;
		}
		if (cooldownAmmoLoader.ProjectileObject != ProjectileObject)
		{
			return false;
		}
		if (cooldownAmmoLoader.Readout != Readout)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetMissileWeaponProjectileEvent.ID)
		{
			return ID == GetProjectileBlueprintEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetProjectileBlueprintEvent E)
	{
		if (!string.IsNullOrEmpty(ProjectileObject))
		{
			E.Blueprint = ProjectileObject;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponProjectileEvent E)
	{
		if (!string.IsNullOrEmpty(ProjectileObject))
		{
			E.Blueprint = ProjectileObject;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			E.AddMissileWeaponDamageTag(GetMissileWeaponPerformanceEvent.GetFor(ParentObject.Equipped, ParentObject));
			if (E.Context != "Tinkering" && CooldownActive())
			{
				string cooldownDisplay = GetCooldownDisplay();
				if (!string.IsNullOrEmpty(cooldownDisplay))
				{
					E.AddTag(cooldownDisplay, -5);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIWantUseWeapon");
		Object.RegisterPartEvent(this, "CheckLoadAmmo");
		Object.RegisterPartEvent(this, "CheckReadyToFire");
		Object.RegisterPartEvent(this, "GetMissileWeaponStatus");
		Object.RegisterPartEvent(this, "GetNotReadyToFireMessage");
		Object.RegisterPartEvent(this, "LoadAmmo");
		base.Register(Object);
	}

	public bool CooldownActive()
	{
		if (LastFireTimeTick != The.Game.TimeTicks)
		{
			return LastFireTimeTick + CurrentCooldown > The.Game.TimeTicks;
		}
		return false;
	}

	public string GetCoolingDownMessage()
	{
		int num = (int)(LastFireTimeTick + CurrentCooldown - The.Game.TimeTicks);
		if (Readout)
		{
			return ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("need") + " " + num.Things("more round", "more rounds") + " before " + ParentObject.it + " can be fired again.";
		}
		return ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " unresponsive as " + ParentObject.it + ParentObject.GetVerb("cool", PrependSpace: true, PronounAntecedent: true) + " down.";
	}

	private void CoolingDown(GameObject GO)
	{
		if (GO.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage(GetCoolingDownMessage());
		}
	}

	public bool ApplyCooldownDisplay(StringBuilder SB)
	{
		long num = LastFireTimeTick + CurrentCooldown - XRLCore.Core.Game.TimeTicks;
		if (num > 0)
		{
			SB.Append("{{y|");
			if (Readout)
			{
				SB.Append('[').Append(num).Append(" sec]");
			}
			else
			{
				SB.Append("[cooldown]");
			}
			SB.Append("}}");
			return true;
		}
		return false;
	}

	public string GetCooldownDisplay()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		ApplyCooldownDisplay(stringBuilder);
		return stringBuilder.ToString();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CheckReadyToFire" || E.ID == "AIWantUseWeapon")
		{
			if (CooldownActive())
			{
				return false;
			}
		}
		else if (E.ID == "GetNotReadyToFireMessage")
		{
			if (CooldownActive())
			{
				E.SetParameter("Message", GetCoolingDownMessage());
			}
		}
		else if (E.ID == "GetMissileWeaponStatus")
		{
			if (!E.HasParameter("Override") && CooldownActive())
			{
				StringBuilder parameter = E.GetParameter<StringBuilder>("Items");
				parameter.Length = 0;
				ApplyCooldownDisplay(parameter);
				E.SetParameter("Override", this);
			}
		}
		else if (E.ID == "CheckLoadAmmo")
		{
			if (CooldownActive())
			{
				CoolingDown(E.GetGameObjectParameter("Loader"));
				return false;
			}
		}
		else if (E.ID == "LoadAmmo")
		{
			if (CooldownActive())
			{
				CoolingDown(E.GetGameObjectParameter("Loader"));
				E.SetParameter("Ammo", null);
				return false;
			}
			if (ProjectileObject != null)
			{
				E.SetParameter("Ammo", GameObject.create(ProjectileObject));
			}
			LastFireTimeTick = The.Game.TimeTicks;
			CurrentCooldown = Stat.Roll(Cooldown);
		}
		return base.FireEvent(E);
	}
}

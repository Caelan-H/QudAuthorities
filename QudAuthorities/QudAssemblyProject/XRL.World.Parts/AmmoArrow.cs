using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class AmmoArrow : IAmmo
{
	public string ProjectileObject;

	public override bool SameAs(IPart p)
	{
		if ((p as AmmoArrow).ProjectileObject != ProjectileObject)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetProjectileObjectEvent.ID)
		{
			return ID == QueryEquippableListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(ProjectileObject);
			if (blueprintIfExists != null)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("(").Append(Convert.ToInt32(blueprintIfExists.GetPartParameter("Projectile", "BasePenetration")) + 4).Append("/")
					.Append(blueprintIfExists.GetPartParameter("Projectile", "BaseDamage"))
					.Append(")");
				E.AddTag(stringBuilder.ToString());
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (E.SlotType.Contains("AmmoArrow") && !E.List.Contains(ParentObject))
		{
			E.List.Add(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetProjectileObjectEvent E)
	{
		if (!string.IsNullOrEmpty(ProjectileObject))
		{
			E.Projectile = GameObject.create(ProjectileObject);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}

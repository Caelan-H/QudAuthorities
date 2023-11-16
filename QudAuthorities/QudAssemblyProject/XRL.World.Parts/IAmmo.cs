using System;

namespace XRL.World.Parts;

[Serializable]
public abstract class IAmmo : IPart
{
	public GameObject LoadedIn;

	public int ScatterOnDeathThreshold = 5;

	public string ScatterOnDeathPercentage = "8-10";

	public override bool SameAs(IPart p)
	{
		if ((p as IAmmo).LoadedIn != LoadedIn)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != DropOnDeathEvent.ID && ID != GetContextEvent.ID && ID != RemoveFromContextEvent.ID && ID != ReplaceInContextEvent.ID)
		{
			return ID == TryRemoveFromContextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(DropOnDeathEvent E)
	{
		if (ScatterOnDeathThreshold > 0 && !string.IsNullOrEmpty(ScatterOnDeathPercentage))
		{
			int count = ParentObject.Count;
			if (count >= ScatterOnDeathThreshold)
			{
				ParentObject.Count = Math.Max(count * ScatterOnDeathPercentage.RollCached() / 100, ScatterOnDeathThreshold);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContextEvent E)
	{
		if (GameObject.validate(ref LoadedIn))
		{
			E.ObjectContext = LoadedIn;
			E.Relation = 5;
			E.RelationManager = this;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		ReplaceAmmo(E.Replacement);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RemoveFromContextEvent E)
	{
		ReplaceAmmo(null);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TryRemoveFromContextEvent E)
	{
		ReplaceAmmo(null);
		return base.HandleEvent(E);
	}

	private void ReplaceAmmo(GameObject Replacement)
	{
		if (GameObject.validate(ref LoadedIn) && LoadedIn.GetPart("MagazineAmmoLoader") is MagazineAmmoLoader magazineAmmoLoader && magazineAmmoLoader.Ammo == ParentObject)
		{
			magazineAmmoLoader.SetAmmo(Replacement);
		}
	}

	public GameObject GetLoadedIn()
	{
		GameObject.validate(ref LoadedIn);
		return LoadedIn;
	}
}

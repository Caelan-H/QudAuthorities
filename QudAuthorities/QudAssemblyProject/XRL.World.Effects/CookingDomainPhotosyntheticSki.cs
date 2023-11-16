using System;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPhotosyntheticSkin_RegenerationUnit : ProceduralCookingEffectUnit
{
	public int Tier = 1;

	public override void Init(GameObject target)
	{
		PhotosyntheticSkin part = target.GetPart<PhotosyntheticSkin>();
		if (part != null)
		{
			Tier = part.Level;
		}
	}

	public override string GetDescription()
	{
		return "+" + (20 + Tier * 10) + "% to natural healing rate";
	}

	public override string GetTemplatedDescription()
	{
		return "+20% + (Photosynthetic Skin level * 10)% to natural healing rate";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "Regenerating2");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "Regenerating2");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "Regenerating2")
		{
			float num = 1f + (float)(20 + Tier * 10) * 0.01f;
			int value = (int)Math.Ceiling((float)E.GetIntParameter("Amount") * num);
			E.SetParameter("Amount", value);
		}
	}
}
[Serializable]
public class CookingDomainPhotosyntheticSkin_UnitQuickness : ProceduralCookingEffectUnit
{
	public int Tier = 1;

	public override void Init(GameObject target)
	{
		PhotosyntheticSkin part = target.GetPart<PhotosyntheticSkin>();
		if (part != null)
		{
			Tier = part.Level;
		}
	}

	public override string GetDescription()
	{
		return "+" + (13 + Tier * 2) + " Quickness";
	}

	public override string GetTemplatedDescription()
	{
		return "+13 + (Photosynthetic Skin level * 2) Quickness";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.Statistics["Speed"].Bonus += 13 + Tier * 2;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["Speed"].Bonus -= 13 + Tier * 2;
	}
}
[Serializable]
public class CookingDomainPhotosyntheticSkin_SatedUnit : ProceduralCookingEffectUnit
{
	public int Tier = 1;

	public override void Init(GameObject target)
	{
		PhotosyntheticSkin part = target.GetPart<PhotosyntheticSkin>();
		if (part != null)
		{
			Tier = part.Level;
		}
	}

	public override string GetDescription()
	{
		return "";
	}

	public override string GetTemplatedDescription()
	{
		return "";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "BeginTakeAction");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.GetPart<Stomach>().GetHungry();
		Object.UnregisterEffectEvent(parent, "BeginTakeAction");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			parent.Object.GetPart<Stomach>().CookingCounter = 0;
			parent.Duration--;
		}
	}
}

using System;

namespace XRL.World.Parts;

[Serializable]
public abstract class IModification : IActivePart
{
	public int Tier;

	public IModification()
	{
		Configure();
	}

	public IModification(int Tier)
		: this()
	{
		ApplyTier(Tier);
	}

	public void ApplyTier(int Tier)
	{
		this.Tier = Tier;
		TierConfigure();
	}

	public virtual void Configure()
	{
	}

	public virtual void TierConfigure()
	{
	}

	public override bool SameAs(IPart p)
	{
		if ((p as IModification).Tier != Tier)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public virtual string GetModificationDisplayName()
	{
		return null;
	}

	public virtual bool ModificationApplicable(GameObject obj)
	{
		return true;
	}

	public virtual bool BeingAppliedBy(GameObject obj, GameObject who)
	{
		return true;
	}

	public virtual void ApplyModification(GameObject obj)
	{
	}

	public virtual void ApplyModification()
	{
		ApplyModification(ParentObject);
	}

	protected void IncreaseDifficulty(int Amount, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		Examiner examiner = (obj.IsCreature ? (obj.GetPart("Examiner") as Examiner) : obj.RequirePart<Examiner>());
		if (examiner != null)
		{
			examiner.Difficulty += Amount;
		}
	}

	protected void IncreaseDifficultyIfDifficult(int Amount, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		if (obj.GetPart("Examiner") is Examiner examiner && examiner.Difficulty > 0)
		{
			examiner.Difficulty += Amount;
		}
	}

	protected void IncreaseDifficultyIfComplex(int Amount, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		if (obj.GetPart("Examiner") is Examiner examiner && examiner.Complexity > 0)
		{
			examiner.Difficulty += Amount;
		}
	}

	protected void IncreaseComplexity(int Amount, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		Examiner examiner = (obj.IsCreature ? (obj.GetPart("Examiner") as Examiner) : obj.RequirePart<Examiner>());
		if (examiner != null)
		{
			examiner.Complexity += Amount;
			obj.RequirePart<IsTechScannableUnlessAlive>();
		}
	}

	protected void IncreaseComplexityIfComplex(int Amount, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		if (obj.GetPart("Examiner") is Examiner examiner && examiner.Complexity > 0)
		{
			examiner.Complexity += Amount;
			obj.RequirePart<IsTechScannableUnlessAlive>();
		}
	}

	protected void IncreaseDifficultyAndComplexity(int Amount1, int Amount2, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		Examiner examiner = (obj.IsCreature ? (obj.GetPart("Examiner") as Examiner) : obj.RequirePart<Examiner>());
		if (examiner != null)
		{
			examiner.Difficulty += Amount1;
			examiner.Complexity += Amount2;
			obj.RequirePart<IsTechScannableUnlessAlive>();
		}
	}

	protected void IncreaseDifficultyAndComplexityIfComplex(int Amount1, int Amount2, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		if (obj.GetPart("Examiner") is Examiner examiner && examiner.Complexity > 0)
		{
			examiner.Difficulty += Amount1;
			examiner.Complexity += Amount2;
			obj.RequirePart<IsTechScannableUnlessAlive>();
		}
	}
}

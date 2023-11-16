using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedMutationBase<T> : IModification where T : BaseMutation, new()
{
	public string MutationDisplayName;

	public string ClassName;

	public string TrackingProperty;

	public int moddedAmount;

	public Guid mutationMod;

	public ModImprovedMutationBase()
	{
	}

	public ModImprovedMutationBase(int Tier)
		: base(Tier)
	{
		base.Tier = Tier;
	}

	public override void Configure()
	{
		T val = new T();
		MutationDisplayName = val.DisplayName;
		ClassName = val.Name;
		TrackingProperty = "Equipped" + ClassName;
		WorksOnEquipper = true;
		NameForStatus = ClassName + "Amp";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Grants you " + MutationDisplayName + " at level " + Tier + ". If you already have " + MutationDisplayName + ", its level is increased by " + Tier + ".");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (ParentObject.IsEquippedProperly())
		{
			mutationMod = E.Actor.RequirePart<Mutations>().AddMutationMod(typeof(T).Name, Tier, Mutations.MutationModifierTracker.SourceType.Equipment, ParentObject.DisplayName);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.RequirePart<Mutations>().RemoveMutationMod(mutationMod);
		return base.HandleEvent(E);
	}
}

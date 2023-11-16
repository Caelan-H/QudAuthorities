using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class AddsMutationOnEquip<T> : IActivePart where T : BaseMutation, new()
{
	public int moddedAmount;

	public int Level = 1;

	public string _MutationDisplayName;

	public string ClassName;

	public string TrackingProperty;

	public bool mutationWasAdded;

	public bool wasApplied;

	public bool canLevel;

	public Guid mutationTracker;

	public string MutationDisplayName
	{
		get
		{
			if (_MutationDisplayName == null)
			{
				T val = new T();
				_MutationDisplayName = val.DisplayName;
			}
			return _MutationDisplayName;
		}
		set
		{
			_MutationDisplayName = value;
		}
	}

	public AddsMutationOnEquip()
	{
		T val = new T();
		_MutationDisplayName = val.DisplayName;
		ClassName = val.Name;
		TrackingProperty = "Equipped" + ClassName;
		canLevel = val.CanLevel();
		ChargeUse = 0;
		IsBootSensitive = false;
		IsEMPSensitive = false;
		WorksOnEquipper = true;
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		AddsMutationOnEquip<T> obj = base.DeepCopy(Parent, MapInv) as AddsMutationOnEquip<T>;
		obj.mutationWasAdded = false;
		return obj;
	}

	public override bool SameAs(IPart p)
	{
		if (!(p is AddsMutationOnEquip<T>))
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CellChanged");
		Object.RegisterPartEvent(this, "EffectApplied");
		Object.RegisterPartEvent(this, "EffectRemoved");
		Object.RegisterPartEvent(this, "BeforeMutationAdded");
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		Object.RegisterPartEvent(this, "GetShortDescription");
		base.Register(Object);
	}

	private void ApplyBonus(GameObject who)
	{
		if (!mutationWasAdded && who != null && who.IsPlayer() && IsObjectActivePartSubject(who))
		{
			mutationTracker = who.RequirePart<Mutations>().AddMutationMod(typeof(T).Name, Level, Mutations.MutationModifierTracker.SourceType.Equipment, ParentObject.DisplayName);
			mutationWasAdded = true;
		}
	}

	private void UnapplyBonus(GameObject who)
	{
		if (mutationWasAdded)
		{
			who.RequirePart<Mutations>().RemoveMutationMod(mutationTracker);
			mutationWasAdded = false;
		}
	}

	public void CheckApplyBonus(GameObject who, bool UseCharge = false)
	{
		if (mutationWasAdded)
		{
			if (IsDisabled(UseCharge, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) || !IsObjectActivePartSubject(who))
			{
				UnapplyBonus(who);
			}
		}
		else if (!IsDisabled(UseCharge, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ApplyBonus(who);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (ParentObject.pPhysics.Equipped != null)
			{
				CheckApplyBonus(ParentObject.pPhysics.Equipped, UseCharge: true);
			}
		}
		else if (E.ID == "Equipped")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("EquippingObject");
			CheckApplyBonus(gameObjectParameter);
		}
		else if (E.ID == "Unequipped")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("UnequippingObject");
			UnapplyBonus(gameObjectParameter2);
		}
		else if (E.ID == "GetShortDescription")
		{
			if (canLevel)
			{
				E.SetParameter("Postfix", E.GetStringParameter("Postfix") + "\n&CGrants you " + MutationDisplayName + " at level " + Level + ".");
			}
			else
			{
				E.SetParameter("Postfix", E.GetStringParameter("Postfix") + "\n&CGrants you " + MutationDisplayName + ".");
			}
		}
		else if (E.ID == "EffectApplied" || E.ID == "EffectRemoved" || E.ID == "CellChanged")
		{
			CheckApplyBonus(ParentObject.pPhysics.Equipped);
		}
		return base.FireEvent(E);
	}
}

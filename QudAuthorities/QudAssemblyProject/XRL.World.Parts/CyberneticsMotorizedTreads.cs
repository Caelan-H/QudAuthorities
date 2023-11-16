using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsMotorizedTreads : IPart
{
	public string PartName;

	public string PartDescription;

	public string PartDependsOn;

	public int PartLaterality;

	public int PartMobility;

	public bool PartIntegral;

	public bool PartPlural;

	public bool PartMass;

	public int PartCategory;

	public string AdditionsManagerID => ParentObject.id + "::MotorizedTreads::Add";

	public string ChangesManagerID => ParentObject.id + "::MotorizedTreads::Change";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		BodyPart bodyPart = E.Part.AddPartAt("Tread", 2, null, null, null, null, Category: 6, Extrinsic: true, Manager: AdditionsManagerID, RequiresLaterality: null, Mobility: null, Appendage: null, Integral: null, Mortal: null, Abstract: null, Plural: null, Mass: null, Contact: null, IgnorePosition: null, InsertAfter: "Tread", OrInsertBefore: "Fungal Outcrop");
		E.Part.AddPartAt(bodyPart, "Tread", 1, null, null, null, null, Category: 6, Extrinsic: true, Manager: AdditionsManagerID);
		E.Part.Manager = ChangesManagerID;
		if (!E.ForDeepCopy)
		{
			PartName = E.Part.Name;
			PartDescription = E.Part.Description;
			PartDependsOn = E.Part.DependsOn;
			PartLaterality = E.Part.Laterality;
			PartMobility = E.Part.Mobility;
			PartIntegral = E.Part.Integral;
			PartPlural = E.Part.Plural;
			PartMass = E.Part.Mass;
			PartCategory = E.Part.Category;
			E.Part.Name = "lower body";
			E.Part.Description = "Lower Body";
			E.Part.DependsOn = null;
			E.Part.Laterality = 0;
			E.Part.Mobility = 0;
			E.Part.Integral = true;
			E.Part.Plural = false;
			E.Part.Mass = false;
			E.Part.Category = 6;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveBodyPartsByManager(AdditionsManagerID, EvenIfDismembered: true);
		BodyPart bodyPartByManager = E.Implantee.GetBodyPartByManager(ChangesManagerID, EvenIfDismembered: true);
		if (bodyPartByManager != null)
		{
			bodyPartByManager.Name = PartName;
			bodyPartByManager.Description = PartDescription;
			bodyPartByManager.DependsOn = PartDependsOn;
			bodyPartByManager.Laterality = PartLaterality;
			bodyPartByManager.Mobility = PartMobility;
			bodyPartByManager.Integral = PartIntegral;
			bodyPartByManager.Plural = PartPlural;
			bodyPartByManager.Mass = PartMass;
			bodyPartByManager.Category = PartCategory;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}

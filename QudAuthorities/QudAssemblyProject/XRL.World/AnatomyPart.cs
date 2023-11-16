using System;
using System.Collections.Generic;

namespace XRL.World;

[Serializable]
public class AnatomyPart
{
	public BodyPartType Type;

	public string SupportsDependent;

	public string DependsOn;

	public string RequiresType;

	public string DefaultBehavior;

	public int? Category;

	public int? Laterality;

	public int? RequiresLaterality;

	public int? Mobility;

	public bool? Integral;

	public bool? Mortal;

	public bool? Abstract;

	public bool? Extrinsic;

	public bool? Plural;

	public bool? Mass;

	public bool? Contact;

	public bool? IgnorePosition;

	public List<AnatomyPart> Subparts = new List<AnatomyPart>(0);

	public AnatomyPart(BodyPartType Type)
	{
		this.Type = Type;
	}

	public void ApplyTo(BodyPart parent)
	{
		if (parent == null)
		{
			MetricsManager.LogError("called with null parent, type " + Type);
			return;
		}
		BodyPart parent2 = parent.AddPart(Type, SupportsDependent: SupportsDependent, DependsOn: DependsOn, RequiresType: RequiresType, DefaultBehavior: DefaultBehavior, Category: Category, Laterality: Laterality.GetValueOrDefault(), Manager: null, RequiresLaterality: RequiresLaterality, Mobility: Mobility, Appendage: null, Integral: Integral, Mortal: Mortal, Abstract: Abstract, Extrinsic: Extrinsic, Plural: Plural, Mass: Mass, Contact: Contact, IgnorePosition: IgnorePosition);
		foreach (AnatomyPart subpart in Subparts)
		{
			subpart.ApplyTo(parent2);
		}
	}
}

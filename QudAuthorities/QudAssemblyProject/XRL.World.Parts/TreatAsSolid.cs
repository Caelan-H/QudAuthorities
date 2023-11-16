using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class TreatAsSolid : IPart
{
	public string TargetPart;

	public string TargetTag;

	public string TargetTagValue;

	public bool RealityDistortionBased;

	public bool Hits = true;

	public bool Match(GameObject GO)
	{
		if (TargetPart != null && GO.HasPart(TargetPart))
		{
			return true;
		}
		if (TargetTag != null && GO.HasTag(TargetTag) && (TargetTagValue == null || GO.GetTag(TargetTag) == TargetTagValue))
		{
			return true;
		}
		if (RealityDistortionBased)
		{
			foreach (RealityStabilized effect in GO.GetEffects("RealityStabilized"))
			{
				int strength = effect.Strength;
				if (strength > 0 && (strength >= 100 || Stat.Random(1, 100) <= strength))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool SameAs(IPart p)
	{
		TreatAsSolid treatAsSolid = p as TreatAsSolid;
		if (treatAsSolid.TargetPart != TargetPart)
		{
			return false;
		}
		if (treatAsSolid.TargetTag != TargetTag)
		{
			return false;
		}
		if (treatAsSolid.TargetTagValue != TargetTagValue)
		{
			return false;
		}
		if (treatAsSolid.RealityDistortionBased != RealityDistortionBased)
		{
			return false;
		}
		if (treatAsSolid.Hits != Hits)
		{
			return false;
		}
		return base.SameAs(p);
	}
}

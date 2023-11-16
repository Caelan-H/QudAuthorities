using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class ExtradimensionalLoot : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeDeathRemoval");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeathRemoval")
		{
			try
			{
				Cell cell = ParentObject.GetCurrentCell();
				if (cell != null && 5.in100())
				{
					GameObject mostValuableItem = ParentObject.GetMostValuableItem();
					if (mostValuableItem != null)
					{
						mostValuableItem.SplitStack(1, ParentObject);
						mostValuableItem.RemovePart("Temporary");
						mostValuableItem.RemovePart("ExistenceSupport");
						if (!mostValuableItem.HasPart("ModExtradimensional"))
						{
							Extradimensional part = ParentObject.GetPart<Extradimensional>();
							TechModding.ApplyModification(mostValuableItem, new ModExtradimensional(part.WeaponModIndex, part.MissileWeaponModIndex, part.ArmorModIndex, part.ShieldModIndex, part.MiscModIndex, part.Training, part.DimensionName, part.SecretID));
							mostValuableItem.SetStringProperty("NeverStack", "1");
						}
						if (Visible())
						{
							IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("drop") + " " + mostValuableItem.a + mostValuableItem.ShortDisplayName + ", and by sheer chance " + mostValuableItem.it + mostValuableItem.GetVerb("quantum tunnel", PrependSpace: true, PronounAntecedent: true) + " and fully" + mostValuableItem.GetVerb("materialize") + " in this dimension.");
						}
						cell.AddObject(mostValuableItem);
					}
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("PsychicHunter loot", x);
			}
			return true;
		}
		return true;
	}
}

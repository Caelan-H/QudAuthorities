using System;
using System.Linq;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class BurnOffGas : IPart
{
	public int DamageTaken;

	public int DamagePer = 10;

	public int Chance = 100;

	public string Number = "1";

	public string DamageTriggerTypes = "Heat;Fire";

	public string Blueprint;

	public bool PopulationRollsAreStatic = true;

	public bool SpawnAsDropColor = true;

	public override bool SameAs(IPart p)
	{
		BurnOffGas burnOffGas = p as BurnOffGas;
		if (burnOffGas.DamagePer != DamagePer)
		{
			return false;
		}
		if (burnOffGas.Chance != Chance)
		{
			return false;
		}
		if (burnOffGas.Number != Number)
		{
			return false;
		}
		if (burnOffGas.Blueprint != Blueprint)
		{
			return false;
		}
		if (burnOffGas.PopulationRollsAreStatic != PopulationRollsAreStatic)
		{
			return false;
		}
		if (burnOffGas.SpawnAsDropColor != SpawnAsDropColor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeTookDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeTookDamage")
		{
			if (!(ParentObject.GetPart("Physics") is Physics physics) || physics.CurrentCell == null)
			{
				return true;
			}
			if (!E.GetParameter<Damage>("Damage").Attributes.Any((string s) => DamageTriggerTypes.Contains(s)))
			{
				return true;
			}
			DamageTaken += E.GetParameter<Damage>("Damage").Amount;
			while (DamageTaken >= DamagePer)
			{
				DamageTaken -= DamagePer;
				if ((Chance < 100 && Stat.Random(1, 100) > Chance) || string.IsNullOrEmpty(Blueprint) || ParentObject.pPhysics.CurrentCell == null)
				{
					continue;
				}
				for (int i = 0; i < Stat.Roll(Number); i++)
				{
					string blueprint = Blueprint;
					if (blueprint.StartsWith("@"))
					{
						blueprint = PopulationManager.RollOneFrom(blueprint.Substring(1)).Blueprint;
						if (PopulationRollsAreStatic)
						{
							Blueprint = blueprint;
						}
					}
					GameObject gameObject = GameObjectFactory.Factory.CreateObject(blueprint);
					ParentObject.pPhysics.CurrentCell.AddObject(gameObject);
					if (ParentObject.IsVisible())
					{
						IComponent<GameObject>.XDidY(ParentObject, "burn", "off " + gameObject.a + gameObject.ShortDisplayName);
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}

using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using XRL.Language;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class PuffInfection : IPart
{
	public string ColorString = "0";

	public override bool SameAs(IPart p)
	{
		if ((p as PuffInfection).ColorString != ColorString)
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
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		base.Register(Object);
	}

	public bool Puff()
	{
		if (ColorString.Length == 1)
		{
			Stat.ReseedFrom("PufferType");
			string key = Algorithms.RandomShuffle(SporePuffer.PufferList)[Convert.ToInt32(ColorString)];
			GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.Blueprints[key];
			ColorString = gameObjectBlueprint.GetPart("Render").Parameters["ColorString"];
		}
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return false;
		}
		List<Cell> localAdjacentCells = cell.GetLocalAdjacentCells();
		bool flag = false;
		if (localAdjacentCells != null)
		{
			foreach (Cell item in localAdjacentCells)
			{
				bool flag2 = false;
				bool flag3 = false;
				foreach (GameObject item2 in item.LoopObjects())
				{
					if (!flag3 && item2.pBrain != null)
					{
						flag3 = true;
					}
					if (!flag2 && item2.HasPart("GasFungalSpores"))
					{
						flag2 = true;
					}
					if (flag3 && flag2)
					{
						break;
					}
				}
				if (flag3 && !flag2)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			GameObject gameObject = ParentObject.Equipped ?? ParentObject;
			if (gameObject.CurrentCell != null)
			{
				gameObject.ParticleBlip("&W*");
			}
			for (int i = 0; i < localAdjacentCells.Count; i++)
			{
				if (!localAdjacentCells[i].HasObjectWithPart("GasFungalSpores"))
				{
					GameObject gameObject2 = localAdjacentCells[i].AddObject("FungalSporeGasPuff");
					gameObject2.GetPart<Gas>().ColorString = ColorString;
					gameObject2.GetPart<Gas>().Creator = gameObject;
				}
			}
		}
		return flag;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			if (25.in100() && Puff())
			{
				GameObject equipped = ParentObject.Equipped;
				if (equipped != null)
				{
					if (equipped.IsPlayer())
					{
						BodyPart bodyPart = ParentObject.EquippedOn();
						IComponent<GameObject>.AddPlayerMessage("&yYour " + bodyPart.GetOrdinalName() + " " + (bodyPart.Plural ? "spew" : "spews") + " a cloud of spores.");
					}
					else if (IComponent<GameObject>.Visible(equipped))
					{
						BodyPart bodyPart2 = ParentObject.EquippedOn();
						IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(equipped.The + equipped.ShortDisplayName) + "&y " + bodyPart2.GetOrdinalName() + " " + (bodyPart2.Plural ? "spew" : "spews") + " a cloud of spores.");
					}
				}
			}
		}
		else if (E.ID == "Equipped")
		{
			E.GetGameObjectParameter("EquippingObject").RegisterPartEvent(this, "BeforeApplyDamage");
		}
		else if (E.ID == "Unequipped")
		{
			E.GetGameObjectParameter("UnequippingObject").UnregisterPartEvent(this, "BeforeApplyDamage");
		}
		return base.FireEvent(E);
	}
}

using System;

namespace XRL.World.Parts;

[Serializable]
public class IgnoreMarkOfDeath : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CheckingHostilityTowardsPlayer");
		base.Register(Object);
	}

	private void ScanForScrambler(GameObject GO)
	{
		if (GO == null || ParentObject.pBrain == null || ParentObject.pBrain.GetFeeling(GO) >= 0)
		{
			return;
		}
		bool flag = false;
		foreach (GameObject item in GO.Inventory.GetObjectsDirect())
		{
			if (item.Blueprint == "Droid Scrambler")
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Body body = GO.Body;
			if (body != null)
			{
				foreach (GameObject equippedObject in body.GetEquippedObjects())
				{
					if (equippedObject.Blueprint == "Droid Scrambler")
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (flag)
		{
			if (ParentObject.pBrain.Target == GO)
			{
				ParentObject.pBrain.Goals.Clear();
			}
			ParentObject.Statistics["XPValue"].BaseValue = 0;
			ParentObject.pBrain.SetFeeling(GO, 200);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CheckingHostilityTowardsPlayer")
		{
			ScanForScrambler(IComponent<GameObject>.ThePlayer);
		}
		return base.FireEvent(E);
	}
}

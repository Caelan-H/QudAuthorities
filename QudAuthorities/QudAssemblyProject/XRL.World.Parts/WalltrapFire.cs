using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class WalltrapFire : IPart
{
	public int JetLength = 6;

	public string JetDamage = "3d5+12";

	public string JetTemperature = "2d50+200";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WalltrapTrigger");
		base.Register(Object);
	}

	public void Flame(Cell C)
	{
		string jetDamage = JetDamage;
		if (C == null)
		{
			return;
		}
		foreach (GameObject item in C.GetObjectsWithPart("Combat", (GameObject GO) => GO.PhaseMatches(ParentObject)))
		{
			item.TemperatureChange(JetTemperature.RollCached(), ParentObject);
			if (25.in100() && IComponent<GameObject>.Visible(item))
			{
				item.Smoke();
			}
		}
		foreach (GameObject objectsViaEvent in C.GetObjectsViaEventList())
		{
			if (objectsViaEvent.HasPart("Combat") || objectsViaEvent.HasPart("CrossFlameOnStep") || objectsViaEvent.HasPart("BurnOffGas"))
			{
				Damage damage = new Damage(jetDamage.RollCached());
				damage.AddAttribute("Fire");
				damage.AddAttribute("Heat");
				Event @event = Event.New("TakeDamage");
				@event.SetParameter("Damage", damage);
				@event.SetParameter("Owner", ParentObject);
				@event.SetParameter("Attacker", ParentObject);
				@event.SetParameter("Message", "from a {{fiery|jet of flames}}!");
				objectsViaEvent.FireEvent(@event);
			}
		}
	}

	public void FireJet(string D)
	{
		Cell cellFromDirection = ParentObject.CurrentCell;
		for (int i = 0; i < JetLength; i++)
		{
			cellFromDirection = cellFromDirection.GetCellFromDirection(D);
			if (cellFromDirection == null)
			{
				break;
			}
			Flame(cellFromDirection);
			if (cellFromDirection.ParentZone.IsActive() && cellFromDirection.IsVisible())
			{
				for (int j = 0; j < 3; j++)
				{
					string text = "&C";
					int num = Stat.Random(1, 3);
					if (num == 1)
					{
						text = "&R";
					}
					if (num == 2)
					{
						text = "&r";
					}
					if (num == 3)
					{
						text = "&W";
					}
					int num2 = Stat.Random(1, 3);
					if (num2 == 1)
					{
						text += "^R";
					}
					if (num2 == 2)
					{
						text += "^r";
					}
					if (num2 == 3)
					{
						text += "^W";
					}
					XRLCore.ParticleManager.Add(text + (char)(219 + Stat.Random(0, 4)), cellFromDirection.X, cellFromDirection.Y, 0f, 0f, 10 + 2 * i + (6 - 2 * j));
				}
			}
			if (cellFromDirection.IsSolid(ForFluid: true))
			{
				break;
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WalltrapTrigger")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null && !IsBroken() && !IsRusted() && !IsEMPed())
			{
				string[] cardinalDirectionList = Directions.CardinalDirectionList;
				foreach (string text in cardinalDirectionList)
				{
					Cell cellFromDirection = cell.GetCellFromDirection(text);
					if (cellFromDirection != null && !cellFromDirection.IsSolid(ForFluid: true))
					{
						FireJet(text);
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}

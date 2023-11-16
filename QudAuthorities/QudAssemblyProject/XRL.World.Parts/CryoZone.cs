using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class CryoZone : IPart
{
	public GameObject Owner;

	public int nFrameOffset = Stat.RandomCosmetic(0, 60);

	public int Duration = 3;

	public int Turn = 1;

	public int Level = 1;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GeneralAmnestyEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Owner = null;
		return base.HandleEvent(E);
	}

	public override bool FinalRender(RenderEvent E, bool bAlt)
	{
		if (bAlt || !Visible())
		{
			return true;
		}
		int num = (XRLCore.CurrentFrame + nFrameOffset) % 60;
		if (num < 15)
		{
			E.BackgroundString = "^C";
			E.DetailColor = "C";
		}
		else if (num < 30)
		{
			E.BackgroundString = "^c";
			E.DetailColor = "c";
		}
		else if (num < 45)
		{
			E.BackgroundString = "^Y";
			E.DetailColor = "Y";
		}
		else
		{
			E.BackgroundString = "^c";
			E.DetailColor = "c";
		}
		if (Stat.RandomCosmetic(1, 5) == 1)
		{
			E.RenderString = "°";
			E.ColorString = "&C";
		}
		else if (Stat.RandomCosmetic(1, 5) == 1)
		{
			E.RenderString = "±";
			E.ColorString = "&Y";
		}
		else
		{
			E.RenderString = "±";
			E.ColorString = "&c";
		}
		ParentObject.pRender.ColorString = "&Y^C";
		return base.FinalRender(E, bAlt);
	}

	public override bool Render(RenderEvent E)
	{
		int num = (XRLCore.CurrentFrame + nFrameOffset) % 60;
		if (num < 15)
		{
			E.RenderString = "°";
			E.BackgroundString = "^C";
		}
		else if (num < 30)
		{
			E.RenderString = "±";
			E.BackgroundString = "^c";
		}
		else if (num < 45)
		{
			E.RenderString = "²";
			E.BackgroundString = "^Y";
		}
		else
		{
			E.RenderString = "Û";
			E.BackgroundString = "^C";
		}
		ParentObject.pRender.ColorString = "&Y^C";
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "FinalRender");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			GameObject.validate(ref Owner);
			if (Duration > 0)
			{
				foreach (GameObject item in ParentObject.CurrentCell.GetObjectsWithPartReadonly("Physics"))
				{
					if (item != ParentObject)
					{
						Damage damage = new Damage((int)Math.Ceiling((float)Stat.Roll(Level + "d" + (Turn + 1)) / 2f));
						damage.AddAttribute("Ice");
						damage.AddAttribute("Cold");
						Event @event = Event.New("TakeDamage");
						@event.SetParameter("Damage", damage);
						@event.SetParameter("Owner", Owner);
						@event.SetParameter("Attacker", Owner);
						@event.SetParameter("Message", "from %o cryokinesis!");
						item.FireEvent(@event);
						item.TemperatureChange((-20 - 60 * Level) / 2, Owner);
					}
				}
				ParentObject.TemperatureChange((-20 - 60 * Level) / 2, Owner);
				Turn++;
			}
			Duration--;
			if (Duration == 0)
			{
				ParentObject.Destroy();
				return false;
			}
		}
		return base.FireEvent(E);
	}
}

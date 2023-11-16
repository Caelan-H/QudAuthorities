using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class PyroZone : IPart
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
		if (!base.WantEvent(ID, cascade) && ID != GeneralAmnestyEvent.ID)
		{
			return ID == RadiatesHeatEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Owner = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RadiatesHeatEvent E)
	{
		if (Duration > 0)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
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
			E.BackgroundString = "^W";
			E.DetailColor = "W";
		}
		else if (num < 30)
		{
			E.BackgroundString = "^r";
			E.DetailColor = "r";
		}
		else if (num < 45)
		{
			E.BackgroundString = "^R";
			E.DetailColor = "R";
		}
		else
		{
			E.BackgroundString = "^r";
			E.DetailColor = "r";
		}
		if (Stat.RandomCosmetic(1, 5) == 1)
		{
			E.RenderString = "°";
			E.ColorString = "&W";
		}
		else if (Stat.RandomCosmetic(1, 5) == 1)
		{
			E.RenderString = "±";
			E.ColorString = "&R";
		}
		else
		{
			E.RenderString = "±";
			E.ColorString = "&r";
		}
		ParentObject.pRender.ColorString = "&r^r";
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		int num = (XRLCore.CurrentFrame + nFrameOffset) % 60;
		if (num < 15)
		{
			E.RenderString = "°";
			E.BackgroundString = "^W";
		}
		else if (num < 30)
		{
			E.RenderString = "±";
			E.BackgroundString = "^r";
		}
		else if (num < 45)
		{
			E.RenderString = "²";
			E.BackgroundString = "^R";
		}
		else
		{
			E.RenderString = "Û";
			E.BackgroundString = "^r";
		}
		ParentObject.pRender.ColorString = "&r^r";
		return true;
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
						string dice = "";
						if (Turn == 1)
						{
							dice = Level + "d3";
						}
						if (Turn == 2)
						{
							dice = Level + "d4";
						}
						if (Turn == 3)
						{
							dice = Level + "d6";
						}
						Damage damage = new Damage((int)Math.Ceiling((float)Stat.Roll(dice) / 2f));
						damage.AddAttribute("Heat");
						damage.AddAttribute("Fire");
						Event @event = Event.New("TakeDamage");
						@event.SetParameter("Damage", damage);
						@event.SetParameter("Owner", Owner);
						@event.SetParameter("Attacker", Owner);
						@event.SetParameter("Message", "from %o pyrokinesis!");
						item.FireEvent(@event);
						item.TemperatureChange((310 + 30 * Level) / 2, Owner);
					}
				}
				ParentObject.TemperatureChange((310 + 30 * Level) / 2, Owner);
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

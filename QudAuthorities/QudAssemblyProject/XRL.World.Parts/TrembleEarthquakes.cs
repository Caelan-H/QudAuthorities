using System;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class TrembleEarthquakes : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && Stat.Random(1, 200) == 1 && ParentObject.CurrentZone.IsActive())
		{
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
			XRLCore.Core.RenderBaseToBuffer(scrapBuffer);
			scrapBuffer.Shake(500, 25, Popup._TextConsole);
			if (ParentObject.pPhysics.CurrentCell.ParentZone.Z > 10)
			{
				MessageQueue.AddPlayerMessage("The ground shakes violently and loose rock falls from the ceiling!");
			}
			else
			{
				MessageQueue.AddPlayerMessage("The ground shakes violently!");
			}
			int num = Stat.RollDamagePenetrations(IComponent<GameObject>.ThePlayer.Statistics["AV"].Value, 0, 0);
			if (num > 0 && (IComponent<GameObject>.ThePlayer.GetPhase() == 1 || IComponent<GameObject>.ThePlayer.GetPhase() == 1))
			{
				string resultColor = Stat.GetResultColor(num);
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					num2 += Stat.Roll("1d3");
				}
				Damage damage = new Damage(num2);
				damage.AddAttribute("Crushing");
				damage.AddAttribute("Cudgel");
				Event @event = Event.New("TakeDamage");
				@event.AddParameter("Damage", damage);
				@event.AddParameter("Owner", null);
				@event.AddParameter("Attacker", null);
				@event.AddParameter("Message", "from falling rocks! " + resultColor + "(x" + num + ")&y");
				IComponent<GameObject>.ThePlayer.FireEvent(@event);
			}
		}
		return base.FireEvent(E);
	}
}

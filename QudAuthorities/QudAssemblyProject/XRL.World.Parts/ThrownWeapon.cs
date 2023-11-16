using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class ThrownWeapon : IPart
{
	public string Damage = "1";

	public int Penetration = 1;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetDisplayNameEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !ParentObject.HasTagOrProperty("HideThrownWeaponPerformance"))
		{
			E.AddTag(GetPerformanceTag());
		}
		return base.HandleEvent(E);
	}

	public string GetPerformanceTag()
	{
		Event @event = Event.New("GetThrownWeaponPerformance");
		@event.SetParameter("Damage", Damage);
		@event.SetParameter("Penetrations", Penetration + 4);
		@event.SetParameter("Vorpal", 0);
		ParentObject.FireEvent(@event);
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("{{c|").Append('\u001a').Append("}}");
		if (@event.HasFlag("Vorpal"))
		{
			stringBuilder.Append('÷');
		}
		else
		{
			stringBuilder.Append(@event.GetIntParameter("Penetrations"));
		}
		stringBuilder.Append(" {{r|").Append('\u0003').Append("}}")
			.Append(@event.GetStringParameter("Damage"));
		return stringBuilder.ToString();
	}
}

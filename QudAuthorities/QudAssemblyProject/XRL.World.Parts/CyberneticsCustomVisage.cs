using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCustomVisage : IPart
{
	public string Faction;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ApplyVisage(E.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		UnapplyVisage(E.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Faction != null)
		{
			E.Postfix.AppendRules("+300 reputation with " + XRL.World.Faction.getFormattedName(Faction));
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void ApplyVisage(GameObject target)
	{
		if (string.IsNullOrEmpty(Faction))
		{
			List<Faction> list = new List<Faction>();
			List<string> list2 = new List<string>();
			foreach (Faction item in Factions.loop())
			{
				if (item.Visible)
				{
					list.Add(item);
				}
			}
			list.Sort((Faction a, Faction b) => a.DisplayName.CompareTo(b.DisplayName));
			foreach (Faction item2 in list)
			{
				list2.Add(item2.DisplayName);
			}
			if (target.IsPlayer())
			{
				Popup.Show("Choose a model faction for your facial reconstruction.");
				int num = Popup.ShowOptionList("", list2.ToArray());
				if (num == -1)
				{
					num = Stat.Random(0, list.Count - 1);
				}
				Faction = list[num].Name;
			}
			else
			{
				Faction randomElement = list.GetRandomElement();
				Faction = randomElement.Name;
			}
		}
		if (target != null && target.IsPlayer())
		{
			The.Game?.PlayerReputation?.modify(Faction, 300, null, null, silent: true, transient: true);
		}
	}

	private void UnapplyVisage(GameObject target)
	{
		if (Faction != null && target != null && target.IsPlayer())
		{
			The.Game?.PlayerReputation?.modify(Faction, -300, null, null, silent: true, transient: true);
		}
	}
}

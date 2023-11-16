#define NLOG_ALL
using System;
using System.Collections.Generic;
using XRL.World.Skills;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsTreeSkillsoft : IPart
{
	public int MinCost;

	public int MaxCost = 50;

	public string Skill;

	public bool Applied;

	public string PowersAdded = "";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ImplantedEvent.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		SkillEntry skillEntry = SkillFactory.Factory.SkillByClass[Skill];
		PowersAdded = "";
		foreach (PowerEntry value in skillEntry.Powers.Values)
		{
			if (string.IsNullOrEmpty(value.Class) || E.Implantee.HasPart(value.Class))
			{
				continue;
			}
			try
			{
				E.Implantee.AddSkill(value.Class);
			}
			catch (Exception ex)
			{
				if (value.Class != null)
				{
					Logger.gameLog.Error("Invalid Skillsoft skill class: " + value.Class);
				}
				Logger.Exception(ex);
			}
			if (PowersAdded != "")
			{
				PowersAdded += ",";
			}
			PowersAdded += value.Class;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		if (!string.IsNullOrEmpty(PowersAdded))
		{
			string[] array = PowersAdded.Split(',');
			foreach (string text in array)
			{
				if (!string.IsNullOrEmpty(text))
				{
					E.Implantee.RemoveSkill(text);
				}
			}
			PowersAdded = "";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Skill == null)
		{
			List<SkillEntry> list = new List<SkillEntry>();
			foreach (SkillEntry value in SkillFactory.Factory.SkillList.Values)
			{
				if (value.Initiatory != true)
				{
					list.Add(value);
				}
			}
			if (list.Count > 0)
			{
				SkillEntry randomElement = list.GetRandomElement();
				ParentObject.pRender.DisplayName = "{{Y|Skillsoft Plus [{{W|" + randomElement.Name + "}}]}}";
				Skill = randomElement.Class;
				int num = randomElement.Cost;
				foreach (PowerEntry value2 in randomElement.Powers.Values)
				{
					num += value2.Cost;
				}
				(ParentObject.GetPart("CyberneticsBaseItem") as CyberneticsBaseItem).Cost = num / 100;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}

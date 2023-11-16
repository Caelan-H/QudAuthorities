using System;
using System.Collections.Generic;
using XRL.World.Skills;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsSingleSkillsoft : IPart
{
	public int MinCost;

	public int MaxCost = 50;

	public string Skill;

	public bool Applied;

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
		if (!E.Implantee.HasPart(Skill))
		{
			E.Implantee.AddSkill(Skill);
			Applied = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		if (Applied)
		{
			E.Implantee?.RemoveSkill(Skill);
			Applied = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Skill == null)
		{
			List<PowerEntry> list = new List<PowerEntry>();
			foreach (SkillEntry value in SkillFactory.Factory.SkillList.Values)
			{
				if (value.Initiatory == true)
				{
					continue;
				}
				foreach (PowerEntry value2 in value.Powers.Values)
				{
					if (value2.Cost >= MinCost && value2.Cost <= MaxCost && !string.IsNullOrEmpty(value2.Class))
					{
						list.Add(value2);
					}
				}
			}
			if (list.Count > 0)
			{
				PowerEntry randomElement = list.GetRandomElement();
				ParentObject.pRender.DisplayName = "{{Y|Skillsoft [{{W|" + randomElement.Name + "}}]}}";
				Skill = randomElement.Class;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}

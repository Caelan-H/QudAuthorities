using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XRL.Messages;
using XRL.UI;
using XRL.Wish;
using XRL.World.Parts.Skill;
using XRL.World.Skills;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class Skills : IPart
{
	[NonSerialized]
	public List<BaseSkill> SkillList = new List<BaseSkill>();

	[NonSerialized]
	private static Dictionary<string, BaseSkill> GenericSkills = new Dictionary<string, BaseSkill>(32);

	public static BaseSkill GetGenericSkill(string Skill, GameObject Actor = null)
	{
		BaseSkill value = Actor?.GetPart(Skill) as BaseSkill;
		if (value != null)
		{
			return value;
		}
		if (GenericSkills.TryGetValue(Skill, out value))
		{
			return value;
		}
		Type type = ModManager.ResolveType("XRL.World.Parts.Skill." + Skill);
		if (type == null)
		{
			Debug.LogWarning("Cannot resolve skill type for " + Skill);
			return null;
		}
		if (!(Activator.CreateInstance(type) is BaseSkill baseSkill))
		{
			Debug.LogWarning(Skill + " is not a skill");
			return null;
		}
		GenericSkills[Skill] = baseSkill;
		return baseSkill;
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		Skills obj = base.DeepCopy(Parent) as Skills;
		obj.SkillList = new List<BaseSkill>(SkillList);
		return obj;
	}

	public override void SaveData(SerializationWriter Writer)
	{
		base.SaveData(Writer);
		Writer.Write(SkillList.Count);
	}

	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
		Reader.ReadInt32();
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public void AddSkill(BaseSkill NewSkill)
	{
		if (ParentObject.HasSkill(NewSkill.Name))
		{
			return;
		}
		BeforeAddSkillEvent.Send(ParentObject, NewSkill);
		NewSkill.ParentObject = ParentObject;
		if (NewSkill.AddSkill(ParentObject))
		{
			ParentObject.AddPart(NewSkill);
			SkillList.Add(NewSkill);
		}
		if (SkillFactory.Factory.SkillByClass.TryGetValue(NewSkill.Name, out var value))
		{
			foreach (PowerEntry value2 in value.Powers.Values)
			{
				if (value2.Cost == 0)
				{
					Type type = ModManager.ResolveType("XRL.World.Parts.Skill." + value2.Class);
					AddSkill(Activator.CreateInstance(type) as BaseSkill);
				}
			}
		}
		AfterAddSkillEvent.Send(ParentObject, NewSkill);
	}

	public void RemoveSkill(BaseSkill Skill)
	{
		BeforeRemoveSkillEvent.Send(ParentObject, Skill);
		if (Skill.RemoveSkill(ParentObject))
		{
			ParentObject.RemovePart(Skill);
			SkillList.Remove(Skill);
		}
		AfterRemoveSkillEvent.Send(ParentObject, Skill);
	}

	[WishCommand(null, null, Command = "skill")]
	public static void WishSkill(string argument)
	{
		if (WishSkillAdd(argument))
		{
			return;
		}
		foreach (SkillEntry value in SkillFactory.Factory.SkillByClass.Values)
		{
			if (value.Name.EqualsNoCase(argument) && WishSkillAdd(value.Class))
			{
				return;
			}
		}
		foreach (PowerEntry value2 in SkillFactory.Factory.PowersByClass.Values)
		{
			if (value2.Name.EqualsNoCase(argument) && WishSkillAdd(value2.Class))
			{
				return;
			}
		}
		Popup.Show("No skill by that name could be found.");
	}

	[WishCommand(null, null, Command = "allskills")]
	public static void WishSkillAll()
	{
		Popup.bSuppressPopups = true;
		MessageQueue.Suppress = true;
		foreach (SkillEntry value in SkillFactory.Factory.SkillByClass.Values)
		{
			WishSkillAdd(value.Class);
		}
		foreach (PowerEntry value2 in SkillFactory.Factory.PowersByClass.Values)
		{
			WishSkillAdd(value2.Class);
		}
		Popup.bSuppressPopups = false;
		MessageQueue.Suppress = false;
		IComponent<GameObject>.XDidY(IComponent<GameObject>.ThePlayer, "gain", "all the skills", "!", null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
	}

	private static bool WishSkillAdd(string Class)
	{
		if (!(IComponent<GameObject>.ThePlayer?.GetPart("Skills") is Skills skills))
		{
			return false;
		}
		Type type = ModManager.ResolveType("XRL.World.Parts.Skill." + Class, IgnoreCase: true);
		if (type == null)
		{
			return false;
		}
		if (!(Activator.CreateInstance(type) is BaseSkill baseSkill))
		{
			return false;
		}
		IComponent<GameObject>.XDidY(IComponent<GameObject>.ThePlayer, "gain", "the skill " + baseSkill.DisplayName, "!", null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		skills.AddSkill(baseSkill);
		return true;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (BaseSkill skill in SkillList)
		{
			stringBuilder.Append(skill.Name).Append('\n');
		}
		return stringBuilder.ToString();
	}
}

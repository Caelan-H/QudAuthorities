using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Snapjaw_Howl : BaseSkill
{
	public int Radius = 5;

	public string Duration = "10-20";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandSnapjawHowl");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandSnapjawHowl");
			}
		}
		else if (E.ID == "CommandSnapjawHowl")
		{
			if (!ParentObject.IsValid() || ParentObject.CurrentCell == null)
			{
				return false;
			}
			DidX("howl", null, "!", null, ParentObject);
			List<GameObject> list = new List<GameObject>(16);
			foreach (Cell localAdjacentCell in ParentObject.CurrentCell.GetLocalAdjacentCells(Radius))
			{
				GameObject firstObjectWithPropertyOrTag = localAdjacentCell.GetFirstObjectWithPropertyOrTag("Snapjaw", (GameObject obj) => !obj.HasEffect("Frenzied"));
				if (firstObjectWithPropertyOrTag == null)
				{
					continue;
				}
				bool flag = false;
				if (firstObjectWithPropertyOrTag.IsVisible())
				{
					if (firstObjectWithPropertyOrTag.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You are frenzied by the howl!", 'G');
					}
					else
					{
						flag = true;
					}
				}
				if (firstObjectWithPropertyOrTag.ApplyEffect(new Frenzied(Duration.RollCached())) && flag)
				{
					list.Add(firstObjectWithPropertyOrTag);
				}
			}
			if (list.Count > 0)
			{
				int num = 0;
				int num2 = 0;
				GameObject gameObject = null;
				Dictionary<string, int> dictionary = new Dictionary<string, int>(list.Count);
				Dictionary<string, GameObject> dictionary2 = new Dictionary<string, GameObject>(list.Count);
				foreach (GameObject item in list)
				{
					string shortDisplayName = item.ShortDisplayName;
					if (dictionary.ContainsKey(shortDisplayName))
					{
						dictionary[shortDisplayName]++;
					}
					else
					{
						dictionary.Add(shortDisplayName, 1);
					}
					if (!dictionary2.ContainsKey(shortDisplayName))
					{
						dictionary2.Add(shortDisplayName, item);
					}
					if (gameObject == null)
					{
						gameObject = item;
					}
					num++;
					if (item.IsHostileTowards(IComponent<GameObject>.ThePlayer))
					{
						num2++;
					}
				}
				List<string> list2 = new List<string>(dictionary.Count);
				StringBuilder stringBuilder = Event.NewStringBuilder();
				foreach (string key in dictionary.Keys)
				{
					int num3 = dictionary[key];
					stringBuilder.Length = 0;
					if (num3 > 1)
					{
						stringBuilder.Append(Grammar.Cardinal(num3)).Append(' ').Append(Grammar.Pluralize(key));
					}
					else
					{
						stringBuilder.Append(dictionary2[key].a).Append(key);
					}
					list2.Add(stringBuilder.ToString());
				}
				string value = ((num2 >= num / 2) ? "&R" : ((num2 <= 0) ? "&g" : "&r"));
				stringBuilder.Length = 0;
				stringBuilder.Append(value).Append(ColorUtility.CapitalizeExceptFormatting(Grammar.MakeAndList(list2))).Append(value)
					.Append((num == 1) ? gameObject.Is : " are")
					.Append(" frenzied by the howl!");
				IComponent<GameObject>.AddPlayerMessage(stringBuilder.ToString());
			}
			ParentObject.Soundwave();
			CooldownMyActivatedAbility(ActivatedAbilityID, 100);
			ParentObject.UseEnergy(1000, "Skill Howl");
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Howl", "CommandSnapjawHowl", "Skill", null, "\u000e");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}

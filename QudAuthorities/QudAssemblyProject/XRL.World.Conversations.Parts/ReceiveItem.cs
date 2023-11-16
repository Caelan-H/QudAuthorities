using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class ReceiveItem : IConversationPart
{
	public string Table;

	public string Blueprints;

	public string Identify;

	public string Mods = "0";

	public bool Pick;

	public bool FromSpeaker;

	public ReceiveItem()
	{
	}

	public ReceiveItem(string Blueprints)
	{
		this.Blueprints = Blueprints;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		List<PopulationResult> list = (Table.IsNullOrEmpty() ? new List<PopulationResult>() : PopulationManager.Generate(Table));
		if (!Blueprints.IsNullOrEmpty())
		{
			string[] array = Blueprints.Split(',');
			foreach (string text in array)
			{
				int num = text.IndexOf(':');
				if (num < 0)
				{
					list.Add(new PopulationResult(text));
				}
				else
				{
					list.Add(new PopulationResult(text.Substring(0, num), text.Substring(num + 1).RollCached()));
				}
			}
		}
		List<GameObject> list2 = new List<GameObject>(list.Count);
		bool flag = Identify == "*" || Identify.EqualsNoCase("all");
		for (int j = 0; j < list.Count; j++)
		{
			for (int num2 = list[j].Number; num2 > 0; num2--)
			{
				ParseItemHints(list[j], out var ModChance);
				int setModNumber = Mods.RollCached();
				GameObject gameObject = (FromSpeaker ? The.Speaker.HasItemWithBlueprint(list[j].Blueprint) : null);
				if (gameObject == null)
				{
					gameObject = GameObjectFactory.Factory.CreateObject(list[j].Blueprint, ModChance, setModNumber);
				}
				if (flag || (!Identify.IsNullOrEmpty() && Identify.Contains(list[j].Blueprint)))
				{
					gameObject.MakeUnderstood();
				}
				if (num2 > 1 && gameObject.CanGenerateStacked())
				{
					gameObject.Count = num2;
					num2 = 0;
				}
				list2.Add(gameObject);
			}
		}
		if (Pick && list2.Count > 1)
		{
			The.Player.ReceiveObject(Popup.PickGameObject("Choose a reward", list2));
		}
		else if (list2.Count > 0)
		{
			List<string> list3 = new List<string>(list.Count);
			for (int k = 0; k < list2.Count; k++)
			{
				list3.Add(list2[k].an());
				The.Player.ReceiveObject(list2[k]);
			}
			Popup.Show("You receive " + Grammar.MakeAndList(list3) + "!");
		}
		return base.HandleEvent(E);
	}

	private void ParseItemHints(PopulationResult Result, out int ModChance)
	{
		ModChance = 0;
		if (Result.Hint != null)
		{
			int num = Result.Hint.IndexOf("SetBonusModChance:", StringComparison.Ordinal);
			if (num >= 0)
			{
				num += 18;
				int num2 = Result.Hint.IndexOf(',', num);
				string dice = ((num2 < 0) ? Result.Hint.Substring(num) : Result.Hint.Substring(num, num2 - num));
				ModChance = dice.RollCached();
			}
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class TakeItem : IConversationPart
{
	public string Blueprints;

	public string IDs;

	public string Amount = "1";

	public bool Unsellable = true;

	public bool ClearQuest;

	public bool Destroy;

	public bool Require = true;

	public TakeItem()
	{
		Priority = -1000;
	}

	public TakeItem(string Blueprints)
	{
		this.Blueprints = Blueprints;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && (!Require || ID != EnterElementEvent.ID))
		{
			if (!Require)
			{
				return ID == EnteredElementEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		if (Execute())
		{
			return base.HandleEvent(E);
		}
		return false;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		Execute();
		return base.HandleEvent(E);
	}

	public bool Execute()
	{
		List<GameObject> contents = The.Player.GetContents();
		string[] source = Blueprints?.Split(',') ?? new string[0];
		string[] source2 = IDs?.Split(',') ?? new string[0];
		bool flag = Amount == "*" || Amount.EqualsNoCase("all");
		int num = (flag ? int.MaxValue : Amount.RollCached());
		int i = 0;
		for (int count = contents.Count; i < count && num > 0; i++)
		{
			GameObject gameObject = contents[i];
			if (!source.Contains(gameObject.Blueprint) && !source2.Contains(gameObject.GetStringProperty("id")))
			{
				continue;
			}
			int num2 = 1;
			if (gameObject.GetPart("Stacker") is Stacker stacker)
			{
				num2 = stacker.Number;
				if (num2 > num)
				{
					stacker.SplitStack(num, The.Player);
					num2 = num;
				}
			}
			if (Destroy)
			{
				if (!gameObject.TryRemoveFromContext())
				{
					Popup.Show("You cannot give " + gameObject.t() + "!");
					continue;
				}
			}
			else
			{
				if (!The.Speaker.ReceiveObject(gameObject))
				{
					Popup.Show("You cannot give " + gameObject.t() + "!");
					The.Player.ReceiveObject(gameObject);
					continue;
				}
				Popup.Show(The.Speaker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + The.Speaker.GetVerb("take") + " " + gameObject.t() + ".");
				if (Unsellable)
				{
					gameObject.SetIntProperty("WontSell", 1);
				}
				if (ClearQuest)
				{
					gameObject.pPhysics.Category = gameObject.GetStringProperty("OriginalCategory") ?? gameObject.GetBlueprint().GetPartParameter("Physics", "Category") ?? "Miscellaneous";
					gameObject.RemoveProperty("QuestItem");
					gameObject.RemoveProperty("NoAIEquip");
				}
			}
			num -= num2;
		}
		if (!flag)
		{
			return num <= 0;
		}
		return true;
	}
}

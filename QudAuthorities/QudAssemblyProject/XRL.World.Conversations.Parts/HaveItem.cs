using System.Collections.Generic;
using System.Linq;
using XRL.Language;

namespace XRL.World.Conversations.Parts;

public class HaveItem : IConversationPart
{
	public string Blueprints;

	public string IDs;

	public int Amount = 1;

	public bool Require;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && (!Require || ID != EnterElementEvent.ID))
		{
			if (!Require)
			{
				return ID == IsElementVisibleEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(IsElementVisibleEvent E)
	{
		if (!PlayerHasAmount())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		if (!PlayerHasAmount())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool PlayerHasAmount()
	{
		List<GameObject> contents = The.Player.GetContents();
		string[] array = Blueprints?.Split(',') ?? new string[0];
		string[] source = IDs?.Split(',') ?? new string[0];
		int num = Amount;
		int i = 0;
		for (int count = contents.Count; i < count; i++)
		{
			if (num <= 0)
			{
				break;
			}
			GameObject gameObject = contents[i];
			if (array.Contains(gameObject.Blueprint) || source.Contains(gameObject.GetStringProperty("id")))
			{
				num -= gameObject.Count;
				if (num <= 0)
				{
					return true;
				}
			}
		}
		if (Require && array.Length != 0)
		{
			GameObject gameObject2 = GameObjectFactory.Factory.CreateSampleObject(array[0]);
			The.Player.ShowFailure((Amount > 1) ? ("You do not have enough " + Grammar.Pluralize(gameObject2.DisplayNameOnlyDirectAndStripped) + ".") : ("You do not have " + gameObject2.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + "."));
		}
		return false;
	}
}

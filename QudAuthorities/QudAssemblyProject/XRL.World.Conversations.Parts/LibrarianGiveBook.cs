using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Qud.API;
using XRL.Language;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class LibrarianGiveBook : IConversationPart
{
	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != EnterElementEvent.ID)
		{
			return ID == HideElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{g|[Give Books]}}";
		return false;
	}

	public static bool IsAwardableBook(GameObject Object)
	{
		if (Object.HasIntProperty("LibrarianAwarded"))
		{
			return false;
		}
		if (Object.HasPart("Book"))
		{
			return true;
		}
		if (Object.HasPart("VillageHistoryBook"))
		{
			return true;
		}
		if (Object.HasPart("MarkovBook"))
		{
			return true;
		}
		if (Object.HasPart("Cookbook"))
		{
			return true;
		}
		return false;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		Inventory inventory = The.Player.Inventory;
		List<string> list = new List<string>();
		List<GameObject> list2 = new List<GameObject>();
		List<int> list3 = new List<int>();
		List<char> list4 = new List<char>();
		List<IRenderable> list5 = new List<IRenderable>();
		bool flag = false;
		char c = 'a';
		foreach (GameObject @object in inventory.GetObjects(IsAwardableBook))
		{
			if (@object.IsMarkedImportantByPlayer())
			{
				flag = true;
				continue;
			}
			double valueEach = @object.ValueEach;
			int num = (int)(valueEach * valueEach / 25.0);
			if (num > 0)
			{
				list2.Add(@object);
				list3.Add(num);
				list4.Add((c <= 'z') ? c++ : ' ');
				list5.Add(@object.pRender);
				list.Add(@object.GetDisplayName(1120, null, null, AsIfKnown: false, Single: true) + " [{{C|" + num + "}} XP]");
			}
		}
		if (list2.Count == 0)
		{
			return The.Player.ShowFailure(flag ? "You only have books you've marked important. Unmark any you wish to donate." : "You have no books to give.");
		}
		List<int> list6 = Popup.PickSeveral("Choose books to give.", list.ToArray(), list4.ToArray(), -1, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true, 0, "", null, Icons: list5.ToArray(), Context: The.Speaker);
		if (list6.IsNullOrEmpty())
		{
			return false;
		}
		int num2 = 0;
		List<GameObject> list7 = new List<GameObject>(list6.Count);
		string displayName = The.Speaker.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: true, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: true, Short: false, BaseOnly: true);
		string text = The.Speaker.DefiniteArticle(capital: true, displayName, forBase: true);
		string text2 = The.Speaker.IndefiniteArticle(capital: true, displayName, forBase: true);
		foreach (int item in list6)
		{
			GameObject gameObject = list2[item];
			if (gameObject.ConfirmUseImportant(The.Player, "donate"))
			{
				gameObject.SplitStack(1, The.Player);
				GameObject speaker = The.Speaker;
				if (speaker != null && speaker.ReceiveObject(gameObject))
				{
					num2 += list3[item];
					list7.Add(gameObject);
					JournalAPI.AddAccomplishment(text2 + displayName + " provided you with insightful commentary on " + gameObject.ShortDisplayName + ".", "Remember the kindness of =name=, who patiently taught " + gameObject.ShortDisplayName + " to " + The.Player.GetPronounProvider().PossessiveAdjective + " simple pupil, " + text2 + displayName + ".", "general", JournalAccomplishment.MuralCategory.LearnsSecret, JournalAccomplishment.MuralWeight.Low, null, -1L);
					gameObject.SetIntProperty("LibrarianAwarded", 1);
				}
			}
		}
		string text3 = Grammar.MakeAndList(list7.Select((GameObject x) => "'" + x.ShortDisplayName + "'").ToList());
		Popup.Show(text + displayName + " provides some insightful commentary on " + text3 + ".");
		Popup.Show("You gain {{C|" + num2 + "}} XP.", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
		The.Player.AwardXP(num2, -1, 0, int.MaxValue, null, The.Speaker);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(HideElementEvent E)
	{
		return false;
	}
}

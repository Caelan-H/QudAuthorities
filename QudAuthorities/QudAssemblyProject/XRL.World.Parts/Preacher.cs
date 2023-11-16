using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Preacher : IPart
{
	public int Volume = 20;

	public int Chance = 8;

	public int ChatWait = 350;

	public string Range = "10";

	public string Duration = "350";

	public string Book = "Quotes";

	public string Filter;

	public string FilterExtras;

	public string Prefix = "The preacher yells &W'";

	public string Frozen = "You hear inaudible mumbling.";

	public string Postfix = "'";

	public bool SmartUse = true;

	public bool inOrder;

	private int line = -1;

	public int LastTalk;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanSmartUse");
		Object.RegisterPartEvent(this, "CommandSmartUseEarly");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public void PreacherHomily(GameObject who, bool Dialog)
	{
		if ((!Dialog && !ParentObject.IsAudible(IComponent<GameObject>.ThePlayer, Volume)) || !ParentObject.FireEvent("CanPreach"))
		{
			return;
		}
		if (ParentObject.IsFrozen())
		{
			EmitMessage(Frozen, Dialog);
			return;
		}
		List<BookPage> list = BookUI.Books[Book];
		if (list.Count <= 0)
		{
			return;
		}
		if (!inOrder || line == -1)
		{
			line = Stat.Random(0, list.Count - 1);
		}
		string text = list[line].FullText.Replace("\n", " ").Replace("  ", " ").Trim();
		if (Filter != null)
		{
			text = TextFilters.Filter(text, Filter, FilterExtras);
		}
		IComponent<GameObject>.EmitMessage(who ?? ParentObject, Prefix + text + Postfix, Dialog);
		if (text.EndsWith(".") && !text.EndsWith("..."))
		{
			text = text.Substring(0, text.Length - 1);
		}
		if (!Dialog)
		{
			ParentObject.ParticleText("{{W|'" + text + "'}}", IgnoreVisibility: true);
		}
		if (inOrder)
		{
			if (line < list.Count - 1)
			{
				line++;
			}
			else
			{
				line = 0;
			}
		}
	}

	public void PreacherHomily(bool Dialog)
	{
		PreacherHomily(null, Dialog);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			if (SmartUse && !ParentObject.IsPlayerLed() && ConversationScript.IsPhysicalConversationPossible(ParentObject))
			{
				return false;
			}
		}
		else if (E.ID == "CommandSmartUseEarly")
		{
			if (SmartUse && ConversationScript.IsPhysicalConversationPossible(ParentObject))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("User");
				if (gameObjectParameter.IsPlayer())
				{
					if (!ParentObject.IsPlayerLed())
					{
						PreacherHomily(gameObjectParameter, Dialog: true);
					}
				}
				else
				{
					PreacherHomily(Dialog: false);
				}
			}
		}
		else if (E.ID == "BeginTakeAction" && ParentObject.InActiveZone())
		{
			LastTalk--;
			if (LastTalk < 0 && Chance.in100())
			{
				LastTalk = ChatWait;
				PreacherHomily(Dialog: false);
			}
		}
		return base.FireEvent(E);
	}
}

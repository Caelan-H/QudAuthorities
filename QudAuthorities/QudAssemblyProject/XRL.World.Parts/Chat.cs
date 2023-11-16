using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class Chat : IPart
{
	public string Says = "Hi!";

	public bool ShowInShortDescription;

	public Chat()
	{
	}

	public Chat(string Says)
		: this()
	{
		this.Says = Says;
	}

	public Chat(string Says, bool ShowInShortDescription)
		: this(Says)
	{
		this.ShowInShortDescription = ShowInShortDescription;
	}

	public override bool SameAs(IPart p)
	{
		Chat chat = p as Chat;
		if (chat.Says != Says)
		{
			return false;
		}
		if (chat.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID)
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return ShowInShortDescription;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (!E.Actor.IsPlayerControlled())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			if (!ParentObject.IsPlayerLed())
			{
				PerformChat(E.Actor, Dialog: true);
				return false;
			}
			return base.HandleEvent(E);
		}
		PerformChat(E.Actor, Dialog: false);
		return false;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription)
		{
			string text = GameText.VariableReplace(Says, ParentObject);
			if (!string.IsNullOrEmpty(text))
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append(E.Infix);
				int length = stringBuilder.Length;
				if (text[0] == '*')
				{
					stringBuilder.Append('\n').Append(text.Substring(1));
				}
				else if (text[0] == '@')
				{
					string[] array = text.Substring(1).Split('~');
					foreach (string text2 in array)
					{
						if (text2[0] == '[')
						{
							stringBuilder.Append('\n').Append(ParentObject.It).Append(ParentObject.GetVerb("bear"))
								.Append(' ')
								.Append(text2.Replace("[", "").Replace("]", ""));
							continue;
						}
						stringBuilder.Append('\n').Append(ParentObject.It).Append(ParentObject.GetVerb("say"))
							.Append(", '")
							.Append(text2)
							.Append('\'');
						if (!text2.EndsWith(".") && !text2.EndsWith("!") && !text2.EndsWith("?"))
						{
							stringBuilder.Append('.');
						}
					}
				}
				else
				{
					string text3 = (text.Contains("~") ? text.Split('~').GetRandomElement() : text);
					if (!string.IsNullOrEmpty(text3))
					{
						if (text3[0] == '[')
						{
							stringBuilder.Append('\n').Append(ParentObject.It).Append(ParentObject.GetVerb("bear"))
								.Append(' ')
								.Append(text3.Replace("[", "").Replace("]", ""));
						}
						else
						{
							stringBuilder.Append('\n').Append(ParentObject.It).Append(ParentObject.GetVerb("read"))
								.Append(", '")
								.Append(text3)
								.Append('\'');
							if (!text3.EndsWith(".") && !text3.EndsWith("!") && !text3.EndsWith("?"))
							{
								stringBuilder.Append('.');
							}
						}
						stringBuilder.Append('\n');
					}
				}
				if (stringBuilder.Length != length)
				{
					E.Infix.Clear().Append(stringBuilder);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ObjectTalking");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectTalking")
		{
			PerformChat(IComponent<GameObject>.ThePlayer, Dialog: true);
			return false;
		}
		return base.FireEvent(E);
	}

	public void PerformChat(GameObject who, bool Dialog)
	{
		string text = GameText.VariableReplace(Says, ParentObject);
		if (string.IsNullOrEmpty(text) || (ParentObject.pBrain != null && !ConversationScript.IsPhysicalConversationPossible(ParentObject, ShowPopup: true)))
		{
			return;
		}
		GameObject what = who ?? ParentObject;
		if (text[0] == '*')
		{
			IComponent<GameObject>.EmitMessage(what, text.Substring(1), Dialog);
		}
		else if (text[0] == '@')
		{
			string[] array = text.Substring(1).Split('~');
			foreach (string text2 in array)
			{
				if (text2[0] == '[')
				{
					IComponent<GameObject>.EmitMessage(what, text2.Replace("[", "").Replace("]", ""), Dialog);
					continue;
				}
				IComponent<GameObject>.EmitMessage(what, ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("say") + ", '{{|" + text2 + "}}'.", Dialog);
			}
		}
		else
		{
			string text3 = (text.Contains("~") ? text.Split('~').GetRandomElement() : text);
			if (!string.IsNullOrEmpty(text3))
			{
				if (text3[0] == '[')
				{
					IComponent<GameObject>.EmitMessage(what, text3.Replace("[", "").Replace("]", ""), Dialog);
				}
				else
				{
					IComponent<GameObject>.EmitMessage(what, ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("say") + ", '{{|" + text3 + "}}'.", Dialog);
				}
			}
		}
		if (who.IsPlayer())
		{
			ParentObject.FireEvent("ChattingWithPlayer");
		}
	}

	public void PerformChat(bool Dialog)
	{
		PerformChat(null, Dialog);
	}
}

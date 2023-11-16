using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Messages;
using XRL.UI;
using XRL.World;

namespace Qud.API;

[Serializable]
public class IBaseJournalEntry
{
	protected static StringBuilder SB = new StringBuilder();

	public bool _revealed;

	public bool secretSold;

	public string secretid;

	public string history = "";

	public string text;

	public List<string> attributes = new List<string>();

	[NonSerialized]
	public static bool NotedPrompt = true;

	public bool revealed => _revealed;

	public bool Has(string att)
	{
		return attributes.Contains(att);
	}

	public bool TryGetAttribute(string Prefix, out string Value)
	{
		foreach (string attribute in attributes)
		{
			if (attribute.StartsWith(Prefix))
			{
				Value = attribute.Substring(Prefix.Length);
				return true;
			}
		}
		Value = null;
		return false;
	}

	public virtual string GetShortText()
	{
		return text;
	}

	public virtual string GetDisplayText()
	{
		if (history.Length > 0)
		{
			return text + "\n" + history;
		}
		return text;
	}

	public virtual void AppendHistory(string Line)
	{
		if (history.Length > 0)
		{
			history += "\n";
		}
		history += Line;
	}

	public virtual void Updated()
	{
	}

	public virtual void Forget(bool fast = false)
	{
		if (revealed && Forgettable())
		{
			if (!fast && XRLCore.Core?.Game?.Player?.Body != null)
			{
				XRLCore.Core.Game.Player.Body.FireEvent(Event.New("BeforeSecretForgotten", "Secret", this));
			}
			_revealed = false;
			if (!fast && XRLCore.Core?.Game?.Player?.Body != null)
			{
				XRLCore.Core.Game.Player.Body.FireEvent(Event.New("AfterSecretForgotten", "Secret", this));
			}
			Updated();
		}
	}

	public virtual void Reveal(bool silent = false)
	{
		if (!revealed)
		{
			if (XRLCore.Core?.Game?.Player?.Body != null)
			{
				XRLCore.Core.Game.Player.Body.FireEvent(Event.New("BeforeSecretRevealed", "Secret", this));
			}
			_revealed = true;
			if (XRLCore.Core?.Game?.Player?.Body != null)
			{
				XRLCore.Core.Game.Player.Body.FireEvent(Event.New("AfterSecretRevealed", "Secret", this));
			}
			Updated();
		}
	}

	public virtual bool Forgettable()
	{
		return true;
	}

	public static void DisplayMessage(string Message)
	{
		if (NotedPrompt)
		{
			Popup.Show(Message);
		}
		else
		{
			MessageQueue.AddPlayerMessage(Message);
		}
	}
}

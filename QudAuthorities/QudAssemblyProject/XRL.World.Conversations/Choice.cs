using System.Linq;

namespace XRL.World.Conversations;

[HasGameBasedStaticCache]
public class Choice : IConversationElement
{
	public bool Transient;

	[GameBasedStaticCache(CreateInstance = false)]
	private static FixedHashSet _Hashes;

	private ulong _Hash;

	private string _Target;

	public static FixedHashSet Hashes
	{
		get
		{
			if (_Hashes == null)
			{
				if (The.Game.ObjectGameState.TryGetValue("ConversationChoiceHashes", out var value))
				{
					_Hashes = new FixedHashSet((ulong[])value);
					The.Game.ObjectGameState.Remove("ConversationChoiceHashes");
				}
				else
				{
					_Hashes = new FixedHashSet();
				}
			}
			return _Hashes;
		}
	}

	public override int Propagation => 1;

	public ulong Hash
	{
		get
		{
			if (_Hash != 0)
			{
				return _Hash;
			}
			IConversationElement parent = Parent;
			while (parent.Parent != null)
			{
				parent = parent.Parent;
			}
			_Hash = FixedHashSet.SDBM(parent.ID ?? "[Unknown]", 0uL);
			_Hash = FixedHashSet.SDBM(ID, _Hash);
			_Hash = FixedHashSet.SDBM(Texts?.FirstOrDefault()?.Text ?? Text ?? "[Empty]", _Hash);
			return _Hash;
		}
	}

	public string Target
	{
		get
		{
			return _Target;
		}
		set
		{
			_Target = value;
			if (value == "End")
			{
				Priority -= 999999;
			}
		}
	}

	public static void StoreHashes()
	{
		if (_Hashes != null && _Hashes.Count != 0)
		{
			The.Game.ObjectGameState["ConversationChoiceHashes"] = _Hashes.GetValues();
		}
	}

	public override void Entered()
	{
		base.Entered();
		if (!Transient)
		{
			Hashes.Add(Hash);
		}
	}

	public override string GetTextColor()
	{
		string Color = "G";
		if (Hashes.Contains(Hash))
		{
			Color = "g";
		}
		ColorTextEvent.Send(this, ref Color);
		return Color;
	}

	public override string GetDisplayText(bool WithColor = false)
	{
		string text = base.GetDisplayText(WithColor);
		string tag = GetTag();
		if (!tag.IsNullOrEmpty())
		{
			text = text + " " + tag;
		}
		return text;
	}

	public string GetTag()
	{
		string text = GetChoiceTagEvent.For(this);
		if (!text.IsNullOrEmpty())
		{
			return text;
		}
		if (Target == "End")
		{
			return "{{K|[End]}}";
		}
		return null;
	}
}

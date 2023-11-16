using System;
using System.Text;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class MakersMark : IPart
{
	public string Mark = "";

	public string Desc = "This item bears a maker's mark.";

	public string Color = "R";

	public bool Initialized;

	public MakersMark()
	{
	}

	public MakersMark(string Mark, string Desc)
		: this()
	{
		this.Mark = Mark;
		this.Desc = Desc;
	}

	public override bool SameAs(IPart p)
	{
		MakersMark makersMark = p as MakersMark;
		if (makersMark.Mark != Mark)
		{
			return false;
		}
		if (makersMark.Desc != Desc)
		{
			return false;
		}
		if (makersMark.Color != Color)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public void Init()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		StringBuilder stringBuilder2 = Event.NewStringBuilder();
		stringBuilder.Append("{{").Append(Color).Append("|")
			.Append(Mark)
			.Append("}}");
		Mark = stringBuilder.ToString();
		stringBuilder2.Append(Mark).Append("{{C|: ").Append(Desc)
			.Append("}}");
		Desc = stringBuilder2.ToString();
		Initialized = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == GetUnknownShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!Initialized)
		{
			Init();
		}
		E.AddMark(Mark);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!Initialized)
		{
			Init();
		}
		E.Postfix.Compound(Desc, '\n');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		if (!Initialized)
		{
			Init();
		}
		E.Postfix.Compound(Desc, '\n');
		return base.HandleEvent(E);
	}

	public static string Generate(bool RecordUse = true)
	{
		int num = 0;
		int num2;
		do
		{
			num2 = Stat.Random(1, 254);
		}
		while (num2 == 94 || num2 == 38 || num2 == 123 || num2 == 125 || num2 == 32 || num2 == 10 || char.IsLetter((char)num2) || char.IsDigit((char)num2) || (The.Game.GetBooleanGameState("MakersMarkUsed_" + (char)num2) && ++num < 32));
		string text = ((char)num2).ToString() ?? "";
		if (RecordUse)
		{
			RecordUsage(text);
		}
		return text;
	}

	public static void RecordUsage(string Mark)
	{
		The.Game.SetBooleanGameState("MakersMarkUsed_" + Mark, Value: true);
	}
}

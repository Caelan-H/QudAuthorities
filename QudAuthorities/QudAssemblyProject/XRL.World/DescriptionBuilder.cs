using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;

namespace XRL.World;

public class DescriptionBuilder : Dictionary<string, int>
{
	public const int ORDER_BASE = 10;

	public const int ORDER_ADJECTIVE = -500;

	public const int ORDER_CLAUSE = 600;

	public const int ORDER_TAG = 1100;

	public const int ORDER_MARK = -800;

	public const int ORDER_ADJUST_EXTREMELY_EARLY = -60;

	public const int ORDER_ADJUST_VERY_EARLY = -40;

	public const int ORDER_ADJUST_EARLY = -20;

	public const int ORDER_ADJUST_SLIGHTLY_EARLY = -5;

	public const int ORDER_ADJUST_SLIGHTLY_LATE = 5;

	public const int ORDER_ADJUST_LATE = 20;

	public const int ORDER_ADJUST_VERY_LATE = 40;

	public const int ORDER_ADJUST_EXTREMELY_LATE = 60;

	public const int SHORT_CUTOFF = 1040;

	public const int PRIORITY_LOW = 10;

	public const int PRIORITY_MEDIUM = 20;

	public const int PRIORITY_HIGH = 30;

	public const int PRIORITY_OVERRIDE = 40;

	public const int PRIORITY_ADJUST_SMALL = 1;

	public const int PRIORITY_ADJUST_MEDIUM = 2;

	public const int PRIORITY_ADJUST_LARGE = 3;

	public int Cutoff = int.MaxValue;

	public string PrimaryBase;

	public string LastAdded;

	public string Color;

	public List<string> WithClauses;

	public int ColorPriority = int.MinValue;

	public bool BaseOnly;

	public DescriptionBuilder()
	{
	}

	public DescriptionBuilder(int Cutoff)
		: this()
	{
		this.Cutoff = Cutoff;
	}

	public DescriptionBuilder(int Cutoff, bool BaseOnly)
		: this(Cutoff)
	{
		this.BaseOnly = BaseOnly;
	}

	public new void Add(string desc, int order = 0)
	{
		if (order >= Cutoff)
		{
			return;
		}
		if (ContainsKey(desc))
		{
			if (base[desc] < order)
			{
				base[desc] = order;
				LastAdded = desc;
			}
		}
		else
		{
			base.Add(desc, order);
			LastAdded = desc;
		}
	}

	public new void Remove(string desc)
	{
		base.Remove(desc);
		if (desc == PrimaryBase)
		{
			PrimaryBase = null;
		}
	}

	public new void Clear()
	{
		base.Clear();
		PrimaryBase = null;
		Color = null;
	}

	public void AddBase(string desc, int orderAdjust = 0, bool secondary = false)
	{
		Add(desc, 10 + orderAdjust);
		if (PrimaryBase == null && !secondary)
		{
			PrimaryBase = desc;
		}
	}

	public void ReplacePrimaryBase(string desc, int orderAdjust = 0)
	{
		if (PrimaryBase != null)
		{
			Remove(PrimaryBase);
		}
		Add(desc, 10 + orderAdjust);
		PrimaryBase = desc;
	}

	public void AddAdjective(string desc, int orderAdjust = 0)
	{
		if (!BaseOnly)
		{
			Add(desc, -500 + orderAdjust);
		}
	}

	public void AddClause(string desc, int orderAdjust = 0)
	{
		if (!BaseOnly)
		{
			Add(desc, 600 + orderAdjust);
		}
	}

	public void AddWithClause(string desc)
	{
		if (!BaseOnly)
		{
			if (WithClauses == null)
			{
				WithClauses = new List<string>();
			}
			WithClauses.Add(desc);
		}
	}

	public void AddTag(string desc, int orderAdjust = 0)
	{
		if (!BaseOnly)
		{
			Add(desc, 1100 + orderAdjust);
		}
	}

	public void AddMark(string desc, int orderAdjust = 0)
	{
		if (!BaseOnly)
		{
			Add(desc, -800 + orderAdjust);
		}
	}

	public void AddColor(string color, int priority = 0)
	{
		if (priority >= ColorPriority)
		{
			Color = color;
			ColorPriority = priority;
		}
	}

	public void AddColor(char color, int priority = 0)
	{
		if (priority >= ColorPriority)
		{
			Color = color.ToString() ?? "";
			ColorPriority = priority;
		}
	}

	public void Reset()
	{
		Clear();
		Cutoff = int.MaxValue;
		ColorPriority = int.MinValue;
		BaseOnly = false;
		if (WithClauses != null)
		{
			WithClauses.Clear();
		}
	}

	public void Resolve()
	{
		if (WithClauses != null && WithClauses.Count > 0)
		{
			if (WithClauses.Count > 1)
			{
				ColorUtility.SortExceptFormattingAndCase(WithClauses);
			}
			AddClause("with " + Grammar.MakeAndList(WithClauses));
			WithClauses.Clear();
		}
	}

	public override string ToString()
	{
		Resolve();
		switch (base.Count)
		{
		case 0:
			return "";
		case 1:
		{
			if (string.IsNullOrEmpty(Color))
			{
				return LastAdded;
			}
			StringBuilder stringBuilder2 = Event.NewStringBuilder();
			stringBuilder2.Append("{{").Append(Color).Append('|')
				.Append(LastAdded)
				.Append("}}");
			return stringBuilder2.ToString();
		}
		default:
		{
			List<string> list = new List<string>(base.Keys);
			if (list.Count > 1)
			{
				list.Sort(delegate(string a, string b)
				{
					int num2 = base[a].CompareTo(base[b]);
					return (num2 != 0) ? num2 : a.CompareTo(b);
				});
			}
			StringBuilder stringBuilder = Event.NewStringBuilder();
			bool flag = false;
			if (!string.IsNullOrEmpty(Color))
			{
				stringBuilder.Append("{{").Append(Color).Append('|');
				flag = true;
			}
			int i = 0;
			for (int num = list.Count; i < num; i++)
			{
				string text = list[i];
				if (i > 0)
				{
					if (flag && base[text] > 600)
					{
						stringBuilder.Append("}}");
						flag = false;
					}
					if (text.Length < 1 || (text[0] != ':' && text[0] != ','))
					{
						stringBuilder.Append(' ');
					}
				}
				stringBuilder.Append(text);
			}
			if (flag)
			{
				stringBuilder.Append("}}");
				flag = false;
			}
			return stringBuilder.ToString();
		}
		}
	}
}

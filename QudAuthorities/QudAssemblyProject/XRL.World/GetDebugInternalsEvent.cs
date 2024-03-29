using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetDebugInternalsEvent : MinEvent
{
	public GameObject Object;

	public Dictionary<string, List<string>> Entries = new Dictionary<string, List<string>>();

	private static GetDebugInternalsEvent instance;

	public new static readonly int ID;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetDebugInternalsEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public GetDebugInternalsEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Object = null;
		Entries.Clear();
		base.Reset();
	}

	public static string GetFor(GameObject Object)
	{
		if (instance == null)
		{
			instance = new GetDebugInternalsEvent();
		}
		instance.Object = Object;
		instance.Entries.Clear();
		if (Object.HandleEvent(instance) && Object.HasRegisteredEvent("GetDebugInternals"))
		{
			Object.FireEvent(Event.New("GetDebugInternals", "Object", Object, "Event", instance));
		}
		return BuildSummary(instance.Entries);
	}

	public void AddEntry(string Source, string Name, string Value)
	{
		if (!Entries.TryGetValue(Source, out var value))
		{
			value = new List<string>();
			Entries[Source] = value;
		}
		if (Value == null)
		{
			Value = "NULL";
		}
		else if (ColorUtility.HasFormatting(Value))
		{
			Value = ColorUtility.EscapeFormatting(Value);
		}
		if (Value.IndexOf('\n') != -1)
		{
			value.Add(Name + ":\n" + Value);
		}
		else
		{
			value.Add(Name + ": " + Value);
		}
	}

	public void AddEntry(string Source, string Name, int Value)
	{
		AddEntry(Source, Name, Value.ToString());
	}

	public void AddEntry(string Source, string Name, float Value)
	{
		AddEntry(Source, Name, Value.ToString());
	}

	public void AddEntry(string Source, string Name, double Value)
	{
		AddEntry(Source, Name, Value.ToString());
	}

	public void AddEntry(string Source, string Name, bool Value)
	{
		AddEntry(Source, Name, Value.ToString());
	}

	public void AddEntry(string Source, string Name, GameObject Value)
	{
		AddEntry(Source, Name, Value?.DebugName);
	}

	public void AddEntry(IPart Part, string Name, string Value)
	{
		AddEntry(Part.Name, Name, Value);
	}

	public void AddEntry(IPart Part, string Name, int Value)
	{
		AddEntry(Part, Name, Value.ToString());
	}

	public void AddEntry(IPart Part, string Name, float Value)
	{
		AddEntry(Part, Name, Value.ToString());
	}

	public void AddEntry(IPart Part, string Name, double Value)
	{
		AddEntry(Part, Name, Value.ToString());
	}

	public void AddEntry(IPart Part, string Name, bool Value)
	{
		AddEntry(Part, Name, Value.ToString());
	}

	public void AddEntry(IPart Part, string Name, GameObject Value)
	{
		AddEntry(Part, Name, Value?.DebugName);
	}

	public void AddEntry(Effect FX, string Name, string Value)
	{
		AddEntry(FX.ClassName, Name, Value);
	}

	public void AddEntry(Effect FX, string Name, int Value)
	{
		AddEntry(FX, Name, Value.ToString());
	}

	public void AddEntry(Effect FX, string Name, float Value)
	{
		AddEntry(FX, Name, Value.ToString());
	}

	public void AddEntry(Effect FX, string Name, double Value)
	{
		AddEntry(FX, Name, Value.ToString());
	}

	public void AddEntry(Effect FX, string Name, bool Value)
	{
		AddEntry(FX, Name, Value.ToString());
	}

	public void AddEntry(Effect FX, string Name, GameObject Value)
	{
		AddEntry(FX, Name, Value?.DebugName);
	}

	private static string BuildSummary(Dictionary<string, List<string>> Entries)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		List<string> list = new List<string>(Entries.Keys);
		list.Sort();
		stringBuilder.Append("{{internals|");
		foreach (string item in list)
		{
			List<string> list2 = Entries[item];
			stringBuilder.Append('Ú').Append(' ').Append(item)
				.Append('\n');
			int i = 0;
			for (int count = list2.Count; i < count; i++)
			{
				string text = list2[i];
				if (text.IndexOf('\n') != -1)
				{
					List<string> list3 = new List<string>(text.Split('\n'));
					if (list3[1] == "")
					{
						list3.RemoveAt(1);
					}
					if (list3[0] == "")
					{
						list3.RemoveAt(0);
					}
					if (list3[list3.Count - 1] == "")
					{
						list3.RemoveAt(list3.Count - 1);
					}
					stringBuilder.Append((i == count - 1) ? 'À' : 'Ã').Append('Ä').Append('Â')
						.Append(' ')
						.Append(list3[0])
						.Append('\n');
					int j = 1;
					for (int count2 = list3.Count; j < count2; j++)
					{
						stringBuilder.Append((i == count - 1) ? 'ÿ' : '³').Append(' ').Append((j == count2 - 1) ? 'À' : 'Ã')
							.Append('Ä')
							.Append('Ä')
							.Append(' ')
							.Append(list3[j])
							.Append('\n');
					}
				}
				else
				{
					stringBuilder.Append((i == count - 1) ? 'À' : 'Ã').Append('Ä').Append('Ä')
						.Append(' ')
						.Append(text)
						.Append('\n');
				}
			}
		}
		stringBuilder.Append("}}");
		return stringBuilder.ToString();
	}
}

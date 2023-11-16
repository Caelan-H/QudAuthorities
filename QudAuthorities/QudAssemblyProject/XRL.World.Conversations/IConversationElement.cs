using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using XRL.UI;

namespace XRL.World.Conversations;

public abstract class IConversationElement : IComparable<IConversationElement>
{
	public const int END_SORT_ORDINAL = 999999;

	public static HashSet<Type> ConvertTypes = new HashSet<Type>
	{
		typeof(object),
		typeof(DBNull),
		typeof(bool),
		typeof(char),
		typeof(sbyte),
		typeof(byte),
		typeof(short),
		typeof(ushort),
		typeof(int),
		typeof(uint),
		typeof(long),
		typeof(ulong),
		typeof(float),
		typeof(double),
		typeof(decimal),
		typeof(DateTime),
		typeof(string)
	};

	public string ID;

	public string Text;

	public int Priority;

	public bool Awoken;

	public IConversationElement Parent;

	public List<IConversationPart> Parts;

	public List<IConversationElement> Elements;

	public List<ConversationText> Texts;

	public Dictionary<string, string> Predicates;

	public Dictionary<string, string> Actions;

	public Dictionary<string, string> Attributes;

	public int Ordinal
	{
		set
		{
			Priority = -value;
		}
	}

	public virtual int Propagation => 3;

	public IConversationElement GetText()
	{
		if (Texts.IsNullOrEmpty())
		{
			return null;
		}
		List<ConversationText> list = Texts.FindAll((ConversationText x) => x.IsVisible());
		if (list.Count == 0)
		{
			return null;
		}
		int max = list.Max((ConversationText x) => x.Priority);
		List<ConversationText> list2 = list.FindAll((ConversationText x) => x.Priority == max);
		ConversationText Selected = list2.GetRandomElement();
		GetTextElementEvent.Send(this, Texts, list, list2, ref Selected);
		if (Selected.Text == null)
		{
			return Selected.GetText();
		}
		return Selected;
	}

	public virtual void Awake()
	{
		if (Awoken)
		{
			return;
		}
		if (Parts != null)
		{
			foreach (IConversationPart part in Parts)
			{
				part.Awake();
			}
		}
		Awoken = true;
	}

	public virtual void Prepare()
	{
		if (Text == null)
		{
			IConversationElement text = GetText();
			if (text != null)
			{
				Text = text.Text.GetRandomSubstring('~', Trim: true);
				StringBuilder stringBuilder = Event.NewStringBuilder(Text.Trim());
				GameObject Subject = The.Speaker;
				GameObject Object = null;
				PrepareTextEvent.Send(text, stringBuilder, ref Subject, ref Object, out var ExplicitSubject, out var ExplicitSubjectPlural, out var ExplicitObject, out var ExplicitObjectPlural);
				GameObject subject = Subject;
				GameObject @object = Object;
				Text = GameText.VariableReplace(stringBuilder, subject, ExplicitSubject, ExplicitSubjectPlural, @object, ExplicitObject, ExplicitObjectPlural);
			}
		}
	}

	public virtual string GetDisplayText(bool WithColor = false)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder(Text);
		GameObject Subject = The.Speaker;
		GameObject Object = null;
		DisplayTextEvent.Send(this, stringBuilder, ref Subject, ref Object, out var ExplicitSubject, out var ExplicitSubjectPlural, out var ExplicitObject, out var ExplicitObjectPlural, out var VariableReplace);
		if (VariableReplace)
		{
			GameObject subject = Subject;
			GameObject @object = Object;
			GameText.VariableReplace(stringBuilder, subject, ExplicitSubject, ExplicitSubjectPlural, @object, ExplicitObject, ExplicitObjectPlural);
		}
		if (WithColor)
		{
			stringBuilder.Insert(0, '|');
			stringBuilder.Insert(0, GetTextColor());
			stringBuilder.Insert(0, "{{");
			stringBuilder.Append("}}");
		}
		return stringBuilder.ToString();
	}

	public virtual string GetTextColor()
	{
		string Color = "y";
		ColorTextEvent.Send(this, ref Color);
		return Color;
	}

	public bool WantEvent(int ID)
	{
		return WantEvent(ID, Propagation);
	}

	private bool WantEvent(int ID, int Propagation)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].PropagateTo(Propagation) && Parts[i].WantEvent(ID, Propagation))
				{
					return true;
				}
			}
		}
		if (Parent != null)
		{
			return Parent.WantEvent(ID, Propagation);
		}
		return false;
	}

	public bool HandleEvent(ConversationEvent E)
	{
		return HandleEvent(E, Propagation);
	}

	private bool HandleEvent(ConversationEvent E, int Propagation)
	{
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				if (Parts[i].PropagateTo(Propagation) && Parts[i].WantEvent(E.ID, Propagation))
				{
					if (!E.HandlePartDispatch(Parts[i]))
					{
						return false;
					}
					Parts[i].HandleEvent(E);
				}
			}
		}
		if (Parent != null)
		{
			return Parent.HandleEvent(E, Propagation);
		}
		return true;
	}

	public virtual bool Enter()
	{
		if (!EnterElementEvent.Check(this))
		{
			return false;
		}
		return true;
	}

	public virtual void Entered()
	{
		if (!Actions.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, string> action in Actions)
			{
				if (ConversationDelegates.Actions.TryGetValue(action.Key, out var value))
				{
					value(this, action.Value);
				}
			}
		}
		EnteredElementEvent.Send(this);
	}

	public virtual bool Leave()
	{
		if (!LeaveElementEvent.Check(this))
		{
			return false;
		}
		return true;
	}

	public virtual void Left()
	{
		Reset();
		LeftElementEvent.Send(this);
	}

	public virtual void Reset()
	{
		if (ConversationUI.StartNode != this)
		{
			Text = null;
		}
	}

	public bool FireEvent(Event E)
	{
		return FireEvent(E, Propagation);
	}

	private bool FireEvent(Event E, int Propagation)
	{
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				if (Parts[i].PropagateTo(Propagation) && !Parts[i].FireEvent(E))
				{
					return false;
				}
			}
		}
		if (Parent != null)
		{
			return Parent.FireEvent(E, Propagation);
		}
		return true;
	}

	public virtual bool IsVisible()
	{
		if (!Predicates.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, string> predicate in Predicates)
			{
				if (ConversationDelegates.Predicates.TryGetValue(predicate.Key, out var value) && !value(this, predicate.Value))
				{
					return false;
				}
			}
		}
		return IsElementVisibleEvent.Check(this);
	}

	public IConversationElement GetElement(params string[] Path)
	{
		IConversationElement conversationElement = this;
		for (int i = 0; i < Path.Length; i++)
		{
			if (conversationElement == null)
			{
				break;
			}
			conversationElement = conversationElement.GetElementByID(Path[i]);
		}
		return conversationElement;
	}

	public IConversationElement GetElementByID(string ID)
	{
		if (Elements != null)
		{
			int i = 0;
			for (int count = Elements.Count; i < count; i++)
			{
				if (Elements[i].ID == ID)
				{
					return Elements[i];
				}
			}
		}
		if (Texts != null)
		{
			int j = 0;
			for (int count2 = Texts.Count; j < count2; j++)
			{
				if (Texts[j].ID == ID)
				{
					return Texts[j];
				}
			}
		}
		return null;
	}

	public bool TryGetAttribute(string Key, out string Value)
	{
		if (Attributes == null)
		{
			Value = null;
			return false;
		}
		return Attributes.TryGetValue(Key, out Value);
	}

	public virtual void LoadAttributes(Dictionary<string, string> Attributes)
	{
		if (Attributes == null || Attributes.Count == 0)
		{
			return;
		}
		Type type = GetType();
		foreach (KeyValuePair<string, string> Attribute in Attributes)
		{
			if (ConversationDelegates.Predicates.ContainsKey(Attribute.Key))
			{
				if (Predicates == null)
				{
					Predicates = new Dictionary<string, string>();
				}
				Predicates[Attribute.Key] = Attribute.Value;
				continue;
			}
			if (ConversationDelegates.Actions.ContainsKey(Attribute.Key))
			{
				if (Actions == null)
				{
					Actions = new Dictionary<string, string>();
				}
				Actions[Attribute.Key] = Attribute.Value;
				continue;
			}
			if (ConversationDelegates.PartGenerators.TryGetValue(Attribute.Key, out var value))
			{
				IConversationPart conversationPart = value(this, Attribute.Value);
				if (conversationPart != null)
				{
					AddPart(conversationPart, Sort: false);
				}
				continue;
			}
			FieldInfo field = type.GetField(Attribute.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)field != null && !field.IsInitOnly && ConvertTypes.Contains(field.FieldType))
			{
				field.SetValue(this, Convert.ChangeType(Attribute.Value, field.FieldType));
				continue;
			}
			PropertyInfo property = type.GetProperty(Attribute.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)property != null && property.CanWrite && ConvertTypes.Contains(property.PropertyType))
			{
				property.SetValue(this, Convert.ChangeType(Attribute.Value, property.PropertyType));
				continue;
			}
			if (this.Attributes == null)
			{
				this.Attributes = new Dictionary<string, string>();
			}
			this.Attributes[Attribute.Key] = Attribute.Value;
		}
	}

	public T Create<T>(ConversationXMLBlueprint Blueprint) where T : IConversationElement, new()
	{
		T val = new T();
		val.Parent = this;
		val.Load(Blueprint);
		return val;
	}

	public virtual bool LoadChild(ConversationXMLBlueprint Blueprint)
	{
		switch (Blueprint.Name)
		{
		case "Node":
			if (Elements == null)
			{
				Elements = new List<IConversationElement>();
			}
			Elements.Add(Create<Node>(Blueprint));
			break;
		case "Choice":
			if (Elements == null)
			{
				Elements = new List<IConversationElement>();
			}
			Elements.Add(Create<Choice>(Blueprint));
			break;
		case "Text":
			if (Texts == null)
			{
				Texts = new List<ConversationText>();
			}
			Texts.Add(Create<ConversationText>(Blueprint));
			break;
		case "Part":
		{
			if (IConversationPart.TryCreate(Blueprint.Attributes["Name"], Blueprint, out var Part))
			{
				AddPart(Part, Sort: false);
			}
			break;
		}
		default:
			return false;
		}
		return true;
	}

	public bool HasPart(IConversationPart Part)
	{
		if (Parts == null)
		{
			return false;
		}
		return Parts.Contains(Part);
	}

	public bool HasPart<T>() where T : IConversationPart
	{
		if (Parts == null)
		{
			return false;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if ((object)Parts[i].GetType() == typeof(T))
			{
				return true;
			}
		}
		return false;
	}

	public void AddPart(IConversationPart Part, bool Sort = true)
	{
		if (Parts == null)
		{
			Parts = new List<IConversationPart>();
		}
		Parts.Add(Part);
		Part.ParentElement = this;
		Part.Initialize();
		if (Sort)
		{
			Parts.Sort();
		}
		if (Awoken)
		{
			Part.Awake();
		}
	}

	public T GetPart<T>() where T : IConversationPart
	{
		if (Parts == null)
		{
			return null;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if ((object)Parts[i].GetType() == typeof(T))
			{
				return Parts[i] as T;
			}
		}
		return null;
	}

	public bool TryGetPart<T>(out T Part) where T : IConversationPart
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if ((object)Parts[i].GetType() == typeof(T))
				{
					Part = Parts[i] as T;
					return true;
				}
			}
		}
		Part = null;
		return false;
	}

	public virtual void LoadText(ConversationXMLBlueprint Blueprint)
	{
		if (!string.IsNullOrWhiteSpace(Blueprint.Text))
		{
			Text = Blueprint.Text;
		}
	}

	public virtual void Load(ConversationXMLBlueprint Blueprint)
	{
		ID = Blueprint.ID;
		LoadAttributes(Blueprint.Attributes);
		LoadText(Blueprint);
		if (Blueprint.Children == null)
		{
			return;
		}
		foreach (ConversationXMLBlueprint child in Blueprint.Children)
		{
			try
			{
				LoadChild(child);
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Error adding " + child.Name + " by ID " + child.ID, x);
			}
		}
		Parts?.Sort();
	}

	public int CompareTo(IConversationElement Other)
	{
		return Other?.Priority.CompareTo(Priority) ?? (-1);
	}
}

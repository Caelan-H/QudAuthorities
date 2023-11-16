using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Tattoos : IPart
{
	public enum ApplyResult
	{
		Success,
		TooManyTattoos,
		AbstractBodyPart,
		NonContactBodyPart,
		InappropriateBodyPart,
		NoUsableBodyParts
	}

	public const int MAXIMUM_TATTOOS_PER_BODY_PART = 3;

	[NonSerialized]
	public Dictionary<int, List<string>> Descriptions = new Dictionary<int, List<string>>();

	public string ColorString;

	public string DetailColor;

	[NonSerialized]
	public static char[] splitterColon = new char[1] { ':' };

	public string InitialTattoos
	{
		set
		{
			ParentObject.SetStringProperty("InitialTattoos", value);
		}
	}

	private static ApplyResult GetBodyPartGeneralTattooability(BodyPart part)
	{
		if (part.Abstract)
		{
			return ApplyResult.AbstractBodyPart;
		}
		if (!part.Contact)
		{
			return ApplyResult.NonContactBodyPart;
		}
		int category = part.Category;
		if (category != 1 && (uint)(category - 3) > 2u && category != 12)
		{
			return ApplyResult.InappropriateBodyPart;
		}
		return ApplyResult.Success;
	}

	public static ApplyResult CanApplyTattoo(GameObject who, BodyPart part)
	{
		ApplyResult bodyPartGeneralTattooability = GetBodyPartGeneralTattooability(part);
		if (bodyPartGeneralTattooability != 0)
		{
			return bodyPartGeneralTattooability;
		}
		if (who.GetPart("Tattoos") is Tattoos tattoos && !tattoos.CanAddTattoo(part))
		{
			return ApplyResult.TooManyTattoos;
		}
		return ApplyResult.Success;
	}

	public static ApplyResult ApplyTattoo(GameObject who, BodyPart part, string desc, string color = null, string detail = null)
	{
		ApplyResult bodyPartGeneralTattooability = GetBodyPartGeneralTattooability(part);
		if (bodyPartGeneralTattooability != 0)
		{
			return bodyPartGeneralTattooability;
		}
		Tattoos tattoos = who.RequirePart<Tattoos>();
		if (!tattoos.AddTattoo(part, desc))
		{
			return ApplyResult.TooManyTattoos;
		}
		if (!string.IsNullOrEmpty(color))
		{
			tattoos.ColorString = color;
			if (!string.IsNullOrEmpty(detail))
			{
				tattoos.DetailColor = detail;
			}
		}
		who.CheckMarkOfDeath();
		return ApplyResult.Success;
	}

	public static ApplyResult ApplyTattoo(GameObject who, string desc, string color = null, string detail = null)
	{
		Body body = who.Body;
		if (body == null)
		{
			return ApplyResult.NoUsableBodyParts;
		}
		List<BodyPart> parts = body.GetParts();
		List<BodyPart> list = new List<BodyPart>(parts.Count);
		foreach (BodyPart item in parts)
		{
			if (CanApplyTattoo(who, item) == ApplyResult.Success)
			{
				list.Add(item);
			}
		}
		if (list.Count == 0)
		{
			return ApplyResult.NoUsableBodyParts;
		}
		return ApplyTattoo(who, list.GetRandomElement(), desc, color, detail);
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		Tattoos tattoos = base.DeepCopy(Parent) as Tattoos;
		tattoos.Descriptions = new Dictionary<int, List<string>>(Descriptions.Count);
		foreach (KeyValuePair<int, List<string>> description in Descriptions)
		{
			tattoos.Descriptions[description.Key] = new List<string>(description.Value);
		}
		return tattoos;
	}

	public override void SaveData(SerializationWriter Writer)
	{
		Writer.Write(Descriptions.Count);
		foreach (int key in Descriptions.Keys)
		{
			Writer.Write(key);
			List<string> list = Descriptions[key];
			Writer.Write(list.Count);
			foreach (string item in list)
			{
				Writer.Write(item);
			}
		}
		base.SaveData(Writer);
	}

	public override void LoadData(SerializationReader Reader)
	{
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int key = Reader.ReadInt32();
			int num2 = Reader.ReadInt32();
			List<string> list = new List<string>(num2);
			for (int j = 0; j < num2; j++)
			{
				list.Add(Reader.ReadString());
			}
			Descriptions.Add(key, list);
		}
		base.LoadData(Reader);
	}

	public void ValidateTattoos()
	{
		if (Descriptions.Count == 0)
		{
			return;
		}
		Body body = ParentObject.Body;
		if (body == null)
		{
			RemoveTattoos();
			return;
		}
		List<int> list = null;
		foreach (int key in Descriptions.Keys)
		{
			if (body.GetPartByID(key) == null)
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(key);
			}
		}
		if (list != null)
		{
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				RemoveTattoo(list[i]);
			}
		}
	}

	public bool CanAddTattoo(BodyPart part)
	{
		if (part._ID != 0 && Descriptions.ContainsKey(part._ID) && Descriptions[part._ID].Count >= 3)
		{
			return false;
		}
		return true;
	}

	public bool CanAddTattoo(int BodyPartID)
	{
		if (Descriptions.ContainsKey(BodyPartID) && Descriptions[BodyPartID].Count >= 3)
		{
			return false;
		}
		return true;
	}

	public bool AddTattoo(BodyPart part, string desc)
	{
		if (part._ID != 0 && Descriptions.ContainsKey(part._ID))
		{
			List<string> list = Descriptions[part.ID];
			if (list.Count >= 3)
			{
				return false;
			}
			list.Add(desc);
		}
		else
		{
			Descriptions.Add(part.ID, new List<string> { desc });
		}
		return true;
	}

	public bool AddTattoo(int BodyPartID, string desc)
	{
		if (Descriptions.ContainsKey(BodyPartID))
		{
			List<string> list = Descriptions[BodyPartID];
			if (list.Count >= 3)
			{
				return false;
			}
			list.Add(desc);
		}
		else
		{
			Descriptions.Add(BodyPartID, new List<string> { desc });
		}
		return true;
	}

	public override void Remove()
	{
		ParentObject?.CheckMarkOfDeath(this);
		base.Remove();
	}

	public bool RemoveTattoos()
	{
		bool result = Descriptions.Count > 0;
		Descriptions.Clear();
		ColorString = null;
		DetailColor = null;
		GameObject parentObject = ParentObject;
		if (parentObject != null)
		{
			parentObject.CheckMarkOfDeath();
			return result;
		}
		return result;
	}

	public bool RemoveTattoo(int BodyPartID)
	{
		bool result = Descriptions.ContainsKey(BodyPartID);
		Descriptions.Remove(BodyPartID);
		if (Descriptions.Count == 0)
		{
			ColorString = null;
			DetailColor = null;
		}
		GameObject parentObject = ParentObject;
		if (parentObject != null)
		{
			parentObject.CheckMarkOfDeath();
			return result;
		}
		return result;
	}

	public List<int> GetBodyPartIDsSortedByPosition()
	{
		List<int> list = new List<int>(Descriptions.Keys);
		if (list.Count < 2)
		{
			return list;
		}
		Body body = ParentObject.Body;
		if (body == null)
		{
			return list;
		}
		List<BodyPart> parts = body.GetParts();
		List<int> PartIDs = new List<int>(parts.Count);
		foreach (BodyPart item in parts)
		{
			if (item._ID != 0)
			{
				PartIDs.Add(item._ID);
			}
		}
		list.Sort((int a, int b) => PartIDs.IndexOf(a).CompareTo(PartIDs.IndexOf(b)));
		return list;
	}

	public string GetTattoosDescription()
	{
		ValidateTattoos();
		if (Descriptions.Count == 0)
		{
			return null;
		}
		Body body = ParentObject.Body;
		if (body == null)
		{
			return null;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		foreach (int item in GetBodyPartIDsSortedByPosition())
		{
			BodyPart partByID = body.GetPartByID(item);
			List<string> list = Descriptions[item];
			stringBuilder.Compound(ParentObject.Its).Append(' ').Append(partByID.Name)
				.Append(' ')
				.Append(partByID.Plural ? "bear" : "bears")
				.Append(' ');
			string text;
			if (list.Count == 1)
			{
				stringBuilder.Append("a tattoo of ").Append(list[0]);
				text = list[0];
			}
			else
			{
				stringBuilder.Append("tattoos of ").Append(Grammar.MakeAndList(list));
				text = list[list.Count - 1];
			}
			if (text.IndexOf('.') != -1 || text.IndexOf('!') != -1 || text.IndexOf('?') != -1)
			{
				string text2 = ColorUtility.StripFormatting(text);
				char c = text2[text2.Length - 1];
				if (c != '.' && c != '!' && c != '?')
				{
					stringBuilder.Append('.');
				}
			}
			else
			{
				stringBuilder.Append('.');
			}
		}
		return stringBuilder.ToString();
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		if (!string.IsNullOrEmpty(ColorString) && (!Options.HPColor || (!ParentObject.IsPlayer() && !ParentObject.IsPlayerLed())) && ParentObject.GetIntProperty("DontOverrideColor") < 1)
		{
			E.ColorString = ColorString;
			if (!string.IsNullOrEmpty(DetailColor))
			{
				E.DetailColor = DetailColor;
			}
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDebugInternalsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "ColorString", ColorString);
		E.AddEntry(this, "DetailColor", DetailColor);
		if (Descriptions != null && Descriptions.Count > 0)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			foreach (int item in GetBodyPartIDsSortedByPosition())
			{
				BodyPart bodyPartByID = ParentObject.GetBodyPartByID(item);
				foreach (string item2 in Descriptions[item])
				{
					stringBuilder.Append(bodyPartByID.GetOrdinalName()).Append(": ").Append(item2)
						.Append('\n');
				}
			}
			E.AddEntry(this, "Descriptions", stringBuilder.ToString());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		string tattoosDescription = GetTattoosDescription();
		if (!string.IsNullOrEmpty(tattoosDescription))
		{
			if (E.Postfix.Length > 0 && E.Postfix[E.Postfix.Length - 1] != '\n')
			{
				E.Postfix.Append('\n');
			}
			E.Postfix.Append('\n').Append(tattoosDescription);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject && (E.Context == "CloningDraught" || E.Context == "Cloneling" || E.Context == "Budding"))
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		string stringProperty = ParentObject.GetStringProperty("InitialTattoos");
		if (stringProperty != null)
		{
			ParentObject.RemoveStringProperty("InitialTattoos");
			Body body = ParentObject.Body;
			if (body != null)
			{
				string[] array = stringProperty.Split('|');
				foreach (string text in array)
				{
					string[] array2 = text.Split(splitterColon, 2);
					if (array2.Length == 2)
					{
						string text2 = array2[0];
						string desc = array2[1];
						if (text2 == "*")
						{
							ApplyTattoo(ParentObject, desc);
							continue;
						}
						BodyPart bodyPart = body.GetPartByName(text2) ?? body.GetFirstPart(text2);
						if (bodyPart != null)
						{
							ApplyTattoo(ParentObject, bodyPart, desc);
							continue;
						}
						MetricsManager.LogError("could not find body part " + text2 + " on " + ParentObject.Blueprint + " for InitialTattoos spec " + text);
					}
					else
					{
						MetricsManager.LogError("bad InitialTattoos spec: " + text);
					}
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
		Object.RegisterPartEvent(this, "HasTattoo");
		Object.RegisterPartEvent(this, "VisibleStatusColor");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VisibleStatusColor")
		{
			if (E.GetStringParameter("Color") == "&Y" && ParentObject.GetIntProperty("DontOverrideColor") < 1)
			{
				if (!string.IsNullOrEmpty(ColorString))
				{
					E.SetParameter("Color", ColorString);
				}
				if (!string.IsNullOrEmpty(DetailColor))
				{
					E.SetParameter("DetailColor", DetailColor);
				}
			}
		}
		else if (E.ID == "HasTattoo")
		{
			string stringParameter = E.GetStringParameter("MatchText");
			if (!string.IsNullOrEmpty(stringParameter))
			{
				CompareOptions comp = ((!E.HasFlag("CaseSensitive")) ? CompareOptions.IgnoreCase : CompareOptions.None);
				foreach (List<string> value in Descriptions.Values)
				{
					foreach (string item in value)
					{
						if (item.Contains(stringParameter, comp))
						{
							return false;
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}

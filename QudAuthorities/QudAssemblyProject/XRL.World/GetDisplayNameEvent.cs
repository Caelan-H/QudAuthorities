using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetDisplayNameEvent : MinEvent
{
	public GameObject Object;

	public DescriptionBuilder DB = new DescriptionBuilder();

	public string Context;

	public bool AsIfKnown;

	public bool Single;

	public bool NoConfusion;

	public bool NoColor;

	public bool ColorOnly;

	public bool Visible;

	public new static readonly int ID;

	private static List<GetDisplayNameEvent> Pool;

	private static int PoolCounter;

	public int Cutoff
	{
		get
		{
			return DB.Cutoff;
		}
		set
		{
			DB.Cutoff = value;
		}
	}

	public bool BaseOnly
	{
		get
		{
			return DB.BaseOnly;
		}
		set
		{
			DB.BaseOnly = value;
		}
	}

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetDisplayNameEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetDisplayNameEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetDisplayNameEvent()
	{
		base.ID = ID;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static GetDisplayNameEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Object = null;
		DB.Reset();
		Context = null;
		AsIfKnown = false;
		Single = false;
		NoConfusion = false;
		NoColor = false;
		base.Reset();
	}

	public static string GetFor(GameObject Object, string Base, int Cutoff = int.MaxValue, string Context = null, bool AsIfKnown = false, bool Single = false, bool NoConfusion = false, bool NoColor = false, bool ColorOnly = false, bool Visible = true, bool BaseOnly = false)
	{
		GetDisplayNameEvent getDisplayNameEvent = FromPool();
		getDisplayNameEvent.Object = Object;
		getDisplayNameEvent.Context = Context;
		getDisplayNameEvent.AsIfKnown = AsIfKnown;
		getDisplayNameEvent.Single = Single;
		getDisplayNameEvent.NoConfusion = NoConfusion;
		getDisplayNameEvent.NoColor = NoColor;
		getDisplayNameEvent.ColorOnly = ColorOnly;
		getDisplayNameEvent.Visible = Visible;
		getDisplayNameEvent.AddBase(Base);
		getDisplayNameEvent.Cutoff = Cutoff;
		getDisplayNameEvent.BaseOnly = BaseOnly;
		if (ColorOnly)
		{
			if (!string.IsNullOrEmpty(getDisplayNameEvent.DB.Color))
			{
				return getDisplayNameEvent.DB.Color;
			}
			return ColorUtility.GetMainForegroundColor(getDisplayNameEvent.ProcessFor(Object));
		}
		return getDisplayNameEvent.ProcessFor(Object);
	}

	public void Add(string desc, int order = 0)
	{
		DB.Add(desc, order);
	}

	public void AddBase(string desc, int orderAdjust = 0, bool secondary = false)
	{
		DB.AddBase(desc, orderAdjust, secondary);
	}

	public void ReplacePrimaryBase(string desc, int orderAdjust = 0)
	{
		DB.ReplacePrimaryBase(desc, orderAdjust);
	}

	public string GetPrimaryBase()
	{
		return DB.PrimaryBase;
	}

	public void AddAdjective(string desc, int orderAdjust = 0)
	{
		DB.AddAdjective(desc, orderAdjust);
	}

	public void AddClause(string desc, int orderAdjust = 0)
	{
		DB.AddClause(desc, orderAdjust);
	}

	public void AddWithClause(string desc)
	{
		DB.AddWithClause(desc);
	}

	public void AddTag(string desc, int orderAdjust = 0)
	{
		DB.AddTag(desc, orderAdjust);
	}

	public void AddMark(string desc, int orderAdjust = 0)
	{
		DB.AddMark(desc, orderAdjust);
	}

	public void AddColor(string color, int priority = 0)
	{
		if (!NoColor)
		{
			DB.AddColor(color, priority);
		}
	}

	public void AddColor(char color, int priority = 0)
	{
		if (!NoColor)
		{
			DB.AddColor(color, priority);
		}
	}

	public bool Understood()
	{
		if (!AsIfKnown && Object != null)
		{
			return Object.Understood();
		}
		return true;
	}

	public void AddMissileWeaponDamageTag(GetMissileWeaponPerformanceEvent E)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (E.Attributes == null || !E.Attributes.Contains("NonPenetrating"))
		{
			stringBuilder.Append("{{").Append(E.PenetrateWalls ? 'm' : (E.PenetrateCreatures ? 'W' : 'c')).Append('|')
				.Append('\u001a')
				.Append("}}");
			if (E.Attributes != null && E.Attributes.Contains("Vorpal"))
			{
				stringBuilder.Append('÷');
			}
			else
			{
				stringBuilder.Append(Math.Max(E.BasePenetration + 4, 1));
			}
		}
		if (E.DamageRoll != null || (!string.IsNullOrEmpty(E.BaseDamage) && E.BaseDamage != "0"))
		{
			stringBuilder.Compound("{{").Append(E.GetDamageColor()).Append('|')
				.Append('\u0003')
				.Append("}}")
				.Append((E.DamageRoll != null) ? E.DamageRoll.ToString() : E.BaseDamage);
		}
		if (stringBuilder.Length > 0)
		{
			AddTag(stringBuilder.ToString(), -20);
		}
	}

	public string ProcessFor(GameObject obj)
	{
		obj.HandleEvent(this);
		if (obj.HasRegisteredEvent("GetDisplayName"))
		{
			string text = DB.ToString();
			StringBuilder stringBuilder = Event.NewStringBuilder();
			StringBuilder stringBuilder2 = Event.NewStringBuilder();
			StringBuilder stringBuilder3 = Event.NewStringBuilder();
			StringBuilder stringBuilder4 = Event.NewStringBuilder();
			StringBuilder stringBuilder5 = Event.NewStringBuilder();
			stringBuilder.Append(text);
			Event @event = Event.New("GetDisplayName");
			@event.SetParameter("Object", obj);
			@event.SetParameter("DisplayName", stringBuilder);
			@event.SetParameter("Prefix", stringBuilder2);
			@event.SetParameter("Infix", stringBuilder3);
			@event.SetParameter("Postfix", stringBuilder4);
			@event.SetParameter("PostPostfix", stringBuilder5);
			@event.SetParameter("Cutoff", Cutoff);
			@event.SetParameter("Context", Context);
			@event.SetFlag("BaseOnly", BaseOnly);
			@event.SetFlag("AsIfKnown", AsIfKnown);
			@event.SetFlag("Single", Single);
			@event.SetFlag("NoConfusion", NoConfusion);
			@event.SetFlag("NoColor", NoColor);
			obj.FireEvent(@event);
			if (stringBuilder.Length != text.Length || stringBuilder.ToString() != text)
			{
				DB.Clear();
				DB.AddBase(stringBuilder.ToString());
			}
			if (!BaseOnly)
			{
				if (stringBuilder2.Length != 0)
				{
					AddAdjective(stringBuilder2.ToString().Trim());
				}
				if (stringBuilder3.Length != 0)
				{
					AddClause(stringBuilder3.ToString().Trim());
				}
				if (stringBuilder4.Length != 0)
				{
					AddTag(stringBuilder4.ToString().Trim());
				}
				if (stringBuilder5.Length != 0)
				{
					AddTag(stringBuilder5.ToString().Trim(), 20);
				}
			}
		}
		return Markup.Wrap(DB.ToString());
	}
}

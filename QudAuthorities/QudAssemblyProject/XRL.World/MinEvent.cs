using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace XRL.World;

public abstract class MinEvent : IEvent
{
	public delegate void EventPoolReset();

	public delegate int EventPoolCount();

	public const int CASCADE_NONE = 0;

	public const int CASCADE_EQUIPMENT = 1;

	public const int CASCADE_INVENTORY = 2;

	public const int CASCADE_SLOTS = 4;

	public const int CASCADE_COMPONENTS = 8;

	public const int CASCADE_EXCEPT_THROWN_WEAPON = 16;

	public const int CASCADE_ALL = 15;

	public int ID;

	public bool InterfaceExit;

	private static List<EventPoolReset> EventPoolResets = new List<EventPoolReset>();

	private static Dictionary<string, EventPoolCount> EventPoolCounts = new Dictionary<string, EventPoolCount>();

	private static int IDSequence;

	public static int CascadeLevel => 0;

	public static int AllocateID()
	{
		return ++IDSequence;
	}

	public static void RegisterPoolReset(EventPoolReset R)
	{
		EventPoolResets.Add(R);
	}

	public static void RegisterPoolCount(string ClassName, EventPoolCount C)
	{
		if (EventPoolCounts.ContainsKey(ClassName))
		{
			Debug.LogError("duplicate pool retrieval registration for " + ClassName);
		}
		else
		{
			EventPoolCounts.Add(ClassName, C);
		}
	}

	public static void ResetPools()
	{
		foreach (EventPoolReset eventPoolReset in EventPoolResets)
		{
			eventPoolReset();
		}
	}

	protected static T FromPool<T>(ref List<T> Pool, ref int Counter, int MaxPool = 8192) where T : MinEvent, new()
	{
		if (Pool == null)
		{
			Pool = new List<T>();
		}
		if (Counter >= Pool.Count)
		{
			if (Pool.Count >= MaxPool)
			{
				return new T();
			}
			int num = Counter - Pool.Count + 1;
			for (int i = 0; i < num; i++)
			{
				Pool.Add(new T());
			}
		}
		return Pool[Counter++];
	}

	public virtual void RequestInterfaceExit()
	{
		InterfaceExit = true;
	}

	public bool InterfaceExitRequested()
	{
		return InterfaceExit;
	}

	public virtual void PreprocessChildEvent(IEvent E)
	{
	}

	public virtual void ProcessChildEvent(IEvent E)
	{
		if (E.InterfaceExitRequested())
		{
			InterfaceExit = true;
		}
	}

	public virtual void Reset()
	{
		InterfaceExit = false;
	}

	public virtual bool WantInvokeDispatch()
	{
		return false;
	}

	public int GetID()
	{
		return ID;
	}

	public virtual int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static bool CascadeTo(int cascade, int level)
	{
		return (cascade & level) != 0;
	}

	public bool CascadeTo(int level)
	{
		return CascadeTo(GetCascadeLevel(), level);
	}

	public static string GetTopPoolCountReport(int num = 20)
	{
		List<string> list = new List<string>(EventPoolCounts.Count);
		Dictionary<string, int> Counts = new Dictionary<string, int>(EventPoolCounts.Count);
		foreach (KeyValuePair<string, EventPoolCount> eventPoolCount in EventPoolCounts)
		{
			list.Add(eventPoolCount.Key);
			Counts.Add(eventPoolCount.Key, eventPoolCount.Value());
		}
		list.Sort(delegate(string a, string b)
		{
			int num3 = Counts[b].CompareTo(Counts[a]);
			return (num3 != 0) ? num3 : a.CompareTo(b);
		});
		StringBuilder stringBuilder = Event.NewStringBuilder();
		int i = 0;
		for (int num2 = Math.Min(list.Count, num + 1); i < num2; i++)
		{
			stringBuilder.Append(list[i]).Append(": ").Append(Counts[list[i]])
				.Append('\n');
		}
		return stringBuilder.ToString();
	}

	public virtual bool handlePartDispatch(IPart part)
	{
		Debug.LogError("base handlePartDispatch called for " + GetType().Name);
		return true;
	}

	public virtual bool handleEffectDispatch(Effect effect)
	{
		Debug.LogError("base handlePartDispatch called for " + GetType().Name);
		return true;
	}

	public bool ActuateOn(GameObject obj)
	{
		if (obj.WantEvent(GetID(), GetCascadeLevel()))
		{
			return obj.HandleEvent(this);
		}
		return true;
	}
}

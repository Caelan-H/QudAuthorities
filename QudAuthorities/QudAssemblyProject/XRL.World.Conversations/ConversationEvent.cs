using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRL.World.Conversations;

public abstract class ConversationEvent
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ParameterAttribute : Attribute, IComparable<ParameterAttribute>
	{
		public object Default;

		public bool Reference;

		public bool Output;

		public bool Required;

		public bool Exclude;

		public Type Input;

		public bool Get;

		public int CompareTo(ParameterAttribute Other)
		{
			if (Other == null)
			{
				if (Required || Reference || Output)
				{
					return -1;
				}
				return 0;
			}
			if (Output)
			{
				if (Other.Output)
				{
					return 0;
				}
				if (Other.Reference)
				{
					return 1;
				}
				if (Other.Required)
				{
					return 1;
				}
			}
			else if (Reference)
			{
				if (Other.Reference)
				{
					return 0;
				}
				if (Other.Required)
				{
					return 1;
				}
				if (Other.Output)
				{
					return -1;
				}
			}
			else if (Required)
			{
				if (!Other.Required)
				{
					return -1;
				}
				if (Other.Reference)
				{
					return -1;
				}
				if (Other.Output)
				{
					return -1;
				}
			}
			return 0;
		}
	}

	public enum Action
	{
		Custom,
		Send,
		Get,
		Check
	}

	public enum Instantiation
	{
		Custom,
		Pooling,
		Stack,
		Singleton
	}

	public readonly int ID;

	[Parameter(Required = true)]
	public IConversationElement Element;

	private static List<MinEvent.EventPoolReset> EventPoolResets = new List<MinEvent.EventPoolReset>();

	private static Dictionary<string, MinEvent.EventPoolCount> EventPoolCounts = new Dictionary<string, MinEvent.EventPoolCount>();

	private static int IDSequence;

	public static int AllocateID()
	{
		return ++IDSequence;
	}

	public ConversationEvent()
	{
	}

	public ConversationEvent(int ID)
	{
		this.ID = ID;
	}

	public virtual bool HandlePartDispatch(IConversationPart Part)
	{
		Debug.LogError("Base HandlePartDispatch called for " + GetType().Name);
		return true;
	}

	public virtual void Reset()
	{
		Element = null;
	}

	public static void RegisterPoolReset(MinEvent.EventPoolReset R)
	{
		EventPoolResets.Add(R);
	}

	public static void RegisterPoolCount(string ClassName, MinEvent.EventPoolCount C)
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
		foreach (MinEvent.EventPoolReset eventPoolReset in EventPoolResets)
		{
			eventPoolReset();
		}
	}

	protected static T FromPool<T>(ref List<T> Pool, ref int Counter, int MaxPool = 8192) where T : ConversationEvent, new()
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
}

using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetItemElementsEvent : IActOnItemEvent
{
	public GameObject Kill;

	public GameObject InfluencedBy;

	public Dictionary<string, int> Weights = new Dictionary<string, int>();

	public new static readonly int ID;

	private static List<GetItemElementsEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		if (!base.handlePartDispatch(Part))
		{
			return false;
		}
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		if (!base.handleEffectDispatch(Effect))
		{
			return false;
		}
		return Effect.HandleEvent(this);
	}

	static GetItemElementsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetItemElementsEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetItemElementsEvent()
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

	public override void Reset()
	{
		Kill = null;
		InfluencedBy = null;
		Weights.Clear();
		base.Reset();
	}

	public void Add(string element, int weight)
	{
		if (Weights.TryGetValue(element, out var value))
		{
			int num = value + weight;
			if (num != 0)
			{
				Weights[element] = num;
			}
			else
			{
				Weights.Remove(element);
			}
		}
		else if (weight != 0)
		{
			Weights.Add(element, weight);
		}
	}

	public static GetItemElementsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetItemElementsEvent FromPool(GameObject Actor, GameObject Item, GameObject Kill = null, GameObject InfluencedBy = null)
	{
		GetItemElementsEvent getItemElementsEvent = FromPool();
		getItemElementsEvent.Actor = Actor;
		getItemElementsEvent.Item = Item;
		getItemElementsEvent.Kill = Kill;
		getItemElementsEvent.InfluencedBy = InfluencedBy;
		return getItemElementsEvent;
	}

	public static Dictionary<string, int> GetFor(GameObject Actor, GameObject Item, GameObject Kill = null, GameObject InfluencedBy = null)
	{
		bool flag = GameObject.validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel);
		bool flag2 = GameObject.validate(ref Item) && Item.WantEvent(ID, MinEvent.CascadeLevel);
		bool flag3 = GameObject.validate(ref Kill) && Kill.WantEvent(ID, MinEvent.CascadeLevel);
		bool flag4 = GameObject.validate(ref InfluencedBy) && InfluencedBy.WantEvent(ID, MinEvent.CascadeLevel);
		if (flag || flag2 || flag3 || flag4)
		{
			GetItemElementsEvent getItemElementsEvent = FromPool(Actor, Item, Kill, InfluencedBy);
			bool flag5 = true;
			if (flag5 && flag && !Actor.HandleEvent(getItemElementsEvent))
			{
				flag5 = false;
			}
			if (flag5 && flag2 && !Item.HandleEvent(getItemElementsEvent))
			{
				flag5 = false;
			}
			if (flag5 && flag3 && !Kill.HandleEvent(getItemElementsEvent))
			{
				flag5 = false;
			}
			if (flag5 && flag4 && !InfluencedBy.HandleEvent(getItemElementsEvent))
			{
				flag5 = false;
			}
			if (getItemElementsEvent.Weights.Count > 0)
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>(getItemElementsEvent.Weights.Count);
				{
					foreach (KeyValuePair<string, int> weight in getItemElementsEvent.Weights)
					{
						dictionary.Add(weight.Key, weight.Value);
					}
					return dictionary;
				}
			}
		}
		return null;
	}
}

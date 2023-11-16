using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class DerivationCreatedEvent : IDerivationEvent
{
	public GameObject Original;

	public List<IPart> PartsToRemove;

	public new static readonly int ID;

	private static List<DerivationCreatedEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 17;

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

	static DerivationCreatedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(DerivationCreatedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public DerivationCreatedEvent()
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

	public static DerivationCreatedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Original = null;
		PartsToRemove = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public void WantToRemove(IPart Part)
	{
		if (PartsToRemove != null)
		{
			if (!PartsToRemove.Contains(Part))
			{
				PartsToRemove.Add(Part);
			}
		}
		else
		{
			Object.RemovePart(Part);
		}
	}

	public static void Send(GameObject Object, GameObject Actor, GameObject Original, string Context = null)
	{
		List<IPart> list = null;
		try
		{
			if (GameObject.validate(ref Object) && Object.HasRegisteredEvent("DerivationCreated"))
			{
				if (list == null)
				{
					list = new List<IPart>();
				}
				Event @event = Event.New("DerivationCreated");
				@event.SetParameter("Object", Object);
				@event.SetParameter("Actor", Actor);
				@event.SetParameter("Context", Context);
				@event.SetParameter("Original", Original);
				@event.SetParameter("PartsToRemove", list);
				Object.FireEvent(@event);
			}
			if (GameObject.validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
			{
				if (list == null)
				{
					list = new List<IPart>();
				}
				DerivationCreatedEvent derivationCreatedEvent = FromPool();
				derivationCreatedEvent.Object = Object;
				derivationCreatedEvent.Actor = Actor;
				derivationCreatedEvent.Original = Original;
				derivationCreatedEvent.Context = Context;
				derivationCreatedEvent.PartsToRemove = list;
				Object.HandleEvent(derivationCreatedEvent);
			}
		}
		finally
		{
			if (list != null)
			{
				foreach (IPart item in list)
				{
					try
					{
						Object.RemovePart(item);
					}
					catch (Exception message)
					{
						MetricsManager.LogError(message);
					}
				}
			}
		}
		if (Object?.pRender != null)
		{
			if (Object.OriginalColorString != null)
			{
				Object.pRender.ColorString = Object.OriginalColorString;
			}
			if (Object.OriginalDetailColor != null)
			{
				Object.pRender.DetailColor = Object.OriginalDetailColor;
			}
			if (Object.OriginalTileColor != null)
			{
				Object.pRender.TileColor = Object.OriginalTileColor;
			}
			Object.UpdateVisibleStatusColor();
		}
	}
}

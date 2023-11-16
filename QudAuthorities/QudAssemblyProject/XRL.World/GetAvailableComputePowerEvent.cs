using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetAvailableComputePowerEvent : MinEvent
{
	public GameObject Actor;

	public int Amount;

	public new static readonly int ID;

	public static GetAvailableComputePowerEvent instance;

	public new static int CascadeLevel => 3;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetAvailableComputePowerEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public GetAvailableComputePowerEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Actor = null;
		Amount = 0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static int GetFor(GameObject Actor)
	{
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			if (instance == null)
			{
				instance = new GetAvailableComputePowerEvent();
			}
			instance.Actor = Actor;
			instance.Amount = 0;
			Actor.HandleEvent(instance);
			return instance.Amount;
		}
		return 0;
	}

	public static int GetFor(IActivePart Part)
	{
		int num = 0;
		if (Part.ActivePartHasMultipleSubjects())
		{
			List<GameObject> activePartSubjects = Part.GetActivePartSubjects();
			int i = 0;
			for (int count = activePartSubjects.Count; i < count; i++)
			{
				num += GetFor(activePartSubjects[i]);
			}
		}
		else
		{
			GameObject activePartFirstSubject = Part.GetActivePartFirstSubject();
			if (activePartFirstSubject != null)
			{
				num += GetFor(activePartFirstSubject);
			}
		}
		return num;
	}

	public static int AdjustUp(GameObject Actor, int Amount)
	{
		if (Amount != 0)
		{
			int @for = GetFor(Actor);
			if (@for != 0)
			{
				Amount = Amount * (100 + @for) / 100;
			}
		}
		return Amount;
	}

	public static int AdjustUp(GameObject Actor, int Amount, float Factor)
	{
		if (Amount != 0 && Factor != 0f)
		{
			int @for = GetFor(Actor);
			if (@for != 0)
			{
				Amount = (int)((float)Amount * (100f + (float)@for * Factor) / 100f);
			}
		}
		return Amount;
	}

	public static float AdjustUp(GameObject Actor, float Amount)
	{
		if (Amount != 0f)
		{
			int @for = GetFor(Actor);
			if (@for != 0)
			{
				Amount = Amount * (float)(100 + @for) / 100f;
			}
		}
		return Amount;
	}

	public static float AdjustUp(GameObject Actor, float Amount, float Factor)
	{
		if (Amount != 0f && Factor != 0f)
		{
			int @for = GetFor(Actor);
			if (@for != 0)
			{
				Amount = Amount * (100f + (float)@for * Factor) / 100f;
			}
		}
		return Amount;
	}

	public static int AdjustDown(GameObject Actor, int Amount, int FloorDivisor = 2)
	{
		if (Amount != 0)
		{
			int @for = GetFor(Actor);
			if (@for != 0)
			{
				Amount = Math.Max(Amount * (100 - @for) / 100, Amount / FloorDivisor);
			}
		}
		return Amount;
	}

	public static int AdjustDown(GameObject Actor, int Amount, float Factor, int FloorDivisor = 2)
	{
		if (Amount != 0)
		{
			int @for = GetFor(Actor);
			if (@for != 0)
			{
				Amount = Math.Max((int)((float)Amount * (100f - (float)@for * Factor) / 100f), Amount / FloorDivisor);
			}
		}
		return Amount;
	}

	public static float AdjustDown(GameObject Actor, float Amount, int FloorDivisor = 2)
	{
		if (Amount != 0f)
		{
			int @for = GetFor(Actor);
			if (@for != 0)
			{
				Amount = Math.Max(Amount * (float)(100 - @for) / 100f, Amount / (float)FloorDivisor);
			}
		}
		return Amount;
	}

	public static float AdjustDown(GameObject Actor, float Amount, float Factor, int FloorDivisor = 2)
	{
		if (Amount != 0f)
		{
			int @for = GetFor(Actor);
			if (@for != 0)
			{
				Amount = Math.Max(Amount * (100f - (float)@for * Factor) / 100f, Amount / (float)FloorDivisor);
			}
		}
		return Amount;
	}

	public static int AdjustUp(IActivePart Part, int Amount)
	{
		if (Amount != 0)
		{
			int @for = GetFor(Part);
			if (@for != 0)
			{
				Amount = Amount * (100 + @for) / 100;
			}
		}
		return Amount;
	}

	public static int AdjustUp(IActivePart Part, int Amount, float Factor)
	{
		if (Amount != 0 && Factor != 0f)
		{
			int @for = GetFor(Part);
			if (@for != 0)
			{
				Amount = (int)((float)Amount * (100f + (float)@for * Factor) / 100f);
			}
		}
		return Amount;
	}

	public static float AdjustUp(IActivePart Part, float Amount)
	{
		if (Amount != 0f)
		{
			int @for = GetFor(Part);
			if (@for != 0)
			{
				Amount = Amount * (float)(100 + @for) / 100f;
			}
		}
		return Amount;
	}

	public static float AdjustUp(IActivePart Part, float Amount, float Factor)
	{
		if (Amount != 0f && Factor != 0f)
		{
			int @for = GetFor(Part);
			if (@for != 0)
			{
				Amount = Amount * (100f + (float)@for * Factor) / 100f;
			}
		}
		return Amount;
	}

	public static int AdjustDown(IActivePart Part, int Amount, int FloorDivisor = 2)
	{
		if (Amount != 0)
		{
			int @for = GetFor(Part);
			if (@for != 0)
			{
				Amount = Math.Max(Amount * (100 - @for) / 100, Amount / FloorDivisor);
			}
		}
		return Amount;
	}

	public static int AdjustDown(IActivePart Part, int Amount, float Factor, int FloorDivisor = 2)
	{
		if (Amount != 0)
		{
			int @for = GetFor(Part);
			if (@for != 0)
			{
				Amount = Math.Max((int)((float)Amount * (100f - (float)@for * Factor) / 100f), Amount / FloorDivisor);
			}
		}
		return Amount;
	}

	public static float AdjustDown(IActivePart Part, float Amount, float FloorDivisor = 2f)
	{
		if (Amount != 0f)
		{
			int @for = GetFor(Part);
			if (@for != 0)
			{
				Amount = Math.Max(Amount * (float)(100 - @for) / 100f, Amount / FloorDivisor);
			}
		}
		return Amount;
	}

	public static float AdjustDown(IActivePart Part, float Amount, float Factor, float FloorDivisor = 2f)
	{
		if (Amount != 0f)
		{
			int @for = GetFor(Part);
			if (@for != 0)
			{
				Amount = Math.Max(Amount * (100f - (float)@for * Factor) / 100f, Amount / FloorDivisor);
			}
		}
		return Amount;
	}
}

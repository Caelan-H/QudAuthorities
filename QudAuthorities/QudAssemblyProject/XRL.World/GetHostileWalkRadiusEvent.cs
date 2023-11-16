using System;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetHostileWalkRadiusEvent : IActOnItemEvent
{
	public int Radius;

	public new static readonly int ID;

	private static List<GetHostileWalkRadiusEvent> Pool;

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

	static GetHostileWalkRadiusEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetHostileWalkRadiusEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetHostileWalkRadiusEvent()
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

	public static GetHostileWalkRadiusEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetHostileWalkRadiusEvent FromPool(GameObject Actor, GameObject Item, int Radius)
	{
		GetHostileWalkRadiusEvent getHostileWalkRadiusEvent = FromPool();
		getHostileWalkRadiusEvent.Actor = Actor;
		getHostileWalkRadiusEvent.Item = Item;
		getHostileWalkRadiusEvent.Radius = Radius;
		return getHostileWalkRadiusEvent;
	}

	public override void Reset()
	{
		Radius = 0;
		base.Reset();
	}

	public void MaxRadius(int Radius)
	{
		if (this.Radius < Radius)
		{
			this.Radius = Radius;
		}
	}

	public static int GetFor(GameObject Actor, GameObject Item)
	{
		int num = 84;
		if (Item.pBrain != null)
		{
			Item.pBrain.checkMobility(out var immobile, out var waterbound, out var wallwalker);
			if ((immobile || waterbound || wallwalker) && !Item.HasReadyMissileWeapon())
			{
				if (immobile)
				{
					num = ((!Item.HasPart("Combat")) ? 1 : 2);
				}
				else if (waterbound && wallwalker)
				{
					num = 1;
					if (Actor != null)
					{
						List<Tuple<Cell, char>> lineTo = Item.GetLineTo(Actor);
						if (lineTo != null)
						{
							int i = 1;
							for (int num2 = Math.Min(lineTo.Count, Item.pBrain.HostileWalkRadius); i < num2 && (lineTo[i].Item1.HasAquaticSupportFor(Item) || lineTo[i].Item1.HasWalkableWallFor(Item)); i++)
							{
								num++;
							}
						}
						if (num < Item.pBrain.HostileWalkRadius)
						{
							num++;
						}
					}
				}
				else if (waterbound)
				{
					num = 1;
					if (Actor != null)
					{
						List<Tuple<Cell, char>> lineTo2 = Item.GetLineTo(Actor);
						if (lineTo2 != null)
						{
							int j = 1;
							for (int num3 = Math.Min(lineTo2.Count, Item.pBrain.HostileWalkRadius); j < num3 && lineTo2[j].Item1.HasAquaticSupportFor(Item); j++)
							{
								num++;
							}
						}
						if (num < Item.pBrain.HostileWalkRadius)
						{
							num++;
						}
					}
				}
				else if (wallwalker)
				{
					num = 1;
					if (Actor != null)
					{
						List<Tuple<Cell, char>> lineTo3 = Item.GetLineTo(Actor);
						if (lineTo3 != null)
						{
							int k = 1;
							for (int num4 = Math.Min(lineTo3.Count, Item.pBrain.HostileWalkRadius); k < num4 && lineTo3[k].Item1.HasWalkableWallFor(Item); k++)
							{
								num++;
							}
						}
						if (num < Item.pBrain.HostileWalkRadius)
						{
							num++;
						}
					}
				}
				else
				{
					num = Item.pBrain.HostileWalkRadius;
				}
			}
		}
		if (Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetHostileWalkRadiusEvent getHostileWalkRadiusEvent = FromPool(Actor, Item, num);
			Item.HandleEvent(getHostileWalkRadiusEvent);
			num = getHostileWalkRadiusEvent.Radius;
		}
		return num;
	}
}

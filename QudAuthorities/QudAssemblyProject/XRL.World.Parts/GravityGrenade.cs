using System;
using System.Collections.Generic;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class GravityGrenade : IGrenade
{
	public int Force = 1000;

	public int Radius = 2;

	public int ForceDropoff = 500;

	public bool RealityDistortionBased = true;

	public override bool SameAs(IPart p)
	{
		GravityGrenade gravityGrenade = p as GravityGrenade;
		if (gravityGrenade.Force != Force)
		{
			return false;
		}
		if (gravityGrenade.Radius != Radius)
		{
			return false;
		}
		if (gravityGrenade.ForceDropoff != ForceDropoff)
		{
			return false;
		}
		if (gravityGrenade.RealityDistortionBased != RealityDistortionBased)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetComponentNavigationWeightEvent.ID)
		{
			return ID == GetComponentAdjacentNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		E.MinWeight(5);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(5);
		return base.HandleEvent(E);
	}

	protected override bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		if (RealityDistortionBased && !ParentObject.FireEvent("CheckRealityDistortionUsability"))
		{
			return false;
		}
		int phase = ParentObject.GetPhase();
		PlayWorldSound(GetPropertyOrTag("DetonatedSound"), 1f, 0f, combat: true);
		DidX("implode", null, "!");
		ParentObject.Destroy(null, Silent: true);
		C.ImplosionSplat(Radius);
		List<Cell> list = new List<Cell>();
		list.Add(C);
		List<GameObject> list2 = Event.NewGameObjectList();
		int num = Force;
		Event e = (RealityDistortionBased ? Event.New("CheckRealityDistortionAccessibility") : null);
		for (int i = 1; i <= Radius; i++)
		{
			if (num <= 0)
			{
				break;
			}
			List<Cell> list3 = C.GetRealAdjacentCells(i).ShuffleInPlace();
			int j = 0;
			for (int count = list3.Count; j < count; j++)
			{
				Cell cell = list3[j];
				if (cell.Objects.Count > 0 && !list.Contains(cell))
				{
					list2.AddRange(cell.Objects);
				}
			}
			list.AddRange(list3);
			list2.ShuffleInPlace();
			int k = 0;
			for (int count2 = list2.Count; k < count2; k++)
			{
				GameObject gameObject = list2[k];
				if (gameObject.CurrentCell != null && !gameObject.IsInGraveyard() && list3.Contains(gameObject.CurrentCell) && gameObject.PhaseMatches(phase) && (!RealityDistortionBased || gameObject.CurrentCell.FireEvent(e)))
				{
					if (gameObject.IsFlying)
					{
						Flight.Fall(gameObject);
					}
					gameObject.Accelerate(num, null, C, null, "Artificial Gravitation", Actor, Accidental: false, ApparentTarget, null, 0.1, SuspendFalling: true, OneShort: false, Repeat: false, BuiltOnly: true, MessageForInanimate: false, DelayForDisplay: false);
				}
			}
			list2.Clear();
			num -= ForceDropoff;
		}
		return true;
	}
}

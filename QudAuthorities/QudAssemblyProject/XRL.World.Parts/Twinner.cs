using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Twinner : IPart
{
	public GameObject Twin;

	public override bool SameAs(IPart p)
	{
		if ((p as Twinner).Twin != null || Twin != null)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != IsSensableAsPsychicEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == OnDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		E.Sensable = true;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDeathRemovalEvent E)
	{
		Untwin();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		Untwin();
		return base.HandleEvent(E);
	}

	private void Untwin()
	{
		if (GameObject.validate(ref Twin))
		{
			if (Twin.GetPart("Twinner") is Twinner twinner)
			{
				twinner.Twin = null;
			}
			Twin = null;
		}
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AITakingAction");
		base.Register(Object);
	}

	public void Spawn()
	{
		if (!ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this)))
		{
			return;
		}
		if (GameObject.validate(ref Twin))
		{
			Twin.Obliterate();
		}
		foreach (Cell item in new List<Cell>(ParentObject.CurrentCell.GetLocalAdjacentCells()).ShuffleInPlace())
		{
			if (!item.IsEmpty())
			{
				continue;
			}
			if (!item.FireEvent(Event.New("InitiateRealityDistortionRemote", "Object", ParentObject, "Mutation", this)))
			{
				break;
			}
			Twin = GameObject.createUnmodified(ParentObject.Blueprint);
			Twin.RemovePart("RandomLoot");
			Twin.Statistics["XPValue"].BaseValue = 0;
			Twin.MakeActive();
			item.AddObject(Twin);
			Twin.RequirePart<Twinner>().Twin = ParentObject;
			Twin.TakeOnAttitudesOf(ParentObject, CopyLeader: true, CopyTarget: true);
			if (IComponent<GameObject>.Visible(Twin))
			{
				for (int i = 0; i < 10; i++)
				{
					XRLCore.ParticleManager.AddRadial("&Mù", item.X, item.Y, Stat.Random(0, 5), Stat.Random(5, 10), 0.01f * (float)Stat.Random(4, 6), -0.05f * (float)Stat.Random(3, 7));
				}
			}
			break;
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AITakingAction" && !GameObject.validate(ref Twin) && ParentObject.IsValid() && ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)))
		{
			Spawn();
		}
		return base.FireEvent(E);
	}
}

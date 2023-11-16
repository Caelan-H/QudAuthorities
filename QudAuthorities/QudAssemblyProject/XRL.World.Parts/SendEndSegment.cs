using System;

namespace XRL.World.Parts;

[Serializable]
public class SendEndSegment : IPart
{
	public int Segments = 60;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectEnteringCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		int num = (int)(1000f / (float)E.Object.Stat("Speed") * (float)Segments);
		Event.New("EndSegment");
		for (int i = 0; i < num; i++)
		{
			EndSegmentEvent.Send(E.Object);
		}
		return base.HandleEvent(E);
	}
}

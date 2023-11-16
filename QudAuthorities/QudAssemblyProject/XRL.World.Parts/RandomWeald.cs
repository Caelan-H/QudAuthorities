using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class RandomWeald : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		switch (Stat.Random(1, 4))
		{
		case 1:
			ParentObject.pRender.ColorString = "&w";
			break;
		case 2:
			ParentObject.pRender.ColorString = "&W";
			break;
		case 3:
			ParentObject.pRender.ColorString = "&G";
			break;
		case 4:
			ParentObject.pRender.ColorString = "&g";
			break;
		}
		if (Stat.Random(0, 1) == 0)
		{
			ParentObject.pRender.ColorString = ParentObject.pRender.ColorString.ToLower();
		}
		int num = Stat.Random(1, 10);
		if (num == 1)
		{
			ParentObject.pRender.RenderString = ",";
		}
		if (num == 2)
		{
			ParentObject.pRender.RenderString = ".";
		}
		if (num >= 3 && num <= 6)
		{
			ParentObject.pRender.RenderString = "õ";
		}
		if (num > 6 && num <= 9)
		{
			ParentObject.pRender.RenderString = "\u009d";
		}
		if (num == 10)
		{
			ParentObject.pRender.RenderString = "\u009f";
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}
}

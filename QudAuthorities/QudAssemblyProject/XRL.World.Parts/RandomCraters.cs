using System;
using Genkit;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class RandomCraters : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		SetupCraters();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneLoaded")
		{
			SetupCraters();
		}
		return base.FireEvent(E);
	}

	public static int GetSeededRange(string Seed, int Low, int High)
	{
		return new Random(Hash.String(Seed)).Next(Low, High);
	}

	private void SetupCraters()
	{
		Render pRender = ParentObject.pRender;
		int num = Stat.Random(1, 2);
		int num2 = Stat.Random(1, 15);
		if (ParentObject.HasProperty("SmallCrater"))
		{
			num2 = 2;
		}
		if (ParentObject.HasProperty("BigCrater"))
		{
			num2 = 4;
		}
		int x = ParentObject.pPhysics.CurrentCell.X;
		x += Stat.Random(-1, 1);
		if (x < 50)
		{
			if (num == 1)
			{
				pRender.ColorString = "&K";
			}
			if (num == 2)
			{
				pRender.ColorString = "&K";
			}
			pRender.ColorString += "^Y";
		}
		else if (x < 55)
		{
			if (num == 1)
			{
				pRender.ColorString = "&K";
			}
			if (num == 2)
			{
				pRender.ColorString = "&K";
			}
			pRender.ColorString += "^y";
		}
		else if (x < 60)
		{
			if (num == 1)
			{
				pRender.ColorString = "&y";
			}
			if (num == 2)
			{
				pRender.ColorString = "&Y";
			}
			pRender.ColorString += "^K";
		}
		else
		{
			if (num == 1)
			{
				pRender.ColorString = "&y";
			}
			if (num == 2)
			{
				pRender.ColorString = "&K";
			}
			pRender.ColorString += "^k";
		}
		if (num2 == 1)
		{
			pRender.RenderString = ":";
		}
		if (num2 == 2)
		{
			pRender.RenderString = ".";
		}
		if (num2 == 3)
		{
			pRender.RenderString = "o";
		}
		if (num2 == 4)
		{
			pRender.RenderString = "O";
		}
		if (num2 == 5)
		{
			pRender.RenderString = "0";
		}
		if (num2 > 5)
		{
			pRender.RenderString = " ";
		}
		int num3 = Stat.Random(1, 4);
		if (num3 == 1)
		{
			pRender.DetailColor = "r";
		}
		if (num3 == 2)
		{
			pRender.DetailColor = "b";
		}
		if (num3 == 3)
		{
			pRender.DetailColor = "m";
		}
		if (num3 == 4)
		{
			pRender.DetailColor = "K";
		}
		ParentObject.RemovePart(this);
	}
}

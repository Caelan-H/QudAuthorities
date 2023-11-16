using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class FeralLahPod : IPart
{
	public string damage = "1d10";

	public bool exploding;

	public int countdown = 2;

	public bool blowingup;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == BeforeRenderEvent.ID)
			{
				return exploding;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (exploding)
		{
			int num = XRLCore.CurrentFrame % 18 / 6;
			if (num == 0)
			{
				ParentObject.pRender.TileColor = "&R^r";
				ParentObject.pRender.DetailColor = "W";
				ParentObject.pRender.ColorString = "&R^r";
			}
			if (num == 1)
			{
				ParentObject.pRender.TileColor = "&W^R";
				ParentObject.pRender.DetailColor = "r";
				ParentObject.pRender.ColorString = "&W^R";
			}
			if (num == 2)
			{
				ParentObject.pRender.TileColor = "&r^W";
				ParentObject.pRender.DetailColor = "R";
				ParentObject.pRender.ColorString = "&r^W";
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeDie");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "EndAction");
		base.Register(Object);
	}

	public void Explode()
	{
		if (blowingup)
		{
			return;
		}
		if (ParentObject.HasStat("XP"))
		{
			ParentObject.GetStat("XPValue").BaseValue = 0;
		}
		blowingup = true;
		Cell cell = ParentObject.GetCurrentCell();
		if (cell != null)
		{
			List<Cell> list = new List<Cell>();
			cell.GetAdjacentCells(2, list);
			List<Cell> list2 = new List<Cell>();
			foreach (Cell item in list)
			{
				if (ParentObject.HasUnobstructedLineTo(item))
				{
					list2.Add(item);
				}
			}
			if (cell.ParentZone.IsActive())
			{
				for (int i = 0; i < 3; i++)
				{
					TextConsole textConsole = Look._TextConsole;
					ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
					XRLCore.Core.RenderMapToBuffer(scrapBuffer);
					foreach (Cell item2 in list2)
					{
						if (item2.PathDistanceTo(cell) == i && item2.IsVisible())
						{
							scrapBuffer.Goto(item2.X, item2.Y);
							if (Stat.RandomCosmetic(1, 2) == 1)
							{
								scrapBuffer.Write("&R*");
							}
							else
							{
								scrapBuffer.Write("&W*");
							}
						}
					}
					textConsole.DrawBuffer(scrapBuffer);
					Thread.Sleep(10);
				}
			}
			foreach (Cell item3 in list2)
			{
				foreach (GameObject item4 in item3.GetObjectsInCell())
				{
					if (item4.PhaseMatches(ParentObject))
					{
						item4.TakeDamage(damage.RollCached(), "from %t explosion!", "Explosion", null, null, null, ParentObject);
					}
				}
			}
		}
		ParentObject.Destroy();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDie")
		{
			Explode();
		}
		else if (E.ID == "BeginTakeAction")
		{
			if (XRLCore.Core.Calm)
			{
				return true;
			}
			if (ParentObject.pBrain.Target != null && ParentObject.pBrain.Target.DistanceTo(ParentObject) <= 1)
			{
				exploding = true;
			}
			if (exploding)
			{
				countdown--;
				if (countdown <= 0)
				{
					Explode();
				}
				return false;
			}
		}
		else if (E.ID == "EndAction")
		{
			if (XRLCore.Core.Calm)
			{
				return true;
			}
			if (ParentObject.pBrain.Target != null && ParentObject.pBrain.Target.DistanceTo(ParentObject) <= 1)
			{
				exploding = true;
			}
		}
		return base.FireEvent(E);
	}
}

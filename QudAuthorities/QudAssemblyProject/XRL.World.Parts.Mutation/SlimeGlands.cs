using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SlimeGlands : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public SlimeGlands()
	{
		DisplayName = "Slime Glands";
		Type = "Physical";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeforeApplyDamage");
		Object.RegisterPartEvent(this, "CommandSpitSlime");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("" + "You produce a viscous slime that you can spit at things.\n\n", "Covers an area with slime\n"), "Range: 8\n"), "Area: 3x3\n"), "Cooldown: 40 rounds\n"), "You can walk over slime without slipping.");
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public static void SlimeAnimation(string Color, Cell StartCell, Cell EndCell)
	{
		if (StartCell == null || EndCell == null || StartCell.ParentZone == null)
		{
			return;
		}
		List<Point> list = Zone.Line(StartCell.X, StartCell.Y, EndCell.X, EndCell.Y);
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
		TextConsole textConsole = Popup._TextConsole;
		CleanQueue<Point> cleanQueue = new CleanQueue<Point>();
		Point point = null;
		foreach (Point item in list)
		{
			point = item;
			bool flag = false;
			cleanQueue.Enqueue(item);
			while (cleanQueue.Count > 3)
			{
				cleanQueue.Dequeue();
			}
			int num = 0;
			for (int i = 0; i < cleanQueue.Items.Count; i++)
			{
				Point point2 = cleanQueue.Items[i];
				if (StartCell.ParentZone.GetCell(point2.X, point2.Y).IsVisible())
				{
					if (!flag)
					{
						flag = true;
						XRLCore.Core.RenderBaseToBuffer(scrapBuffer);
					}
					scrapBuffer.Goto(point2.X, point2.Y);
					if (cleanQueue.Count == 1)
					{
						scrapBuffer.Write(Color + "\a");
					}
					else if (cleanQueue.Count == 2)
					{
						if (num == 0)
						{
							scrapBuffer.Write(Color + "ú");
						}
						if (num == 1)
						{
							scrapBuffer.Write(Color + "\a");
						}
					}
					else
					{
						if (num == 0)
						{
							scrapBuffer.Write(Color + "ù");
						}
						if (num == 1)
						{
							scrapBuffer.Write(Color + "ú");
						}
						if (num == 2)
						{
							scrapBuffer.Write(Color + "\a");
						}
					}
				}
				num++;
			}
			if (flag)
			{
				textConsole.DrawBuffer(scrapBuffer);
				Thread.Sleep(30);
			}
		}
		if (point != null)
		{
			for (int j = 0; j < 5; j++)
			{
				float num2 = 0f;
				float num3 = 0f;
				float num4 = (float)Stat.Random(0, 359) / 58f;
				num2 = (float)Math.Sin(num4) / 2f;
				num3 = (float)Math.Cos(num4) / 2f;
				XRLCore.ParticleManager.Add(Color + ".", point.X, point.Y, num2, num3, 5, 0f, 0f);
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") < 8 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && !ParentObject.IsFrozen() && !ParentObject.OnWorldMap())
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
				if (ParentObject.HasLOSTo(gameObjectParameter, IncludeSolid: true, UseTargetability: true))
				{
					E.AddAICommand("CommandSpitSlime");
				}
			}
		}
		else if (E.ID == "CommandSpitSlime")
		{
			if (!ParentObject.CheckFrozen())
			{
				return false;
			}
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot do that on the world map.");
				}
				return false;
			}
			List<Cell> list = PickBurst(1, 8, bLocked: false, AllowVis.OnlyVisible);
			if (list == null)
			{
				return false;
			}
			foreach (Cell item in list)
			{
				if (item.DistanceTo(ParentObject) > 8)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("That is out of range! (8 squares)");
					}
					return false;
				}
			}
			SlimeAnimation("&g", ParentObject.CurrentCell, list[0]);
			CooldownMyActivatedAbility(ActivatedAbilityID, 40);
			int num = 0;
			foreach (Cell item2 in list)
			{
				if (num == 0 || 80.in100())
				{
					item2.AddObject("SlimePuddle");
				}
				num++;
			}
			UseEnergy(1000, "Physical Mutation Slime Glands");
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		GO.Slimewalking = true;
		ActivatedAbilityID = AddMyActivatedAbility("Spit Slime", "CommandSpitSlime", "Physical Mutation", null, "­");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.Slimewalking = false;
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

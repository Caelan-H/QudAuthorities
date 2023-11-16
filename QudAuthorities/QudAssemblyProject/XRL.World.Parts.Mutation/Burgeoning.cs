using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Burgeoning : BaseMutation
{
	public const int PRIME_PLANT_CHANCE = 50;

	public new Guid ActivatedAbilityID = Guid.Empty;

	public bool QudzuOnly;

	[NonSerialized]
	public static string[] ColorList = new string[6] { "&R", "&G", "&B", "&M", "&Y", "&W" };

	public Burgeoning()
	{
		DisplayName = "Burgeoning";
		Type = "Mental";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandBurgeoning");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You cause plants to spontaneously grow in a nearby area, hindering your enemies.";
	}

	public override string GetLevelText(int Level)
	{
		int num = 115 - 10 * Level;
		if (num < 5)
		{
			num = 5;
		}
		string text = "";
		text += "Range: 8\n";
		text += "Area: 3x3 + growth into adjacent tiles\n";
		text = text + "Cooldown: {{rules|" + num + "}} rounds\n";
		if (Level != base.Level)
		{
			text += "More powerful plants summoned\n";
		}
		return text + "+200 reputation with {{w|the Consortium of Phyta}}";
	}

	public static string GetRandomRainbowColor()
	{
		return ColorList.GetRandomElement();
	}

	public static void GrowPlant(Cell C, bool bFriendly, GameObject Owner, int Level, bool QudzuOnly, string primePlant = null)
	{
		if (C.ParentZone.IsActive() && C.IsVisible())
		{
			TextConsole.LoadScrapBuffers();
			ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
			XRLCore.Core.RenderMapToBuffer(TextConsole.ScrapBuffer);
			scrapBuffer.Goto(C.X, C.Y);
			scrapBuffer.Write(GetRandomRainbowColor() + "\a");
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			Thread.Sleep(50);
			XRLCore.Core.RenderMapToBuffer(scrapBuffer);
			scrapBuffer.Goto(C.X, C.Y);
			scrapBuffer.Write(GetRandomRainbowColor() + "\u000f");
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			Thread.Sleep(25);
			if (Stat.Random(0, 1) == 0)
			{
				XRLCore.ParticleManager.AddSinusoidal(GetRandomRainbowColor() + "\r", C.X, C.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999);
			}
			else
			{
				XRLCore.ParticleManager.AddSinusoidal(GetRandomRainbowColor() + "\u000e", C.X, C.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999);
			}
		}
		GameObject gameObject = ((primePlant == null) ? GameObject.create(GetPlantByTier(Level, QudzuOnly)) : GameObject.create(primePlant));
		if (gameObject.pBrain != null)
		{
			if (bFriendly)
			{
				if (Owner.IsPlayer())
				{
					gameObject.IsTrifling = true;
				}
				gameObject.pBrain.SetFeeling(Owner, 100);
				gameObject.pBrain.PartyLeader = Owner;
				GameObject gameObject2 = Owner;
				while (gameObject2.pBrain != null && (gameObject2 = gameObject2.pBrain.PartyLeader) != null && gameObject2 != Owner)
				{
					gameObject.pBrain.SetFeeling(gameObject2, 100);
				}
				gameObject.UpdateVisibleStatusColor();
			}
			else
			{
				gameObject.pBrain.FactionMembership.Add("GerminatedPlants", 100);
			}
			gameObject.MakeActive();
			if (gameObject.HasStat("XPValue"))
			{
				gameObject.GetStat("XPValue").BaseValue = 0;
			}
		}
		C.AddObject(gameObject);
	}

	public static void PlantSummoning(List<Cell> Cells, bool bFriendly, GameObject Owner, int Level, bool bQudzuOnly)
	{
		List<Cell> list = new List<Cell>(32);
		string plantByTier = GetPlantByTier(Level);
		foreach (Cell Cell in Cells)
		{
			if (!Cell.IsOccluding())
			{
				if (50.in100())
				{
					GrowPlant(Cell, bFriendly, Owner, Level, bQudzuOnly, plantByTier);
				}
				else
				{
					GrowPlant(Cell, bFriendly, Owner, Level, bQudzuOnly);
				}
			}
			foreach (Cell adjacentCell in Cell.GetAdjacentCells())
			{
				if (!list.CleanContains(adjacentCell) && !Cells.CleanContains(adjacentCell))
				{
					list.Add(adjacentCell);
				}
			}
		}
		foreach (Cell item in list)
		{
			if (!item.IsOccluding() && 20.in100())
			{
				if (50.in100())
				{
					GrowPlant(item, bFriendly, Owner, Level, bQudzuOnly, plantByTier);
				}
				else
				{
					GrowPlant(item, bFriendly, Owner, Level, bQudzuOnly);
				}
			}
		}
	}

	public static string GetPlantByTier(int Level, bool bQudzuOnly = false)
	{
		int num = (int)(Math.Ceiling((float)Level / 2f) + (double)Stat.Random(-1, 2));
		if (num < 1)
		{
			num = 1;
		}
		if (num > 9)
		{
			num = 9;
		}
		GameObject gameObject = ((!bQudzuOnly) ? GameObject.create(PopulationManager.RollOneFrom("PlantSummoning" + num).Blueprint) : GameObject.create("Qudzu"));
		return gameObject.Blueprint;
	}

	public bool Burgeon()
	{
		List<Cell> list = PickBurst(1, 8, bLocked: false, AllowVis.OnlyVisible);
		if (list == null)
		{
			return false;
		}
		foreach (Cell item in list)
		{
			if (item.DistanceTo(ParentObject) > 9)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("That is out of range! (8 squares)");
				}
				return false;
			}
		}
		int turns = Math.Max(115 - 10 * base.Level, 5);
		CooldownMyActivatedAbility(ActivatedAbilityID, turns);
		PlantSummoning(list, bFriendly: true, ParentObject, base.Level, QudzuOnly);
		UseEnergy(1000, "Mental Mutation Burgeoning");
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			if (intParameter <= 8 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.HasLOSTo(gameObjectParameter, IncludeSolid: false))
			{
				E.AddAICommand("CommandBurgeoning");
			}
		}
		else if (E.ID == "CommandBurgeoning" && !Burgeon())
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Burgeoning", "CommandBurgeoning", "Mental Mutation", null, "\r");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

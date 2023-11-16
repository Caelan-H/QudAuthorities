using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class StunningForce : BaseMutation
{
	[Obsolete("save compat")]
	public int Placeholder;

	public new Guid ActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private static Dictionary<Cell, bool> VisitedCells = new Dictionary<Cell, bool>();

	[NonSerialized]
	private static CleanQueue<ConcussionEntry> CellQueue = new CleanQueue<ConcussionEntry>();

	[NonSerialized]
	private static List<GameObject> Pushed = new List<GameObject>();

	[NonSerialized]
	private static List<GameObject> Stunned = new List<GameObject>();

	[NonSerialized]
	private static string[] Dirs = new string[8] { "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public StunningForce()
	{
		DisplayName = "Stunning Force";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("might", 1);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandStunningForce");
		base.Register(Object);
	}

	public static string GetDamageIncrement(int Level)
	{
		if (Level < 3)
		{
			return "1d3";
		}
		return "1d3+" + (Level - 1) / 2;
	}

	public override string GetDescription()
	{
		return "You invoke a concussive force in a nearby area, throwing enemies back and stunning them.";
	}

	public static void ShockwaveAnimation(Cell StartCell, Cell EndCell)
	{
		if (StartCell == null || EndCell == null || StartCell.ParentZone == null)
		{
			return;
		}
		List<Point> list = Zone.Line(StartCell.X, StartCell.Y, EndCell.X, EndCell.Y);
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
		TextConsole textConsole = Popup._TextConsole;
		CleanQueue<Point> cleanQueue = new CleanQueue<Point>();
		foreach (Point item in list)
		{
			bool flag = false;
			cleanQueue.Enqueue(item);
			while (cleanQueue.Count > 3)
			{
				cleanQueue.Dequeue();
			}
			int num = 0;
			for (int i = 0; i < cleanQueue.Items.Count; i++)
			{
				Point point = cleanQueue.Items[i];
				if (StartCell.ParentZone.GetCell(point.X, point.Y).IsVisible())
				{
					if (!flag)
					{
						flag = true;
						XRLCore.Core.RenderBaseToBuffer(scrapBuffer);
					}
					int X = point.X + Stat.Random(-1, 1);
					int Y = point.Y + Stat.Random(-1, 1);
					scrapBuffer.Constrain(ref X, ref Y);
					scrapBuffer[point.X, point.Y].Copy(scrapBuffer[X, Y]);
					if (num == 0)
					{
						scrapBuffer[point.X, point.Y].Char = '\u00af';
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
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		text += "Range: 8\n";
		text += "Area: 7x7\n";
		text = ((Level != base.Level) ? (text + "{{rules|Increased stun save difficulty}}\n") : (text + "Creatures are pushed away from center of blast, stunned, and dealt crushing damage in up to 3 increments.\n"));
		text = text + "Damage increment: {{rules|" + GetDamageIncrement(Level) + "}}\n";
		return text + "Cooldown: 50 rounds";
	}

	public static void Concussion(Cell StartCell, GameObject ParentObject, int Level, int Distance, int Phase = 1, GameObject Target = null, bool Stun = true, bool Damage = true)
	{
		VisitedCells.Clear();
		CellQueue.Clear();
		Pushed.Clear();
		Stunned.Clear();
		CellQueue.Enqueue(new ConcussionEntry(StartCell, Distance, "-"));
		while (CellQueue.Count > 0)
		{
			ConcussionEntry concussionEntry = CellQueue.Dequeue();
			if (concussionEntry.Distance > 0)
			{
				Algorithms.RandomShuffleInPlace(Dirs);
				string[] dirs = Dirs;
				foreach (string text in dirs)
				{
					Cell localCellFromDirection = concussionEntry.C.GetLocalCellFromDirection(text);
					if (localCellFromDirection != null && !VisitedCells.ContainsKey(localCellFromDirection))
					{
						CellQueue.Enqueue(new ConcussionEntry(localCellFromDirection, concussionEntry.Distance - 1, text));
						VisitedCells.Add(localCellFromDirection, value: true);
					}
				}
			}
			foreach (GameObject item in concussionEntry.C.GetObjectsWithPart("Physics"))
			{
				if (item != ParentObject && !Pushed.Contains(item) && item.PhaseMatches(Phase) && !item.IsOpenLiquidVolume() && item.pRender != null && item.pRender.RenderLayer >= 1 && item.IsReal)
				{
					string text2 = concussionEntry.Direction;
					if (text2 == "-")
					{
						text2 = Directions.GetRandomDirection();
					}
					Pushed.Add(item);
					item.Push(text2, Level * 1000, 4);
					if (Damage)
					{
						int amount = GetDamageIncrement(Level).RollCached();
						bool accidental = item != Target;
						item.TakeDamage(amount, "from %o stunning force!", "Concussion", null, null, null, ParentObject, null, null, accidental);
					}
					if (Stun && item.IsValid() && !item.IsInGraveyard() && !Stunned.Contains(item) && item.HasPart("Combat"))
					{
						Stunned.Add(item);
					}
				}
			}
		}
		if (StartCell != null && StartCell.IsVisible())
		{
			for (int j = 0; j < 360; j++)
			{
				for (int k = 5; k < 30; k += Stat.Random(2, 6))
				{
					if (Stat.Random(1, 120 - Distance * 10) == 1)
					{
						XRLCore.ParticleManager.Add("&" + XRL.World.Capabilities.Phase.getRandomStunningForceColor(Phase) + "±", StartCell.X, StartCell.Y, (float)Math.Sin((double)(float)j * 0.06) / (float)k, (float)Math.Cos((double)(float)j * 0.06) / (float)k, 60);
					}
					XRLCore.ParticleManager.AddRadial("@", StartCell.X, StartCell.Y, 0f, 0f, 0.005f * (float)k, 0.01f * (float)k);
				}
				XRLCore.ParticleManager.Add("@", StartCell.X, StartCell.Y, (float)Math.Sin((double)(float)j * 0.06) / 4f, (float)Math.Cos((double)(float)j * 0.06) / 4f, 60);
			}
		}
		foreach (GameObject item2 in Stunned)
		{
			if (item2.IsValid() && !item2.IsInGraveyard() && !item2.MakeSave("Toughness", 15 + Level, null, null, "StunningForce Stun", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				item2.ApplyEffect(new Stun(3, 15 + Level));
			}
		}
		VisitedCells.Clear();
		CellQueue.Clear();
		Pushed.Clear();
		Stunned.Clear();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 8 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.HasLOSTo(E.GetGameObjectParameter("Target"), IncludeSolid: false))
			{
				E.AddAICommand("CommandStunningForce");
			}
		}
		else if (E.ID == "CommandStunningForce")
		{
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("You cannot use Stunning Force on the world map.");
				}
				return false;
			}
			List<Cell> list = PickBurst(3, 8, bLocked: false, AllowVis.OnlyVisible);
			if (list == null || list.Count <= 0)
			{
				return false;
			}
			if (50.in100())
			{
				Cell randomLocalAdjacentCell = list[0].GetRandomLocalAdjacentCell();
				if (randomLocalAdjacentCell != null)
				{
					list[0] = randomLocalAdjacentCell;
				}
			}
			if (ParentObject.CurrentCell != null && list[0] != null)
			{
				ShockwaveAnimation(ParentObject.CurrentCell, list[0]);
			}
			Concussion(list[0], ParentObject, base.Level, 3, ParentObject.GetPhase(), list[0].GetCombatTarget(ParentObject, IgnoreFlight: true));
			CooldownMyActivatedAbility(ActivatedAbilityID, 50);
			UseEnergy(1000);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Stunning Force", "CommandStunningForce", "Mental Mutation", null, "#");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

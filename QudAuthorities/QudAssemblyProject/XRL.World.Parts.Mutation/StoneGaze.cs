using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Language;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class StoneGaze : BaseMutation
{
	public const string ABL_NAME = "Lithifying Gaze";

	public const string ABL_CMD = "CommandStoneGaze";

	public const int ABL_CLD = 50;

	public const int PAINT_DURATION = 500;

	public new Guid ActivatedAbilityID;

	public int Gazing;

	[FieldSaveVersion(258)]
	public Cell Target;

	[NonSerialized]
	public List<Cell> _TargetLine;

	[NonSerialized]
	private static GameObject _Projectile;

	public List<Cell> TargetLine
	{
		get
		{
			Cell cell = ParentObject.CurrentCell;
			if (_TargetLine == null)
			{
				if (cell == null || Target == null)
				{
					return null;
				}
				List<Point> list = Zone.Line(cell.X, cell.Y, Target.X, Target.Y);
				if (list.Count <= 1)
				{
					List<Cell> obj = new List<Cell> { cell };
					List<Cell> result = obj;
					_TargetLine = obj;
					return result;
				}
				list.RemoveAt(0);
				_TargetLine = new List<Cell>(GetGazeDistance(base.Level) + 1) { cell };
				int num = 0;
				int num2 = cell.X;
				int num3 = cell.Y;
				int gazeDistance = GetGazeDistance(base.Level);
				while (_TargetLine.Count <= gazeDistance)
				{
					int num4 = num % list.Count;
					num2 += list[num4].X - ((num4 > 0) ? list[num4 - 1].X : cell.X);
					num3 += list[num4].Y - ((num4 > 0) ? list[num4 - 1].Y : cell.Y);
					Cell cell2 = cell.ParentZone.GetCell(num2, num3);
					if (cell2 == null || !ParentObject.HasLOSTo(cell2))
					{
						break;
					}
					if (cell2 != _TargetLine.Last())
					{
						_TargetLine.Add(cell2);
					}
					num++;
				}
			}
			else if (_TargetLine.Count == 0 || _TargetLine[0] != cell)
			{
				Gazing = 0;
				Target = null;
				_TargetLine = null;
			}
			return _TargetLine;
		}
	}

	private static GameObject Projectile
	{
		get
		{
			if (!GameObject.validate(ref _Projectile))
			{
				_Projectile = GameObject.createUnmodified("ProjectileStoneGaze");
			}
			return _Projectile;
		}
	}

	public StoneGaze()
	{
		DisplayName = "Lithifying Gaze";
	}

	private bool ValidGazeTarget(GameObject obj)
	{
		return obj?.IsCombatObject() ?? false;
	}

	public bool PickGazeTarget()
	{
		if (Target != null)
		{
			return false;
		}
		List<Cell> list = PickLine(GetGazeDistance(base.Level), AllowVis.OnlyVisible, ValidGazeTarget, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, null, null, "Lithifying Gaze");
		if (list.IsNullOrEmpty())
		{
			return false;
		}
		Cell cell = (Target = list.Last());
		Gazing = GetTurnsToCharge();
		return true;
	}

	public static GameObject CreateStatueOf(GameObject source)
	{
		GameObject gameObject = GameObject.create("Random Stone Statue");
		gameObject.RequirePart<RandomStatue>().SetCreature(source.DeepCopy());
		if (Stat.Random(1, 100) <= 50)
		{
			gameObject.SetStringProperty("QuestVerb", "pray at");
			gameObject.SetStringProperty("QuestEvent", "Prayed");
		}
		else
		{
			gameObject.SetStringProperty("QuestVerb", "desecrate");
			gameObject.SetStringProperty("QuestEvent", "Desecrated");
		}
		gameObject.RequirePart<Shrine>();
		return gameObject;
	}

	public void Refract(List<Cell> Path)
	{
		Event @event = null;
		for (int i = 1; i < Path.Count; i++)
		{
			Cell cell = Path[i];
			if (!cell.HasObjectWithRegisteredEvent("RefractLight"))
			{
				continue;
			}
			if (@event == null)
			{
				Cell cell2 = Path[0];
				Cell cell3 = Path[Path.Count - 1];
				@event = Event.New("RefractLight");
				@event.SetParameter("Projectile", Projectile);
				@event.SetParameter("Attacker", ParentObject);
				@event.SetParameter("Angle", (float)Math.Atan2(cell3.X - cell2.X, cell3.Y - cell2.Y).toDegrees());
				@event.SetParameter("Sound", "refract");
			}
			@event.SetParameter("Cell", cell);
			@event.SetParameter("Direction", Stat.Random(0, 359));
			@event.SetParameter("Verb", null);
			@event.SetParameter("By", (object)null);
			if (cell.FireEvent(@event))
			{
				continue;
			}
			GameObject obj = @event.GetGameObjectParameter("By");
			if (!GameObject.validate(ref obj))
			{
				continue;
			}
			Path.RemoveRange(i, Path.Count - i);
			PlayWorldSound(@event.GetStringParameter("Sound"), 0.5f, 0f, combat: true);
			IComponent<GameObject>.XDidY(obj, @event.GetStringParameter("Verb") ?? "refract", "the lithifying gaze", "!", null, obj);
			int num = @event.GetIntParameter("Direction").normalizeDegrees();
			float num2 = cell.X;
			float num3 = cell.Y;
			float num4 = Mathf.Sin((float)num * ((float)Math.PI / 180f));
			float num5 = Mathf.Cos((float)num * ((float)Math.PI / 180f));
			Cell cell4 = cell;
			do
			{
				num2 += num4;
				num3 += num5;
				Cell cell5 = cell.ParentZone.GetCell((int)num2, (int)num3);
				if (cell5 == null)
				{
					break;
				}
				if (cell5 != cell4)
				{
					Path.Add(cell4 = cell5);
					if (cell5.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, Projectile, null, AllowInanimate: true, InanimateSolidOnly: true) != null || cell5.HasSolidObjectForMissile(ParentObject, Projectile))
					{
						break;
					}
				}
			}
			while (num2 > 0f && num2 < 79f && num3 > 0f && num3 < 24f && Path.Count < 400);
		}
	}

	public void PerformGaze()
	{
		List<Cell> targetLine = TargetLine;
		Gazing = 0;
		Target = null;
		_TargetLine = null;
		if (targetLine.IsNullOrEmpty())
		{
			return;
		}
		Refract(targetLine);
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		List<GameObject> list = Event.NewGameObjectList();
		bool flag = false;
		int i = 0;
		for (int count = targetLine.Count; i < count; i++)
		{
			Cell cell = targetLine[i];
			foreach (GameObject @object in cell.Objects)
			{
				if (@object != ParentObject && @object.IsCombatObject() && !@object.MakeSave("Toughness", 20, ParentObject, "Ego", "Lithofex Stoning Gaze Beam"))
				{
					list.Add(@object);
				}
			}
			int num = 0;
			while (num < list.Count)
			{
				GameObject gameObject = list[num];
				if (gameObject.IsPlayer())
				{
					AchievementManager.SetAchievement("ACH_TURNED_STONE");
				}
				gameObject.SetIntProperty("SuppressCorpseDrops", 1);
				if (gameObject.Die(ParentObject, null, "You were turned to stone by the gaze of " + ParentObject.a + ParentObject.ShortDisplayName + ".", gameObject.It + gameObject.GetVerb("were", PrependSpace: true, PronounAntecedent: true) + " @@turned to stone by the gaze of " + ParentObject.a + ParentObject.ShortDisplayName + ".") && gameObject.IsValid())
				{
					flag = gameObject.IsPlayer();
					cell.AddObject(CreateStatueOf(gameObject));
				}
				list.RemoveAt(0);
			}
			if (cell.IsVisible())
			{
				scrapBuffer.RenderBase();
				if (i > 0)
				{
					scrapBuffer.WriteAt(targetLine[i - 1], "&b*");
				}
				if (i > 1)
				{
					scrapBuffer.WriteAt(targetLine[i - 2], "&K*");
				}
				scrapBuffer.WriteAt(cell, "&B*");
				scrapBuffer.Draw();
				Thread.Sleep(10);
			}
		}
		if (flag)
		{
			The.Core.RenderDelay(3000);
		}
		CooldownMyActivatedAbility(ActivatedAbilityID, 50);
		UseEnergy(1000);
	}

	public void AlertGazeCells()
	{
		if (Target == null)
		{
			return;
		}
		List<Cell> targetLine = TargetLine;
		if (targetLine.IsNullOrEmpty())
		{
			return;
		}
		int i = 0;
		for (int count = targetLine.Count; i < count; i++)
		{
			int j = 0;
			for (int count2 = targetLine[i].Objects.Count; j < count2; j++)
			{
				GameObject gameObject = targetLine[i].Objects[j];
				if (gameObject.IsPlayer())
				{
					AutoAct.Interrupt("you are in the path of a lithofex's baleful gaze", targetLine[i]);
				}
				else if (gameObject.IsPotentiallyMobile())
				{
					gameObject.pBrain.PushGoal(new FleeLocation(targetLine[i], 2));
				}
			}
		}
	}

	public int GetTurnsToCharge()
	{
		return 3;
	}

	public override bool FinalRender(RenderEvent E, bool bAlt)
	{
		E.WantsToPaint = Target != null;
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (Target != null)
		{
			int num = 500;
			if (IComponent<GameObject>.frameTimerMS % num < num / 2)
			{
				E.ColorString = ((E.BackgroundString == "&r") ? "&Y" : "&r");
				E.BackgroundString = ((E.BackgroundString == "^R") ? "^Y" : "^R");
			}
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer SB)
	{
		if (Target == null)
		{
			return;
		}
		List<Cell> targetLine = TargetLine;
		if (targetLine.IsNullOrEmpty())
		{
			return;
		}
		int num = (int)(IComponent<GameObject>.frameTimerMS % 500 / (500 / targetLine.Count));
		for (int i = 0; i < targetLine.Count; i++)
		{
			Cell cell = targetLine[i];
			if (cell.IsVisible() && cell != ParentObject.CurrentCell && cell.ParentZone == ParentObject.CurrentZone)
			{
				ConsoleChar consoleChar = SB[cell];
				if (i != num)
				{
					Color color3 = (consoleChar.TileBackground = (consoleChar.Background = ConsoleLib.Console.ColorUtility.ColorMap['R']));
					color3 = (consoleChar.TileForeground = (consoleChar.Detail = ConsoleLib.Console.ColorUtility.ColorMap['r']));
				}
				else
				{
					Color color3 = (consoleChar.TileBackground = (consoleChar.Background = ConsoleLib.Console.ColorUtility.ColorMap['r']));
					color3 = (consoleChar.TileForeground = (consoleChar.Detail = ConsoleLib.Console.ColorUtility.ColorMap['R']));
				}
				consoleChar.SetForeground('r');
			}
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID)
		{
			return ID == CommandEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Gazing > 0)
		{
			if (--Gazing <= 0)
			{
				PerformGaze();
			}
			else
			{
				if (ParentObject.IsPlayer())
				{
					The.Core.RenderDelay(500);
				}
				UseEnergy(1000);
				AlertGazeCells();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandStoneGaze" && PickGazeTarget())
		{
			UseEnergy(1000);
			AlertGazeCells();
			DidXToY("focus", ParentObject, "baleful gaze", null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true, null, null, null, DescribeSubjectDirection: true, DescribeSubjectDirectionLate: false, Target == The.Player.CurrentCell);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList" && Target == null && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.GetIntParameter("Distance") <= GetGazeDistance(base.Level))
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			if (gameObjectParameter != null && ParentObject.HasLOSTo(gameObjectParameter))
			{
				E.AddAICommand("CommandStoneGaze");
			}
		}
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		return "You turn things to stone with your gaze.";
	}

	public int GetGazeDistance(int Level)
	{
		return 7 + Level;
	}

	public override string GetLevelText(int Level)
	{
		return "You can gaze {{rules|" + GetGazeDistance(Level) + "}} squares after a " + Grammar.Cardinal(GetTurnsToCharge()) + "-turn warmup and turn targets to stone.\nCooldown: {{rules|" + 50 + "}} rounds";
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Lithifying Gaze", "CommandStoneGaze", "Physical Mutation", null, "è", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

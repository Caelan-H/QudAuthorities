using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Confusion : BaseMutation
{
	public int Range = 8;

	public new Guid ActivatedAbilityID = Guid.Empty;

	public static int CurrentConfusionLevel
	{
		get
		{
			if (XRLCore.Core == null)
			{
				return 0;
			}
			if (XRLCore.Core.Game == null)
			{
				return 0;
			}
			if (XRLCore.Core.Game.Player.Body == null)
			{
				return 0;
			}
			return XRLCore.Core.Game.Player.Body.GetConfusion();
		}
	}

	public Confusion()
	{
		DisplayName = "Confusion";
		Type = "Mental";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandConfuse");
		base.Register(Object);
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
		E.Add("chance", 1);
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You confuse nearby enemies.";
	}

	public static int GetCooldown(int Level)
	{
		return 75;
	}

	public static int GetConeAngle(int Level)
	{
		return 29 + Level;
	}

	public static int GetConeLength(int Level)
	{
		return 4 + Level / 3;
	}

	public static int GetConeAnimationDelay(int Level)
	{
		return Math.Max(10, 26 - Level);
	}

	public static int GetLowDuration(int Level)
	{
		return (int)((double)(Level / 2 + 10) * 0.8);
	}

	public static int GetHighDuration(int Level)
	{
		return (int)((double)(Level / 2 + 10) * 1.2);
	}

	public static string GetDuration(int Level)
	{
		return GetLowDuration(Level) + "-" + GetHighDuration(Level);
	}

	public static int GetMentalPenalty(int Level)
	{
		return (int)Math.Min(10.0, Math.Floor((double)(Level - 1) / 2.0 + 3.0));
	}

	public static int GetConfusionLevel(int Level)
	{
		return (int)Math.Min(10.0, Math.Floor((double)(Level - 1) / 2.0 + 3.0));
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat(string.Concat("Affected creatures act semi-randomly and receive a {{rules|-" + GetMentalPenalty(Level) + "}} penalty to their mental abilities.\n", "Cone angle: {{rules|", GetConeAngle(Level).ToString(), "}} degrees\n"), "Cone length: {{rules|", GetConeLength(Level).ToString(), "}}\n"), "Duration: {{rules|", GetDuration(Level), "}} rounds\n"), "Cooldown: ", GetCooldown(Level).ToString(), " rounds");
	}

	public static bool Confuse(MentalAttackEvent E, bool Attack, int Level, int Penalty, bool Silent = false)
	{
		GameObject defender = E.Defender;
		GameObject attacker = E.Attacker;
		if (E.Penetrations > 0)
		{
			int magnitude = E.Magnitude;
			if (magnitude > 0 && defender.FireEvent("CanApplyConfusion") && CanApplyEffectEvent.Check(defender, "Confusion", magnitude) && (!Attack || defender.FireEvent(Event.New("ApplyAttackConfusion", "Duration", magnitude))) && defender.FireEvent(Event.New("ApplyConfusion", "Duration", magnitude)) && defender.ApplyEffect(new Confused(magnitude, Level, Penalty)))
			{
				for (int i = 0; i < E.Penetrations; i++)
				{
					defender.ParticleText("&W?");
				}
				return true;
			}
			if (!Silent && attacker != null && attacker.IsPlayer())
			{
				IComponent<GameObject>.XDidYToZ(attacker, "attack", "does not affect", defender, null, null, null, null, attacker);
			}
		}
		else if (!Silent)
		{
			IComponent<GameObject>.XDidY(defender, "resist", "being confused", null, null, defender);
		}
		return false;
	}

	public void Confuse(List<Cell> Cells)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		string duration = GetDuration(base.Level);
		int coneAnimationDelay = GetConeAnimationDelay(base.Level);
		foreach (Cell Cell in Cells)
		{
			foreach (GameObject item in Cell.GetObjectsInCell())
			{
				if (item != ParentObject && item.pBrain != null)
				{
					int level = GetConfusionLevel(base.Level);
					int penalty = GetMentalPenalty(base.Level);
					PerformMentalAttack((MentalAttackEvent E) => Confuse(E, Attack: true, level, penalty), ParentObject, item, null, "Confuse", "1d8", 8388609, duration.RollCached(), int.MinValue, Math.Max(ParentObject.StatMod("Ego"), base.Level));
				}
			}
			if (Cell.IsVisible())
			{
				scrapBuffer.Goto(Cell.X, Cell.Y);
				scrapBuffer.Write("&B?");
				textConsole.DrawBuffer(scrapBuffer);
				Thread.Sleep(coneAnimationDelay);
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= Range && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
				if (gameObjectParameter != null && gameObjectParameter.DistanceTo(ParentObject) < GetConeLength(base.Level) && gameObjectParameter.pBrain != null && !gameObjectParameter.HasEffect("Confused") && gameObjectParameter.FireEvent("CanApplyConfusion") && CanApplyEffectEvent.Check(gameObjectParameter, "Confusion"))
				{
					E.AddAICommand("CommandConfuse");
				}
			}
		}
		else if (E.ID == "CommandConfuse")
		{
			List<Cell> list = PickCone(GetConeLength(base.Level), GetConeAngle(base.Level), AllowVis.Any);
			if (list == null)
			{
				return false;
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(base.Level));
			Confuse(list);
			UseEnergy(1000, "Mental Mutation Confusion");
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Confusion", "CommandConfuse", "Mental Mutation", null, "?");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

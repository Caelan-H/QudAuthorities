using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Language;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class LightManipulation : BaseMutation
{
	public const int RANGE = 999;

	public const int COUNT = 1000;

	public const int BASE_RADIUS_REGROWTH_TURNS = 15;

	public const int WILLPOWER_BASELINE = 16;

	public const int WILLPOWER_FACTOR = 5;

	public const int WILLPOWER_CEILING_FACTOR = 5;

	public const int WILLPOWER_FLOOR_DIVISOR = 5;

	public Guid LaseActivatedAbilityID = Guid.Empty;

	public Guid LightActivatedAbilityID = Guid.Empty;

	public int RadiusPenalty;

	public int RadiusRegrowthTimer;

	[NonSerialized]
	private static GameObject _Projectile;

	private static GameObject Projectile
	{
		get
		{
			if (!GameObject.validate(ref _Projectile))
			{
				_Projectile = GameObject.createUnmodified("ProjectileLightManipulation");
			}
			return _Projectile;
		}
	}

	public int MaxLightRadius => GetMaxLightRadius(base.Level);

	public LightManipulation()
	{
		DisplayName = "Light Manipulation";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID)
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (RadiusPenalty < MaxLightRadius && IsMyActivatedAbilityUsable(LightActivatedAbilityID))
		{
			AddLight(MaxLightRadius - RadiusPenalty);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("stars", 1);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AfterMassMind");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandAmbientLight");
		Object.RegisterPartEvent(this, "CommandLase");
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "RefractLight");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You manipulate light to your advantage.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("" + "You produce ambient light within a radius of {{rules|" + GetMaxLightRadius(Level) + "}}.\n", "You may focus the light into a laser beam (doing so reduces the radius of your ambient light by 1).\n"), "Laser damage increment: {{rules|", GetDamage(Level), "}}\n"), "Laser penetration: {{rules|", ((Level - 1) / 2 + 4).ToString(), "}}\n"), "Ambient light recharges at a rate of 1 unit every ", GetRadiusRegrowthTurns().ToString(), " rounds until it reaches its maximum value.\n"), "{{rules|", GetReflectChance().ToString(), "%}} chance to reflect light-based damage");
	}

	public int GetMaxLightRadius(int Level)
	{
		return (int)(4.0 + Math.Floor((float)Level / 2f));
	}

	public string GetDamage(int Level)
	{
		if (Level <= 1)
		{
			return "1d3";
		}
		if (Level <= 2)
		{
			return "1d4";
		}
		if (Level <= 3)
		{
			return "1d5";
		}
		if (Level <= 4)
		{
			return "1d4+1";
		}
		if (Level <= 5)
		{
			return "1d5+1";
		}
		if (Level <= 6)
		{
			return "1d4+2";
		}
		if (Level <= 7)
		{
			return "1d5+2";
		}
		if (Level <= 8)
		{
			return "1d4+3";
		}
		if (Level <= 9)
		{
			return "1d5+3";
		}
		if (Level > 9)
		{
			return "1d5+" + (Level - 6);
		}
		return "1d4+4";
	}

	public int GetLasePenetrationBonus()
	{
		return 4 + (base.Level - 1) / 2;
	}

	public bool Lase(Cell C, int PathLength = 0)
	{
		_ = Look._TextConsole;
		ScreenBuffer screenBuffer = TextConsole.ScrapBuffer.WithMap();
		bool result = false;
		if (C != null)
		{
			GameObject combatTarget = C.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, Projectile, null, AllowInanimate: true, InanimateSolidOnly: true);
			if (combatTarget != null)
			{
				int lasePenetrationBonus = GetLasePenetrationBonus();
				int num = Stat.RollDamagePenetrations(combatTarget.Stat("AV"), lasePenetrationBonus, lasePenetrationBonus);
				if (num > 0)
				{
					string resultColor = Stat.GetResultColor(num);
					int num2 = 0;
					string damage = GetDamage(base.Level);
					for (int i = 0; i < num; i++)
					{
						num2 += damage.RollCached();
					}
					combatTarget.TakeDamage(num2, Owner: ParentObject, Message: "from %t lase beam! {{" + resultColor + "|(x" + num + ")}}", Attributes: "Light Laser", DeathReason: null, ThirdPersonDeathReason: null, Attacker: null, Source: null, Perspective: null, Accidental: false, Environmental: false, Indirect: false, ShowUninvolved: false, ShowForInanimate: true);
				}
				else if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Your lase beam doesn't penetrate " + Grammar.MakePossessive(combatTarget.the + combatTarget.ShortDisplayName) + " armor.", 'r');
				}
				else if (combatTarget.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(ParentObject.the + ParentObject.ShortDisplayName) + " lase beam doesn't penetrate your armor.", 'g');
				}
				result = true;
			}
		}
		if (C.IsVisible() || ParentObject.IsPlayer())
		{
			switch (Stat.Random(1, 3))
			{
			case 1:
				screenBuffer.WriteAt(C, "&C\u000f");
				break;
			case 2:
				screenBuffer.WriteAt(C, "&Y\u000f");
				break;
			default:
				screenBuffer.WriteAt(C, "&B\u000f");
				break;
			}
			screenBuffer.Draw();
			int num3 = 10 - PathLength / 5;
			if (num3 > 0)
			{
				Thread.Sleep(num3);
			}
		}
		return result;
	}

	public int GetReflectChance()
	{
		return 10 + 3 * base.Level;
	}

	public void SyncAbilityName()
	{
		ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(LaseActivatedAbilityID);
		if (activatedAbilityEntry != null)
		{
			activatedAbilityEntry.DisplayName = "Lase (" + (MaxLightRadius - RadiusPenalty) + " charges)";
		}
	}

	public static int GetRadiusRegrowthTurns(int Willpower)
	{
		int num = 15;
		if (!GlobalConfig.GetBoolSetting("LightManipulationWillpowerRecharge"))
		{
			return num;
		}
		int num2 = num;
		int num3 = (Willpower - 16) * 5;
		if (num3 != 0)
		{
			num2 = num2 * (100 - num3) / 100;
		}
		return Math.Max(Math.Min(num2, num * 5), num / 5);
	}

	public int GetRadiusRegrowthTurns()
	{
		return GetRadiusRegrowthTurns(ParentObject?.Stat("Willpower") ?? 16);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterMassMind")
		{
			RadiusPenalty = 0;
			SyncAbilityName();
		}
		else if (E.ID == "EndTurn")
		{
			if (RadiusPenalty > 0)
			{
				RadiusRegrowthTimer++;
				if (RadiusRegrowthTimer >= GetRadiusRegrowthTurns())
				{
					RadiusRegrowthTimer = 0;
					RadiusPenalty--;
					SyncAbilityName();
				}
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 999 && RadiusPenalty < MaxLightRadius && IsMyActivatedAbilityAIUsable(LaseActivatedAbilityID) && ParentObject.HasLOSTo(E.GetGameObjectParameter("Target"), IncludeSolid: true, UseTargetability: true))
			{
				E.AddAICommand("CommandLase", 2);
			}
		}
		else if (E.ID == "RefractLight")
		{
			if (GetReflectChance().in100())
			{
				E.SetParameter("By", ParentObject);
				E.SetParameter("Direction", (int)(float)E.GetParameter("Angle") + 180);
				E.SetParameter("Verb", "reflect");
				return false;
			}
		}
		else if (E.ID == "CommandLase")
		{
			if (!IsMyActivatedAbilityUsable(LaseActivatedAbilityID))
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
			if (RadiusPenalty >= MaxLightRadius)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("Your capacity is too weak.");
				}
				return false;
			}
			List<Cell> list = PickLine(999, AllowVis.Any, (GameObject o) => o.HasPart("Combat") && o.PhaseMatches(ParentObject), IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, null, null, null, Snap: true);
			if (list == null || list.Count <= 0)
			{
				return false;
			}
			if (list.Count > 1000)
			{
				list.RemoveRange(1000, list.Count - 1000);
			}
			UseEnergy(1000);
			Cell cell = list[0];
			Cell cell2 = list[list.Count - 1];
			float num = (float)Math.Atan2(cell2.X - cell.X, cell2.Y - cell.Y).toDegrees();
			list.RemoveAt(0);
			for (int i = 0; i < list.Count; i++)
			{
				Cell cell3 = list[i];
				if (!cell3.HasObjectWithRegisteredEvent("RefractLight") && !cell3.HasObjectWithRegisteredEvent("ReflectProjectile"))
				{
					continue;
				}
				bool flag = true;
				GameObject obj = null;
				string clip = null;
				string verb = "refract";
				int num2 = -1;
				if (cell3.HasObjectWithRegisteredEvent("RefractLight"))
				{
					Event @event = Event.New("RefractLight");
					@event.SetParameter("Projectile", (object)null);
					@event.SetParameter("Attacker", ParentObject);
					@event.SetParameter("Cell", cell3);
					@event.SetParameter("Angle", num);
					@event.SetParameter("Direction", Stat.Random(0, 359));
					@event.SetParameter("Verb", null);
					@event.SetParameter("Sound", "refract");
					@event.SetParameter("By", (object)null);
					flag = cell3.FireEvent(@event);
					if (!flag)
					{
						obj = @event.GetGameObjectParameter("By");
						clip = @event.GetStringParameter("Sound");
						verb = @event.GetStringParameter("Verb") ?? "refract";
						num2 = @event.GetIntParameter("Direction").normalizeDegrees();
					}
				}
				if (flag && cell3.HasObjectWithRegisteredEvent("ReflectProjectile"))
				{
					Event event2 = Event.New("ReflectProjectile");
					event2.SetParameter("Projectile", (object)null);
					event2.SetParameter("Attacker", ParentObject);
					event2.SetParameter("Cell", cell3);
					event2.SetParameter("Angle", num);
					event2.SetParameter("Direction", Stat.Random(0, 359));
					event2.SetParameter("Verb", null);
					event2.SetParameter("Sound", "refract");
					event2.SetParameter("By", (object)null);
					flag = cell3.FireEvent(event2);
					if (!flag)
					{
						obj = event2.GetGameObjectParameter("By");
						clip = event2.GetStringParameter("Sound");
						verb = event2.GetStringParameter("Verb") ?? "refract";
						num2 = event2.GetIntParameter("Direction").normalizeDegrees();
					}
				}
				if (flag || !GameObject.validate(ref obj))
				{
					continue;
				}
				PlayWorldSound(clip, 0.5f, 0f, combat: true);
				IComponent<GameObject>.XDidY(obj, verb, "the lase beam", "!", null, obj);
				float num3 = cell3.X;
				float num4 = cell3.Y;
				float num5 = (float)Math.Sin((float)num2 * ((float)Math.PI / 180f));
				float num6 = (float)Math.Cos((float)num2 * ((float)Math.PI / 180f));
				list.RemoveRange(i, list.Count - i);
				Cell cell4 = cell3;
				do
				{
					num3 += num5;
					num4 += num6;
					Cell cell5 = cell3.ParentZone.GetCell((int)num3, (int)num4);
					if (cell5 == null)
					{
						break;
					}
					if (cell5 != cell4)
					{
						list.Add(cell5);
						cell4 = cell5;
						if (cell5.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, Projectile, null, AllowInanimate: true, InanimateSolidOnly: true) != null || cell5.HasSolidObjectForMissile(ParentObject, Projectile))
						{
							break;
						}
					}
				}
				while (num3 > 0f && num3 < 79f && num4 > 0f && num4 < 24f && list.Count < 400);
			}
			int j = 0;
			for (int count = list.Count; j < count && !Lase(list[j], count); j++)
			{
			}
			RadiusPenalty++;
			SyncAbilityName();
			if (RadiusPenalty <= MaxLightRadius)
			{
			}
		}
		else if (E.ID == "CommandAmbientLight")
		{
			if (IsMyActivatedAbilityToggledOn(LightActivatedAbilityID))
			{
				ToggleMyActivatedAbility(LightActivatedAbilityID);
			}
			else
			{
				if (IsMyActivatedAbilityCoolingDown(LaseActivatedAbilityID))
				{
					if (ParentObject.IsPlayer())
					{
						if (Options.GetOption("OptionAbilityCooldownWarningAsMessage").ToUpper() == "YES")
						{
							MessageQueue.AddPlayerMessage("You must wait {{C|" + GetMyActivatedAbilityCooldownDescription(LaseActivatedAbilityID) + "}} before you can enable ambient light.");
						}
						else
						{
							Popup.ShowFail("You must wait {{C|" + GetMyActivatedAbilityCooldownDescription(LaseActivatedAbilityID) + "}} before you can enable ambient light.");
						}
					}
					return false;
				}
				ToggleMyActivatedAbility(LightActivatedAbilityID);
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		LaseActivatedAbilityID = AddMyActivatedAbility("Lase", "CommandLase", "Mental Mutation", null, "\u000f");
		LightActivatedAbilityID = AddMyActivatedAbility("Ambient Light", "CommandAmbientLight", "Mental Mutation", null, "\a", null, Toggleable: true, DefaultToggleState: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref LightActivatedAbilityID);
		RemoveMyActivatedAbility(ref LaseActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class FlamingHands : BaseDefaultEquipmentMutation
{
	public string BodyPartType = "Hands";

	public bool CreateObject = true;

	public string Sound = "burn_crackling";

	public Guid FlamingHandsActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private static GameObject _Projectile;

	[NonSerialized]
	private static List<string> variants = new List<string> { "Hands", "Face", "Feet" };

	private static GameObject Projectile
	{
		get
		{
			if (!GameObject.validate(ref _Projectile))
			{
				_Projectile = GameObject.createUnmodified("ProjectileFlamingHands");
			}
			return _Projectile;
		}
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public FlamingHands()
	{
		DisplayName = "Flaming Ray";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "AttackerHit");
		Object.RegisterPartEvent(this, "CommandFlamingHands");
		base.Register(Object);
	}

	public override string GetCreateCharacterDisplayName()
	{
		return DisplayName + " (" + BodyPartType + ")";
	}

	public override string GetDescription()
	{
		BodyPart registeredSlot = GetRegisteredSlot(BodyPartType, evenIfDismembered: true);
		if (registeredSlot != null)
		{
			return "You emit a ray of flame from your " + registeredSlot.GetOrdinalName() + ".";
		}
		return "You emit a ray of flame.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("Emits a 9-square ray of flame in the direction of your choice.\n" + "Damage: {{rules|" + ComputeDamage(Level) + "}}\n", "Cooldown: 10 rounds\n"), "Melee attacks heat opponents by {{rules|", GetHeatOnHitAmount(Level), "}} degrees");
	}

	public string GetHeatOnHitAmount(int Level)
	{
		return Level * 2 + "d8";
	}

	public string ComputeDamage(int UseLevel)
	{
		string text = UseLevel + "d4";
		if (ParentObject != null)
		{
			int partCount = ParentObject.Body.GetPartCount(BodyPartType);
			if (partCount > 0)
			{
				text = text + "+" + partCount;
			}
		}
		else
		{
			text += "+1";
		}
		return text;
	}

	public string ComputeDamage()
	{
		return ComputeDamage(base.Level);
	}

	public void Flame(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		string dice = ComputeDamage();
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				if (!item.PhaseMatches(ParentObject))
				{
					continue;
				}
				item.TemperatureChange(310 + 25 * base.Level, ParentObject);
				if (doEffect)
				{
					for (int i = 0; i < 5; i++)
					{
						item.ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int j = 0; j < 5; j++)
					{
						item.ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int k = 0; k < 5; k++)
					{
						item.ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
				}
			}
			int phase = ParentObject.GetPhase();
			DieRoll cachedDieRoll = dice.GetCachedDieRoll();
			foreach (GameObject item2 in C.GetObjectsWithPartReadonly("Combat"))
			{
				item2.TakeDamage(cachedDieRoll.Resolve(), "from %o flames!", "Fire", null, null, ParentObject, null, null, null, Accidental: false, Environmental: false, Indirect: false, ShowUninvolved: false, ShowForInanimate: false, SilentIfNoDamage: false, phase);
			}
		}
		if (doEffect)
		{
			Buffer.Goto(C.X, C.Y);
			string text = "&C";
			int num = Stat.Random(1, 3);
			if (num == 1)
			{
				text = "&R";
			}
			if (num == 2)
			{
				text = "&r";
			}
			if (num == 3)
			{
				text = "&W";
			}
			int num2 = Stat.Random(1, 3);
			if (num2 == 1)
			{
				text += "^R";
			}
			if (num2 == 2)
			{
				text += "^r";
			}
			if (num2 == 3)
			{
				text += "^W";
			}
			if (C.ParentZone == XRLCore.Core.Game.ZoneManager.ActiveZone)
			{
				Stat.Random(1, 3);
				Buffer.Write(text + (char)(219 + Stat.Random(0, 4)));
				Popup._TextConsole.DrawBuffer(Buffer);
				Thread.Sleep(10);
			}
		}
	}

	public static bool Cast(FlamingHands mutation = null, string level = "5-6")
	{
		if (mutation == null)
		{
			mutation = new FlamingHands();
			mutation.Level = Stat.Roll(level);
			mutation.ParentObject = XRLCore.Core.Game.Player.Body;
		}
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		List<Cell> list = mutation.PickLine(9, AllowVis.Any, null, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, null, null, null, Snap: true);
		if (list == null || list.Count <= 0)
		{
			return false;
		}
		if (list.Count == 1 && mutation.ParentObject.IsPlayer() && Popup.ShowYesNoCancel("Are you sure you want to target " + mutation.ParentObject.itself + "?") != 0)
		{
			return false;
		}
		mutation.CooldownMyActivatedAbility(mutation.FlamingHandsActivatedAbilityID, 10);
		mutation.UseEnergy(1000, "Physical Mutation Flaming Hands");
		mutation.PlayWorldSound(mutation.Sound, 0.5f, 0f, combat: true);
		int i = 0;
		for (int num = Math.Min(list.Count, 10); i < num; i++)
		{
			if (list.Count == 1 || list[i] != mutation.ParentObject.CurrentCell)
			{
				mutation.Flame(list[i], scrapBuffer);
			}
			if (i < num - 1 && list[i].IsSolidFor(Projectile, mutation.ParentObject))
			{
				break;
			}
		}
		BodyPart registeredSlot = mutation.GetRegisteredSlot(mutation.BodyPartType, evenIfDismembered: false);
		IComponent<GameObject>.XDidY(mutation.ParentObject, "emit", "a flaming ray" + ((registeredSlot != null) ? (" from " + mutation.ParentObject.its + " " + registeredSlot.GetOrdinalName()) : ""), "!", null, mutation.ParentObject);
		return true;
	}

	public bool CheckObjectProperlyEquipped()
	{
		if (!CreateObject)
		{
			return true;
		}
		if (HasRegisteredSlot(BodyPartType))
		{
			return GetRegisteredSlot(BodyPartType, evenIfDismembered: false) != null;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerHit")
		{
			if (!CheckObjectProperlyEquipped())
			{
				return true;
			}
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter != null)
			{
				string heatOnHitAmount = GetHeatOnHitAmount(base.Level);
				int num = 400;
				if ((Stat.RollMax(heatOnHitAmount) > 0 && gameObjectParameter.pPhysics.Temperature < num) || (Stat.RollMax(heatOnHitAmount) < 0 && gameObjectParameter.pPhysics.Temperature > num))
				{
					gameObjectParameter.TemperatureChange(heatOnHitAmount.RollCached(), E.GetGameObjectParameter("Attacker"), Radiant: false, MinAmbient: false, MaxAmbient: false, ParentObject.GetPhase());
				}
			}
		}
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (CheckObjectProperlyEquipped() && E.GetIntParameter("Distance") <= 9 && IsMyActivatedAbilityAIUsable(FlamingHandsActivatedAbilityID) && ParentObject.HasLOSTo(E.GetGameObjectParameter("Target"), IncludeSolid: true, UseTargetability: true))
			{
				E.AddAICommand("CommandFlamingHands");
			}
		}
		else if (E.ID == "CommandFlamingHands")
		{
			if (!CheckObjectProperlyEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("Your " + BodyPartType + " is too damaged to do that!");
				}
				return false;
			}
			if (!Cast(this))
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	private void AddAbility()
	{
		FlamingHandsActivatedAbilityID = AddMyActivatedAbility("Flaming Ray", "CommandFlamingHands", "Physical Mutation", null, "\u00a8");
	}

	public override List<string> GetVariants()
	{
		return variants;
	}

	public override void SetVariant(int n)
	{
		if (n < variants.Count)
		{
			BodyPartType = variants[n];
		}
		else
		{
			BodyPartType = variants[0];
		}
		base.SetVariant(n);
	}

	public override void OnRegenerateDefaultEquipment(Body body)
	{
		base.OnRegenerateDefaultEquipment(body);
	}

	public void MakeFlaming(BodyPart part)
	{
		if (part == null)
		{
			return;
		}
		if (part.DefaultBehavior != null && part.DefaultBehavior.Blueprint != "Ghostly Flames" && !part.DefaultBehavior.pRender.DisplayName.Contains("{{fiery|flaming}}"))
		{
			part.DefaultBehavior.pRender.DisplayName = "{{fiery|flaming}} " + part.DefaultBehavior.pRender.DisplayName;
		}
		if (part.Parts != null)
		{
			for (int i = 0; i < part.Parts.Count; i++)
			{
				MakeFlaming(part.Parts[i]);
			}
		}
	}

	public override void OnDecorateDefaultEquipment(Body body)
	{
		if (CreateObject)
		{
			BodyPart bodyPart;
			if (!HasRegisteredSlot(BodyPartType))
			{
				bodyPart = body.GetFirstPart(BodyPartType);
				if (bodyPart != null)
				{
					RegisterSlot(BodyPartType, bodyPart);
				}
			}
			else
			{
				bodyPart = GetRegisteredSlot(BodyPartType, evenIfDismembered: false);
			}
			if (bodyPart != null && bodyPart.DefaultBehavior == null)
			{
				GameObject gameObject = GameObject.create("Ghostly Flames");
				gameObject.GetPart<Armor>().WornOn = BodyPartType;
				bodyPart.DefaultBehavior = gameObject;
			}
			MakeFlaming(bodyPart);
			if (BodyPartType == "Hands")
			{
				foreach (BodyPart part in body.GetParts())
				{
					if (part.Type == "Hand")
					{
						MakeFlaming(part);
					}
				}
			}
		}
		base.OnDecorateDefaultEquipment(body);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		AddAbility();
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref FlamingHandsActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class FreezingHands : BaseDefaultEquipmentMutation
{
	public string BodyPartType = "Hands";

	public bool CreateObject = true;

	public int OldFreeze = -1;

	public int OldBrittle = -1;

	public string Sound = "hiss_low";

	public Guid FreezingHandsActivatedAbilityID = Guid.Empty;

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
				_Projectile = GameObject.createUnmodified("ProjectileFreezingHands");
			}
			return _Projectile;
		}
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override string GetCreateCharacterDisplayName()
	{
		return DisplayName + " (" + BodyPartType + ")";
	}

	public FreezingHands()
	{
		DisplayName = "Freezing Ray";
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
		E.Add("ice", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "AttackerHit");
		Object.RegisterPartEvent(this, "CommandFreezingHands");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		BodyPart registeredSlot = GetRegisteredSlot(BodyPartType, evenIfDismembered: true);
		if (registeredSlot != null)
		{
			return "You emit a ray of frost from your " + registeredSlot.GetOrdinalName() + ".";
		}
		return "You emit a ray of frost.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("Emits a 9-square ray of frost in the direction of your choice.\n" + "Damage: {{rules|" + ComputeDamage(Level) + "}}\n", "Cooldown: 20 rounds\n"), "Melee attacks cool opponents by {{rules|", GetCoolOnHitAmount(Level), "}} degrees");
	}

	public string GetCoolOnHitAmount(int Level)
	{
		return "-" + Level + "d4";
	}

	public string ComputeDamage(int UseLevel)
	{
		string text = UseLevel + "d3";
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

	public void Freeze(Cell C, ScreenBuffer Buffer)
	{
		string dice = ComputeDamage();
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				if (item.PhaseMatches(ParentObject) && item.TemperatureChange(-120 - 7 * base.Level, ParentObject))
				{
					for (int i = 0; i < 5; i++)
					{
						item.ParticleText("&C" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int j = 0; j < 5; j++)
					{
						item.ParticleText("&c" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int k = 0; k < 5; k++)
					{
						item.ParticleText("&Y" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
				}
			}
			foreach (GameObject item2 in C.GetObjectsWithPart("Combat"))
			{
				if (item2.PhaseMatches(ParentObject))
				{
					Damage damage = new Damage(Stat.Roll(dice));
					damage.AddAttribute("Cold");
					Event @event = Event.New("TakeDamage");
					@event.SetParameter("Damage", damage);
					@event.SetParameter("Owner", ParentObject);
					@event.SetParameter("Attacker", ParentObject);
					@event.SetParameter("Message", "from %o freezing effect!");
					item2.FireEvent(@event);
				}
			}
		}
		Buffer.Goto(C.X, C.Y);
		string text = "&C";
		int num = Stat.Random(1, 3);
		if (num == 1)
		{
			text = "&C";
		}
		if (num == 2)
		{
			text = "&B";
		}
		if (num == 3)
		{
			text = "&Y";
		}
		int num2 = Stat.Random(1, 3);
		if (num2 == 1)
		{
			text += "^C";
		}
		if (num2 == 2)
		{
			text += "^B";
		}
		if (num2 == 3)
		{
			text += "^Y";
		}
		if (C.ParentZone == XRLCore.Core.Game.ZoneManager.ActiveZone)
		{
			Stat.Random(1, 3);
			Buffer.Write(text + (char)(219 + Stat.Random(0, 4)));
			Popup._TextConsole.DrawBuffer(Buffer);
			Thread.Sleep(10);
		}
	}

	public static bool Cast(FreezingHands mutation = null, string level = "5-6")
	{
		if (mutation == null)
		{
			mutation = new FreezingHands();
			mutation.Level = Stat.Roll(level);
			mutation.ParentObject = XRLCore.Core.Game.Player.Body;
		}
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
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
		mutation.CooldownMyActivatedAbility(mutation.FreezingHandsActivatedAbilityID, 20);
		mutation.UseEnergy(1000, "Physical Mutation Freezing Hands");
		mutation.PlayWorldSound(mutation.Sound, 0.5f, 0f, combat: true);
		int i = 0;
		for (int num = Math.Min(list.Count, 10); i < num; i++)
		{
			if (list.Count == 1 || list[i] != mutation.ParentObject.CurrentCell)
			{
				mutation.Freeze(list[i], scrapBuffer);
			}
			if (i < num - 1 && list[i].IsSolidFor(Projectile, mutation.ParentObject))
			{
				break;
			}
		}
		BodyPart registeredSlot = mutation.GetRegisteredSlot(mutation.BodyPartType, evenIfDismembered: false);
		IComponent<GameObject>.XDidY(mutation.ParentObject, "emit", "a freezing ray" + ((registeredSlot != null) ? (" from " + mutation.ParentObject.its + " " + registeredSlot.GetOrdinalName()) : ""), "!", null, mutation.ParentObject);
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
				string coolOnHitAmount = GetCoolOnHitAmount(base.Level);
				int num = -10 * base.Level;
				if ((Stat.RollMax(coolOnHitAmount) > 0 && gameObjectParameter.pPhysics.Temperature < num) || (Stat.RollMax(coolOnHitAmount) < 0 && gameObjectParameter.pPhysics.Temperature > num))
				{
					gameObjectParameter.TemperatureChange(coolOnHitAmount.RollCached(), E.GetGameObjectParameter("Attacker"), Radiant: false, MinAmbient: false, MaxAmbient: false, ParentObject.GetPhase());
				}
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			if (CheckObjectProperlyEquipped() && E.GetIntParameter("Distance") <= 9 && IsMyActivatedAbilityAIUsable(FreezingHandsActivatedAbilityID) && ParentObject.HasLOSTo(E.GetGameObjectParameter("Target"), IncludeSolid: true, UseTargetability: true))
			{
				E.AddAICommand("CommandFreezingHands");
			}
		}
		else if (E.ID == "CommandFreezingHands")
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
		ParentObject.pPhysics.BrittleTemperature = -600 + -300 * base.Level;
		return base.ChangeLevel(NewLevel);
	}

	private void AddAbility()
	{
		FreezingHandsActivatedAbilityID = AddMyActivatedAbility("Freezing Ray", "CommandFreezingHands", "Physical Mutation", null, "*");
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

	public void MakeFreezing(BodyPart part)
	{
		if (part == null)
		{
			return;
		}
		if (part.DefaultBehavior != null && part.DefaultBehavior.Blueprint != "Icy Vapor" && !part.DefaultBehavior.pRender.DisplayName.Contains("{{icy|icy}}"))
		{
			part.DefaultBehavior.pRender.DisplayName = "{{icy|icy}} " + part.DefaultBehavior.pRender.DisplayName;
		}
		if (part.Parts != null)
		{
			for (int i = 0; i < part.Parts.Count; i++)
			{
				MakeFreezing(part.Parts[i]);
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
				GameObject gameObject = GameObject.create("Icy Vapor");
				gameObject.GetPart<Armor>().WornOn = BodyPartType;
				bodyPart.DefaultBehavior = gameObject;
			}
			MakeFreezing(bodyPart);
		}
		base.OnDecorateDefaultEquipment(body);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (GO.pPhysics != null)
		{
			OldFreeze = GO.pPhysics.FreezeTemperature;
			OldBrittle = GO.pPhysics.BrittleTemperature;
		}
		AddAbility();
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		if (GO.pPhysics != null)
		{
			if (OldFreeze != -1)
			{
				GO.pPhysics.FreezeTemperature = OldFreeze;
			}
			if (OldBrittle != -1)
			{
				GO.pPhysics.BrittleTemperature = OldBrittle;
			}
			OldFreeze = -1;
			OldBrittle = -1;
			GO.pPhysics.Temperature = 25;
		}
		RemoveMyActivatedAbility(ref FreezingHandsActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

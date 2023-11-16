using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class FreezeBreath : BaseMutation
{
	public string BodyPartType = "Face";

	public bool CreateObject = true;

	public int Range = 30;

	public GameObject VaporObject;

	public int OldFreeze = -1;

	public int OldBrittle = -1;

	public string Sound = "hiss_high";

	public Guid FreezeBreathActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private static GameObject _Projectile;

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

	public FreezeBreath()
	{
		DisplayName = "Freeze Breath";
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
		E.Add("ice", 3);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandFreezeBreath");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You emit jets of frost from your mouth.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat("Emits a " + Range + "-square ray of frost in the direction of your choice\n", "Cooldown: 30 rounds\n"), "Damage: ", ComputeDamage(Level), "\n"), "Cannot wear face accessories");
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

	public void Freeze(Cell C, ScreenBuffer Buffer)
	{
		string dice = ComputeDamage();
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				if (item.PhaseMatches(ParentObject) && item.TemperatureChange(-20 - 60 * base.Level, ParentObject))
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
					@event.AddParameter("Damage", damage);
					@event.AddParameter("Owner", ParentObject);
					@event.AddParameter("Attacker", ParentObject);
					@event.AddParameter("Message", "from %o freeze!");
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

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= Range && IsMyActivatedAbilityAIUsable(FreezeBreathActivatedAbilityID) && ParentObject.HasLOSTo(E.GetGameObjectParameter("Target"), IncludeSolid: true, UseTargetability: true))
			{
				E.AddAICommand("CommandFreezeBreath");
			}
		}
		else if (E.ID == "CommandFreezeBreath")
		{
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			XRLCore.Core.RenderMapToBuffer(scrapBuffer);
			List<Cell> list = PickLine(Range, AllowVis.Any, null, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, null, null, null, Snap: true);
			if (list == null || list.Count <= 0)
			{
				return false;
			}
			if (list.Count == 1 && ParentObject.IsPlayer() && Popup.ShowYesNoCancel("Are you sure you want to target " + ParentObject.itself + "?") != 0)
			{
				return false;
			}
			CooldownMyActivatedAbility(FreezeBreathActivatedAbilityID, 30);
			UseEnergy(1000, "Physical Mutation Freeze Breath");
			PlayWorldSound(Sound, 0.5f, 0f, combat: true);
			int i = 0;
			for (int num = Math.Min(list.Count, Range); i < num; i++)
			{
				if (list.Count == 1 || list[i] != ParentObject.CurrentCell)
				{
					Freeze(list[i], scrapBuffer);
				}
				if (i < num - 1 && list[i].IsSolidFor(Projectile, ParentObject))
				{
					break;
				}
			}
		}
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		ParentObject.pPhysics.BrittleTemperature = -600 + -300 * base.Level;
		if (VaporObject != null && VaporObject.IsInvalid())
		{
			VaporObject = null;
		}
		if (VaporObject != null)
		{
			VaporObject.GetPart<TemperatureOnHit>().Amount = "-" + base.Level + "d4";
		}
		return base.ChangeLevel(NewLevel);
	}

	private void AddAbility()
	{
		FreezeBreathActivatedAbilityID = AddMyActivatedAbility("Freeze Breath", "CommandFreezeBreath", "Physical Mutation", null, "*");
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (GO.pPhysics != null)
		{
			OldFreeze = GO.pPhysics.FreezeTemperature;
			OldBrittle = GO.pPhysics.BrittleTemperature;
		}
		if (CreateObject)
		{
			Body body = GO.Body;
			if (body != null)
			{
				BodyPart firstPart = body.GetFirstPart(BodyPartType);
				if (firstPart != null)
				{
					firstPart.ForceUnequip(Silent: true);
					VaporObject = GameObjectFactory.Factory.CreateObject("Icy Vapor");
					VaporObject.GetPart<Armor>().WornOn = firstPart.Type;
					GO.ForceEquipObject(VaporObject, firstPart, Silent: true, 0);
					AddAbility();
				}
			}
		}
		else
		{
			AddAbility();
		}
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
		CleanUpMutationEquipment(GO, ref VaporObject);
		RemoveMyActivatedAbility(ref FreezeBreathActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

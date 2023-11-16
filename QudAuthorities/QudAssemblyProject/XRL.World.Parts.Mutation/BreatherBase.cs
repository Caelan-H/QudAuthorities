using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class BreatherBase : BaseMutation
{
	public string BodyPartType = "Face";

	public bool CreateObject = true;

	public GameObject FaceObject;

	public Guid ActivatedAbilityId = Guid.Empty;

	[NonSerialized]
	public string _CommandID;

	public string CommandID
	{
		get
		{
			if (_CommandID == null)
			{
				_CommandID = "CommandBreathe_" + GetType().Name;
			}
			return _CommandID;
		}
	}

	public override bool GeneratesEquipment()
	{
		if (CreateObject)
		{
			return GetFaceObject() != null;
		}
		return false;
	}

	public BreatherBase()
	{
		DisplayName = "[BreatherBase::BreatherBase()]";
	}

	public virtual string GetFaceObject()
	{
		return null;
	}

	public virtual string GetGasBlueprint()
	{
		return null;
	}

	public virtual string GetCommandDisplayName()
	{
		return "[BreatherBase::GetCommandDisplayName]";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, CommandID);
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "[BreatherBase::GetDescription]";
	}

	public override string GetLevelText(int Level)
	{
		return "[BreatherBase::GetLevelText]\n";
	}

	public int GetConeLength(int L = -1)
	{
		if (L == -1)
		{
			return 4 + base.Level;
		}
		return 4 + L;
	}

	public int GetConeAngle(int L = -1)
	{
		if (L == -1)
		{
			return 20 + 2 * base.Level;
		}
		return 20 + 2 * L;
	}

	public virtual void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
	}

	public void DrawBreathInCell(Cell C, ScreenBuffer Buffer, string color1, string color2, string color3)
	{
		Buffer.Goto(C.X, C.Y);
		string text = "&G";
		int num = Stat.Random(1, 3);
		if (num == 1)
		{
			text = "&" + color1;
		}
		if (num == 2)
		{
			text = "&" + color2;
		}
		if (num == 3)
		{
			text = "&" + color3;
		}
		int num2 = Stat.Random(1, 3);
		if (num2 == 1)
		{
			text = text + "^" + color1;
		}
		if (num2 == 2)
		{
			text = text + "^" + color2;
		}
		if (num2 == 3)
		{
			text = text + "^" + color3;
		}
		if (C.ParentZone == XRLCore.Core.Game.ZoneManager.ActiveZone)
		{
			Stat.Random(1, 3);
			Buffer.Write(text + (char)(219 + Stat.Random(0, 4)));
			Popup._TextConsole.DrawBuffer(Buffer);
			Thread.Sleep(Math.Max(1, 15 - base.Level));
		}
	}

	public virtual Gas BreatheGasInCell(Cell C, Event CreatorEvent)
	{
		string gasBlueprint = GetGasBlueprint();
		if (gasBlueprint == null)
		{
			return null;
		}
		Gas part = C.AddObject(gasBlueprint).GetPart<Gas>();
		part.Creator = ParentObject;
		CreatorEvent.SetParameter("Gas", part);
		return part;
	}

	public virtual int SortCells(Cell c1, Cell c2)
	{
		return c1.PathDistanceTo(ParentObject.pPhysics.CurrentCell).CompareTo(c2.PathDistanceTo(ParentObject.pPhysics.CurrentCell));
	}

	public static bool Cast(BreatherBase mutation = null)
	{
		if (mutation == null)
		{
			return false;
		}
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		List<Cell> list = mutation.PickCone(mutation.GetConeLength(), mutation.GetConeAngle(), AllowVis.Any);
		if (list == null || list.Count <= 0)
		{
			return false;
		}
		if (list.Count == 1 && mutation.ParentObject.IsPlayer() && Popup.ShowYesNoCancel("Are you sure you want to target " + mutation.ParentObject.itself + "?") != 0)
		{
			return false;
		}
		mutation.CooldownMyActivatedAbility(mutation.ActivatedAbilityId, 15);
		mutation.UseEnergy(1000);
		list.Sort(mutation.SortCells);
		string gasBlueprint = mutation.GetGasBlueprint();
		Event @event = ((gasBlueprint != null) ? Event.New("CreatorModifyGas", "Gas", (object)null) : null);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if ((i == 0 || mutation.ParentObject.HasLOSTo(list[i])) && (list.Count == 1 || list[i] != mutation.ParentObject.CurrentCell))
			{
				if (gasBlueprint != null)
				{
					mutation.BreatheGasInCell(list[i], @event);
					mutation.ParentObject.FireEvent(@event);
				}
				mutation.BreatheInCell(list[i], scrapBuffer);
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			if (intParameter <= GetConeLength() - 2 && IsMyActivatedAbilityAIUsable(ActivatedAbilityId) && ParentObject.HasLOSTo(gameObjectParameter, IncludeSolid: true, UseTargetability: true))
			{
				E.AddAICommand(CommandID);
			}
		}
		else if (E.ID == CommandID && !Cast(this))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public virtual bool OnMutate(GameObject GO)
	{
		return true;
	}

	public virtual bool OnUnmutate(GameObject GO)
	{
		return true;
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		Unmutate(GO);
		OnMutate(GO);
		if (CreateObject && GetFaceObject() != null)
		{
			Body body = GO.Body;
			if (body != null)
			{
				foreach (BodyPart item in body.GetPart(BodyPartType))
				{
					if (item.ForceUnequip(Silent: true))
					{
						FaceObject = GameObject.create(GetFaceObject());
						FaceObject.GetPart<Armor>().WornOn = item.Type;
						GO.ForceEquipObject(FaceObject, item, Silent: true, 0);
						break;
					}
				}
			}
		}
		ActivatedAbilityId = AddMyActivatedAbility(GetCommandDisplayName(), CommandID, "Physical Mutation", GetDescription(), "­");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		OnUnmutate(GO);
		CleanUpMutationEquipment(GO, ref FaceObject);
		if (GameObject.validate(ref FaceObject))
		{
			FaceObject.Obliterate();
		}
		RemoveMyActivatedAbility(ref ActivatedAbilityId);
		return base.Unmutate(GO);
	}
}

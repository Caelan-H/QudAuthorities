using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.Language;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class StiltWell : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CheckAttackableEvent.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CheckAttackableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Sacrifice", "sacrifice", "Sacrifice", null, 's', FireOnActor: false, 100);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Sacrifice" && GiveArtifacts(E.Actor, ParentObject))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanSmartUse");
		Object.RegisterPartEvent(this, "CommandSmartUse");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			return false;
		}
		if (E.ID == "CommandSmartUse")
		{
			GiveArtifacts(E.GetGameObjectParameter("User"), ParentObject);
		}
		return base.FireEvent(E);
	}

	public static int GetArtifactReputationValue(GameObject obj)
	{
		Examiner examiner = obj.GetPart("Examiner") as Examiner;
		int result = 0;
		if (examiner != null && examiner.Complexity > 0 && Scanning.GetScanTypeFor(obj) != 0)
		{
			result = ((obj.GetPart("Commerce") is Commerce commerce) ? Math.Max(1, (int)(commerce.Value / 10.0)) : 10);
		}
		return result;
	}

	public static bool IsValuedArtifact(GameObject Object)
	{
		return GetArtifactReputationValue(Object) > 0;
	}

	public static bool GiveArtifacts(GameObject Object, GameObject Well)
	{
		Inventory inventory = Object.Inventory;
		List<GameObject> list = new List<GameObject>();
		List<string> list2 = new List<string>();
		List<char> list3 = new List<char>();
		List<int> list4 = new List<int>();
		List<IRenderable> list5 = new List<IRenderable>();
		bool flag = false;
		char c = 'a';
		foreach (GameObject @object in inventory.Objects)
		{
			int artifactReputationValue = GetArtifactReputationValue(@object);
			if (artifactReputationValue > 0)
			{
				if (@object.IsMarkedImportantByPlayer())
				{
					flag = true;
					continue;
				}
				list.Add(@object);
				list4.Add(artifactReputationValue);
				list3.Add((c <= 'z') ? c++ : ' ');
				list5.Add(@object.pRender);
				list2.Add(@object.GetDisplayName(1120, null, null, AsIfKnown: false, Single: true) + " [{{C|+" + artifactReputationValue + "}} reputation]");
			}
		}
		if (list2.Count <= 0)
		{
			return The.Player.ShowFailure(flag ? "You have no artifacts to offer that are not important." : "You have no artifacts to offer.");
		}
		List<int> list6;
		if (Object.IsPlayer())
		{
			string[] options = list2.ToArray();
			char[] hotkeys = list3.ToArray();
			IRenderable[] icons = list5.ToArray();
			list6 = Popup.PickSeveral("Choose artifacts to throw down the well.", options, hotkeys, -1, 1, null, 60, RespectOptionNewlines: false, AllowEscape: true, 0, "", null, Well, icons);
		}
		else
		{
			list6 = new List<int> { 0 };
		}
		if (list6.IsNullOrEmpty())
		{
			return false;
		}
		int num = 0;
		List<GameObject> list7 = new List<GameObject>(list6.Count);
		foreach (int item in list6)
		{
			GameObject gameObject = list[item];
			if (!gameObject.ConfirmUseImportant(Object, "throw", "down"))
			{
				continue;
			}
			gameObject.SplitFromStack();
			if (gameObject.TryRemoveFromContext())
			{
				num += list4[item];
				list7.Add(gameObject);
				if (list4[item] >= 200 && Object.IsPlayer())
				{
					AchievementManager.SetAchievement("ACH_DONATE_ITEM_200_REP");
				}
			}
		}
		if (!list7.IsNullOrEmpty())
		{
			string text = Grammar.MakeAndList(list7.Select((GameObject x) => x.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: true)).ToList());
			IComponent<GameObject>.XDidYToZ(Object, "throw", text + " down", Well, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
			if (Object.IsPlayer())
			{
				The.Game.PlayerReputation.modify("Mechanimists", num);
			}
		}
		return true;
	}
}

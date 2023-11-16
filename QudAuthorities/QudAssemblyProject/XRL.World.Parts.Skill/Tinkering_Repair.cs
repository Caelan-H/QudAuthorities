using System;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Tinkering;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering_Repair : BaseSkill, IRepairSifrahHandler
{
	public static void Init()
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != InventoryActionEvent.ID)
		{
			return ID == OwnerGetInventoryActionsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
	{
		if (ParentObject.HasSkill("Tinkering_Repair") && IsRepairableEvent.Check(E.Actor, E.Object, null, this))
		{
			E.AddAction("Repair", "repair", "Repair", null, 'R', FireOnActor: true, 300);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Repair")
		{
			GameObject actor = E.Actor;
			GameObject item = E.Item;
			actor.GetPart<Tinkering>();
			BitLocker bitLocker = actor.RequirePart<BitLocker>();
			if (actor.GetTotalConfusion() > 0)
			{
				if (actor.IsPlayer())
				{
					Popup.ShowFail("You could not possibly attempt that in your confusion.");
				}
				return false;
			}
			if (!actor.FlightMatches(item))
			{
				if (actor.IsPlayer())
				{
					Popup.ShowFail("You cannot reach " + item.t() + " to repair " + item.them + ".");
				}
				return false;
			}
			if (!actor.PhaseMatches(item))
			{
				if (actor.IsPlayer())
				{
					Popup.ShowFail(item.Does("are") + " insubstantial; you cannot repair " + item.them + ".");
				}
				return false;
			}
			if (actor.AreHostilesNearby() && actor.FireEvent("CombatPreventsRepair"))
			{
				if (actor.IsPlayer())
				{
					Popup.ShowFail("You can't perform repairs with hostiles nearby!");
				}
				return false;
			}
			if (actor.IsPlayer() && !item.Understood())
			{
				Popup.ShowFail("You cannot repair " + item.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " until you understand " + item.them + ".");
				return false;
			}
			if (Options.SifrahRepair && actor.IsPlayer())
			{
				if (!string.IsNullOrEmpty(item.Owner) && !item.HasPropertyOrTag("DontWarnOnRepair") && Popup.ShowYesNoCancel(item.Does("are") + " not owned by you, and trying to repair " + item.them + " risks damaging " + item.them + ". Are you sure you want to do so?") != 0)
				{
					return false;
				}
				GameObject inInventory = item.InInventory;
				if (inInventory != null && inInventory != actor && !string.IsNullOrEmpty(inInventory.Owner) && inInventory.Owner != item.Owner && !inInventory.HasPropertyOrTag("DontWarnOnRepair") && Popup.ShowYesNoCancel(inInventory.Does("are", int.MaxValue, null, null, AsIfKnown: false, Single: true) + " not owned by you, and trying to repair " + item.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " inside " + inInventory.them + " risks causing damage. Are you sure you want to do so?") != 0)
				{
					return false;
				}
				RepairSifrah repairSifrah = new RepairSifrah(item, item.GetComplexity(), item.GetTier() / 2, actor.Stat("Intelligence"));
				repairSifrah.HandlerID = ParentObject.id;
				repairSifrah.HandlerPartName = base.Name;
				repairSifrah.Play(item);
				if (repairSifrah.InterfaceExitRequested)
				{
					E.RequestInterfaceExit();
				}
			}
			else
			{
				bool flag = false;
				BitCost bitCost = new BitCost(GetRepairCost());
				ModifyBitCostEvent.Process(actor, bitCost, "Repair");
				if (actor.IsPlayer())
				{
					if (!bitLocker.HasBits(bitCost))
					{
						Popup.ShowFail("You don't have <" + bitCost.ToString() + "> to repair " + item.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + "! You have:\n\n" + bitLocker.GetBitsString());
						return false;
					}
					if (Popup.ShowYesNoCancel("Do you want to spend <" + bitCost.ToString() + "> to repair " + item.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + "? You have:\n\n" + bitLocker.GetBitsString()) == DialogResult.Yes)
					{
						flag = true;
						bitLocker.UseBits(bitCost);
					}
				}
				else if (IsRepairableBy(item, actor, bitCost))
				{
					flag = true;
				}
				else
				{
					actor.FireEvent(Event.New("UnableToRepair", "Object", item));
				}
				if (flag)
				{
					RepairResultSuccess(actor, item);
				}
			}
			actor.UseEnergy(1000, "Skill Tinkering Repair");
		}
		return base.HandleEvent(E);
	}

	public static bool IsRepairableBy(GameObject obj, GameObject actor, BitCost RepairCost = null, Tinkering_Repair Skill = null, int? MaxRepairTier = null)
	{
		if (!MaxRepairTier.HasValue && Skill == null)
		{
			Skill = actor.GetPart("Tinkering_Repair") as Tinkering_Repair;
			if (Skill == null)
			{
				return false;
			}
		}
		if (!IsRepairableEvent.Check(actor, obj, null, Skill, MaxRepairTier))
		{
			return false;
		}
		int num = ((RepairCost != null) ? GetRepairTier(RepairCost) : GetRepairTier(GetRepairCost(obj)));
		if (obj.GetPart("Repair") is Repair repair)
		{
			int num2 = Math.Min(Math.Max(repair.Difficulty / 2, 0), 8);
			if (num2 > num)
			{
				num = num2;
			}
		}
		if (num > 0)
		{
			if (MaxRepairTier.HasValue)
			{
				if (MaxRepairTier < num)
				{
					return false;
				}
			}
			else if (!actor.HasSkill(DataDisk.GetRequiredSkill(num)))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsRepairableBy(GameObject actor, string RepairCost)
	{
		return IsRepairableBy(ParentObject, actor, new BitCost(RepairCost));
	}

	public static int GetRepairTier(BitCost RepairCost)
	{
		int num = 0;
		foreach (char key in RepairCost.Keys)
		{
			int bitTier = BitType.GetBitTier(key);
			if (bitTier > num)
			{
				num = bitTier;
			}
		}
		return num;
	}

	public static int GetRepairTier(string RepairCost)
	{
		int num = 0;
		int i = 0;
		for (int length = RepairCost.Length; i < length; i++)
		{
			int bitTier = BitType.GetBitTier(RepairCost[i]);
			if (bitTier > num)
			{
				num = bitTier;
			}
		}
		return num;
	}

	public int GetRepairTier()
	{
		return GetRepairTier(GetRepairCost());
	}

	public string GetRepairCost()
	{
		return GetRepairCost(ParentObject);
	}

	public static string GetRepairCost(GameObject obj)
	{
		TinkerItem tinkerItem = obj.GetPart("TinkerItem") as TinkerItem;
		bool flag = obj.HasEffect("Rusted");
		string text = (flag ? "RustedRepairCost" : "RepairCost");
		string text2 = ((tinkerItem == null) ? obj.GetStringProperty(text) : (flag ? tinkerItem.RustedRepairCost : tinkerItem.RepairCost));
		if (!string.IsNullOrEmpty(text2))
		{
			return text2;
		}
		text2 = "";
		string text3 = ((tinkerItem == null) ? BitType.ToRealBits("BC", obj.Blueprint) : tinkerItem.Bits);
		while (text2 == "")
		{
			Random random = new Random(The.Game.GetWorldSeed(obj.Blueprint + "RepairCost"));
			int num = (flag ? 75 : 50);
			int num2 = (flag ? (text3.Length - 1) : random.Next(0, text3.Length));
			for (int i = 0; i < text3.Length; i++)
			{
				if (num2 == i || random.Next(0, 100) < num)
				{
					text2 += text3[i];
				}
			}
		}
		if (tinkerItem == null)
		{
			obj.SetStringProperty(text, text2);
		}
		else if (flag)
		{
			tinkerItem.RustedRepairCost = text2;
		}
		else
		{
			tinkerItem.RepairCost = text2;
		}
		return text2;
	}

	public override bool AddSkill(GameObject GO)
	{
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		return true;
	}

	public void RepairResultSuccess(GameObject who, GameObject obj)
	{
		if (who.IsPlayer())
		{
			Popup.ShowBlock("You repair " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		}
		RepairedEvent.Send(who, obj, null, this);
	}

	public void RepairResultExceptionalSuccess(GameObject who, GameObject obj)
	{
		RepairResultSuccess(who, obj);
		string randomBits = BitType.GetRandomBits("3d6".Roll(), obj.GetComplexity());
		if (!string.IsNullOrEmpty(randomBits))
		{
			who.RequirePart<BitLocker>().AddBits(randomBits);
			if (who.IsPlayer())
			{
				Popup.Show("You receive tinkering bits <{{|" + BitType.GetDisplayString(randomBits) + "}}>");
			}
		}
	}

	public void RepairResultPartialSuccess(GameObject who, GameObject obj)
	{
		if (who.IsPlayer())
		{
			Popup.Show("You make some progress repairing " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		}
	}

	public void RepairResultFailure(GameObject who, GameObject obj)
	{
		if (who.IsPlayer())
		{
			Popup.Show("You can't figure out how to fix " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		}
	}

	public void RepairResultCriticalFailure(GameObject who, GameObject obj)
	{
		if (!RepairCriticalFailureEvent.Check(who, obj))
		{
			return;
		}
		if (obj.IsBroken())
		{
			if (!obj.HasPropertyOrTag("CantDestroyOnRepair"))
			{
				obj.PotentiallyAngerOwner(who, "DontWarnOnRepair");
				IComponent<GameObject>.XDidYToZ(who, "accidentally", "destroy", obj, null, "!", null, null, who);
				obj.Destroy();
			}
			else
			{
				RepairResultFailure(who, obj);
			}
			return;
		}
		string message = "You think you broke " + obj.them + "...";
		if (obj.ApplyEffect(new Broken()) && obj.IsBroken())
		{
			if (who.IsPlayer())
			{
				Popup.Show(message);
			}
			obj.PotentiallyAngerOwner(who, "DontWarnOnRepair");
		}
		else
		{
			RepairResultFailure(who, obj);
		}
	}
}

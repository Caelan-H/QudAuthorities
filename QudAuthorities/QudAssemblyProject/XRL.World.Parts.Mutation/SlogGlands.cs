using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SlogGlands : BaseMutation
{
	public const string EQUIPMENT_BLUEPRINT = "Bilge Sphincter";

	public new Guid ActivatedAbilityID = Guid.Empty;

	public string BodyPartType = "Tail";

	public string BackupBodyPartType = "Feet";

	public SlogGlands()
	{
		DisplayName = "Bilge Sphincter";
		Type = "Physical";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool GeneratesEquipment()
	{
		return true;
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

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeforeApplyDamage");
		Object.RegisterPartEvent(this, "CommandSlog");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You bear a sphincter-choked bilge hose that you use to slurp up nearby liquids and spew them at enemies, occasionally knocking them down.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("" + "+6 Strength\n", "+1 AV\n"), "+100 Acid Resistance\n"), "+300 reputation with mollusks\n"), "Bilge sphincter acts as a melee weapon.\n"), "+50 move speed when moving through tiles with 200+ drams of liquid\n"), "You can spew liquid from your tile into a nearby area.\n"), "Spew cooldown: 10 rounds\n"), "Spew range: 8\n"), "Spew area: 3x3\n"), "Spew chance to knock the targets down: Strength/Agility save vs. character level\n");
	}

	private LiquidVolume FindSpitVolume()
	{
		GameObject gameObject = ParentObject.CurrentCell?.GetFirstObjectWithPart("LiquidVolume");
		if (gameObject != null)
		{
			LiquidVolume liquidVolume = gameObject.LiquidVolume;
			if (liquidVolume.MaxVolume == -1 && liquidVolume.Volume > 0)
			{
				return liquidVolume;
			}
		}
		return null;
	}

	private GameObject FindBilgeSphincter()
	{
		return ParentObject.FindEquippedObject("Bilge Sphincter")?.Equipped;
	}

	private bool HasBilgeSphincter()
	{
		return FindBilgeSphincter() != null;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 10 && FindSpitVolume() != null && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && HasBilgeSphincter())
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
				if (ParentObject.HasLOSTo(gameObjectParameter, IncludeSolid: true, UseTargetability: true))
				{
					E.AddAICommand("CommandSlog");
				}
			}
		}
		else if (E.ID == "CommandSlog")
		{
			if (!HasBilgeSphincter())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("Your bilge sphincter is missing.");
				}
				return false;
			}
			if (!ParentObject.CheckFrozen())
			{
				return false;
			}
			LiquidVolume liquidVolume = FindSpitVolume();
			if (liquidVolume == null)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("There is no liquid here for you to spew.");
				}
				return false;
			}
			int statValue = ParentObject.GetStatValue("Level", 15);
			List<Cell> list = PickBurst(1, 10, bLocked: false, AllowVis.OnlyVisible);
			if (list == null)
			{
				return false;
			}
			foreach (Cell item in list)
			{
				if (item.DistanceTo(ParentObject) > 10)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("That is out of range! (10 squares)");
					}
					return false;
				}
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, 10);
			UseEnergy(1000, "Physical Mutation Bilge Sphincter");
			SlimeGlands.SlimeAnimation("&w", ParentObject.CurrentCell, list[0]);
			List<LiquidVolume> list2 = new List<LiquidVolume>();
			int num = 0;
			foreach (Cell item2 in list)
			{
				if (num != 0 && !80.in100())
				{
					continue;
				}
				GameObject gameObject = GameObject.create("Water");
				LiquidVolume liquidVolume2 = gameObject.LiquidVolume;
				liquidVolume2.ComponentLiquids.Clear();
				foreach (KeyValuePair<string, int> componentLiquid in liquidVolume.ComponentLiquids)
				{
					liquidVolume2.ComponentLiquids.Add(componentLiquid.Key, componentLiquid.Value);
				}
				list2.Add(liquidVolume2);
				item2.AddObject(gameObject);
				num++;
			}
			if (liquidVolume.Volume < list2.Count)
			{
				liquidVolume.MixWith(new LiquidVolume("slime", list2.Count - liquidVolume.Volume));
			}
			foreach (LiquidVolume item3 in list2)
			{
				item3.Volume = Math.Max(Math.Min(1000, liquidVolume.Volume) / num, 1);
				item3.Update();
			}
			liquidVolume.UseDrams(1000);
			foreach (Cell item4 in list)
			{
				foreach (GameObject item5 in item4.GetObjectsWithPart("Combat"))
				{
					if (item5 != ParentObject && !item5.MakeSave("Agility,Strength", statValue, null, null, "SlogGlands Knockdown"))
					{
						item5.ApplyEffect(new Prone());
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		BodyPart bodyPart = ParentObject.GetFirstBodyPart(BodyPartType) ?? ParentObject.GetFirstBodyPart(BackupBodyPartType);
		if (bodyPart != null)
		{
			bodyPart.ForceUnequip(Silent: true);
			GameObject gameObject = GameObject.create("Bilge Sphincter");
			gameObject.GetPart<Armor>().WornOn = bodyPart.Type;
			gameObject.RequirePart<SlogGladsItem>();
			ParentObject.ForceEquipObject(gameObject, bodyPart, Silent: true, 0);
		}
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Spew", "CommandSlog", "Physical Mutation", "You slurp up nearby liquids and spew them with your bilge sphincter, occasionally knocking enemies prone.", "­");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		CleanUpMutationEquipment(GO, FindBilgeSphincter());
		return base.Unmutate(GO);
	}
}

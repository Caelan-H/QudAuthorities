using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class DeployableInfrastructure : IPart
{
	public string DeployNoun = "tech";

	public string DeployVerb = "deploy";

	public string ModPart;

	public string ObjectBlueprint;

	public string SkillRequired;

	public string NoModIfPart;

	public string SuppressIfPart;

	public string MessageByCountSuffix;

	public int Cells = 1;

	[FieldSaveVersion(241)]
	public int MaxCells;

	public int EnergyCost = 1000;

	[FieldSaveVersion(237)]
	public bool AllowExistenceSupport;

	[FieldSaveVersion(237)]
	public bool MakeUnderstood = true;

	[FieldSaveVersion(238)]
	public bool RepairDuplicates = true;

	[FieldSaveVersion(238)]
	public bool SuppressDuplicates = true;

	[FieldSaveVersion(241)]
	public bool UsesStack;

	[FieldSaveVersion(241)]
	public bool RequireVisibility = true;

	public override bool SameAs(IPart p)
	{
		DeployableInfrastructure deployableInfrastructure = p as DeployableInfrastructure;
		if (deployableInfrastructure.DeployNoun != DeployNoun)
		{
			return false;
		}
		if (deployableInfrastructure.DeployVerb != DeployVerb)
		{
			return false;
		}
		if (deployableInfrastructure.ModPart != ModPart)
		{
			return false;
		}
		if (deployableInfrastructure.ObjectBlueprint != ObjectBlueprint)
		{
			return false;
		}
		if (deployableInfrastructure.SkillRequired != SkillRequired)
		{
			return false;
		}
		if (deployableInfrastructure.NoModIfPart != NoModIfPart)
		{
			return false;
		}
		if (deployableInfrastructure.SuppressIfPart != SuppressIfPart)
		{
			return false;
		}
		if (deployableInfrastructure.Cells != Cells)
		{
			return false;
		}
		if (deployableInfrastructure.MaxCells != MaxCells)
		{
			return false;
		}
		if (deployableInfrastructure.EnergyCost != EnergyCost)
		{
			return false;
		}
		if (deployableInfrastructure.AllowExistenceSupport != AllowExistenceSupport)
		{
			return false;
		}
		if (deployableInfrastructure.MakeUnderstood != MakeUnderstood)
		{
			return false;
		}
		if (deployableInfrastructure.RepairDuplicates != RepairDuplicates)
		{
			return false;
		}
		if (deployableInfrastructure.SuppressDuplicates != SuppressDuplicates)
		{
			return false;
		}
		if (deployableInfrastructure.UsesStack != UsesStack)
		{
			return false;
		}
		if (deployableInfrastructure.RequireVisibility != RequireVisibility)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (string.IsNullOrEmpty(SkillRequired) || IComponent<GameObject>.ThePlayer.HasSkill(SkillRequired))
		{
			E.AddAction("Deploy", string.IsNullOrEmpty(DeployNoun) ? "deploy" : ("deploy " + DeployNoun), "DeployInfrastructure", null, 'y');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "DeployInfrastructure")
		{
			if (E.Actor == null || !E.Actor.IsPlayer())
			{
				return false;
			}
			if (!string.IsNullOrEmpty(SkillRequired) && !IComponent<GameObject>.ThePlayer.HasSkill(SkillRequired))
			{
				return false;
			}
			int num = (UsesStack ? ParentObject.Count : Cells);
			if (MaxCells > 0 && num > MaxCells)
			{
				num = MaxCells;
			}
			if (num <= 0)
			{
				return false;
			}
			List<Cell> list;
			if (num != 1 || UsesStack)
			{
				list = ((!string.IsNullOrEmpty(DeployNoun)) ? PickFieldAdjacent(num, E.Actor, ColorUtility.CapitalizeExceptFormatting(DeployNoun), ReturnNullForAbort: false, RequireVisibility) : PickFieldAdjacent(num, E.Actor, null, ReturnNullForAbort: false, RequireVisibility));
			}
			else
			{
				list = new List<Cell>();
				Cell cell = ((!string.IsNullOrEmpty(DeployNoun)) ? PickDirection(ForAttack: false, POV: E.Actor, Label: ColorUtility.CapitalizeExceptFormatting(DeployNoun)) : PickDirection(ForAttack: false, null, E.Actor));
				if (cell == null)
				{
					return false;
				}
				list.Add(cell);
			}
			if (list == null || list.Count == 0)
			{
				return false;
			}
			int num2 = 0;
			foreach (Cell item in list)
			{
				if (!string.IsNullOrEmpty(SuppressIfPart) && item.HasObjectWithPart(SuppressIfPart))
				{
					continue;
				}
				if (RepairDuplicates || SuppressDuplicates)
				{
					if (!string.IsNullOrEmpty(ObjectBlueprint))
					{
						GameObject firstObject = item.GetFirstObject(ObjectBlueprint);
						if (firstObject != null)
						{
							if (RepairDuplicates)
							{
								bool flag = false;
								if (firstObject.IsBroken())
								{
									firstObject.RemoveEffect("Broken");
									flag = true;
								}
								if (firstObject.isDamaged())
								{
									firstObject.RestorePristineHealth();
									flag = true;
								}
								if (flag)
								{
									num2++;
								}
							}
							if (SuppressDuplicates)
							{
								continue;
							}
						}
					}
					if (!string.IsNullOrEmpty(ModPart))
					{
						GameObject firstObjectWithPart = item.GetFirstObjectWithPart(ModPart);
						if (firstObjectWithPart != null)
						{
							if (RepairDuplicates)
							{
								bool flag2 = false;
								if (firstObjectWithPart.IsBroken())
								{
									firstObjectWithPart.RemoveEffect("Broken");
									flag2 = true;
								}
								if (firstObjectWithPart.isDamaged())
								{
									firstObjectWithPart.RestorePristineHealth();
									flag2 = true;
								}
								if (flag2)
								{
									num2++;
								}
							}
							if (SuppressDuplicates)
							{
								continue;
							}
						}
					}
				}
				GameObject gameObject = null;
				if (!string.IsNullOrEmpty(ModPart))
				{
					gameObject = item.GetFirstObjectWithPropertyOrTag("Wall", ValidInstall) ?? item.GetFirstObjectWithPart("Physics", ValidInstall);
				}
				if (gameObject != null)
				{
					TechModding.ApplyModification(gameObject, ModPart);
					ZoneManager.PaintWalls(item.ParentZone, item.X - 1, item.Y - 1, item.X + 1, item.Y + 1);
					num2++;
				}
				else
				{
					if (string.IsNullOrEmpty(ObjectBlueprint))
					{
						continue;
					}
					GameObject gameObject2 = item.AddObject(GameObject.createUnmodified(ObjectBlueprint));
					if (gameObject2 != null)
					{
						if (MakeUnderstood)
						{
							gameObject2.MakeUnderstood();
						}
						num2++;
					}
				}
			}
			if (num2 == 0)
			{
				Popup.ShowFail("There is no useful way to " + DeployVerb + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " there.");
			}
			else
			{
				if (!string.IsNullOrEmpty(MessageByCountSuffix))
				{
					Popup.Show("You " + DeployVerb + " " + num2 + MessageByCountSuffix);
				}
				else
				{
					Popup.Show("You " + DeployVerb + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				}
				if (EnergyCost > 0)
				{
					E.Actor.UseEnergy(EnergyCost, "Tinkering");
					E.RequestInterfaceExit();
				}
				if (UsesStack)
				{
					for (int i = 0; i < num2; i++)
					{
						ParentObject.Destroy();
					}
				}
				else
				{
					ParentObject.Destroy();
				}
				The.ZoneManager.PaintWalls();
			}
		}
		return base.HandleEvent(E);
	}

	private bool ValidInstall(GameObject obj)
	{
		if (obj.IsTakeable())
		{
			return false;
		}
		if (!obj.ConsiderSolid() && !obj.IsDoor())
		{
			return false;
		}
		if (!obj.IsReal)
		{
			return false;
		}
		if (!AllowExistenceSupport && obj.HasPart("ExistenceSupport"))
		{
			return false;
		}
		if (obj.IsCombatObject())
		{
			return false;
		}
		if (!string.IsNullOrEmpty(ModPart) && obj.HasPart(ModPart))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(NoModIfPart) && obj.HasPart(NoModIfPart))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(SuppressIfPart) && obj.HasPart(SuppressIfPart))
		{
			return false;
		}
		return true;
	}
}

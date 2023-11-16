using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics_Charge : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private string ForcedMoveDirection;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (E.Forced && E.Type != "Teleporting")
		{
			ForcedMoveDirection = E.Direction ?? "?";
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "AttackerGetWeaponPenModifier");
		Object.RegisterPartEvent(this, "CommandMeleeCharge");
		base.Register(Object);
	}

	private bool ValidChargeTarget(GameObject obj)
	{
		if (obj != null && obj.HasPart("Combat"))
		{
			return obj.FlightMatches(ParentObject);
		}
		return false;
	}

	public int GetMinimumRange()
	{
		return 2;
	}

	public int GetMaximumRange()
	{
		return 3 + ParentObject.GetIntProperty("ChargeRangeModifier");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerGetWeaponPenModifier")
		{
			if ((E.GetStringParameter("Properties", "") ?? "").Contains("Charging") && E.GetStringParameter("Hand") == "Primary")
			{
				E.SetParameter("PenBonus", E.GetIntParameter("PenBonus") + 1);
				E.SetParameter("CapBonus", E.GetIntParameter("CapBonus") + 1);
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			int minimumRange = GetMinimumRange();
			int maximumRange = GetMaximumRange();
			if (intParameter >= minimumRange && intParameter <= maximumRange + 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && !ParentObject.IsFlying && ParentObject.CanChangeBodyPosition() && ParentObject.CanChangeMovementMode() && !ParentObject.IsOverburdened() && !ParentObject.AreViableHostilesAdjacent())
			{
				List<Cell> list = PickLine(maximumRange + 1, AllowVis.OnlyVisible, ValidChargeTarget);
				if (list != null)
				{
					int num = list.Count - 1;
					if (num >= minimumRange && num <= maximumRange)
					{
						int Nav = 268435456;
						int i = 0;
						for (int count = list.Count; i < count; i++)
						{
							Cell cell = list[i];
							GameObject combatTarget = cell.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 5, null, null, AllowInanimate: true, InanimateSolidOnly: true);
							if (combatTarget == gameObjectParameter)
							{
								E.AddAICommand("CommandMeleeCharge");
								break;
							}
							if (combatTarget != null && !ParentObject.IsHostileTowards(combatTarget))
							{
								break;
							}
							int num2 = cell.NavigationWeight(ParentObject, ref Nav);
							if (num2 > 40 && (i == count - 1 || num2 > 80))
							{
								break;
							}
						}
					}
				}
			}
		}
		else if (E.ID == "CommandMeleeCharge" && !PerformCharge())
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public bool PerformCharge()
	{
		if (ParentObject.OnWorldMap())
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("You cannot charge on the world map.");
			}
			return false;
		}
		if (ParentObject.IsFlying)
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("You cannot charge while flying.");
			}
			return false;
		}
		if (ParentObject.IsOverburdened())
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("You cannot charge while overburdened.");
			}
			return false;
		}
		if (!ParentObject.CanChangeBodyPosition("Charging", ShowMessage: true))
		{
			return false;
		}
		if (!ParentObject.CanChangeMovementMode("Charging", ShowMessage: true))
		{
			return false;
		}
		int minimumRange = GetMinimumRange();
		int maximumRange = GetMaximumRange();
		List<Cell> list = PickLine(maximumRange + 1, AllowVis.OnlyVisible, ValidChargeTarget, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, null, null, null, Snap: true);
		if (list == null || list.Count <= 0)
		{
			return false;
		}
		if (ParentObject.IsPlayer())
		{
			list.RemoveAt(0);
		}
		int num = list.Count - 1;
		if (num < minimumRange)
		{
			if (IsPlayer())
			{
				Popup.ShowFail("You must charge at least " + Grammar.Cardinal(minimumRange) + " " + ((minimumRange == 1) ? "space" : "spaces") + ".");
			}
			return false;
		}
		if (num > maximumRange)
		{
			if (IsPlayer())
			{
				Popup.ShowFail("You can't charge more than " + Grammar.Cardinal(maximumRange) + " " + ((maximumRange == 1) ? "space" : "spaces") + ".");
			}
			return false;
		}
		if (ParentObject.AreViableHostilesAdjacent())
		{
			if (IsPlayer())
			{
				Popup.ShowFail("You cannot charge while in melee combat.");
			}
			return false;
		}
		GameObject gameObject = null;
		Cell cell = list[list.Count - 1];
		gameObject = ((!ParentObject.IsPlayer()) ? ParentObject.Target : cell.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 5));
		if (gameObject == null)
		{
			if (IsPlayer())
			{
				gameObject = cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
				if (gameObject != null)
				{
					Popup.ShowFail("You cannot charge a flying target.");
				}
				else
				{
					Popup.ShowFail("You must charge at a target!");
				}
			}
			return false;
		}
		string text = null;
		string text2 = null;
		string colorString = null;
		string detailColor = null;
		int num2 = 10;
		if (ParentObject.GetEffect("Disguised") is Disguised disguised)
		{
			if (!string.IsNullOrEmpty(disguised.Tile) && Options.UseTiles)
			{
				text2 = disguised.Tile;
				colorString = (string.IsNullOrEmpty(disguised.TileColor) ? disguised.ColorString : disguised.TileColor);
				detailColor = disguised.DetailColor;
			}
			else
			{
				text = disguised.ColorString + disguised.RenderString;
			}
		}
		else if (!string.IsNullOrEmpty(ParentObject.pRender.Tile) && Options.UseTiles)
		{
			text2 = ParentObject.pRender.Tile;
			colorString = (string.IsNullOrEmpty(ParentObject.pRender.TileColor) ? ParentObject.pRender.ColorString : ParentObject.pRender.TileColor);
			detailColor = ParentObject.pRender.DetailColor;
		}
		else
		{
			text = ParentObject.pRender.ColorString + ParentObject.pRender.RenderString;
		}
		if (Visible())
		{
			if (text2 != null)
			{
				ParentObject.TileParticleBlip(text2, colorString, detailColor, num2, IgnoreVisibility: false, ParentObject.pRender.HFlip, ParentObject.pRender.VFlip);
			}
			else
			{
				ParentObject.ParticleBlip(text, num2);
			}
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		Cell cell2 = ParentObject.CurrentCell;
		string item = null;
		List<string> list2 = new List<string>(maximumRange + 2);
		int i = 0;
		for (int num3 = maximumRange + 2; i < num3; i++)
		{
			if (i >= list.Count)
			{
				list2.Add(item);
				continue;
			}
			Cell cell3 = list[i];
			string directionFromCell = cell2.GetDirectionFromCell(cell3);
			list2.Add(directionFromCell);
			item = directionFromCell;
			cell2 = cell3;
		}
		int j = 0;
		int count = list2.Count;
		while (true)
		{
			if (j < count)
			{
				string text3 = list2[j];
				Cell cellFromDirection = ParentObject.CurrentCell.GetCellFromDirection(text3, BuiltOnly: false);
				if (cellFromDirection != null)
				{
					bool flag6 = cellFromDirection.Objects.Contains(gameObject);
					GameObject combatTarget = cellFromDirection.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, AllowInanimate: true, InanimateSolidOnly: true);
					if (combatTarget != null)
					{
						DidXToY("charge", combatTarget, null, "!", null, null, combatTarget.IsPlayer() ? combatTarget : null);
						if (ParentObject.DistanceTo(cellFromDirection) <= 1)
						{
							ParentObject.FireEvent(Event.New("CommandAttackCell", "Cell", cellFromDirection, "Properties", "Charging"));
						}
						else
						{
							ParentObject.UseEnergy(1000, "Charging");
						}
						ParentObject.FireEvent(Event.New("ChargedTarget", "Defender", combatTarget));
						combatTarget.FireEvent(Event.New("WasCharged", "Attacker", ParentObject));
						break;
					}
					if (flag6)
					{
						flag3 = true;
					}
					else if (flag3)
					{
						flag4 = true;
						flag3 = false;
					}
					if (ParentObject.DistanceTo(gameObject) == 1)
					{
						flag = true;
					}
					else if (flag)
					{
						flag2 = true;
					}
					if (j >= maximumRange)
					{
						flag5 = true;
					}
					ForcedMoveDirection = null;
					if (ParentObject.Move(text3, Forced: false, System: false, IgnoreGravity: false, NoStack: false, null, NearestAvailable: false, 0, "Charge"))
					{
						if (ForcedMoveDirection != null)
						{
							if (ForcedMoveDirection == "U" || ForcedMoveDirection == "D" || ForcedMoveDirection == "?")
							{
								goto IL_06dc;
							}
							if (ForcedMoveDirection != text3)
							{
								int index = j + 1;
								for (; j < count; j++)
								{
									list2[index] = ForcedMoveDirection;
								}
							}
						}
						num2 += 5;
						if (Visible())
						{
							if (text2 != null)
							{
								ParentObject.TileParticleBlip(text2, colorString, detailColor, num2, IgnoreVisibility: false, ParentObject.pRender.HFlip, ParentObject.pRender.VFlip);
							}
							else
							{
								ParentObject.ParticleBlip(text, num2);
							}
						}
						j++;
						continue;
					}
				}
			}
			goto IL_06dc;
			IL_06dc:
			ForcedMoveDirection = null;
			if (flag4)
			{
				DidXToY("charge", "right through", gameObject, null, "!", null, null, ParentObject);
			}
			else if (flag2)
			{
				DidXToY("charge", "right past", gameObject, null, "!", null, null, ParentObject);
			}
			else if (flag3 || flag || flag5)
			{
				DidXToY("charge", gameObject, ", but" + ParentObject.GetVerb("fail") + " to make contact", "!", null, null, ParentObject);
			}
			else
			{
				DidX("charge", ", but" + ParentObject.Is + " brought up short", "!", null, null, ParentObject);
			}
			if (flag5)
			{
				ParentObject.ApplyEffect(new Dazed(1));
			}
			ParentObject.UseEnergy(1000, "Charging");
			break;
		}
		CooldownMyActivatedAbility(ActivatedAbilityID, 15);
		return true;
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Charge", "CommandMeleeCharge", "Skill", null, "\u0010", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}

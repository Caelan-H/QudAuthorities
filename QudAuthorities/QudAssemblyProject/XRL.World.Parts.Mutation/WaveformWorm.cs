using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class WaveformWorm : BaseMutation
{
	public new Guid ActivatedAbilityID;

	public WaveformWorm()
	{
		DisplayName = "Waveform";
		Type = "Physical";
	}

	public int GetRange(int Level)
	{
		return 5 + Level / 2;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandWaveformWorm");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You dash along a waveform.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "You dash in a direction, dealing damage to creatures you pass through.\n";
		text = text + "Range: " + GetRange(Level) + "\n";
		text = text + "Damage: " + GetDamage(Level) + "\n";
		int cooldownTurns = GetCooldownTurns(Level);
		return text + "Cooldown: " + cooldownTurns + " " + ((cooldownTurns == 1) ? "round" : "rounds") + "\n";
	}

	public virtual int GetCooldownTurns(int Level)
	{
		return 20;
	}

	public virtual string GetDamage(int Level)
	{
		return Level + "d4";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && intParameter <= GetRange(base.Level) - 1 && !ParentObject.OnWorldMap())
			{
				Cell cell = gameObjectParameter.CurrentCell;
				Cell cell2 = ParentObject.CurrentCell;
				if (cell != null && cell2 != null && (cell.X == cell2.X || cell.Y == cell2.Y || Math.Abs(cell.X - cell2.X) == Math.Abs(cell.Y - cell2.Y)))
				{
					E.AddAICommand("CommandWaveformWorm");
				}
			}
		}
		else if (E.ID == "CommandWaveformWorm")
		{
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot do that on the world map.");
				}
				return false;
			}
			Cell cell3 = PickDirection();
			if (cell3 == null)
			{
				return false;
			}
			UseEnergy(1000, "Physical Mutation Waveform");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldownTurns(base.Level));
			string directionFromCell = ParentObject.CurrentCell.GetDirectionFromCell(cell3);
			List<Cell> list = new List<Cell>();
			List<Cell> list2 = new List<Cell>();
			Cell targetCell = null;
			Cell cellFromDirection = ParentObject.pPhysics.CurrentCell;
			int num = 0;
			int i = 0;
			for (int range = GetRange(base.Level); i < range; i++)
			{
				cellFromDirection = cellFromDirection.GetCellFromDirection(directionFromCell);
				list2.Add(cellFromDirection);
				if (cellFromDirection == null)
				{
					break;
				}
				if (cellFromDirection.IsEmpty())
				{
					targetCell = cellFromDirection;
					num = i;
					list.AddRange(list2);
					list2.Clear();
				}
			}
			if (num > 0)
			{
				if (ParentObject.InActiveZone())
				{
					ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
					for (int j = 0; j <= num + 5; j++)
					{
						scrapBuffer.RenderBase();
						for (int num2 = j; num2 > j - 5; num2--)
						{
							if (num2 > 0 && num2 < list.Count)
							{
								string text = null;
								if (num2 - j == 0)
								{
									text = "&K.";
								}
								if (num2 - j == -1)
								{
									text = "&wo";
								}
								if (num2 - j == -2)
								{
									text = "&wO";
								}
								if (num2 - j == -3)
								{
									text = "&yo";
								}
								if (num2 - j == -4)
								{
									text = "&K.";
								}
								if (text != null)
								{
									scrapBuffer.Goto(list[num2].X, list[num2].Y);
									scrapBuffer.Write(text);
								}
							}
						}
						scrapBuffer.Draw();
						Thread.Sleep(10);
					}
				}
				string damage = GetDamage(base.Level);
				foreach (Cell item in list)
				{
					foreach (GameObject objectsViaEvent in item.GetObjectsViaEventList())
					{
						if (objectsViaEvent != ParentObject)
						{
							objectsViaEvent.TakeDamage(damage.RollCached(), "from %t passage!", "Crushing", null, null, null, ParentObject, null, null, Accidental: false, Environmental: false, Indirect: false, ShowUninvolved: true);
						}
					}
				}
				ParentObject.DirectMoveTo(targetCell);
			}
		}
		return base.FireEvent(E);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Waveform Dash", "CommandWaveformWorm", "Physical Mutation", GetDescription(), "~");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

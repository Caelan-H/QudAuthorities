using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Tinkering;

public class Disassembly : OngoingAction
{
	public GameObject Object;

	public int NumberWanted;

	public int NumberDone;

	public int OriginalCount;

	public int BitChance = int.MinValue;

	public int EnergyCostPer = 1000;

	public bool Auto;

	public bool WasTemporary;

	public bool Abort;

	public bool DoBitMessage;

	public string DisassembledWhat;

	public string DisassembledWhere;

	public string ReverseEngineeringMessage;

	public string BitsDone = "";

	public string InterruptBecause;

	public List<Action<GameObject>> Alarms;

	public Disassembly(GameObject Object = null, int NumberWanted = 1, bool Auto = false, List<Action<GameObject>> Alarms = null, int EnergyCostPer = 1000)
	{
		this.Object = Object;
		this.NumberWanted = NumberWanted;
		this.Auto = Auto;
		this.Alarms = Alarms;
		this.EnergyCostPer = EnergyCostPer;
		OriginalCount = this.Object.Count;
	}

	public override string GetDescription()
	{
		return "disassembling";
	}

	public override bool ShouldHostilesInterrupt()
	{
		return NumberWanted > 1;
	}

	public override bool Continue()
	{
		GameObject player = The.Player;
		if (!GameObject.validate(ref Object))
		{
			InterruptBecause = "the item you were working on disappeared";
			return false;
		}
		if (Object.IsInGraveyard())
		{
			InterruptBecause = "the item you were working on was destroyed";
			return false;
		}
		if (Object.IsNowhere())
		{
			InterruptBecause = "the item you were working on disappeared";
			return false;
		}
		if (Object.InInventory != player && !Object.InSameOrAdjacentCellTo(player))
		{
			InterruptBecause = Object.does("are") + " no longer within your reach";
			return false;
		}
		if (Object.IsInStasis())
		{
			InterruptBecause = "you can no longer interact with " + Object.t();
			return false;
		}
		if (!(Object.GetPart("TinkerItem") is TinkerItem tinkerItem))
		{
			InterruptBecause = Object.t() + " can no longer be disassembled";
			return false;
		}
		if (!tinkerItem.CanBeDisassembled(player))
		{
			InterruptBecause = Object.t() + " can no longer be disassembled";
			return false;
		}
		if (!player.CanMoveExtremities("Disassemble", ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
		{
			InterruptBecause = "you can no longer move your extremities";
			return false;
		}
		int num = 0;
		try
		{
			bool Interrupt = false;
			if (BitChance == int.MinValue)
			{
				if (tinkerItem.Bits.Length == 1)
				{
					BitChance = 0;
				}
				else
				{
					int intProperty = player.GetIntProperty("DisassembleBonus");
					BitChance = 50;
					intProperty = GetTinkeringBonusEvent.GetFor(player, Object, "Disassemble", BitChance, intProperty, ref Interrupt);
					if (Interrupt)
					{
						return false;
					}
					BitChance += intProperty;
				}
			}
			string activeBlueprint = tinkerItem.ActiveBlueprint;
			TinkerData tinkerData = null;
			List<TinkerData> list = null;
			if (player.HasSkill("Tinkering_ReverseEngineer"))
			{
				foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
				{
					if (!(tinkerRecipe.Type == "Build") || !(tinkerRecipe.Blueprint == activeBlueprint))
					{
						continue;
					}
					tinkerData = tinkerRecipe;
					foreach (TinkerData knownRecipe in TinkerData.KnownRecipes)
					{
						if (knownRecipe.Blueprint == activeBlueprint)
						{
							tinkerData = null;
							break;
						}
					}
					break;
				}
				if (Options.SifrahReverseEngineer)
				{
					foreach (TinkerData tinkerRecipe2 in TinkerData.TinkerRecipes)
					{
						if (!(tinkerRecipe2.Type == "Mod") || !Object.HasPart(tinkerRecipe2.PartName))
						{
							continue;
						}
						bool flag = false;
						foreach (TinkerData knownRecipe2 in TinkerData.KnownRecipes)
						{
							if (knownRecipe2.Blueprint == tinkerRecipe2.Blueprint)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							if (list == null)
							{
								list = new List<TinkerData>();
							}
							list.Add(tinkerRecipe2);
						}
					}
				}
			}
			int chance = 0;
			int num2 = 0;
			if (tinkerData != null || (list != null && list.Count > 0))
			{
				chance = 25;
				num2 = GetTinkeringBonusEvent.GetFor(player, Object, "ReverseEngineer", chance, num2, ref Interrupt);
				if (Interrupt)
				{
					return false;
				}
				chance += num2;
			}
			bool flag2 = Options.SifrahReverseEngineer && (tinkerData != null || (list != null && list.Count > 0));
			ReverseEngineeringSifrah reverseEngineeringSifrah = null;
			int reverseEngineerRating = 0;
			int complexity = 0;
			int difficulty = 0;
			if (flag2)
			{
				reverseEngineerRating = player.Stat("Intelligence") + num2;
				Examiner examiner = Object.GetPart("Examiner") as Examiner;
				complexity = examiner?.Complexity ?? Object.GetTier();
				difficulty = examiner?.Difficulty ?? 0;
			}
			try
			{
				InventoryActionEvent.Check(Object, player, Object, "EmptyForDisassemble");
			}
			catch (Exception x)
			{
				MetricsManager.LogError("EmptyForDisassemble", x);
			}
			bool flag3 = NumberWanted > 1 && OriginalCount > 1;
			bool isTemporary = Object.IsTemporary;
			if (DisassembledWhat == null)
			{
				DisassembledWhat = (flag3 ? Object.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true) : Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: true));
				if (flag3)
				{
					MessageQueue.AddPlayerMessage("You start disassembling " + Object.t() + ".");
				}
			}
			if (DisassembledWhere == null && Object.CurrentCell != null)
			{
				DisassembledWhere = player.DescribeDirectionToward(Object);
			}
			if (NumberDone < NumberWanted)
			{
				string text = "";
				bool flag4 = true;
				bool flag5 = false;
				bool flag6 = false;
				int num3 = 0;
				GameObject gameObject = null;
				if (!isTemporary)
				{
					if (tinkerItem.Bits.Length == 1)
					{
						if (tinkerItem.NumberMade <= 1 || Stat.Random(1, tinkerItem.NumberMade + 1) == 1)
						{
							text += tinkerItem.Bits;
						}
					}
					else
					{
						int num4 = tinkerItem.Bits.Length - 1;
						for (int i = 0; i < tinkerItem.Bits.Length; i++)
						{
							if ((num4 == i || BitChance.in100()) && (tinkerItem.NumberMade <= 1 || Stat.Random(1, tinkerItem.NumberMade + 1) == 1))
							{
								text += tinkerItem.Bits[i];
							}
						}
					}
					if (flag2)
					{
						reverseEngineeringSifrah = new ReverseEngineeringSifrah(Object, complexity, difficulty, reverseEngineerRating, tinkerData);
						reverseEngineeringSifrah.Play(Object);
						if (reverseEngineeringSifrah.Succeeded)
						{
							flag5 = true;
							if (list != null)
							{
								if (reverseEngineeringSifrah.Mods > 0)
								{
									if (list.Count > reverseEngineeringSifrah.Mods)
									{
										List<TinkerData> list2 = new List<TinkerData>();
										for (int j = 0; j < reverseEngineeringSifrah.Mods; j++)
										{
											list2.Add(list[j]);
										}
										list = list2;
									}
								}
								else
								{
									list = null;
								}
							}
							if (reverseEngineeringSifrah.Critical)
							{
								flag4 = false;
								flag6 = true;
							}
							num3 = reverseEngineeringSifrah.XP;
						}
						else
						{
							tinkerData = null;
							list = null;
							if (reverseEngineeringSifrah.Critical)
							{
								Abort = true;
								BitsDone = "";
							}
						}
					}
					else if (!chance.in100())
					{
						tinkerData = null;
						list = null;
					}
					if (tinkerData != null || (list != null && list.Count > 0))
					{
						bool flag7 = false;
						string text2 = null;
						if (tinkerData != null)
						{
							gameObject = GameObject.createSample(tinkerData.Blueprint);
							tinkerData.DisplayName = gameObject.DisplayNameOnlyDirect;
							text2 = "build " + (gameObject.IsPlural ? gameObject.DisplayNameOnlyDirect : gameObject.GetPluralName());
							flag7 = true;
						}
						if (list != null)
						{
							List<string> list3 = new List<string>();
							foreach (TinkerData item in list)
							{
								list3.Add(item.DisplayName);
							}
							string text3 = "install the " + ((list3.Count == 1) ? "mod" : "mods") + " " + Grammar.MakeAndList(list3);
							text2 = ((text2 != null) ? (text2 + " and " + text3) : text3);
						}
						if (text2 != null)
						{
							ReverseEngineeringMessage = "{{G|Eureka! You may now " + text2;
							if (flag4)
							{
								ReverseEngineeringMessage += ".";
							}
							else
							{
								ReverseEngineeringMessage = ReverseEngineeringMessage + "... and were able to work out how without needing to destroy " + ((!flag7) ? Object.t() : (Object.IsPlural ? "these" : "this one")) + "!";
							}
							ReverseEngineeringMessage += "}}";
						}
						if (tinkerData != null)
						{
							TinkerData.KnownRecipes.Add(tinkerData);
						}
						if (list != null)
						{
							TinkerData.KnownRecipes.AddRange(list);
						}
					}
					else if (flag5)
					{
						ReverseEngineeringMessage = "You are unable to make further progress reverse engineering " + Object.poss("modding") + ".";
					}
					if (num3 > 0)
					{
						player.AwardXP(num3);
					}
					if (flag6)
					{
						TinkeringSifrah.AwardInsight();
					}
				}
				NumberDone++;
				if (NumberWanted > 1)
				{
					Loading.SetLoadingStatus("Disassembled " + NumberDone.Things("item") + " of " + NumberWanted + "...");
				}
				if (!Abort)
				{
					if (player.HasRegisteredEvent("ModifyBitsReceived"))
					{
						Event @event = Event.New("ModifyBitsReceived", "Item", Object, "Bits", text);
						player.FireEvent(@event);
						text = @event.GetStringParameter("Bits", "");
					}
					BitsDone += text;
				}
				num += EnergyCostPer;
				DoBitMessage = true;
				if (flag4)
				{
					if (Alarms != null)
					{
						foreach (Action<GameObject> alarm in Alarms)
						{
							alarm(player);
						}
						Alarms = null;
					}
					Object.Destroy();
				}
			}
		}
		finally
		{
			if (num > 0)
			{
				player.UseEnergy(num, "Skill Tinkering Disassemble");
			}
		}
		return true;
	}

	public override string GetInterruptBecause()
	{
		return InterruptBecause;
	}

	public override bool CanComplete()
	{
		if (!Abort)
		{
			return NumberDone >= NumberWanted;
		}
		return true;
	}

	public override void Interrupt()
	{
		if (NumberWanted > 1)
		{
			Loading.SetLoadingStatus("Interrupted!");
		}
		base.Interrupt();
	}

	public override void Complete()
	{
		if (NumberWanted > 1)
		{
			Loading.SetLoadingStatus("Finished disassembling.");
		}
		base.Complete();
	}

	public override void End()
	{
		GameObject player = The.Player;
		if (NumberDone > 0)
		{
			string text = null;
			if (BitsDone != "")
			{
				player.RequirePart<BitLocker>().AddBits(BitsDone);
				if (DoBitMessage)
				{
					text = "You receive tinkering bits <{{|" + BitType.GetDisplayString(BitsDone) + "}}>.";
				}
			}
			else if (WasTemporary)
			{
				text = "The parts crumble into dust.";
			}
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("You disassemble ").Append(DisassembledWhat ?? "something");
			if (NumberDone > 1 || NumberWanted > 1)
			{
				stringBuilder.Append(" x").Append(NumberDone);
			}
			if (!string.IsNullOrEmpty(DisassembledWhere))
			{
				stringBuilder.Append(' ').Append(DisassembledWhere);
			}
			stringBuilder.Append('.');
			if (!string.IsNullOrEmpty(ReverseEngineeringMessage))
			{
				stringBuilder.Compound(ReverseEngineeringMessage, ' ');
			}
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.Compound(text, ' ');
			}
			if (Auto && string.IsNullOrEmpty(ReverseEngineeringMessage))
			{
				MessageQueue.AddPlayerMessage(stringBuilder.ToString());
			}
			else
			{
				Popup.Show(stringBuilder.ToString());
			}
		}
		base.End();
	}
}

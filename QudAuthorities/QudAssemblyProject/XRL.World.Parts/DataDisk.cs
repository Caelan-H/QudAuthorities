using System;
using System.Text;
using XRL.Core;
using XRL.Language;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class DataDisk : IPart
{
	public TinkerData Data;

	public int TargetTier = 1;

	[NonSerialized]
	public string ObjectName;

	[NonSerialized]
	private static StringBuilder SB = new StringBuilder();

	[NonSerialized]
	private static BitCost Cost = new BitCost();

	public override bool SameAs(IPart p)
	{
		DataDisk dataDisk = p as DataDisk;
		if (dataDisk.Data != Data)
		{
			return false;
		}
		if (dataDisk.TargetTier != TargetTier)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool CanGenerateStacked()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		SB.Clear();
		if (Data.Type == "Build")
		{
			try
			{
				if (ObjectName == null)
				{
					if (Data.Blueprint == null)
					{
						ObjectName = "invalid blueprint: " + Data.Blueprint;
					}
					else
					{
						ObjectName = TinkeringHelpers.TinkeredItemShortDisplayName(Data.Blueprint);
					}
				}
			}
			catch
			{
				ObjectName = "error:" + Data.Blueprint;
			}
			if (E.AsIfKnown || (E.Understood() && The.Player != null && (The.Player.HasSkill("Tinkering") || Scanning.HasScanningFor(The.Player, Scanning.Scan.Tech))))
			{
				SB.Append(": {{C|").Append(ObjectName).Append("}} <");
				Cost.Clear();
				Cost.Import(TinkerItem.GetBitCostFor(Data.Blueprint));
				ModifyBitCostEvent.Process(The.Player, Cost, "DataDisk");
				Cost.ToStringBuilder(SB);
				SB.Append('>');
			}
		}
		else if (Data.Type == "Mod")
		{
			ObjectName = "[{{W|Item mod}}] - {{C|" + Data.DisplayName + "}}";
			if (E.AsIfKnown || (E.Understood() && The.Player != null && (The.Player.HasSkill("Tinkering") || Scanning.HasScanningFor(The.Player, Scanning.Scan.Tech))))
			{
				SB.Append(": ").Append(ObjectName);
			}
		}
		if (SB.Length > 0)
		{
			E.AddBase(SB.ToString(), 5);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Data.Type == "Mod")
		{
			E.Postfix.Append("\nAdds item modification: ").Append(TechModding.GetModificationDescription(Data.Blueprint, 0));
		}
		else
		{
			GameObject gameObject = GameObject.createSample(Data.Blueprint);
			if (gameObject != null)
			{
				TinkeringHelpers.StripForTinkering(gameObject);
				TinkerItem tinkerItem = gameObject.GetPart("TinkerItem") as TinkerItem;
				Description description = gameObject.GetPart("Description") as Description;
				E.Postfix.Append("\n{{rules|Creates:}} ");
				if (tinkerItem != null && tinkerItem.NumberMade > 1)
				{
					E.Postfix.Append(Grammar.Cardinal(tinkerItem.NumberMade)).Append(' ').Append(Grammar.Pluralize(gameObject.DisplayNameOnlyDirect));
				}
				else
				{
					E.Postfix.Append(gameObject.DisplayNameOnlyDirect);
				}
				E.Postfix.Append("\n");
				if (description != null)
				{
					E.Postfix.Append('\n').Append(description._Short);
				}
				gameObject.Obliterate();
			}
		}
		E.Postfix.Append("\n\n{{rules|Requires:}} ").Append(GetRequiredSkillFriendly());
		if (TinkerData.RecipeKnown(Data))
		{
			E.Postfix.Append("\n\n{{rules|You already know this recipe.}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood())
		{
			if (Data.Type == "Build")
			{
				E.AddAction("Build", "build", "BuildFromDataDisk", null, 'b', FireOnActor: false, 200);
			}
			E.AddAction("Learn", "learn", "LearnFromDataDisk", null, 'n', FireOnActor: false, 300);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "LearnFromDataDisk")
		{
			if (TinkerData.RecipeKnown(Data))
			{
				Popup.ShowFail("You already know that recipe!");
			}
			else if (!IComponent<GameObject>.ThePlayer.HasSkill(GetRequiredSkill()))
			{
				Popup.ShowFail("You don't have the required skill: " + GetRequiredSkillFriendly() + "!");
			}
			else
			{
				if (Data.Type == "Mod")
				{
					Popup.Show("You learn the item modification {{W|" + Data.DisplayName + "}}.");
				}
				else
				{
					GameObject gameObject = GameObject.createSample(Data.Blueprint);
					gameObject.MakeUnderstood();
					Popup.Show("You learn to build " + (gameObject.IsPlural ? gameObject.DisplayNameOnlyDirect : Grammar.Pluralize(gameObject.DisplayNameOnlyDirect)) + ".");
					gameObject.Destroy();
				}
				TinkerData.KnownRecipes.Add(Data);
				ParentObject.Destroy();
			}
		}
		else if (E.Command == "BuildFromDataDisk")
		{
			GameObject actor = E.Actor;
			actor.GetPart<XRL.World.Parts.Skill.Tinkering>();
			BitLocker bitLocker = actor.RequirePart<BitLocker>();
			if (!(Data.Type != "Build"))
			{
				if (!actor.HasSkill(GetRequiredSkill()))
				{
					Popup.ShowFail("You don't have the required skill: " + GetRequiredSkillFriendly() + "!");
				}
				else
				{
					string bitCostFor = TinkerItem.GetBitCostFor(Data.Blueprint);
					if (!bitLocker.HasBits(bitCostFor))
					{
						Popup.ShowFail("You don't have the required <" + BitType.GetString(bitCostFor) + "> bits! You have:\n\n" + bitLocker.GetBitsString());
					}
					else
					{
						bool flag = actor.AreHostilesNearby();
						if (flag && actor.FireEvent("CombatPreventsTinkering"))
						{
							if (actor.IsPlayer())
							{
								Popup.ShowFail("You can't tinker with hostiles nearby!");
							}
							return false;
						}
						if (!actor.CheckFrozen())
						{
							return false;
						}
						Inventory inventory = actor.Inventory;
						GameObject gameObject2 = null;
						if (!string.IsNullOrEmpty(Data.Ingredient))
						{
							string[] array = Data.Ingredient.Split(',');
							foreach (string blueprint in array)
							{
								gameObject2 = inventory.FindObjectByBlueprint(blueprint);
								if (gameObject2 != null)
								{
									break;
								}
							}
							if (gameObject2 == null)
							{
								string text = "";
								array = Data.Ingredient.Split(',');
								foreach (string blueprint2 in array)
								{
									if (text != "")
									{
										text += " or ";
									}
									text += GameObjectFactory.Factory.CreateSampleObject(blueprint2).DisplayName;
								}
								Popup.ShowFail("You don't have the required ingredient: " + text + "!");
								goto IL_049c;
							}
						}
						if (!string.IsNullOrEmpty(Data.Ingredient))
						{
							gameObject2.SplitStack(1, XRLCore.Core.Game.Player.Body);
							if (!inventory.FireEvent(Event.New("CommandRemoveObject", "Object", gameObject2)))
							{
								Popup.ShowFail("You cannot use the ingredient!");
								goto IL_049c;
							}
						}
						bitLocker.UseBits(TinkerItem.GetBitCostFor(Data.Blueprint));
						GameObject gameObject3 = GameObject.createSample(Data.Blueprint);
						TinkeringHelpers.ProcessTinkeredItem(gameObject3);
						TinkerItem part = gameObject3.GetPart<TinkerItem>();
						GameObject gameObject4 = null;
						for (int j = 0; j < Math.Max(part.NumberMade, 1); j++)
						{
							gameObject4 = GameObject.create(Data.Blueprint);
							TinkeringHelpers.ProcessTinkeredItem(gameObject4);
							inventory.AddObject(gameObject4);
						}
						if (part.NumberMade > 1)
						{
							Popup.Show("You tinker up " + Grammar.Cardinal(part.NumberMade) + " " + Grammar.Pluralize(gameObject3.DisplayNameOnly) + "!");
						}
						else
						{
							Popup.Show("You tinker up " + gameObject4.a + gameObject4.DisplayNameOnly + "!");
						}
						actor.UseEnergy(1000, "Skill Tinkering Data Disk Build");
						if (flag)
						{
							E.RequestInterfaceExit();
						}
						gameObject3.Obliterate();
					}
				}
			}
		}
		goto IL_049c;
		IL_049c:
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Data == null)
		{
			GameObjectBlueprint value;
			do
			{
				Data = TinkerData.TinkerRecipes.GetRandomElement();
			}
			while (GameObjectFactory.Factory.Blueprints.TryGetValue(Data.Blueprint, out value) && value.Tags.ContainsKey("NoDataDisk"));
		}
		if (ParentObject.GetPart("Commerce") is Commerce commerce)
		{
			if (Data.Cost.Contains("M"))
			{
				commerce.Value = 450.0;
			}
			else if (Data.Cost.Contains("Y"))
			{
				commerce.Value = 400.0;
			}
			else if (Data.Cost.Contains("W"))
			{
				commerce.Value = 350.0;
			}
			else if (Data.Cost.Contains("K"))
			{
				commerce.Value = 300.0;
			}
			else if (Data.Cost.Contains("c"))
			{
				commerce.Value = 250.0;
			}
			else if (Data.Cost.Contains("b"))
			{
				commerce.Value = 200.0;
			}
			else if (Data.Cost.Contains("g"))
			{
				commerce.Value = 150.0;
			}
			else if (Data.Cost.Contains("r"))
			{
				commerce.Value = 100.0;
			}
			else
			{
				commerce.Value = 50.0;
			}
		}
		return base.HandleEvent(E);
	}

	public static string GetRequiredSkill(int Tier)
	{
		if (Tier <= 3)
		{
			return "Tinkering_Tinker1";
		}
		if (Tier <= 6)
		{
			return "Tinkering_Tinker2";
		}
		return "Tinkering_Tinker3";
	}

	public string GetRequiredSkill()
	{
		return GetRequiredSkill(Data.Tier);
	}

	public static string GetRequiredSkillFriendly(int Tier)
	{
		if (Tier <= 3)
		{
			return "Tinker I";
		}
		if (Tier <= 6)
		{
			return "Tinker II";
		}
		return "Tinker III";
	}

	public string GetRequiredSkillFriendly()
	{
		return GetRequiredSkillFriendly(Data.Tier);
	}
}

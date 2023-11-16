using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qud.API;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Tinkering;

namespace XRL.World.Conversations.Parts;

public class KithAndKinFinale : IKithAndKinPart
{
	private HindrenQuestOutcome Outcome;

	private string Key;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != EnteredElementEvent.ID && ID != LeftElementEvent.ID && ID != PrepareTextEvent.ID)
		{
			return ID == GetTextElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		Outcome = new HindrenQuestOutcome
		{
			loveState = ((The.Game.GetQuestFinishTime("Love and Fear") > 0) ? "love" : "nolove"),
			thief = base.Thief,
			circumstance = base.CircumstanceInfluence,
			motive = base.MotiveInfluence
		};
		The.Game.SetStringGameState("HindrenMysteryOutcomeClimate", IKithAndKinPart.KeyOf(Outcome.DetermineClimate()));
		The.Game.SetStringGameState("HindrenMysteryOutcomeCircumstance", IKithAndKinPart.KeyOf(Outcome.circumstance));
		The.Game.SetStringGameState("HindrenMysteryOutcomeThief", IKithAndKinPart.KeyOf(Outcome.thief));
		The.Game.SetStringGameState("HindrenMysteryOutcomeHindriarch", IKithAndKinPart.KeyOf(Outcome.CurrentHindriarch()));
		The.Game.RemoveStringGameState("KithAndKinEliminated");
		Key = IKithAndKinPart.KeyOf(Outcome.thief) + IKithAndKinPart.KeyOf(Outcome.climate) + IKithAndKinPart.KeyOf(Outcome.loveState);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftElementEvent E)
	{
		GameObject gameObject = GameObject.create("Widget");
		gameObject.AddPart(Outcome);
		HindrenMysteryGamestate.instance.getBeyLahZone().GetCell(0, 0).AddObject(gameObject);
		The.Game.FinishQuest("Kith and Kin");
		The.Game.FinishQuest("Find Eskhind");
		Outcome.ApplyPrologue();
		Reward();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTextElementEvent E)
	{
		E.Selected = E.Visible.FirstOrDefault((ConversationText x) => x.ID == Key) ?? E.Selected;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		string circumstanceInfluence = base.CircumstanceInfluence;
		string motiveInfluence = base.MotiveInfluence;
		string newValue = ((circumstanceInfluence == motiveInfluence) ? circumstanceInfluence : (circumstanceInfluence + " and " + motiveInfluence));
		E.Text.Replace("=all.influence=", newValue).Replace("=motive.influence=", motiveInfluence).Replace("=thief.name=", base.ThiefName);
		return base.HandleEvent(E);
	}

	public void Reward()
	{
		_ = Outcome.climate;
		string motive = Outcome.motive;
		string circumstance = Outcome.circumstance;
		string thief = Outcome.thief;
		List<GameObject> list = new List<GameObject>();
		List<string> list2 = new List<string>();
		bool flag = false;
		switch (thief)
		{
		case "keh":
			switch (motive)
			{
			case "violence":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Dagger3"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Dagger3"));
				}
				break;
			case "illness":
			{
				int num4 = 4;
				if (circumstance == "violence")
				{
					num4 = 8;
				}
				for (int l = 0; l < num4; l++)
				{
					list.Add(GameObject.create(PopulationManager.RollOneFrom("DynamicObjectsTable:Tonics_NonRare").Blueprint));
				}
				break;
			}
			case "craft":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Homoelectric Wrist Warmer"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Homoelectric Wrist Warmer"));
				}
				break;
			case "trade":
			{
				int num3 = 3;
				if (circumstance == "violence")
				{
					num3 = 8;
				}
				if (circumstance == "trade")
				{
					num3 = 6;
					flag = true;
				}
				for (int k = 0; k < num3; k++)
				{
					list.Add(GameObject.create(PopulationManager.RollOneFrom("BooksAndRandomBooks").Blueprint));
				}
				break;
			}
			}
			break;
		case "esk":
			switch (motive)
			{
			case "violence":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Chain Pistol"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Chain Pistol"));
				}
				break;
			case "illness":
				list.Add(GameObjectFactory.Factory.CreateObject("FluxPhial"));
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject("FluxPhial"));
				}
				break;
			case "craft":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Magnetized Boots"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Magnetized Boots"));
				}
				break;
			case "trade":
			{
				int num2 = 3;
				for (int j = 0; j < num2; j++)
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints[PopulationManager.RollOneFrom("DynamicObjectsTable:EnergyCells:Tier0-4").Blueprint], 0, 2));
				}
				break;
			}
			}
			break;
		case "kese":
			switch (motive)
			{
			case "violence":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Ari"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Ari"));
				}
				if (circumstance == "craft")
				{
					list2.Add("mod:ModFlaming");
				}
				break;
			case "illness":
				BodyPart.MakeSeveredBodyParts((circumstance == "violence") ? 4 : 2, null, "Face", null, null, null, null, list);
				break;
			case "craft":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Carbide Plate Armor", 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Carbide Plate Armor"));
				}
				break;
			case "trade":
			{
				int num = ((circumstance == "violence") ? 8 : 4);
				for (int i = 0; i < num; i++)
				{
					list.Add(GameObject.create(PopulationManager.RollOneFrom("DynamicObjectsTable:Grenades").Blueprint));
				}
				break;
			}
			}
			break;
		case "kendren":
			switch (motive)
			{
			case "violence":
			{
				List<GameObject> list4 = new List<GameObject>();
				while (list4.Count < 3)
				{
					string blueprint = EncountersAPI.GetAnItemBlueprint((GameObjectBlueprint b) => b.Tier == 3 && b.DescendsFrom("MeleeWeapon") && EncountersAPI.IsEligibleForDynamicEncounters(b));
					if (!list4.Any((GameObject o) => o.Blueprint == blueprint))
					{
						if (circumstance == "violence")
						{
							list4.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints[blueprint], 0, 2));
						}
						else
						{
							list4.Add(GameObjectFactory.Factory.Blueprints[blueprint].createOne());
						}
					}
				}
				list4.ForEach(delegate(GameObject o)
				{
					o.MakeUnderstood();
				});
				list.Add(ConversationsAPI.chooseOneItem(list4, "Choose an item", allowEscape: false));
				break;
			}
			case "illness":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Bio-Scanning Bracelet"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Bio-Scanning Bracelet"));
				}
				break;
			case "craft":
			{
				List<GameObject> list3 = new List<GameObject>();
				while (list3.Count < 3)
				{
					string blueprint2 = EncountersAPI.GetAnItemBlueprint((GameObjectBlueprint b) => b.Tier == 3 && b.DescendsFrom("Armor") && EncountersAPI.IsEligibleForDynamicEncounters(b));
					if (!list3.Any((GameObject o) => o.Blueprint == blueprint2))
					{
						if (circumstance == "violence")
						{
							list3.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints[blueprint2], 0, 2));
						}
						else
						{
							list3.Add(GameObjectFactory.Factory.Blueprints[blueprint2].createOne());
						}
					}
				}
				list3.ForEach(delegate(GameObject o)
				{
					o.MakeUnderstood();
				});
				list.Add(ConversationsAPI.chooseOneItem(list3, "Choose an item", allowEscape: false));
				break;
			}
			case "trade":
				list.Add(GameObjectFactory.Factory.CreateObject("Hoversled"));
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Hoversled"));
				}
				break;
			}
			break;
		}
		if (circumstance == "illness")
		{
			for (int m = 0; m < 4; m++)
			{
				list.Add(GameObject.create(PopulationManager.RollOneFrom("DynamicObjectsTable:Tonics_NonRare").Blueprint));
			}
		}
		if (circumstance == "trade" && !flag)
		{
			List<GameObject> list5 = new List<GameObject>();
			foreach (GameObject item in list)
			{
				list5.Add(item.DeepCopy(CopyEffects: false, CopyID: true));
			}
			list.AddRange(list5);
		}
		list.ForEach(delegate(GameObject o)
		{
			o.MakeUnderstood();
		});
		list.Sort((GameObject o1, GameObject o2) => string.Compare(o1.DisplayNameOnlyDirect, o2.DisplayNameOnlyDirect, StringComparison.Ordinal));
		StringBuilder stringBuilder = Event.NewStringBuilder();
		int num5 = 0;
		stringBuilder.AppendLine("In return for your service, you receive:\n");
		foreach (GameObject item2 in list)
		{
			stringBuilder.AppendLine(item2.DisplayName);
			The.Player.ReceiveObject(item2);
			if (circumstance == "craft" && (!(thief == "kese") || !(motive == "violence")))
			{
				if (item2.HasPart("TinkerItem") && item2.GetPart<TinkerItem>().CanBuild)
				{
					list2.Add(item2.Blueprint);
				}
				else
				{
					num5 = 1;
				}
			}
		}
		if (num5 > 0)
		{
			stringBuilder.AppendLine("failed energy relay x" + num5);
			stringBuilder.AppendLine("fried processing core x" + num5);
			stringBuilder.AppendLine("cracked robotics housing x" + num5 * 2);
			for (int n = 0; n < num5; n++)
			{
				The.Player.ReceiveObject("Scrap 1");
			}
			for (int num6 = 0; num6 < num5; num6++)
			{
				The.Player.ReceiveObject("Scrap 2");
			}
			for (int num7 = 0; num7 < num5 * 2; num7++)
			{
				The.Player.ReceiveObject("Scrap 2");
			}
		}
		if (list2.Count > 0)
		{
			foreach (string item3 in list2)
			{
				GameObject gameObject = TinkerData.createDataDisk(item3);
				gameObject.MakeUnderstood();
				The.Player.TakeObject(gameObject, Silent: true, 0);
				stringBuilder.AppendLine(gameObject.DisplayName);
			}
		}
		Popup.Show(stringBuilder.ToString());
	}
}

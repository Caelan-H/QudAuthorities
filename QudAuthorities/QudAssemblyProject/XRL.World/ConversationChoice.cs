using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Qud.API;
using UnityEngine;
using XRL.Annals;
using XRL.Language;
using XRL.Names;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.QuestManagers;

namespace XRL.World;

[Serializable]
public class ConversationChoice : IComparable<ConversationChoice>
{
	public const int WATER_RITUAL_ORDINAL = 980;

	public const int TRADE_ORDINAL = 990;

	public const int ASK_NAME_ORDINAL = 10000;

	public const int END_SORT_ORDINAL = 999999;

	public int Ordinal;

	public string Text = "";

	public string GotoID = "";

	public string Achievement = "";

	public string ID = "";

	public int LineNumber;

	public string IfHaveQuest;

	public string IfHaveItemWithID;

	public string IfHaveState;

	public string IfNotHaveState;

	public string IfTestState;

	public string IfHavePart;

	public string IfNotHavePart;

	public string IfNotHaveQuest;

	public string IfFinishedQuest;

	public string IfNotFinishedQuest;

	public string IfFinishedQuestStep;

	public string IfNotFinishedQuestStep;

	public string IfHaveObservation;

	public string IfNotHaveObservation;

	public string IfHaveObservationWithTag;

	public string IfHaveSultanNoteWithTag;

	public string IfHaveVillageNote;

	public bool IfTrueKin;

	public bool IfNotTrueKin;

	public string IfGenotype;

	public string IfSubtype;

	public string IfNotGenotype;

	public string IfNotSubtype;

	public string RevealObservation;

	public string StartQuest;

	public string FinishQuest;

	public string RevealMapNoteId;

	public string CompleteQuestStep;

	public string SpecialRequirement;

	public string Execute;

	public string IdGift;

	public string GiveItem;

	public string GiveOneItem;

	public string TakeItem;

	public string ClearOwner;

	public string CallScript;

	public string SetStringState;

	public string SetIntState;

	public string AddIntState;

	public string SetBooleanState;

	public string ToggleBooleanState;

	public Func<bool> IfDelegate;

	public string IfWearingBlueprint;

	public string IfHasBlueprint;

	public string TakeBlueprint;

	public string Filter;

	public object ScriptObject;

	public string AcceptQuestText;

	public Func<bool> onAction;

	public ConversationNode ParentNode;

	private Event eGetConversationNode = new Event("GetConversationNode");

	[NonSerialized]
	private static int lastChoice = 0;

	private static char[] commaSplitter = new char[1] { ',' };

	public bool Visited => ConversationNode.VisitedNodes.ContainsKey(ParentNode?.ID + ID);

	public void Copy(ConversationChoice Source)
	{
		Ordinal = Source.Ordinal;
		Text = Source.Text;
		GotoID = Source.GotoID;
		Achievement = Source.Achievement;
		ID = Source.ID;
		IfHaveQuest = Source.IfHaveQuest;
		IfHaveItemWithID = Source.IfHaveItemWithID;
		IfHaveState = Source.IfHaveState;
		IfNotHaveState = Source.IfNotHaveState;
		IfTestState = Source.IfTestState;
		IfHavePart = Source.IfHavePart;
		IfNotHavePart = Source.IfNotHavePart;
		IfNotHaveQuest = Source.IfNotHaveQuest;
		IfFinishedQuest = Source.IfFinishedQuest;
		IfNotFinishedQuest = Source.IfNotFinishedQuest;
		IfFinishedQuestStep = Source.IfFinishedQuestStep;
		IfNotFinishedQuestStep = Source.IfNotFinishedQuestStep;
		IfHaveObservation = Source.IfHaveObservation;
		IfNotHaveObservation = Source.IfNotHaveObservation;
		IfHaveSultanNoteWithTag = Source.IfHaveSultanNoteWithTag;
		IfHaveObservationWithTag = Source.IfHaveObservationWithTag;
		IfHaveVillageNote = Source.IfHaveVillageNote;
		IfTrueKin = Source.IfTrueKin;
		IfNotTrueKin = Source.IfNotTrueKin;
		IfGenotype = Source.IfGenotype;
		IfNotGenotype = Source.IfNotGenotype;
		IfSubtype = Source.IfSubtype;
		IfNotSubtype = Source.IfNotSubtype;
		RevealObservation = Source.RevealObservation;
		StartQuest = Source.StartQuest;
		FinishQuest = Source.FinishQuest;
		RevealMapNoteId = Source.RevealMapNoteId;
		CompleteQuestStep = Source.CompleteQuestStep;
		SpecialRequirement = Source.SpecialRequirement;
		Execute = Source.Execute;
		IdGift = Source.IdGift;
		GiveItem = Source.GiveItem;
		GiveOneItem = Source.GiveOneItem;
		TakeItem = Source.TakeItem;
		ClearOwner = Source.ClearOwner;
		CallScript = Source.CallScript;
		SetStringState = Source.SetStringState;
		SetIntState = Source.SetIntState;
		AddIntState = Source.AddIntState;
		SetBooleanState = Source.SetBooleanState;
		ToggleBooleanState = Source.ToggleBooleanState;
		IfDelegate = Source.IfDelegate;
		IfWearingBlueprint = Source.IfWearingBlueprint;
		IfHasBlueprint = Source.IfHasBlueprint;
		TakeBlueprint = Source.TakeBlueprint;
		Filter = Source.Filter;
		ScriptObject = Source.ScriptObject;
		AcceptQuestText = Source.AcceptQuestText;
		onAction = Source.onAction;
		ParentNode = Source.ParentNode;
	}

	public string GetScriptClassName()
	{
		return "ConversationChoiceScript_" + ParentNode.ParentConversation.ID + "_" + ParentNode.ID + "_" + Ordinal;
	}

	public static bool TestSpecialRequirement(string SpecialRequirement)
	{
		if (string.IsNullOrEmpty(SpecialRequirement))
		{
			return true;
		}
		if (SpecialRequirement.StartsWith("LovedByConsortium") && The.Game.PlayerReputation.getAttitude("Consortium") != 2)
		{
			return false;
		}
		if (SpecialRequirement.StartsWith("IsMapNoteRevealed:"))
		{
			return JournalAPI.IsMapOrVillageNoteRevealed(SpecialRequirement.Split(':')[1]);
		}
		if (SpecialRequirement.StartsWith("!IsMapNoteRevealed:"))
		{
			return !JournalAPI.IsMapOrVillageNoteRevealed(SpecialRequirement.Split(':')[1]);
		}
		return true;
	}

	public static bool TestHaveItemWithID(string IfHaveItemWithID)
	{
		if (string.IsNullOrEmpty(IfHaveItemWithID))
		{
			return true;
		}
		bool num = IfHaveItemWithID[0] == '!';
		if (num)
		{
			IfHaveItemWithID = IfHaveItemWithID.Substring(1);
		}
		bool flag = The.Player.HasObjectInInventory((GameObject o) => o.idmatch(IfHaveItemWithID));
		if (!num)
		{
			return flag;
		}
		return !flag;
	}

	public virtual bool Test()
	{
		if (TestFilter() && (IfDelegate == null || IfDelegate()) && (IfHaveState == null || The.Game.HasGameState(IfHaveState)) && (IfNotHaveState == null || !The.Game.HasGameState(IfNotHaveState)) && (IfTestState == null || The.Game.TestGameState(IfTestState)) && (IfHavePart == null || The.Player.HasPart(IfHavePart)) && (IfNotHavePart == null || !The.Player.HasPart(IfHavePart)) && (IfHaveQuest == null || The.Game.HasQuest(IfHaveQuest)) && TestHaveItemWithID(IfHaveItemWithID) && (IfNotHaveQuest == null || !The.Game.HasQuest(IfNotHaveQuest)) && (IfNotFinishedQuest == null || !The.Game.FinishedQuest(IfNotFinishedQuest)) && (IfHaveSultanNoteWithTag == null || JournalAPI.HasSultanNoteWithTag(IfHaveSultanNoteWithTag)) && (!IfTrueKin || The.Player.IsTrueKin()) && (!IfNotTrueKin || !The.Player.IsTrueKin()) && (IfGenotype == null || The.Player.GetGenotype() == IfGenotype) && (IfSubtype == null || The.Player.GetSubtype() == IfSubtype) && (IfNotGenotype == null || The.Player.GetGenotype() != IfNotGenotype) && (IfNotSubtype == null || The.Player.GetSubtype() != IfNotSubtype) && (IfHaveObservation == null || JournalAPI.HasObservation(IfHaveObservation)) && (IfNotHaveObservation == null || !JournalAPI.HasObservation(IfNotHaveObservation)) && (IfHaveVillageNote == null || JournalAPI.HasVillageNote(IfHaveVillageNote)) && (IfNotFinishedQuestStep == null || !The.Game.FinishedQuestStep(IfNotFinishedQuestStep)) && (IfFinishedQuestStep == null || The.Game.FinishedQuestStep(IfFinishedQuestStep)) && (IfFinishedQuest == null || The.Game.FinishedQuest(IfFinishedQuest)) && (IfHasBlueprint == null || The.Player.GetPart<Inventory>().FireEvent(Event.New("HasBlueprint", "Blueprint", IfHasBlueprint))) && (IfWearingBlueprint == null || The.Player.HasObjectEquipped(IfWearingBlueprint)) && TestSpecialRequirement(SpecialRequirement))
		{
			return true;
		}
		return false;
	}

	public bool TestFilter()
	{
		if (string.IsNullOrEmpty(Filter))
		{
			return true;
		}
		if (Filter == "IsMerchant")
		{
			return GritGateScripts.IsMerchant();
		}
		if (Filter == "IsSlynthMayor")
		{
			string propertyOrTag = Conversation.Speaker.GetPropertyOrTag("Mayor");
			if (propertyOrTag == null)
			{
				return false;
			}
			return The.Game.GetStringGameState("SlynthSettlementFaction") == propertyOrTag;
		}
		if (Filter == "HaveSlynthCandidates")
		{
			SlynthQuestSystem system = The.Game.GetSystem<SlynthQuestSystem>();
			if (system == null)
			{
				return false;
			}
			return system.candidateFactionsCount() >= 3;
		}
		return true;
	}

	public virtual ConversationNode Goto(GameObject Speaker, bool peekOnly = false, Conversation conversation = null)
	{
		if (GotoID == "*trade")
		{
			return null;
		}
		if (GotoID == "*askname")
		{
			return null;
		}
		if (GotoID.StartsWith("*"))
		{
			eGetConversationNode.SetParameter("Speaker", Speaker);
			eGetConversationNode.SetParameter("GotoID", GotoID);
			eGetConversationNode.SetParameter("ConversationNode", null);
			Speaker?.FireEvent(eGetConversationNode);
			The.Game.FireSystemsEvent(eGetConversationNode);
			ConversationNode conversationNode = eGetConversationNode.GetParameter("ConversationNode") as ConversationNode;
			eGetConversationNode.SetParameter("Speaker", (object)null);
			eGetConversationNode.SetParameter("ConversationNode", (object)null);
			eGetConversationNode.SetParameter("GotoID", null);
			if (conversationNode != null)
			{
				eGetConversationNode.SetParameter("ConversationNode", null);
				return conversationNode;
			}
		}
		if (GotoID == "End")
		{
			return null;
		}
		if (GotoID == "EndFight")
		{
			return null;
		}
		try
		{
			if (ParentNode.ParentConversation == null)
			{
				return conversation.NodesByID[GotoID];
			}
			return ParentNode.ParentConversation.NodesByID[GotoID];
		}
		catch
		{
			Debug.LogError("Bad node ID: " + GotoID);
			throw;
		}
	}

	public virtual string GetDisplayText()
	{
		string text = "";
		if (CompleteQuestStep != null)
		{
			text += " {{W|[Complete Quest Step]}}";
		}
		if (StartQuest != null)
		{
			if (!string.IsNullOrEmpty(AcceptQuestText))
			{
				text = text + "\n{{W|[" + AcceptQuestText + "]}}";
			}
			else
			{
				string text2 = " {{W|[Accept Quest]}}";
				if (QuestLoader.Loader.QuestsByID.ContainsKey(StartQuest))
				{
					Quest quest = QuestLoader.Loader.QuestsByID[StartQuest];
					if (quest != null && !string.IsNullOrEmpty(quest.BonusAtLevel))
					{
						text2 = " {{W|[Accept Quest - level-based reward]}}";
					}
				}
				text += text2;
			}
		}
		if (GotoID == "End")
		{
			text += " {{K|[End]}}";
		}
		if (GotoID == "EndFight")
		{
			text += " {{R|[Fight]}}";
		}
		if (SpecialRequirement == "PaxInfection")
		{
			text = " {{g|[select limb to infect]}}";
		}
		if (SpecialRequirement == "GiveArtifact")
		{
			text = " {{g|[Give Artifact]}}";
		}
		if (SpecialRequirement == "GiveBook")
		{
			text = " {{g|[Give Book]}}";
		}
		if (SpecialRequirement == "*CrossIntoBrightsheol")
		{
			text = " {{M|[lesser victory]}}";
		}
		if (SpecialRequirement == "GiveReshephSecret")
		{
			text = " {{g|[Share a secret from Resheph's life]}}";
		}
		if (SpecialRequirement == "StartDance")
		{
			text = " {{g|[Begin Dance]}}";
		}
		return Text + text;
	}

	public virtual bool CheckSpecialRequirements(GameObject Speaker, GameObject Player, out bool removeChoice, out bool terminateConversation)
	{
		removeChoice = false;
		terminateConversation = false;
		if (SpecialRequirement == "StartDance")
		{
			if (!Player.HasPart("PlayerDanceRitual"))
			{
				Player.AddPart(new PlayerDanceRitual());
				Popup.Show("THE DANCE BEGINS!");
			}
		}
		else if (SpecialRequirement == "StartAngorNegotiation")
		{
			if (Speaker != null)
			{
				Speaker.RequirePart<SpindleNegotiation>();
				Speaker.FireEvent("BeginSpindleNegotiation");
			}
		}
		else if (SpecialRequirement == "GiveArtifact")
		{
			List<GameObject> list = new List<GameObject>();
			List<string> list2 = new List<string>();
			List<char> list3 = new List<char>();
			char c = 'a';
			foreach (GameObject item in Player.GetInventory())
			{
				if (item.HasPart("Examiner") && item.HasPart("TinkerItem") && item.GetPart<Examiner>().Complexity > 0)
				{
					list.Add(item);
					list3.Add(c);
					list2.Add(item.DisplayName);
					c = (char)(c + 1);
				}
			}
			if (list.Count == 0)
			{
				Popup.Show("You have no artifacts to give.");
				return false;
			}
			int num = Popup.ShowOptionList("", list2.ToArray(), list3.ToArray(), 0, "Select an artifact to give.", 60, RespectOptionNewlines: false, AllowEscape: true);
			if (num < 0)
			{
				return false;
			}
			list[num].SplitStack(1, The.Player);
			if (!Player.FireEvent(Event.New("CommandRemoveObject", "Object", list[num])))
			{
				Popup.Show("You can't give that object.");
				return false;
			}
		}
		else if (SpecialRequirement == "GiveReshephSecret")
		{
			List<JournalSultanNote> list4 = (from e in JournalAPI.GetKnownNotesForResheph()
				where !The.Game.StringGameState.ContainsKey("soldcultist_" + e.eventId)
				select e).ToList();
			List<JournalSultanNote> source = (from e in JournalAPI.GetKnownNotesForResheph()
				where The.Game.StringGameState.ContainsKey("soldcultist_" + e.eventId)
				select e).ToList();
			if (list4.Count == 0)
			{
				Popup.Show("You do not have any unshared secrets about the life of Resheph.");
			}
			else
			{
				string[] options = list4.Select((JournalSultanNote g) => g.text).ToArray();
				int num2 = Popup.ShowOptionList("Choose a secret about the life of Resheph to share.", options, null, 1, null, 60, RespectOptionNewlines: false, AllowEscape: true);
				if (num2 >= 0)
				{
					The.Game.SetStringGameState("soldcultist_" + list4[num2].eventId, "1");
					Popup.Show("You muse over the secret with Tszappur and gain some insight.");
					int reshephGospelXP = QudHistoryHelpers.GetReshephGospelXP(1 + source.Count());
					Popup.Show("You gain {{C|" + reshephGospelXP + "}} XP.", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
					The.Player.AwardXP(reshephGospelXP, -1, 0, int.MaxValue, null, Speaker);
				}
			}
		}
		else if (SpecialRequirement == "*CrossIntoBrightsheol")
		{
			if (ThinWorld.CrossIntoBrightsheol())
			{
				terminateConversation = true;
			}
		}
		else if (SpecialRequirement == "GiveBook")
		{
			Inventory inventory = Player.Inventory;
			List<string> list5 = new List<string>();
			List<GameObject> list6 = new List<GameObject>();
			List<char> list7 = new List<char>();
			char c2 = 'a';
			foreach (GameObject @object in inventory.GetObjects())
			{
				if ((@object.HasPart("Book") || @object.HasPart("VillageHistoryBook") || @object.HasPart("MarkovBook") || @object.HasPart("Cookbook")) && !@object.HasIntProperty("LibrarianAwarded"))
				{
					double valueEach = @object.ValueEach;
					double num3 = Math.Floor(valueEach * valueEach / 25.0);
					list6.Add(@object);
					list7.Add((c2 <= 'z') ? c2++ : ' ');
					list5.Add(@object.DisplayName + " [{{C|" + num3 + "}} XP]\n");
				}
			}
			if (list6.Count == 0)
			{
				Popup.Show("You have no books to give.");
				return false;
			}
			int defaultSelected = Math.Min(list5.Count - 1, Math.Max(0, lastChoice));
			int num4 = Popup.ShowOptionList("Choose a book to give.", list5.ToArray(), list7.ToArray(), 0, null, 60, RespectOptionNewlines: false, AllowEscape: true, defaultSelected);
			if (num4 < 0)
			{
				return false;
			}
			lastChoice = num4;
			GameObject gameObject = list6[num4];
			if (gameObject.IsImportant() && Popup.ShowYesNo(gameObject.The + gameObject.ShortDisplayName + gameObject.Is + " important. Are you sure you want to donate " + gameObject.them + "?") != 0)
			{
				return false;
			}
			gameObject.SplitStack(1, The.Player);
			if (!inventory.FireEvent(Event.New("CommandRemoveObject", "Object", gameObject)))
			{
				Popup.Show("You can't give that to me.");
				return false;
			}
			Commerce commerce = gameObject.GetPart("Commerce") as Commerce;
			int amount = (int)Math.Floor(commerce.Value * commerce.Value / 25.0);
			Popup.Show("Sheba Hagadias provides some insightful commentary on '" + gameObject.DisplayName + "'.");
			Popup.Show("You gain {{C|" + amount + "}} XP.", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
			The.Player.AwardXP(amount, -1, 0, int.MaxValue, null, Speaker);
			JournalAPI.AddAccomplishment("Sheba Hagadias provided you with insightful commentary on " + gameObject.DisplayName + ".", "Remember the kindness of =name=, who patiently taught " + gameObject.DisplayName + " to " + The.Player.GetPronounProvider().PossessiveAdjective + " simple pupil, Sheba Hagadias.", "general", JournalAccomplishment.MuralCategory.LearnsSecret, JournalAccomplishment.MuralWeight.Low, null, -1L);
			if (Speaker != null)
			{
				Speaker.ReceiveObject(gameObject);
			}
			else
			{
				gameObject.Destroy();
			}
			gameObject.SetIntProperty("LibrarianAwarded", 1);
		}
		else if (SpecialRequirement == "PaxInfection")
		{
			try
			{
				List<BodyPart> list8 = new List<BodyPart>();
				List<string> list9 = new List<string>();
				List<BodyPart> parts = Player.Body.GetParts();
				foreach (BodyPart item2 in parts)
				{
					if (FungalSporeInfection.BodyPartPreferableForFungalInfection(item2))
					{
						list8.Add(item2);
						list9.Add(item2.GetOrdinalName());
					}
				}
				foreach (BodyPart item3 in parts)
				{
					if (!list8.Contains(item3) && FungalSporeInfection.BodyPartSuitableForFungalInfection(item3))
					{
						list8.Add(item3);
						list9.Add(item3.GetOrdinalName());
					}
				}
				if (list8.Count == 0)
				{
					Popup.Show("You have no infectable body parts.");
					return false;
				}
				int num5 = Popup.ShowOptionList("", list9.ToArray(), null, 1, "Choose a limb to infect with Klanq.", 75, RespectOptionNewlines: false, AllowEscape: true);
				if (num5 < 0)
				{
					return false;
				}
				if (list8[num5].Equipped == null || list8[num5].TryUnequip())
				{
					GameObject gameObject2 = GameObjectFactory.Factory.CreateObject("PaxInfection");
					if (list8[num5].SupportsDependent != null)
					{
						foreach (BodyPart part2 in Player.Body.GetParts())
						{
							if (part2 != list8[num5] && part2.DependsOn == list8[num5].SupportsDependent && FungalSporeInfection.BodyPartSuitableForFungalInfection(part2))
							{
								gameObject2.UsesSlots = list8[num5].Type + "," + part2.Type;
								break;
							}
						}
					}
					if (list8[num5].Type == "Hand")
					{
						MeleeWeapon part = gameObject2.GetPart<MeleeWeapon>();
						part.BaseDamage = "1d4";
						part.Skill = "Cudgel";
						part.PenBonus = 0;
						part.MaxStrengthBonus = 4;
					}
					if (list8[num5].Type == "Body")
					{
						gameObject2.GetPart<Armor>().AV = 3;
					}
					else
					{
						gameObject2.GetPart<Armor>().AV = 1;
					}
					if (list8[num5].Equip(gameObject2, null, Silent: true) && Player.IsPlayer())
					{
						JournalAPI.AddAccomplishment("You contracted " + gameObject2.DisplayNameOnly + " on your " + list8[num5].GetOrdinalName() + ", endearing " + Player.itself + " to fungi across Qud.", "In a show of unprecedented solidarity with fungi, =name= deigned to contract " + gameObject2.DisplayNameOnly + " on " + The.Player.GetPronounProvider().PossessiveAdjective + " " + list8[num5].GetOrdinalName() + ".", "general", JournalAccomplishment.MuralCategory.BodyExperienceNeutral, JournalAccomplishment.MuralWeight.Medium, null, -1L);
						Popup.Show("You've contracted " + gameObject2.DisplayNameOnly + " on your " + list8[num5].GetOrdinalName() + ".");
					}
				}
				if (list8[num5].Equipped?.Blueprint != "PaxInfection")
				{
					Popup.Show("The limb rejects the infection!");
					return false;
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("PaxInfection", x);
				return false;
			}
		}
		return true;
	}

	public virtual bool Visit(GameObject Speaker, GameObject Player, out bool removeChoice, out bool terminateConversation)
	{
		if (!CheckSpecialRequirements(Speaker, Player, out removeChoice, out terminateConversation))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(ID) && !string.IsNullOrEmpty(ParentNode?.ID))
		{
			ConversationNode.VisitedNodes[ParentNode.ID + ID] = true;
		}
		if (SetStringState != null)
		{
			try
			{
				string[] array = SetStringState.Split(commaSplitter, 2);
				if (array.Length > 1)
				{
					The.Game.SetStringGameState(array[0], array[1]);
				}
				else
				{
					The.Game.RemoveStringGameState(array[0]);
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("SetStringState: " + SetStringState, x);
			}
		}
		if (SetIntState != null)
		{
			try
			{
				string[] array2 = SetIntState.Split(commaSplitter, 2);
				if (array2.Length > 1)
				{
					The.Game.SetIntGameState(array2[0], Convert.ToInt32(array2[1]));
				}
				else
				{
					The.Game.RemoveIntGameState(array2[0]);
				}
			}
			catch (Exception x2)
			{
				MetricsManager.LogException("SetIntState: " + SetIntState, x2);
			}
		}
		if (AddIntState != null)
		{
			try
			{
				string[] array3 = AddIntState.Split(commaSplitter, 2);
				if (array3.Length > 1)
				{
					The.Game.SetIntGameState(array3[0], The.Game.GetIntGameState(array3[0]) + Convert.ToInt32(array3[1]));
				}
			}
			catch (Exception x3)
			{
				MetricsManager.LogException("AddIntState: " + AddIntState, x3);
			}
		}
		if (SetBooleanState != null)
		{
			try
			{
				string[] array4 = SetBooleanState.Split(commaSplitter, 2);
				if (array4.Length > 1)
				{
					The.Game.SetBooleanGameState(array4[0], Convert.ToBoolean(array4[1]));
				}
				else
				{
					The.Game.RemoveBooleanGameState(array4[0]);
				}
			}
			catch (Exception x4)
			{
				MetricsManager.LogException("SetBooleanState: " + SetBooleanState, x4);
			}
		}
		if (ToggleBooleanState != null)
		{
			try
			{
				The.Game.SetBooleanGameState(ToggleBooleanState, !The.Game.GetBooleanGameState(ToggleBooleanState));
			}
			catch (Exception x5)
			{
				MetricsManager.LogException("ToggleBooleanState: " + ToggleBooleanState, x5);
			}
		}
		if (CallScript != null)
		{
			string typeID = CallScript.Substring(0, CallScript.LastIndexOf('.'));
			string name = CallScript.Substring(CallScript.LastIndexOf('.') + 1);
			ModManager.ResolveType(typeID).GetMethod(name).Invoke(null, null);
		}
		if (TakeItem != null)
		{
			string takeItem = TakeItem;
			List<string> list = takeItem.CachedCommaExpansion();
			List<GameObject> list2 = Event.NewGameObjectList();
			List<GameObject> contents = Player.GetContents();
			bool flag = takeItem.Contains("[byid]");
			foreach (string item in list)
			{
				if (item.StartsWith("["))
				{
					continue;
				}
				int i = 0;
				for (int num = contents.Count; i < num; i++)
				{
					GameObject gameObject = contents[i];
					if (gameObject == null || !(flag ? gameObject.idmatch(item) : (gameObject.Blueprint == item)) || list2.Contains(gameObject))
					{
						continue;
					}
					if (!gameObject.TryRemoveFromContext())
					{
						Popup.Show("You cannot give that object!");
						list2.Add(gameObject);
						continue;
					}
					if (!takeItem.Contains("[destroy]"))
					{
						if (!Speaker.ReceiveObject(gameObject))
						{
							Popup.Show("You cannot give that object!");
							Player.ReceiveObject(gameObject);
						}
						else
						{
							Popup.Show(Speaker.The + Speaker.ShortDisplayNameStripped + Speaker.GetVerb("take") + " " + gameObject.the + gameObject.ShortDisplayName + ".");
							if (!takeItem.Contains("[allowsell]"))
							{
								gameObject.SetIntProperty("WontSell", 1);
							}
							if (takeItem.Contains("[restorecategory]"))
							{
								string category = gameObject.GetStringProperty("OriginalCategory") ?? gameObject.GetBlueprint().GetPartParameter("Physics", "Category") ?? "Miscellaneous";
								gameObject.pPhysics.Category = category;
							}
							if (takeItem.Contains("[removequestitem]"))
							{
								gameObject.RemoveProperty("QuestItem");
							}
							if (takeItem.Contains("[removenoaiequip]"))
							{
								gameObject.RemoveProperty("NoAIEquip");
							}
						}
					}
					if (takeItem.Contains("[takeall]"))
					{
						if (contents.Count < num)
						{
							int num2 = num - contents.Count;
							i -= num2;
							num -= num2;
						}
						continue;
					}
					goto end_IL_04e1;
				}
				continue;
				end_IL_04e1:
				break;
			}
		}
		if (!string.IsNullOrEmpty(Achievement))
		{
			AchievementManager.SetAchievement(Achievement);
		}
		if (GiveOneItem != null)
		{
			string[] array5 = GiveOneItem.Split(',');
			List<GameObject> list3 = new List<GameObject>();
			List<string> list4 = new List<string>();
			List<char> list5 = new List<char>();
			char c = 'a';
			string[] array6 = array5;
			for (int j = 0; j < array6.Length; j++)
			{
				GameObject gameObject2 = GameObject.create(array6[j]);
				gameObject2.SetEpistemicStatus(2);
				list3.Add(gameObject2);
				list4.Add(gameObject2.DisplayName);
				list5.Add(c);
				c = (char)(c + 1);
			}
			int index = Popup.ShowOptionList("Choose a reward", list4.ToArray(), list5.ToArray(), 1);
			Player.TakeObject(list3[index], Silent: false, 0);
		}
		if (!string.IsNullOrEmpty(Execute))
		{
			try
			{
				string[] array7 = Execute.Split(':');
				if (!(bool)ModManager.ResolveType(array7[0]).GetMethod(array7[1]).Invoke(null, null))
				{
					return false;
				}
			}
			catch (Exception x6)
			{
				MetricsManager.LogException("error executing Execute command (you probably forgot to have your static return a bool instead of void):" + Execute, x6);
			}
		}
		if (GiveItem != null && !The.Game.Player.ConversationItemsGiven.ContainsKey(ParentNode.ID))
		{
			The.Game.Player.ConversationItemsGiven.Add(ParentNode.ID, value: true);
			List<string> list6 = new List<string>();
			string[] array6 = GiveItem.Split(',');
			for (int j = 0; j < array6.Length; j++)
			{
				GameObject gameObject3 = GameObject.create(array6[j]);
				if (IdGift != null && IdGift.Contains(gameObject3.Blueprint, CompareOptions.IgnoreCase))
				{
					gameObject3.MakeUnderstood();
				}
				list6.Add(gameObject3.a + gameObject3.ShortDisplayName);
				Player.ReceiveObject(gameObject3);
			}
			if (list6.Count > 0)
			{
				Popup.Show("You receive " + Grammar.MakeAndList(list6) + "!");
			}
		}
		if (StartQuest != null)
		{
			The.Game.StartQuest(StartQuest);
		}
		if (CompleteQuestStep != null)
		{
			string[] array6 = CompleteQuestStep.Split(';');
			for (int j = 0; j < array6.Length; j++)
			{
				string[] array8 = array6[j].Split('~');
				string text = array8[1];
				int num3 = -1;
				if (text.Contains("|"))
				{
					string[] array9 = text.Split('|');
					text = array9[0];
					num3 = Convert.ToInt32(array9[1]);
				}
				if (!The.Game.HasQuest(array8[0]))
				{
					continue;
				}
				Quest quest = The.Game.Quests[array8[0]];
				QuestStep value = null;
				if (!quest.StepsByID.TryGetValue(text, out value))
				{
					MetricsManager.LogError("Error completing quest step '" + text + "' step not found.");
				}
				if (value != null)
				{
					if (num3 == -1)
					{
						The.Game.FinishQuestStep(array8[0], text);
					}
					else
					{
						The.Game.FinishQuestStepXP(array8[0], text, num3);
					}
				}
			}
		}
		if (FinishQuest != null)
		{
			The.Game.FinishQuest(FinishQuest);
		}
		if (RevealMapNoteId != null)
		{
			JournalMapNote mapNote = JournalAPI.GetMapNote(RevealMapNoteId);
			if (Speaker != null && Speaker.pBrain != null)
			{
				string primaryFaction = Speaker.pBrain.GetPrimaryFaction();
				if (!string.IsNullOrEmpty(primaryFaction))
				{
					Faction faction = Factions.get(primaryFaction);
					mapNote.attributes.Add(faction.NoBuySecretString);
					if (faction.Visible)
					{
						if (mapNote.history.Length > 0)
						{
							mapNote.history += "\n";
						}
						mapNote.history = mapNote.history + " {{K|-learned from " + faction.getFormattedName() + "}}";
					}
				}
			}
			JournalAPI.RevealMapNote(mapNote);
		}
		if (RevealObservation != null)
		{
			JournalObservation observation = JournalAPI.GetObservation(RevealObservation);
			if (observation != null && Speaker != null && Speaker.pBrain != null)
			{
				string primaryFaction2 = Speaker.pBrain.GetPrimaryFaction();
				if (!string.IsNullOrEmpty(primaryFaction2))
				{
					Faction faction2 = Factions.get(primaryFaction2);
					observation.attributes.Add(faction2.NoBuySecretString);
					if (faction2.Visible)
					{
						if (observation.history.Length > 0)
						{
							observation.history += "\n";
						}
						observation.history = observation.history + " {{K|-learned from " + faction2.getFormattedName() + "}}";
					}
				}
				JournalAPI.RevealObservation(observation);
			}
			else
			{
				MetricsManager.LogError("unknown observation " + RevealObservation);
			}
		}
		if (GotoID == "EndFight")
		{
			if (Speaker.pBrain.PartyLeader == Player)
			{
				Speaker.pBrain.PartyLeader = null;
			}
			Speaker.pBrain.SetFeeling(Player, -100);
			Speaker.pBrain.Target = Player;
		}
		if (!string.IsNullOrEmpty(ClearOwner))
		{
			Player.CurrentZone.ForeachCell(delegate(Cell C)
			{
				foreach (GameObject @object in C.Objects)
				{
					if (@object.HasTagOrProperty(ClearOwner))
					{
						@object.pPhysics.Owner = null;
					}
				}
			});
		}
		if (onAction != null && !onAction())
		{
			return false;
		}
		if (GotoID == "*trade")
		{
			TradeUI.ShowTradeScreen(Speaker);
			return false;
		}
		if (GotoID == "*askname")
		{
			if (!Speaker.HasProperName)
			{
				Speaker.DisplayName = NameMaker.MakeName(Speaker);
				Speaker.HasProperName = true;
			}
			GotoID = "TellName";
			removeChoice = true;
		}
		return true;
	}

	public int CompareTo(ConversationChoice other)
	{
		int num = ((GotoID == "End") ? 999999 : Ordinal);
		int value = ((other.GotoID == "End") ? 999999 : other.Ordinal);
		int num2 = num.CompareTo(value);
		if (num2 == 0)
		{
			return string.Compare(Text, other.Text, StringComparison.Ordinal);
		}
		return num2;
	}
}

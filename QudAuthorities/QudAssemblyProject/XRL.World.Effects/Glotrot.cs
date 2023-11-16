using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Conversations.Parts;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Glotrot : Effect
{
	public int Stage = 1;

	public int Count;

	public int DrankIck;

	public Glotrot()
	{
		base.DisplayName = "glotrot";
	}

	public override string GetDetails()
	{
		if (Stage < 3)
		{
			return "Tongue is rotting away.\nStarts bleeding when eating or drinking.";
		}
		return "Tongue has rotted away.\nCan't speak.";
	}

	public override int GetEffectType()
	{
		return 100679700;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Glotrot") || Object.HasEffect("GlotrotOnset"))
		{
			return false;
		}
		if (Object.FireEvent("ApplyDisease") && ApplyEffectEvent.Check(Object, "Disease", this) && Object.FireEvent("ApplyGlotrot") && ApplyEffectEvent.Check(Object, "Glotrot", this))
		{
			if (Object.IsPlayer())
			{
				AchievementManager.SetAchievement("ACH_GET_GLOTROT");
				Popup.Show("You have contracted glotrot! Your tongue begins to bleed as the muscle rots away.");
				JournalAPI.AddAccomplishment("You contracted glotrot.", "Woe to the scroundrels and dastards who conspired to have =name= contract the rotting tongue!", "general", JournalAccomplishment.MuralCategory.BodyExperienceBad, JournalAccomplishment.MuralWeight.Medium, null, -1L);
				Object.ApplyEffect(new Bleeding("1", 25, Object));
				AskPulldown();
			}
			base.Duration = 1;
			Stage = 1;
			return true;
		}
		return false;
	}

	public override string GetDescription()
	{
		if (DrankIck > 0)
		{
			return null;
		}
		return "{{glotrot|glotrot}}";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID && ID != GenericNotifyEvent.ID && ID != GenericQueryEvent.ID && ID != GetTradePerformanceEvent.ID)
		{
			return ID == BeginConversationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GenericQueryEvent E)
	{
		if (E.Query == "AnyRegenerableLimbs" && (Stage >= 3 || DrankIck > 0))
		{
			E.Result = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		AdvanceGlotrot(1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericNotifyEvent E)
	{
		if (E.Notify != null && E.Notify.Contains("Regenerate") && E.Notify.Contains("Limb") && DrankIck > 0)
		{
			RegrowTongue();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTradePerformanceEvent E)
	{
		E.LinearAdjustment -= 3.0;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginConversationEvent E)
	{
		if (base.Object.IsPlayer() && Stage >= 3 && !base.Object.HasPart("Telepathy"))
		{
			ConversationUI.CurrentDialogue?.AddPart(new GlotrotFilter
			{
				Propagation = 1
			});
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDrank");
		Object.RegisterEffectEvent(this, "DrinkingFrom");
		Object.RegisterEffectEvent(this, "Eating");
		Object.RegisterEffectEvent(this, "EndTurn");
		Object.RegisterEffectEvent(this, "Regenera");
		Object.RegisterEffectEvent(this, "ShowConversationChoices");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDrank");
		Object.UnregisterEffectEvent(this, "DrinkingFrom");
		Object.UnregisterEffectEvent(this, "Eating");
		Object.UnregisterEffectEvent(this, "EndTurn");
		Object.UnregisterEffectEvent(this, "Regenera");
		Object.UnregisterEffectEvent(this, "ShowConversationChoices");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Regenera" && E.GetIntParameter("Level") >= 5 && (!E.HasIntParameter("Involuntary") || DrankIck > 0))
		{
			RegrowTongue();
		}
		else if (E.ID == "ShowConversationChoices")
		{
			if (!base.Object.HasPart("Telepathy") && Stage >= 3 && base.Object.IsPlayer())
			{
				List<ConversationChoice> list = new List<ConversationChoice>();
				foreach (ConversationChoice item in E.GetParameter<List<ConversationChoice>>("Choices"))
				{
					if (item.GotoID == "*trade")
					{
						list.Add(item);
					}
				}
				E.SetParameter("Choices", list);
				ConversationChoice conversationChoice = new ConversationChoice();
				conversationChoice.GotoID = "End";
				conversationChoice.Text = "Nnnnnnnnnnnnnnn.";
				list.Add(conversationChoice);
				return false;
			}
		}
		else if (E.ID == "AfterDrank" || E.ID == "Eating")
		{
			if (Stage < 3 && !E.HasFlag("External"))
			{
				if (base.Object.IsPlayer())
				{
					Popup.ShowBlock("You tear open the tender muscle fibers of your tongue.");
				}
				base.Object.ApplyEffect(new Bleeding("1", 25, base.Object));
				AskPulldown();
			}
		}
		else if (E.ID == "DrinkingFrom")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Container");
			LiquidVolume liquidVolume = gameObjectParameter.LiquidVolume;
			if (gameObjectParameter.IsAflame())
			{
				int num = 1;
				while (true)
				{
					if (num <= 3)
					{
						string stringGameState = XRLCore.Core.Game.GetStringGameState("GlotrotCure" + num);
						if (!liquidVolume.ComponentLiquids.ContainsKey(stringGameState) || (num == 3 && liquidVolume.GetPrimaryLiquidID() != stringGameState))
						{
							break;
						}
						num++;
						continue;
					}
					DrankIck = 100;
					if (base.Object.IsPlayer())
					{
						Popup.Show("It tastes even worse than you had imagined -- like a dead turtle boiled in phlegm.");
					}
					break;
				}
			}
			else if (Stat.Random(1, 100) <= 25)
			{
				LiquidVolume liquidVolume2 = GameObject.create("Water").LiquidVolume;
				liquidVolume2.InitialLiquid = "putrid-1000";
				int num2 = Math.Min(4, liquidVolume.Volume);
				if (num2 > 0)
				{
					liquidVolume.UseDrams(num2);
				}
				liquidVolume2.Volume = Math.Max(num2, 1);
				liquidVolume.MixWith(liquidVolume2);
			}
		}
		return base.FireEvent(E);
	}

	public void RegrowTongue()
	{
		if (base.Duration <= 0)
		{
			return;
		}
		Stage = 1;
		if (DrankIck > 0)
		{
			if (base.Object.IsPlayer())
			{
				AchievementManager.SetAchievement("ACH_CURE_GLOTROT");
				Popup.ShowBlock("You are cured of glotrot. Your tongue regrows.");
				JournalAPI.AddAccomplishment("You were cured of glotrot and your tongue regrew.", "Blessed was the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", in the year of " + Calendar.getYear() + " AR, when =name= was cured of the rotting tongue!", "general", JournalAccomplishment.MuralCategory.BodyExperienceGood, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			}
			base.Duration = 0;
			Stage = 0;
		}
		else if (base.Object.IsPlayer())
		{
			Popup.Show("Your tongue regrows.");
			JournalAPI.AddAccomplishment("Your tongue regrew.", "Remember the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", in the year of " + Calendar.getYear() + " AR, when through sheer strength of will =name= regrew " + The.Player.GetPronounProvider().PossessiveAdjective + " tongue.", "general", JournalAccomplishment.MuralCategory.BodyExperienceGood, JournalAccomplishment.MuralWeight.Medium, null, -1L);
		}
	}

	public void AdvanceGlotrot(int Amount)
	{
		if (DrankIck > 0)
		{
			return;
		}
		Count += Amount;
		if (Count < 1200 || Stage >= 3)
		{
			return;
		}
		Stage++;
		if (Stage == 2)
		{
			if (base.Object.IsPlayer())
			{
				Popup.ShowBlock("Your tongue begins to bleed as the muscle rots away.");
			}
			base.Object.ApplyEffect(new Bleeding("1", 25, base.Object));
			AskPulldown();
		}
		if (Stage == 3)
		{
			if (base.Object.IsPlayer())
			{
				Popup.ShowBlock("Your tongue has rotted away.");
				JournalAPI.AddAccomplishment("Your tongue rotted away.", "On " + Calendar.getDay() + " of " + Calendar.getMonth() + ", in the year of " + Calendar.getYear() + " AR, =name= took a vow of silence and removed " + The.Player.GetPronounProvider().PossessiveAdjective + " own tongue.", "general", JournalAccomplishment.MuralCategory.BodyExperienceBad, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			}
			base.Object.ApplyEffect(new Bleeding("1", 25, base.Object));
			AskPulldown();
		}
	}

	public void AskPulldown()
	{
		if (base.Object.OnWorldMap() && Popup.ShowYesNo("Do you want to stop travelling?") == DialogResult.Yes)
		{
			base.Object.PullDown();
		}
	}
}

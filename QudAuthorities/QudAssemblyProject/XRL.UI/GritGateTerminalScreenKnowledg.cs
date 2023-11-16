using Qud.API;
using XRL.Core;
using XRL.Language;

namespace XRL.UI;

public class GritGateTerminalScreenKnowledge : GritGateTerminalScreen
{
	private bool bSecretRevealed
	{
		get
		{
			return XRLCore.Core.Game.HasIntGameState("EreshkigalSecret");
		}
		set
		{
			if (value)
			{
				XRLCore.Core.Game.SetIntGameState("EreshkigalSecret", 1);
			}
			else
			{
				XRLCore.Core.Game.IntGameState.Remove("EreshkigalSecret");
			}
		}
	}

	public GritGateTerminalScreenKnowledge()
	{
		if (bSecretRevealed)
		{
			mainText = "I've done as you asked. Your repetition is ungainly.";
			Options.Add("Are you sapient?");
			Options.Add("What is the Thin World?");
			Options.Add("Exit.");
			return;
		}
		mainText = "It is done.";
		Options.Add("Thank you, Ereshkigal.");
		optionActions.Add(delegate
		{
			bSecretRevealed = true;
			IBaseJournalEntry randomUnrevealedNote = JournalAPI.GetRandomUnrevealedNote();
			JournalMapNote obj = randomUnrevealedNote as JournalMapNote;
			string text = "";
			text = ((obj == null) ? randomUnrevealedNote.text : ("The location of " + Grammar.InitLowerIfArticle(randomUnrevealedNote.text)));
			Popup.Show("Ereshkigal delivers insight from the Thin World:\n\n" + text);
			randomUnrevealedNote.Reveal();
			base.terminal.currentScreen = null;
		});
	}

	public override void Back()
	{
		base.terminal.currentScreen = null;
	}

	public override void Activate()
	{
		if (bSecretRevealed)
		{
			if (base.terminal.nSelected == 0)
			{
				base.terminal.currentScreen = new GritGateTerminalScreenWhoAreYou();
			}
			if (base.terminal.nSelected == 1)
			{
				base.terminal.currentScreen = new GritGateTerminalScreenThinWorld();
			}
			if (base.terminal.nSelected == 2)
			{
				base.terminal.currentScreen = null;
			}
		}
		else
		{
			optionActions[base.terminal.nSelected]();
		}
	}
}

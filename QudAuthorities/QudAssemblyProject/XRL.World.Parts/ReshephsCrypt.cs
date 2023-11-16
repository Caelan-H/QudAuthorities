using System;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class ReshephsCrypt : IPart
{
	public string Inscription = "";

	public string Prefix = "";

	public string Postfix = "";

	public bool bFirst = true;

	public string faction;

	public string name;

	public bool transitallowed;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "BeforeClosing");
		Object.RegisterPartEvent(this, "SyncClosed");
		Object.RegisterPartEvent(this, "ObjectEntered");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && ParentObject.pPhysics.CurrentCell.ParentZone.FindObject("Biographer") == null && ParentObject.HasIntProperty("Sealed"))
		{
			IComponent<GameObject>.XDidY(ParentObject, "open");
			ParentObject.RemoveIntProperty("Sealed");
			ParentObject.FireEvent("SyncOpened");
			foreach (GameObject item in ParentObject.pPhysics.CurrentCell.ParentZone.GetObjectsWithPart("ReshephsCrypt"))
			{
				if (item != ParentObject)
				{
					item.RemoveIntProperty("Sealed");
					item.FireEvent("SyncOpened");
				}
			}
		}
		if (E.ID == "SyncClosed" && transitallowed)
		{
			transitallowed = false;
			GameObject crypt = null;
			foreach (GameObject item2 in ParentObject.pPhysics.currentCell.ParentZone.GetObjectsWithPart("ReshephsCrypt"))
			{
				item2.SetStringProperty("EntombTriggered", "1");
				item2.SetIntProperty("Sealed", 1);
				item2.FireEvent("SyncClosed");
				crypt = item2;
			}
			GameManager.Instance.gameQueue.queueTask(delegate
			{
				ThinWorld.TransitToThinWorld(crypt);
			});
		}
		if (E.ID == "BeforeClosing")
		{
			if (ParentObject.HasStringProperty("EntombTriggered"))
			{
				Popup.Show("The sarcophagus is inert.");
				transitallowed = false;
				return true;
			}
			if (!(Popup.AskString("The sealing mechanisms inside this sarcophagus will certainly kill you if you close " + IComponent<GameObject>.ThePlayer.itself + " inside. Are you sure you want to enter the sarcophagus? (type ENTOMB to confirm)", "", 6) == "ENTOMB"))
			{
				transitallowed = false;
				return false;
			}
			Popup.Show("You climb into the sarcophagus.");
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			Cell cell = IComponent<GameObject>.ThePlayer.GetCurrentCell();
			cell.RemoveObject(IComponent<GameObject>.ThePlayer);
			cell.ParentZone.LightAll();
			cell.ParentZone.VisAll();
			cell.ParentZone.Render(scrapBuffer);
			scrapBuffer.Draw();
			cell.AddObject(IComponent<GameObject>.ThePlayer);
			GameManager.Instance.ForceGameView();
			Thread.Sleep(1000);
			transitallowed = true;
		}
		if (E.ID == "ObjectEntered")
		{
			_ = transitallowed;
		}
		return base.FireEvent(E);
	}

	public static void AddMarkOfDeath()
	{
		XRLCore.Core.Game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
	}
}

using System;

namespace XRL.World.Parts;

[Serializable]
public class QuestStepFinisher : IPart
{
	public string IfFinishedQuestStep;

	public string Quest;

	public string Step;

	public string Trigger;

	public QuestStepFinisher()
	{
		Trigger = "Created";
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == BeforeRenderEvent.ID)
			{
				return Trigger == "Created";
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (Trigger == "Created" && ParentObject?.CurrentZone?.ZoneID == The.Player?.CurrentZone?.ZoneID && ParentObject?.CurrentZone?.ZoneID != null)
		{
			Activate();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		if (Trigger == "OnScreen")
		{
			Object.RegisterPartEvent(this, "EndTurn");
		}
		if (Trigger == "Taken")
		{
			Object.RegisterPartEvent(this, "GotNewQuest");
			Object.RegisterPartEvent(this, "Taken");
			Object.RegisterPartEvent(this, "Equipped");
		}
		base.Register(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (Trigger == "Seen")
		{
			Activate();
		}
		return base.Render(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (Trigger == "OnScreen")
			{
				Activate();
			}
		}
		else if ((E.ID == "Taken" || E.ID == "GotNewQuest" || E.ID == "Equipped") && Trigger == "Taken")
		{
			GameObject gameObject = E.GetGameObjectParameter("TakingObject") ?? E.GetGameObjectParameter("EquippingObject");
			if (gameObject != null && gameObject.IsPlayer())
			{
				Activate();
			}
		}
		return base.FireEvent(E);
	}

	public void Activate()
	{
		if (string.IsNullOrEmpty(IfFinishedQuestStep) || The.Game.FinishedQuestStep(IfFinishedQuestStep))
		{
			The.Game.FinishQuestStep(Quest, Step);
		}
	}
}

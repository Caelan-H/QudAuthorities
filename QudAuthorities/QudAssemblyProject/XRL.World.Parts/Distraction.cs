using System;

namespace XRL.World.Parts;

[Serializable]
public class Distraction : IPart
{
	public GameObject DistractionFor;

	public int Difficulty = 15;

	public GameObject DistractionGeneratedBy;

	public Distraction()
	{
	}

	public Distraction(GameObject DistractionFor, GameObject DistractionGeneratedBy = null, int Difficulty = 15)
		: this()
	{
		this.DistractionFor = DistractionFor;
		this.DistractionGeneratedBy = DistractionGeneratedBy;
		this.Difficulty = Difficulty;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && !ParentObject.IsNowhere() && !ParentObject.IsInGraveyard() && GameObject.validate(ref DistractionFor))
		{
			Event @event = Event.New("AIDistractionBroadcast");
			@event.SetParameter("OriginalTarget", DistractionFor);
			@event.SetParameter("DistractionTarget", ParentObject);
			@event.SetParameter("DistractionGeneratedBy", DistractionGeneratedBy);
			@event.SetParameter("Difficulty", Difficulty);
			foreach (GameObject item in ParentObject.CurrentZone.FastFloodVisibility(ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, 20, "Brain", ParentObject))
			{
				item.FireEvent(@event);
			}
		}
		return base.FireEvent(E);
	}
}

using System;

namespace XRL.World.Parts;

[Serializable]
public class LeaveTrailWhileHasEffect : IPart
{
	public string Effect = "Burrowed";

	public string TrailObject = "PlantWall";

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "LeavingCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LeavingCell")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null && ParentObject.HasEffect(Effect) && !ParentObject.OnWorldMap() && !cell.HasObjectWithBlueprint(TrailObject))
			{
				cell.AddObject(TrailObject);
			}
		}
		return base.FireEvent(E);
	}
}

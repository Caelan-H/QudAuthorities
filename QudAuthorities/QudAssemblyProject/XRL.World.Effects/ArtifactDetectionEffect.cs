using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class ArtifactDetectionEffect : Effect
{
	public int Radius = 1;

	public int IdentifyChance;

	public bool Identified;

	public GameObject Source;

	[NonSerialized]
	private ArtifactDetection Detector;

	[NonSerialized]
	private Examiner Examiner;

	[NonSerialized]
	private bool Initialized;

	public ArtifactDetectionEffect()
	{
		base.Duration = 1;
	}

	public ArtifactDetectionEffect(ArtifactDetection Detector, Examiner Examiner)
		: this()
	{
		this.Detector = Detector;
		this.Examiner = Examiner;
		Initialized = Examiner != null && Detector != null;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool SuppressInLookDisplay()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 64;
	}

	public override string GetDescription()
	{
		return null;
	}

	public bool Initialize()
	{
		if (Initialized)
		{
			return true;
		}
		Initialized = true;
		Detector = Source.GetPart("ArtifactDetection") as ArtifactDetection;
		Examiner = base.Object.GetPart("Examiner") as Examiner;
		if (Examiner != null)
		{
			return Detector != null;
		}
		return false;
	}

	public bool CheckDetection()
	{
		if (!GameObject.validate(ref Source) || base.Object.CurrentCell == null)
		{
			return Unapply();
		}
		if (!Initialize() || !Detector.WasReady())
		{
			return Unapply();
		}
		if (!Detector.DetectKnown && Examiner.GetEpistemicStatus() == 2)
		{
			return Unapply();
		}
		int num = base.Object.DistanceTo(The.Player);
		if (num > Radius)
		{
			return Unapply();
		}
		if (!Identified && IdentifyChance > 0 && ((int)((double)(100 + IdentifyChance) / Math.Pow(num + 9, 2.0) * 100.0)).in100())
		{
			Identified = true;
		}
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		if (!Initialize())
		{
			return false;
		}
		if (Examiner.Complexity < 0)
		{
			return false;
		}
		return CheckDetection();
	}

	private bool Unapply()
	{
		Source = null;
		base.Object.RemoveEffect(this);
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EndTurnEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		CheckDetection();
		return base.HandleEvent(E);
	}

	public bool ValidTarget(GameObject Object)
	{
		Cell cell = Object.CurrentCell;
		if (cell == null)
		{
			Unapply();
			return false;
		}
		if (!Object.IsVisible())
		{
			return true;
		}
		if (!cell.IsLit() || !cell.IsExplored())
		{
			return true;
		}
		return false;
	}

	public override bool FinalRender(RenderEvent E, bool Alt)
	{
		if (E.CustomDraw)
		{
			return true;
		}
		if (!ValidTarget(base.Object))
		{
			return true;
		}
		if (!Initialize())
		{
			Unapply();
			return true;
		}
		if (!Detector.WasReady())
		{
			Unapply();
			return true;
		}
		if (Identified)
		{
			E.HighestLayer = 0;
			E.RenderString = base.Object.pRender.RenderString;
			E.Tile = (Options.UseTiles ? base.Object.pRender.Tile : null);
			base.Object.Render(E);
		}
		else
		{
			Examiner.RenderUnknown(E);
		}
		E.CustomDraw = true;
		return false;
	}
}

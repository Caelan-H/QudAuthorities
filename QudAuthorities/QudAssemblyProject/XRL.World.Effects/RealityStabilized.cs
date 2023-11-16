using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class RealityStabilized : Effect
{
	public enum ContestResult
	{
		Abjection,
		Surrender,
		Failure,
		Success
	}

	public const int DEFAULT_VISIBILITY = 2;

	public int Strength;

	public int Visibility = 2;

	public bool Projective;

	public GameObject Owner;

	public bool ScanAdjacentCells;

	public int IndependentStrength;

	public int IndependentStrengthDecline;

	public int IndependentVisibility = 2;

	public bool IndependentlyProjective;

	private int FrameOffset;

	public RealityStabilized()
	{
		base.Duration = 1;
		base.DisplayName = "{{Y|astrally tethered}}";
	}

	public RealityStabilized(int Strength = 0, GameObject Owner = null, int Duration = 1, int IndependentStrength = 0, int IndependentStrengthDecline = 0, int IndependentVisibility = 2, bool IndependentlyProjective = false)
		: this()
	{
		this.Strength = Strength;
		this.Owner = Owner;
		base.Duration = Duration;
		this.IndependentStrength = IndependentStrength;
		this.IndependentStrengthDecline = IndependentStrengthDecline;
		this.IndependentVisibility = IndependentVisibility;
		this.IndependentlyProjective = IndependentlyProjective;
	}

	public override int GetEffectType()
	{
		return 64;
	}

	public override bool SameAs(Effect e)
	{
		RealityStabilized realityStabilized = e as RealityStabilized;
		if (realityStabilized.Strength != Strength)
		{
			return false;
		}
		if (realityStabilized.Visibility != Visibility)
		{
			return false;
		}
		if (realityStabilized.Projective != Projective)
		{
			return false;
		}
		if (realityStabilized.Owner != Owner)
		{
			return false;
		}
		if (realityStabilized.ScanAdjacentCells != ScanAdjacentCells)
		{
			return false;
		}
		if (realityStabilized.IndependentStrength != IndependentStrength)
		{
			return false;
		}
		if (realityStabilized.IndependentStrengthDecline != IndependentStrengthDecline)
		{
			return false;
		}
		if (realityStabilized.IndependentVisibility != IndependentVisibility)
		{
			return false;
		}
		if (realityStabilized.IndependentlyProjective != IndependentlyProjective)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDescription()
	{
		if (Visibility < 1)
		{
			return null;
		}
		if (Strength < 100)
		{
			return "{{y|astrally burdened}}";
		}
		return "{{Y|astrally tethered}}";
	}

	public override string GetDetails()
	{
		if (Strength < 100)
		{
			return "Has trouble distorting spacetime in the local region.";
		}
		return "Cannot distort spacetime in the local region.";
	}

	public override bool CanApplyToStack()
	{
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("RealityStabilized"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyRealityStabilized"))
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeBeginTakeActionEvent.ID)
		{
			return ID == EndTurnEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		if (base.Object?.pBrain != null)
		{
			if (Maintain())
			{
				Stabilize();
			}
			if (base.Duration > 1 && base.Duration != 9999)
			{
				base.Duration--;
				if (base.Duration <= 1 && Owner != null)
				{
					Owner = null;
				}
			}
			if (IndependentStrengthDecline != 0)
			{
				IndependentStrength = Math.Max(IndependentStrength - IndependentStrengthDecline, 0);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (base.Object?.pBrain == null)
		{
			if (Maintain())
			{
				Stabilize();
			}
			if (base.Duration > 1 && base.Duration != 9999)
			{
				base.Duration--;
				if (base.Duration <= 1 && Owner != null)
				{
					Owner = null;
				}
			}
			if (IndependentStrengthDecline != 0)
			{
				IndependentStrength = Math.Max(IndependentStrength - IndependentStrengthDecline, 0);
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "CheckRealityDistortionAccessibility");
		Object.RegisterEffectEvent(this, "CheckRealityDistortionAdvisability");
		Object.RegisterEffectEvent(this, "CheckRealityDistortionUsability");
		Object.RegisterEffectEvent(this, "EquipperUnequipped");
		Object.RegisterEffectEvent(this, "InitiateRealityDistortionLocal");
		Object.RegisterEffectEvent(this, "InitiateRealityDistortionRemote");
		Object.RegisterEffectEvent(this, "InitiateRealityDistortionTransit");
		Object.RegisterEffectEvent(this, "MaintainRealityStabilization");
		Object.RegisterEffectEvent(this, "ObjectEnteringCellBlockedBySolid");
		Object.RegisterEffectEvent(this, "SpaceTimeVortexContact");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "CheckRealityDistortionAccessibility");
		Object.UnregisterEffectEvent(this, "CheckRealityDistortionAdvisability");
		Object.UnregisterEffectEvent(this, "CheckRealityDistortionUsability");
		Object.UnregisterEffectEvent(this, "EquipperUnequipped");
		Object.UnregisterEffectEvent(this, "InitiateRealityDistortionLocal");
		Object.UnregisterEffectEvent(this, "InitiateRealityDistortionRemote");
		Object.UnregisterEffectEvent(this, "InitiateRealityDistortionTransit");
		Object.UnregisterEffectEvent(this, "MaintainRealityStabilization");
		Object.UnregisterEffectEvent(this, "ObjectEnteringCellBlockedBySolid");
		Object.UnregisterEffectEvent(this, "SpaceTimeVortexContact");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (Visibility > 1)
		{
			if (FrameOffset == 0)
			{
				FrameOffset = Stat.Random(1, 1000);
			}
			if ((XRLCore.CurrentFrameLong10 + FrameOffset) % 2000 >= 1000)
			{
				E.ColorString = ((Visibility > 2) ? "&y^Y" : "&y");
				E.DetailColor = "K";
			}
		}
		return true;
	}

	protected void ProcessGameObject(GameObject obj)
	{
		if (!(obj.GetPart("RealityStabilization") is RealityStabilization realityStabilization) || !realityStabilization.IsObjectActivePartSubject(base.Object))
		{
			return;
		}
		if (realityStabilization.WorksOnAdjacentCellContents && !ScanAdjacentCells && !obj.InSameCellAs(base.Object))
		{
			ScanAdjacentCells = true;
		}
		int effectiveStrength = realityStabilization.EffectiveStrength;
		if (effectiveStrength > 0)
		{
			if (effectiveStrength > Strength)
			{
				Strength = effectiveStrength;
			}
			int visibilityFor = realityStabilization.GetVisibilityFor(base.Object);
			if (visibilityFor > Visibility)
			{
				Visibility = visibilityFor;
			}
			if (realityStabilization.Projective && obj.CurrentCell == null)
			{
				Projective = true;
			}
		}
	}

	public virtual bool Maintain()
	{
		if (base.Duration > 1)
		{
			Strength = IndependentStrength;
			Visibility = IndependentVisibility;
			Projective = IndependentlyProjective;
		}
		else
		{
			Strength = 0;
			Visibility = 0;
			Projective = false;
		}
		if (base.Object == null)
		{
			return false;
		}
		ProcessGameObject(base.Object);
		if (base.Object.GetEffect("AmbientRealityStabilized") is AmbientRealityStabilized ambientRealityStabilized && ambientRealityStabilized.Duration > 0)
		{
			if (Strength < ambientRealityStabilized.Strength)
			{
				Strength = ambientRealityStabilized.Strength;
			}
			if (Visibility < ambientRealityStabilized.Visibility)
			{
				Visibility = ambientRealityStabilized.Visibility;
			}
			if (ambientRealityStabilized.Projective && !Projective)
			{
				Projective = true;
			}
		}
		Cell cell = base.Object.CurrentCell;
		if (cell != null)
		{
			int i = 0;
			for (int count = cell.Objects.Count; i < count; i++)
			{
				if (cell.Objects[i] != base.Object)
				{
					ProcessGameObject(cell.Objects[i]);
				}
			}
			if (ScanAdjacentCells)
			{
				ScanAdjacentCells = false;
				List<Cell> localAdjacentCells = cell.GetLocalAdjacentCells();
				int j = 0;
				for (int count2 = localAdjacentCells.Count; j < count2; j++)
				{
					Cell cell2 = localAdjacentCells[j];
					int k = 0;
					for (int count3 = cell2.Objects.Count; k < count3; k++)
					{
						ProcessGameObject(cell2.Objects[k]);
					}
				}
			}
		}
		List<GameObject> equippedObjectsAndInstalledCyberneticsReadonly = base.Object.GetEquippedObjectsAndInstalledCyberneticsReadonly();
		if (equippedObjectsAndInstalledCyberneticsReadonly != null)
		{
			int l = 0;
			for (int count4 = equippedObjectsAndInstalledCyberneticsReadonly.Count; l < count4; l++)
			{
				ProcessGameObject(equippedObjectsAndInstalledCyberneticsReadonly[l]);
			}
		}
		if (Strength <= 0 && base.Duration <= 1)
		{
			base.Duration = 0;
			base.Object.RemoveEffect(this);
			return false;
		}
		return true;
	}

	public bool SynchronizeEffect()
	{
		if (base.Object == null)
		{
			return false;
		}
		int strength = Strength;
		int visibility = Visibility;
		bool projective = Projective;
		if (!Maintain())
		{
			return false;
		}
		if (Strength <= 0)
		{
			return false;
		}
		if (strength == Strength && visibility == Visibility && projective == Projective)
		{
			return false;
		}
		Stabilize();
		return true;
	}

	public bool RandomlyTakeEffect()
	{
		return Strength.in100();
	}

	public bool RandomlyTakeEffect(int Adjust)
	{
		return (Strength + Adjust).in100();
	}

	public bool TakeEffectBasedOnPerformance(int Performance)
	{
		return Strength > Performance;
	}

	private void FailedToContest(GameObject GO)
	{
		BodyPart bodyPart = GO.Body?.GetFirstPart("Face");
		if (GO.IsPlayer())
		{
			if (bodyPart != null && bodyPart.VariantType == "Face")
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("You try to push through the normality lattice, but it snaps back into place.");
				if (GO.GetIntProperty("Analgesia") <= 0)
				{
					stringBuilder.Append(" You wince in pain.");
				}
				Popup.Show(stringBuilder.ToString());
			}
			else
			{
				Popup.Show("You try to push through the normality lattice, but it snaps back into place.");
			}
		}
		else
		{
			SensePsychicEffect sensePsychicEffect = SensePsychic.SensePsychicFromPlayer(GO);
			if (sensePsychicEffect != null)
			{
				IComponent<GameObject>.AddPlayerMessage("You feel a psychic thud as " + (sensePsychicEffect.Identified ? GO.does("push") : "someone pushes") + " against the structure of spacetime and " + (sensePsychicEffect.Identified ? GO.GetVerb("fail") : "fails") + " to break through.");
			}
			if (bodyPart != null && bodyPart.VariantType == "Face" && GO.GetIntProperty("Analgesia") <= 0 && IComponent<GameObject>.Visible(GO))
			{
				IComponent<GameObject>.AddPlayerMessage(GO.Does("wince") + ".");
			}
		}
		if (GO.TakeDamage(Stat.Random(1, 6), "from %t failed assault on the structure of spacetime.", "Cosmic RealityDistortionFailure Unavoidable", "You dashed " + GO.itself + " against the crags of spacetime.", GO.It + " @@dashed " + GO.itself + " against the crags of spacetime.", null, GO, null, null, Accidental: true) && !GO.MakeSave("Willpower", 20, null, null, "RealityStabilization Daze"))
		{
			GO.ApplyEffect(new Dazed(Stat.Random(1, 6)));
		}
		GO.UseEnergy(1000, "Effect");
	}

	private ContestResult TryContest(GameObject GO, int Adjust = 0, int Performance = int.MinValue)
	{
		if ((Performance == int.MinValue) ? RandomlyTakeEffect(Adjust) : TakeEffectBasedOnPerformance(Performance))
		{
			FailedToContest(GO);
			return ContestResult.Failure;
		}
		if (!GO.IsPlayer())
		{
			SensePsychicEffect sensePsychicEffect = SensePsychic.SensePsychicFromPlayer(GO);
			if (sensePsychicEffect != null)
			{
				IComponent<GameObject>.AddPlayerMessage("You feel a psychic whiff as " + (sensePsychicEffect.Identified ? GO.does("push") : "something pushes") + " past resistance in the structure of spacetime.");
			}
		}
		return ContestResult.Success;
	}

	private int ProcessEvent(Event E, out bool Dual, bool MarkHandled = false)
	{
		Dual = false;
		int num = -E.GetIntParameter("RealityStabilizationPenetration");
		if (E.HasParameter("Mutation") && E.GetParameter("Mutation") is IPart part)
		{
			GameObject parentObject = part.ParentObject;
			if (parentObject != null && parentObject != base.Object)
			{
				if (parentObject.GetEffectByClassName("RealityStabilized") is RealityStabilized realityStabilized)
				{
					Dual = true;
					int strength = realityStabilized.Strength;
					if (strength > Strength + num)
					{
						num += strength - (Strength + num);
					}
				}
				if (MarkHandled)
				{
					E.SetParameter("InterdictionHandled", 1);
					MarkHandled = false;
				}
			}
		}
		if (E.HasParameter("Cell"))
		{
			Cell c = E.GetParameter("Cell") as Cell;
			GameObject strongestRealityStabilizationSource = GetStrongestRealityStabilizationSource(c);
			if (strongestRealityStabilizationSource != null && strongestRealityStabilizationSource != base.Object)
			{
				if (strongestRealityStabilizationSource.GetEffectByClassName("RealityStabilized") is RealityStabilized realityStabilized2)
				{
					Dual = true;
					int strength2 = realityStabilized2.Strength;
					if (strength2 > Strength + num)
					{
						num += strength2 - (Strength + num);
					}
				}
				if (MarkHandled)
				{
					E.SetParameter("InterdictionHandled", 1);
					MarkHandled = false;
				}
			}
		}
		return num;
	}

	private int ProcessEvent(Event E, bool MarkHandled = false)
	{
		bool Dual;
		return ProcessEvent(E, out Dual, MarkHandled);
	}

	public ContestResult OptionToContest(GameObject GO, int Adjust = 0, bool Dual = false)
	{
		int num = 100 - (Strength + Adjust);
		int num2 = GO.Stat("Intelligence");
		Mutations part = GO.GetPart<Mutations>();
		if (part != null)
		{
			if (part.HasMutation("Intuition"))
			{
				num2 += 10;
			}
			if (part.HasMutation("Precognition"))
			{
				num2 += 5;
			}
			if (part.HasMutation("SpacetimeVortex"))
			{
				num2 += 5;
			}
			if (part.HasMutation("Skittish"))
			{
				num -= 10;
			}
		}
		if (GO.HasSkill("Discipline_IronMind"))
		{
			num2 += 2;
		}
		if (GO.HasSkill("Discipline_Lionheart"))
		{
			num += 3;
		}
		if (num2 <= 10)
		{
			num += 20;
		}
		else if (num2 <= 15)
		{
			num += 10;
		}
		else if (num2 <= 20)
		{
			num -= 10;
		}
		else if (num2 <= 25)
		{
			num -= 5;
		}
		int num3 = ((num2 <= 10) ? 20 : ((num2 <= 20) ? 10 : ((num2 > 30) ? 1 : 5)));
		if (num3 > 1)
		{
			num -= num % num3;
		}
		if (num > 100 - num3)
		{
			num = 100 - num3;
		}
		bool flag = GO.IsPlayer() && Options.SifrahRealityDistortion;
		if (num <= 0 && !flag)
		{
			return ContestResult.Abjection;
		}
		if (GO.IsPlayer())
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			string chanceColor = Stat.GetChanceColor(num);
			if (Dual)
			{
				stringBuilder.Append("A normality lattice prevents you from altering spacetime in both your local region and the local region you're trying to interact with.");
			}
			else
			{
				stringBuilder.Append("A normality lattice prevents you from altering spacetime in ").Append((GO.CurrentCell == base.Object.CurrentCell) ? "the" : "that").Append(" local region.");
			}
			stringBuilder.Append(" You can try to push through at some risk.");
			if (flag)
			{
				int num4 = GO.Stat("Ego");
				int num5 = 20 - num / 5 - num4 / 5;
				string value = ((num5 >= 12) ? "almost impossible" : ((num5 >= 9) ? "challenging" : ((num5 >= 6) ? "moderately difficult" : ((num5 < 3) ? "very easy" : "easy"))));
				stringBuilder.Append(" Your feeling is that success would be ").Append(value).Append(". Do you want to try?");
				if (Popup.ShowYesNo(stringBuilder.ToString()) == DialogResult.Yes)
				{
					GameObject contextObject = Owner ?? base.Object ?? GO;
					RealityDistortionSifrah realityDistortionSifrah = new RealityDistortionSifrah(contextObject, "RealityStabilizationPenetration", "overcoming a normality lattice", num4, Math.Max((Strength + Adjust) / 5, 1));
					realityDistortionSifrah.Play(contextObject);
					if (realityDistortionSifrah.Abort || realityDistortionSifrah.InterfaceExitRequested)
					{
						return ContestResult.Surrender;
					}
					return TryContest(GO, 0, realityDistortionSifrah.Performance);
				}
				return ContestResult.Surrender;
			}
			stringBuilder.Append(" You estimate");
			if (num < num3)
			{
				stringBuilder.Append(" less than a {{").Append(chanceColor).Append('|')
					.Append(num3.ToString())
					.Append("%}}");
			}
			else
			{
				stringBuilder.Append(" about a {{").Append(chanceColor).Append('|')
					.Append(num.ToString())
					.Append("%}}");
			}
			stringBuilder.Append(chanceColor).Append(" chance of success. Do you want to try?");
			if (Popup.ShowYesNo(stringBuilder.ToString()) == DialogResult.Yes)
			{
				return TryContest(GO, Adjust);
			}
			return ContestResult.Surrender;
		}
		if (num.in100() && num.in100())
		{
			return TryContest(GO, Adjust);
		}
		return ContestResult.Surrender;
	}

	private bool AttemptInterdictedAction(Event E)
	{
		if (E.HasFlag("Forced"))
		{
			return true;
		}
		if (E.HasParameter("Mutation"))
		{
			GameObject parentObject = (E.GetParameter("Mutation") as IPart).ParentObject;
			bool Dual;
			int num = ProcessEvent(E, out Dual, MarkHandled: true);
			ContestResult contestResult = OptionToContest(parentObject, num, num > 0);
			if (contestResult == ContestResult.Abjection)
			{
				if (Dual && num > 0)
				{
					ShowDualInterdictMessage(parentObject, E);
				}
				else if (parentObject.CurrentCell == base.Object.CurrentCell)
				{
					ShowGenericInterdictMessage(parentObject, E);
				}
				else
				{
					ShowDistantInterdictMessage(parentObject, E);
				}
			}
			switch (contestResult)
			{
			case ContestResult.Success:
				return true;
			case ContestResult.Failure:
				E.RequestInterfaceExit();
				break;
			}
		}
		else if (E.HasParameter("Device"))
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Device");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Operator");
			int adjust = -GetRealityStabilizationPenetrationEvent.GetFor(gameObjectParameter) + ProcessEvent(E, MarkHandled: true);
			if (!RandomlyTakeEffect(adjust))
			{
				return true;
			}
			ShortCircuitDevice(gameObjectParameter, gameObjectParameter2, E);
		}
		else if (E.HasParameter("Food"))
		{
			int adjust2 = -GetRealityStabilizationPenetrationEvent.GetFor(E.GetGameObjectParameter("Food")) + ProcessEvent(E, MarkHandled: true);
			if (!RandomlyTakeEffect(adjust2))
			{
				return true;
			}
		}
		else if (E.HasParameter("Source"))
		{
			int adjust3 = -GetRealityStabilizationPenetrationEvent.GetFor(E.GetGameObjectParameter("Source")) + ProcessEvent(E, MarkHandled: true);
			if (!RandomlyTakeEffect(adjust3))
			{
				return true;
			}
		}
		else if (!RandomlyTakeEffect())
		{
			return true;
		}
		return false;
	}

	public static void ShowGenericInterdictMessage(GameObject CheckObject = null, Event E = null)
	{
		if (CheckObject == null || CheckObject.IsPlayer())
		{
			string text = E?.GetStringParameter("Purpose");
			Popup.Show("You cannot alter spacetime through the normality lattice in the local region" + ((!string.IsNullOrEmpty(text)) ? (", in order to " + text) : "") + ".");
		}
	}

	public static void ShowDistantInterdictMessage(GameObject CheckObject = null, Event E = null)
	{
		if (CheckObject == null || CheckObject.IsPlayer())
		{
			string text = E?.GetStringParameter("Purpose");
			Popup.Show("You cannot alter spacetime through the normality lattice in the local region" + ((!string.IsNullOrEmpty(text)) ? (", in order to " + text) : "") + ".");
		}
	}

	public static void ShowDualInterdictMessage(GameObject CheckObject = null, Event E = null)
	{
		if (CheckObject == null || CheckObject.IsPlayer())
		{
			string text = E?.GetStringParameter("Purpose");
			Popup.Show("You cannot alter spacetime through either the normality lattice in your local region or the local region you're trying to interact with" + ((!string.IsNullOrEmpty(text)) ? (", in order to " + text) : "") + ".");
		}
	}

	private static void ShortCircuitDevice(GameObject Device, GameObject User, Event E = null)
	{
		if (User == null)
		{
			User = Device.Equipped ?? Device.Implantee ?? Device.InInventory;
		}
		if (User == null)
		{
			IComponent<GameObject>.AddPlayerMessage(Device.Does("shower") + " sparks everywhere.");
		}
		else if (User.IsPlayer())
		{
			if (User == Device)
			{
				Popup.Show("You emit a shower of sparks!");
			}
			else
			{
				Popup.Show(Device.Does("emit") + " a shower of sparks!");
			}
		}
		else if (IComponent<GameObject>.Visible(User))
		{
			if (User == Device)
			{
				IComponent<GameObject>.AddPlayerMessage(User.Does("shower") + " sparks everywhere.");
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage(User.Poss(Device) + Device.GetVerb("shower") + " sparks everywhere.");
			}
		}
		if (User != null)
		{
			if (User.TakeDamage(Stat.Random(1, 6), "from %t electrical discharge!", "Electric", null, null, User, Device, null, null, Accidental: true))
			{
				for (int i = 0; i < 1; i++)
				{
					User.ParticleText("&Wú");
				}
				for (int j = 0; j < 1; j++)
				{
					User.ParticleText("&Yú");
				}
			}
			Device.UseCharge("10d100".RollCached(), LiveOnly: false, 0L);
			User.UseEnergy(1000, "Item Use Failure");
		}
		E?.RequestInterfaceExit();
	}

	private static void ShortCircuitDevice(Event E)
	{
		ShortCircuitDevice(E.GetGameObjectParameter("Device"), E.GetGameObjectParameter("Operator"), E);
	}

	public bool Stabilize(bool Projecting = false)
	{
		bool Relevant;
		bool CanDestroy;
		return Stabilize(base.Object, Projecting, out Relevant, out CanDestroy);
	}

	public bool Stabilize(GameObject GO, bool Projecting = false)
	{
		bool Relevant;
		bool CanDestroy;
		return Stabilize(GO, Projecting, out Relevant, out CanDestroy);
	}

	public bool Stabilize(GameObject GO, out bool Relevant)
	{
		bool CanDestroy;
		return Stabilize(GO, Projecting: false, out Relevant, out CanDestroy);
	}

	public bool Stabilize(out bool Relevant, out bool CanDestroy)
	{
		return Stabilize(base.Object, Projecting: false, out Relevant, out CanDestroy);
	}

	public bool Stabilize(bool Projecting, out bool Relevant, out bool CanDestroy)
	{
		return Stabilize(base.Object, Projecting, out Relevant, out CanDestroy);
	}

	public bool Stabilize(GameObject GO, bool Projecting, out bool Relevant)
	{
		bool CanDestroy;
		return Stabilize(GO, Projecting, out Relevant, out CanDestroy);
	}

	public bool Stabilize(GameObject GO, bool Projecting, out bool Relevant, out bool CanDestroy)
	{
		RealityStabilizeEvent.Send(this, GO, Projecting, out Relevant, out CanDestroy);
		if (CanDestroy)
		{
			if (GO != null && !GO.IsInvalid())
			{
				return GO.IsInGraveyard();
			}
			return true;
		}
		return false;
	}

	private GameObject GetStrongestRealityStabilizationSource(Cell C)
	{
		GameObject result = null;
		int num = 0;
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			if (C.Objects[i].GetEffectByClassName("RealityStabilized") is RealityStabilized realityStabilized && realityStabilized.Strength > num)
			{
				result = C.Objects[i];
				num = realityStabilized.Strength;
			}
		}
		return result;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectEnteringCellBlockedBySolid")
		{
			if (Projective)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
				bool Relevant = false;
				bool CanDestroy = false;
				if (Stabilize(gameObjectParameter, Projecting: true, out Relevant, out CanDestroy))
				{
					return false;
				}
				if (CanDestroy)
				{
					if (base.Object.IsPlayer())
					{
						StringBuilder stringBuilder = Event.NewStringBuilder();
						stringBuilder.Append(gameObjectParameter.T());
						if (gameObjectParameter.HasPart("Metal"))
						{
							stringBuilder.Append(" {{electrical|").Append(gameObjectParameter.GetVerb("spark", PrependSpace: false)).Append(" and")
								.Append(gameObjectParameter.GetVerb("buzz"))
								.Append("}}");
						}
						else
						{
							stringBuilder.Append(" {{C|").Append(gameObjectParameter.GetVerb("shift", PrependSpace: false)).Append(" and")
								.Append(gameObjectParameter.GetVerb("waver"))
								.Append("}}");
						}
						stringBuilder.Append(" for a moment, but nothing else happens.");
						Popup.Show(stringBuilder.ToString());
					}
					base.Object.UseEnergy(1000, "Movement Failure");
				}
			}
		}
		else if (E.ID == "CheckRealityDistortionAccessibility")
		{
			if ((base.Object.CurrentCell == null || base.Object == GetStrongestRealityStabilizationSource(base.Object.CurrentCell)) && RandomlyTakeEffect())
			{
				return false;
			}
		}
		else if (E.ID == "CheckRealityDistortionUsability")
		{
			if (E.HasParameter("Threshold"))
			{
				if (Math.Min(E.GetIntParameter("Threshold"), 100) <= Strength)
				{
					return false;
				}
			}
			else if (RandomlyTakeEffect())
			{
				return false;
			}
		}
		else if (E.ID == "CheckRealityDistortionAdvisability")
		{
			int num = (E.HasParameter("Mutation") ? 80 : 30) - E.GetIntParameter("RealityStabilizationPenetration");
			if (Strength > num)
			{
				return false;
			}
		}
		else if (E.ID == "InitiateRealityDistortionLocal")
		{
			if (!AttemptInterdictedAction(E))
			{
				return false;
			}
		}
		else if (E.ID == "InitiateRealityDistortionRemote" || E.ID == "InitiateRealityDistortionTransit")
		{
			if (E.GetIntParameter("InterdictionHandled") < 1 && (base.Object == E.GetGameObjectParameter("Object") || base.Object == E.GetGameObjectParameter("Subject") || base.Object.CurrentCell == null || base.Object == GetStrongestRealityStabilizationSource(base.Object.CurrentCell)))
			{
				return AttemptInterdictedAction(E);
			}
		}
		else if (E.ID == "SpaceTimeVortexContact")
		{
			if (Stabilize(E.GetGameObjectParameter("Object")))
			{
				return false;
			}
		}
		else if (E.ID == "MaintainRealityStabilization" || E.ID == "EquipperUnequipped")
		{
			Maintain();
		}
		return base.FireEvent(E);
	}
}

using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialGeneric : IPart
{
	public int nFrameOffset;

	public int AnimationLength = 60;

	public int LowFrameOffset = 1;

	public int HighFrameOffset = 1;

	public string ColorStringAnimationFrames;

	[NonSerialized]
	private List<int> ColorStringAnimationTimes;

	[NonSerialized]
	private List<string> ColorStringAnimationColors;

	public string BackgroundStringAnimationFrames;

	[NonSerialized]
	private List<int> BackgroundStringAnimationTimes;

	[NonSerialized]
	private List<string> BackgroundStringAnimationColors;

	public string DetailColorAnimationFrames;

	[NonSerialized]
	private List<int> DetailColorAnimationTimes;

	[NonSerialized]
	private List<string> DetailColorAnimationColors;

	[FieldSaveVersion(244)]
	public string TileColorAnimationFrames;

	[NonSerialized]
	private List<int> TileColorAnimationTimes;

	[NonSerialized]
	private List<string> TileColorAnimationColors;

	public string RenderStringAnimationFrames;

	[NonSerialized]
	private List<int> RenderStringAnimationTimes;

	[NonSerialized]
	private List<string> RenderStringAnimationColors;

	public string TileAnimationFrames;

	[NonSerialized]
	private List<int> TileAnimationTimes;

	[NonSerialized]
	private List<string> TileAnimationColors;

	public string RequiresOperationalActivePart;

	[NonSerialized]
	private bool? HasOperationalActivePart;

	public string RequiresUnpoweredActivePart;

	[NonSerialized]
	private bool? HasUnpoweredActivePart;

	public bool RequiresAnyUnpoweredActivePart;

	[NonSerialized]
	private bool? HasAnyUnpoweredActivePart;

	public bool ActivePartStatusIgnoreCharge;

	public bool ActivePartStatusIgnoreBreakage;

	public bool ActivePartStatusIgnoreRust;

	public bool ActivePartStatusIgnoreEMP;

	public bool ActivePartStatusIgnoreRealityStabilization;

	public bool ActivePartStatusIgnoreSubject;

	public bool ActivePartStatusIgnoreLocallyDefinedFailure;

	public string RequiresEvent;

	public string RequiresInverseEvent;

	[NonSerialized]
	private bool? HasEvent;

	[NonSerialized]
	private bool? HasInverseEvent;

	[FieldSaveVersion(248)]
	public string RequiresEffect;

	[FieldSaveVersion(248)]
	public string RequiresInverseEffect;

	[NonSerialized]
	private bool? HasEffect;

	[NonSerialized]
	private bool? HasInverseEffect;

	public AnimatedMaterialGeneric()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 60);
	}

	public override bool SameAs(IPart p)
	{
		AnimatedMaterialGeneric animatedMaterialGeneric = p as AnimatedMaterialGeneric;
		if (animatedMaterialGeneric.AnimationLength != AnimationLength)
		{
			return false;
		}
		if (animatedMaterialGeneric.LowFrameOffset != LowFrameOffset)
		{
			return false;
		}
		if (animatedMaterialGeneric.HighFrameOffset != HighFrameOffset)
		{
			return false;
		}
		if (animatedMaterialGeneric.ColorStringAnimationFrames != ColorStringAnimationFrames)
		{
			return false;
		}
		if (animatedMaterialGeneric.BackgroundStringAnimationFrames != BackgroundStringAnimationFrames)
		{
			return false;
		}
		if (animatedMaterialGeneric.DetailColorAnimationFrames != DetailColorAnimationFrames)
		{
			return false;
		}
		if (animatedMaterialGeneric.TileColorAnimationFrames != TileColorAnimationFrames)
		{
			return false;
		}
		if (animatedMaterialGeneric.RenderStringAnimationFrames != RenderStringAnimationFrames)
		{
			return false;
		}
		if (animatedMaterialGeneric.TileAnimationFrames != TileAnimationFrames)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresOperationalActivePart != RequiresOperationalActivePart)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresUnpoweredActivePart != RequiresUnpoweredActivePart)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresAnyUnpoweredActivePart != RequiresAnyUnpoweredActivePart)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreCharge != ActivePartStatusIgnoreCharge)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreBreakage != ActivePartStatusIgnoreBreakage)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreRust != ActivePartStatusIgnoreRust)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreEMP != ActivePartStatusIgnoreEMP)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreRealityStabilization != ActivePartStatusIgnoreRealityStabilization)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreLocallyDefinedFailure != ActivePartStatusIgnoreLocallyDefinedFailure)
		{
			return false;
		}
		if (animatedMaterialGeneric.ActivePartStatusIgnoreSubject != ActivePartStatusIgnoreSubject)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresEvent != RequiresEvent)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresInverseEvent != RequiresInverseEvent)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresEffect != RequiresEffect)
		{
			return false;
		}
		if (animatedMaterialGeneric.RequiresInverseEffect != RequiresInverseEffect)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		TurnProcess();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		TurnProcess();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		TurnProcess();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EffectAppliedEvent.ID)
		{
			return ID == EffectRemovedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		HasEffect = null;
		HasInverseEffect = null;
		FlushNonEffectStateCaches();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private ActivePartStatus? StatusOf(IActivePart p)
	{
		return p?.GetActivePartStatus(UseCharge: false, ActivePartStatusIgnoreCharge, IgnoreBootSequence: false, ActivePartStatusIgnoreBreakage, ActivePartStatusIgnoreRust, ActivePartStatusIgnoreEMP, ActivePartStatusIgnoreRealityStabilization, ActivePartStatusIgnoreSubject, ActivePartStatusIgnoreLocallyDefinedFailure, 1, null, UseChargeIfUnpowered: false, 0L);
	}

	private ActivePartStatus? StatusOf(string PartName)
	{
		return StatusOf(ParentObject.GetPart(PartName) as IActivePart);
	}

	public override bool Render(RenderEvent E)
	{
		if (!string.IsNullOrEmpty(RequiresOperationalActivePart))
		{
			if (!HasOperationalActivePart.HasValue)
			{
				HasOperationalActivePart = StatusOf(RequiresOperationalActivePart) == ActivePartStatus.Operational;
			}
			if (HasOperationalActivePart != true)
			{
				return true;
			}
		}
		if (!string.IsNullOrEmpty(RequiresUnpoweredActivePart))
		{
			if (!HasUnpoweredActivePart.HasValue)
			{
				HasUnpoweredActivePart = StatusOf(RequiresUnpoweredActivePart) == ActivePartStatus.Unpowered;
			}
			if (HasUnpoweredActivePart != true)
			{
				return true;
			}
		}
		if (RequiresAnyUnpoweredActivePart)
		{
			if (!HasAnyUnpoweredActivePart.HasValue)
			{
				if (HasUnpoweredActivePart == true)
				{
					HasAnyUnpoweredActivePart = true;
				}
				else
				{
					int i = 0;
					for (int count = ParentObject.PartsList.Count; i < count; i++)
					{
						if (ParentObject.PartsList[i] is IActivePart p && StatusOf(p) == ActivePartStatus.Unpowered)
						{
							HasAnyUnpoweredActivePart = true;
							break;
						}
					}
				}
			}
			if (HasAnyUnpoweredActivePart != true)
			{
				return true;
			}
		}
		if (!string.IsNullOrEmpty(RequiresEvent))
		{
			if (!HasEvent.HasValue)
			{
				HasEvent = ParentObject.FireEvent(RequiresEvent);
			}
			if (HasEvent != true)
			{
				return true;
			}
		}
		if (!string.IsNullOrEmpty(RequiresInverseEvent))
		{
			if (!HasInverseEvent.HasValue)
			{
				HasInverseEvent = !ParentObject.FireEvent(RequiresInverseEvent);
			}
			if (HasInverseEvent != true)
			{
				return true;
			}
		}
		if (!string.IsNullOrEmpty(RequiresEffect))
		{
			if (!HasEffect.HasValue)
			{
				HasEffect = ParentObject.HasEffect(RequiresEffect);
			}
			if (HasEffect != true)
			{
				return true;
			}
		}
		if (!string.IsNullOrEmpty(RequiresInverseEffect))
		{
			if (!HasInverseEffect.HasValue)
			{
				HasInverseEffect = !ParentObject.HasEffect(RequiresInverseEffect);
			}
			if (HasInverseEffect != true)
			{
				return true;
			}
		}
		int num = (XRLCore.GetCurrentFrameAtFPS(60) + nFrameOffset) % AnimationLength;
		nFrameOffset += Stat.RandomCosmetic(LowFrameOffset, HighFrameOffset);
		if (ColorStringAnimationTimes == null && !string.IsNullOrEmpty(ColorStringAnimationFrames))
		{
			if (ColorStringAnimationFrames == "0=default" || ColorStringAnimationFrames == "disable")
			{
				ColorStringAnimationTimes = new List<int>();
				ColorStringAnimationColors = new List<string>();
			}
			else
			{
				string[] array = ColorStringAnimationFrames.Split(',');
				ColorStringAnimationTimes = new List<int>(array.Length);
				ColorStringAnimationColors = new List<string>(array.Length);
				string[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					string[] array3 = array2[j].Split('=');
					ColorStringAnimationTimes.Add(int.Parse(array3[0]));
					ColorStringAnimationColors.Add(array3[1]);
				}
			}
		}
		if (TileColorAnimationTimes == null && !string.IsNullOrEmpty(TileColorAnimationFrames))
		{
			if (TileColorAnimationFrames == "0=default" || TileColorAnimationFrames == "disable")
			{
				TileColorAnimationTimes = new List<int>();
				TileColorAnimationColors = new List<string>();
			}
			else
			{
				string[] array4 = TileColorAnimationFrames.Split(',');
				TileColorAnimationTimes = new List<int>(array4.Length);
				TileColorAnimationColors = new List<string>(array4.Length);
				string[] array2 = array4;
				for (int j = 0; j < array2.Length; j++)
				{
					string[] array5 = array2[j].Split('=');
					TileColorAnimationTimes.Add(int.Parse(array5[0]));
					TileColorAnimationColors.Add(array5[1]);
				}
			}
		}
		if (E.ColorsVisible)
		{
			if (TileColorAnimationTimes != null && Globals.RenderMode == RenderModeType.Tiles)
			{
				string text = null;
				for (int k = 0; k < TileColorAnimationTimes.Count && num >= TileColorAnimationTimes[k]; k++)
				{
					text = TileColorAnimationColors[k];
				}
				switch (text)
				{
				case "default":
					E.ColorString = ParentObject.pRender.GetRenderColor();
					break;
				case "liquid":
					E.ColorString = "&" + ParentObject.GetLiquidColor();
					break;
				default:
					E.ColorString = text;
					break;
				case null:
					break;
				}
			}
			else if (ColorStringAnimationTimes != null)
			{
				string text2 = null;
				for (int l = 0; l < ColorStringAnimationTimes.Count && num >= ColorStringAnimationTimes[l]; l++)
				{
					text2 = ColorStringAnimationColors[l];
				}
				switch (text2)
				{
				case "default":
					E.ColorString = ParentObject.pRender.GetRenderColor();
					break;
				case "liquid":
					E.ColorString = "&" + ParentObject.GetLiquidColor();
					break;
				default:
					E.ColorString = text2;
					break;
				case null:
					break;
				}
			}
		}
		if (BackgroundStringAnimationTimes == null && !string.IsNullOrEmpty(BackgroundStringAnimationFrames))
		{
			string[] array6 = BackgroundStringAnimationFrames.Split(',');
			BackgroundStringAnimationTimes = new List<int>(array6.Length);
			BackgroundStringAnimationColors = new List<string>(array6.Length);
			string[] array2 = array6;
			for (int j = 0; j < array2.Length; j++)
			{
				string[] array7 = array2[j].Split('=');
				BackgroundStringAnimationTimes.Add(int.Parse(array7[0]));
				BackgroundStringAnimationColors.Add(array7[1]);
			}
		}
		if (BackgroundStringAnimationTimes != null && E.ColorsVisible)
		{
			string text3 = null;
			for (int m = 0; m < BackgroundStringAnimationTimes.Count && num >= BackgroundStringAnimationTimes[m]; m++)
			{
				text3 = BackgroundStringAnimationColors[m];
			}
			if (text3 != null)
			{
				if (text3 == "default")
				{
					E.BackgroundString = null;
				}
				else
				{
					E.BackgroundString = text3;
				}
			}
		}
		if (DetailColorAnimationTimes == null && !string.IsNullOrEmpty(DetailColorAnimationFrames))
		{
			string[] array8 = DetailColorAnimationFrames.Split(',');
			DetailColorAnimationTimes = new List<int>(array8.Length);
			DetailColorAnimationColors = new List<string>(array8.Length);
			string[] array2 = array8;
			for (int j = 0; j < array2.Length; j++)
			{
				string[] array9 = array2[j].Split('=');
				DetailColorAnimationTimes.Add(int.Parse(array9[0]));
				DetailColorAnimationColors.Add(array9[1]);
			}
		}
		if (DetailColorAnimationTimes != null && E.ColorsVisible)
		{
			string text4 = null;
			for (int n = 0; n < DetailColorAnimationTimes.Count && num >= DetailColorAnimationTimes[n]; n++)
			{
				text4 = DetailColorAnimationColors[n];
			}
			switch (text4)
			{
			case "default":
				E.DetailColor = ParentObject.pRender.DetailColor;
				break;
			case "liquid":
				E.DetailColor = ParentObject.GetLiquidColor();
				break;
			default:
				E.DetailColor = text4;
				break;
			case null:
				break;
			}
		}
		if (RenderStringAnimationTimes == null && !string.IsNullOrEmpty(RenderStringAnimationFrames))
		{
			string[] array10 = RenderStringAnimationFrames.Split(',');
			RenderStringAnimationTimes = new List<int>(array10.Length);
			RenderStringAnimationColors = new List<string>(array10.Length);
			string[] array2 = array10;
			for (int j = 0; j < array2.Length; j++)
			{
				string[] array11 = array2[j].Split('=');
				RenderStringAnimationTimes.Add(int.Parse(array11[0]));
				RenderStringAnimationColors.Add(array11[1]);
			}
		}
		if (RenderStringAnimationTimes != null)
		{
			string text5 = null;
			for (int num2 = 0; num2 < RenderStringAnimationTimes.Count && num >= RenderStringAnimationTimes[num2]; num2++)
			{
				text5 = RenderStringAnimationColors[num2];
			}
			if (text5 != null)
			{
				if (text5 == "default")
				{
					E.RenderString = ParentObject.pRender.RenderString;
				}
				else
				{
					E.RenderString = text5;
				}
			}
		}
		if (TileAnimationTimes == null && !string.IsNullOrEmpty(TileAnimationFrames))
		{
			string[] array12 = TileAnimationFrames.Split(',');
			TileAnimationTimes = new List<int>(array12.Length);
			TileAnimationColors = new List<string>(array12.Length);
			string[] array2 = array12;
			for (int j = 0; j < array2.Length; j++)
			{
				string[] array13 = array2[j].Split('=');
				TileAnimationTimes.Add(int.Parse(array13[0]));
				TileAnimationColors.Add(array13[1]);
			}
		}
		if (TileAnimationTimes != null)
		{
			string text6 = null;
			for (int num3 = 0; num3 < TileAnimationTimes.Count && num >= TileAnimationTimes[num3]; num3++)
			{
				text6 = TileAnimationColors[num3];
			}
			if (text6 != null)
			{
				if (text6 == "default")
				{
					E.Tile = ParentObject.pRender.Tile;
				}
				else
				{
					E.Tile = text6;
				}
			}
		}
		return true;
	}

	[Obsolete("save compat")]
	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
		if (Reader.FileVersion <= 262 && ParentObject.HasIntProperty("Wall") && TileColorAnimationFrames.IsNullOrEmpty())
		{
			GameObjectBlueprint blueprint = ParentObject.GetBlueprint();
			TileColorAnimationFrames = blueprint.GetPartParameter("AnimatedMaterialGeneric", "TileColorAnimationFrames");
			DetailColorAnimationFrames = blueprint.GetPartParameter("AnimatedMaterialGeneric", "DetailColorAnimationFrames");
		}
	}

	private void TurnProcess()
	{
		FlushNonEffectStateCaches();
	}

	private void FlushNonEffectStateCaches()
	{
		HasOperationalActivePart = null;
		HasUnpoweredActivePart = null;
		HasAnyUnpoweredActivePart = null;
		HasEvent = null;
		HasInverseEvent = null;
	}
}

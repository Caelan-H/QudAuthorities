using System;
using UnityEngine;

namespace XRL.UI.Framework;

[Serializable]
public class SummaryBlockData : FrameworkDataElement
{
	public string Title;

	public string IconPath;

	public Color IconForegroundColor;

	public Color IconDetailColor;

	public int SortOrder;
}

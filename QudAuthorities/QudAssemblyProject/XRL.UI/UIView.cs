using System;

namespace XRL.UI;

/// <summary>
///             Defines a UI View to be pushed to the Game Manager.  When used on a <c>XRL.UI.IWantsTextConsoleInit</c>, it will be passed the text
///             console and screen buffer at initializtion.  When used on a <c>QupKit.BaseView</c>, it will attach the Unity view to the QupKit views.
///             </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class UIView : Attribute
{
	public string ID;

	public bool WantsTileOver;

	public bool ForceFullscreen;

	public bool ForceFullscreenInLegacy;

	public bool IgnoreForceFullscreen;

	public string NavCategory;

	/// <summary>
	///             The name of the UI Canvas (Unity) GameObject which should be displayed when this view is active
	///             </summary>
	public string UICanvas;

	public int UICanvasHost;

	/// <summary>
	///             a flag that determines if the view allows itself to be zoomed in and out with the mouse wheel
	///             </summary>
	public bool TakesScroll;

	public UIView(string ID, bool WantsTileOver = false, bool ForceFullscreen = false, bool IgnoreForceFullscreen = false, string NavCategory = null, string UICanvas = null, bool TakesScroll = false, int UICanvasHost = 0, bool ForceFullscreenInLegacy = false)
	{
		this.ID = ID;
		this.WantsTileOver = WantsTileOver;
		this.IgnoreForceFullscreen = IgnoreForceFullscreen;
		this.ForceFullscreen = ForceFullscreen;
		this.ForceFullscreenInLegacy = ForceFullscreenInLegacy;
		this.NavCategory = NavCategory;
		this.UICanvas = UICanvas;
		this.TakesScroll = TakesScroll;
		this.UICanvasHost = UICanvasHost;
	}

	public GameManager.ViewInfo AsGameManagerViewInfo()
	{
		return new GameManager.ViewInfo(WantsTileOver, ForceFullscreen, NavCategory, UICanvas, 0, ExecuteActions: false, TakesScroll, UICanvasHost, IgnoreForceFullscreen, ForceFullscreenInLegacy);
	}
}

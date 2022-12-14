using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qud.API;
using UnityEngine.UI;
using XRL;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[UIView("SaveManagement", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "SaveManagement", UICanvasHost = 1)]
public class SaveManagement : SingletonWindowBase<SaveManagement>, ControlManager.IControllerChangedEvent
{
	protected List<SaveInfoData> saves;

	public Image background;

	public FrameworkScroller hotkeyBar;

	public FrameworkScroller savesScroller;

	public EmbarkBuilderModuleBackButton backButton;

	public TaskCompletionSource<SaveGameInfo> completionSource;

	public NavigationContext globalContext = new NavigationContext();

	public ScrollContext<NavigationContext> midHorizNav = new ScrollContext<NavigationContext>();

	private bool SelectFirst = true;

	public bool wasInScroller;

	public void SetupContext()
	{
		globalContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		globalContext.buttonHandlers.Set(InputButtonTypes.CancelButton, Event.Helpers.Handle(Exit));
		midHorizNav.SetAxis(InputAxisTypes.NavigationXAxis);
		midHorizNav.contexts.Clear();
		midHorizNav.contexts.Add(backButton.navigationContext);
		midHorizNav.contexts.Add(savesScroller.GetNavigationContext());
		midHorizNav.Setup();
		midHorizNav.parentContext = globalContext;
	}

	public override void Show()
	{
		base.Show();
		backButton?.gameObject.SetActive(value: true);
		if (backButton.navigationContext == null)
		{
			backButton.Awake();
		}
		backButton.navigationContext.buttonHandlers = new Dictionary<InputButtonTypes, Action>();
		backButton.navigationContext.buttonHandlers.Set(InputButtonTypes.AcceptButton, Event.Helpers.Handle(Exit));
		saves = (from info in SavesAPI.GetSavedGameInfo()
			select new SaveInfoData
			{
				SaveGame = info
			}).ToList();
		if (saves.Count == 0)
		{
			completionSource.TrySetResult(null);
		}
		savesScroller.scrollContext.wraps = true;
		savesScroller.BeforeShow(null, saves);
		foreach (SaveManagementRow item in savesScroller.selectionClones.Select((FrameworkUnityScrollChild s) => s.GetComponent<SaveManagementRow>()))
		{
			if (item != null)
			{
				item.deleteButton.context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
				{
					InputButtonTypes.AcceptButton,
					Event.Helpers.Handle(HandleDelete)
				} };
				item.context.context.commandHandlers = new Dictionary<string, Action> { 
				{
					"U Negative",
					Event.Helpers.Handle(HandleDelete)
				} };
			}
		}
		if (SelectFirst)
		{
			SelectFirst = false;
			savesScroller.scrollContext.selectedPosition = 0;
		}
		else if (savesScroller.scrollContext.selectedPosition >= saves.Count)
		{
			savesScroller.scrollContext.selectedPosition = Math.Max(saves.Count - 1, 0);
		}
		savesScroller.onSelected.RemoveAllListeners();
		savesScroller.onSelected.AddListener(SelectedInfo);
		SetupContext();
		EnableNavContext();
		UpdateMenuBars();
	}

	public override void Hide()
	{
		base.Hide();
		DisableNavContext();
		base.gameObject.SetActive(value: false);
	}

	public void EnableNavContext()
	{
		globalContext.disabled = false;
		savesScroller.GetNavigationContext().Activate();
	}

	public void DisableNavContext(bool deactivate = true)
	{
		if (deactivate)
		{
			NavigationContext activeContext = NavigationController.instance.activeContext;
			if (activeContext != null && activeContext.IsInside(globalContext))
			{
				NavigationController.instance.activeContext = null;
			}
		}
		globalContext.disabled = true;
	}

	public void SelectedInfo(FrameworkDataElement data)
	{
		if (data is SaveInfoData saveInfoData)
		{
			completionSource?.TrySetResult(saveInfoData.SaveGame);
		}
	}

	public async Task<XRLGame> ContinueMenu()
	{
		SelectFirst = true;
		while (true)
		{
			completionSource?.TrySetCanceled();
			completionSource = new TaskCompletionSource<SaveGameInfo>();
			await The.UiContext;
			ControlManager.ResetInput();
			Show();
			SaveGameInfo info = await completionSource.Task;
			DisableNavContext();
			await The.UiContext;
			if (info == null)
			{
				break;
			}
			try
			{
				if (await info.TryRestoreModsAndLoadAsync())
				{
					Hide();
					return The.Game;
				}
			}
			catch (Exception ex)
			{
				MetricsManager.LogEditorError("Continue Menu", ex.ToString());
			}
		}
		Hide();
		return null;
	}

	public void Exit()
	{
		MetricsManager.LogEditorInfo("Exiting continue screen");
		completionSource?.TrySetResult(null);
		GameManager.Instance.skipAnInput = true;
	}

	public async void HandleDelete()
	{
		if (NavigationController.instance.activeContext.IsInside(savesScroller.GetNavigationContext()))
		{
			SaveInfoData saveInfo = saves[savesScroller.selectedPosition];
			string title = "{{R|Delete " + saveInfo.SaveGame.Name + "}}";
			if ((await Popup.NewPopupMessageAsync("Are you sure you want to delete the save game for " + saveInfo.SaveGame.Name + "?", PopupMessage.AcceptCancelButton, null, title, null, 1)).command != "Cancel")
			{
				DisableNavContext();
				saveInfo.SaveGame.Delete();
				Show();
				await Popup.NewPopupMessageAsync("Game Deleted!", PopupMessage.AcceptButton);
			}
		}
	}

	public void UpdateMenuBars()
	{
		List<MenuOption> list = new List<MenuOption>();
		list.Add(new MenuOption
		{
			InputCommand = "NavigationXYAxis",
			Description = "navigate"
		});
		list.Add(new MenuOption
		{
			KeyDescription = ControlManager.getCommandInputDescription("Accept"),
			Description = "select"
		});
		hotkeyBar.GetNavigationContext().disabled = true;
		hotkeyBar.BeforeShow(null, list);
	}

	public void Update()
	{
		if ((NavigationController.instance.activeContext?.IsInside(savesScroller.GetNavigationContext()) ?? false) != wasInScroller)
		{
			UpdateMenuBars();
		}
	}

	public void ControllerChanged()
	{
		UpdateMenuBars();
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Qud.UI;
using QupKit;
using UnityEngine;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds;

/// <summary>
///             The main Unity MonoBehaviour that controls the new Embark experience.  
///             There can only ever be one of these because it's attached to the GameManager.
///             </summary>
public class EmbarkBuilder : MonoBehaviour
{
	public static class EventNames
	{
		public static readonly string EditableGameModeQuery = "EditableGameModeQuery";
	}

	public static TaskCompletionSource<EmbarkInfo> finishedEvent = new TaskCompletionSource<EmbarkInfo>();

	private bool exitGuard;

	protected EmbarkInfo embarkInfo = new EmbarkInfo();

	protected List<AbstractEmbarkBuilderModule> modules;

	protected List<EmbarkBuilderModuleWindowDescriptor> windowDescriptors;

	public EmbarkBuilderModuleWindowDescriptor activeWindow;

	protected List<AbstractBuilderModuleWindowBase> windows;

	public List<AbstractBuilderModuleWindowBase> createdWindows = new List<AbstractBuilderModuleWindowBase>();

	/// <summary>
	///                 Static helper to get at the game object.
	///             </summary>
	public new static GameObject gameObject => GameManager.Instance.gameObject;

	public EmbarkInfo info => embarkInfo;

	public IEnumerable<AbstractEmbarkBuilderModule> enabledModules => modules.Where((AbstractEmbarkBuilderModule m) => m.enabled);

	public static async Task<EmbarkInfo> Begin()
	{
		finishedEvent = new TaskCompletionSource<EmbarkInfo>();
		if (Thread.CurrentThread == GameManager.Instance.uiQueue.threadContext)
		{
			throw new InvalidOperationException("EmbarkBuilder::Begin can only be called from the game thread.");
		}
		BaseView oldGameView = GameManager.Instance.Views.ActiveView;
		GameManager.Instance.CurrentGameView = "EmbarkBuilder";
		await The.UiContext;
		oldGameView?.Leave();
		ControlManager.EnableLayer("Adventure");
		ControlManager.EnableLayer("AltAdventure");
		ControlManager.EnableLayer("Chargen");
		gameObject.GetComponent<EmbarkBuilder>()?.Destroy();
		GameManager.Instance.gameObject.AddComponent<EmbarkBuilder>().InitModules();
		try
		{
			return await finishedEvent.Task.ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			await The.UiContext;
			oldGameView?.Enter();
			ControlManager.EnableLayer("Adventure");
			ControlManager.EnableLayer("AltAdventure");
			ControlManager.EnableLayer("Chargen");
		}
	}

	public string generateCode()
	{
		return CodeCompressor.generateCode(enabledModules);
	}

	public object handleUIEvent(string id, object element = null)
	{
		foreach (AbstractEmbarkBuilderModule enabledModule in enabledModules)
		{
			element = enabledModule.handleUIEvent(id, element);
		}
		return element;
	}

	public void ShowWindow<T>() where T : AbstractBuilderModuleWindowBase
	{
		EmbarkBuilderModuleWindowDescriptor embarkBuilderModuleWindowDescriptor = windowDescriptors.Find((EmbarkBuilderModuleWindowDescriptor desc) => desc.getWindow() is T);
		if (embarkBuilderModuleWindowDescriptor == null)
		{
			throw new Exception("Window of type T not found.");
		}
		ShowWindow(embarkBuilderModuleWindowDescriptor);
	}

	public void ShowWindow(EmbarkBuilderModuleWindowDescriptor windowDescriptor)
	{
		windowDescriptor?.show();
		if ((windowDescriptor?.getWindow()?.UseOverlay()).GetValueOrDefault())
		{
			GetOverlayWindow().BeforeShowWithWindow(this, windowDescriptor);
			GetOverlayWindow().Show();
		}
		else
		{
			GetOverlayWindow().Hide();
		}
		NavigationContext navigationContext = windowDescriptor?.getWindow()?.GetNavigationContext();
		if (navigationContext == null)
		{
			NavigationController.instance.activeContext = null;
		}
		else
		{
			navigationContext.Activate();
		}
	}

	public void RefreshActiveWindow()
	{
		ShowWindow(activeWindow);
	}

	public EmbarkBuilderModuleWindowDescriptor GetNextEnabledWindowDescriptor(bool editableOnly)
	{
		int num = windowDescriptors.IndexOf(activeWindow);
		for (num++; num < windowDescriptors.Count; num++)
		{
			if (windowDescriptors[num].enabled && (!editableOnly || windowDescriptors[num].module.shouldBeEditable()))
			{
				return windowDescriptors[num];
			}
		}
		return null;
	}

	public EmbarkBuilderModuleWindowDescriptor GetWindowDescriptorByViewID(string id)
	{
		int num = windowDescriptors.IndexOf(activeWindow);
		for (num++; num < windowDescriptors.Count; num++)
		{
			if (windowDescriptors[num].viewID == id)
			{
				return windowDescriptors[num];
			}
		}
		return null;
	}

	public void advanceToSummary()
	{
		EmbarkBuilderModuleWindowDescriptor embarkBuilderModuleWindowDescriptor = activeWindow;
		do
		{
			advance();
			if (activeWindow != embarkBuilderModuleWindowDescriptor)
			{
				embarkBuilderModuleWindowDescriptor = activeWindow;
				continue;
			}
			break;
		}
		while (activeWindow?.viewID != "Chargen/BuildSummary");
	}

	/// <summary>
	///                 Checks the state of the current window for data errors and warnings, potentially showing popup messages.
	///                 In the event of errors, it will show a dialog box, wait for dismissal and return false.
	///                 In the event of warnings, it will ask the player to confirm advancing the screen, returning true or false.
	///                 If no errors or warnings, returns true.
	///             </summary><returns>If the current window is "ready" to advance</returns>
	public async Task<bool> checkStateAsync()
	{
		string text = activeWindow?.getWindow()?.DataErrors();
		if (text != null)
		{
			await Popup.NewPopupMessageAsync(text, PopupMessage.SingleButton, null, "{{r|Error!}}");
			return false;
		}
		string text2 = activeWindow?.getWindow()?.DataWarnings();
		if (text2 != null && (await Popup.NewPopupMessageAsync(text2 + "\n\nContinue anyway?", PopupMessage.YesNoButton, null, "{{W|Warning!}}")).command != "Yes")
		{
			return false;
		}
		return true;
	}

	public async void advance(bool force = false, bool editableOnly = false)
	{
		if (force || await checkStateAsync())
		{
			EmbarkBuilderModuleWindowDescriptor nextEnabledWindowDescriptor = GetNextEnabledWindowDescriptor(editableOnly);
			if (nextEnabledWindowDescriptor == null)
			{
				exitWithInfo();
			}
			else
			{
				ShowWindow(nextEnabledWindowDescriptor);
			}
		}
	}

	public void advanceToViewId(string viewid)
	{
		EmbarkBuilderModuleWindowDescriptor windowDescriptorByViewID = GetWindowDescriptorByViewID(viewid);
		if (windowDescriptorByViewID == null)
		{
			exitWithInfo();
		}
		else
		{
			ShowWindow(windowDescriptorByViewID);
		}
	}

	public void back()
	{
		int num = windowDescriptors.IndexOf(activeWindow);
		for (num--; num >= 0; num--)
		{
			if (windowDescriptors[num].enabled && windowDescriptors[num].module.shouldBeEditable())
			{
				ShowWindow(windowDescriptors[num]);
				return;
			}
		}
		exitWithoutInfo();
	}

	public void exitWithInfo()
	{
		Debug.Log("exiting with info");
		try
		{
			File.WriteAllText(DataManager.SavePath("lastcharacter.txt"), generateCode());
			ShowWindow(null);
			foreach (AbstractBuilderModuleWindowBase createdWindow in createdWindows)
			{
				createdWindow.DestroyWindow();
			}
			createdWindows.Clear();
			embarkInfo.modules.AddRange(modules.Where((AbstractEmbarkBuilderModule m) => m.enabled));
			embarkInfo.modules.ForEach(delegate(AbstractEmbarkBuilderModule m)
			{
				AbstractEmbarkBuilderModuleData data = m.getData();
				if (data != null)
				{
					embarkInfo._data.Add(data);
				}
			});
		}
		finally
		{
			finishedEvent.TrySetResult(embarkInfo);
			this.Destroy();
		}
	}

	public void exitWithoutInfo()
	{
		try
		{
			ShowWindow(null);
			foreach (AbstractBuilderModuleWindowBase createdWindow in createdWindows)
			{
				createdWindow.DestroyWindow();
			}
			createdWindows.Clear();
			Debug.Log("exiting without info");
			embarkInfo = null;
		}
		finally
		{
			finishedEvent.TrySetResult(null);
			this.Destroy();
		}
		if (GameManager.Instance.PrereleaseInput)
		{
			GameManager.Instance.skipAnInput = true;
		}
	}

	public void Update()
	{
	}

	public T GetModule<T>() where T : AbstractEmbarkBuilderModule
	{
		return modules.Find((AbstractEmbarkBuilderModule m) => m.GetType() == typeof(T)) as T;
	}

	public EmbarkBuilderOverlayWindow GetOverlayWindow()
	{
		return UIManager.getWindow<EmbarkBuilderOverlayWindow>("Chargen/Overlay");
	}

	public AbstractBuilderModuleWindowBase createWindow(string viewID, Type windowType)
	{
		Transform parent = GameObject.Find("UI Manager/Chargen").transform;
		AbstractBuilderModuleWindowBase abstractBuilderModuleWindowBase = UIManager.createWindow(viewID, windowType, parent) as AbstractBuilderModuleWindowBase;
		createdWindows.Add(abstractBuilderModuleWindowBase);
		return abstractBuilderModuleWindowBase;
	}

	public IEnumerable<UIBreadcrumb> GetBreadcrumbs()
	{
		foreach (EmbarkBuilderModuleWindowDescriptor windowDescriptor in windowDescriptors)
		{
			if (windowDescriptor.enabled && windowDescriptor.module.shouldBeEditable())
			{
				UIBreadcrumb breadcrumb = windowDescriptor.window.GetBreadcrumb();
				if (breadcrumb != null)
				{
					yield return breadcrumb;
				}
			}
		}
	}

	public async void ShowFromCrumb(FrameworkDataElement choice)
	{
		bool isBeforeActive = true;
		foreach (EmbarkBuilderModuleWindowDescriptor desc in windowDescriptors)
		{
			if (desc == activeWindow)
			{
				isBeforeActive = false;
			}
			if (desc.window.GetBreadcrumb()?.Id == choice.Id)
			{
				if (desc == activeWindow)
				{
					return;
				}
				if (isBeforeActive)
				{
					ShowWindow(desc);
				}
				else if (await checkStateAsync())
				{
					advanceToViewId(desc.viewID);
				}
			}
		}
	}

	public void fireBootEvent(string id)
	{
		modules.ForEach(delegate(AbstractEmbarkBuilderModule module)
		{
			if (module.enabled)
			{
				module.handleBootEvent(id, null, info);
			}
		});
	}

	public T fireBootEvent<T>(string id, T element)
	{
		modules?.ForEach(delegate(AbstractEmbarkBuilderModule module)
		{
			if (module != null && module.enabled)
			{
				element = (T)(module?.handleBootEvent(id, null, info, element));
			}
		});
		return element;
	}

	public bool IsEditableGameMode()
	{
		try
		{
			return (bool)handleUIEvent(EventNames.EditableGameModeQuery, true);
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Error while processing EditableGameMode", x);
			return true;
		}
	}

	private void InitModules(bool silent = false)
	{
		EmbarkBuilderConfiguration.Init();
		modules = new List<AbstractEmbarkBuilderModule>();
		windowDescriptors = new List<EmbarkBuilderModuleWindowDescriptor>();
		windows = new List<AbstractBuilderModuleWindowBase>();
		modules.AddRange(EmbarkBuilderConfiguration.activeModules);
		foreach (AbstractEmbarkBuilderModule module in modules)
		{
			module.builder = this;
			if (module.shouldBeEnabled())
			{
				module.enable();
			}
			else
			{
				module.disable();
			}
		}
		foreach (AbstractEmbarkBuilderModule module2 in modules)
		{
			module2.Init();
		}
		if (silent)
		{
			return;
		}
		foreach (AbstractEmbarkBuilderModule module3 in modules)
		{
			module3.assembleWindowDescriptors(windowDescriptors);
		}
		windowDescriptors.ForEach(delegate(EmbarkBuilderModuleWindowDescriptor descriptor)
		{
			AbstractBuilderModuleWindowBase window = descriptor.getWindow();
			descriptor.windowInit(window);
		});
		ShowWindow(windowDescriptors.First());
	}

	public void NotifyModuleChanges(AbstractEmbarkBuilderModule module, AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
		foreach (AbstractEmbarkBuilderModule module2 in modules)
		{
			if (module2 != module)
			{
				module2.handleModuleDataChange(module, oldValues, newValues);
			}
			bool flag = module2.shouldBeEnabled();
			if (flag != module2.enabled)
			{
				if (flag)
				{
					module2.enable();
				}
				else
				{
					module2.disable();
				}
			}
		}
		if ((activeWindow?.getWindow()?.UseOverlay()).GetValueOrDefault())
		{
			GetOverlayWindow().UpdateBreadcrumbs();
		}
	}

	public static EmbarkBuilder FromCode(string code, bool silent = false)
	{
		EmbarkBuilder embarkBuilder = new EmbarkBuilder();
		embarkBuilder.InitModulesFromCode(code, silent);
		return embarkBuilder;
	}

	public void InitModulesFromCode(string code, bool silent = false)
	{
		if (modules == null)
		{
			InitModules(silent);
		}
		CodeCompressor.loadCode(code, modules, silent);
	}

	public void ResetForwardViews()
	{
		foreach (EmbarkBuilderModuleWindowDescriptor item in windowDescriptors.SkipWhile((EmbarkBuilderModuleWindowDescriptor desc) => desc != activeWindow).Skip(1))
		{
			item.getWindow().ResetSelection();
		}
	}
}

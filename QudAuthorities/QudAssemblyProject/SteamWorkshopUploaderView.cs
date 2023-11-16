using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using QupKit;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using XRL;

public class SteamWorkshopUploaderView : BaseView
{
	private ModScrollerController modScrollerController;

	private ModInfo currentMod;

	private Action UpdateAction;

	protected CallResult<CreateItemResult_t> m_itemCreated;

	protected CallResult<SubmitItemUpdateResult_t> m_itemSubmitted;

	private UGCUpdateHandle_t currentHandle = UGCUpdateHandle_t.Invalid;

	public bool bFilter;

	public static void OpenInWinFileBrowser(string path)
	{
		bool flag = false;
		string text = path.Replace("/", "\\");
		if (Directory.Exists(text))
		{
			flag = true;
		}
		try
		{
			Process.Start("explorer.exe", (flag ? "/root," : "/select,") + text);
		}
		catch (Win32Exception ex)
		{
			ex.HelpLink = "";
		}
	}

	public void ShowProgress(string Text)
	{
		base.rootObject.transform.Find("ProgressPanel").gameObject.SetActive(value: true);
		base.rootObject.transform.Find("ProgressPanel/ProgressLabel").GetComponent<Text>().text = Text;
	}

	public void SetProgress(string Text, float Progress)
	{
		base.rootObject.transform.Find("ProgressPanel/ProgressLabel").GetComponent<Text>().text = Text;
	}

	public void ClearProgress()
	{
		base.rootObject.transform.Find("ProgressPanel").gameObject.SetActive(value: false);
	}

	public void SetModInfo(ModInfo info)
	{
		currentMod = info;
		if (info.WorkshopInfo == null)
		{
			base.rootObject.transform.Find("CreatePanel").gameObject.SetActive(value: true);
			base.rootObject.transform.Find("DetailsPanel").gameObject.SetActive(value: false);
		}
		else
		{
			base.rootObject.transform.Find("CreatePanel").gameObject.SetActive(value: false);
			base.rootObject.transform.Find("DetailsPanel").gameObject.SetActive(value: true);
			if (currentMod.WorkshopInfo.Title != null)
			{
				base.rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/TitleField").GetComponent<InputField>().text = currentMod.WorkshopInfo.Title;
			}
			if (currentMod.WorkshopInfo.Description != null)
			{
				base.rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/DescriptionField").GetComponent<InputField>().text = currentMod.WorkshopInfo.Description;
			}
			if (currentMod.WorkshopInfo.Tags != null)
			{
				base.rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/TagsField").GetComponent<InputField>().text = currentMod.WorkshopInfo.Tags;
			}
			if (!string.IsNullOrEmpty(currentMod.WorkshopInfo.Visibility))
			{
				base.rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/Visibility").GetComponent<Dropdown>().value = Convert.ToInt32(currentMod.WorkshopInfo.Visibility);
			}
		}
		base.rootObject.transform.Find("SelectedModLabel").GetComponent<Text>().text = "Managing - " + info.ID;
		UpdatePreview();
	}

	public override void Enter()
	{
		base.Enter();
		GetChild("Mod Scroller").Select();
		modScrollerController = GameObject.Find("ModScrollerController").GetComponent<ModScrollerController>();
		modScrollerController.Refresh();
		if (m_itemCreated == null)
		{
			m_itemCreated = CallResult<CreateItemResult_t>.Create(OnItemCreated);
			m_itemSubmitted = CallResult<SubmitItemUpdateResult_t>.Create(OnItemSubmitted);
		}
	}

	public void Popup(string text)
	{
		base.rootObject.transform.Find("PopupPanel").gameObject.SetActive(value: true);
		base.rootObject.transform.Find("PopupPanel/Panel/PopupLabel").GetComponent<Text>().text = text;
	}

	private void OnItemSubmitted(SubmitItemUpdateResult_t callback, bool ioFailure)
	{
		if (ioFailure)
		{
			Popup("Error: I/O Failure! :(");
		}
		else if (callback.m_eResult == EResult.k_EResultOK)
		{
			Popup("SUCCESS! Item submitted!");
			ClearProgress();
		}
		else
		{
			Popup("Unknown result: " + callback.m_eResult);
		}
	}

	private void OnItemCreated(CreateItemResult_t callback, bool ioFailure)
	{
		ClearProgress();
		if (ioFailure)
		{
			Popup("Error: I/O Failure!");
			return;
		}
		switch (callback.m_eResult)
		{
		case EResult.k_EResultInsufficientPrivilege:
			Popup("Error: Unfortunately, you're banned by the community from uploading to the workshop! Bummer.");
			return;
		case EResult.k_EResultTimeout:
			Popup("Error: Timeout");
			return;
		case EResult.k_EResultNotLoggedOn:
			Popup("Error: You're not logged into Steam!");
			return;
		}
		if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
			Application.OpenURL("https://steamcommunity.com/workshop/workshoplegalagreement/");
		}
		if (callback.m_eResult == EResult.k_EResultOK)
		{
			Popup("Item creation successful! Published Item ID: " + callback.m_nPublishedFileId.ToString());
			UnityEngine.Debug.Log("Item created: Id: " + callback.m_nPublishedFileId.ToString());
			currentMod.InitializeWorkshopInfo(callback.m_nPublishedFileId.m_PublishedFileId);
			SetModInfo(currentMod);
		}
	}

	public override void Update()
	{
		if (UpdateAction != null)
		{
			UpdateAction();
			UpdateAction = null;
		}
		if (bFilter && !PickFileView.IsShowing())
		{
			bFilter = false;
		}
		else if (currentHandle != UGCUpdateHandle_t.Invalid)
		{
			ulong punBytesProcessed;
			ulong punBytesTotal;
			EItemUpdateStatus itemUpdateProgress = SteamUGC.GetItemUpdateProgress(currentHandle, out punBytesProcessed, out punBytesTotal);
			float progress = (float)punBytesProcessed / (float)punBytesTotal;
			switch (itemUpdateProgress)
			{
			case EItemUpdateStatus.k_EItemUpdateStatusCommittingChanges:
				SetProgress("Committing changes...", progress);
				break;
			case EItemUpdateStatus.k_EItemUpdateStatusInvalid:
				SetProgress("Item invalid ... dunno why! :(", progress);
				break;
			case EItemUpdateStatus.k_EItemUpdateStatusUploadingPreviewFile:
				SetProgress("Uploading preview image...", progress);
				break;
			case EItemUpdateStatus.k_EItemUpdateStatusUploadingContent:
				SetProgress("Uploading content...", progress);
				break;
			case EItemUpdateStatus.k_EItemUpdateStatusPreparingConfig:
				SetProgress("Preparing configuration...", progress);
				break;
			case EItemUpdateStatus.k_EItemUpdateStatusPreparingContent:
				SetProgress("Preparing content...", progress);
				break;
			}
		}
	}

	public void SubmitCurrentMod()
	{
		try
		{
			ShowProgress("Submitting update... Please wait.");
			int value = base.rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/Visibility").GetComponent<Dropdown>().value;
			currentMod.WorkshopInfo.Title = base.rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/TitleField").GetComponent<InputField>().text;
			currentMod.WorkshopInfo.Description = base.rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/DescriptionField").GetComponent<InputField>().text;
			currentMod.WorkshopInfo.Tags = base.rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/TagsField").GetComponent<InputField>().text;
			currentMod.WorkshopInfo.Visibility = value.ToString();
			currentMod.SaveWorkshopInfo();
			ulong workshopId = currentMod.WorkshopInfo.WorkshopId;
			UGCUpdateHandle_t uGCUpdateHandle_t = SteamUGC.StartItemUpdate(nPublishedFileID: new PublishedFileId_t(workshopId), nConsumerAppId: new AppId_t(SteamManager.AppID));
			SteamUGC.SetItemTitle(uGCUpdateHandle_t, currentMod.WorkshopInfo.Title);
			SteamUGC.SetItemDescription(uGCUpdateHandle_t, currentMod.WorkshopInfo.Description);
			SteamUGC.SetItemVisibility(uGCUpdateHandle_t, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate);
			if (value == 0)
			{
				SteamUGC.SetItemVisibility(uGCUpdateHandle_t, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate);
			}
			if (value == 1)
			{
				SteamUGC.SetItemVisibility(uGCUpdateHandle_t, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityFriendsOnly);
			}
			if (value == 2)
			{
				SteamUGC.SetItemVisibility(uGCUpdateHandle_t, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);
			}
			SteamUGC.SetItemContent(uGCUpdateHandle_t, currentMod.Path);
			if (!string.IsNullOrEmpty(currentMod.WorkshopInfo.ImagePath))
			{
				SteamUGC.SetItemPreview(uGCUpdateHandle_t, Path.Combine(currentMod.Path, currentMod.WorkshopInfo.ImagePath));
			}
			SteamUGC.SetItemTags(uGCUpdateHandle_t, currentMod.WorkshopInfo.Tags.Split(','));
			currentHandle = uGCUpdateHandle_t;
			SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(uGCUpdateHandle_t, base.rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/ChangelistField").GetComponent<InputField>().text);
			m_itemSubmitted.Set(hAPICall);
		}
		catch (Exception ex)
		{
			ClearProgress();
			Popup(ex.ToString());
		}
	}

	public void UpdatePreview()
	{
		if (currentMod.WorkshopInfo != null && !string.IsNullOrEmpty(currentMod.WorkshopInfo.ImagePath))
		{
			byte[] data = File.ReadAllBytes(Path.Combine(currentMod.Path, "preview.png"));
			Texture2D texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(data);
			base.rootObject.transform.Find("DetailsPanel/Scroll View/Viewport/Content/Image").GetComponent<Image>().sprite = UnityEngine.Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		}
	}

	public void SetImage(string Path)
	{
		try
		{
			File.Copy(Path, System.IO.Path.Combine(currentMod.Path, "preview.png"), overwrite: true);
			currentMod.WorkshopInfo.ImagePath = "preview.png";
			UpdatePreview();
			currentMod.SaveWorkshopInfo();
		}
		catch (Exception ex)
		{
			Popup(ex.ToString());
		}
	}

	public override void OnCommand(string Command)
	{
		if (PickFileView.IsShowing())
		{
			bFilter = true;
			return;
		}
		switch (Command)
		{
		case "SelectImage":
			if (currentMod != null && currentMod.WorkshopInfo != null)
			{
				PickFileView.Show(delegate(string s)
				{
					SetImage(s);
				}, FileBrowser.PickFileModes.Select, "*.png");
			}
			return;
		case "UploadContent":
			if (currentMod != null && currentMod.WorkshopInfo != null)
			{
				SubmitCurrentMod();
			}
			return;
		case "CreateWorkshopItem":
			if (currentMod != null && currentMod.WorkshopInfo == null)
			{
				try
				{
					ShowProgress("Requesting a new Steam Workshop item... Please wait.");
					SteamAPICall_t hAPICall = SteamUGC.CreateItem(new AppId_t(SteamManager.AppID), EWorkshopFileType.k_EWorkshopFileTypeFirst);
					m_itemCreated.Set(hAPICall);
					return;
				}
				catch
				{
					ClearProgress();
					return;
				}
			}
			return;
		case "Back":
			LegacyViewManager.Instance.SetActiveView("MainMenu");
			break;
		}
		if (Command == "RefreshMods")
		{
			modScrollerController.Refresh();
		}
		if (Command == "BrowseMods")
		{
			if (Directory.Exists(Path.Combine(Application.persistentDataPath, "Mods")))
			{
				OpenInWinFileBrowser(Path.Combine(Application.persistentDataPath, "Mods"));
			}
			else
			{
				OpenInWinFileBrowser(Application.persistentDataPath);
			}
		}
	}
}

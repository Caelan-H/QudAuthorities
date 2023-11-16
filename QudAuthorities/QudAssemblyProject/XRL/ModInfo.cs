using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using HarmonyLib;
using Kobold;
using Qud.UI;
using UnityEngine;
using XRL.Rules;
using XRL.UI;

namespace XRL;

[HasModSensitiveStaticCache]
public class ModInfo
{
	public string Path;

	/// <summary>A unique identifier for this mod.</summary><seealso cref="M:XRL.ModManager.RegisterMod(XRL.ModInfo)" />
	public string ID = "";

	public ModSource Source;

	/// <inheritdoc cref="F:XRL.ModInfo.Path" />
	public DirectoryInfo Directory;

	public Assembly Assembly;

	/// <summary>A harmony instance using <see cref="F:XRL.ModInfo.ID" />.</summary>
	public Harmony Harmony;

	public List<FileInfo> ScriptFiles = new List<FileInfo>();

	/// <summary>A list of all visible files within <see cref="F:XRL.ModInfo.Directory" /> that have an <c>xml</c> extension.</summary>
	public List<FileInfo> XMLFiles = new List<FileInfo>();

	public int LoadPriority;

	/// <value><c>true</c> if this mod contains any <see cref="F:XRL.ModInfo.ScriptFiles" />; otherwise, <c>false</c>.</value>
	public bool IsScripting;

	public bool IsApproved;

	/// <summary>The total size in bytes of all visible files within <see cref="F:XRL.ModInfo.Directory" />.</summary><seealso cref="M:XRL.ModInfo.CheckApproval" />
	public long Size;

	public ModSettings Settings;

	/// <summary>A manifest.json read from the mod's root <see cref="F:XRL.ModInfo.Directory" />.</summary><seealso cref="M:XRL.ModInfo.ReadConfigurations" />
	public ModManifest Manifest = new ModManifest();

	public SteamWorkshopInfo WorkshopInfo;

	/// <summary>A modconfig.json read from the mod's root <see cref="F:XRL.ModInfo.Directory" />.</summary><remarks>This might be obsoleted in the future with a per-file texture configuration like <c>"cute_snapjaw.png.cfg"</c>.</remarks><seealso cref="M:XRL.ModInfo.ReadConfigurations" />
	public TextureConfiguration TextureConfiguration = new TextureConfiguration();

	[ModSensitiveStaticCache(true)]
	private static Dictionary<string, Sprite> spriteByPath = new Dictionary<string, Sprite>();

	public bool IsEnabled
	{
		get
		{
			return State == ModState.Enabled;
		}
		set
		{
			Settings.Enabled = value;
		}
	}

	/// <summary>
	///             An enum representation of the mod's current state: lacks approval, failed to compile, enabled, disabled.
	///             </summary>
	public ModState State
	{
		get
		{
			if (IsScripting && !Options.AllowCSMods)
			{
				return ModState.Disabled;
			}
			if (!Settings.Enabled)
			{
				return ModState.Disabled;
			}
			if (!IsApproved)
			{
				return ModState.NeedsApproval;
			}
			if (Settings.Failed)
			{
				return ModState.Failed;
			}
			return ModState.Enabled;
		}
	}

	public string DisplayTitleStripped => ConsoleLib.Console.ColorUtility.StripFormatting(DisplayTitle);

	/// <summary>
	///             A display title for the <see cref="T:Qud.UI.ModManagerUI" />.
	///             </summary>
	public string DisplayTitle
	{
		get
		{
			if (!string.IsNullOrEmpty(Manifest.Title))
			{
				return Manifest.Title;
			}
			return ID;
		}
	}

	public ModInfo(string Path, string ID = null, ModSource Source = ModSource.Unknown, bool Initialize = false)
	{
		this.Path = Path;
		Directory = new DirectoryInfo(Path);
		this.ID = ID;
		this.Source = Source;
		if (Initialize)
		{
			this.Initialize();
		}
	}

	public void Initialize()
	{
		if (Directory.Exists)
		{
			ReadConfigurations();
			LoadSettings();
			CheckFiles();
			IsScripting = ScriptFiles.Count > 0;
			IsApproved = CheckApproval();
			if (IsEnabled)
			{
				Settings.Errors.Clear();
				Settings.Warnings.Clear();
			}
		}
	}

	/// <summary>
	///             Read mod root configuration files: manifest.json, config.json, workshop.json, modconfig.json.
	///             </summary>
	public void ReadConfigurations()
	{
		foreach (FileInfo item in Directory.EnumerateFiles())
		{
			try
			{
				ReadConfiguration(item);
			}
			catch (Exception msg)
			{
				Error(msg);
			}
		}
		ID = Regex.Replace(Manifest.ID ?? ID, "[^\\w ]", "");
		LoadPriority = Manifest.LoadOrder;
	}

	private void ReadConfiguration(FileInfo File)
	{
		switch (File.Name.ToLower())
		{
		case "manifest.json":
			ModManager.JsonSerializer.Populate(File.FullName, Manifest);
			break;
		case "config.json":
		{
			ModManifest modManifest = ModManager.JsonSerializer.Deserialize<ModManifest>(File.FullName);
			if (Manifest.ID == null)
			{
				Manifest.ID = modManifest.ID;
			}
			if (Manifest.LoadOrder == 0)
			{
				Manifest.LoadOrder = modManifest.LoadOrder;
			}
			Warn("Mod using config.json, please convert to manifest.json and check out https://wiki.cavesofqud.com/Modding:Overview for other options to set");
			break;
		}
		case "workshop.json":
			WorkshopInfo = ModManager.JsonSerializer.Deserialize<SteamWorkshopInfo>(File.FullName);
			if (Manifest.Tags == null)
			{
				Manifest.Tags = WorkshopInfo.Tags;
			}
			if (Manifest.PreviewImage == null)
			{
				Manifest.PreviewImage = WorkshopInfo.ImagePath;
			}
			if (Manifest.Title == null && WorkshopInfo.Title != null)
			{
				Manifest.Title = ConsoleLib.Console.ColorUtility.EscapeFormatting(WorkshopInfo.Title);
			}
			break;
		case "modconfig.json":
			ModManager.JsonSerializer.Populate(File.FullName, TextureConfiguration);
			break;
		}
	}

	public void LoadSettings()
	{
		if (!ModManager.ModSettingsMap.TryGetValue(ID, out Settings))
		{
			Settings = new ModSettings();
			ModManager.ModSettingsMap[ID] = Settings;
		}
		Settings.Title = DisplayTitleStripped;
	}

	/// <summary>
	///             Check for script and XML files within the mod directory and sort them into their respective lists.
	///             </summary>
	public void CheckFiles()
	{
		foreach (FileInfo item in EnumerateAllFiles())
		{
			Size += item.Length;
			string text = item.Name.ToLower();
			if (text.EndsWith(".cs"))
			{
				ScriptFiles.Add(item);
			}
			else if (text.EndsWith(".xml"))
			{
				XMLFiles.Add(item);
			}
		}
		Comparison<FileInfo> comparison = (FileInfo a, FileInfo b) => a.FullName.CompareTo(b.FullName);
		ScriptFiles.Sort(comparison);
		XMLFiles.Sort(comparison);
	}

	public bool CheckApproval()
	{
		if (!IsScripting)
		{
			return true;
		}
		if (!Options.ApproveCSMods)
		{
			return true;
		}
		if (Settings.FilesHash == null)
		{
			return false;
		}
		if (Settings.SourceHash == null)
		{
			return false;
		}
		string text = Settings.CalcFilesHash(ScriptFiles.Concat(XMLFiles), Path);
		if (Settings.FilesHash != text)
		{
			return false;
		}
		text = Settings.CalcSourceHash(ScriptFiles.Concat(XMLFiles));
		if (Settings.SourceHash != text)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	///             Approve the mod and update hashes with current mod data.
	///             </summary>
	public void Approve()
	{
		Settings.FilesHash = Settings.CalcFilesHash(ScriptFiles.Concat(XMLFiles), Path);
		Settings.SourceHash = Settings.CalcSourceHash(ScriptFiles.Concat(XMLFiles));
		Settings.Failed = false;
		IsApproved = true;
	}

	public void ConfirmFailure()
	{
		int count = Settings.Errors.Count;
		if (count > 0)
		{
			LogToClipboard();
			string title = DisplayTitle + " - {{R|Errors}}";
			string text = string.Join("\n", Settings.Errors.Take(3));
			if (count > 3)
			{
				text = text + "\n(... {{R|+" + (count - 3) + "}} more)";
			}
			text = text + "\n\nAutomatically on your clipboard should you wish to forward it to " + (Manifest.Author ?? "the mod author") + ".";
			List<QudMenuItem> list = new List<QudMenuItem>(PopupMessage.CancelButton);
			list.Add(new QudMenuItem
			{
				text = "{{W|[R]}} {{y|Retry}}",
				command = "retry",
				hotkey = "R"
			});
			if (WorkshopInfo != null)
			{
				list.Add(new QudMenuItem
				{
					text = "{{W|[W]}} {{y|Workshop}}",
					command = "workshop",
					hotkey = "W"
				});
			}
			Popup.WaitNewPopupMessage(text, list, delegate(QudMenuItem i)
			{
				if (i.command == "workshop")
				{
					WorkshopInfo?.OpenWorkshopPage();
				}
				else if (i.command == "retry")
				{
					Settings.Failed = false;
				}
			}, null, title);
		}
		else
		{
			Settings.Failed = false;
		}
	}

	/// <summary>
	///             Write mod error and warning messages to the clipboard.
	///             </summary>
	public void LogToClipboard()
	{
		StringBuilder stringBuilder = Strings.SB.Clear().Append("=== ").Append(DisplayTitleStripped);
		if (!Manifest.Version.IsNullOrEmpty())
		{
			stringBuilder.Append(" ").Append(Manifest.Version);
		}
		stringBuilder.Append(" Errors ===\n");
		if (Settings.Errors.Any())
		{
			stringBuilder.AppendRange(Settings.Errors, "\n");
		}
		else
		{
			stringBuilder.Append("None");
		}
		stringBuilder.Append("\n== Warnings ==\n");
		if (Settings.Warnings.Any())
		{
			stringBuilder.AppendRange(Settings.Warnings, "\n");
		}
		else
		{
			stringBuilder.Append("None");
		}
		ClipboardHelper.SetClipboardData(stringBuilder.ToString());
	}

	public Sprite GetDefaultSprite()
	{
		return SpriteManager.GetUnitySprite("Text/0.bmp");
	}

	public Sprite GetSprite()
	{
		string text = Manifest.PreviewImage ?? WorkshopInfo?.ImagePath;
		if (string.IsNullOrEmpty(text))
		{
			return GetDefaultSprite();
		}
		string text2 = System.IO.Path.Combine(Path, text);
		if (!text2.StartsWith(Path))
		{
			return GetDefaultSprite();
		}
		Sprite value = null;
		if (spriteByPath.TryGetValue(text2, out value))
		{
			return value;
		}
		Texture2D texture2D = null;
		if (File.Exists(text2))
		{
			byte[] data = File.ReadAllBytes(text2);
			texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
			texture2D.LoadImage(data);
			texture2D.filterMode = UnityEngine.FilterMode.Trilinear;
		}
		if (texture2D != null)
		{
			value = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f));
		}
		spriteByPath.Add(text2, value);
		return value;
	}

	/// <summary>
	///             Enumerate files in directory and subdirectories, excluding hidden entries.
	///             </summary><param name="Directory">The directory to enumerate through, defaults to the current mod directory.</param>
	public IEnumerable<FileInfo> EnumerateAllFiles(DirectoryInfo Directory = null)
	{
		if (Directory == null)
		{
			Directory = this.Directory;
		}
		foreach (FileSystemInfo item in Directory.EnumerateFileSystemInfos())
		{
			if ((item.Attributes & FileAttributes.Hidden) > (FileAttributes)0)
			{
				continue;
			}
			if ((item.Attributes & FileAttributes.Directory) > (FileAttributes)0)
			{
				foreach (FileInfo item2 in EnumerateAllFiles((DirectoryInfo)item))
				{
					yield return item2;
				}
			}
			else
			{
				yield return (FileInfo)item;
			}
		}
	}

	public void Warn(object msg)
	{
		MetricsManager.LogModWarning(this, msg);
	}

	/// <summary>
	///             Log an error with mod context.
	///             </summary><remarks>Also added to <see cref="F:XRL.ModSettings.Errors" />.</remarks>
	public void Error(object msg)
	{
		MetricsManager.LogModError(this, msg);
	}

	public void ApplyHarmonyPatches()
	{
		try
		{
			if (!(Assembly == null) && Assembly.GetTypes().Any((Type x) => x.IsDefined(typeof(HarmonyAttribute), inherit: true)))
			{
				Logger.buildLog.Info("Applying Harmony patches...");
				Harmony = Harmony ?? new Harmony(ID);
				Harmony.PatchAll(Assembly);
				Logger.buildLog.Info("Success :)");
			}
		}
		catch (Exception ex)
		{
			Error("Exception applying harmony patches: " + ex);
			Logger.buildLog.Info("Failure :(");
		}
	}

	/// <summary>
	///             Unapply harmony patches using this mod's <see cref="F:XRL.ModInfo.ID" />.
	///             </summary>
	public void UnapplyHarmonyPatches()
	{
		try
		{
			if (Harmony.GetPatchedMethods().Any())
			{
				Logger.buildLog.Info("Unapplying Harmony patches...");
				Harmony.UnpatchAll(Harmony.Id);
			}
		}
		catch (Exception ex)
		{
			Error("Exception unapplying harmony patches: " + ex);
		}
	}

	public void InitializeWorkshopInfo(ulong PublishedFileId)
	{
		WorkshopInfo = new SteamWorkshopInfo();
		WorkshopInfo.WorkshopId = PublishedFileId;
		SaveWorkshopInfo();
	}

	/// <summary>
	///             Save workshop.json to mod directory.
	///             </summary>
	public void SaveWorkshopInfo()
	{
		if (WorkshopInfo != null)
		{
			ModManager.JsonSerializer.Serialize(System.IO.Path.Combine(Path, "workshop.json"), WorkshopInfo);
		}
	}
}

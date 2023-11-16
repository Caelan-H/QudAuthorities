using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using Newtonsoft.Json;
using RoslynCSharp;
using RoslynCSharp.Compiler;
using Steamworks;
using XRL.Core;
using XRL.UI;

namespace XRL;

[HasModSensitiveStaticCache]
public static class ModManager
{
	public static List<ModInfo> Mods = null;

	public static Dictionary<string, ModInfo> ModMap = null;

	public static Dictionary<string, ModSettings> ModSettingsMap = null;

	public static bool Compiled = false;

	public static JsonSerializer JsonSerializer = new JsonSerializer
	{
		Formatting = Formatting.Indented,
		NullValueHandling = NullValueHandling.Ignore
	};

	[ModSensitiveStaticCache(true)]
	private static Dictionary<Type, List<FieldInfo>> _fieldsWithAttribute = new Dictionary<Type, List<FieldInfo>>();

	[ModSensitiveStaticCache(true)]
	private static Dictionary<Type, List<MethodInfo>> _methodsWithAttribute = new Dictionary<Type, List<MethodInfo>>();

	[ModSensitiveStaticCache(true)]
	private static Dictionary<string, Type> _typeResolutions = new Dictionary<string, Type>();

	public static Dictionary<Type, string> typeNames = new Dictionary<Type, string>();

	private static Harmony harmony = new Harmony("com.freeholdgames.cavesofqud");

	public const BindingFlags ATTRIBUTE_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	public static IEnumerable<Type> ActiveTypes
	{
		get
		{
			foreach (Assembly activeAssembly in ActiveAssemblies)
			{
				Type[] types = activeAssembly.GetTypes();
				for (int i = 0; i < types.Length; i++)
				{
					yield return types[i];
				}
			}
		}
	}

	/// <summary>
	///             Yields the current executing assembly followed by any enabled script mod assemblies in priority order.
	///             </summary>
	public static IEnumerable<Assembly> ActiveAssemblies
	{
		get
		{
			yield return Assembly.GetExecutingAssembly();
			foreach (Assembly modAssembly in ModAssemblies)
			{
				yield return modAssembly;
			}
		}
	}

	public static IEnumerable<Assembly> ModAssemblies
	{
		get
		{
			Init();
			foreach (ModInfo mod in Mods)
			{
				if (mod.IsEnabled && !(mod.Assembly == null))
				{
					yield return mod.Assembly;
				}
			}
		}
	}

	/// <summary>
	///             Register mod in the manager if a mod with that ID has not already been registered.
	///             </summary><returns><c>true</c> if the mod was successfully registered; otherwise, <c>false</c>.</returns>
	public static bool RegisterMod(ModInfo Mod)
	{
		if (ModMap.ContainsKey(Mod.ID))
		{
			Mod.Warn("A mod with the ID \"" + Mod.ID + "\" already exists in " + DataManager.SanitizePathForDisplay(ModMap[Mod.ID].Path) + ", skipping.");
			return false;
		}
		ModMap[Mod.ID] = Mod;
		Mods.Add(Mod);
		return true;
	}

	public static bool DoesModDefineType(Type T)
	{
		return ModAssemblies.Contains(T.Assembly);
	}

	public static bool DoesModDefineType(string TypeID)
	{
		if (_typeResolutions.TryGetValue(TypeID, out var value))
		{
			return ModAssemblies.Contains(value.Assembly);
		}
		return ModAssemblies.Any((Assembly x) => x.GetType(TypeID) != null);
	}

	public static Type ResolveType(string TypeID, bool IgnoreCase = false, bool Cache = true)
	{
		Type value = null;
		if (_typeResolutions.TryGetValue(TypeID, out value))
		{
			return value;
		}
		foreach (Assembly modAssembly in ModAssemblies)
		{
			value = modAssembly.GetType(TypeID, throwOnError: false, IgnoreCase);
			if (value != null)
			{
				break;
			}
		}
		if (value == null)
		{
			value = Type.GetType(TypeID, throwOnError: false, IgnoreCase);
		}
		if (!IgnoreCase && Cache)
		{
			_typeResolutions[TypeID] = value;
		}
		return value;
	}

	public static string ResolveTypeName(Type T)
	{
		if (typeNames.TryGetValue(T, out var value))
		{
			return value;
		}
		typeNames.Add(T, T.Name);
		return typeNames[T];
	}

	public static void ResetModSensitiveStaticCaches()
	{
		Type typeFromHandle = typeof(ModSensitiveStaticCacheAttribute);
		Type typeFromHandle2 = typeof(ModSensitiveCacheInitAttribute);
		Type typeFromHandle3 = typeof(HasModSensitiveStaticCacheAttribute);
		foreach (FieldInfo item in GetFieldsWithAttribute(typeFromHandle, typeFromHandle3))
		{
			if (item.IsStatic)
			{
				try
				{
					bool flag = item.FieldType.IsValueType || item.GetCustomAttribute<ModSensitiveStaticCacheAttribute>().CreateEmptyInstance;
					item.SetValue(null, flag ? Activator.CreateInstance(item.FieldType) : null);
				}
				catch (Exception arg)
				{
					MetricsManager.LogAssemblyError(item, $"Error initializing {item.DeclaringType.FullName}.{item.Name}: {arg}");
				}
			}
		}
		foreach (MethodInfo item2 in GetMethodsWithAttribute(typeFromHandle2, typeFromHandle3))
		{
			try
			{
				item2.Invoke(null, new object[0]);
			}
			catch (Exception arg2)
			{
				MetricsManager.LogAssemblyError(item2, $"Error invoking {item2.DeclaringType.FullName}.{item2.Name}: {arg2}");
			}
		}
		XRLCore.Core?.ReloadUIViews();
	}

	public static void CallAfterGameLoaded()
	{
		foreach (MethodInfo item in GetMethodsWithAttribute(typeof(CallAfterGameLoadedAttribute), typeof(HasCallAfterGameLoadedAttribute)))
		{
			try
			{
				item.Invoke(null, new object[0]);
			}
			catch (Exception arg)
			{
				MetricsManager.LogAssemblyError(item, $"Error invoking {item.DeclaringType.FullName}.{item.Name}: {arg}");
			}
		}
	}

	public static void BuildScriptMods()
	{
		if (Thread.CurrentThread == XRLCore.CoreThread)
		{
			MetricsManager.LogInfo("Awaiting script build attempt on UI thread");
			GameManager.Instance.uiQueue.awaitTask(BuildScriptMods);
		}
		else
		{
			Loading.LoadTask("Building script mods", DoBuildScriptMods);
			Loading.LoadTask("Resetting static caches", ResetModSensitiveStaticCaches);
		}
	}

	private static void UnapplyAllHarmonyPatches()
	{
		try
		{
			if (Harmony.GetAllPatchedMethods().Any())
			{
				Logger.buildLog.Info("Unapplying all Harmony patches...");
				harmony.UnpatchAll();
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("unapply harmony patches", x);
		}
	}

	private static bool MainAssemblyPredicate(Assembly assembly)
	{
		if (assembly.IsDynamic)
		{
			return false;
		}
		if (assembly.Location.IsNullOrEmpty())
		{
			return false;
		}
		if (assembly.Location.Contains("ModAssemblies"))
		{
			return false;
		}
		if (assembly.Location.Contains("UIElements"))
		{
			return false;
		}
		if (assembly.Location.Contains("UnityEditor."))
		{
			return false;
		}
		if (assembly.FullName.Contains("ExCSS"))
		{
			return false;
		}
		return true;
	}

	private static bool ScriptModPredicate(ModInfo Mod)
	{
		if (Mod.IsScripting)
		{
			return Mod.IsEnabled;
		}
		return false;
	}

	private static RoslynCSharpCompiler GetCompilerService()
	{
		RoslynCSharpCompiler roslynCompilerService = ScriptDomain.CreateDomain("ModsDomain").RoslynCompilerService;
		roslynCompilerService.GenerateInMemory = !Options.OutputModAssembly;
		roslynCompilerService.OutputDirectory = DataManager.SavePath("ModAssemblies");
		roslynCompilerService.OutputPDBExtension = ".pdb";
		roslynCompilerService.GenerateSymbols = !roslynCompilerService.GenerateInMemory;
		foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies().Where(MainAssemblyPredicate))
		{
			roslynCompilerService.ReferenceAssemblies.Add(AssemblyReference.FromAssembly(item));
		}
		System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
		string text = $"BUILD_{version.Major}_{version.Minor}_{version.Build}";
		roslynCompilerService.DefineSymbols.Add(text);
		Logger.buildLog.Info("Defined symbol: " + text);
		return roslynCompilerService;
	}

	private static void DoBuildScriptMods()
	{
		if (Compiled)
		{
			return;
		}
		Harmony.DEBUG = Options.HarmonyDebug;
		UnapplyAllHarmonyPatches();
		Compiled = true;
		if (!Options.EnableMods || !Options.AllowCSMods || !Mods.Any(ScriptModPredicate))
		{
			return;
		}
		Logger.buildLog.Info("==== BUILDING SCRIPT MODS ====");
		RoslynCSharpCompiler compilerService = GetCompilerService();
		foreach (ModInfo mod in Mods)
		{
			mod.Assembly = null;
			if (!ScriptModPredicate(mod))
			{
				continue;
			}
			try
			{
				string[] array = mod.ScriptFiles.Select((FileInfo x) => x.FullName).ToArray();
				string arg = ((array.Length == 1) ? "file" : "files");
				Logger.buildLog.Info("=== " + mod.DisplayTitleStripped.ToUpper() + " ===");
				Logger.buildLog.Info($"Compiling {array.Length} {arg}...");
				compilerService.OutputName = mod.ID;
				CompilationResult compilationResult = compilerService.CompileFromFiles(array);
				if (compilationResult.Success)
				{
					Logger.buildLog.Info("Success :)");
					mod.Assembly = compilationResult.OutputAssembly;
					mod.Settings.Failed = false;
					string text = "MOD_" + mod.ID.ToUpperInvariant().Replace(" ", "_");
					compilerService.DefineSymbols.Add(text);
					Logger.buildLog.Info("Defined symbol: " + text);
					if (compilationResult.OutputFile.IsNullOrEmpty())
					{
						compilerService.ReferenceAssemblies.Add(AssemblyReference.FromImage(compilationResult.OutputAssemblyImage));
					}
					else
					{
						compilerService.ReferenceAssemblies.Add(AssemblyReference.FromNameOrFile(compilationResult.OutputFile));
						Logger.buildLog.Info("Location: " + compilationResult.OutputFile);
					}
				}
				else
				{
					Logger.buildLog.Info("Failure :(");
					mod.Settings.Failed = true;
				}
				if (compilationResult.ErrorCount > 0)
				{
					Logger.buildLog.Info("== COMPILER ERRORS ==");
					CompilationError[] errors = compilationResult.Errors;
					foreach (CompilationError compilationError in errors)
					{
						if (compilationError.IsError)
						{
							string text2 = DataManager.SanitizePathForDisplay(compilationError.ToString());
							Logger.buildLog.Error(text2);
							mod.Error(text2);
						}
					}
				}
				if (compilationResult.WarningCount > 0)
				{
					Logger.buildLog.Info("== COMPILER WARNINGS ==");
					CompilationError[] errors = compilationResult.Errors;
					foreach (CompilationError compilationError2 in errors)
					{
						if (compilationError2.IsWarning)
						{
							string text3 = DataManager.SanitizePathForDisplay(compilationError2.ToString());
							Logger.buildLog.Info(text3);
							mod.Warn(text3);
						}
					}
				}
				mod.ApplyHarmonyPatches();
			}
			catch (Exception ex)
			{
				mod.Error("Exception compiling mod assembly: " + ex);
				mod.Settings.Failed = mod.Assembly == null;
			}
		}
	}

	public static void Refresh()
	{
		Init(Reload: true);
	}

	public static void RefreshModDirectory(string Path, ModSource Source = ModSource.Local)
	{
		DirectoryInfo directoryInfo = Directory.CreateDirectory(Path);
		if (!directoryInfo.Exists)
		{
			return;
		}
		foreach (DirectoryInfo item in directoryInfo.EnumerateDirectories())
		{
			if ((item.Attributes & FileAttributes.Hidden) <= (FileAttributes)0)
			{
				try
				{
					RegisterMod(new ModInfo(item.FullName, item.Name, Source, Initialize: true));
				}
				catch (Exception x)
				{
					MetricsManager.LogError("Exception reading local mod directory " + item.Name, x);
				}
			}
		}
	}

	private static void RefreshWorkshopSubscriptions()
	{
		if (!SteamManager.Initialized)
		{
			MetricsManager.LogInfo("Skipping workshop subscription info because steam isn't connected");
			return;
		}
		PublishedFileId_t[] array = new PublishedFileId_t[4096];
		uint subscribedItems = SteamUGC.GetSubscribedItems(array, 4096u);
		MetricsManager.LogInfo("Subscribed workshop items: " + subscribedItems);
		for (int i = 0; i < subscribedItems; i++)
		{
			try
			{
				if (SteamUGC.GetItemInstallInfo(array[i], out var _, out var pchFolder, 4096u, out var _))
				{
					if (!Directory.Exists(pchFolder))
					{
						MetricsManager.LogError("Mod directory does not exist: " + pchFolder);
					}
					else
					{
						RegisterMod(new ModInfo(pchFolder, array[i].ToString(), ModSource.Steam, Initialize: true));
					}
				}
			}
			catch (Exception x)
			{
				PublishedFileId_t publishedFileId_t = array[i];
				MetricsManager.LogError("Exception reading workshop mod subscription " + publishedFileId_t.ToString(), x);
			}
		}
	}

	/// <summary>
	///             Read current mod settings from ModSettings.json.
	///             </summary><param name="Reload">Force reload from disk even if current state in memory (used for undo).</param>
	public static void ReadModSettings(bool Reload = false)
	{
		if (ModSettingsMap != null && !Reload)
		{
			return;
		}
		try
		{
			string text = DataManager.SavePath("ModSettings.json");
			if (File.Exists(text))
			{
				ModSettingsMap = JsonSerializer.Deserialize<Dictionary<string, ModSettings>>(text);
			}
			else
			{
				ModSettingsMap = new Dictionary<string, ModSettings>();
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Failed reading ModSettings.json", x);
			ModSettingsMap = ModSettingsMap ?? new Dictionary<string, ModSettings>();
		}
		if (Reload)
		{
			Init(Reload: true);
		}
	}

	public static void WriteModSettings()
	{
		if (ModSettingsMap != null)
		{
			string file = DataManager.SavePath("ModSettings.json");
			JsonSerializer.Serialize(file, ModSettingsMap);
		}
	}

	public static int SortModInfo(ModInfo f1, ModInfo f2)
	{
		if (f1.LoadPriority == f2.LoadPriority)
		{
			return f1.ID.CompareTo(f2.ID);
		}
		return f1.LoadPriority.CompareTo(f2.LoadPriority);
	}

	public static void Init(bool Reload = false)
	{
		if (!Reload && Mods != null)
		{
			return;
		}
		Compiled = false;
		Mods = new List<ModInfo>();
		ModMap = new Dictionary<string, ModInfo>();
		if (Options.EnableMods)
		{
			ReadModSettings();
			try
			{
				RefreshModDirectory(DataManager.SavePath("Mods"));
			}
			catch (Exception message)
			{
				MetricsManager.LogError(message);
			}
			try
			{
				RefreshWorkshopSubscriptions();
			}
			catch (Exception message2)
			{
				MetricsManager.LogError(message2);
			}
			Mods.Sort(SortModInfo);
			LogRunningMods();
		}
	}

	/// <summary>
	///             Get the mod associated with specified ID.
	///             </summary><returns>The <see cref="T:XRL.ModInfo" /> if one by that ID exists; otherwise, <c>null</c>.</returns>
	public static ModInfo GetMod(string ID)
	{
		if (ID == null)
		{
			return null;
		}
		if (ModMap == null)
		{
			Init();
		}
		if (ModMap.TryGetValue(ID, out var value))
		{
			return value;
		}
		return null;
	}

	public static ModInfo GetMod(Assembly Assembly)
	{
		if (Assembly == null)
		{
			return null;
		}
		if (Mods == null)
		{
			Init();
		}
		foreach (ModInfo mod in Mods)
		{
			if (!(mod.Assembly != Assembly))
			{
				return mod;
			}
		}
		return null;
	}

	/// <summary>Get the first mod encountered in the currently executing stack.</summary>
	public static bool TryGetCallingMod(out ModInfo Mod, out StackFrame Frame)
	{
		return TryGetStackMod(new StackTrace(1), out Mod, out Frame);
	}

	public static bool TryGetStackMod(Exception Exception, out ModInfo Mod, out StackFrame Frame)
	{
		return TryGetStackMod(new StackTrace(Exception), out Mod, out Frame);
	}

	/// <summary>Get the first mod encountered in the specified stack.</summary><remarks>Mostly for logging purposes when the ModInfo isn't readily accessible.</remarks><todo>
	///             File name and line number not available even when debug symbols enabled.
	///             Possibly due to lack of support for the portable pdb format in current build target.
	///             </todo>
	public static bool TryGetStackMod(StackTrace Trace, out ModInfo Mod, out StackFrame Frame)
	{
		try
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			StackFrame[] frames = Trace.GetFrames();
			foreach (StackFrame stackFrame in frames)
			{
				Assembly assembly = stackFrame.GetMethod().DeclaringType.Assembly;
				if (!(assembly == executingAssembly))
				{
					ModInfo modInfo = Mods.FirstOrDefault((ModInfo x) => x.Assembly == assembly);
					if (modInfo != null)
					{
						Mod = modInfo;
						Frame = stackFrame;
						return true;
					}
				}
			}
		}
		catch
		{
		}
		Mod = null;
		Frame = null;
		return false;
	}

	public static string GetModTitle(string ID)
	{
		ModInfo mod = GetMod(ID);
		if (mod != null)
		{
			return mod.DisplayTitle;
		}
		ReadModSettings();
		if (ModSettingsMap.TryGetValue(ID, out var value) && !value.Title.IsNullOrEmpty())
		{
			return value.Title;
		}
		return ID;
	}

	public static void LogRunningMods()
	{
		string text = "Enabled mods: ";
		text = ((Mods != null && Mods.Count != 0) ? (text + string.Join(", ", from m in Mods
			where m.IsEnabled
			select m.DisplayTitleStripped)) : (text + "None"));
		MetricsManager.LogInfo(text);
	}

	public static List<string> GetRunningMods()
	{
		List<string> result = new List<string>();
		ForEachMod(delegate(ModInfo mod)
		{
			if (mod.IsEnabled)
			{
				result.Add(mod.ID);
			}
		});
		return result;
	}

	public static List<string> GetAvailableMods()
	{
		List<string> result = new List<string>();
		ForEachMod(delegate(ModInfo mod)
		{
			result.Add(mod.ID);
		}, IncludeDisabled: true);
		return result;
	}

	public static bool AreAnyModsUnapproved()
	{
		for (int i = 0; i < Mods.Count; i++)
		{
			if (!Mods[i].IsApproved)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AreAnyModsFailed()
	{
		for (int i = 0; i < Mods.Count; i++)
		{
			if (Mods[i].Settings.Failed)
			{
				return true;
			}
		}
		return false;
	}

	public static void ForEachMod(Action<ModInfo> ModAction, bool IncludeDisabled = false)
	{
		Init();
		foreach (ModInfo mod in Mods)
		{
			if (mod.IsEnabled || IncludeDisabled)
			{
				try
				{
					ModAction(mod);
				}
				catch (Exception ex)
				{
					mod.Error(DataManager.SanitizePathForDisplay(ex.ToString()));
				}
			}
		}
	}

	public static void ForEveryFile(Action<string, ModInfo> FileAction, bool IncludeDisabled = false)
	{
		ForEachMod(delegate(ModInfo mod)
		{
			if (!mod.Directory.Exists)
			{
				return;
			}
			foreach (FileInfo item in mod.Directory.EnumerateFiles())
			{
				FileAction(item.FullName, mod);
			}
		}, IncludeDisabled);
	}

	public static void ForEveryFileRecursive(Action<string, ModInfo> FileAction, string SearchPattern = "*.*", bool IncludeDisabled = false)
	{
		ForEachMod(delegate(ModInfo mod)
		{
			if (!mod.Directory.Exists)
			{
				return;
			}
			foreach (string item in Directory.EnumerateFiles(mod.Path, SearchPattern, SearchOption.AllDirectories))
			{
				FileAction(item, mod);
			}
		}, IncludeDisabled);
	}

	public static void ForEachFile(string FileName, Action<string> FileAction, bool IncludeDisabled = false)
	{
		ForEachFile(FileName, delegate(string f, ModInfo i)
		{
			FileAction(f);
		}, IncludeDisabled);
	}

	public static void ForEachFile(string FileName, Action<string, ModInfo> FileAction, bool IncludeDisabled = false)
	{
		string name = FileName.ToLower();
		ForEachMod(delegate(ModInfo mod)
		{
			if (!mod.Directory.Exists)
			{
				return;
			}
			foreach (FileInfo item in mod.Directory.EnumerateFiles())
			{
				if (!(item.Name.ToLower() != name))
				{
					try
					{
						FileAction(item.FullName, mod);
						break;
					}
					catch (Exception ex)
					{
						mod.Error(DataManager.SanitizePathForDisplay(mod.Path + "/" + FileName + ": " + ex.ToString()));
					}
				}
			}
		}, IncludeDisabled);
	}

	public static void ForEachFileIn(string Subdirectory, Action<string, ModInfo> FileAction, bool bIncludeBase = false)
	{
		Init();
		if (bIncludeBase)
		{
			_ForEachFileIn(DataManager.FilePath(Subdirectory), FileAction, null);
		}
		foreach (ModInfo mod in Mods)
		{
			_ForEachFileIn(Path.Combine(mod.Path, Subdirectory), FileAction, mod);
		}
	}

	private static void _ForEachFileIn(string Subdirectory, Action<string, ModInfo> FileAction, ModInfo mod)
	{
		if (!Directory.Exists(Subdirectory))
		{
			return;
		}
		foreach (string item in Directory.EnumerateFiles(Subdirectory))
		{
			try
			{
				FileAction(item, mod);
			}
			catch (Exception ex)
			{
				mod.Error(DataManager.SanitizePathForDisplay(Subdirectory + ": " + ex.ToString()));
			}
		}
		string[] directories = Directory.GetDirectories(Subdirectory);
		for (int i = 0; i < directories.Length; i++)
		{
			_ForEachFileIn(directories[i], FileAction, mod);
		}
	}

	public static object CreateInstance(string className)
	{
		Type type = ResolveType(className);
		if (type == null)
		{
			throw new TypeLoadException("No class with name \"" + className + "\" could be found. A full name including namespaces is required.");
		}
		return Activator.CreateInstance(type);
	}

	public static T CreateInstance<T>(string className) where T : class
	{
		return CreateInstance(className) as T;
	}

	public static IEnumerable<Type> GetClassesWithAttribute(Type attributeToSearchFor, Type classFilterAttribute = null)
	{
		List<Type> list = new List<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.FullName.Contains("Assembly-CSharp"))
			{
				list.AddRange(from m in assembly.GetTypes().Concat(assembly.GetTypes().SelectMany((Type type) => type.GetNestedTypes()))
					where m.IsDefined(attributeToSearchFor, inherit: false) && m.IsClass
					select m);
			}
		}
		return list;
	}

	public static List<T> GetInstancesWithAttribute<T>(Type attributeType) where T : class
	{
		List<T> list = new List<T>();
		foreach (Type item2 in GetTypesWithAttribute(attributeType))
		{
			if (Activator.CreateInstance(item2) is T item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static IEnumerable<Type> GetTypesWithAttribute(Type attributeType)
	{
		foreach (Type activeType in ActiveTypes)
		{
			if (activeType.IsDefined(attributeType, inherit: true))
			{
				yield return activeType;
			}
		}
	}

	public static List<MethodInfo> GetMethodsWithAttribute(Type attributeType, Type classFilterType = null, bool Cache = true)
	{
		if (_methodsWithAttribute.TryGetValue(attributeType, out var value))
		{
			return value;
		}
		value = new List<MethodInfo>(128);
		foreach (Type item in (classFilterType == null) ? ActiveTypes : ActiveTypes.Where((Type t) => t.IsDefined(classFilterType, inherit: false)))
		{
			MethodInfo[] methods = item.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.IsDefined(attributeType, inherit: false))
				{
					value.Add(methodInfo);
				}
			}
		}
		if (Cache)
		{
			_methodsWithAttribute.Add(attributeType, value);
			value.TrimExcess();
		}
		return value;
	}

	public static List<FieldInfo> GetFieldsWithAttribute(Type attributeType, Type classFilterType = null, bool Cache = true)
	{
		if (_fieldsWithAttribute.TryGetValue(attributeType, out var value))
		{
			return value;
		}
		value = new List<FieldInfo>(128);
		foreach (Type item in (classFilterType == null) ? ActiveTypes : ActiveTypes.Where((Type t) => t.IsDefined(classFilterType, inherit: false)))
		{
			FieldInfo[] fields = item.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.IsDefined(attributeType, inherit: false))
				{
					value.Add(fieldInfo);
				}
			}
		}
		if (Cache)
		{
			_fieldsWithAttribute.Add(attributeType, value);
			value.TrimExcess();
		}
		return value;
	}
}

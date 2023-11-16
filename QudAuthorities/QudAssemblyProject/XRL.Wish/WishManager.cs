using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using XRL.UI;

namespace XRL.Wish;

[HasModSensitiveStaticCache]
[HasWishCommand]
public static class WishManager
{
	private struct Command
	{
		public WishCommand wish;

		public Type type;

		public MethodInfo method;

		public Regex regex;
	}

	[ModSensitiveStaticCache(false)]
	private static List<Command> CommandCollection;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<Type, object> CommandObjects;

	public static void UpdateCommandCollection()
	{
		CommandCollection = new List<Command>();
		foreach (MethodInfo item in ModManager.GetMethodsWithAttribute(typeof(WishCommand), typeof(HasWishCommand)))
		{
			WishCommand[] array = item.GetCustomAttributes(typeof(WishCommand), inherit: false) as WishCommand[];
			foreach (WishCommand wishCommand in array)
			{
				if (wishCommand.Command == null)
				{
					wishCommand.Command = item.Name.ToLower();
				}
				ParameterInfo[] parameters = item.GetParameters();
				Regex regex;
				if (wishCommand.Regex != null)
				{
					if (parameters.Count() != 0 && (parameters.Count() != 1 || !parameters[0].ParameterType.IsAssignableFrom(typeof(Match))))
					{
						MetricsManager.LogError("WishCommand defined its own regex but method does not take (Match) or () parameters.");
						continue;
					}
					regex = new Regex(wishCommand.Regex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
				}
				else if (parameters.Count() == 0)
				{
					regex = new Regex("^" + wishCommand.Command + "$");
				}
				else
				{
					if (parameters.Count() != 1 || (!(parameters[0].ParameterType == typeof(string)) && !(parameters[0].ParameterType == typeof(Match))))
					{
						MetricsManager.LogError("WishCommand must take either no arguments, or a single string, or a Match");
						continue;
					}
					regex = new Regex("^" + wishCommand.Command + "(?::|\\s+)(.*)$");
				}
				CommandCollection.Add(new Command
				{
					wish = wishCommand,
					type = item.DeclaringType,
					method = item,
					regex = regex
				});
			}
		}
	}

	public static object GetInstanceForType(Type type)
	{
		if (CommandObjects == null)
		{
			CommandObjects = new Dictionary<Type, object>();
		}
		if (!CommandObjects.TryGetValue(type, out var value))
		{
			value = Activator.CreateInstance(type);
			CommandObjects.Add(type, value);
		}
		return value;
	}

	public static void ClaimCommandInstance(Type type, object instance)
	{
		CommandObjects[type] = instance;
	}

	[WishCommand(null, null)]
	public static void Rewish()
	{
		Loading.LoadTask("Loading wish commands", UpdateCommandCollection);
	}

	[WishCommand(null, null, Command = "wish")]
	public static bool HandleWish(string wish)
	{
		if (CommandCollection == null)
		{
			Rewish();
		}
		foreach (Command item in CommandCollection)
		{
			ParameterInfo[] parameters = item.method.GetParameters();
			Match match = item.regex.Match(wish);
			if (!match.Success)
			{
				continue;
			}
			List<object> list = new List<object>();
			if (parameters.Count() > 0)
			{
				if (parameters[0].ParameterType.IsAssignableFrom(typeof(Match)))
				{
					list.Add(match);
				}
				if (parameters[0].ParameterType == typeof(string))
				{
					list.Add(match.Groups[1].ToString());
				}
			}
			object obj = (item.method.IsStatic ? null : GetInstanceForType(item.type));
			object obj2 = item.method.Invoke(obj, list.ToArray());
			if (item.method.ReturnType == typeof(void) || (bool)obj2)
			{
				return true;
			}
		}
		return false;
	}
}

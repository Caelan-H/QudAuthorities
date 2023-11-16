using System.Collections.Generic;
using System.Linq;

namespace XRL.CharacterBuilds;

public class EmbarkInfo
{
	public string GameSeed;

	public List<AbstractEmbarkBuilderModuleData> _data = new List<AbstractEmbarkBuilderModuleData>();

	public List<AbstractEmbarkBuilderModule> _modules = new List<AbstractEmbarkBuilderModule>();

	public List<AbstractEmbarkBuilderModule> modules => _modules;

	public IEnumerable<AbstractEmbarkBuilderModule> enabledModules => _modules.Where((AbstractEmbarkBuilderModule m) => m.enabled);

	public T getData<T>() where T : AbstractEmbarkBuilderModuleData
	{
		return _data.Where((AbstractEmbarkBuilderModuleData d) => d.GetType() == typeof(T)).FirstOrDefault() as T;
	}

	public T getModule<T>() where T : AbstractEmbarkBuilderModule
	{
		for (int i = 0; i < modules.Count; i++)
		{
			if (modules[i] is T)
			{
				return modules[i] as T;
			}
		}
		return null;
	}

	public void bootGame(XRLGame game)
	{
		foreach (AbstractEmbarkBuilderModule enabledModule in enabledModules)
		{
			enabledModule.bootGame(game, this);
		}
		foreach (IGameSystem system in game.Systems)
		{
			system.PlayerEmbarking();
		}
	}

	public void fireBootEvent(string id, XRLGame game)
	{
		foreach (AbstractEmbarkBuilderModule enabledModule in enabledModules)
		{
			enabledModule.handleBootEvent(id, game, this);
		}
	}

	public T fireBootEvent<T>(string id, XRLGame game, T element)
	{
		foreach (AbstractEmbarkBuilderModule enabledModule in enabledModules)
		{
			element = (T)enabledModule.handleBootEvent(id, game, this, element);
		}
		return element;
	}
}

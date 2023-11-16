using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using XRL.UI.ObjectFinderClassifiers;
using XRL.UI.ObjectFinderContexts;
using XRL.UI.ObjectFinderSorters;
using XRL.Wish;
using XRL.World;

namespace XRL.UI;

[HasWishCommand]
[HasGameBasedStaticCache]
public class ObjectFinder
{
	/// <summary> 
	///               Contexts are created when the object finder is open, and the context has been "enabled".
	///             </summary>
	public abstract class Context
	{
		protected ObjectFinder finder;

		public virtual string GetDisplayName()
		{
			return GetType().Name;
		}

		public virtual IRenderable GetIcon()
		{
			return null;
		}

		public virtual void Enable()
		{
		}

		public virtual void Disable()
		{
		}

		public Context()
		{
		}

		public Context(ObjectFinder finder)
		{
			setFinder(finder);
		}

		public void setFinder(ObjectFinder finder)
		{
			this.finder = finder;
		}
	}

	public abstract class Classifier
	{
		public virtual string GetDisplayName()
		{
			return GetType().Name;
		}

		public virtual IRenderable GetIcon()
		{
			return null;
		}

		public abstract bool Check(GameObject go);
	}

	public class FilterRule
	{
		public bool enabled = true;

		public bool show = true;

		public Classifier classifier;

		public string stateName
		{
			get
			{
				if (!enabled)
				{
					return "Disabled";
				}
				if (!show)
				{
					return "Hide";
				}
				return "Show";
			}
		}

		public string stateColor
		{
			get
			{
				if (!enabled)
				{
					return "K";
				}
				if (!show)
				{
					return "R";
				}
				return "G";
			}
		}
	}

	public abstract class Sorter : IComparer<GameObject>
	{
		public virtual string GetDisplayName()
		{
			return GetType().Name;
		}

		public virtual IRenderable GetIcon()
		{
			return null;
		}

		public virtual int Compare(GameObject a, GameObject b)
		{
			return 0;
		}
	}

	public static ObjectFinder instance;

	private List<Context> contexts = new List<Context>();

	private Dictionary<Context, List<GameObject>> contextObjects = new Dictionary<Context, List<GameObject>>();

	private bool contextUpdated;

	private List<GameObject> filterCache = new List<GameObject>();

	private bool filterCacheUpdated;

	private List<FilterRule> filters = new List<FilterRule>();

	private Sorter activeSorter = new IdSorter();

	private List<GameObject> _updateFilterAccepted = new List<GameObject>();

	private List<GameObject> _updateFilterRejected = new List<GameObject>();

	[GameBasedCacheInit]
	public static ObjectFinder Reset()
	{
		instance?.Destroy();
		instance = new ObjectFinder();
		instance.filters.Add(new FilterRule
		{
			classifier = new Everything(),
			enabled = true,
			show = true
		});
		return instance;
	}

	public void LoadDefaults()
	{
		filters.Insert(0, new FilterRule
		{
			classifier = new Cosmetic(),
			enabled = true,
			show = false
		});
		filters.Insert(0, new FilterRule
		{
			classifier = new Pools(),
			enabled = true,
			show = false
		});
		filters.Insert(0, new FilterRule
		{
			classifier = new NonCombatPlantlife(),
			enabled = true,
			show = false
		});
		filters.Insert(0, new FilterRule
		{
			classifier = new Walls(),
			enabled = false,
			show = false
		});
		filters.Insert(0, new FilterRule
		{
			classifier = new Player(),
			enabled = true,
			show = false
		});
		Add(new NearbyItems());
	}

	public void ReadOptions()
	{
		FilterRule filterRule = filters.Find((FilterRule r) => r.classifier is NonCombatPlantlife);
		FilterRule filterRule2 = filters.Find((FilterRule r) => r.classifier is Pools);
		if (filterRule != null)
		{
			filterRule.show = Options.OverlayNearbyObjectsPlants;
		}
		if (filterRule2 != null)
		{
			filterRule2.show = Options.OverlayNearbyObjectsPools;
		}
		contextUpdated = true;
	}

	public void Add(Context context)
	{
		if (context == null)
		{
			throw new ArgumentNullException();
		}
		if (contexts.Contains(context))
		{
			throw new ArgumentException();
		}
		context.setFinder(this);
		contexts.Add(context);
		context.Enable();
	}

	public void Remove(Context context)
	{
		if (context == null)
		{
			throw new ArgumentNullException();
		}
		if (!contexts.Contains(context))
		{
			throw new ArgumentException();
		}
		context.Disable();
		context.setFinder(null);
		if (contexts.Contains(context))
		{
			contexts.Remove(context);
		}
	}

	public void Destroy()
	{
		if (contextObjects != null)
		{
			foreach (Context key in contextObjects.Keys)
			{
				key.Disable();
			}
			contextObjects = null;
		}
		filterCache = null;
		_updateFilterAccepted = null;
		_updateFilterRejected = null;
	}

	protected List<GameObject> GetContextList(Context context)
	{
		if (context == null)
		{
			return null;
		}
		if (contextObjects == null)
		{
			return null;
		}
		if (contextObjects.TryGetValue(context, out var value))
		{
			return value;
		}
		value = new List<GameObject>(64);
		contextObjects.Add(context, value);
		return value;
	}

	[WishCommand(null, null)]
	public static bool ConfigObjectFinderFilters()
	{
		instance?.ConfigFilters();
		return true;
	}

	public void ConfigFilters()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		while (true)
		{
			string[] array = new string[filters.Count];
			for (int i = 0; i < filters.Count; i++)
			{
				array[i] = stringBuilder.Clear().Append(filters[i].classifier.GetDisplayName()).AppendMarkupNode(filters[i].stateColor, " [" + filters[i].stateName + "]")
					.ToString();
			}
			int num = Popup.ShowOptionList("Pick a filter to change", array, null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
			if (num < 0)
			{
				break;
			}
			List<string> list = new List<string> { "Show Items", "Hide Items", "Ignore Rule", "Move Up", "Move Down" };
			FilterRule filterRule = filters[num];
			if (num == filters.Count - 1)
			{
				list.RemoveAt(4);
			}
			if (num == 0)
			{
				list.RemoveAt(3);
			}
			if (!filterRule.enabled)
			{
				list.RemoveAt(2);
			}
			else if (filterRule.show)
			{
				list.RemoveAt(0);
			}
			else
			{
				list.RemoveAt(1);
			}
			int num2 = Popup.ShowOptionList(filterRule.classifier.GetDisplayName(), list.ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
			if (num2 != -1)
			{
				if (list[num2] == "Show Items")
				{
					filterRule.enabled = (filterRule.show = true);
				}
				if (list[num2] == "Hide Items")
				{
					filterRule.enabled = true;
					filterRule.show = false;
				}
				if (list[num2] == "Ignore Rule")
				{
					filterRule.enabled = false;
				}
				if (list[num2] == "Move Up")
				{
					filters.RemoveAt(num);
					filters.Insert(num - 1, filterRule);
				}
				if (list[num2] == "Move Down")
				{
					filters.RemoveAt(num);
					filters.Insert(num + 1, filterRule);
				}
			}
		}
		contextUpdated = true;
	}

	public void UpdateContext(Context context, IEnumerable<GameObject> list)
	{
		List<GameObject> contextList = GetContextList(context);
		if (contextList == null)
		{
			return;
		}
		lock (contextList)
		{
			contextList.Clear();
			contextList.AddRange(list);
			contextUpdated = true;
		}
	}

	public bool UpdateFilter(bool force = false)
	{
		if (contextUpdated || force)
		{
			contextUpdated = false;
			_updateFilterAccepted.Clear();
			_updateFilterRejected.Clear();
			foreach (List<GameObject> value in contextObjects.Values)
			{
				lock (value)
				{
					foreach (GameObject item in value)
					{
						if (_updateFilterAccepted.Contains(item) || _updateFilterRejected.Contains(item))
						{
							continue;
						}
						bool? flag = null;
						int num = 0;
						while (flag != false && num < filters.Count)
						{
							if (filters[num].enabled && filters[num].classifier.Check(item))
							{
								flag = filters[num].show;
							}
							num++;
						}
						if (flag == true)
						{
							_updateFilterAccepted.Add(item);
						}
						else
						{
							_updateFilterRejected.Add(item);
						}
					}
				}
			}
			if (_updateFilterAccepted.Count != filterCache.Count || !_updateFilterAccepted.All(filterCache.Contains))
			{
				List<GameObject> updateFilterAccepted = filterCache;
				filterCache = _updateFilterAccepted;
				_updateFilterAccepted = updateFilterAccepted;
				filterCacheUpdated = true;
			}
			filterCache.Sort(activeSorter);
		}
		return filterCacheUpdated;
	}

	public IEnumerable<GameObject> peekItems()
	{
		return readItems(peek: true);
	}

	public IEnumerable<GameObject> readItems(bool peek = false)
	{
		if (!peek)
		{
			filterCacheUpdated = false;
		}
		return filterCache;
	}
}

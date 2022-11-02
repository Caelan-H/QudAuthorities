using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XRL;
using XRL.UI;

namespace Qud.UI;

[ExecuteAlways]
[HasGameBasedStaticCache]
[UIView("NearbyItems", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "NearbyItems", UICanvasHost = 1)]
public class NearbyItemsWindow : MovableSceneFrameWindowBase<NearbyItemsWindow>
{
	public QudItemList Items = new QudItemList();

	public bool Dirty;

	private bool FinderInitialized;

	public GameObject lineItemPrefab;

	public GameObject contentRoot;

	public int Lines;

	private Queue<GameObject> LinePool = new Queue<GameObject>();

	public static ObjectFinder Finder => ObjectFinder.instance;

	[GameBasedCacheInit]
	public static void GameInit()
	{
		lock (SingletonWindowBase<NearbyItemsWindow>.instance.Items)
		{
			SingletonWindowBase<NearbyItemsWindow>.instance.Items.Clear();
			SingletonWindowBase<NearbyItemsWindow>.instance.Dirty = true;
		}
		SingletonWindowBase<NearbyItemsWindow>.instance.FinderInitialized = false;
	}

	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public void TogglePreferredState()
	{
		Toggle();
		SaveOptions();
	}

	public void SaveOptions()
	{
		Options.OverlayNearbyObjects = base.Visible;
	}

	public void ShowIfEnabled()
	{
		if (Options.OverlayNearbyObjects)
		{
			Show();
			if (!FinderInitialized && Finder != null)
			{
				Finder.LoadDefaults();
				Finder.ReadOptions();
				FinderInitialized = true;
			}
		}
		else
		{
			Hide();
			if (FinderInitialized)
			{
				ObjectFinder.Reset();
				FinderInitialized = false;
			}
		}
	}

	public override void Update()
	{
		try
		{
			if (Dirty)
			{
				lock (Items)
				{
					Clear();
					foreach (QudItemListElement item in Items.objects.Take(50))
					{
						GameObject line = GetLine();
						ItemLineManager component = line.GetComponent<ItemLineManager>();
						component.SetMode("Nearby");
						component.SetGameObject(item);
						line.transform.SetAsLastSibling();
					}
					Dirty = false;
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		base.Update();
	}

	public void UpdateGameContext()
	{
		lock (Items)
		{
			if (Finder != null && Finder.UpdateFilter())
			{
				Items.Clear();
				Items.Add(Finder.readItems());
				Dirty = true;
			}
		}
	}

	public void Clear()
	{
		foreach (Transform item in contentRoot.transform)
		{
			if (!item)
			{
				continue;
			}
			ItemLineManager component = item.GetComponent<ItemLineManager>();
			if ((bool)component)
			{
				GameObject gameObject = item.gameObject;
				if (gameObject.activeSelf)
				{
					component.SetGameObject(null);
					gameObject.SetActive(value: false);
					LinePool.Enqueue(gameObject);
				}
			}
		}
	}

	public GameObject GetLine()
	{
		if (LinePool.Count > 0)
		{
			GameObject obj = LinePool.Dequeue();
			obj.SetActive(value: true);
			return obj;
		}
		GameObject obj2 = UnityEngine.Object.Instantiate(lineItemPrefab, contentRoot.transform, worldPositionStays: false);
		obj2.name = "NearbyItemsLine " + ++Lines;
		return obj2;
	}
}

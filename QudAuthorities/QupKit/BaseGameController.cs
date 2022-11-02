using System.Collections.Generic;
using UnityEngine;

namespace QupKit;

public class BaseGameController<T> : MonoBehaviour where T : class
{
	public Canvas MainCanvas;

	public LegacyViewManager Views;

	public List<GameObject> ViewCanvases = new List<GameObject>();

	public bool Hello = true;

	public virtual void OnStart()
	{
	}

	public virtual void RegisterViews()
	{
	}

	public virtual void AfterOnGUI()
	{
	}

	public void RegisterView(string ID, BaseView V)
	{
		foreach (GameObject viewCanvase in ViewCanvases)
		{
			if (viewCanvase != null && viewCanvase.name == ID)
			{
				V.AttachTo(viewCanvase);
				Views.AddView(ID, V);
				break;
			}
		}
	}

	private void Start()
	{
		Views = new LegacyViewManager();
		RegisterViews();
		Views.RegisterRemainingViews(ViewCanvases);
		Views.Create();
		OnStart();
	}

	public virtual void OnUpdate()
	{
	}

	private void Update()
	{
		OnUpdate();
		Views.Update();
	}
}

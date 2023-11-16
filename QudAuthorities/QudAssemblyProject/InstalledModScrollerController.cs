using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using XRL;

public class InstalledModScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
{
	private List<InstalledModScrollerData> _data = new List<InstalledModScrollerData>();

	public EnhancedScroller myScroller;

	public InstalledModCellView animalCellViewPrefab;

	public void Refresh()
	{
		_data.Clear();
		ModManager.Refresh();
		for (int i = 0; i < ModManager.Mods.Count; i++)
		{
			_data.Add(new InstalledModScrollerData(ModManager.Mods[i]));
		}
		myScroller.ReloadData();
	}

	private void Start()
	{
		myScroller.Delegate = this;
	}

	public int GetNumberOfCells(EnhancedScroller scroller)
	{
		return _data.Count;
	}

	public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
	{
		return 40f;
	}

	public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
	{
		InstalledModCellView obj = scroller.GetCellView(animalCellViewPrefab) as InstalledModCellView;
		obj.SetData(_data[dataIndex]);
		return obj;
	}
}

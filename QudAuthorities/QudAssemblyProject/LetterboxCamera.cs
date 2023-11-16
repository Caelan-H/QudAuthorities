using UnityEngine;
using XRL.UI;

public class LetterboxCamera : MonoBehaviour
{
	private bool _Refresh;

	public int UpdateDelay;

	public float _DesiredZoomFactor = 1f;

	public Vector3 _DesiredPosition;

	public float DesiredWidth = 1280f;

	private int LastWidth;

	private int LastHeight;

	private float LastZoomFactor;

	private bool lastPrerelaseStage;

	private double lastStageScale;

	public bool suspendUpdate;

	public float DesiredZoomFactor
	{
		get
		{
			return _DesiredZoomFactor;
		}
		set
		{
			if (_DesiredZoomFactor != value)
			{
				_DesiredZoomFactor = value;
			}
		}
	}

	public Vector3 DesiredPosition
	{
		get
		{
			return _DesiredPosition;
		}
		set
		{
			if (_DesiredPosition != value)
			{
				_DesiredPosition = value;
			}
		}
	}

	public Vector3 CurrentPosition => base.gameObject.transform.localPosition;

	public void Refresh()
	{
		_Refresh = true;
	}

	public void Awake()
	{
		DesiredPosition = base.transform.localPosition;
		suspendUpdate = false;
	}

	public void SetPositionImmediately(Vector3 position)
	{
		if (UpdateDelay <= 0)
		{
			base.gameObject.transform.localPosition = position;
		}
		DesiredPosition = position;
	}

	public void Update()
	{
		if (UpdateDelay > 0)
		{
			UpdateDelay--;
		}
		else if (Screen.width != LastWidth || Screen.height != LastHeight || DesiredZoomFactor != LastZoomFactor || _Refresh || Options.ModernUI != lastPrerelaseStage || Options.StageScale != lastStageScale || base.transform.localPosition != DesiredPosition)
		{
			_Refresh = false;
			LastWidth = Screen.width;
			LastHeight = Screen.height;
			LastZoomFactor = DesiredZoomFactor;
			lastPrerelaseStage = Options.ModernUI;
			lastStageScale = Options.StageScale;
			base.transform.localPosition = DesiredPosition;
			float num = Screen.width;
			float num2 = Screen.height;
			float num3 = (Options.ModernUI ? ((float)(216.0 * Options.StageScale)) : 8f);
			float b = 0.5f * (600f + num3 * (600f / num2));
			float a = DesiredWidth / num * (num2 / 2f);
			GetComponent<Camera>().orthographicSize = Mathf.Max(a, b);
			_ = GetComponent<Camera>().orthographicSize;
			GetComponent<Camera>().orthographicSize = GetComponent<Camera>().orthographicSize / DesiredZoomFactor;
			if (base.gameObject.GetComponent<CC_AnalogTV>() != null)
			{
				base.gameObject.GetComponent<CC_AnalogTV>().scanlinesCount = 1853f / DesiredZoomFactor;
			}
		}
	}
}

using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class InfoChip : MonoBehaviour
{
	public string title;

	public Color titleColor;

	public string value;

	public Color valueColor;

	public Text titleText;

	public Text valueText;

	private string _lastTitle;

	private string _lastValue = "not it";

	private Color _lastTitleColor;

	private Color _lastValueColor;

	private void Start()
	{
	}

	private void Update()
	{
		if (_lastTitleColor != titleColor && titleText != null)
		{
			titleText.color = (_lastTitleColor = titleColor);
		}
		if (_lastTitle != title && titleText != null)
		{
			_lastTitle = title;
			titleText.text = title + ":";
		}
		if (_lastValueColor != valueColor && valueText != null)
		{
			valueText.color = (_lastValueColor = valueColor);
		}
		if (_lastValue != value && valueText != null)
		{
			valueText.text = (_lastValue = value);
		}
	}
}

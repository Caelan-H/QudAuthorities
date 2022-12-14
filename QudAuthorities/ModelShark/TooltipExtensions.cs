using UnityEngine;
using UnityEngine.UI;

namespace ModelShark;

public static class TooltipExtensions
{
	public static void SetPosition(this Tooltip tooltip, TooltipTrigger trigger, Canvas canvas, Camera camera)
	{
		Vector3[] array = new Vector3[4];
		RectTransform component = trigger.gameObject.GetComponent<RectTransform>();
		if (component != null)
		{
			component.GetWorldCorners(array);
		}
		else
		{
			Collider component2 = trigger.GetComponent<Collider>();
			Vector3 center = component2.bounds.center;
			Vector3 extents = component2.bounds.extents;
			Vector3 position = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
			Vector3 position2 = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
			Vector3 position3 = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
			Vector3 position4 = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);
			array[0] = camera.WorldToScreenPoint(position);
			array[1] = camera.WorldToScreenPoint(position2);
			array[2] = camera.WorldToScreenPoint(position3);
			array[3] = camera.WorldToScreenPoint(position4);
		}
		tooltip.SetPosition(trigger.tipPosition, trigger.tooltipStyle, array, tooltip.BackgroundImage, tooltip.RectTransform, canvas, camera);
		if (!TooltipManager.Instance.overflowProtection)
		{
			return;
		}
		Vector3[] array2 = new Vector3[4];
		tooltip.RectTransform.GetWorldCorners(array2);
		if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
		{
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = RectTransformUtility.WorldToScreenPoint(camera, array2[i]);
			}
		}
		else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = RectTransformUtility.WorldToScreenPoint(null, array2[j]);
			}
		}
		Rect rect = new Rect(0f, 0f, Screen.width, Screen.height);
		TooltipOverflow tooltipOverflow = new TooltipOverflow
		{
			BottomLeftCorner = !rect.Contains(array2[0]),
			TopLeftCorner = !rect.Contains(array2[1]),
			TopRightCorner = !rect.Contains(array2[2]),
			BottomRightCorner = !rect.Contains(array2[3])
		};
		if (tooltipOverflow.IsAny)
		{
			tooltip.SetPosition(tooltipOverflow.SuggestNewPosition(trigger.tipPosition), trigger.tooltipStyle, array, tooltip.BackgroundImage, tooltip.RectTransform, canvas, camera);
		}
	}

	private static void SetPosition(this Tooltip tooltip, TipPosition tipPosition, TooltipStyle style, Vector3[] triggerCorners, Image bkgImage, RectTransform tooltipRectTrans, Canvas canvas, Camera camera)
	{
		Vector3 position = Vector3.zero;
		Vector3 vector = Vector3.zero;
		bool flag = tipPosition == TipPosition.MouseBottomLeftCorner || tipPosition == TipPosition.MouseTopLeftCorner || tipPosition == TipPosition.MouseBottomRightCorner || tipPosition == TipPosition.MouseTopRightCorner || tipPosition == TipPosition.MouseTopMiddle || tipPosition == TipPosition.MouseLeftMiddle || tipPosition == TipPosition.MouseRightMiddle || tipPosition == TipPosition.MouseBottomMiddle;
		Vector3 vector2 = Input.mousePosition;
		if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
		{
			vector2.z = canvas.planeDistance;
			vector2 = camera.ScreenToWorldPoint(vector2);
		}
		switch (tipPosition)
		{
		case TipPosition.TopRightCorner:
		case TipPosition.MouseTopRightCorner:
			vector = new Vector3(-1 * style.tipOffset, -1 * style.tipOffset);
			position = (flag ? vector2 : triggerCorners[2]);
			tooltipRectTrans.pivot = new Vector2(0f, 0f);
			bkgImage.sprite = style.bottomLeftCorner;
			break;
		case TipPosition.BottomRightCorner:
		case TipPosition.MouseBottomRightCorner:
			vector = new Vector3(-1 * style.tipOffset, style.tipOffset);
			position = (flag ? vector2 : triggerCorners[3]);
			tooltipRectTrans.pivot = new Vector2(0f, 1f);
			bkgImage.sprite = style.topLeftCorner;
			break;
		case TipPosition.TopLeftCorner:
		case TipPosition.MouseTopLeftCorner:
			vector = new Vector3(style.tipOffset, -1 * style.tipOffset);
			position = (flag ? vector2 : triggerCorners[1]);
			tooltipRectTrans.pivot = new Vector2(1f, 0f);
			bkgImage.sprite = style.bottomRightCorner;
			break;
		case TipPosition.BottomLeftCorner:
		case TipPosition.MouseBottomLeftCorner:
			vector = new Vector3(style.tipOffset, style.tipOffset);
			position = (flag ? vector2 : triggerCorners[0]);
			tooltipRectTrans.pivot = new Vector2(1f, 1f);
			bkgImage.sprite = style.topRightCorner;
			break;
		case TipPosition.TopMiddle:
		case TipPosition.MouseTopMiddle:
			vector = new Vector3(0f, -1 * style.tipOffset);
			position = (flag ? vector2 : (triggerCorners[1] + (triggerCorners[2] - triggerCorners[1]) / 2f));
			tooltipRectTrans.pivot = new Vector2(0.5f, 0f);
			bkgImage.sprite = style.topMiddle;
			break;
		case TipPosition.BottomMiddle:
		case TipPosition.MouseBottomMiddle:
			vector = new Vector3(0f, style.tipOffset);
			position = (flag ? vector2 : (triggerCorners[0] + (triggerCorners[3] - triggerCorners[0]) / 2f));
			tooltipRectTrans.pivot = new Vector2(0.5f, 1f);
			bkgImage.sprite = style.bottomMiddle;
			break;
		case TipPosition.LeftMiddle:
		case TipPosition.MouseLeftMiddle:
			vector = new Vector3(style.tipOffset, 0f);
			position = (flag ? vector2 : (triggerCorners[0] + (triggerCorners[1] - triggerCorners[0]) / 2f));
			tooltipRectTrans.pivot = new Vector2(1f, 0.5f);
			bkgImage.sprite = style.leftMiddle;
			break;
		case TipPosition.RightMiddle:
		case TipPosition.MouseRightMiddle:
			vector = new Vector3(-1 * style.tipOffset, 0f);
			position = (flag ? vector2 : (triggerCorners[3] + (triggerCorners[2] - triggerCorners[3]) / 2f));
			tooltipRectTrans.pivot = new Vector2(0f, 0.5f);
			bkgImage.sprite = style.rightMiddle;
			break;
		}
		tooltip.GameObject.transform.position = position;
		tooltipRectTrans.anchoredPosition += (Vector2)vector;
	}

	public static void SetPositionManual(this Tooltip tooltip, Vector3 pos, TipPosition tipPosition, TooltipStyle style, Vector3[] triggerCorners, Image bkgImage, RectTransform tooltipRectTrans, Canvas canvas, Camera camera)
	{
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = camera.WorldToScreenPoint(pos);
		tipPosition = TipPosition.TopRightCorner;
		tipPosition = ((vector2.x < (float)(Screen.width / 2)) ? ((!(vector2.y < (float)(Screen.height / 2))) ? TipPosition.BottomRightCorner : TipPosition.TopRightCorner) : ((!(vector2.y < (float)(Screen.height / 2))) ? TipPosition.BottomLeftCorner : TipPosition.TopLeftCorner));
		switch (tipPosition)
		{
		case TipPosition.TopRightCorner:
		case TipPosition.MouseTopRightCorner:
			vector = new Vector3(-1 * style.tipOffset, -1 * style.tipOffset);
			tooltipRectTrans.pivot = new Vector2(0f, 0f);
			bkgImage.sprite = style.bottomLeftCorner;
			break;
		case TipPosition.BottomRightCorner:
		case TipPosition.MouseBottomRightCorner:
			vector = new Vector3(-1 * style.tipOffset, style.tipOffset);
			tooltipRectTrans.pivot = new Vector2(0f, 1f);
			bkgImage.sprite = style.topLeftCorner;
			break;
		case TipPosition.TopLeftCorner:
		case TipPosition.MouseTopLeftCorner:
			vector = new Vector3(style.tipOffset, -1 * style.tipOffset);
			tooltipRectTrans.pivot = new Vector2(1f, 0f);
			bkgImage.sprite = style.bottomRightCorner;
			break;
		case TipPosition.BottomLeftCorner:
		case TipPosition.MouseBottomLeftCorner:
			vector = new Vector3(style.tipOffset, style.tipOffset);
			tooltipRectTrans.pivot = new Vector2(1f, 1f);
			bkgImage.sprite = style.topRightCorner;
			break;
		case TipPosition.TopMiddle:
		case TipPosition.MouseTopMiddle:
			vector = new Vector3(0f, -1 * style.tipOffset);
			tooltipRectTrans.pivot = new Vector2(0.5f, 0f);
			bkgImage.sprite = style.topMiddle;
			break;
		case TipPosition.BottomMiddle:
		case TipPosition.MouseBottomMiddle:
			vector = new Vector3(0f, style.tipOffset);
			tooltipRectTrans.pivot = new Vector2(0.5f, 1f);
			bkgImage.sprite = style.bottomMiddle;
			break;
		case TipPosition.LeftMiddle:
		case TipPosition.MouseLeftMiddle:
			vector = new Vector3(style.tipOffset, 0f);
			tooltipRectTrans.pivot = new Vector2(1f, 0.5f);
			bkgImage.sprite = style.leftMiddle;
			break;
		case TipPosition.RightMiddle:
		case TipPosition.MouseRightMiddle:
			vector = new Vector3(-1 * style.tipOffset, 0f);
			tooltipRectTrans.pivot = new Vector2(0f, 0.5f);
			bkgImage.sprite = style.rightMiddle;
			break;
		}
		if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{
			Vector2 vector3 = RectTransformUtility.WorldToScreenPoint(Camera.main, pos);
			vector3.x *= (canvas.transform as RectTransform).rect.width / (float)Camera.main.pixelWidth;
			vector3.y *= (canvas.transform as RectTransform).rect.height / (float)Camera.main.pixelHeight;
			tooltipRectTrans.anchoredPosition = vector3 - (canvas.transform as RectTransform).sizeDelta / 2f;
		}
		else
		{
			tooltip.GameObject.transform.position = pos;
			tooltipRectTrans.anchoredPosition += (Vector2)vector;
		}
	}
}

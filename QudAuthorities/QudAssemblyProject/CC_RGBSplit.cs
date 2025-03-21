using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/RGB Split")]
public class CC_RGBSplit : CC_Base
{
	public float amount;

	public float angle;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (amount == 0f)
		{
			Graphics.Blit(source, destination);
			return;
		}
		base.material.SetFloat("_RGBShiftAmount", amount * 0.001f);
		base.material.SetFloat("_RGBShiftAngleCos", Mathf.Cos(angle));
		base.material.SetFloat("_RGBShiftAngleSin", Mathf.Sin(angle));
		Graphics.Blit(source, destination, base.material);
	}
}

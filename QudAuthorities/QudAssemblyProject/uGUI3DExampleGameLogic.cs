using UnityEngine;
using UnityEngine.UI;

public class uGUI3DExampleGameLogic : MonoBehaviour
{
	public GameObject m_DetailPanel;

	public Text m_DetailsLabel1;

	public Text m_DetailsLabel2;

	public Text m_ObjectNameLabel;

	private void Start()
	{
		CloseDetailsPanel();
	}

	public void CloseDetailsPanel()
	{
		m_DetailPanel.SetActive(value: false);
		m_ObjectNameLabel.text = "";
	}

	private void ShowObjectInfo(string detailsText1, string detailsText2)
	{
		m_DetailsLabel1.text = detailsText1;
		m_DetailsLabel2.text = detailsText2;
		m_DetailPanel.SetActive(value: true);
	}

	public void ShowCylinderInfo()
	{
		ShowObjectInfo("A cylinder is a three dimensional shape with two round shapes at either end and two parallel lines connecting the round ends.", "An example of a cylinder is a can of tomato soup.");
	}

	public void HighlightCylinder(bool highlighted, UAP_BaseElement.EHighlightSource source)
	{
		if (highlighted)
		{
			m_ObjectNameLabel.text = "Cylinder";
		}
		else
		{
			m_ObjectNameLabel.text = "";
		}
	}

	public void ShowSphereInfo()
	{
		ShowObjectInfo("A sphere is a geometrical figure that is perfectly round, 3-dimensional and circular, with all points equidistant from a single point in space.", "An example of a sphere is a basketball.");
	}

	public void HighlightSphere(bool highlighted, UAP_BaseElement.EHighlightSource source)
	{
		if (highlighted)
		{
			m_ObjectNameLabel.text = "Sphere";
		}
		else
		{
			m_ObjectNameLabel.text = "";
		}
	}

	public void ShowCubeInfo()
	{
		ShowObjectInfo("A cube is a three-dimensional solid object bounded by six square faces, facets or sides, with three meeting at each vertex.", "An example of a cube is a playing die in a game.");
	}

	public void HighlightCube(bool highlighted, UAP_BaseElement.EHighlightSource source)
	{
		if (highlighted)
		{
			m_ObjectNameLabel.text = "Cube";
		}
		else
		{
			m_ObjectNameLabel.text = "";
		}
	}
}

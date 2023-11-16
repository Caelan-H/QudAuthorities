using EnhancedUI.EnhancedScroller;
using Overlay.MapEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.EditorFormats.Map;
using XRL.World;

public class BlueprintCellView : EnhancedScrollerCellView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public Text blueprintID;

	public UIThreeColorProperties image;

	public static BlueprintCellView selectedCell;

	private MapEditorView mapEditorView => MapEditorView.instance;

	public void SetData(BlueprintScrollerData data)
	{
		blueprintID.text = data.ID;
		RefreshCellView();
	}

	public override void RefreshCellView()
	{
		Button component = base.gameObject.GetComponent<Button>();
		ColorBlock colors = component.colors;
		string text = (mapEditorView.IsSingleObjectBrush() ? mapEditorView.GetBrushText() : "");
		if (blueprintID.text == text)
		{
			colors.normalColor = new Color(0f, 1f, 0f, 0.25f);
			selectedCell = this;
		}
		else
		{
			colors.normalColor = new Color(0f, 0f, 0f, 0f);
		}
		MapFileRegion region = new MapFileRegion(1, 1);
		region.GetOrCreateCellAt(0, 0).Objects.Add(new MapFileObjectBlueprint(blueprintID.text));
		MapFileCellRender mapFileCellRender = new MapFileCellRender();
		mapFileCellRender.RenderBlueprint(GameObjectFactory.Factory.Blueprints[blueprintID.text], new MapFileCellReference(region, 0, 0, null));
		mapFileCellRender.To3C(image);
		component.colors = colors;
	}

	public void OnSelected()
	{
		MapEditorView.instance.SetSelectedBlueprint(blueprintID.text);
	}

	public void OnPointerEnter(PointerEventData data)
	{
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			mapEditorView.FindChild("BlueprintReadout").SetActive(value: true);
			mapEditorView.FindChild("BlueprintReadout/ReadoutText").SetActive(value: true);
			mapEditorView.FindChild("BlueprintReadout/ReadoutText").GetComponent<Text>().text = GameObjectFactory.Factory.Blueprints[blueprintID.text].BlueprintXML();
		}
	}

	public void OnPointerExit(PointerEventData data)
	{
		mapEditorView.FindChild("BlueprintReadout").SetActive(value: false);
	}
}

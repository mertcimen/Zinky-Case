using _Main.Scripts.Containers;
using _Main.Scripts.Datas;
using UnityEditor;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
	private const string DEFAULT_SAVE_PATH = "Assets/_Main/ScriptableObjects/Levels";
	private const float CELL_SIZE = 42f;
	private const float CELL_SPACING = 4f;
	private const float COLOR_SWATCH_SIZE = 30f;
	private const float COLOR_SWATCH_SPACING = 6f;

	private static readonly ColorType[] ColorSelectionOrder =
	{
		ColorType.Blue,
		ColorType.Green,
		ColorType.Red,
		ColorType.Orange,
		ColorType.Yellow,
		ColorType.Purple,
		ColorType.Pink,
		ColorType.None
	};

	private LevelDataSO levelData;
	private ColorType selectedColorType = ColorType.Blue;

	[MenuItem("Tools/Level Editor")]
	private static void OpenWindow()
	{
		var window = GetWindow<LevelEditorWindow>("Level Editor");
		window.minSize = new Vector2(360f, 300f);
		window.Show();
	}

	private void OnGUI()
	{
		HandleKeyboardColorSelection(Event.current);
		DrawHeader();
		EditorGUILayout.Space(8f);
		DrawAssetSelection();

		if (levelData == null)
		{
			EditorGUILayout.HelpBox("Select or create a LevelDataSO asset to edit the grid.", MessageType.Info);
			return;
		}

		if (levelData.EnsureGridData())
			EditorUtility.SetDirty(levelData);
		DrawGridSize();
		EditorGUILayout.Space(8f);
		DrawColorSelection();
		EditorGUILayout.Space(8f);
		DrawGrid();
	}

	private void DrawHeader()
	{
		EditorGUILayout.LabelField("Level Editor", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Edit grid colors with Container.ColorType values.", EditorStyles.miniLabel);
	}

	private void DrawAssetSelection()
	{
		using (new EditorGUILayout.HorizontalScope())
		{
			levelData = (LevelDataSO)EditorGUILayout.ObjectField("Level Data", levelData, typeof(LevelDataSO), false);

			if (GUILayout.Button("Create", GUILayout.Width(80f)))
				CreateNewLevelDataAsset();
		}
	}

	private void DrawGridSize()
	{
		int currentColumns = levelData.ColumnCount;
		int currentRows = levelData.RowCount;

		using (new EditorGUILayout.HorizontalScope())
		{
			EditorGUILayout.LabelField("Grid Size", GUILayout.Width(70f));
			int newColumns = EditorGUILayout.IntField("Columns", currentColumns);
			int newRows = EditorGUILayout.IntField("Rows", currentRows);

			if (newColumns != currentColumns || newRows != currentRows)
			{
				Undo.RecordObject(levelData, "Change Level Grid Size");
				levelData.SetGridSize(newColumns, newRows);
				EditorUtility.SetDirty(levelData);
				Repaint();
			}
		}
	}

	private void DrawColorSelection()
	{
		EditorGUILayout.LabelField("Paint Color", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Use 1-8 keys for quick color selection.", EditorStyles.miniLabel);
		EditorGUILayout.Space(2f);

		using (new EditorGUILayout.HorizontalScope())
		{
			for (int i = 0; i < ColorSelectionOrder.Length; i++)
			{
				ColorType colorType = ColorSelectionOrder[i];
				DrawColorSwatch(i, colorType);
				GUILayout.Space(COLOR_SWATCH_SPACING);
			}
		}

		Rect previewRect = GUILayoutUtility.GetRect(1f, 20f, GUILayout.ExpandWidth(true));
		EditorGUI.DrawRect(previewRect, ToDisplayColor(selectedColorType));
		EditorGUI.LabelField(previewRect, $"Selected: {selectedColorType}", EditorStyles.centeredGreyMiniLabel);
	}

	private void DrawGrid()
	{
		int columns = levelData.ColumnCount;
		int rows = levelData.RowCount;

		for (int row = 0; row < rows; row++)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				for (int column = 0; column < columns; column++)
				{
					ColorType cellColorType = levelData.GetCellColor(row, column);
					Color previousBackground = GUI.backgroundColor;
					GUI.backgroundColor = ToDisplayColor(cellColorType);

					if (GUILayout.Button(GUIContent.none, GUILayout.Width(CELL_SIZE), GUILayout.Height(CELL_SIZE)))
						PaintCell(row, column);

					GUI.backgroundColor = previousBackground;
					GUILayout.Space(CELL_SPACING);
				}
			}

			GUILayout.Space(CELL_SPACING);
		}
	}

	private void PaintCell(int row, int column)
	{
		Undo.RecordObject(levelData, "Paint Level Grid Cell");
		if (!levelData.SetCellColor(row, column, selectedColorType))
			return;

		EditorUtility.SetDirty(levelData);
		Repaint();
	}

	private void CreateNewLevelDataAsset()
	{
		string selectedPath = EditorUtility.SaveFilePanelInProject(
			"Create LevelDataSO",
			"LevelData",
			"asset",
			"Select a location for the new LevelDataSO asset.",
			DEFAULT_SAVE_PATH
		);

		if (string.IsNullOrEmpty(selectedPath))
			return;

		var newLevelData = CreateInstance<LevelDataSO>();
		newLevelData.SetGridSize(5, 4);

		AssetDatabase.CreateAsset(newLevelData, selectedPath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		levelData = newLevelData;
		Selection.activeObject = newLevelData;
		EditorGUIUtility.PingObject(newLevelData);
	}

	private void DrawColorSwatch(int index, ColorType colorType)
	{
		Rect rect = GUILayoutUtility.GetRect(COLOR_SWATCH_SIZE, COLOR_SWATCH_SIZE, GUILayout.Width(COLOR_SWATCH_SIZE), GUILayout.Height(COLOR_SWATCH_SIZE));
		EditorGUI.DrawRect(rect, ToDisplayColor(colorType));
		DrawSelectionBorder(rect, selectedColorType == colorType);

		string shortcutLabel = (index + 1).ToString();
		GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
		{
			alignment = TextAnchor.MiddleCenter,
			normal = { textColor = GetContrastTextColor(colorType) }
		};
		EditorGUI.LabelField(rect, shortcutLabel, labelStyle);

		if (GUI.Button(rect, new GUIContent(string.Empty, $"{shortcutLabel}: {colorType}"), GUIStyle.none))
		{
			selectedColorType = colorType;
			Repaint();
		}
	}

	private static void DrawSelectionBorder(Rect rect, bool isSelected)
	{
		Color borderColor = isSelected ? Color.white : new Color(0f, 0f, 0f, 0.35f);
		const float borderSize = 2f;

		EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, borderSize), borderColor);
		EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMax - borderSize, rect.width, borderSize), borderColor);
		EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, borderSize, rect.height), borderColor);
		EditorGUI.DrawRect(new Rect(rect.xMax - borderSize, rect.yMin, borderSize, rect.height), borderColor);
	}

	private static Color GetContrastTextColor(ColorType colorType)
	{
		return colorType == ColorType.Yellow ? Color.black : Color.white;
	}

	private void HandleKeyboardColorSelection(Event currentEvent)
	{
		if (currentEvent.type != EventType.KeyDown)
			return;

		int selectedIndex = GetQuickSelectIndex(currentEvent.keyCode);
		if (selectedIndex < 0 || selectedIndex >= ColorSelectionOrder.Length)
			return;

		selectedColorType = ColorSelectionOrder[selectedIndex];
		currentEvent.Use();
		Repaint();
	}

	private static int GetQuickSelectIndex(KeyCode keyCode)
	{
		switch (keyCode)
		{
			case KeyCode.Alpha1:
			case KeyCode.Keypad1:
				return 0;
			case KeyCode.Alpha2:
			case KeyCode.Keypad2:
				return 1;
			case KeyCode.Alpha3:
			case KeyCode.Keypad3:
				return 2;
			case KeyCode.Alpha4:
			case KeyCode.Keypad4:
				return 3;
			case KeyCode.Alpha5:
			case KeyCode.Keypad5:
				return 4;
			case KeyCode.Alpha6:
			case KeyCode.Keypad6:
				return 5;
			case KeyCode.Alpha7:
			case KeyCode.Keypad7:
				return 6;
			case KeyCode.Alpha8:
			case KeyCode.Keypad8:
				return 7;
			default:
				return -1;
		}
	}

	private static Color ToDisplayColor(ColorType colorType)
	{
		switch (colorType)
		{
			case ColorType.Blue:
				return new Color(0.23f, 0.47f, 0.96f);
			case ColorType.Green:
				return new Color(0.24f, 0.75f, 0.38f);
			case ColorType.Red:
				return new Color(0.89f, 0.29f, 0.29f);
			case ColorType.Orange:
				return new Color(0.96f, 0.58f, 0.19f);
			case ColorType.Yellow:
				return new Color(0.95f, 0.85f, 0.22f);
			case ColorType.Purple:
				return new Color(0.62f, 0.39f, 0.89f);
			case ColorType.Pink:
				return new Color(0.95f, 0.45f, 0.69f);
			case ColorType.None:
			default:
				return new Color(0.23f, 0.23f, 0.23f);
		}
	}
}

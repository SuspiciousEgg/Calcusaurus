using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileGrid : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The parent container holding the 4x4 tiles (with GridLayoutGroup)")]
    public Transform gridContainer;

    [Tooltip("The text field above the grid that displays the current equation")]
    public TMP_Text equationText;

    [Tooltip("The button that clears the selected tile equation")]
    public Button clearButton;

    [Tooltip("The button that submits the current equation")]
    public Button submitButton;

    [Header("Tile Settings")]
    public int totalTiles = 16;
    public int numberCount = 10;
    public int operatorCount = 6;

    [Header("Colors (Retro UI Matching)")]
    public Color normalTileColor = new Color(0.024f, 0.200f, 0.243f, 1f);       // Dark Teal (#06333E)
    public Color normalTextColor = Color.white;
    public Color selectedTileColor = new Color(0.961f, 0.961f, 0.961f, 1f);     // Light Highlight (#F5F5F5)
    public Color selectedTextColor = new Color(0.024f, 0.200f, 0.243f, 1f);     // Dark Teal text on light tile

    [Header("Font Asset")]
    public TMP_FontAsset pixelFont;

    [Header("Debug Settings")]
    [Tooltip("Temporary debug key to toggle TilePanel on/off")]
    public KeyCode toggleKey = KeyCode.T;

    [System.Serializable]
    public class TileData
    {
        public GameObject gameObject;
        public Button button;
        public Image background;
        public TMP_Text text;
        public string value;
        public bool isSelected;
    }

    [SerializeField]
    private List<TileData> tiles = new List<TileData>();
    private readonly List<TileData> selectedTiles = new List<TileData>();

    private readonly string[] operators = new string[] { "+", "-", "x" };

    void Awake()
    {
        InitializeTileReferences();
    }

    void Start()
    {
        if (clearButton != null)
        {
            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(ClearSelection);
        }

        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitClicked);
        }

        if (tiles.Count == 0 || string.IsNullOrEmpty(tiles[0].value))
        {
            GenerateGrid();
        }
    }

    void OnEnable()
    {
        if (tiles.Count == 0 || string.IsNullOrEmpty(tiles[0].value))
        {
            InitializeTileReferences();
            GenerateGrid();
        }
    }

    void Update()
    {
        // When TilePanel is active, pressing T toggles it off
        if (IsToggleHotkeyPressed())
        {
            ToggleTilePanel();
        }
    }

    /// <summary>
    /// Checks for T key press supporting both Legacy Input Manager and New Input System.
    /// </summary>
    public static bool IsToggleHotkeyPressed()
    {
        try
        {
            if (Input.GetKeyDown(KeyCode.T)) return true;
        }
        catch {}

        #if ENABLE_INPUT_SYSTEM
        try
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.tKey.wasPressedThisFrame) return true;
        }
        catch {}
        #endif

        return false;
    }

    /// <summary>
    /// Toggles the TilePanel active/inactive state.
    /// </summary>
    public static void ToggleTilePanel()
    {
        // 1. Try finding TilePanel under Canvas
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            var panel = canvas.transform.Find("TilePanel");
            if (panel != null)
            {
                bool newState = !panel.gameObject.activeSelf;
                panel.gameObject.SetActive(newState);
                Debug.Log($"[TileGrid] TilePanel toggled: {(newState ? "ON (Visible)" : "OFF (Hidden)")}");
                return;
            }
        }

        // 2. Fallback search across all loaded TileGrid components (including inactive)
        TileGrid[] grids = Resources.FindObjectsOfTypeAll<TileGrid>();
        foreach (var grid in grids)
        {
            if (grid != null && grid.gameObject.scene.isLoaded)
            {
                bool newState = !grid.gameObject.activeSelf;
                grid.gameObject.SetActive(newState);
                Debug.Log($"[TileGrid] TilePanel toggled: {(newState ? "ON (Visible)" : "OFF (Hidden)")}");
                return;
            }
        }
    }

    /// <summary>
    /// Instance method to toggle this TilePanel.
    /// </summary>
    public void Toggle()
    {
        ToggleTilePanel();
    }

    /// <summary>
    /// Runtime runner created when Play Mode begins to listen for the T key even when TilePanel is inactive.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitDebugToggleRunner()
    {
        if (Object.FindAnyObjectByType<TileDebugRunner>() != null) return;

        var runnerGO = new GameObject("[TileDebugRunner]");
        runnerGO.AddComponent<TileDebugRunner>();
        Object.DontDestroyOnLoad(runnerGO);
    }

    /// <summary>
    /// Helper MonoBehaviour that stays active in the background during play mode to detect key presses.
    /// </summary>
    private class TileDebugRunner : MonoBehaviour
    {
        void Update()
        {
            if (TileGrid.IsToggleHotkeyPressed())
            {
                TileGrid.ToggleTilePanel();
            }
        }
    }

    /// <summary>
    /// Finds or caches tile components from the gridContainer children.
    /// </summary>
    public void InitializeTileReferences()
    {
        if (gridContainer == null) return;

        tiles.Clear();
        for (int i = 0; i < gridContainer.childCount; i++)
        {
            Transform child = gridContainer.GetChild(i);
            Button btn = child.GetComponent<Button>();
            Image img = child.GetComponent<Image>();
            TMP_Text txt = child.GetComponentInChildren<TMP_Text>();

            if (btn != null && img != null && txt != null)
            {
                TileData tile = new TileData
                {
                    gameObject = child.gameObject,
                    button = btn,
                    background = img,
                    text = txt,
                    value = txt.text,
                    isSelected = false
                };

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnTileClicked(tile));

                tiles.Add(tile);
            }
        }
    }

    /// <summary>
    /// Generates randomly mixed values (roughly 10 numbers and 6 operators) and fills the tiles.
    /// </summary>
    public void GenerateGrid()
    {
        ClearSelection();

        // 1. Generate 10 numbers (1-9)
        List<string> values = new List<string>();
        for (int i = 0; i < numberCount; i++)
        {
            int num = Random.Range(1, 10);
            values.Add(num.ToString());
        }

        // 2. Generate 6 operators (+, -, x) - balanced 2 of each
        for (int i = 0; i < operatorCount; i++)
        {
            string op = operators[i % operators.Length];
            values.Add(op);
        }

        // 3. Shuffle values (Fisher-Yates)
        for (int i = values.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            string temp = values[i];
            values[i] = values[rnd];
            values[rnd] = temp;
        }

        // Ensure we have tile references
        if (tiles.Count == 0 && gridContainer != null)
        {
            InitializeTileReferences();
        }

        // 4. Assign values to tiles
        for (int i = 0; i < tiles.Count && i < values.Count; i++)
        {
            TileData tile = tiles[i];
            tile.value = values[i];
            tile.isSelected = false;

            if (tile.text != null)
            {
                tile.text.text = tile.value;
                tile.text.color = normalTextColor;
                if (pixelFont != null) tile.text.font = pixelFont;
            }

            if (tile.background != null)
            {
                tile.background.color = normalTileColor;
            }
        }

        UpdateEquationText();
    }

    /// <summary>
    /// Checks whether a tile value is a number (1-9).
    /// </summary>
    public bool IsNumber(string val)
    {
        return int.TryParse(val, out _);
    }

    /// <summary>
    /// Checks whether a tile value is an operator (+, -, x).
    /// </summary>
    public bool IsOperator(string val)
    {
        return val == "+" || val == "-" || val == "x" || val == "X" || val == "×" || val == "*";
    }

    /// <summary>
    /// Called when any tile is clicked.
    /// Handles single-selection, deselecting tile and subsequent chain,
    /// and enforces alternating number/operator pattern.
    /// </summary>
    public void OnTileClicked(TileData tile)
    {
        if (tile == null) return;

        // Rule 1: Clicking a selected tile again deselects it and removes it and everything after it from the chain.
        if (tile.isSelected)
        {
            int index = selectedTiles.IndexOf(tile);
            if (index >= 0)
            {
                for (int i = selectedTiles.Count - 1; i >= index; i--)
                {
                    DeselectTile(selectedTiles[i]);
                    selectedTiles.RemoveAt(i);
                }
                UpdateEquationText();
            }
            return;
        }

        // Rule 3: The equation must alternate number, operator, number.
        // Ignore clicks that break this pattern (e.g. starting with operator, or two operators/numbers in a row).
        bool isNum = IsNumber(tile.value);
        bool isOp = IsOperator(tile.value);

        if (selectedTiles.Count == 0)
        {
            // Must start with a number
            if (!isNum) return;
        }
        else
        {
            bool lastIsNum = IsNumber(selectedTiles[selectedTiles.Count - 1].value);
            if (lastIsNum && !isOp) return;   // Number after number -> ignore
            if (!lastIsNum && !isNum) return; // Operator after operator -> ignore
        }

        // Select and highlight tile
        SelectTile(tile);
        selectedTiles.Add(tile);
        UpdateEquationText();
    }

    private void SelectTile(TileData tile)
    {
        tile.isSelected = true;
        if (tile.background != null)
        {
            tile.background.color = selectedTileColor;
        }
        if (tile.text != null)
        {
            tile.text.color = selectedTextColor;
        }
    }

    private void DeselectTile(TileData tile)
    {
        tile.isSelected = false;
        if (tile.background != null)
        {
            tile.background.color = normalTileColor;
        }
        if (tile.text != null)
        {
            tile.text.color = normalTextColor;
        }
    }

    /// <summary>
    /// Resets all tile selections and clears the equation text.
    /// </summary>
    public void ClearSelection()
    {
        foreach (TileData tile in selectedTiles)
        {
            DeselectTile(tile);
        }
        selectedTiles.Clear();
        UpdateEquationText();
    }

    /// <summary>
    /// Handles Submit button click: logs the current equation to Console and clears selection.
    /// </summary>
    public void OnSubmitClicked()
    {
        string equation = GetCurrentEquation();
        Debug.Log($"Submitted equation: {equation}");
        ClearSelection();
    }

    /// <summary>
    /// Updates the equation display live with spaces between tile values (e.g. "3 + 4 x 2").
    /// </summary>
    private void UpdateEquationText()
    {
        if (equationText == null) return;

        if (selectedTiles.Count == 0)
        {
            equationText.text = "";
        }
        else
        {
            List<string> parts = new List<string>();
            foreach (TileData t in selectedTiles)
            {
                parts.Add(t.value);
            }
            equationText.text = string.Join(" ", parts);
        }
    }

    /// <summary>
    /// Returns the currently formed equation string.
    /// </summary>
    public string GetCurrentEquation()
    {
        if (selectedTiles.Count == 0) return "";
        List<string> parts = new List<string>();
        foreach (TileData t in selectedTiles)
        {
            parts.Add(t.value);
        }
        return string.Join(" ", parts);
    }
}

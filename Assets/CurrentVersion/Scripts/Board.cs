using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Runtime.ConstrainedExecution;

public delegate void BoardCompleted();

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[] {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
        KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z, 
    };

    private static readonly string[] SEPARATOR = new string[] { "\r\n", "\r", "\n" };

    public event BoardCompleted OnCompleted;
    private bool checkWord = true;
    private Row[] rows;
    public Component rowPrefab;
    public Component tileColumnPrefab;
    public Component tilePrefab;
    public GlassAnimation glass;
    public bool isWordleSolved { get; private set; } = false;
    public bool isScrabbleGame = false;

    private int rowIndex;
    private int columnIndex;
    private int columnLockIndex = -1;
    private float continuationDelay;
    private float continuationTimePassed = 0.0f;
    private Row continuationRow;
    private bool isContinuing = false;
    private float roundTime;
    
    private string[] solutions;
    private string[] validWords;
    public string word { get; private set; }
    private char lastLetter;
    public char LastLetter => lastLetter;
    private Title titleReference;
    public bool IsRegularGame = true; //=> titleReference != null && titleReference.IsRegularGame;

    [Header("Tiles")]
    public Tile.State emptyState;
    public Tile.State occupidedState;
    public Tile.State correctState;
    public Tile.State wrongSpotState;
    public Tile.State incorrectState;
    public Tile.State lockedState;
    public Tile.State validScrabbleWordState;

    [Header("UI")]
    public GameObject tryAgainButton;
    public GameObject newWordButton;
    public GameObject invalidWordText;
    private string[] validScrabbleWords;

    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();
        titleReference = FindAnyObjectByType<Title>();
    }

    public void LoadData()
    {
        TextAsset textFile = Resources.Load("official_wordle_common") as TextAsset;
        solutions = textFile.text.Split(SEPARATOR, System.StringSplitOptions.None);

        textFile = Resources.Load("official_wordle_all") as TextAsset;
        validWords = textFile.text.Split(SEPARATOR, System.StringSplitOptions.None);

        TextAsset scrabText = Resources.Load("dictionary") as TextAsset;
        validScrabbleWords = scrabText.text.Split(SEPARATOR, System.StringSplitOptions.None)
            .Where(sword => sword.Length == 6)
            .ToArray();
    }

    public void GenerateRows(int numRows = 6) {
        ClearBoard();
        rows = new Row[numRows];
        for (int i = 0; i < numRows; i++) {
            Component rowComponent = Instantiate(rowPrefab, transform);
            rowComponent.name = i.ToString();
            Row row = rowComponent.GetComponent<Row>();
            rows[i] = row;
        }
    }

    public void ClearBoard()
    {
        if (rows != null) {
            for (int i = 0; i < rows.Length; i++) {
                if (rows[i] != null) {
                     if (IsRegularGame) { AudioManager.instance.PlayButtonSound(); }
                    Destroy(rows[i].gameObject);
                }
            }
        }
        rowIndex = 0;
        columnIndex = 0;
        columnLockIndex = -1;
        continuationRow = null;
        isContinuing = false;
        checkWord = true;
        roundTime = 0f;
        glass.Show();
    }

    public void SetRandomWord()
    {
        if (solutions == null) {
            LoadData();
        }
        word = solutions[Random.Range(0, solutions.Length)];
        word = word.ToLower().Trim();
        lastLetter = word.Last();
    }

    private void Update()
    {
        if (enabled && rows.Length > 0) {
            roundTime += Time.deltaTime;

            if (isContinuing)
            {
                continuationTimePassed += Time.deltaTime;
                if (continuationTimePassed >= continuationDelay)
                {
                    isContinuing = false;
                    isScrabbleGame = true;
                    GenerateRows(2);
                    return;
                }
            }

            // Wordle downward tile creation logic
            if (isContinuing && continuationRow != null)
            {
                continuationTimePassed += Time.deltaTime;
                if (continuationTimePassed >= continuationDelay)
                {
                    // Create downward tiles from one of the current tiles, chosen at random
                    int tileColumnCount = continuationRow.tiles.Length - 1;
                    int rowDownIndex = Random.Range(0, tileColumnCount);
                    Tile rowDownTile = continuationRow.tiles[rowDownIndex];
                    rowDownTile.SetState(lockedState);

                    // Position column so it will be below the selected random tile
                    RectTransform rowDownTransform = rowDownTile.GetComponent<RectTransform>();
                    float columnTotalPadding = Constants.TILE_COLUMN_PADDING * (tileColumnCount - 1);
                    Component tileColumnComponent = Instantiate(tileColumnPrefab, rowDownTile.transform);
                    RectTransform tileColumnTransform = tileColumnComponent.GetComponent<RectTransform>();
                    tileColumnTransform.sizeDelta = new Vector2(
                        tileColumnTransform.sizeDelta.x, (rowDownTransform.sizeDelta.y * tileColumnCount) + columnTotalPadding
                    );
                    tileColumnTransform.localPosition = new Vector3(
                        tileColumnTransform.localPosition.x,
                        -((tileColumnTransform.sizeDelta.y / 2) + (columnTotalPadding + Constants.TILE_COLUMN_PADDING)),
                        tileColumnTransform.localPosition.z
                    );

                    // Create tiles in tile column, and update this row to include these tiles
                    Tile[] newRowTiles = new Tile[tileColumnCount + 1];
                    newRowTiles[0] = rowDownTile;
                    for (int i = 1; i < newRowTiles.Length; i++) {
                        Component tileComponent = Instantiate(tilePrefab, tileColumnTransform);
                        Tile tile = tileComponent.GetComponent<Tile>();
                        newRowTiles[i] = tile;
                    }
                    continuationRow.UpdateTiles(newRowTiles);
                    rows = new Row[1] {continuationRow};
                    rowIndex = 0;
                    columnIndex = 1;
                    columnLockIndex = 0;
                    checkWord = false;
                    continuationRow = null;
                }
            }

            // Handle input differently based on game mode
            Row currentRow = rows[rowIndex];

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                columnIndex = Mathf.Max(columnIndex - 1, columnLockIndex + 1);
                currentRow.tiles[columnIndex].SetLetter('\0');
                currentRow.tiles[columnIndex].SetState(emptyState);
                invalidWordText.SetActive(false);
            }
            else if (columnIndex >= rows[rowIndex].tiles.Length)
            {
                if (Input.GetKeyDown(KeyCode.Return)) {
                    SubmitRow(currentRow);
                }
            }
            else
            {
                for (int i = 0; i < SUPPORTED_KEYS.Length; i++)
                {
                    if (Input.GetKeyDown(SUPPORTED_KEYS[i]))
                    {
                        currentRow.tiles[columnIndex].SetLetter((char)SUPPORTED_KEYS[i]);
                        currentRow.tiles[columnIndex].SetState(occupidedState);
                        columnIndex++;
                        break;
                    }
                }
            }

            if (roundTime >= Constants.GLASS_WARN_ROUND_TIME) {
                glass.StartWarning();
            }
        }
    }

    private bool IsValidScrabbleWord(string word)
    {
        return validScrabbleWords.Contains(word.ToLower());
    }

    private void SubmitRow(Row row)
    {
        glass.Flip();
        roundTime = 0f;

        if (isScrabbleGame) {
            string enteredScrabbleWord = row.word.ToLower();

            // Check the word is valid and starts with the last letter of Wordle
            if (!validScrabbleWords.Contains(enteredScrabbleWord) || enteredScrabbleWord[0] != lastLetter)
            {
                invalidWordText.SetActive(true);
                return;
            }
            
            lastLetter = enteredScrabbleWord.Last();
            foreach (var tile in row.tiles)
            {
                tile.SetState(validScrabbleWordState);
            }

            rowIndex++;
            columnIndex = 0;
            return;
        }

        // Wordle Validation
        if (!IsValidWord(row.word))
        {
            invalidWordText.SetActive(true);
            return;
        }

        string remaining = word;
        if (!checkWord)
        {
            for (int i = 0; i < row.tiles.Length; i++)
            {
                if (i != columnLockIndex)
                {
                    row.tiles[i].SetState(incorrectState);
                }
            }
        }
        else
        {
            // Check correct/incorrect letters first
            for (int i = 0; i < row.tiles.Length; i++)
            {
                if (i != columnLockIndex)
                {
                    Tile tile = row.tiles[i];

                    if (tile.letter == word[i])
                    {
                        tile.SetState(correctState);

                        remaining = remaining.Remove(i, 1);
                        remaining = remaining.Insert(i, " ");
                    }
                    else if (!word.Contains(tile.letter))
                    {
                        tile.SetState(incorrectState);
                    }
                }
            }

            // Check for wrong spots after
            for (int i = 0; i < row.tiles.Length; i++)
            {
                if (i != columnLockIndex)
                {
                    Tile tile = row.tiles[i];

                    if (tile.state != correctState && tile.state != incorrectState)
                    {
                        if (remaining.Contains(tile.letter))
                        {
                            tile.SetState(wrongSpotState);

                            int index = remaining.IndexOf(tile.letter);
                            remaining = remaining.Remove(index, 1);
                            remaining = remaining.Insert(index, " ");
                        }
                        else
                        {
                            tile.SetState(incorrectState);
                        }
                    }
                }
            }
        }

        if (HasWonWordle(row)) {
            // Start fading effect
            continuationDelay = Constants.ROW_FADE_TIME;
            continuationTimePassed = 0.0f;
            for (int i = 0; i < rows.Length; i++) {
                if (i != rowIndex) {
                    Row fadeRow = rows[i];
                    for (int j = 0; j < fadeRow.tiles.Length; j++) {
                        if (fadeRow.tiles[j].state == null) {
                            fadeRow.tiles[j].SetState(emptyState);
                        }
                    }
                    fadeRow.Disappear(i);
                }
                continuationDelay += Constants.ROW_FADE_DELAY_FACTOR;
            }
            continuationRow = row;
            isContinuing = true;
        }
        else {
            rowIndex++;
            columnIndex = 0;
            columnLockIndex = -1;
            if (rowIndex >= rows.Length) {
                OnCompleted?.Invoke();
                glass.Hide();
            }
        }
    }

    private bool IsValidWord(string word)
    {
        for (int i = 0; i < validWords.Length; i++)
        {
            if (string.Equals(word, validWords[i], System.StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }

    private bool HasWonWordle(Row row)
    {
        if (row.tiles.All(tile => tile.state == correctState))
        {
            isWordleSolved = true;
            if (IsRegularGame)
            {
                AudioManager.instance.PlayWin();
            }

            isContinuing = true;
            return true;
        }
        return false;
    }

    private void OnEnable()
    {
        tryAgainButton.SetActive(false);
        newWordButton.SetActive(false);
    }

    private void OnDisable()
    {
        tryAgainButton.SetActive(true);
        newWordButton.SetActive(true);
    }

    public void SetLastLetter(char letter)
    {
        lastLetter = letter;
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public delegate void BoardCompleted();

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[] {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
        KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z
    };

    private static readonly string[] SEPARATOR = new string[] { "\r\n", "\r", "\n" };

    public event BoardCompleted OnCompleted;

    [Header("Prefabs / References")]
    public Component rowPrefab;
    public GlassAnimation glass;

    [Header("UI")]
    public GameObject tryAgainButton;
    public GameObject newWordButton;
    public GameObject invalidWordText;

    [Header("Tiles - States")]
    public Tile.State emptyState;
    public Tile.State occupidedState;
    public Tile.State correctState;
    public Tile.State wrongSpotState;
    public Tile.State incorrectState;
    public Tile.State lockedState;
    public Tile.State validScrabbleWordState;

    private Row[] rows;
    private string[] solutions;
    private string[] validWords;
    private string[] validScrabbleWords;

    private int rowIndex;
    private int columnIndex;
    private int columnLockIndex = -1;
    private bool checkWord = true;

    public string word { get; private set; }
    public bool isWordleSolved { get; private set; }
    private char lastLetter;
    public char LastLetter => lastLetter;

    public bool isScrabbleGame = false;
    public bool IsRegularGame = true;

    private float roundTime;

    private bool isContinuing = false;
    private float continuationDelay = 0f;
    private float continuationTimePassed = 0f;
    private Row continuationRow;

    private void Awake()
    {
        validScrabbleWordState.fillColor   = correctState.fillColor;
        validScrabbleWordState.outlineColor = correctState.outlineColor;
        rows = GetComponentsInChildren<Row>();
    }

    private void Start()
    {
        LoadData();

        ClearBoard();
        GenerateRows(6, true);
        SetRandomWord();
    }

    private void Update()
    {
        if (!enabled || rows == null || rows.Length == 0) return;
        roundTime += Time.deltaTime;
        if (isContinuing)
        {
            continuationTimePassed += Time.deltaTime;
            if (continuationTimePassed >= continuationDelay)
            {
                isContinuing = false;
                isScrabbleGame = true;
                GenerateRows(2, false);
            }
        }

        if (roundTime >= Constants.GLASS_WARN_ROUND_TIME)
        {
            glass.StartWarning();
        }

        Row currentRow = rows[rowIndex];

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            columnIndex = Mathf.Max(columnIndex - 1, columnLockIndex + 1);
            currentRow.tiles[columnIndex].SetLetter('\0');
            currentRow.tiles[columnIndex].SetState(emptyState);
            invalidWordText.SetActive(false);
        }
        else if (columnIndex >= currentRow.tiles.Length)
        {

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Debug.Log("Enter pressed -> SubmitRow");
                SubmitRow(currentRow);
            }
        }
        else
        {
            foreach (KeyCode key in SUPPORTED_KEYS)
            {
                if (Input.GetKeyDown(key))
                {
                    currentRow.tiles[columnIndex].SetLetter((char)key);
                    currentRow.tiles[columnIndex].SetState(occupidedState);
                    columnIndex++;
                    break;
                }
            }
        }
    }

    private void LoadData()
    {
        if (solutions == null || solutions.Length == 0)
        {
            TextAsset common = Resources.Load("official_wordle_common") as TextAsset;
            if (common != null)
            {
                solutions = common.text.Split(SEPARATOR, System.StringSplitOptions.None);
            }
        }

        if (validWords == null || validWords.Length == 0)
        {
            TextAsset allWords = Resources.Load("official_wordle_all") as TextAsset;
            if (allWords != null)
            {
                validWords = allWords.text.Split(SEPARATOR, System.StringSplitOptions.None);
            }
        }

        if (validScrabbleWords == null || validScrabbleWords.Length == 0)
        {
            TextAsset scrabText = Resources.Load("dictionary") as TextAsset;
            if (scrabText != null)
            {
                validScrabbleWords = scrabText.text
                    .Split(SEPARATOR, System.StringSplitOptions.None)
                    .Where(s => s.Length == 5)
                    .Select(s => s.ToLower())
                    .ToArray();
            }
        }
    }

    public void SetRandomWord()
    {
        if (solutions == null || solutions.Length == 0) { LoadData(); }
        if (solutions == null || solutions.Length == 0)
        {
            Debug.LogWarning("No solutions found in Resources");
            word = "random";
        }
        else
        {
            word = solutions[Random.Range(0, solutions.Length)].Trim().ToLower();
        }

        lastLetter = word[word.Length - 1];
    }

    public void ClearBoard()
    {
        if (rows != null)
        {
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] != null)
                {
                    Destroy(rows[i].gameObject);
                }
            }
        }
        rows = new Row[0];
        rowIndex = 0;
        columnIndex = 0;
        columnLockIndex = -1;
        checkWord = true;
        isContinuing = false;
        continuationRow = null;
        roundTime = 0f;

        glass.Show();
    }

    public void PartialClear()
    {
        Debug.LogWarning("PartialClear called");
        if (continuationRow == null)
        {
            Debug.LogWarning("PartialClear called but continuationRow is null");
            return;
        }

        // Destroy every row except the winning one
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] != null && rows[i] != continuationRow)
            {
                Destroy(rows[i].gameObject);
            }
        }

        rows = new Row[] { continuationRow };

        rowIndex = 1;
        columnIndex = 0;
        columnLockIndex = -1;

        glass.Show();

        roundTime = 0f;
        checkWord = true;
    }

    public void GenerateRows(int numRows = 6, bool partialClear = false)
    {
        if (partialClear == false)
        {
            ClearBoard();
        }

        if (partialClear == true)
        {
            PartialClear();
        }
        
        rows = new Row[numRows];
        for (int i = 0; i < numRows; i++)
        {
            Component rowComp = Instantiate(rowPrefab, transform);
            rowComp.name = "Row " + i;
            Row row = rowComp.GetComponent<Row>();
            rows[i] = row;
        }

        rowIndex = 0;
        columnIndex = 0;
        columnLockIndex = -1;
    }

    private void SubmitRow(Row row)
    {
        glass.Flip();
        roundTime = 0f;

        if (isScrabbleGame)
        {
            string enteredScrabbleWord = row.word.ToLower();
            Debug.Log($"[Scrabble] Player typed: {enteredScrabbleWord}");

            if (!IsValidScrabbleWord(enteredScrabbleWord) || enteredScrabbleWord[0] != lastLetter)
            {
                invalidWordText.SetActive(true);
                return;
            }

            foreach (Tile t in row.tiles)
            {
                t.SetState(validScrabbleWordState);
            }

            lastLetter = enteredScrabbleWord[enteredScrabbleWord.Length - 1];

            rowIndex++;
            columnIndex = 0;
            if (rowIndex >= rows.Length)
            {
                OnCompleted?.Invoke();
                glass.Hide();
                enabled = false;
            }
            return;
        }

        if (!IsValidWord(row.word))
        {
            invalidWordText.SetActive(true);
            Debug.Log("[Wordle] Invalid typed word: " + row.word);
            return;
        }

        string solution = word;
        string remaining = solution;

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
            for (int i = 0; i < row.tiles.Length; i++)
            {
                if (i == columnLockIndex) continue;
                Tile tile = row.tiles[i];

                if (tile.letter == solution[i])
                {
                    tile.SetState(correctState);
                    remaining = remaining.Remove(i, 1).Insert(i, " ");
                }
                else if (!solution.Contains(tile.letter))
                {
                    tile.SetState(incorrectState);
                }
            }

            for (int i = 0; i < row.tiles.Length; i++)
            {
                if (i == columnLockIndex) continue;
                Tile tile = row.tiles[i];

                if (tile.state != correctState && tile.state != incorrectState)
                {
                    if (remaining.Contains(tile.letter))
                    {
                        tile.SetState(wrongSpotState);
                        int idx = remaining.IndexOf(tile.letter);
                        remaining = remaining.Remove(idx, 1).Insert(idx, " ");
                    }
                    else
                    {
                        tile.SetState(incorrectState);
                    }
                }
            }
        }

        if (HasWonWordle(row))
        {
            Debug.Log("Wordle solved");
            isWordleSolved = true;

            continuationDelay = Constants.ROW_FADE_TIME;
            continuationTimePassed = 0f;
            isContinuing = true;

            for (int i = 0; i < rows.Length; i++)
            {
                if (i != rowIndex)
                {
                    Row fadeRow = rows[i];
                    fadeRow.Disappear(i);
                }
                continuationDelay += Constants.ROW_FADE_DELAY_FACTOR;
            }

            continuationRow = row;
        }
        else
        {
            rowIndex++;
            columnIndex = 0;
            columnLockIndex = -1;

            if (rowIndex >= rows.Length)
            {
                OnCompleted?.Invoke();
                glass.Hide();
                enabled = false;
            }
        }
    }

    private bool HasWonWordle(Row row)
    {
        bool allCorrect = row.tiles.All(t => t.state == correctState);
        return allCorrect;
    }

    private bool IsValidWord(string typedWord)
    {
        if (validWords == null || validWords.Length == 0) return false;
        return validWords.Any(v => string.Equals(v, typedWord, System.StringComparison.OrdinalIgnoreCase));
    }

    private bool IsValidScrabbleWord(string typedWord)
    {
        if (validScrabbleWords == null || validScrabbleWords.Length == 0) return false;
        return validScrabbleWords.Contains(typedWord.ToLower());
    }

    public void SetLastLetter(char letter)
    {
        lastLetter = letter;
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
}
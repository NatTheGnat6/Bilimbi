using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;


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
    private enum ContinuationType {
        None,
        BuildOff,
        BuildBetween
    }
    public event Helper.Event OnCompleted;
    private bool checkWord = true;
    private Row[] rows;
    public Row rowPrefab;
    public Tile tilePrefab;
    public GlassAnimation glass;
    public bool isWordleSolved { get; private set; } = false;

    private int rowIndex;
    private int columnIndex;
    private int columnSkipIndex = -1;
    private bool waitForFade = false;
    private int rowsFading = 0;
    private ContinuationType continuationType = ContinuationType.None;
    private int continuationCount = 0;
    private Row continuationOffRow;
    private float roundTime;
    
    private string[] solutions;
    private string[] validWords;
    private bool submittingRow = false;
    private Row submittedRow;
    private bool ignoreDestroyUpdate = false;
    public string word { get; private set; }
    private char lastLetter;
    public char LastLetter => lastLetter;
    private Title titleReference;
    public bool IsRegularGame = true; //=> titleReference != null && titleReference.IsRegularGame;

    [Header("UI")]
    public GameObject tryAgainButton;
    public GameObject newWordButton;
    public GameObject invalidWordText;

    public Row CreateRow(int length, Row.Direction direction = Row.Direction.Horizontal, Tile tileOff = null, int tileOffIndex = 0)
    {
        bool hasTileOff = tileOff != null;
        Row row = Instantiate(rowPrefab, transform);
        row.length = length;
        row.direction = direction;
        if (hasTileOff) {
            tileOff.SetState(Tile.State.Locked);
            row.tileOff = tileOff;
            row.tileOffIndex = tileOffIndex;
        }
        return row;
    }

    private void UpdateRows(Row addRow = null)
    {
        List<Row> newRows = new List<Row>();
        int rowIndex = 0;
        for (int i = 0; i < rows.Length; i++) {
            Row row = rows[i];
            if (row != null && !row.HasDestroyed) {
                row.SetOrder(rowIndex);
                newRows.Add(row);
                row.name = rowIndex.ToString();
                rowIndex++;
            }
        }
        if (addRow != null) {
            addRow.SetOrder(rowIndex);
            newRows.Add(addRow);
            addRow.name = rowIndex.ToString();
        }
        rows = newRows.ToArray();
    }

    private void AddRow(Row row)
    {
        row.OnDestroyed += () => {
            if (!ignoreDestroyUpdate) {
                UpdateRows();
            }
        };
        UpdateRows(row);
    }
    private void DisappearRow(int order)
    {
        rowsFading++;
        Row row = rows[order];
        row.Disappear(order);
        row.OnDisappeared += () => {
            rowsFading--;
        };
    }

    private void Awake()
    {
        titleReference = FindAnyObjectByType<Title>();
    }

    public void LoadData()
    {
        TextAsset textFile = Resources.Load("official_wordle_common") as TextAsset;
        solutions = textFile.text.Split(SEPARATOR, System.StringSplitOptions.None);

        textFile = Resources.Load("official_wordle_all") as TextAsset;
        validWords = textFile.text.Split(SEPARATOR, System.StringSplitOptions.None);
    }

    public void GenerateRows() {
        ClearBoard();  
        rows = new Row[6];
        for (int i = 0; i < 6; i++) {
            Row row = CreateRow(5);
            AddRow(row);
            row.AddTiles();
        }
    }

    public void ClearBoard()
    {
        if (rows != null) {
            ignoreDestroyUpdate = true;
            for (int i = 0; i < rows.Length; i++) {
                print(rows[i]);
                if (rows[i] != null) {
                    rows[i].DestroyRow();
                }
            }
            if (IsRegularGame) {
                AudioManager.instance.PlayButtonSound();
            }
            ignoreDestroyUpdate = false;
            UpdateRows();

        }
        rowIndex = 0;
        columnIndex = 0;
        columnSkipIndex = -1;
        continuationCount = 0;
        continuationOffRow = null;
        continuationType = ContinuationType.None;
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
        name = word;
    }

    private void Update()
    {
        if (waitForFade) {
            if (rowsFading == 0) {
                waitForFade = false;
            }
        } else if (enabled && rows != null && rows.Length > 0) {
            if (submittingRow && submittedRow != null) {
                if (submittedRow.IsRevealed) {
                    submittingRow = false;
                    ContinuationType continueType = GetContinuationType(submittedRow);
                    if (continueType != ContinuationType.None) {
                        // Fade rows that aren't winning row out
                        for (int i = 0; i < rows.Length; i++) {
                            if (i != rowIndex) {
                                DisappearRow(i);
                            }
                        }
                        waitForFade = true;
                        continuationOffRow = submittedRow;
                        continuationType = continueType;
                        continuationCount++;
                        print("CONTINUE: " + rows.Length + " | " + rowIndex);
                    } else {
                        // No continuation, see if we have reached the final row. If not, continue to next rows
                        print("NONE: " + rows.Length + " | " + rowIndex);
                        if (rowIndex >= rows.Length)
                        {
                            OnCompleted?.Invoke();
                            glass.Hide();
                        }
                        else
                        {
                            if (IsRegularGame)
                            {
                                AudioManager.instance.PlayWrongGuess();
                            }
                        }
                        rowIndex++;
                        columnIndex = 0;
                        columnSkipIndex = -1;
                        continuationCount = 0;
                        if (rowIndex >= rows.Length) {
                            if (IsRegularGame && continuationType == ContinuationType.None)
                            {
                                AudioManager.instance.PlayLose();
                            }
                            OnCompleted?.Invoke();
                            glass.Hide();
                        }
                    }
                    submittedRow = null;
                }
            } else {
                roundTime += Time.deltaTime;
                if (continuationType != ContinuationType.None && continuationOffRow != null)
                {
                    // Select which tile off of the last row should be the "locked" tile
                    int tileOffRowIndex = continuationOffRow.tileOffIndex;
                    while (tileOffRowIndex == continuationOffRow.tileOffIndex) {
                        tileOffRowIndex = Random.Range(0, continuationOffRow.tiles.Length - 1);
                    }

                    // Create a row off of the selected tile
                    Tile tileOff = continuationOffRow.tiles[tileOffRowIndex];
                    continuationOffRow.RemoveTile(tileOffRowIndex);
                    int continueLength = 5;
                    int tileOffIndex = 0;
                    Row continueRow = CreateRow(
                        continueLength,
                        continuationOffRow.direction == Row.Direction.Horizontal ? Row.Direction.Vertical : Row.Direction.Horizontal,
                        tileOff, tileOffIndex
                    );
                    AddRow(continueRow);
                    RectTransform tileOffTransform = tileOff.GetComponent<RectTransform>();
                    print(tileOffTransform.anchoredPosition);
                    tileOff.transform.SetParent(continueRow.transform, true);
                    print(tileOffTransform.anchoredPosition);
                    continueRow.AddTile(tileOff, tileOffIndex);
                    print(tileOffTransform.anchoredPosition);
                    for (int j = 0; j < continueLength; j++) {
                        if (j != tileOffIndex) {
                            continueRow.AddTile(continueRow.CreateTile(j), j);
                        }
                    }
                    rowIndex = rows.Length - 1;
                    columnIndex = tileOffIndex == 0 ? 1 : 0;
                    columnSkipIndex = tileOffIndex;
                    continuationOffRow = null;
                    checkWord = false;
                    continuationType = ContinuationType.None;
                }
                // print("ROW: " + rowIndex + " | " + rows.Length);
                Row currentRow = rows[rowIndex];

                if (currentRow != null) {
                    if (Input.GetKeyDown(KeyCode.Backspace))
                    {
                        print(columnIndex);
                        columnIndex = System.Math.Clamp(columnIndex - 1, 0, currentRow.tiles.Length);
                        if (columnIndex == columnSkipIndex) {
                            columnIndex = columnSkipIndex + (columnSkipIndex == 0 ? 1 : -1);
                        }
                        print(columnIndex);
                        currentRow.tiles[columnIndex].SetLetter('\0');
                        currentRow.tiles[columnIndex].SetState(Tile.State.Empty);
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
                                currentRow.tiles[columnIndex].SetState(Tile.State.Occupied);
                                columnIndex++;
                                if (columnIndex == columnSkipIndex) {
                                    columnIndex = columnSkipIndex + 1;
                                }
                                break;
                            }
                        }
                    }
                }

                if (roundTime >= Constants.GLASS_WARN_ROUND_TIME) {
                    glass.StartWarning();
                }
            }
        }
    }

    private void SubmitRow(Row row)
    {
        glass.Flip();
        roundTime = 0f;
        if (!IsValidWord(row.word))
        {
            invalidWordText.SetActive(true);
            return;
        }

        string remaining = word;

        Tile.State[] submittedStates = new Tile.State[row.tiles.Length];
        print(row.word + " | " + word + " | " + remaining);
        if (!checkWord)
        {
            // If not checking word set all to inactive
            for (int i = 0; i < row.tiles.Length; i++)
            {
                submittedStates[i] = Tile.State.Incorrect;
            }
        }
        else
        {
            // Check correct/incorrect letters first
            for (int i = 0; i < row.tiles.Length; i++)
            {
                Tile tile = row.tiles[i];

                if (tile.letter == word[i])
                {
                    submittedStates[i] = Tile.State.Correct;

                    remaining = remaining.Remove(i, 1);
                    remaining = remaining.Insert(i, " ");
                }
                else if (!word.Contains(tile.letter))
                {
                    submittedStates[i] = Tile.State.Incorrect;
                }
            }

            // Check for wrong spots after
            for (int i = 0; i < row.tiles.Length; i++)
            {
                Tile tile = row.tiles[i];

                if (submittedStates[i] != Tile.State.Correct && submittedStates[i] != Tile.State.Incorrect)
                {
                    if (remaining.Contains(tile.letter))
                    {
                        submittedStates[i] = Tile.State.WrongSpot;

                        int index = remaining.IndexOf(tile.letter);
                        remaining = remaining.Remove(index, 1);
                        remaining = remaining.Insert(index, " ");
                    }
                    else
                    {
                        submittedStates[i] = Tile.State.Incorrect;
                    }
                }
            }
        }
        print(rows.Length + " | " + rowIndex);
        row.Reveal(submittedStates);
        submittedRow = row;
        submittingRow = true;
        print(rows.Length + " | " + rowIndex);
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

    private ContinuationType GetContinuationType(Row row)
    {
        if (HasWonWordle(row)) {
            return ContinuationType.BuildOff;
        } else if (continuationCount == 1) {
            return ContinuationType.BuildBetween;
        }
        return ContinuationType.None;
    }

    private bool HasWonWordle(Row row)
    {
        if (row.tiles.All(tile => tile.state == Tile.State.Correct))
        {
            isWordleSolved = true;
            if (IsRegularGame)
            {
                AudioManager.instance.PlayWin();
            }
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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

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
    private int rowIndex;
    private int columnIndex;
    private int columnLockIndex = -1;
    private float continuationDelay;
    private float continuationTimePassed = 0.0f;
    private Row continuationRow;
    
    private string[] solutions;
    private string[] validWords;
    private string word;
    private char lastLetter;
    public char LastLetter => lastLetter;
    public bool isWordleSolved { get; private set; } = false;

    [Header("Tiles")]
    public Tile.State emptyState;
    public Tile.State occupidedState;
    public Tile.State correctState;
    public Tile.State wrongSpotState;
    public Tile.State incorrectState;
    public Tile.State lockedState;

    [Header("UI")]
    public GameObject tryAgainButton;
    public GameObject newWordButton;
    public GameObject invalidWordText;

    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();
    }
    
    private void Start()
    {
        LoadData();
        NewGame();
    }

    private void LoadData()
    {
        TextAsset textFile = Resources.Load("official_wordle_common") as TextAsset;
        solutions = textFile.text.Split(SEPARATOR, System.StringSplitOptions.None);

        textFile = Resources.Load("official_wordle_all") as TextAsset;
        validWords = textFile.text.Split(SEPARATOR, System.StringSplitOptions.None);
    }

    public void NewGame()
    {
        ClearBoard();
        SetRandomWord();

        enabled = true;
    }

    public void TryAgain()
    {
        ClearBoard();

        enabled = true;
    }

    private void SetRandomWord()
    {
        word = solutions[Random.Range(0, solutions.Length)];
        word = word.ToLower().Trim();
        lastLetter = word.Last();
    }

    private void Update()
    {
        if (enabled)
        {
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
        }
    }

    private void SubmitRow(Row row)
    {
        if (!IsValidWord(row.word))
        {
            invalidWordText.SetActive(true);
            return;
        }

        string remaining = word;

        for (int i = 0; i < row.tiles.Length; i++)
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

        for (int i = 0; i < row.tiles.Length; i++)
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

        if (HasWon(row)) {
            enabled = false;
        }

        rowIndex++;
        columnIndex = 0;

        if (rowIndex >= rows.Length) {
            enabled = false;
        }
    }

    private bool IsValidWord(string word)
    {
        return validWords.Contains(word, System.StringComparer.OrdinalIgnoreCase);
    }

    private bool HasWon(Row row)
    {
        if (row.tiles.All(tile => tile.state == correctState))
        {
            isWordleSolved = true;
            return true;
        }
        return false;
    }

    private void ClearBoard()
    {
        foreach (Row row in rows)
        {
            foreach (Tile tile in row.tiles)
            {
                tile.SetLetter('\0');
                tile.SetState(emptyState);
            }
        }
        rowIndex = 0;
        columnIndex = 0;
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
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
using TMPro;

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
        InitialWin,
        BuildOff,
        // BuildBetween // The locked letter is not the start but in the middle of word
    }
    public event Helper.Event OnCompleted;

    [Header("Prefabs / References")]
    public Row rowPrefab;
    public Tile tilePrefab;
    public GlassAnimation glass;
    public ScoreBoard scoreBoard;

    [Header("UI")]
    public Pallette pallette;
    public GameObject tryAgainButton;
    public GameObject newWordButton;
    public GameObject invalidWordObject;
    public GameObject timerObject;
    private TMP_Text timerText;
    public GameOverImage gameOverImage;
    
    // Row info
    private Row[] rows;
    private int rowIndex;
    private int columnIndex;
    private int columnSkipIndex = -1;
    private bool checkWord = true;
    private float roundTime;
    private float roundTimer;
    private bool ignoreDestroyUpdate = false;

    // Row fading
    private bool waitForFade = false;
    private int rowsFading = 0;

    // Row submitting
    private bool submittingRow = false;
    private Row submittedRow;

    // Row continuations
    private ContinuationType continuationType = ContinuationType.None;
    private int continuationCount = 0;
    private Row continuationOffRow;
    private bool hideRetryButtons;

    // Valid words/solutions
    private string[] solutions;
    private string[] validWords;
    private string[] validScrabbleWords;
    
    public string word { get; private set; }
    private char lastLetter;
    public char LastLetter => lastLetter;

    private bool isScrabbleGame = false;
    private int score = 0;
    private bool bonusActive = false;

    public void Awake()
    {
        timerText = timerObject.GetComponent<TMP_Text>();
    }
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
    
    private ContinuationType GetContinuationType(Row row)
    {
        if (HasWonWordle(row)) {
            return ContinuationType.InitialWin;
        } else if (continuationCount >= 1) {
            return ContinuationType.BuildOff;
        }
        return ContinuationType.None;
    }

    private void Update()
    {
        // If waiting for fade, do not run rest of update until all rows are faded
        if (waitForFade)
        {
            if (rowsFading != 0)
            {
                return;
            }
            waitForFade = false;
        }
        
        if (!enabled || rows == null || rows.Length == 0) return;

        // If submitting row, wait until row is revealed
        if (submittingRow && submittedRow != null)
        {
            if (!submittedRow.IsRevealed) return;
            
            submittingRow = false;

            // Get what type of continuation should occur after this
            ContinuationType continueType = GetContinuationType(submittedRow);
            if (continueType != ContinuationType.None)
            {
                // Fade rows that aren't winning row out
                if (continueType == ContinuationType.InitialWin)
                {
                    AudioManager.instance.PlayWin();
                }
                for (int i = 0; i < rows.Length; i++)
                {
                    if (i != rowIndex)
                    {
                        DisappearRow(i);
                    }
                }
                waitForFade = true;
                isScrabbleGame = true;
                continuationOffRow = submittedRow;
                continuationType = continueType;
                roundTimer = Mathf.Max(Constants.ROUND_TIMER_INITIAL - continuationCount, Constants.ROUND_TIMER_MINIMUM);
                continuationCount++;
            }
            else
            {
                // No continuation, see if we have reached the final row. If not, continue to next rows
                rowIndex++;
                columnIndex = 0;
                columnSkipIndex = -1;
                if (rowIndex >= rows.Length) {
                    GameOver();
                }
                continuationCount = 0;
            }
            submittedRow = null;
            return;
        }

        // Not submitting, see if we are continuing
        if (continuationType != ContinuationType.None && continuationOffRow != null)
        {
            // Select which tile off of the last row should be the "locked" tile
            int tileOffRowIndex = continuationOffRow.tileOffIndex;
            while (tileOffRowIndex == continuationOffRow.tileOffIndex) {
                tileOffRowIndex = Random.Range(0, continuationOffRow.tiles.Length);
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
            tileOff.transform.SetParent(continueRow.transform, true);
            continueRow.AddTile(tileOff, tileOffIndex);
            for (int j = 0; j < continueLength; j++)
            {
                if (j != tileOffIndex)
                {
                    continueRow.AddTile(continueRow.CreateTile(j), j);
                }
            }
            rowIndex = rows.Length - 1;
            columnIndex = tileOffIndex == 0 ? 1 : 0;
            columnSkipIndex = tileOffIndex;
            continuationOffRow = null;
            checkWord = false;
            continuationType = ContinuationType.None;
            if (continuationCount > 0) {
                Row lastRow = rows[rowIndex - 1];
                for (int i = 0; i < lastRow.tiles.Length; i++)
                {
                    lastRow.tiles[i].SetState(Tile.State.ValidScrabbleWord);
                }
            }
        }

        // Determine round status based on time passed
        roundTime += Time.deltaTime;
        float roundAlpha = roundTime / roundTimer;
        if (continuationCount > 0) {
            if (roundAlpha >= 1) {
                // Took too long after continuation, game over
                GameOver();
                return;
            }
            else if (roundAlpha >= Constants.ROUND_TIMER_ALERT_ALPHA)
            {
                timerText.color = pallette.timerAlertTextColor;
            }
            else if (roundAlpha >= Constants.ROUND_TIMER_WARNING_ALPHA)
            {
                glass.StartWarning();
                timerText.color = pallette.timerWarningTextColor;
            }
            else
            {
                timerText.color = pallette.timerDefaultTextColor;
            }
            timerText.text = Helper.FormatTimer(roundTimer - roundTime);
            timerObject.SetActive(true);
        }

        // Listen to inputs for words in rows
        Row currentRow = rows[rowIndex];
        if (currentRow != null) {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                columnIndex = Mathf.Clamp(columnIndex - 1, 0, currentRow.tiles.Length);
                if (columnIndex == columnSkipIndex)
                {
                    columnIndex = columnSkipIndex + (columnSkipIndex == 0 ? 1 : -1);
                }
                currentRow.tiles[columnIndex].SetLetter('\0');
                currentRow.tiles[columnIndex].SetState(Tile.State.Empty);
                invalidWordObject.SetActive(false);
            }
            else if (columnIndex >= rows[rowIndex].tiles.Length)
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
                        currentRow.tiles[columnIndex].SetLetter((char) key);
                        currentRow.tiles[columnIndex].SetState(Tile.State.Occupied);
                        columnIndex++;
                        if (columnIndex == columnSkipIndex)
                        {
                            columnIndex = columnSkipIndex + 1;
                        }
                        break;
                    }
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
        name = word;
    }

    public void ClearBoard()
    {
        if (rows != null) {
            ignoreDestroyUpdate = true;
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] != null)
                {
                    rows[i].DestroyRow();
                }
            }
            ignoreDestroyUpdate = false;
            UpdateRows();
        }
        // Reset rows
        rows = new Row[0];
        rowIndex = 0;
        columnIndex = 0;
        columnSkipIndex = -1;
        checkWord = true;

        // Reset continuation
        continuationType = ContinuationType.None;
        continuationOffRow = null;
        continuationCount = 0;
        roundTime = 0f;
        roundTimer = Constants.ROUND_TIMER_INITIAL;
        timerObject.SetActive(false);
        glass.Show();
        scoreBoard.CloseSavePrompt();
    }

    public void GenerateRows(int numRows = 6)
    {
        ClearBoard();
        rows = new Row[numRows];
        for (int i = 0; i < numRows; i++)
        {
            Row row = CreateRow(5);
            AddRow(row);
            row.name = "Row " + i;
            row.AddTiles();
        }

        rowIndex = 0;
        columnIndex = 0;
        columnSkipIndex = -1;
    }

    private void SubmitRow(Row row)
    {
        roundTime = 0f;

        Tile.State[] submittedStates = new Tile.State[row.tiles.Length];

        if (isScrabbleGame)
        {
            string enteredScrabbleWord = row.word.ToLower();
            Debug.Log($"[Scrabble] Player typed: {enteredScrabbleWord}");

            if (!IsValidScrabbleWord(enteredScrabbleWord))
            {
                for (int i = 0; i < row.tiles.Length; i++)
                {
                    submittedStates[i] = Tile.State.InvalidScrabbleWord;
                }
                row.Reveal(submittedStates);
                GameOver();
                return;
            } else {
                for (int i = 0; i < row.tiles.Length; i++)
                {
                    submittedStates[i] = Tile.State.ValidScrabbleWord;
                }

                lastLetter = enteredScrabbleWord[enteredScrabbleWord.Length - 1];
            }
        } else {

            if (!IsValidWord(row.word))
            {
                invalidWordObject.SetActive(true);
                Debug.Log("[Wordle] Invalid typed word: " + row.word);
                return;
            }

            string solution = word;
            string remaining = word;

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

                    if (tile.letter == solution[i])
                    {
                        submittedStates[i] = Tile.State.Correct;
                        remaining = remaining.Remove(i, 1).Insert(i, " ");
                    }
                    else if (!solution.Contains(tile.letter))
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
                            remaining = remaining.Remove(index, 1).Insert(index, " ");
                        }
                        else
                        {
                            submittedStates[i] = Tile.State.Incorrect;
                        }
                    }
                }
            }
        }
        row.Reveal(submittedStates);
        AudioManager.instance.PlayGuess();
        timerText.text = Helper.FormatTimer(0);
        timerText.color = pallette.timerResettingTextColor;
        submittedRow = row;
        submittingRow = true;
        glass.Flip();
    }

    private bool HasWonWordle(Row row)
    {
        bool allCorrect = row.tiles.All(t => t.state == Tile.State.Correct);
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

    public void SetAsRegularGame()
    {
        isScrabbleGame = false;
    }

    private void GameOver()
    {
        AudioManager.instance.PlayLose();
        glass.Hide();
        gameOverImage.StartEffect();
        timerObject.SetActive(false);

        StartBonusScoring();

        string[,] finalScore = new string[1, 2] { { word, (continuationCount * 10).ToString() } };

        if (continuationCount > 0)
        {
            hideRetryButtons = true;
            scoreBoard.PromptScoreSave(finalScore);
            
            Helper.Event showRetry = null;
            showRetry = () => {
                hideRetryButtons = false;
                ToggleRetryButtons(true);
                scoreBoard.OnPromptFinished -= showRetry;
            };
            scoreBoard.OnPromptFinished += showRetry;
        }

        OnCompleted?.Invoke();
    }

    private void StartBonusScoring()
    {
        if (!bonusActive)
        {
            bonusActive = true;
            StartCoroutine(BonusScoreCoroutine());
        }
    }

    private IEnumerator BonusScoreCoroutine()
    {
        while (roundTime < roundTimer)
        {
            int correctLetters = 0;

            for (int i = 0; i < rows.Length; i++)
            {
                correctLetters += GetCorrectLetterCount(rows[i]);
            }

            score += correctLetters * 10;
            Debug.Log($"Bonus Score: {score} (Correct Letters: {correctLetters})");

            yield return new WaitForSeconds(1f);
        }

        bonusActive = false;
        Debug.Log("Bonus scoring ended.");
    }

    private int GetWordleAttempts()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] != null && HasWonWordle(rows[i]))
            {
                return i + 1;
            }
        }
        return rows.Length;
    }

    private int GetWordleScore(int attempts)
    {
        switch (attempts)
        {
            case 1: return 500;
            case 2: return 400;
            case 3: return 300;
            case 4: return 200;
            case 5: return 100;
            case 6: return 50;
            default: return 0;
        }
    }


    private string[,] GetFinalScoreMatrix()
    {
        int attempts = GetWordleAttempts();
        int wordleScore = GetWordleScore(attempts);
        int totalScore = wordleScore + score;

        string[,] scoreMatrix = new string[1, 2];
        scoreMatrix[0, 0] = word;
        scoreMatrix[0, 1] = totalScore.ToString();

        Debug.Log($"Final Score Matrix: [{scoreMatrix[0,0]}, {scoreMatrix[0,1]}]");

        return scoreMatrix;
    }


    private int GetCorrectLetterCount(Row row)
    {
        return row.tiles.Count(tile => tile.state == Tile.State.Correct);
    }


    private void ToggleRetryButtons(bool active)
    {
        tryAgainButton.SetActive(active);
        newWordButton.SetActive(active); 
    }

    private void OnEnable() => ToggleRetryButtons(false);

    private void OnDisable()
    {
        if (!hideRetryButtons) {
            ToggleRetryButtons(true);
        }
    }
}
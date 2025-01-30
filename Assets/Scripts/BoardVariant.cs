using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ScrabbleBoard : MonoBehaviour
{
    public Board board;
    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[] {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
        KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z, 
    };

    private static readonly string[] SEPARATOR = new string[] { "\r\n", "\r", "\n" };

    private RowVariant[] vrows;
    private int vrowIndex;
    private int vcolumnIndex;
    
    private string[] validScrabbleWords;
    private string sword;
    private char finalLetter;
    private bool wordleSolved;

    [Header("Vtiles")]
    public TileVariant.State emptyState;
    public TileVariant.State occupidedState;
    public TileVariant.State correctState;
    public TileVariant.State wrongSpotState;
    public TileVariant.State incorrectState;
    public TileVariant.State validScrabbleWordState;

    [Header("UI")]
    public GameObject tryAgainButton;
    public GameObject newWordButton;
    public GameObject invalidWordText;

    private void Awake()
    {
        vrows = GetComponentsInChildren<RowVariant>();
    }
    
    private void Start()
    {
        LoadData();
        NewGame();
    }

    private void LoadData()
    {
        TextAsset textFile = Resources.Load("dictionary") as TextAsset;
        validScrabbleWords = textFile.text.Split(SEPARATOR, System.StringSplitOptions.None)
            .Where(sword => sword.Length == 6)
            .ToArray();
    }

    public void NewGame()
    {
        ClearBoard();
        wordleSolved = false;
        enabled = false;
    }

    public void TryAgain()
    {
        ClearBoard();

        enabled = true;
    }


    private void Update()
    {
        if (!wordleSolved) return;
        
        RowVariant currentRow = vrows[vrowIndex];

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            vcolumnIndex = Mathf.Max(vcolumnIndex - 1, 0);
            currentRow.Vtiles[vcolumnIndex].SetLetter('\0');
            currentRow.Vtiles[vcolumnIndex].SetState(emptyState);
            invalidWordText.SetActive(false);
        }
        else if (vcolumnIndex >= vrows[vrowIndex].Vtiles.Length)
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
                    currentRow.Vtiles[vcolumnIndex].SetLetter((char)SUPPORTED_KEYS[i]);
                    currentRow.Vtiles[vcolumnIndex].SetState(occupidedState);
                    vcolumnIndex++;
                    break;
                }
            }
        }
    }

    private void SubmitRow(RowVariant vrow)
    {
        string enteredSWord = new string(vrow.Vtiles.Select(vtile => vtile.letter).ToArray()).ToLower();
        if (!validScrabbleWords.Contains(enteredSWord) || enteredSWord[0] != board.LastLetter)
        {
            invalidWordText.SetActive(true);
            return;
        }
        
        foreach (var vtile in vrow.Vtiles)
        {
            vtile.SetState(validScrabbleWordState);
        }

        finalLetter = enteredSWord.Last();
        board.SetLastLetter(finalLetter);
    }

    private bool IsValidWord(string sword)
    {
        for (int i = 0; i < validScrabbleWords.Length; i++)
        {
            if (string.Equals(sword, validScrabbleWords[i], System.StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }

    private void ClearBoard()
    {
        for (int vrow = 0; vrow < vrows.Length; vrow++)
        {
            for (int col = 0; col < vrows[vrow].Vtiles.Length; col++)
            {
                vrows[vrow].Vtiles[col].SetLetter('\0');
                vrows[vrow].Vtiles[col].SetState(emptyState);
            }
        }

        vrowIndex = 0;
        vcolumnIndex = 0;
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

using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public delegate void BoardVariantCompleted();

[DefaultExecutionOrder(-1)]
public class BoardVariantOG : MonoBehaviour
{
    private static readonly KeyCode[] SUPPORTED_KEYS_VARIANT = new KeyCode[] {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
        KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z, 
    };

    private static readonly string[] SEPARATOR_VARIANT = new string[] { "\r\n", "\r", "\n" };

    public event BoardVariantCompleted OnVariantCompleted;
    private bool checkWordVariant = true;
    private RowVariantOG[] rowsVariant;
    public Component rowPrefabVariant;
    public Component tileColumnPrefabVariant;
    public Component tilePrefabVariant;
    public bool isVariantSolved { get; private set; } = false;

    private int rowIndexVariant;
    private int columnIndexVariant;
    private int columnLockIndexVariant = -1;
    private float continuationDelayVariant;
    private float continuationTimePassedVariant = 0.0f;
    private RowVariantOG continuationRowVariant;
    
    private string[] solutionsVariant;
    private string[] validWordsVariant;
    public string wordVariant { get; private set; }
    private char lastLetterVariant;
    public char LastLetterVariant => lastLetterVariant;

    [Header("Tiles")]
    public TileVariantOG.StateVariant emptyStateVariant;
    public TileVariantOG.StateVariant occupiedStateVariant;
    public TileVariantOG.StateVariant correctStateVariant;
    public TileVariantOG.StateVariant wrongSpotStateVariant;
    public TileVariantOG.StateVariant incorrectStateVariant;
    public TileVariantOG.StateVariant lockedStateVariant;

    [Header("UI")]
    public GameObject tryAgainButtonVariant;
    public GameObject newWordButtonVariant;
    public GameObject invalidWordTextVariant;

    private void Awake()
    {
        rowsVariant = GetComponentsInChildren<RowVariantOG>();
    }

    public void LoadDataVariant()
    {
        TextAsset textFile = Resources.Load("official_wordle_variant_common") as TextAsset;
        solutionsVariant = textFile.text.Split(SEPARATOR_VARIANT, System.StringSplitOptions.None);

        textFile = Resources.Load("official_wordle_variant_all") as TextAsset;
        validWordsVariant = textFile.text.Split(SEPARATOR_VARIANT, System.StringSplitOptions.None);
    }

    public void GenerateRowsVariant() {
        ClearBoardVariant();
        rowsVariant = new RowVariantOG[6];
        for (int i = 0; i < 6; i++) {
            Component rowComponent = Instantiate(rowPrefabVariant, transform);
            rowComponent.name = "Variant" + i.ToString();
            RowVariantOG row = rowComponent.GetComponent<RowVariantOG>();
            rowsVariant[i] = row;
        }
    }

    public void ClearBoardVariant()
    {
        if (rowsVariant != null) {
            for (int i = 0; i < rowsVariant.Length; i++) {
                if (rowsVariant[i] != null) {
                    Destroy(rowsVariant[i].gameObject);
                }
            }
        }
        rowIndexVariant = 0;
        columnIndexVariant = 0;
        columnLockIndexVariant = -1;
        continuationRowVariant = null;
        checkWordVariant = true;
    }

    public void SetRandomWordVariant()
    {
        if (solutionsVariant == null) {
            LoadDataVariant();
        }
        wordVariant = solutionsVariant[Random.Range(0, solutionsVariant.Length)];
        wordVariant = wordVariant.ToLower().Trim();
        lastLetterVariant = wordVariant.Last();
    }

    private void Update()
    {
        if (enabled && rowIndexVariant < rowsVariant.Length) {
            RowVariantOG currentRowVariant = rowsVariant[rowIndexVariant];

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (columnIndexVariant > 0) {
                    columnIndexVariant = Mathf.Max(columnIndexVariant - 1, columnLockIndexVariant + 1);
                    currentRowVariant.tilesVariant[columnIndexVariant].SetLetterVariant('\0');
                    currentRowVariant.tilesVariant[columnIndexVariant].SetStateVariant(emptyStateVariant);
                    invalidWordTextVariant.SetActive(false);
                }
            }
            else if (columnIndexVariant >= currentRowVariant.tilesVariant.Length)
            {
                if (Input.GetKeyDown(KeyCode.Return)) {
                    SubmitRowVariant(currentRowVariant);
                }
            }
            else
            {
                for (int i = 0; i < SUPPORTED_KEYS_VARIANT.Length; i++)
                {
                    if (Input.GetKeyDown(SUPPORTED_KEYS_VARIANT[i]))
                    {
                        currentRowVariant.tilesVariant[columnIndexVariant].SetLetterVariant((char)SUPPORTED_KEYS_VARIANT[i]);
                        currentRowVariant.tilesVariant[columnIndexVariant].SetStateVariant(occupiedStateVariant);
                        columnIndexVariant++;
                        break;
                    }
                }
            }
        }
    }

    private void SubmitRowVariant(RowVariantOG row)
    {
        if (!IsValidWordVariant(row.wordVariant))
        {
            invalidWordTextVariant.SetActive(true);
            return;
        }

        if (HasWonVariant(row)) {
            isVariantSolved = true;
            OnVariantCompleted?.Invoke();
        } else {
            rowIndexVariant++;
            columnIndexVariant = 0;
            columnLockIndexVariant = -1;
        }
    }

    private bool IsValidWordVariant(string word)
    {
        return validWordsVariant.Contains(word, System.StringComparer.OrdinalIgnoreCase);
    }

    private bool HasWonVariant(RowVariantOG row)
    {
        return row.tilesVariant.All(tile => tile.stateVariant == correctStateVariant);
    }

    public void SetLastLetterVariant(char letter)
    {
        lastLetterVariant = letter;
    }
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [System.Serializable]
    public enum ScreenType {
        Title,
        Game,
        Credits,
        VersionNotes,
        NewWordle
    }

    public Canvas titleScreen;
    public Canvas gameScreen;
    public Canvas creditsScreen;
    public Canvas versionNotesScreen;
    public Canvas newWordleScreen;
    public Component homeButton;
    public TMP_Text startButtonText;
    public Button newWordleButton;
    public Board initialBoard;
    private Board currentBoard;
    private bool gameActive = false;
    private bool initialStarted = false;

    public void Start() {
        ToTitleScreen();
    }

    private void BeginGame(Board board, bool maintainPreviousWord = false) {
        if (!gameActive && currentBoard == null) {
            gameActive = true;
            startButtonText.text = "Continue";
            currentBoard = board;
            board.GenerateRows();
            if (board.word == null || !maintainPreviousWord) {
                board.SetRandomWord();
            }
            board.enabled = true;
            board.OnCompleted += StopGame;
        }
    }

    public void StartGame(Board board) {
        BeginGame(board);
    }

    public void TryGameAgain(Board board) {
        BeginGame(board, true);
    }

    public void StopGame() {
        if (gameActive && currentBoard != null) {
            currentBoard.enabled = false;
            currentBoard.OnCompleted -= StopGame;
            currentBoard = null;
            gameActive = false;
        }
    }

    public void OpenOriginalVersionScene() {
        SceneManager.LoadSceneAsync(Constants.ORIGINAL_SCENE_PATH);
    }

    public void OpenCurrentVersionScene() {
        SceneManager.LoadSceneAsync(Constants.CURRENT_SCENE_PATH);
    }
    
    public void OpenNewWordleScene() {
        SceneManager.LoadSceneAsync(Constants.NEW_WORDLE_SCENE_PATH);
    }

    public void ToTitleScreen() {
        SwitchScreens(ScreenType.Title);
    }

    public void ToGameScreen() {
        if (!initialStarted) {
            initialStarted = true;
            StartGame(initialBoard);
        }
        SwitchScreens(ScreenType.Game);
    }

    public void ToCreditsScreen() {
        SwitchScreens(ScreenType.Credits);
    }

    public void ToVersionNotesScreen() {
        SwitchScreens(ScreenType.VersionNotes);
    }
    
    public void ToNewWordleScreen() {
        SwitchScreens(ScreenType.NewWordle);
    }

    private void SwitchScreens(ScreenType type) {
        titleScreen.gameObject.SetActive(type == ScreenType.Title);
        gameScreen.gameObject.SetActive(type == ScreenType.Game);
        creditsScreen.gameObject.SetActive(type == ScreenType.Credits);
        versionNotesScreen.gameObject.SetActive(type == ScreenType.VersionNotes);
        newWordleScreen.gameObject.SetActive(type == ScreenType.NewWordle);
        homeButton.gameObject.SetActive(type != ScreenType.Title);
    }
}
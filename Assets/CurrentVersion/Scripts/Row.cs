using System;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class Row : MonoBehaviour
{
    [System.Serializable]
    public enum Direction
    {
        Horizontal,
        Vertical
    }

    public event Helper.Event OnDestroyed;
    public event Helper.Event OnDisappeared;
    private float targetHeight;
    private RectTransform rectTransform;
    private float revealTimePassed = 0.0f;
    private Tile.State[] revealStates;
    private bool hasRevealed;
    public bool IsRevealed { get => hasRevealed; }
    private bool destroyed;
    public bool HasDestroyed { get => destroyed; }
    private bool isFading;
    private float fadeDelay = 0;
    private float fadeTimePassed = 0.0f;
    public int length;
    public Direction direction;
    public Tile tileOff;
    public int tileOffIndex = -1;
    public Tile[] tiles { get; private set; }
    public Tile tilePrefab;
    
    public string word
    {
        get
        {
            string word = "";

            for (int i = 0; i < tiles.Length; i++) {
                word += tiles[i].letter;
            }

            return word;
        }
    }

    public Tile CreateTile(int order)
    {
        Tile tile = Instantiate(tilePrefab, transform);
        tile.name = order.ToString();
        return tile;
    }
    public void AddTile(Tile tile, int order)
    {
        if (order > length) {
            throw new IndexOutOfRangeException("Order is out of row length");
        }
        if (tiles == null) {        
            tiles = new Tile[length];
        }
        RectTransform tileTransform = tile.GetComponent<RectTransform>();
        float offset = order * (tileTransform.sizeDelta.x + Constants.TILE_PADDING);
        tileTransform.anchoredPosition = new Vector3(
            direction == Direction.Horizontal ? offset : 0,
            direction == Direction.Vertical ? -offset : 0,
            0
        );
        tile.row = this;
        tiles[order] = tile;
    }
    public void RemoveTile(int removeIndex)
    {
        Tile[] newTiles = new Tile[tiles.Length - 1];
        int tileIndex = 0;
        for (int i = 0; i < tiles.Length; i++) {
            if (i != removeIndex) {
                newTiles[tileIndex] = tiles[i];
                tileIndex++;
            }
        }
        tiles = newTiles;
    }

    public void AddTiles() {
        for (int j = 0; j < length; j++) {
            AddTile(CreateTile(j), j);
        }
    }

    private void Update() {
        if (!hasRevealed && revealStates != null) {
            revealTimePassed += Time.deltaTime;
            float submittingAlpha = revealTimePassed / Constants.SUBMIT_ROW_TIME;
            if (submittingAlpha >= 1) {
                hasRevealed = true;
                for (int i = 0; i < revealStates.Length; i++) {
                    Tile tile = tiles[i];
                    if (tile != tileOff) {
                        tiles[i].SetSwapAlpha(1);
                        tiles[i].SetState(revealStates[i]);
                    }
                }
            } else {
                float tileSubmitTime = Constants.SUBMIT_ROW_TIME / revealStates.Length;
                for (int i = 0; i < revealStates.Length; i++) {
                    Tile tile = tiles[i];
                    if (tile != tileOff) {
                        if (submittingAlpha >= i * tileSubmitTime) {
                            float swapAlpha = (submittingAlpha - (i * tileSubmitTime)) / tileSubmitTime;
                            tile.SetSwapAlpha(swapAlpha);
                            if (swapAlpha >= 0.5) {
                                tile.SetState(revealStates[i]);
                            }
                        }
                    }
                }
            }
        } else if (isFading) {
            fadeTimePassed += Time.deltaTime;
            if (fadeTimePassed > fadeDelay) {
                float alpha = DisappearEasingFunction(1 - ((fadeTimePassed - fadeDelay) / Constants.ROW_FADE_TIME));
                for (int i = 0; i < tiles.Length; i++) {
                    if (tiles[i] != null) {
                        tiles[i].SetAlpha(alpha);
                    }
                }
                if (alpha < 0) {
                    OnDisappeared?.Invoke();
                    DestroyRow();
                }
            }
        }
    }

    public void DestroyRow()
    {
        isFading = false;
        Destroy(gameObject);
        destroyed = true;
        OnDestroyed?.Invoke();
    }

    private float DisappearEasingFunction(float alpha) => Helper.CubicEase(alpha);

    public void Disappear(int delayOrder)
    {
        fadeDelay = delayOrder * Constants.ROW_FADE_DELAY_FACTOR;
        isFading = true;
    }

    public void Reveal(Tile.State[] revealStates)
    {
        this.revealStates = revealStates;
    }

    public void SetOrder(int order)
    {
        if (rectTransform == null) {
            rectTransform = GetComponent<RectTransform>();
        }
        if (tileOff != null) {
            RectTransform tileOffTransform = tileOff.GetComponent<RectTransform>();
            RectTransform tileOffRowTransform = tileOff.row.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector3(
                direction == Direction.Horizontal ? order * tileOffRowTransform.anchoredPosition.x :
                    tileOffTransform.anchoredPosition.x,
                    //tileOff.row != this ? tileOffTransform.anchoredPosition.x : rectTransform.anchoredPosition.x,
                direction == Direction.Vertical ? order * tileOffRowTransform.anchoredPosition.y : 
                    tileOffTransform.anchoredPosition.y,
                    //tileOff.row != this ? tileOffTransform.anchoredPosition.y : rectTransform.anchoredPosition.y,
                0
            );
        } else {
            targetHeight = order * -(rectTransform.sizeDelta.y + Constants.TILE_PADDING);
            rectTransform.anchoredPosition = new Vector3(
                0, Helper.Approach(rectTransform.anchoredPosition.y, targetHeight, Time.deltaTime * Constants.ROW_MOVE_SPEED), 0
            );
        }
    }
}
using UnityEngine;

public class Pallette : MonoBehaviour
{
    [Header("Tiles")]
    public Tile.StateColor emptyState = new Tile.StateColor() {
        fillColor = Helper.ColorFromHex("003838", 0),
        outlineColor = Helper.ColorFromHex("016B6B")
    };
    public Tile.StateColor occupidedState = new Tile.StateColor() {
        fillColor = Helper.ColorFromHex("003838", 0),
        outlineColor = Helper.ColorFromHex("138484")
    };
    public Tile.StateColor correctState = new Tile.StateColor() {
        fillColor = Helper.ColorFromHex("1E9B37"),
        outlineColor = Helper.ColorFromHex("1E9B37")
    };
    public Tile.StateColor wrongSpotState = new Tile.StateColor() {
        fillColor = Helper.ColorFromHex("AC780F"),
        outlineColor = Helper.ColorFromHex("AC780F")
    };
    public Tile.StateColor incorrectState = new Tile.StateColor() {
        fillColor = Helper.ColorFromHex("016B6B"),
        outlineColor = Helper.ColorFromHex("016B6B")
    };
    public Tile.StateColor lockedState = new Tile.StateColor() {
        fillColor = Helper.ColorFromHex("0092BF"),
        outlineColor = Helper.ColorFromHex("0092BF")
    };
    public Tile.StateColor validScrabbleState = new Tile.StateColor() {
        fillColor = Helper.ColorFromHex("1E9B37"),
        outlineColor = Helper.ColorFromHex("1E9B37")
    };
    public Tile.StateColor invalidScrabbleState = new Tile.StateColor() {
        fillColor = Helper.ColorFromHex("C23232"),
        outlineColor = Helper.ColorFromHex("C23232")
    };
    [Header("Misc Colors")]
    public Color timerDefaultTextColor = new Color(1, 1, 1);
    public Color timerWarningTextColor = Helper.ColorFromHex("FFF269");
    public Color timerAlertTextColor = Helper.ColorFromHex("FF1938");
    public Color timerResettingTextColor = Helper.ColorFromHex("55D999");
}
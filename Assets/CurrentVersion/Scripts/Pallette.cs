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
}
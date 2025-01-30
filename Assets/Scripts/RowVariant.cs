using UnityEngine;

public class RowVariant : MonoBehaviour
{
    public TileVariant[] Vtiles { get; private set; }
    
    public string sword
    {
        get
        {
            string sword = "";

            for (int i = 0; i < Vtiles.Length; i++) {
                sword += Vtiles[i].letter;
            }

            return sword;
        }
    }

    private void Awake()
    {
        Vtiles = GetComponentsInChildren<TileVariant>();
    }
}
using System;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    public int OwnerId { get; private set; }
    public AxialCoord Coord { get; private set; }
    public event Action<int, Color> OnOwnerChanged;

    public void SetOwner(int newOwnerId, Color newColor)
    {
        if (newOwnerId == OwnerId) return;

        Debug.Log($"Changing owner of {gameObject.name} to ID: {newOwnerId} with Color: {newColor}");
        OwnerId = newOwnerId;
        OnOwnerChanged?.Invoke(newOwnerId, newColor);
    }
    public void SetCoord(AxialCoord axialCoord) => Coord = axialCoord;
}

using DG.Tweening;
using System;
using UnityEngine;

public class HexViewAnimator : MonoBehaviour
{
    [SerializeField] private float _jumpPower;

    public void Stretch(HexView hexView)
    {
        Debug.Log("Stretched");
    }

    public void Jump(HexView hexView)
    {
        throw new NotImplementedException();
    }
}

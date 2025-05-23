using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class HexViewAnimator : MonoBehaviour
{
    private Transform _hexViewTransform;

    private Vector3 _position;
    private float offset = 1f;

    public void Init(Transform hexViewTransform)
    {
        _hexViewTransform = hexViewTransform;
        _position = _hexViewTransform.transform.position;

        _position.y += offset;
    }

    public void Stretch()
    {

    }

    private IEnumerator JumpCorotuine()
    {
        yield return _hexViewTransform.DOMoveY(_position.y, 0.5f);
        _hexViewTransform.DOMoveY(_hexViewTransform.position.y, 0.5f);
    }

    public void Jump()
    {
        //StartCoroutine(JumpCorotuine());
        Debug.Log("Jumped");
    }
}

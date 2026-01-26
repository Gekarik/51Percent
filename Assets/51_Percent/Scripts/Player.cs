using System;
using UnityEngine;

public class Player : CharacterAbstract
{
    private Camera _camera;
    public Camera Camera => _camera;

    private void Awake()
    {
        _camera = Camera.main;
        Camera.GetComponent<CameraFollower>().Init(transform);
        BaseInit();
    }
}
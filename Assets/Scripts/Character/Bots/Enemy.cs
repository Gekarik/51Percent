using System;
using UnityEngine;

public class Enemy : CharacterAbstract
{
    [SerializeField] private Sensor _sensor;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void Awake()
    {
        BaseInit();
    }
}

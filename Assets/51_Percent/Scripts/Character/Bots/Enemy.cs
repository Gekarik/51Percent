using System;
using UnityEngine;

public class Enemy : CharacterAbstract
{
    [SerializeField] private Sensor _sensor;
    
    private void Awake()
    {
        BaseInit();
    }
}

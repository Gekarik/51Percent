using System;
using System.Collections;

using UnityEngine;

[RequireComponent(typeof(Collider), typeof(ICharacter))]
public class Sensor : MonoBehaviour
{
    [SerializeField] private float _tickDelay;
    
    public event Action<Vector3> CollectableDetected;
    public event Action<Vector3> EnemyDetected;
    
    ICharacter _sensorOwner;

    private void Awake()
    {
        _sensorOwner = GetComponent<ICharacter>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<IGrabbable>() != null)
            CollectableDetected(other.transform.position);
        
        if(other.GetComponent<ICharacter>() != null)
            EnemyDetected(other.transform.position);
    }
    
    private IEnumerator Tick()
    {
        yield return new WaitForSeconds(1f);
    }
}

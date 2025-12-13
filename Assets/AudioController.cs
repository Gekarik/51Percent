using UnityEngine;

public class AudioController : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioClip[] _concreteStepSounds;
    [SerializeField] private AudioSource _footstepSource;
    
    [Header("PitchVolumeBounds")]
    [SerializeField] private float _pitchMin = 0.8f;
    [SerializeField] private float _pitchMax = 1.2f;
    [SerializeField] private float _volumeMin = 0.8f;
    [SerializeField] private float _volumeMax = 1.2f;

    private int _index;
    
    private void DoStep()
    {
        _index = Random.Range(0, _concreteStepSounds.Length);
        
        _footstepSource.pitch = Random.Range(_pitchMin, _pitchMax);
        _footstepSource.volume = Random.Range(_volumeMin, _volumeMax);

        _footstepSource.PlayOneShot(_concreteStepSounds[_index]);
    }
}

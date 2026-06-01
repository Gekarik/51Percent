using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(RagdollController))]
public class CharacterView : MonoBehaviour
{
    private CharacterBase _character;
    private Animator _animator;
    private Vector3 _initialLocalScale;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _initialLocalScale = transform.localScale;
    }

    private void Update()
    {
        if (_character == null) return;
        SetSpeed(_character.Speed);
    }

    public void Init(CharacterBase character)
    {
        _character = character;
    }

    public void SetModelScale(float factor)
    {
        transform.localScale = _initialLocalScale * factor;
    }

    private void SetSpeed(float speed)
    {
        _animator.SetFloat(AnimatorParams.Speed, speed);
    }
}

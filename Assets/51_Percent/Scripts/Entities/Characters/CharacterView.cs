using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(RagdollController))]
public class CharacterView : MonoBehaviour
{
    private CharacterBase _character;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
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

    private void SetSpeed(float speed)
    {
        _animator.SetFloat(AnimatorParams.Speed, speed);
    }
}

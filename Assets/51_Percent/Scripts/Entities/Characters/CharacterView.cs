using UnityEngine;

[RequireComponent(typeof(Animator))]
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

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
        if (_character == null)
            return;

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

    // Переключение режима походки через параметр — переходы и crossfade живут в графе контроллера
    public void SetLocomotionMode(LocomotionMode mode)
    {
        _animator.SetInteger(AnimatorParams.LocomotionMode, (int)mode);
    }

    // Единственная запись флага приземления: источник — доменное окно, владелец решения — CharacterBase
    public void SetLanding(bool isLanding)
    {
        _animator.SetBool(AnimatorParams.IsLanding, isLanding);
    }

    private void SetSpeed(float speed)
    {
        _animator.SetFloat(AnimatorParams.Speed, speed);
    }
}

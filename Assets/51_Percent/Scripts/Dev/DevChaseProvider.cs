using UnityEngine;

// Dev: примитивный источник направления — бежит по прямой к цели, без цели стоит
public class DevChaseProvider : VectorProviderComponent
{
    private Transform _target;

    public bool HasTarget => _target != null;

    public void SetTarget(Transform target) => _target = target;

    public void ClearTarget() => _target = null;

    public override Vector3 GetMoveDirection()
    {
        if (_target == null)
            return Vector3.zero;

        Vector3 direction = _target.position - transform.position;
        direction.y = 0f;
        return direction.normalized;
    }
}

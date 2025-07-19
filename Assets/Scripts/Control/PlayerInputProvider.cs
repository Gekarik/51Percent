using UnityEngine;

[RequireComponent(typeof(Mover))]
public class PlayerInputProvider : VectorProviderComponent
{
    private const string Horizontal = nameof(Horizontal);
    private const string Vertical = nameof(Vertical);

    public override Vector3 GetMoveDirection()
    {
        var MoveX = Input.GetAxis(Horizontal);
        var MoveZ = Input.GetAxis(Vertical);

        Vector3 dir = new Vector3(MoveX, 0, MoveZ);

        return dir.sqrMagnitude > 1f ? dir.normalized : dir;
    }
}

using UnityEngine;

public abstract class VectorProviderComponent : MonoBehaviour, IVectorProvider

{
    public abstract Vector3 GetMoveDirection();
}

using UnityEngine;

public struct TransformSnapshot
{
    public Vector3 LocalPosition;
    public Quaternion LocalRotation;
    public Vector3 LocalScale;
    public Transform Parent;
    public int SiblingIndex;

    public TransformSnapshot(Transform transform)
    {
        LocalPosition = transform.localPosition;
        LocalRotation = transform.localRotation;
        LocalScale = transform.localScale;
        Parent = transform.parent;
        SiblingIndex = transform.GetSiblingIndex();
    }

    public void Restore(Transform transform)
    {
        transform.SetParent(Parent);
        transform.SetSiblingIndex(SiblingIndex);
        transform.localPosition = LocalPosition;
        transform.localRotation = LocalRotation;
        transform.localScale = LocalScale;
    }
}

using UnityEngine;

// Переводит расстояние элемента до источника волны в задержку его старта
public class WaveDelayResolver
{
    private readonly WaveDirection _direction;

    public WaveDelayResolver(WaveDirection direction)
    {
        _direction = direction;
    }

    public float[] Resolve(Transform[] elements, Vector3 origin, float maxDelay)
    {
        var delays = new float[elements.Length];

        float minDistance = float.MaxValue;
        float maxDistance = float.MinValue;

        for (int i = 0; i < elements.Length; i++)
        {
            float distance = Vector3.Distance(elements[i].position, origin);
            delays[i] = distance;
            minDistance = Mathf.Min(minDistance, distance);
            maxDistance = Mathf.Max(maxDistance, distance);
        }

        float range = maxDistance - minDistance;

        for (int i = 0; i < elements.Length; i++)
        {
            float normalized = range > 0f ? (delays[i] - minDistance) / range : 0f;

            if (_direction == WaveDirection.ToOrigin)
                normalized = 1f - normalized;

            delays[i] = normalized * maxDelay;
        }

        return delays;
    }
}

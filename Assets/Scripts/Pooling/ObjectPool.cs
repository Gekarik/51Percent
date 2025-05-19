using System.Collections.Generic;
using System;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour, IGrabbable
{
    private readonly T _prefab;
    private readonly Transform _container;
    private readonly Queue<T> _pool = new Queue<T>();

    public ObjectPool(T prefab, Transform container)
    {
        _prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
        _container = container;
    }

    public T Get()
    {
        if (_pool.Count > 0)
        {
            var item = _pool.Dequeue();
            item.gameObject.SetActive(true);
            return item;
        }

        var instance = UnityEngine.Object.Instantiate(_prefab, _container);
        return instance;
    }

    public void Release(T item)
    {
        item.gameObject.SetActive(false);
        _pool.Enqueue(item);
    }
}
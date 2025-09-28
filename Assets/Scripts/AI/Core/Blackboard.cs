using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Core
{
    /// <summary>
    /// Система общей памяти для ИИ - хранит данные, которые могут использоваться
    /// разными компонентами (поведениями, сенсорами, etc.)
    /// </summary>
    [System.Serializable]
    public class Blackboard
    {
        [SerializeField] private Dictionary<string, object> _data = new Dictionary<string, object>();

        /// <summary>
        /// Получить значение по ключу с возможностью указать значение по умолчанию
        /// </summary>
        public T Get<T>(string key, T defaultValue = default(T))
        {
            if (_data.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)value;
                }
                catch (InvalidCastException)
                {
                    Debug.LogWarning($"[Blackboard] Cannot cast value for key '{key}' to type {typeof(T)}");
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Установить значение по ключу
        /// </summary>
        public void Set<T>(string key, T value)
        {
            if (key == null)
            {
                Debug.LogError("[Blackboard] Key cannot be null");
                return;
            }

            _data[key] = value;
        }

        /// <summary>
        /// Проверить, есть ли значение по ключу
        /// </summary>
        public bool HasKey(string key)
        {
            return _data.ContainsKey(key);
        }

        /// <summary>
        /// Удалить значение по ключу
        /// </summary>
        public bool Remove(string key)
        {
            return _data.Remove(key);
        }

        /// <summary>
        /// Очистить все данные
        /// </summary>
        public void Clear()
        {
            _data.Clear();
        }

        /// <summary>
        /// Получить все ключи (для отладки)
        /// </summary>
        public IEnumerable<string> GetAllKeys()
        {
            return _data.Keys;
        }

        /// <summary>
        /// Получить количество записей
        /// </summary>
        public int Count => _data.Count;

        /// <summary>
        /// Получить значение как float с безопасным приведением
        /// </summary>
        public float GetFloat(string key, float defaultValue = 0f)
        {
            var value = Get<object>(key);
            if (value == null) return defaultValue;

            try
            {
                return Convert.ToSingle(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Получить значение как bool с безопасным приведением
        /// </summary>
        public bool GetBool(string key, bool defaultValue = false)
        {
            var value = Get<object>(key);
            if (value == null) return defaultValue;

            try
            {
                return Convert.ToBoolean(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Увеличить числовое значение на указанную величину
        /// </summary>
        public void Increment(string key, float amount = 1f)
        {
            var current = GetFloat(key, 0f);
            Set(key, current + amount);
        }

        /// <summary>
        /// Установить значение с ограничением времени жизни
        /// </summary>
        public void SetWithTTL(string key, object value, float timeToLive)
        {
            Set(key, value);
            Set($"{key}_ttl", Time.time + timeToLive);
        }

        /// <summary>
        /// Проверить и удалить устаревшие значения с TTL
        /// </summary>
        public void CleanupExpiredValues()
        {
            var keysToRemove = new List<string>();
            var currentTime = Time.time;

            foreach (var key in _data.Keys)
            {
                if (key.EndsWith("_ttl"))
                {
                    var expirationTime = GetFloat(key, 0f);
                    if (currentTime > expirationTime)
                    {
                        var originalKey = key.Substring(0, key.Length - 4); // Remove "_ttl"
                        keysToRemove.Add(key);
                        keysToRemove.Add(originalKey);
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                Remove(key);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class ReferenceValidator
{
    // Ограничение вложенности [Serializable]-классов — защита от случайных циклов
    private const int MaxNestingDepth = 4;

    public readonly struct Violation
    {
        public readonly Component Component;
        // Путь в формате SerializedObject.FindProperty (например "_spawnPoint._area")
        public readonly string PropertyPath;

        public Violation(Component component, string propertyPath)
        {
            Component = component;
            PropertyPath = propertyPath;
        }
    }

    public static List<Violation> FindSceneViolations()
    {
        var violations = new List<Violation>();

        foreach (var component in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true))
        {
            if (component == null)
                continue;

            CollectViolations(component, component, string.Empty, violations, MaxNestingDepth);
        }

        return violations;
    }

    public static void LogViolations(List<Violation> violations)
    {
        foreach (var violation in violations)
        {
            string field = $"{violation.Component.GetType().Name}.{violation.PropertyPath}";
            Debug.LogWarning(
                $"<color=#ff5c5c>Проблемная ссылка</color>: <b>{field}</b> на объекте '{violation.Component.gameObject.name}'",
                violation.Component);
        }
    }

    private static void CollectViolations(Component owner, object target, string pathPrefix, List<Violation> violations, int depth)
    {
        if (depth == 0)
            return;

        foreach (var field in GetSerializedFields(target.GetType()))
        {
            string path = pathPrefix.Length == 0 ? field.Name : $"{pathPrefix}.{field.Name}";
            object value = field.GetValue(target);
            bool isRequired = field.IsDefined(typeof(RequiredAttribute), inherit: false);

            if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
            {
                CheckReference(owner, (UnityEngine.Object)value, path, isRequired, violations);
            }
            else if (TryGetObjectListElementType(field.FieldType, out _))
            {
                CheckList(owner, value as IList, path, isRequired, violations);
            }
            else if (value != null && IsNestedSerializableClass(field.FieldType, target))
            {
                // Вложенный [Serializable]-класс (например, SpawnPointProvider внутри спавнера)
                CollectViolations(owner, value, path, violations, depth - 1);
            }
        }
    }

    // Сломанная ссылка — проблема всегда, пустое поле — только если оно [Required]
    private static void CheckReference(Component owner, UnityEngine.Object value, string path, bool isRequired, List<Violation> violations)
    {
        if (IsMissing(value) || (value == null && isRequired))
            violations.Add(new Violation(owner, path));
    }

    private static void CheckList(Component owner, IList list, string path, bool isRequired, List<Violation> violations)
    {
        if (list == null || list.Count == 0)
        {
            if (isRequired)
                violations.Add(new Violation(owner, path));
            return;
        }

        for (int i = 0; i < list.Count; i++)
            CheckReference(owner, list[i] as UnityEngine.Object, $"{path}.Array.data[{i}]", isRequired, violations);
    }

    // Объект удалён, но сериализованная ссылка на него осталась:
    // обёртка не null по ReferenceEquals, но Unity-сравнение даёт null, а instanceID сохранился
    private static bool IsMissing(UnityEngine.Object value)
    {
        return value == null && !ReferenceEquals(value, null) && value.GetInstanceID() != 0;
    }

    // Сериализуемые поля: публичные или с [SerializeField], включая приватные в базовых классах
    private static IEnumerable<FieldInfo> GetSerializedFields(Type type)
    {
        while (type != null && type != typeof(MonoBehaviour) && type != typeof(object))
        {
            var fields = type.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (var field in fields)
                if (field.IsPublic || field.IsDefined(typeof(SerializeField), inherit: false))
                    yield return field;

            type = type.BaseType;
        }
    }

    private static bool TryGetObjectListElementType(Type type, out Type elementType)
    {
        elementType = null;

        if (type.IsArray)
            elementType = type.GetElementType();
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            elementType = type.GetGenericArguments()[0];

        return elementType != null && typeof(UnityEngine.Object).IsAssignableFrom(elementType);
    }

    private static bool IsNestedSerializableClass(Type fieldType, object target)
    {
        return fieldType.IsClass
               && fieldType != typeof(string)
               && !fieldType.IsArray
               && !fieldType.IsGenericType
               && fieldType.IsDefined(typeof(SerializableAttribute), inherit: false)
               && fieldType.Assembly == target.GetType().Assembly;
    }
}

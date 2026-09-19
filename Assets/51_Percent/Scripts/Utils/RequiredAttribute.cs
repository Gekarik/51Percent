using System;
using UnityEngine;

// Помечает ссылку, без которой компонент неработоспособен:
// пустое поле подсвечивается красным в инспекторе, а запуск сцены с ним блокируется
[AttributeUsage(AttributeTargets.Field)]
public class RequiredAttribute : PropertyAttribute
{
}

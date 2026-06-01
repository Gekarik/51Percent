# Socket / Attachment Point Pattern в Unity

Концепция сокетов (известная по Unreal Engine) в Unity не является встроенной фичей,
но легко реализуется через именованные дочерние трансформы в иерархии префаба.

---

## Суть паттерна

Персонаж имеет именованные точки крепления ("сокеты") — пустые GameObject'ы,
размещённые в нужных местах иерархии (над головой, в руке, на спине).
Внешний код не знает о геометрии персонажа — он просто просит сокет по имени
и крепит к нему нужный объект.

---

## Официальная документация Unity

- Transform.Find() — поиск дочернего трансформа по имени/пути:
  https://docs.unity3d.com/ScriptReference/Transform.Find.html

- Animator.GetBoneTransform() — получить трансформ кости Humanoid-рига:
  https://docs.unity3d.com/ScriptReference/Animator.GetBoneTransform.html

- HumanBodyBones — список всех костей гуманоида (Head, Spine, Hand...):
  https://docs.unity3d.com/ScriptReference/HumanBodyBones.html

- Transform.SetParent() — прикрепить объект к сокету в рантайме:
  https://docs.unity3d.com/ScriptReference/Transform.SetParent.html

- Configuring the Avatar (настройка Humanoid rig):
  https://docs.unity3d.com/Manual/ConfiguringtheAvatar.html

---

## Реализация в Unity (без Humanoid rig)

### 1. В префабе персонажа

Добавить пустой GameObject как дочерний к нужной кости или точке модели:

  Character (root)
  └── Model
      └── Head
          └── CrownSocket     ← пустой GO, позиционируется вручную

### 2. Интерфейс / компонент

```csharp
public enum SocketType { Head, LeftHand, RightHand }

public interface ICharacter
{
    Transform GetSocket(SocketType socket);
    // ...
}
```

```csharp
public class CharacterBase : MonoBehaviour, ICharacter
{
    [SerializeField] private Transform _headSocket;
    [SerializeField] private Transform _leftHandSocket;

    public Transform GetSocket(SocketType socket) => socket switch
    {
        SocketType.Head      => _headSocket,
        SocketType.LeftHand  => _leftHandSocket,
        _                    => throw new ArgumentOutOfRangeException()
    };
}
```

### 3. Прикрепление объекта к сокету

```csharp
// CrownController при смене лидера:
var socket = newLeader.GetSocket(SocketType.Head);
_crown.SetParent(socket, worldPositionStays: false);
_crown.localPosition = Vector3.zero;
_crown.localRotation = Quaternion.identity;
```

---

## Реализация через Humanoid rig (если rig есть)

```csharp
var headBone = animator.GetBoneTransform(HumanBodyBones.Head);
_crown.SetParent(headBone, worldPositionStays: false);
_crown.localPosition = Vector3.up * 0.1f; // небольшой offset над костью
```

---

## Почему не Transform.Find()?

Transform.Find() ищет по строке — хрупко (опечатка = null в рантайме),
медленнее сериализованной ссылки, и не даёт compile-time safety.
Предпочтительнее сериализовать ссылки напрямую через [SerializeField].

---

## Аналог в Unreal Engine

В Unreal сокеты — первоклассная фича: создаются в Skeleton Editor,
имеют имя, позицию и ротацию относительно кости.
Код обращается к ним по имени через GetSocketTransform("CrownSocket").
Unity не имеет встроенного эквивалента, но паттерн воспроизводится
через именованные дочерние трансформы + сериализованные ссылки.

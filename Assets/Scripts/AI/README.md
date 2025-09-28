# 🤖 Новая AI система для 51Percent

## 📋 Обзор

Это полностью новая архитектура ИИ для игры 51Percent, заменяющая старую систему `EnemyAIController`. Новая система основана на модульных поведениях, интеллектуальном анализе ситуации и гибких стратегиях.

## 🏗️ Архитектура

### Основные компоненты:

1. **AIBrain** - главный контроллер ИИ
2. **AIContext** - контекст с информацией о состоянии игры
3. **Blackboard** - система общей памяти
4. **IAIBehavior** - интерфейс для поведений
5. **Поведения** - конкретные действия (Idle, Explore, Attack, etc.)
6. **AIManager** - центральный менеджер всех ботов

### Схема взаимодействия:
```
AIManager -> NewAIEnemy -> AIBrain -> Behaviors
                    ↓
               AIContext + Blackboard
```

## 🚀 Быстрый старт

### 1. Создание нового AI бота

```csharp
// 1. Создайте GameObject с компонентами:
// - NewAIEnemy
// - AIBrain  
// - PathProvider
// - Mover, Conquester, etc.

// 2. Настройте в инспекторе:
NewAIEnemy enemy = GetComponent<NewAIEnemy>();
AIBrain brain = GetComponent<AIBrain>();

// 3. Установите личность бота
brain.Personality = BotPersonality.Aggressive; // или другую
```

### 2. Инициализация через AIManager

```csharp
// Добавьте AIManager в сцену
AIManager aiManager = FindObjectOfType<AIManager>();

// Боты автоматически найдутся и инициализируются
// Или зарегистрируйте бота вручную:
aiManager.RegisterAIBot(newAIEnemy);
```

### 3. Замена старых врагов

```csharp
// Старый код:
Enemy enemy = GetComponent<Enemy>();
enemy.InitAI(grid);

// Новый код:
NewAIEnemy newEnemy = GetComponent<NewAIEnemy>();
newEnemy.InitializeAI(grid, allCharacters);
```

## 🎭 Типы личности ботов

### BotPersonality.Aggressive
- **Поведение**: Активно атакует, мало времени на ожидание
- **Стратегия**: Ищет уязвимые следы врагов, рискует ради атак
- **Подходит для**: Создания давления на игрока

### BotPersonality.Defensive  
- **Поведение**: Фокус на защите территории, осторожные действия
- **Стратегия**: Держится близко к своей территории, избегает рисков
- **Подходит для**: Стабильного противника

### BotPersonality.Opportunist
- **Поведение**: Использует возможности, умеренный риск
- **Стратегия**: Атакует только при хороших шансах на успех
- **Подходит для**: Непредсказуемого противника

### BotPersonality.Territorial
- **Поведение**: Стремится расширить территорию
- **Стратегия**: Планомерное захватывание новых областей
- **Подходит для**: Постоянного роста угрозы

### BotPersonality.Balanced
- **Поведение**: Сбалансированный подход
- **Стратегия**: Адаптируется к ситуации
- **Подходит для**: Универсального противника

## 🛠️ Настройка поведений

### Создание нового поведения

```csharp
using AI.Core;
using AI.Behaviors;

public class MyCustomBehavior : BaseBehavior
{
    public override string Name => "MyCustom";
    public override Priority Priority => Priority.Medium;

    public override bool CanExecute(AIContext context)
    {
        // Логика проверки возможности выполнения
        return context.GetTerritoryPercentage() > 0.3f;
    }

    public override void OnEnter(AIContext context)
    {
        base.OnEnter(context);
        UpdateBehaviorStatus(context, "Starting custom behavior");
    }

    public override BehaviorResult Execute(AIContext context)
    {
        LastUpdateTime = Time.time;
        
        // Основная логика поведения
        if (SomeCondition())
        {
            return BehaviorResult.Success;
        }
        
        return BehaviorResult.Running;
    }
}
```

### Добавление поведения в AIBrain

```csharp
// В методе InitializeBehaviors() класса AIBrain:
_availableBehaviors.Add(new MyCustomBehavior(_pathProvider));
```

## 🔧 Отладка и мониторинг

### Включение отладки

```csharp
// В инспекторе AIBrain:
[SerializeField] private bool _enableDebugLogging = true;
[SerializeField] private bool _showDebugGUI = true;

// В инспекторе AIManager:
[SerializeField] private bool _enableGlobalDebug = true;

// В коде NewAIEnemy:
enemy.SetDebugVisualization(true);
```

### Отладочные команды в контекстном меню

**AIBrain:**
- Force Idle - принудительно переключить на ожидание
- Force Explore - принудительно начать исследование  
- Print Debug Info - вывести информацию о контексте

**AIManager:**
- Refresh Character Lists - обновить списки персонажей
- Reinitialize All Bots - переинициализировать всех ботов
- Print AI Statistics - показать статистику ИИ

**NewAIEnemy:**
- Initialize AI (Test) - тестовая инициализация
- Print AI Status - показать статус ИИ
- Toggle Debug Visualization - включить/выключить визуализацию

### Мониторинг в реальном времени

```csharp
// Получение статистики
var stats = aiManager.GetAIStatistics();
Debug.Log($"Active bots: {stats["TotalBots"]}");

// Проверка статуса конкретного бота
string status = aiBot.GetAIStatus();
Debug.Log($"Bot status: {status}");

// Получение контекста для детального анализа
AIContext context = aiBrain.Context;
string debugInfo = context.GetDebugInfo();
```

## ⚡ Производительность

### Настройки производительности

```csharp
// Интервал обновления мышления (AIBrain)
[SerializeField] private float _thinkInterval = 0.2f; // 5 FPS

// Интервал обновления менеджера (AIManager)  
[SerializeField] private float _updateInterval = 1f; // 1 FPS

// Оптимизация в AIContext
private const float ENEMY_UPDATE_INTERVAL = 0.5f; // Обновление врагов
```

### Рекомендации по оптимизации

1. **Think Interval**: 0.1-0.3s для быстрых реакций, 0.5s+ для спокойных ботов
2. **Update Interval**: 1-2s для обновления списков персонажей  
3. **Blackboard Cleanup**: автоматическая очистка устаревших данных
4. **Кэширование**: соседи в HexGrid, пути в Pathfinder

## 🔄 Миграция со старой системы

### Пошаговая замена

1. **Сохраните старые файлы** как backup
2. **Добавьте новые компоненты** к существующим врагам:
   ```csharp
   // Добавьте компоненты:
   NewAIEnemy, AIBrain, PathProvider (если нет)
   ```

3. **Обновите спавнеры**:
   ```csharp
   // Старый код:
   enemy.InitAI(grid);
   
   // Новый код:
   newEnemy.InitializeAI(grid, allCharacters);
   ```

4. **Добавьте AIManager** в сцену

5. **Протестируйте** новую систему

6. **Удалите старые компоненты**: EnemyAIController, TrailPlanner, etc.

### Совместимость

NewAIEnemy включает метод `InitAI(IHexGridProvider grid)` для совместимости со старым кодом.

## 📊 Преимущества новой системы

### ✅ Модульность
- Легко добавлять новые поведения
- Независимые компоненты
- Переиспользуемый код

### ✅ Интеллектуальность  
- Анализ ситуации через Blackboard
- Адаптивные стратегии
- Учет состояния игры

### ✅ Производительность
- Распределение вычислений по времени
- Кэширование данных
- Оптимизированные алгоритмы

### ✅ Отладка
- Подробная диагностика
- Визуализация состояний
- Мониторинг в реальном времени

### ✅ Расширяемость
- Простое добавление новых личностей
- Гибкая система приоритетов
- Событийная архитектура

## 🐛 Устранение неисправностей

### Частые проблемы

**"AI not initialized"**
- Проверьте наличие AIManager в сцене
- Убедитесь, что HexGrid найден
- Проверьте вызов InitializeAI()

**"No ICharacter component"**
- NewAIEnemy должен наследовать CharacterAbstract
- Убедитесь в правильности иерархии наследования

**"Pathfinding failed"**
- Проверьте настройки HexGrid
- Убедитесь в корректности соседних гексов
- Проверьте логику canEnter в Pathfinder

**"Behavior not switching"**
- Проверьте логику CanExecute() в поведениях
- Убедитесь в правильности приоритетов
- Проверьте состояние AIContext

### Логи для диагностики

```csharp
// Включите подробное логирование:
AISettings.EnableDebugLogging = true;

// Проверьте состояние:
Debug.Log(aiBrain.Context.GetDebugInfo());
Debug.Log(aiManager.GetAIStatistics());
```

## 📈 Планы развития

- **Сенсоры** для анализа окружения
- **Поведения атаки и защиты**  
- **Координация между ботами**
- **Система обучения**
- **Визуальный редактор поведений**

---

*Создано для улучшения AI системы в 51Percent игре. При возникновении вопросов обращайтесь к документации или комментариям в коде.*
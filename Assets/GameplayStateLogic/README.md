# Gameplay State Logic

## Устройство

Сущности не опрашиваются — они сами встают на учёт, пока участвуют в геймплее.

| Событие | Что происходит |
|---|---|
| `OnEnable` | сущность встаёт на учёт |
| `OnDisable` | снимается с учёта |
| `Complete()` | снимается с учёта, оставаясь на сцене |

Инвариант реестра жёсткий: **в нём лежат только активные**. Всё остальное следует из него —
запросу не нужны ни фильтрация, ни проверки на null, ни периодическая чистка мёртвых ссылок.

Ключевое здесь то, что «уничтожена» и «выключена» — не два разных случая. Unity вызывает
`OnDisable` и при деактивации объекта, и непосредственно перед `OnDestroy`, поэтому один колбэк
закрывает оба требования задания. Отдельной обработки удаления не существует, и это не упущение,
а следствие выбранной модели.

Третий случай — `completed` — намеренно отделён от активности `GameObject`. «Завершён» это
геймплейное состояние: сюжетный актёр может остаться включённым и видимым на сцене, но выбыть из
логики. Приравнивать его к `activeInHierarchy` значит потерять смысл.

## Использование

```csharp
public class Enemy : GameplayEntity { }

// все активные
IReadOnlyList<GameplayEntity> all = GameplayEntityRegistry.Active;

// только нужного типа, без аллокаций: буфер переиспользуется вызывающим
private readonly List<Enemy> _buffer = new List<Enemy>();

GameplayEntityRegistry.GetActive(_buffer);

// реакция вместо опроса
GameplayEntityRegistry.Unregistered += OnEntityLeft;
```

`Active` отдаётся как `IReadOnlyList`, чтобы снаружи нельзя было испортить реестр. Если потребитель
меняет состояние сущностей во время обхода — нужен `GetActive`: работа по копии, поэтому сущность
может спокойно выключиться или уничтожиться прямо в цикле. Иначе состав изменится под итератором и
обход упадёт с `InvalidOperationException`.

**Демо:** `Assets/GameplayStateLogicExample` — рантайм-панель на `OnGUI`. Сущности создаются на
лету, так что настройка сцены не нужна: повесить `GameplayEntityDemo` на пустой объект и нажать
Play. Панель показывает содержимое реестра рядом со списком всего созданного, поэтому разницу
между «активна», «выключена», «завершена» и «уничтожена» видно сразу.

## Масштабирование на крупный проект
Все зависит от проекта :)

___

#

5. Gameplay / State Logic (3D + Systems Thinking) This task
   focuses on gameplay logic, not UI.
   Context
   We have multiple gameplay entities in a scene, such as enemies, interactables, or story
   actors. Some of them become inactive due to gameplay events - destroyed, disabled,
   completed, etc.
   Task
   Design and implement a method or small system that:
   ●
   tracks gameplay entities;
   ●
   returns only active entities;
   ●
   cleanly handles entities being removed;
   ●
   cleanly handles entities being disabled;
   ●
   is safe and readable for production use.
   You may choose an OOP approach, event-driven approach, simple manager, or service.
   Explain your reasoning briefly.

# Popup System

## Устройство

Система разделяет две независимые вещи:

| Что | Кто задаёт |
|---|---|
| какой префаб | `PopupType` — аргумент `ShowAsync`, объявлен на самом префабе |
| что показать | данные, свой независимый тип на каждый вид попапа |

Одни и те же данные показываются двумя префабами с разной вёрсткой без единой строки нового
кода. Новый вид попапа добавляется, не открывая ни одного существующего файла.

Типы данных разных попапов не связаны наследованием — связь «данные ↔ вью» задаёт
`IPopupView<TData>`. Поэтому `Core/` не содержит ни `Title`, ни `Button`: требование «1–5 кнопок»
принадлежит `MessagePopupData`, а не системе попапов.

## Использование

```csharp
var data = new MessagePopupData()
    .Title("Выйти в меню?")
    .Body("Несохранённый прогресс будет потерян.")
    .Button("Отмена")
    .Button("Выйти", onClick: ReturnToMenu);

await _popups.ShowAsync(PopupType.ConfirmHorizontal, data);   // кнопки в ряд
await _popups.ShowAsync(PopupType.ConfirmVertical,   data);   // те же данные, столбиком
```

`await` завершается в момент выбора игрока. Показ мгновенный.
Результат: `.Index`, `.WasDismissed` или типизированно `.As<T>()`.
`PrewarmAsync` — оптимизация, не предусловие: непрогретый попап грузится лениво при показе.

**Демо:** `Assets/PopupSystemExample/Demo.unity` — там же префабы и каталог.
Визуально голо, дефолтные спрайты Unity.

## Компоненты префаба
Реализовал демо сцену, вместо описания комопнентов

## Масштабирование на крупный проект

Всё зависит от проекта. Что упрётся раньше всего:

* **Пересмотреть ключ попапа** — `enum` плохо переживает рост и мержи; ключ-ассет заводится
  дизайнером.
* **Загрузка и выгрузка редких попапов** — Addressables вместо каталога, прогрев и `Release` по
  главам. Шов уже асинхронный, сервис и вызывающий код не изменятся.
* **Стек → очередь с приоритетом и политикой показа** — дисконнект вытесняет всё, warning не
  перекрывает туториал. Меняется только сервис.
* **Разные руты и слои под разные попапы** — `World Space` рядом со `Screen Space`,
  целевой рут приходит из каталога, позиция попапа.
* Система попапов должна идти через ScreenManager в целом + интеграция в проект через регисрацию

В системе попапов, конечно, лучше понимать менее абстрактно, какая логика работы системы ожидается.

---
#

3. Popup / UI System (UI + Architecture) Our games
   use popups for:
   ●
   confirmations;
   ●
   story choices;
   ● warnings;
   ●
   tutorials.
   Task
   Design a simple popup system that supports:
   ●
   loading a popup;
   ●
   setting the title text and body text;
   ● displaying between 1-5 buttons;
   ● assigning callbacks to buttons.
   3.1 Unity Components Question
   Which Unity components would you use to build the popup prefab, and why?

# SaveSystem

Универсальный слой Save/Load для Unity. Базовая сборка не зависит от внешних пакетов.

## Интеграции

Newtonsoft, UniTask и VContainer подключаются отдельными asmdef. Их код включается одним соответствующим define (`SAVESYSTEM_NEWTONSOFT`, `SAVESYSTEM_UNITASK`, `SAVESYSTEM_VCONTAINER`).Define Manager добавляет или удаляет `SAVESYSTEM_*` в Player Settings для выбранной платформы. Пакет интеграции должен быть установлен: соответствующий asmdef содержит прямую ссылку на его assembly. Define включает или исключает код интеграции.

## Масштабирование и эксплуатация

### Масштабирование

`SaveService` остаётся фасадом, поэтому систему можно расширять без изменения игрового API:

- MemoryPack/бинарный сериализатор для больших payload;
- migration pipeline перед десериализацией;
- сжатие, шифрование и cloud save через декораторы `IPersistentStorage`;
- независимые сегменты (`SaveId`) вместо одного большого файла;
- editor validation: дубликаты `SaveType`, несовместимые версии и отсутствующие интеграции.

MemoryPack потребует адаптации моделей (`[MemoryPackable]`, `partial` либо отдельные DTO). Для development можно использовать Newtonsoft JSON, для release — бинарный формат. Это разные `FormatId`, поэтому прямое чтение одного формата другим не поддерживается.

Для отладки нужен editor-конвертер: он читает production-save через production serializer, десериализует модель и записывает отдельную JSON-копию через Newtonsoft. Исходный save при этом не изменяется.

### Работа дизайнеров

Editor-инструменты:

- просмотр debug-save и `LoadStatus`;
- настройка default-данных через `ScriptableObject`;
- выбор слота и сброс отдельного сегмента;
- создание тестовых состояний для QA.

### Профилирование и отладка

Для профилирования и диагностики можно добавить:

- `ProfilerMarker` для отображения этапов Save/Load в Unity Profiler;
- структурированные события с `SaveId`, размером файла и выбранным storage;
- замеры времени сериализации, чтения и записи;

---
#
2. Save / Load Utility (Production Basics) Many of
   our projects require persistent data:
   ● player progress;
   ●
   settings;
   ● VN state;
   ● gameplay flags.
   Task
   Implement a generic save/load utility that:
   ●
   saves any serializable class to file;
   ●
   loads it back safely;
   ● handles missing or invalid data gracefully.
   Notes
   ● You may use JSON serialization;
   ●
   focus on clean API and robustness;
   ● assume this utility will be reused across multiple projects.

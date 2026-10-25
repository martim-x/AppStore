---
name: drawio-uml-schemas
description: Use when drawing or regenerating UML/ER diagrams for this project in draw.io (class, sequence, use case, ERD) or when putting them into the report. Covers swimlane rows, edge anchoring, generic escaping and export.
---

# Схемы draw.io для проекта AppStore

Правила выведены на реальных ошибках: первая версия диаграмм классов и
последовательности получилась с наложением текста и линиями сквозь блоки.

## Порядок работы

1. Правь `.drawio` файл или генератор `scripts/diagrams/make_diagrams.py`.
2. Экспортируй PNG: `draw.io.exe --export --format png --scale 2.5 --border 20 --output out.png in.drawio`.
3. Уменьши копию до ~900 px и посмотри глазами: наложения строк, обрезка, линии сквозь блоки.
4. Вставь в `docs/report.json` (`figure.file`, `width_cm`) и пересобери отчёт.
5. Проверь, что подпись рисунка на той же странице, что и рисунок.

## Классы: заголовок и строки — разные ячейки

Нельзя писать многострочный текст в одну swimlane — строки наезжают на заголовок.

```xml
<mxCell id="c1" value="MainViewModel : ViewModelBase"
        style="shape=swimlane;html=1;startSize=30;fillColor=none;strokeColor=#000000;fontSize=13;align=left;"
        vertex="1" parent="1">
  <mxGeometry x="20" y="110" width="250" height="144" as="geometry"/>
</mxCell>
<mxCell id="c2" value="- IRepository _repo"
        style="text;html=1;align=left;verticalAlign=middle;spacingLeft=6;fontSize=13;strokeColor=none;fillColor=none;whiteSpace=nowrap;"
        vertex="1" parent="c1">
  <mxGeometry x="0" y="30" width="250" height="19" as="geometry"/>
</mxCell>
```

Высота блока `30 + 19 × строк`, строка `i` — `y = 30 + 19 × i`, высота `19`.

## Экранирование generic-типов

`html=1` съедает `<App>` как тег. В модели писать `&amp;lt;App&amp;gt;` — тогда
после разбора XML остаётся `&lt;App&gt;` и на картинке видно `List<App>`.
В Python-генераторе: двойное экранирование до вызова `html.escape`.

## Связи

- Точки крепления обязательны: `exitX/exitY` + `entryX/entryY`, иначе линия режет блоки.
- `edgeStyle=orthogonalEdgeStyle;rounded=0;`.
- Наследование: `endArrow=block;endFill=0;`.
- Реализация интерфейса: то же + `dashed=1`.
- Зависимость: `endArrow=open;dashed=1;`. Ассоциация: `endArrow=none;`.
- Не подписывай связи словами «наследует»/«implements» — тип виден по наконечнику, подписи наезжают.
- Соединяй только соседние блоки: через две колонки линия проходит сквозь промежуточные классы.

## Компоновка и масштаб

- Колонки 240–250, зазор 30–40, ширина композиции ≤ 1100 единиц.
- Размер текста в PDF ≈ `fontSize × 453 / ширина_композиции` pt; при 1100 ед и fontSize 13 это ≈ 5.4 pt — предел для A4.
- Для ERD с пропорцией 1.3 ставить `width_cm=11` (иначе высота 21 см и подпись уезжает на другую страницу), квадратные схемы — 14–16 см.

## Последовательность

- `shape=umlLifeline`, шаг сообщений 45–50 px.
- Вызов — сплошная стрелка, возврат — пунктир.
- Самовызов: `source(x,y) → (x+45,y) → (x+45,y+26) → target(x,y+26)`.

## Что не трогать

`use_case.drawio` и `erd.drawio` ведутся вручную и приняты. Генератор
`scripts/diagrams/make_diagrams.py` собирает только `class_diagram.drawio` и
`sequence_diagram.drawio`; не возвращай остальные в `BUILDERS` — ручные схемы
затрутся (уже случалось, восстанавливали из git).

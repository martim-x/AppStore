#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""Генератор UML-диаграмм AppStore в формате .drawio (mxGraphModel XML).

Требования (методичка кафедры ИТ + замечания руководителя 26.10.2026):
  * все чертежи чёрно-белые, связи ортогональные;
  * классы — прямоугольник с тремя секциями, видимость +/-; ассоциация без стрелки,
    обобщение — сплошная линия с незалитым треугольником, реализация интерфейса —
    штриховая линия с незалитым треугольником, зависимость — штриховая с открытой
    стрелкой;
  * варианты использования — актёры, граница системы, ассоциации, включение и
    расширение пунктиром со стрелкой;
  * последовательность — линии жизни, фокус управления, вызов сплошной стрелкой,
    возврат штриховой.

Ширина композиции ≤ 1100 единиц: при вставке 16–17 см это даёт кегль ≈8–9 pt
на печати (прежние 1540 ед. давали 5.3 pt).

Экспорт PNG: drawio --export --format png --scale 2.5 --border 20.
erd.drawio ведётся вручную и генератором не перезаписывается.
"""
import html
import os
import shutil
import subprocess
import sys

UML_DIR = os.environ.get(
    "UML_DIR",
    os.path.join(
        os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))),
        "docs", "UML",
    ),
)
DRAWIO = os.environ.get("DRAWIO", "drawio")

# Позиционные аргументы: [UML_DIR] [DRAWIO]
_positional = [a for a in sys.argv[1:] if not a.startswith("--")]
if len(_positional) > 0:
    UML_DIR = _positional[0]
if len(_positional) > 1:
    DRAWIO = _positional[1]

FONT = "fontFamily=Times New Roman;"
ST_ACTOR = (
    "shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;html=1;"
    "align=center;outlineConnect=0;fillColor=#ffffff;strokeColor=#000000;fontSize=20;" + FONT
)
ST_UC = (
    "ellipse;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#000000;"
    "fontSize=20;" + FONT
)
ST_BOUNDARY = (
    "rounded=0;whiteSpace=wrap;html=1;fillColor=none;strokeColor=#000000;"
    "verticalAlign=top;align=center;spacingTop=8;fontSize=20;fontStyle=1;" + FONT
)
ST_CLASS = (
    "swimlane;html=1;startSize=54;fillColor=#ffffff;strokeColor=#000000;"
    "align=center;verticalAlign=middle;whiteSpace=wrap;fontSize=21;" + FONT
)
ST_ROW = (
    "text;html=1;align=left;verticalAlign=middle;spacingLeft=5;strokeColor=none;"
    "fillColor=none;whiteSpace=nowrap;fontSize=18;" + FONT
)
ST_ASSOC = "endArrow=none;html=1;strokeWidth=1;rounded=0;" + FONT
ST_DEP = (
    "endArrow=open;dashed=1;html=1;strokeWidth=1;rounded=0;"
    "edgeStyle=orthogonalEdgeStyle;" + FONT
)
ST_GEN = (
    "endArrow=block;endFill=0;html=1;strokeWidth=1;rounded=0;"
    "edgeStyle=orthogonalEdgeStyle;" + FONT
)
ST_REAL = (
    "endArrow=block;endFill=0;dashed=1;html=1;strokeWidth=1;rounded=0;"
    "edgeStyle=orthogonalEdgeStyle;" + FONT
)
ST_INCLUDE = (
    "endArrow=open;dashed=1;html=1;strokeWidth=1;rounded=0;"
    "edgeStyle=orthogonalEdgeStyle;fontSize=16;labelBackgroundColor=#ffffff;" + FONT
)
ST_LIFE = (
    "shape=umlLifeline;html=1;whiteSpace=wrap;fillColor=#ffffff;"
    "strokeColor=#000000;size=54;fontSize=19;" + FONT
)
ST_CALL = (
    "endArrow=block;endFill=1;html=1;strokeWidth=1;rounded=0;"
    "edgeStyle=orthogonalEdgeStyle;verticalAlign=bottom;fontSize=17;" + FONT
)
ST_RET = (
    "endArrow=open;dashed=1;html=1;strokeWidth=1;rounded=0;"
    "edgeStyle=orthogonalEdgeStyle;verticalAlign=bottom;fontSize=17;" + FONT
)
ST_ACT = "html=1;fillColor=#ffffff;strokeColor=#000000;"


class Dia:
    """Накопитель mxCell."""

    def __init__(self, name, width=1100, height=900):
        self.name = name
        self.width = width
        self.height = height
        self.cells = []
        self.n = 0

    def _id(self, prefix="v"):
        self.n += 1
        return f"{prefix}{self.n}"

    def vertex(self, value, style, x, y, w, h, parent="1"):
        cid = self._id()
        val = html.escape(value, quote=True).replace("\n", "&#10;")
        self.cells.append(
            f'        <mxCell id="{cid}" value="{val}" style="{style}" vertex="1" '
            f'parent="{parent}">\n'
            f'          <mxGeometry x="{x}" y="{y}" width="{w}" height="{h}" as="geometry" />\n'
            f"        </mxCell>"
        )
        return cid

    def edge(self, source, target, style=ST_ASSOC, label="", exit_=None, entry=None):
        cid = self._id("e")
        st = style
        if exit_:
            st += f"exitX={exit_[0]};exitY={exit_[1]};exitDx=0;exitDy=0;"
        if entry:
            st += f"entryX={entry[0]};entryY={entry[1]};entryDx=0;entryDy=0;"
        val = html.escape(label, quote=True)
        lbl = f' value="{val}"' if val else ""
        self.cells.append(
            f'        <mxCell id="{cid}"{lbl} style="{st}" edge="1" parent="1" '
            f'source="{source}" target="{target}">\n'
            f'          <mxGeometry relative="1" as="geometry" />\n'
            f"        </mxCell>"
        )
        return cid

    def free_edge(self, value, style, x1, y1, x2, y2):
        cid = self._id("m")
        val = html.escape(value, quote=True).replace("\n", "&#10;")
        lbl = f' value="{val}"' if val else ""
        self.cells.append(
            f'        <mxCell id="{cid}"{lbl} style="{style}" edge="1" parent="1">\n'
            f'          <mxGeometry relative="1" as="geometry">\n'
            f'            <mxPoint x="{x1}" y="{y1}" as="sourcePoint" />\n'
            f'            <mxPoint x="{x2}" y="{y2}" as="targetPoint" />\n'
            f"          </mxGeometry>\n"
            f"        </mxCell>"
        )
        return cid

    def xml(self):
        body = "\n".join(self.cells)
        return (
            '<?xml version="1.0" encoding="UTF-8"?>\n'
            '<mxfile host="app.diagrams.net" agent="hermes" type="device">\n'
            f'  <diagram id="{self.name}" name="{self.name}">\n'
            f'    <mxGraphModel dx="1200" dy="900" grid="1" gridSize="10" guides="1" '
            f'tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" '
            f'pageWidth="{self.width}" pageHeight="{self.height}" math="0" shadow="0">\n'
            "      <root>\n"
            '        <mxCell id="0" />\n'
            '        <mxCell id="1" parent="0" />\n'
            f"{body}\n"
            "      </root>\n"
            "    </mxGraphModel>\n"
            "  </diagram>\n"
            "</mxfile>\n"
        )


def write(dia, filename):
    path = os.path.join(UML_DIR, filename)
    with open(path, "w", encoding="utf-8") as f:
        f.write(dia.xml())
    print(f"WROTE {path}")
    return path


def export_png(path, scale=2.5, border=20):
    out = path[: -len(".drawio")] + ".png"
    if shutil.which(DRAWIO) is None and not os.path.exists(DRAWIO):
        print(f"EXPORT SKIP {out}: draw.io CLI '{DRAWIO}' not found")
        return False
    cmd = [DRAWIO, "--export", "--format", "png", "--scale", str(scale),
           "--border", str(border), "--output", out, path]
    try:
        r = subprocess.run(cmd, capture_output=True, text=True, timeout=300)
    except OSError as exc:
        print(f"EXPORT FAIL {out}: {exc}")
        return False
    ok = os.path.exists(out) and os.path.getsize(out) > 5000
    print(f"EXPORT {'OK ' if ok else 'FAIL'} {out} "
          f"({os.path.getsize(out) if os.path.exists(out) else 0} bytes)")
    if not ok:
        print("  stdout:", r.stdout.strip()[:400])
        print("  stderr:", r.stderr.strip()[:400])
    return ok


# ══════════════════════════════════════════════════════════════════════════
# 1. ВАРИАНТЫ ИСПОЛЬЗОВАНИЯ
# ══════════════════════════════════════════════════════════════════════════
USE_CASES = [
    ("Войти в аккаунт", "client"),
    ("Просмотреть каталог", "client"),
    ("Найти приложение", "client"),
    ("Отфильтровать приложения", "client"),
    ("Отсортировать приложения", "client"),
    ("Просмотреть карточку приложения", "client"),
    ("Установить приложение", "client"),
    ("Удалить установленное приложение", "client"),
    ("Просмотреть профиль", "client"),
    ("Изменить профиль", "client"),
    ("Сменить пароль", "client"),
    ("Сменить язык интерфейса", "client"),
    ("Сменить тему оформления", "client"),
    ("Добавить приложение", "admin"),
    ("Редактировать приложение", "admin"),
    ("Удалить запись каталога", "admin"),
    ("Восстановить настройки по умолчанию", "admin"),
]
UC_EXTRA = ["Отменить действие", "Повторить действие"]
# (источник, приёмник, тип): include | extend — только между соседними прецедентами,
# иначе ортогональная линия идёт сквозь промежуточные эллипсы
UC_LINKS = [
    ("Установить приложение", "Просмотреть карточку приложения", "include"),
    ("Изменить профиль", "Просмотреть профиль", "include"),
    ("Установить приложение", "Отменить действие", "extend"),
    ("Удалить установленное приложение", "Повторить действие", "extend"),
]


def build_use_case():
    """Актёр слева, один столбец его прецедентов; расширения и админские
    прецеденты — во втором столбце, администратор справа. Такая раскладка
    исключает линии, проходящие сквозь блоки."""
    d = Dia("use-case", 800, 1150)
    bx, by, bw, bh = 110, 20, 520, 1100
    d.vertex("Приложение AppStore", ST_BOUNDARY, bx, by, bw, bh)
    client = d.vertex("Клиент", ST_ACTOR, 40, 500, 50, 90)
    admin = d.vertex("Администратор", ST_ACTOR, 680, 400, 50, 90)

    coords = {}
    col_w, row_h, gap_y = 230, 58, 18
    # столбец 1 — прецеденты клиента
    for i, (name, actor) in enumerate(USE_CASES):
        if actor != "client":
            continue
        coords[name] = d.vertex(name, ST_UC, 140, 80 + i * (row_h + gap_y), col_w, row_h)
    # столбец 2 — расширения (сверху) и прецеденты администратора
    for j, name in enumerate(UC_EXTRA):
        coords[name] = d.vertex(name, ST_UC, 400, 80 + j * (row_h + gap_y), col_w, row_h)
    y = 300
    for name, actor in USE_CASES:
        if actor != "admin":
            continue
        coords[name] = d.vertex(name, ST_UC, 400, y, col_w, row_h)
        y += row_h + gap_y

    for name, actor in USE_CASES:
        d.edge(client if actor == "client" else admin, coords[name], ST_ASSOC,
               exit_=(1, 0.5), entry=(0, 0.5))
    for src, tgt, kind in UC_LINKS:
        label = "«include»" if kind == "include" else "«extend»"
        d.edge(coords[src], coords[tgt], ST_INCLUDE, label)
    return d


# ══════════════════════════════════════════════════════════════════════════
# 2. КЛАССЫ
# ══════════════════════════════════════════════════════════════════════════
# (имя, стереотип, строки)
CLASSES = {
    "MainWindow": ("«View»", ["+ MainViewModel"]),
    "LoginWindow": ("«View»", ["+ ShowDialog() : bool"]),
    "AddEditWindow": ("«View»", ["+ AddEditAppViewModel"]),
    "ProfilePopup": ("«View»", ["+ ProfileViewModel"]),
    "App": ("", ["+ Guid Id", "+ string FullName", "+ double Price", "+ List<string> Tags"]),
    "User": ("", ["+ Guid Id", "+ string Login", "+ UserRole Role"]),
    "UsersApps": ("", ["+ Guid UserId", "+ Guid AppId", "+ DateTime InstalledAt"]),

    "ViewModelBase": ("«abstract»", ["# OnPropertyChanged(name)", "# bool SetField<T>()"]),
    "MainViewModel": ("", ["- IRepository _repo", "- IAuthService _auth",
                          "- IUndoRedoService _undo", "+ ObservableCollection Apps",
                          "+ void Reload()"]),
    "ProfileViewModel": ("", ["- IAuthService _auth", "- IThemeService _theme",
                              "+ ICommand SaveProfileCmd", "- Task CheckDatabase()"]),
    "AddEditAppViewModel": ("", ["- IRepository _repo", "+ App Draft",
                                 "+ ICommand SaveCommand"]),
    "RelayCommand": ("«ICommand»", ["- Action _execute", "+ void Execute(obj)"]),
    "AsyncRelayCommand": ("«ICommand»", ["- Func<Task> _execute", "+ bool IsRunning"]),

    "IAuthService": ("«interface»", ["+ User? Login(login, pwd)", "+ void Logout()",
                                     "+ void UpdateProfile(...)", "+ void ChangePassword(...)"]),
    "AuthService": ("", ["- IRepository _repo", "- IPasswordHasher _hasher",
                         "- User? _currentUser"]),
    "IPasswordHasher": ("«interface»", ["+ string HashPassword(pwd)",
                                        "+ bool VerifyPassword(...)"]),
    "BCryptHasher": ("", ["- int _workFactor"]),
    "SHA256Hasher": ("", ["- bool _legacy"]),
    "IUndoRedoService": ("«interface»", ["+ void Push(action)", "+ void Undo()", "+ void Redo()"]),
    "UndoRedoService": ("", ["- Stack _undoStack"]),

    "IThemeService": ("«interface»", ["+ void SetTheme(bool)"]),
    "ThemeService": ("", []),
    "ILocalizationService": ("«interface»", ["+ void SetLanguage(code)"]),
    "LocalizationService": ("", []),
    "DatabaseHealthCheckService": ("", ["- string _connectionString", "+ Task CheckDatabase()"]),
    "CompositionRoot": ("«static»", ["+ IServiceProvider Build()"]),

    "IRepository": ("«interface»", ["+ GetAllApps()", "+ void AddApp(App)",
                                    "+ void UpdateApp(App)", "+ User? GetUserByLogin()",
                                    "+ void UpdateUser(User)"]),
    "PostgreSQLRepEF": ("", ["- string _conn"]),
    "PostgreSQLRepADO": ("", ["- string _conn"]),
    "JsonRep": ("", ["- string _path"]),
}

CLASS_COLUMNS = [
    ["MainWindow", "LoginWindow", "AddEditWindow", "ProfilePopup",
     "App", "User", "UsersApps"],
    ["ViewModelBase", "MainViewModel", "ProfileViewModel", "AddEditAppViewModel",
     "RelayCommand", "AsyncRelayCommand"],
    ["IAuthService", "AuthService", "IPasswordHasher", "BCryptHasher",
     "SHA256Hasher", "IUndoRedoService", "UndoRedoService"],
    ["IThemeService", "ThemeService", "ILocalizationService", "LocalizationService",
     "DatabaseHealthCheckService", "CompositionRoot", "IRepository",
     "PostgreSQLRepEF", "PostgreSQLRepADO", "JsonRep"],
]

# (src, tgt, kind): assoc | dep | real | gen
CLASS_LINKS = [
    ("MainWindow", "MainViewModel", "assoc"),
    ("AddEditWindow", "AddEditAppViewModel", "assoc"),
    ("ProfilePopup", "ProfileViewModel", "assoc"),
    ("LoginWindow", "AuthService", "dep"),
    ("MainViewModel", "ViewModelBase", "gen"),
    ("ProfileViewModel", "ViewModelBase", "gen"),
    ("AddEditAppViewModel", "ViewModelBase", "gen"),
    ("MainViewModel", "RelayCommand", "dep"),
    ("MainViewModel", "IAuthService", "dep"),
    ("MainViewModel", "IUndoRedoService", "dep"),
    ("MainViewModel", "IRepository", "dep"),
    ("ProfileViewModel", "IAuthService", "dep"),
    ("ProfileViewModel", "IThemeService", "dep"),
    ("AuthService", "IAuthService", "real"),
    ("AuthService", "IPasswordHasher", "dep"),
    ("AuthService", "User", "dep"),
    ("BCryptHasher", "IPasswordHasher", "real"),
    ("SHA256Hasher", "IPasswordHasher", "real"),
    ("UndoRedoService", "IUndoRedoService", "real"),
    ("ThemeService", "IThemeService", "real"),
    ("LocalizationService", "ILocalizationService", "real"),
    ("CompositionRoot", "AuthService", "dep"),
    ("CompositionRoot", "IRepository", "dep"),
    ("CompositionRoot", "DatabaseHealthCheckService", "dep"),
    ("PostgreSQLRepEF", "IRepository", "real"),
    ("PostgreSQLRepADO", "IRepository", "real"),
    ("JsonRep", "IRepository", "real"),
]

CLASS_STYLE = {"assoc": ST_ASSOC, "dep": ST_DEP, "real": ST_REAL, "gen": ST_GEN}


def build_class():
    box_w, row_h, head_h, gap_x, gap_y = 250, 20, 54, 25, 26
    heights = [
        sum(head_h + row_h * max(1, len(CLASSES[n][1])) + gap_y for n in col)
        for col in CLASS_COLUMNS
    ]
    total_h = max(heights)
    width = len(CLASS_COLUMNS) * box_w + (len(CLASS_COLUMNS) - 1) * gap_x + 20
    d = Dia("class", width + 20, total_h + 40)
    ids = {}
    for ci, col in enumerate(CLASS_COLUMNS):
        y = 20
        x = 10 + ci * (box_w + gap_x)
        for name in col:
            stereo, rows = CLASSES[name]
            h = head_h + row_h * max(1, len(rows))
            ids[name] = d.vertex(name, ST_CLASS, x, y, box_w, h)
            if stereo:
                d.vertex(stereo, ST_ROW.replace("align=left", "align=center")
                         .replace("spacingLeft=5", ""), 0, 1, box_w, 20,
                         parent=ids[name])
            for i, row in enumerate(rows):
                d.vertex(
                    row.replace("<", "&lt;").replace(">", "&gt;"),
                    ST_ROW, 0, head_h + i * row_h, box_w, row_h, parent=ids[name],
                )
            y += h + gap_y
    col_of = {name: ci for ci, col in enumerate(CLASS_COLUMNS) for name in col}
    row_of = {}
    for ci, col in enumerate(CLASS_COLUMNS):
        y = 20
        for name in col:
            row_of[name] = y
    for src, tgt, kind in CLASS_LINKS:
        if src not in ids or tgt not in ids:
            continue
        if col_of[src] == col_of[tgt]:
            exit_, entry = (0.5, 0.0), (0.5, 1.0)
            if row_of[tgt] < row_of[src]:
                exit_, entry = (0.5, 0.0), (0.5, 1.0)
        elif col_of[tgt] > col_of[src]:
            exit_, entry = (1.0, 0.5), (0.0, 0.5)
        else:
            exit_, entry = (0.0, 0.5), (1.0, 0.5)
        d.edge(ids[src], ids[tgt], CLASS_STYLE[kind], exit_=exit_, entry=entry)
    return d


# ══════════════════════════════════════════════════════════════════════════
# 3. ПОСЛЕДОВАТЕЛЬНОСТЬ
# ══════════════════════════════════════════════════════════════════════════
LIFELINES = ["Пользователь", "Приложение", "Окно входа",
             "Служба аутентификации", "Хранилище данных", "CompositionRoot"]

# (источник, приёмник, подпись, тип): call | ret
MESSAGES = [
    ("Пользователь", "Приложение", "запуск приложения", "call"),
    ("Приложение", "CompositionRoot", "создать контейнер зависимостей", "call"),
    ("CompositionRoot", "Служба аутентификации", "создать службу", "call"),
    ("Приложение", "Окно входа", "открыть окно входа", "call"),
    ("Пользователь", "Окно входа", "ввод логина и пароля", "call"),
    ("Окно входа", "Служба аутентификации", "проверить учётные данные", "call"),
    ("Служба аутентификации", "Хранилище данных", "запрос пользователя по логину", "call"),
    ("Хранилище данных", "Служба аутентификации", "запись пользователя", "ret"),
    ("Служба аутентификации", "Окно входа", "результат проверки", "ret"),
    ("Окно входа", "Приложение", "успешный вход", "ret"),
    ("Пользователь", "Приложение", "запрос списка приложений", "call"),
    ("Приложение", "Хранилище данных", "выборка каталога", "call"),
    ("Хранилище данных", "Приложение", "список приложений", "ret"),
    ("Пользователь", "Приложение", "установить приложение", "call"),
    ("Приложение", "Хранилище данных", "запись установки", "call"),
    ("Хранилище данных", "Приложение", "установка сохранена", "ret"),
    ("Пользователь", "Приложение", "отменить действие", "call"),
    ("Приложение", "Хранилище данных", "удаление установки", "call"),
    ("Хранилище данных", "Приложение", "список обновлён", "ret"),
    ("Пользователь", "Приложение", "выход из аккаунта", "call"),
    ("Приложение", "Окно входа", "показать окно входа", "ret"),
]


def build_sequence():
    col_w, gap, top, step = 165, 28, 20, 52
    width = len(LIFELINES) * col_w + (len(LIFELINES) - 1) * gap + 40
    height = top + 70 + step * (len(MESSAGES) + 1) + 30
    d = Dia("sequence", width, height)
    xs = {}
    for i, name in enumerate(LIFELINES):
        x = 20 + i * (col_w + gap)
        xs[name] = x + col_w // 2
        d.vertex(name, ST_LIFE, x, top, col_w, height - top - 20)
    # фокус управления «Приложения» на время сценария
    d.vertex(
        "", ST_ACT,
        xs["Приложение"] - 8, top + 60, 16,
        step * (len(MESSAGES) - 1), parent="1",
    )
    for i, (src, tgt, label, kind) in enumerate(MESSAGES):
        y = top + 82 + i * step
        style = ST_CALL if kind == "call" else ST_RET
        d.free_edge(label, style, xs[src], y, xs[tgt], y)
    return d


BUILDERS = [
    ("use_case.drawio", build_use_case),
    ("class_diagram.drawio", build_class),
    ("sequence_diagram.drawio", build_sequence),
]

if __name__ == "__main__":
    only_png = "--png-only" in sys.argv
    ok_all = True
    for fname, builder in BUILDERS:
        path = os.path.join(UML_DIR, fname)
        if not only_png:
            write(builder(), fname)
        if not export_png(path):
            ok_all = False
    print("ALL_OK" if ok_all else "SOME_FAILED")
    sys.exit(0 if ok_all else 1)

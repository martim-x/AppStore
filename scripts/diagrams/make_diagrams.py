#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
Генератор UML/ERD диаграмм AppStore в формате .drawio (mxGraphModel XML).
Экспорт в PNG выполняется draw.io CLI:  draw.io.exe -x -f png -s 2 -b 15 ...
"""
import html
import os
import shutil
import subprocess
import sys

UML_DIR = os.environ.get("UML_DIR", os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), "docs", "UML"))
# Нейтральный deфолт: имя исполняемого файла из PATH (draw.io на PATH)
DRAWIO = os.environ.get("DRAWIO", "draw.io")

# Только позиционные аргументы: флаги вида --png-only не считаются путями
_positional = [a for a in sys.argv[1:] if not a.startswith("--")]
if len(_positional) > 0:
    UML_DIR = _positional[0]
if len(_positional) > 1:
    DRAWIO = _positional[1]

# ── стили ────────────────────────────────────────────────────────────────
ST_ACTOR = ("shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;"
            "html=1;align=center;outlineConnect=0;fillColor=none;strokeColor=#000000;")
ST_UC = ("shape=ellipse;whiteSpace=wrap;html=1;fillColor=none;strokeColor=#000000;"
         "fontSize=14;")
ST_FRAME = ("rounded=0;whiteSpace=wrap;html=1;fillColor=none;dashed=1;dashPattern=8 8;"
            "strokeColor=#000000;verticalAlign=top;align=left;spacingLeft=8;"
            "spacingTop=2;fontSize=12;fontStyle=1;")
ST_CLASS = ("shape=swimlane;html=1;startSize=30;horizontal=1;whiteSpace=wrap;"
            "fillColor=none;strokeColor=#000000;fontSize=14;align=left;")
ST_IFACE = ("shape=swimlane;html=1;startSize=30;horizontal=1;whiteSpace=wrap;"
            "fillColor=none;strokeColor=#000000;fontSize=14;align=left;")
ST_TABLE = ("shape=swimlane;html=1;startSize=30;horizontal=1;whiteSpace=wrap;"
            "fillColor=none;strokeColor=#000000;fontSize=14;align=left;")
ST_ASSOC = "endArrow=none;html=1;rounded=0;edgeStyle=orthogonalEdgeStyle;"
ST_DEP = "endArrow=open;dashed=1;html=1;strokeColor=#000000;rounded=0;edgeStyle=orthogonalEdgeStyle;"
ST_MSG = "endArrow=open;html=1;verticalAlign=bottom;strokeWidth=1;labelBackgroundColor=none;fontSize=13;rounded=0;edgeStyle=orthogonalEdgeStyle;"
ST_MSG_D = "endArrow=open;html=1;verticalAlign=bottom;dashed=1;strokeColor=#000000;rounded=0;edgeStyle=orthogonalEdgeStyle;"
ST_ER = "endArrow=ERmany;startArrow=ERone;html=1;endFill=0;startFill=0;strokeColor=#000000;rounded=0;edgeStyle=orthogonalEdgeStyle;"
ST_LIFE = ("shape=umlLifeline;html=1;whiteSpace=wrap;fillColor=none;"
           "strokeColor=#000000;fontSize=14;")
ST_NOTE = ("shape=note;whiteSpace=wrap;html=1;size=16;fillColor=none;"
           "strokeColor=#000000;fontSize=13;align=left;")


class Dia:
    """Накопитель mxCell."""

    def __init__(self, name, width=1400, height=900):
        self.name = name
        self.width = width
        self.height = height
        self.cells = []
        self.n = 0

    def _id(self, prefix):
        self.n += 1
        return f"{prefix}{self.n}"

    def vertex(self, value, style, x, y, w, h, parent="1"):
        cid = self._id("v")
        # Переносы строк внутри XML-атрибута энкодируем в &#10; иначе парсер
        # draw.io нормализует их в пробелы и текст «склеивается» в одну строку.
        val = html.escape(value, quote=True).replace("\n", "&#10;")
        self.cells.append(
            f'        <mxCell id="{cid}" value="{val}" style="{style}" vertex="1" parent="{parent}">\n'
            f'          <mxGeometry x="{x}" y="{y}" width="{w}" height="{h}" as="geometry" />\n'
            f'        </mxCell>')
        return cid

    def table(self, title, body, x, y, w, h, style):
        """Таблица как swimlane (заголовок) + дочерняя вершина со столбцами."""
        head = self.vertex(title, style, x, y, w, h)
        self.cells.append(
            f'        <mxCell id="body{len(self.cells)}" value="{body}" '
            f'style="text;html=1;align=left;verticalAlign=top;spacingLeft=6;'
            f'spacingTop=2;fontSize=14;whiteSpace=wrap;" vertex="1" parent="{head}">\n'
            f'          <mxGeometry x="0" y="26" width="{w}" height="{h - 26}" '
            f'as="geometry" />\n'
            f'        </mxCell>')
        return head

    def edge(self, source, target, style=ST_ASSOC, label="", x=0, y=0, points=None):
        cid = self._id("e")
        val = html.escape(label, quote=True).replace("\n", "&#10;")
        lbl = (f' value="{val}"' if val else "")
        geom = (f'          <mxGeometry relative="1" as="geometry">\n'
                f'            <mxPoint x="{x}" y="{y}" as="offset" />\n'
                f'          </mxGeometry>')
        if points:
            pts = "".join(f'            <mxPoint x="{px}" y="{py}" />\n' for px, py in points)
            geom = (f'          <mxGeometry relative="1" as="geometry">\n{pts}'
                    f'          </mxGeometry>')
        self.cells.append(
            f'        <mxCell id="{cid}"{lbl} style="{style}" edge="1" parent="1" '
            f'source="{source}" target="{target}">\n{geom}\n        </mxCell>')
        return cid

    def xml(self):
        return (
            '<?xml version="1.0" encoding="UTF-8"?>\n'
            '<mxfile host="app.diagrams.net" agent="hermes" version="24.7.17" type="device">\n'
            f'  <diagram id="{self.name}" name="{self.name}">\n'
            f'    <mxGraphModel dx="{self.width}" dy="{self.height}" grid="1" gridSize="10" '
            'guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" '
            f'pageWidth="{self.width}" pageHeight="{self.height}" math="0" shadow="0">\n'
            '      <root>\n'
            '        <mxCell id="0" />\n'
            '        <mxCell id="1" parent="0" />\n'
            + "\n".join(self.cells) + "\n"
            '      </root>\n'
            '    </mxGraphModel>\n'
            '  </diagram>\n'
            '</mxfile>\n')


def write(dia, filename):
    path = os.path.join(UML_DIR, filename)
    with open(path, "w", encoding="utf-8") as f:
        f.write(dia.xml())
    print(f"WROTE {path}")
    return path


def export_png(path, scale=2, border=15):
    out = path[:-len(".drawio")] + ".png"
    if shutil.which(DRAWIO) is None:
        print(f"EXPORT SKIP {out}: draw.io CLI '{DRAWIO}' not found in PATH "
              "(set DRAWIO=... or install draw.io)")
        return False
    cmd = [DRAWIO, "-x", "-f", "png", "-s", str(scale), "-b", str(border), "-o", out, path]
    try:
        r = subprocess.run(cmd, capture_output=True, text=True, timeout=300)
    except OSError as exc:
        print(f"EXPORT FAIL {out}: {exc}")
        return False
    ok = os.path.exists(out) and os.path.getsize(out) > 5000
    print(f"EXPORT {'OK ' if ok else 'FAIL'} {out} "
          f"({os.path.getsize(out) if os.path.exists(out) else 0} bytes)")
    if not ok:
        print("  stdout:", r.stdout.strip()[:300])
        print("  stderr:", r.stderr.strip()[:300])
    return ok


# ══════════════════════════════════════════════════════════════════════════
# 1. USE CASE
# ══════════════════════════════════════════════════════════════════════════
def build_use_case():
    d = Dia("UseCase", 1000, 980)

    client = d.vertex("Клиент", ST_ACTOR, 40, 460, 40, 70)
    admin = d.vertex("Администратор", ST_ACTOR, 940, 120, 40, 70)

    client_ucs = [
        "Войти в аккаунт",
        "Выйти из аккаунта",
        "Зарегистрироваться",
        "Просмотреть каталог",
        "Найти приложение",
        "Отфильтровать приложения",
        "Отсортировать приложения",
        "Просмотреть карточку приложения",
        "Установить приложение",
        "Удалить приложение",
        "Отменить действие",
        "Повторить действие",
        "Просмотреть все приложения",
        "Просмотреть профиль",
        "Изменить профиль",
        "Сменить пароль",
        "Сменить язык",
        "Сменить тему",
    ]
    admin_ucs = [
        "Добавить приложение",
        "Редактировать приложение",
        "Удалить приложение",
        "Восстановить настройки по умолчанию",
    ]

    # Lay out in two columns, 9 per column
    half = (len(client_ucs) + 1) // 2
    col1 = client_ucs[:half]
    col2 = client_ucs[half:]
    client_ids = []
    for i, name in enumerate(col1):
        cid = d.vertex(name, ST_UC, 160, 60 + i * 52, 190, 40)
        client_ids.append(cid)
        d.edge(client, cid)
    for i, name in enumerate(col2):
        cid = d.vertex(name, ST_UC, 380, 60 + i * 52, 190, 40)
        client_ids.append(cid)
        d.edge(client, cid)

    admin_ids = []
    for i, name in enumerate(admin_ucs):
        cid = d.vertex(name, ST_UC, 680, 60 + i * 52, 220, 40)
        admin_ids.append(cid)
        d.edge(admin, cid)

    return d


# ══════════════════════════════════════════════════════════════════════════
# 2. CLASS DIAGRAM
# ══════════════════════════════════════════════════════════════════════════
ST_ROW = ("text;html=1;align=left;verticalAlign=middle;spacingLeft=6;"
          "fontSize=13;strokeColor=none;fillColor=none;whiteSpace=nowrap;")
ST_INH = ("endArrow=block;endFill=0;html=1;strokeColor=#000000;rounded=0;"
          "edgeStyle=orthogonalEdgeStyle;")


def build_class():
    d = Dia("ClassDiagram", 1090, 820)

    def esc(t):
        # html=1 в draw.io съедает <App> как тег; двойное экранирование сохраняет угловые скобки
        return t.replace("<", "&lt;").replace(">", "&gt;")

    def box(title, rows, x, y, w=250, style=ST_CLASS):
        h = 30 + 19 * max(1, len(rows))
        head = d.vertex(esc(title), style, x, y, w, h)
        for i, row in enumerate(rows):
            d.vertex(esc(row), ST_ROW, 0, 30 + 19 * i, w, 19, parent=head)
        return head

    def link(a, b, ex, ey, nx, ny, style=ST_DEP):
        st = (style + f"exitX={ex};exitY={ey};exitDx=0;exitDy=0;"
                      f"entryX={nx};entryY={ny};entryDx=0;entryDy=0;")
        return d.edge(a, b, st)

    # ── колонка 1: представление и модели представления ──────────────────
    x1 = 20
    main_win = box("MainWindow", ["«View»", "+ DataContext : MainViewModel"], x1, 20)
    main_vm = box("MainViewModel : ViewModelBase", [
        "- IRepository _repo",
        "- IAuthService _auth",
        "- IUndoRedoService _undo",
        "+ ObservableCollection<App> Apps",
        "+ string SearchQuery",
        "+ ICommand UndoCommand",
    ], x1, 110)
    vm_base = box("ViewModelBase", ["«abstract»", "+ INotifyPropertyChanged"], x1, 310)
    profile_vm = box("ProfileViewModel : ViewModelBase", [
        "- IAuthService _auth",
        "+ string FirstName",
        "+ string Email",
        "+ ICommand SaveCommand",
    ], x1, 400)

    # ── колонка 2: сервисы ───────────────────────────────────────────────
    x2 = 300
    auth = box("AuthService", [
        "- IRepository _repo",
        "- IPasswordHasher _hasher",
        "- User? _currentUser",
        "+ User? Login(login, password)",
        "+ void Logout()",
        "+ bool IsAdmin",
    ], x2, 20)
    hasher = box("«interface» IPasswordHasher", [
        "+ string HashPassword(string pwd)",
        "+ bool VerifyPassword(pwd, hash)",
    ], x2, 200, style=ST_IFACE)
    bcrypt = box("BCryptHasher", ["«work factor 12»"], x2, 310)
    undo = box("UndoRedoService", [
        "- Stack<IUndoRedoAction> _stack",
        "+ void Undo()",
        "+ void Redo()",
    ], x2, 400)
    theme = box("ThemeService", ["«Singleton»", "+ bool IsDarkMode"], x2, 530)
    loc = box("LocalizationService", ["«Singleton»", "+ string Language"], x2, 620)

    # ── колонка 3: доступ к данным ───────────────────────────────────────
    x3 = 580
    repo = box("«interface» IRepository", [
        "+ List<App> GetAllApps()",
        "+ App? GetAppById(Guid id)",
        "+ void AddApp(App app)",
        "+ void UpdateApp(App app)",
        "+ void DeleteApp(Guid id)",
        "+ User? GetUserByLogin(string login)",
        "+ void UpdateUser(User user)",
    ], x3, 20, style=ST_IFACE)
    ef = box("PostgreSQLRepEF", ["«EF Core 8 + Npgsql»"], x3, 240)
    ado = box("PostgreSQLRepADO", ["«Npgsql ADO.NET»"], x3, 310)
    js = box("JsonRep", ["«Resources/Buffer/*.json»"], x3, 380)

    # ── колонка 4: модели предметной области ─────────────────────────────
    x4 = 850
    iapp = box("«interface» IApp", [
        "+ Guid Id",
        "+ string ShortName",
        "+ string FullName",
        "+ double Rating",
        "+ double Price",
    ], x4, 20, style=ST_IFACE)
    app = box("App : IApp", [
        "+ Guid Id",
        "+ string ShortName",
        "+ string FullName",
        "+ double Rating",
        "+ double Price",
        "+ bool IsFeatured",
    ], x4, 180)
    iuser = box("«interface» IUser", [
        "+ Guid Id",
        "+ string Login",
        "+ string PasswordHash",
        "+ UserRole Role",
        "+ string AvatarColor",
    ], x4, 340, style=ST_IFACE)
    user = box("User : IUser", [
        "+ Guid Id",
        "+ string Login",
        "+ string PasswordHash",
        "+ UserRole Role",
        "+ string AvatarColor",
        "+ string? Email",
    ], x4, 500)
    uapp = box("UsersApps", ["+ Guid UserId", "+ Guid AppId", "+ DateTime InstalledAt"], x4, 700)

    # ── связи: короткие, между соседними блоками ─────────────────────────
    link(main_win, main_vm, 0.5, 1, 0.5, 0)
    link(main_vm, vm_base, 0.5, 1, 0.5, 0, ST_INH)
    link(profile_vm, vm_base, 0.5, 0, 0.5, 1, ST_INH)

    link(main_vm, auth, 1, 0.35, 0, 0.75)
    link(main_vm, undo, 1, 0.75, 0, 0.35)
    link(profile_vm, undo, 1, 0.25, 0, 0.75)

    link(auth, hasher, 0.5, 1, 0.5, 0)
    link(hasher, bcrypt, 0.5, 1, 0.5, 0, ST_INH)
    link(auth, repo, 1, 0.25, 0, 0.06)

    link(repo, ef, 0.5, 1, 0.5, 0, ST_INH)
    link(ef, ado, 0.5, 1, 0.5, 0)
    link(ado, js, 0.5, 1, 0.5, 0)

    link(iapp, app, 0.5, 1, 0.5, 0, ST_INH)
    link(iuser, user, 0.5, 1, 0.5, 0, ST_INH)
    link(user, uapp, 0.5, 1, 0.5, 0, "endArrow=none;html=1;strokeColor=#000000;rounded=0;edgeStyle=orthogonalEdgeStyle;")
    return d


# ══════════════════════════════════════════════════════════════════════════
# 3. SEQUENCE DIAGRAM (SD-01: запуск и вход)
# ══════════════════════════════════════════════════════════════════════════
def build_sequence():
    d = Dia("SequenceDiagram", 820, 780)

    lifelines = [
        ("Пользователь", 80),
        ("Application", 230),
        ("LoginWindow", 400),
        ("AuthService", 570),
        ("IRepository", 720),
    ]
    for name, x in lifelines:
        d.vertex(name, ST_LIFE, x - 65, 40, 130, 36)
        d.cells.append(
            f'        <mxCell id="life{len(d.cells)}" style="endArrow=none;html=1;'
            f'strokeColor=#000000;dashed=1;rounded=0;" edge="1" parent="1">\n'
            f'          <mxGeometry relative="1" as="geometry">\n'
            f'            <mxPoint x="{x}" y="76" as="sourcePoint" />\n'
            f'            <mxPoint x="{x}" y="750" as="targetPoint" />\n'
            f'          </mxGeometry>\n'
            f'        </mxCell>')

    def m(src, dst, label, y, dashed=False):
        sx = [x for n, x in lifelines if n == src][0]
        dx = [x for n, x in lifelines if n == dst][0]
        val = html.escape(label, quote=True)
        style = ST_MSG_D if dashed else ST_MSG
        if sx == dx:
            d.cells.append(
                f'        <mxCell id="msg{len(d.cells)}" value="{val}" style="{style}" '
                f'edge="1" parent="1">\n'
                f'          <mxGeometry relative="1" as="geometry">\n'
                f'            <mxPoint x="{sx}" y="{y}" as="sourcePoint" />\n'
                f'            <mxPoint x="{sx}" y="{y + 26}" as="targetPoint" />\n'
                f'            <Array as="points">\n'
                f'              <mxPoint x="{sx + 45}" y="{y}" />\n'
                f'              <mxPoint x="{sx + 45}" y="{y + 26}" />\n'
                f'            </Array>\n'
                f'          </mxGeometry>\n'
                f'        </mxCell>')
            return
        d.cells.append(
            f'        <mxCell id="msg{len(d.cells)}" value="{val}" style="{style}" '
            f'edge="1" parent="1">\n'
            f'          <mxGeometry relative="1" as="geometry">\n'
            f'            <mxPoint x="{sx}" y="{y}" as="sourcePoint" />\n'
            f'            <mxPoint x="{dx}" y="{y}" as="targetPoint" />\n'
            f'          </mxGeometry>\n'
            f'        </mxCell>')

    m("Пользователь", "Application", "запуск приложения", 120)
    m("Application", "Application", "CompositionRoot.Build()", 165)
    m("Application", "AuthService", "создание AuthService", 220)
    m("Application", "LoginWindow", "ShowDialog()", 270)
    m("Пользователь", "LoginWindow", "логин и пароль", 320)
    m("LoginWindow", "AuthService", "Login(login, password)", 370)
    m("AuthService", "IRepository", "GetUserByLogin(login)", 420)
    m("IRepository", "AuthService", "User / null", 470, dashed=True)
    m("AuthService", "LoginWindow", "User / null", 520, dashed=True)
    m("LoginWindow", "Application", "DialogResult = true", 570, dashed=True)
    m("Application", "IRepository", "GetAllApps()", 620)
    m("IRepository", "Application", "List&lt;App&gt;", 670, dashed=True)
    return d


# ══════════════════════════════════════════════════════════════════════════
# 4. ER DIAGRAM
# ══════════════════════════════════════════════════════════════════════════
def build_erd():
    d = Dia("ERDiagram", 900, 560)

    users = d.table(
        "users",
        "PK id uuid&#10;UK login text&#10;  password_hash text&#10;  first_name text&#10;"
        "  last_name text&#10;  email text&#10;  role integer&#10;  avatar_color text",
        40, 60, 250, 200, ST_TABLE)

    apps = d.table(
        "apps",
        "PK id uuid&#10;  short_name text&#10;  full_name text&#10;  description text&#10;"
        "  developer text&#10;  category text&#10;  rating double&#10;  price double&#10;"
        "  version text&#10;  size_mb double&#10;  release_date date&#10;  is_featured bool",
        600, 60, 250, 230, ST_TABLE)

    ua = d.table(
        "users_apps",
        "PK (user_id, app_id) uuid&#10;FK user_id uuid&#10;FK app_id uuid&#10;"
        "  installed_at date",
        320, 330, 240, 160, ST_TABLE)

    d.edge(users, ua, ST_ER, "имеет")
    d.edge(apps, ua, ST_ER, "установлено")

    d.vertex("Связь «многие ко многим» реализована через промежуточную таблицу "
             "users_apps: ON DELETE CASCADE с обеих сторон.\n"
             "Роли: role = 0 (User), role = 1 (Admin).",
             ST_NOTE, 600, 330, 280, 100)
    return d


# use_case.drawio и erd.drawio ведутся вручную в draw.io — генератор их не перезаписывает.
BUILDERS = [
    ("class_diagram.drawio", build_class),
    ("sequence_diagram.drawio", build_sequence),
]

if __name__ == "__main__":
    only_png = "--png-only" in sys.argv
    ok_all = True
    for fname, builder in BUILDERS:
        path = os.path.join(UML_DIR, fname)
        if not only_png:
            dia = builder()
            write(dia, fname)
        if not export_png(path):
            ok_all = False
    print("ALL_OK" if ok_all else "SOME_FAILED")
    sys.exit(0 if ok_all else 1)

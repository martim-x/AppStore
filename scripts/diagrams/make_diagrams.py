#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
Генератор UML/ERD диаграмм AppStore в формате .drawio (mxGraphModel XML).
Экспорт в PNG выполняется draw.io CLI:  draw.io.exe -x -f png -s 2 -b 15 ...
"""
import os
import subprocess
import sys

UML_DIR = r"C:\Users\timof\Documents\Visual Studio Code\AppStore\docs\UML"
DRAWIO = r"C:\Users\timof\AppData\Local\Programs\draw.io\draw.io.exe"

# ── стили ────────────────────────────────────────────────────────────────
ST_ACTOR = ("shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;"
            "html=1;align=center;outlineConnect=0;fillColor=#ffe6cc;strokeColor=#d79b00;")
ST_UC = ("shape=ellipse;whiteSpace=wrap;html=1;fillColor=#ffe6cc;strokeColor=#d79b00;"
         "fontSize=14;")
ST_FRAME = ("rounded=0;whiteSpace=wrap;html=1;fillColor=none;dashed=1;dashPattern=8 8;"
            "strokeColor=#666666;verticalAlign=top;align=left;spacingLeft=8;"
            "spacingTop=2;fontSize=12;fontStyle=1;")
ST_CLASS = ("shape=swimlane;html=1;startSize=30;horizontal=1;whiteSpace=wrap;"
            "fillColor=#d5e8d4;strokeColor=#82b366;fontSize=14;align=left;")
ST_IFACE = ("shape=swimlane;html=1;startSize=30;horizontal=1;whiteSpace=wrap;"
            "fillColor=#dae8fc;strokeColor=#6c8ebf;fontSize=14;align=left;")
ST_TABLE = ("shape=swimlane;html=1;startSize=30;horizontal=1;whiteSpace=wrap;"
            "fillColor=#f8cecc;strokeColor=#b85450;fontSize=14;align=left;")
ST_ASSOC = "endArrow=none;html=1;rounded=0;"
ST_DEP = "endArrow=open;dashed=1;html=1;strokeColor=#6c8ebf;"
ST_MSG = "endArrow=open;html=1;verticalAlign=bottom;strokeWidth=1;labelBackgroundColor=none;fontSize=13;"
ST_MSG_D = "endArrow=open;html=1;verticalAlign=bottom;dashed=1;strokeColor=#666666;"
ST_ER = "endArrow=ERmany;startArrow=ERone;html=1;endFill=0;startFill=0;strokeColor=#b85450;"
ST_LIFE = ("shape=umlLifeline;html=1;whiteSpace=wrap;fillColor=#dae8fc;"
           "strokeColor=#6c8ebf;fontSize=14;")
ST_NOTE = ("shape=note;whiteSpace=wrap;html=1;size=16;fillColor=#fff2cc;"
           "strokeColor=#d6b656;fontSize=13;align=left;")


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
        val = (value.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
               .replace("\n", "&#10;"))
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
        val = (label.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;"))
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
    cmd = [DRAWIO, "-x", "-f", "png", "-s", str(scale), "-b", str(border), "-o", out, path]
    r = subprocess.run(cmd, capture_output=True, text=True, timeout=300)
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
    d = Dia("UseCase", 900, 720)

    auth = d.vertex("Аутентификация", ST_FRAME, 110, 20, 330, 110)
    uc_login = d.vertex("UC-01: Вход", ST_UC, 130, 55, 140, 42)
    uc_logout = d.vertex("UC-02: Выход", ST_UC, 290, 55, 140, 42)

    shared = d.vertex("Каталог (общий)", ST_FRAME, 110, 150, 500, 250)
    uc_view = d.vertex("UC-03: Просмотр каталога", ST_UC, 125, 190, 150, 40)
    uc_search = d.vertex("UC-04: Поиск", ST_UC, 290, 190, 140, 40)
    uc_filter = d.vertex("UC-05: Фильтрация", ST_UC, 445, 190, 140, 40)
    uc_sort = d.vertex("UC-06: Сортировка", ST_UC, 125, 250, 140, 40)
    uc_detail = d.vertex("UC-07: Карточка", ST_UC, 290, 250, 140, 40)
    uc_install = d.vertex("UC-08: Установка", ST_UC, 445, 250, 140, 40)
    uc_undo = d.vertex("UC-09: Отмена / повтор", ST_UC, 125, 310, 165, 40)
    uc_seemore = d.vertex("UC-10: Все приложения", ST_UC, 310, 310, 150, 40)

    prof = d.vertex("Профиль (общий)", ST_FRAME, 110, 420, 500, 190)
    uc_pview = d.vertex("UC-11: Просмотр профиля", ST_UC, 125, 460, 160, 40)
    uc_pedit = d.vertex("UC-12: Изменение профиля", ST_UC, 300, 460, 165, 40)
    uc_pwd = d.vertex("UC-13: Смена пароля", ST_UC, 480, 460, 120, 40)
    uc_lang = d.vertex("UC-14: Язык RU/EN", ST_UC, 125, 520, 150, 40)
    uc_theme = d.vertex("UC-15: Тема Light/Dark", ST_UC, 300, 520, 170, 40)

    adm = d.vertex("Только администратор", ST_FRAME, 630, 20, 260, 240)
    uc_add = d.vertex("UC-16: Добавление", ST_UC, 645, 60, 220, 40)
    uc_edit = d.vertex("UC-17: Редактирование", ST_UC, 645, 115, 220, 40)
    uc_del = d.vertex("UC-18: Удаление", ST_UC, 645, 170, 220, 40)
    uc_restore = d.vertex("UC-19: Восстановление", ST_UC, 645, 225, 220, 40)

    admin = d.vertex("Администратор", ST_ACTOR, 20, 150, 40, 70)
    client = d.vertex("Клиент", ST_ACTOR, 20, 480, 40, 70)

    for u in (uc_login, uc_logout, uc_view, uc_search, uc_filter, uc_sort, uc_detail,
              uc_install, uc_undo, uc_seemore, uc_pview, uc_pedit, uc_pwd, uc_lang, uc_theme):
        d.edge(admin, u)
        d.edge(client, u)
    for u in (uc_add, uc_edit, uc_del, uc_restore):
        d.edge(admin, u)

    d.edge(uc_install, uc_undo, ST_DEP, "вкл.")
    d.edge(uc_pedit, uc_pwd, ST_DEP, "вкл.")
    d.edge(uc_restore, uc_del, ST_DEP, "расш.")
    return d


# ══════════════════════════════════════════════════════════════════════════
# 2. CLASS DIAGRAM
# ══════════════════════════════════════════════════════════════════════════
def build_class():
    d = Dia("ClassDiagram", 1060, 1000)

    iapp = d.vertex(
        "«interface» IApp\n"
        "+ Guid Id\n+ string ShortName\n+ string FullName\n+ string Description\n"
        "+ string Developer\n+ string Category\n+ double Rating\n+ double Price\n"
        "+ string Version\n+ DateTime ReleaseDate\n+ List&lt;string&gt; Tags\n"
        "+ bool IsFeatured\n+ bool IsDownloaded",
        ST_IFACE, 20, 20, 230, 295)

    app = d.vertex(
        "App\n"
        "+ Guid Id\n+ string ShortName\n+ string FullName\n+ string Description\n"
        "+ string Developer\n+ string Category\n+ double Rating\n+ double Price\n"
        "+ string Version\n+ DateTime ReleaseDate\n+ List&lt;string&gt; Tags\n"
        "+ bool IsFeatured\n+ bool IsDownloaded",
        ST_CLASS, 280, 20, 230, 295)

    iuser = d.vertex(
        "«interface» IUser\n"
        "+ Guid Id\n+ string Login\n+ string PasswordHash\n+ string? FirstName\n"
        "+ string? LastName\n+ string? Email\n+ UserRole Role\n+ string AvatarColor",
        ST_IFACE, 20, 340, 230, 215)

    user = d.vertex(
        "User\n"
        "+ Guid Id\n+ string Login\n+ string PasswordHash\n+ string? FirstName\n"
        "+ string? LastName\n+ string? Email\n+ UserRole Role\n+ string AvatarColor",
        ST_CLASS, 280, 340, 230, 215)

    uapp = d.vertex(
        "UsersApps\n"
        "+ Guid Id\n+ Guid UserId\n+ Guid AppId\n+ DateTime InstalledAt\n"
        "+ bool IsPurchased",
        ST_CLASS, 280, 590, 230, 155)

    undo = d.vertex("UndoRedoService\n«Stack, лимит 10»\n+ void Undo()\n+ void Redo()",
                    ST_CLASS, 20, 590, 230, 135)
    action = d.vertex("«interface» IUndoRedoAction\n+ void Undo()\n+ void Redo()",
                      ST_IFACE, 20, 760, 230, 115)

    irepo = d.vertex(
        "«interface» IRepository\n"
        "+ List&lt;App&gt; GetAllApps()\n+ App? GetAppById(Guid id)\n"
        "+ void AddApp(App app)\n+ void UpdateApp(App app)\n+ void DeleteApp(Guid id)\n"
        "+ void DownloadApp(Guid id)\n+ void UninstallApp(Guid id)\n"
        "+ void RestoreDefaults()\n+ User? GetUserByLogin(string login)\n"
        "+ void UpdateUser(User user)",
        ST_IFACE, 540, 20, 250, 250)

    auth = d.vertex(
        "AuthService\n"
        "- IRepository _repo\n- IPasswordHasher _hasher\n- User? _currentUser\n"
        "+ User? Login(string login, string password)\n"
        "+ void Logout()\n+ bool IsAdmin",
        ST_CLASS, 540, 310, 250, 195)

    hasher = d.vertex("«interface» IPasswordHasher\n+ string HashPassword(string pwd)\n"
                      "+ bool VerifyPassword(string pwd, string hash)",
                      ST_IFACE, 540, 550, 250, 115)

    mainvm = d.vertex(
        "MainViewModel : ViewModelBase\n"
        "- ObservableCollection&lt;App&gt; _apps\n- IRepository _repo\n"
        "- IAuthService _auth\n- IUndoRedoService _undoRedo\n"
        "+ ObservableCollection&lt;App&gt; Apps\n+ App? SelectedApp\n"
        "+ string SearchQuery\n+ bool IsAdmin\n+ bool IsDetailOpen\n"
        "+ ICommand SelectCategoryCommand\n+ ICommand UndoCommand",
        ST_CLASS, 540, 710, 250, 300)

    pgef = d.vertex("PostgreSQLRepEF\n«EF Core 8 + Npgsql»", ST_CLASS, 820, 20, 230, 85)
    pgado = d.vertex("PostgreSQLRepADO\n«Npgsql ADO.NET»", ST_CLASS, 820, 120, 230, 85)
    jsonrep = d.vertex("JsonRep\n«Resources/Buffer/*.json»", ST_CLASS, 820, 220, 230, 85)
    bcrypt = d.vertex("BCryptHasher\n«work factor 12»", ST_CLASS, 820, 330, 230, 85)
    sha = d.vertex("SHA256Hasher\n«legacy fallback»", ST_CLASS, 820, 430, 230, 85)
    relay = d.vertex("RelayCommand : ICommand\n+ void Execute()", ST_CLASS, 820, 530, 230, 85)
    vmbase = d.vertex("ViewModelBase\n«INotifyPropertyChanged»", ST_CLASS, 820, 630, 230, 85)
    theme = d.vertex("ThemeService\n«Singleton»\n+ bool IsDarkMode", ST_CLASS, 820, 730, 230, 85)
    loc = d.vertex("LocalizationService\n«Singleton»\n+ string Language", ST_CLASS, 820, 830, 230, 85)

    d.edge(iapp, app, ST_DEP, "«implements»")
    d.edge(iuser, user, ST_DEP, "«implements»")
    d.edge(irepo, pgef, ST_DEP, "«implements»")
    d.edge(irepo, pgado, ST_DEP, "«implements»")
    d.edge(irepo, jsonrep, ST_DEP, "«implements»")
    d.edge(auth, irepo, ST_DEP, "использует")
    d.edge(auth, hasher, ST_DEP)
    d.edge(hasher, bcrypt, ST_DEP, "«implements»")
    d.edge(hasher, sha, ST_DEP, "«implements»")
    d.edge(mainvm, irepo, ST_DEP)
    d.edge(mainvm, auth, ST_DEP)
    d.edge(mainvm, undo, ST_DEP)
    d.edge(undo, action, ST_DEP)
    d.edge(mainvm, vmbase, ST_DEP, "«наследует»")
    d.edge(relay, vmbase, ST_DEP)
    d.edge(user, uapp, ST_DEP)
    d.edge(app, uapp, ST_DEP)
    d.edge(mainvm, theme, ST_DEP)
    d.edge(mainvm, loc, ST_DEP)
    return d


# ══════════════════════════════════════════════════════════════════════════
# 3. SEQUENCE DIAGRAM (SD-01: запуск и вход)
# ══════════════════════════════════════════════════════════════════════════
def build_sequence():
    d = Dia("SequenceDiagram", 1000, 800)

    d.vertex("SD-01: Запуск приложения и вход пользователя", ST_NOTE,
             20, 15, 420, 40)

    lifelines = [
        ("Пользователь", 90),
        ("ApplicationApp", 250),
        ("LoginWindow", 420),
        ("AuthService", 590),
        ("IRepository", 760),
        ("MainWindow / MainVM", 920),
    ]
    heads = {}
    for name, x in lifelines:
        heads[name] = d.vertex(name, ST_LIFE, x - 70, 80, 140, 38)
        # линия жизни — ребро без источника/цели, с явной геометрией-линией
        d.cells.append(
            f'        <mxCell id="life{len(d.cells)}" style="endArrow=none;html=1;'
            f'strokeColor=#6c8ebf;dashed=0;" edge="1" parent="1">\n'
            f'          <mxGeometry relative="1" as="geometry">\n'
            f'            <mxPoint x="{x}" y="118" as="sourcePoint" />\n'
            f'            <mxPoint x="{x}" y="700" as="targetPoint" />\n'
            f'          </mxGeometry>\n'
            f'        </mxCell>')

    def m(src_name, dst_name, label, y):
        sx = [x for n, x in lifelines if n == src_name][0]
        dx = [x for n, x in lifelines if n == dst_name][0]
        if src_name == dst_name:
            # самосообщение: короткая петля справа от lifeline
            d.cells.append(
                f'        <mxCell id="msg{len(d.cells)}" value="{label}" style="{ST_MSG}" '
                f'edge="1" parent="1">\n'
                f'          <mxGeometry relative="1" as="geometry">\n'
                f'            <mxPoint x="{sx}" y="{y}" as="sourcePoint" />\n'
                f'            <mxPoint x="{sx + 45}" y="{y + 25}" as="targetPoint" />\n'
                f'            <Array as="points">\n'
                f'              <mxPoint x="{sx + 45}" y="{y}" />\n'
                f'            </Array>\n'
                f'          </mxGeometry>\n'
                f'        </mxCell>')
            return
        d.cells.append(
            f'        <mxCell id="msg{len(d.cells)}" value="{label}" style="{ST_MSG}" '
            f'edge="1" parent="1">\n'
            f'          <mxGeometry relative="1" as="geometry">\n'
            f'            <mxPoint x="{sx}" y="{y}" as="sourcePoint" />\n'
            f'            <mxPoint x="{dx}" y="{y}" as="targetPoint" />\n'
            f'          </mxGeometry>\n'
            f'        </mxCell>')

    m("Пользователь", "ApplicationApp", "Запуск приложения", 170)
    m("ApplicationApp", "ApplicationApp", "CompositionRoot.Build()", 205)
    m("ApplicationApp", "AuthService", "new AuthService(repo)", 240)
    m("ApplicationApp", "LoginWindow", "new LoginWindow(auth)", 275)
    m("ApplicationApp", "LoginWindow", "ShowDialog()", 310)
    m("Пользователь", "LoginWindow", "Логин и пароль", 350)
    m("LoginWindow", "AuthService", "Login(login, password)", 385)
    m("AuthService", "AuthService", "BCrypt.Verify(hash, password)", 420)
    m("AuthService", "IRepository", "GetUserByLogin(login)", 455)
    m("IRepository", "AuthService", "User / null", 490)
    m("AuthService", "IRepository", "SetCurrentUser(user.Id)", 525)
    m("AuthService", "LoginWindow", "User / null", 560)
    m("LoginWindow", "ApplicationApp", "DialogResult = true", 595)
    m("ApplicationApp", "MainWindow / MainVM", "OpenCatalog()", 630)
    m("MainWindow / MainVM", "IRepository", "GetAllApps()", 665)
    m("IRepository", "MainWindow / MainVM", "List<App>", 705)

    d.vertex("alt Вход выполнен: открывается каталог", ST_NOTE, 20, 745, 300, 28)
    d.vertex("else Отмена: Shutdown()", ST_NOTE, 20, 785, 220, 28)
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
        "PK id uuid&#10;FK user_id uuid&#10;FK app_id uuid&#10;  installed_at date&#10;"
        "  is_purchased bool&#10;  purchase_price double",
        320, 330, 240, 160, ST_TABLE)

    d.edge(users, ua, ST_ER, "имеет")
    d.edge(apps, ua, ST_ER, "установлено")

    d.vertex("Связь «многие ко многим» реализована через промежуточную таблицу "
             "users_apps: ON DELETE CASCADE с обеих сторон.\n"
             "Роли: role = 0 (User), role = 1 (Admin).",
             ST_NOTE, 600, 330, 280, 100)
    return d


BUILDERS = [
    ("use_case.drawio", build_use_case),
    ("class_diagram.drawio", build_class),
    ("sequence_diagram.drawio", build_sequence),
    ("erd.drawio", build_erd),
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

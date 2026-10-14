# Sequence Diagrams — AppStore

---

## SD-01: Application Startup & Login Flow

```mermaid
sequenceDiagram
    actor User
    participant App as ApplicationApp
    participant Login as LoginWindow
    participant Auth as AuthService
    participant Repo as IRepository
    participant Main as MainWindow / MainVM
    participant Theme as ThemeService
    participant Loc as LocalizationService

    User->>App: Launch application
    App->>Theme: new ThemeService()
    App->>App: InitializeComponent()
    App->>App: Register global cursor handler
    App->>App: ShowLogin()

    App->>Repo: new PostgreSQLRepEF() / JsonRep()
    App->>Auth: new AuthService(repo)
    App->>Login: new LoginWindow(auth)
    Login->>Auth: (binds to AuthService)
    App->>Login: ShowDialog()

    alt User enters credentials
        User->>Login: Login + Password
        Login->>Auth: Login(login, pass)
        Auth->>Auth: hash = HashPassword(pass)
        Auth->>Repo: GetUserByLogin(login)
        Repo-->>Auth: User / null
        Auth->>Auth: Verify hash == user.PasswordHash
        Auth->>Repo: SetCurrentUser(user.Id) (if EF/ADO/JSON)
        Auth-->>Login: User / null
        alt Success
            Login-->>App: DialogResult = true
            App->>App: OpenCatalog(repo, auth)
        else Failure
            Login-->>User: Show error "Invalid credentials"
        end
    else User closes window
        Login-->>App: DialogResult = false
        App->>App: Shutdown()
    end
```

---

## SD-02: Open Catalog (Post-Login)

```mermaid
sequenceDiagram
    participant App as ApplicationApp
    participant Repo as IRepository
    participant Auth as AuthService
    participant VM as MainViewModel
    participant Main as MainWindow
    participant Theme as ThemeService
    participant Loc as LocalizationService

    App->>VM: new MainViewModel(repo, loc, auth, theme)
    VM->>VM: _undoRedo = new UndoRedoService()
    VM->>Repo: GetAllApps() → loads all, sets IsDownloaded per user
    VM->>Repo: GetAllApps() → filters featured → FeaturedApps
    VM->>Repo: GetAllApps() → extracts distinct tags → AllTags
    VM->>VM: BuildQuery() → applies filters/sort → Apps (first 6)
    VM->>VM: OnPropertyChanged for all collections
    App->>Main: new MainWindow { DataContext = VM }
    Main->>VM: (XAML bindings resolve)
    App->>Main: Show()
    App->>Main: Subscribe to Closed + LogoutRequested
```

---

## SD-03: Client Installs App with Undo/Redo

```mermaid
sequenceDiagram
    actor User
    participant VM as MainViewModel
    participant Repo as IRepository
    participant URS as UndoRedoService
    participant App as App (model)

    User->>VM: Click "Install" on AppRow (DownloadCommand)
    VM->>VM: DownloadCommand.Execute(app)
    VM->>Repo: DownloadApp(app.Id)
    Repo->>Repo: Insert into users_apps (user_id, app_id)
    Repo->>Repo: app.DownloadCount++
    Repo-->>VM: (void)
    VM->>URS: Push(GetAction(undo: Uninstall, redo: Download))
    VM->>VM: RefreshAfterDownload(app.Id)
    VM->>VM: Reload() → GetAllApps(), GetFeatured(), BuildQuery()
    VM->>VM: OnPropertyChanged(Apps, DownloadedCount, etc.)
    VM-->>User: UI updates: button shows "Open", count increments

    alt User clicks Undo (within timeout / before other action)
        User->>VM: Click Undo button (UndoCommand)
        VM->>URS: Undo()
        URS->>URS: Pop last action, execute Undo delegate
        URS->>Repo: UninstallApp(app.Id)
        URS->>Repo: app.DownloadCount--
        URS-->>VM: (void)
        VM->>VM: Reload()
        VM-->>User: UI reverts: button shows "Install", count decrements
    end

    alt User clicks Redo
        User->>VM: Click Redo button (RedoCommand)
        VM->>URS: Redo()
        URS->>URS: Pop from redo stack, execute Redo delegate
        URS->>Repo: DownloadApp(app.Id)
        URS->>Repo: app.DownloadCount++
        URS-->>VM: (void)
        VM->>VM: Reload()
        VM-->>User: UI reapplies: button shows "Open"
    end
```

---

## SD-04: Admin Adds New App

```mermaid
sequenceDiagram
    actor Admin
    participant VM as MainViewModel
    participant AddEditVM as AddEditAppViewModel
    participant Dialog as AddEditWindow
    participant Repo as IRepository

    Admin->>VM: Click "Add App" button (admin only visible)
    VM->>AddEditVM: new AddEditAppViewModel(repo, null)
    VM->>Dialog: new AddEditWindow { DataContext = AddEditVM }
    Dialog->>Admin: ShowDialog()

    Admin->>Dialog: Fill all fields (validates on blur)
    Admin->>Dialog: Click "Save"
    Dialog->>AddEditVM: SaveCommand.Execute()
    AddEditVM->>AddEditVM: Validate() → all required, price≥0, etc.
    AddEditVM->>Repo: AddApp(newApp)
    Repo->>Repo: INSERT into apps (all fields)
    Repo-->>AddEditVM: (void)
    AddEditVM-->>Dialog: DialogResult = true
    Dialog-->>VM: DialogResult = true
    VM->>VM: Reload() → refresh Apps, FeaturedApps, AllTags
    VM-->>Admin: New app appears in catalog
```

---

## SD-05: Language / Theme Switch

```mermaid
sequenceDiagram
    actor User
    participant VM as MainViewModel
    participant Loc as LocalizationService
    participant Theme as ThemeService
    participant App as Application (WPF)

    alt Language Switch
        User->>VM: Click RU/EN menu item (SwitchLanguageCommand)
        VM->>Loc: SetLanguage("en" / "ru")
        Loc->>Loc: Merge ResourceDictionary (en.xaml / ru.xaml)
        Loc-->>VM: CurrentLanguage updated
        VM->>VM: Reload() → rebuild collections (currency, labels)
        VM-->>User: UI text updates instantly
    else Theme Switch
        User->>VM: Click Light/Dark toggle
        VM->>Theme: ToggleTheme() / SetTheme(bool)
        Theme->>Theme: _isDark = new value
        Theme->>App: Replace ResourceDictionary (LightAppTheme / DarkAppTheme)
        Theme-->>VM: IsDark updated
        VM-->>User: All colors update instantly
    end
```

---

## SD-06: Logout & Return to Login

```mermaid
sequenceDiagram
    actor User
    participant VM as MainViewModel
    participant URS as UndoRedoService
    participant Auth as AuthService
    participant App as ApplicationApp
    participant Main as MainWindow

    User->>VM: Click "Logout" in profile menu (LogoutCommand)
    VM->>URS: Clear() → discard undo/redo history
    VM->>Auth: Logout()
    Auth->>Auth: _currentUser = null
    Auth->>Repo: SetCurrentUser(null) (if EF/ADO/JSON)
    VM->>VM: LogoutRequested?.Invoke(this, EventArgs.Empty)
    App->>App: (handles LogoutRequested)
    App->>Main: Close() → isLogout = true
    App->>App: ShowLogin() → new LoginWindow, new AuthService
    App-->>User: Login screen appears
```
# Use Case Diagram — AppStore

```mermaid
useCaseDiagram
    left to right direction
    actor Admin
    actor Client
    actor System

    package "Authentication" {
        usecase "UC-01: Login" as UC01
        usecase "UC-02: Logout" as UC02
    }

    package "Catalog (Shared)" {
        usecase "UC-03: View Catalog" as UC03
        usecase "UC-04: Search Apps" as UC04
        usecase "UC-05: Filter Apps" as UC05
        usecase "UC-06: Sort Apps" as UC06
        usecase "UC-07: View App Detail" as UC07
        usecase "UC-08: Install/Uninstall App" as UC08
        usecase "UC-09: Undo/Redo Last Action" as UC09
    }

    package "Profile (Shared)" {
        usecase "UC-10: View Profile" as UC10
        usecase "UC-11: Edit Profile (Name, Email)" as UC11
        usecase "UC-12: Change Password" as UC12
        usecase "UC-13: Switch Language (RU/EN)" as UC13
        usecase "UC-14: Switch Theme (Light/Dark)" as UC14
    }

    package "Admin Only" {
        usecase "UC-15: Add New App" as UC15
        usecase "UC-16: Edit Any App" as UC16
        usecase "UC-17: Delete Any App" as UC17
        usecase "UC-18: Restore Default Catalog" as UC18
    }

    Admin --> UC01
    Admin --> UC02
    Admin --> UC03
    Admin --> UC04
    Admin --> UC05
    Admin --> UC06
    Admin --> UC07
    Admin --> UC08
    Admin --> UC09
    Admin --> UC10
    Admin --> UC11
    Admin --> UC12
    Admin --> UC13
    Admin --> UC14
    Admin --> UC15
    Admin --> UC16
    Admin --> UC17
    Admin --> UC18

    Client --> UC01
    Client --> UC02
    Client --> UC03
    Client --> UC04
    Client --> UC05
    Client --> UC06
    Client --> UC07
    Client --> UC08
    Client --> UC09
    Client --> UC10
    Client --> UC11
    Client --> UC12
    Client --> UC13
    Client --> UC14

    UC01 ..> UC02 : <<include>>
    UC08 ..> UC09 : <<extend>>
    UC15 ..> UC03 : <<extend>>
    UC16 ..> UC03 : <<extend>>
    UC17 ..> UC03 : <<extend>>
```
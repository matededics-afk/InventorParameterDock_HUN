# Mumur – Inventor Paraméterpanel (MVP)

## Funkciók
- Dokkolható panel az Inventor jobb oldalán.
- Aktív IPT User Parameters megjelenítése, beleértve a kifejezéseket.
- Kifejezések szerkesztése a középső oszlopban (Enter / cella elhagyása).
- Új hosszparaméter (mm) létrehozása névvel és kifejezéssel.
- Frissítés dokumentumváltáskor és kézi Frissítés gombbal.

## Fordítás
1. Inventor 2024 célverzió. Windows alatt Visual Studio 2022, .NET desktop development workload és .NET Framework 4.8 targeting pack szükséges.
2. Nyisd meg az `InventorParameterDock.csproj` fájlt.
3. A projektnek szüksége van az Inventor telepítésében található `Autodesk.Inventor.Interop.dll` fájlra. A projekt alapból a `C:\Program Files\Autodesk\Inventor 2024\Bin\Autodesk.Inventor.Interop.dll` útvonalon keresi. Ha ott nincs, keresd meg a DLL-t az Inventor telepítésében, és módosítsd a HintPath értéket, vagy add meg az `InventorInteropPath` MSBuild property-t.
4. Build, x64. A DLL buildje nincs elvégezve és Inventor alatt nincs tesztelve.
5. Másold a lefordított DLL-t egy állandó mappába, például `%APPDATA%\Autodesk\Inventor Addins\MumurParameterDock\`. A `.addin` manifestet az Inventor által figyelt `%APPDATA%\Autodesk\Inventor Addins\` mappába tedd (ne almappába).
6. A manifest `Assembly` sorában a DLL TELJES, abszolút útvonalát add meg, például `C:\Users\FELHASZNÁLÓ\AppData\Roaming\Autodesk\Inventor Addins\MumurParameterDock\InventorParameterDock.dll`. A `%APPDATA%` változót itt ne hagyd feloldatlanul.
7. Indítsd újra az Inventor 2024-et; Tools > Add-Ins alatt ellenőrizd a betöltést. Tesztelj egy MÁSOLAT `.ipt` fájlon, amelyben `a = 18 mm` User Parameter szerepel.

## Korlátok
- Első verziós forrás: fordítása és API-tesztje szükséges az adott Inventor-verzióval.
- A panel csak IPT-fájlok user paramétereit kezeli, nem minden model- vagy reference-paramétert.
- Az új paraméterek egysége egyelőre mindig mm. A meglévő paraméterek egységét kijelzi.
- Más UI-ból történő paraméterváltozásnál a kézi Frissítés szükséges lehet.
- Nem módosítja az IPT sablont, így az `a = 18 mm` alapparamétert külön tedd bele a sablonba.
- Az assembly in-place szerkesztés eseménykezelését a célverzióban ellenőrizni kell.

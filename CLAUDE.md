# CLAUDE.md — Osiedle '77: Magia i Sztacheta

Ten plik czytasz na początku każdej sesji. Opisuje, co budujemy i jak.

## Projekt

- Gra: 3D roguelike twin-stick (widok z góry), PRL 1977, osiedle z wielkiej płyty. Tylko jeden gracz, tylko język polski.
- Silnik: **Unity 6 LTS, URP**. Pakiety: Input System, Cinemachine, ProBuilder, AI Navigation, Test Framework, TextMeshPro.
- Pełny opis gry: `Docs/GDD.md`. Przed każdym etapem czytaj sekcje, które wskazuje prompt.
- Postęp prac: `Docs/PROGRESS.md`. Czytaj na początku sesji, aktualizuj na jej końcu.
- Autor projektu jest początkującym programistą. Tłumacz krótko i prosto, po polsku, co zrobiłeś i co ma kliknąć w Unity.

## Sposób pracy

1. Zanim napiszesz kod, pokaż krótki plan (pliki do utworzenia i zmiany) i poczekaj na akceptację.
2. Jeden etap = jedna gałąź git. Nie ruszaj systemów spoza zakresu etapu, chyba że inaczej się nie da (wtedy powiedz dlaczego).
3. Nie edytuj ręcznie plików `.unity`, `.prefab`, `.asset` ani `.meta`. Sceny, prefaby i pliki danych twórz **skryptami edytora** w `Scripts/Editor/`, z pozycją menu `Osiedle/Build/...`. Budowniczy musi dać się uruchomić ponownie i odtworzyć wynik od zera.
4. Po skończeniu wypisz: co zrobiłeś, co kliknąć w Unity, listę rzeczy do przetestowania i które pola danych stroić.
5. Na koniec etapu dopisz wpis do `Docs/PROGRESS.md`: data, etap, co działa, znane błędy, co odłożone.
6. Gdy dostajesz błąd z konsoli Unity, naprawiaj tylko jego przyczynę. Nie przepisuj przy okazji innych rzeczy.
7. Jeśli zadanie jest za duże na jedną sesję, podziel je na części i zrób pierwszą.

## Struktura

```
Assets/_Osiedle/
  Scripts/
    Runtime/   (asmdef Osiedle.Runtime)
      Core/ Player/ Combat/ Weapons/ Enemies/ Bosses/ Upgrades/ Rooms/ Meta/ UI/ Audio/
    Editor/    (asmdef Osiedle.Editor, tylko edytor)
    Tests/     (asmdef Osiedle.Tests)
  Data/        ScriptableObjecty: Player, Weapons, Enemies, Upgrades, Suppliers, Acts, Rooms, Jars, Texts_PL;
               Input (mapa klawiszy OsiedleControls)
  Prefabs/     Player, Enemies, Bosses, Projectiles, Pickups, Rooms, UI
  Scenes/      Boot, Hub, Run, Test_*
  Art/  Audio/
Docs/          GDD.md, PROGRESS.md, CREDITS.md
```

## Zasady kodu

- C#, namespace `Osiedle.<Folder>`, jedna klasa na plik, nazwy w kodzie po angielsku, komentarze i teksty gry po polsku.
- **Żadnych magicznych liczb.** Każda wartość do balansu siedzi w ScriptableObjecie (`PlayerData`, `MeleeWeaponData`, `RangedWeaponData`, `SpellData`, `EnemyData`, `BossData`, `UpgradeData`, `RoomData`, `ActData`).
- Komunikacja między systemami przez statyczne zdarzenia w `GameEvents` (`OnHit`, `OnKill`, `OnDash`, `OnPerfectDash`, `OnDamageTaken`, `OnRoomCleared` i kolejne). Ulepszenia, UI i dźwięk tylko się podpinają; zawsze odpinaj w `OnDisable`.
- Obrażenia: `Hitbox` zadaje, `Hurtbox` przyjmuje, `Health` liczy, `DamageInfo` niesie liczbę, żywioł, odrzut, źródło, krytyk. Reakcje żywiołów tylko w `ElementReactions`.
- Singletony tylko: `RunManager`, `AudioManager`, `SaveSystem`. Resztę podpinaj przez Inspector (`[SerializeField]`) albo `GetComponent`.
- Pociski i efekty przez `UnityEngine.Pool.ObjectPool`. Żadnego `Instantiate`/`Destroy` w pętli walki.
- Wrogowie: maszyna stanów w C# (`EnemyBrain`), `NavMeshAgent`, `AttackTokenManager` (maks. 3 atakujących naraz), każdy atak ma `Telegraph` min. 0,4 s.
- Bossowie: `BossController` + fazy jako dane (`BossPhase`) + ataki jako osobne klasy (`BossAttackPattern`).
- Ulepszenia: plik `UpgradeData` + mała klasa `UpgradeEffect`. Nigdy wielki `switch` po nazwach ulepszeń.
- Ruch: `CharacterController`. Celowanie: promień z kursora na płaszczyznę na wysokości biodra. Dash kontekstowy: obiekt z `Vaultable` przed graczem zamienia dash w skok.
- UI: uGUI + TextMeshPro. Teksty z `Data/Texts_PL`.
- Zapis: JSON w `Application.persistentDataPath`, z numerem wersji i kopią zapasową.
- Logika bez zależności od sceny (obrażenia, reakcje, kumulacja ulepszeń, zapis) ma testy w `Tests/`.
- Logi debug z prefiksem `[Osiedle]`.

## Czytelność gry (nie łam tych zasad)

- Czerwień zarezerwowana dla telegrafów ataków wroga i dla finałowego bossa.
- Kolory magii (fiolet, zieleń kwasu, pomarańcz ognia, błękit prądu) nigdy na otoczeniu.
- Żaden atak wroga nie trafia bez ostrzeżenia.
- Do etapu M10 grafika to szare bryły. Nie dodawaj modeli ani efektów „na zapas”.
- Wzorzec wyglądu: Docs/Concept/wzorzec_walka.png. Grafikę robimy dopiero od M10, do tego czasu tylko szare bryły.

## Nazwy w grze

Tylko szydercze zamienniki marek z GDD (Dolarex, Benzopol, Kiosk Bezruchu, Kolorowy Sen, Unitrąba, Kurdupel 126z, Abibas, Klejopren, Wichry 3, Guma Kaczor, Bazar Szemrany, Klub Beton). Osiedle nazywa się Osiedle Świetlana Przyszłość. Nigdy prawdziwe marki.

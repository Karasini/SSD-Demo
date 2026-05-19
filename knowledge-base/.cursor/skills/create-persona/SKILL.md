---
name: create-persona
description: Create and update detailed user personas for Responia using the standard persona template. Use when the user wants to define, refine, or compare personas based on the template in templates/persona-template.md.
---

# Tworzenie person dla Responia

## Kiedy używać

Używaj tego skilla, gdy:

- użytkownik prosi o stworzenie nowej persony (np. architekt, prawnik, właściciel firmy transportowej),
- trzeba ujednolicić istniejące persony z aktualnym template,
- trzeba zrefaktorować lub doprecyzować personę (dopisać cele, bóle, zachowania),
- użytkownik wspomina o `persona`, `AI-persona`, `persona-template.md` lub prosi o „dopasowanie persony do Responia”.

## Źródło prawdy

- Jedynym wzorcem struktury jest plik `.cursor/skills/create-persona/assets/persona-template.md`.

## Format wyjścia

Zawsze generuj pełny dokument w formacie markdown zgodnym z template:

- zachowaj nagłówki z template (`## Basic Information`, `## Demographics`, itp.),
- uzupełnij wszystkie sekcje sensowną treścią (nie zostawiaj placeholderów typu `[Goal 1]`),
- pisz po polsku (chyba że użytkownik wyraźnie zażąda innego języka),
- styl: zrozumiały dla biznesu, konkretny, bez „marketingowego bełkotu”.

## Kroki tworzenia persony

1. **Ustal kontekst persony**
   - Z wyjaśnień użytkownika wyciągnij: branżę, rolę, wielkość firmy, typ pracy, kanały komunikacji.
   - Jeśli czegoś brakuje, **nie zatrzymuj się** – rozsądnie załóż typowe realia dla tej branży w Polsce.

2. **Wypełnij sekcję „Basic Information”**
   - `Name`: imię + branża/rola (np. „Michał – prawnik procesowy”, „Jan – architekt wnętrz”).
   - `Age`: realny zakres (np. `35–45`).
   - `Occupation`: konkretna rola (np. `Właściciel małej firmy transportowej`).

3. **Demographics**
   - Opisuj **prawdopodobny** stan: rodzina, liczba zależnych osób, poziom komfortu z technologią.
   - Unikaj stereotypów; bazuj na realnych wzorcach z reszty materiałów w repo.

4. **Psychographics**
   - Personality Traits: 3–5 cech, które wyjaśniają, jak ta osoba pracuje i podejmuje decyzje.
   - Communication Style: jak pisze maile, jak rozmawia z klientami/podwykonawcami.
   - Decision-Making Style: czy bazuje na danych, intuicji, konsultuje z zespołem, itp.

5. **Values & Motivations**
   - Core Values: co jest „nienegocjowalne” (np. terminowość, jakość, bezpieczeństwo, reputacja).
   - Primary Goals: co chce osiągnąć w pracy/firmie (konkretne, mierzalne tam, gdzie ma to sens).
   - Fears & Concerns: realne obawy (np. utrata kluczowego klienta, błędy w umowach, chaos w komunikacji).
   - Aspirations: „co by chciał, żeby było możliwe” za 2–3 lata.

6. **Behavioral Patterns**
   - Technology Usage: jakie urządzenia, w jakich kontekstach (biuro, w drodze, w domu).
   - Apps & Platforms: Gmail/Outlook, WhatsApp, Teams, inne systemy branżowe.
   - Online Behavior: kiedy siada do maili, jak często zagląda do skrzynki, czy lubi mobilne aplikacje.

7. **Goals & Pain Points**
   - Primary Goals:
     - Użyj 3–5 punktów.
     - Każdy w formacie: **[Krótki tytuł]** – 1–2 zdania doprecyzowania.
   - Pain Points & Frustrations:
     - Zmapuj na realne problemy z Discovery (np. `res-cj-transport.md`, `Business value proposition`),
     - Opisz wpływ (czas, stres, ryzyko finansowe) i częstotliwość.

8. **Current Solutions**
   - Opisz, czego używa dzisiaj (poczta, Excel, notatnik, WhatsApp, CRM),
   - Wskaż, co działa (np. „ma poczucie kontroli nad jednym klientem”) i co **nie działa** (brak skalowalności, chaos, duplikacja pracy).

9. **Needs & Expectations**
   - Functional Needs: co produkt ma robić, żeby rozwiązać bóle.
   - Emotional Needs: jak persona chce się czuć (spokój, kontrola, bezpieczeństwo, brak wstydu przed klientem).
   - Social Needs: jak wpływa to na relacje z klientami/podwykonawcami/zespołem.

10. **Barriers & Constraints**
    - Czas: kiedy realnie może się uczyć nowego narzędzia, ile ma „głowy” na zmiany.
    - Budżet: poziom akceptowalnego abonamentu i awersja do ryzyka.
    - Techniczne: sprzęt, łącze, ograniczenia bezpieczeństwa/RODO.
    - Środowiskowe: praca w terenie, w samochodzie, między spotkaniami.

11. **Quotes & Voice**
    - Przygotuj 2–3 cytaty w pierwszej osobie, które brzmią jak prawdziwe zdania tej osoby.
    - Cytaty mają:
      - pokazywać **emocje** (frustracja, ulga, niepewność),
      - odnosić się do konkretnych sytuacji (np. opisanie dnia, problemu z mailem, stresu przed klientem).
    - Dopasuj: tone, vocabulary, preferred channels do reszty opisu.

## Dobre praktyki

- **Spójność z Responia:** zawsze myśl, jak dana persona korzysta z maila i zleceń – to trzon produktu.
- **Konkret > ogólniki:** unikaj pustych fraz typu „chce rozwijać firmę”; doprecyzuj „chce zwiększyć liczbę zleceń o 20% bez wydłużania czasu pracy”.
- **Realizm:** persona ma być wiarygodna dla polskiego MŚP, nie „idealnym klientem z podręcznika marketingu”.
- **Brak sprzeczności:** upewnij się, że cele, bóle, zachowania i cytaty nie kłócą się ze sobą.

## Jak odpowiadać użytkownikowi

1. Jeśli użytkownik prosi „stwórz personę X”:
   - zaproponuj nazwę i krótki opis roli,
   - wygeneruj pełny dokument markdown zgodny z template,
   - na końcu dodaj 2–3 zdania podsumowania, jak ta persona łączy się z Responia.

2. Jeśli użytkownik prosi o doprecyzowanie istniejącej persony:
   - najpierw streść, jakie są już mocne elementy,
   - dopisz brakujące sekcje lub popraw niespójności,
   - zachowaj istniejące unikalne szczegóły (nie „wygładzaj” charakteru persony bez potrzeby).

3. Jeśli użytkownik chce porównać persony:
   - wskaż 3–5 kluczowych różnic: cele, bóle, sposób pracy, stosunek do technologii,
   - zasugeruj, które ficzery Responia są krytyczne dla której persony.


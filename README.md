<!--
Open Wheel Racing Manager — README (Top-tier / Senior)
Theme: Red + Black + Yellow (motorsport / high-contrast)
Author: Deangelo Marques (GitHub: danmarques127-sys)
-->

<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=waving&color=0:0B0B0B,30:111111,65:FF1E00,100:0B0B0B&height=270&section=header&text=Open%20Wheel%20Racing%20Manager&fontSize=52&fontColor=FFFFFF&animation=fadeIn&fontAlignY=38&desc=Simulation-first%20manager%20engine%20%E2%80%A2%20Data-driven%20systems%20%E2%80%A2%20Architecture%20before%20UI%20%E2%80%A2%20Unity%20(C%23)&descAlignY=66&descSize=18" />
</p>

<p align="center">
  <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&size=20&pause=900&color=FF1E00&center=true&vCenter=true&width=1050&lines=Simulation-first+open-wheel+racing+manager+engine;Deterministic%2C+data-driven+systems+with+long-term+extensibility;Architecture+before+UI+%E2%80%94+by+design%2C+to+avoid+coupling;Built+to+evolve+across+seasons%2C+regulations%2C+and+series;No+licensed+assets.+No+official+trademarks.+Original+simulation+project." />
</p>

<p align="center">
  <a href="https://github.com/danmarques127-sys">
    <img src="https://img.shields.io/badge/Author-Deangelo%20Marques-0B0B0B?style=for-the-badge&logo=github&logoColor=F5C400" />
  </a>
  <img src="https://img.shields.io/badge/Engine-Unity-111111?style=for-the-badge&logo=unity&logoColor=FFFFFF" />
  <img src="https://img.shields.io/badge/Language-C%23-111111?style=for-the-badge&logo=csharp&logoColor=FFFFFF" />
  <img src="https://img.shields.io/badge/Theme-Red%20%2B%20Black%20%2B%20Yellow-FF1E00?style=for-the-badge&labelColor=0B0B0B&color=FF1E00" />
</p>

<p align="center">
  <img src="https://img.shields.io/badge/STATUS-ARCHITECTURE%20PHASE-FF1E00?style=for-the-badge&labelColor=0B0B0B" />
  <img src="https://img.shields.io/badge/FOCUS-SIMULATION-0B0B0B?style=for-the-badge&labelColor=111111&color=0B0B0B" />
  <img src="https://img.shields.io/badge/UI-DEFERRED-F5C400?style=for-the-badge&labelColor=0B0B0B&color=F5C400" />
  <img src="https://img.shields.io/badge/LICENSE-MIT-22C55E?style=for-the-badge&labelColor=0B0B0B&color=22C55E" />
</p>

<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=rect&color=0:0B0B0B,50:FF1E00,100:0B0B0B&height=2&section=header" />
</p>

<br/>

## 🧠 Executive Summary

**Open Wheel Racing Manager** is a **simulation-first racing management engine** built in **Unity (C#)**.
This repository focuses on a **clean, deterministic, data-driven simulation layer** — the backbone of a long-term manager game.

**No UI, visuals, audio, or player-facing presentation are included at this stage — by design.**  
The objective is to establish a stable, extensible simulation foundation before introducing any presentation concerns.

<br/>

## ✅ What This Project Is (and Is Not)

<table>
  <tr>
    <td valign="top" width="50%">

### ✅ This project **is**
- A **simulation backbone** for a racing manager game
- A **data-driven systems** architecture
- A foundation for **multi-season evolution**
- A reference for **manager / strategy simulation patterns**
- Built with **explicit orchestration + minimal hidden coupling**

    </td>
    <td valign="top" width="50%">

### ❌ This project **is not**
- A UI prototype or “vertical slice”
- A visual demo meant to impress via graphics

    </td>
  </tr>
</table>

<br/>

## 🧩 Design Principles

> **In management games, systems are the product.**

This engine is designed around:

- **Simulation-first** — gameplay emerges from rules, not UI
- **Data-driven behavior** — minimal hard-coded logic
- **Deterministic where possible** — reproducible outcomes for testing
- **Presentation-agnostic** — UI consumes simulation; it never controls it
- **Explicit orchestration** — clear entry points and ownership boundaries
- **Extensible rulesets** — regulations and formats evolve without rewrites

<br/>

## 🚧 Status

**Current Phase:** **Core Simulation & Architecture**

### Current Focus
- Season lifecycle modeling (calendar → weekends → results → standings)
- Deterministic race weekend resolution (format-driven)
- Points / standings / tie-breakers
- Rule modeling and long-term extensibility
- Strict separation between **data definitions** and **pure logic systems**

### Explicitly Deferred (Intentionally)
- UI / HUD / menus
- Graphics & visuals
- Audio
- Player input and UX loops

Deferring presentation avoids early coupling and long-term technical debt.

<br/>

## 🗺️ Roadmap & Milestones

Development is tracked via **GitHub Issues** and **Milestones**.

### 🔥 Milestone `v0.1` — MVP Simulation Core
Goal: simulate a complete season end-to-end (log-driven).

- Calendar System
- Race Weekend Model
- Results & Points Engine
- Standings System (drivers + teams)
- Season Flow Orchestrator (single simulation entry point)

### ⚙️ Milestone `v0.2` — Depth & Regulation Evolution
- Regulation variants (scoring, weekend formats, constraints)
- Tie-breaker expansions
- Data validation and consistency checks
- Expanded event types (future series formats)

### 🧱 Milestone `v0.3` — Stability & Documentation
- Architecture diagrams
- Refactoring for clarity + boundaries
- Test coverage expansion
- Serialization and persistence foundations

<br/>

## 🧠 Architecture Overview

This project follows a **system-oriented, simulation-first architecture** with a strict separation between:

- **Data** (ScriptableObjects / definitions)
- **Pure logic systems** (no Unity UI dependencies)
- **Orchestration layer** (single entry point for simulation flow)

### Core Concepts
- ScriptableObjects define **rulesets, calendars, drivers, teams, tracks**
- Systems run **deterministic logic** and publish results
- Orchestration advances time and coordinates subsystems

> Detailed notes live in: **Architecture.md**

<br/>

## 🧩 Core Systems (Planned / In Progress)

### 📅 Calendar System
Defines season structure:
- rounds
- event ordering
- weekend types (standard, sprint, future formats)

### 🏁 Race Weekend Model
Defines weekend composition:
- sessions (practice, qualifying, sprint, race)
- order driven by rulesets
- extensible for new formats

### 🧮 Results & Points Engine
Responsible for:
- processing session results
- applying scoring rules
- awarding driver + team points

### 📊 Standings System
Maintains championship state:
- driver standings
- team standings
- tie-breaker logic

### ⏱️ Season Flow Orchestrator
Single entry point:
- advances time
- coordinates calendar, weekends, results, standings
- produces log-driven simulation output (initially)

## 🛠️ Tech Stack

- **Engine:** Unity
- **Language:** C#
- **Design:** system-oriented, data-driven
- **Data Layer:** ScriptableObjects (definitions)
- **Persistence:** planned (JSON-based, engine-agnostic)

## 📁 Project Structure (High-Level)

Assets/
  _Project/
    Runtime/
      Core/
        Calendar/
        Results/
        Rulesets/
        Season/
        Standings/
      Orchestration/
    Data/
      ScriptableObjects/
      Databases/
    Tests/
      EditMode/
      PlayMode/
Packages/
ProjectSettings/

---

## 🧪 What This Repository Demonstrates

- **Simulation-first game architecture**
- **Clear separation of data and logic**
- **Extensible rule modeling** (regulation-ready)
- **Orchestration-driven lifecycle**  
  *(season → events → results → standings)*
- **Transferable architectural patterns** for:
  - Manager games
  - Strategy simulations
  - Data-heavy systems

---

## ⚠️ Legal / Trademark Disclaimer

This is an **original simulation project**.

It is **not affiliated with Formula 1, FIA, racing teams, drivers, or Liberty Media**.  
No official trademarks, logos, or licensed assets are used or intended.

---

## 👤 Author

**Deangelo Marques**  
*Full-Stack Developer • Game Systems & Simulation Architect*

### Focus Areas
- Data-driven systems
- Simulation engines
- Long-term architecture for management games

Licensed under the **MIT License**.  
See the [`LICENSE`](LICENSE) file for details.

---

<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=rect&color=0:FF1E00,50:0B0B0B,100:F5C400&height=3&section=footer" />
</p>

<p align="center">
  <i>Systems first. Speed later.</i>
</p>

<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=waving&color=0:0B0B0B,35:FF1E00,70:F5C400,100:0B0B0B&height=140&section=footer" />
</p>


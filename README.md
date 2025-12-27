<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=rect&color=0:0B0B0B,50:FF3C00,100:0B0B0B&height=260&section=header&text=Open%20Wheel%20Racing%20Manager&fontSize=44&fontColor=ffffff&animation=fadeIn&fontAlignY=38" />
</p>

<p align="center">
  <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&size=22&pause=700&color=FF3C00&center=true&vCenter=true&width=900&lines=Simulation-First+Racing+Manager+Engine;Data-Driven+Systems+%7C+Long-Term+Architecture;Built+Before+UI.+By+Design.;Designed+for+Evolution%2C+Not+Demos." />
</p>

<h3 align="center">
  Simulation Engine • Manager Game Backbone • Unity (C#)
</h3>

<p align="center">
  Architecture before visuals. Systems before UI.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/STATUS-ARCHITECTURE%20PHASE-FF3C00?style=for-the-badge">
  <img src="https://img.shields.io/badge/FOCUS-SIMULATION-black?style=for-the-badge">
  <img src="https://img.shields.io/badge/UI-DEFERRED-white?style=for-the-badge">
</p>

---

## 🧠 What Is This Project?

**Open Wheel Racing Manager** is a **simulation-first racing management engine**
built in **Unity (C#)**.

This repository contains the **core simulation backbone** of a manager game —
not a UI prototype, not a visual demo.

All presentation layers (UI, graphics, audio) are **intentionally deferred**
until the simulation reaches a stable, extensible foundation.

This project is about **systems, rules, and evolution**.

---

## 🎯 Project Vision

The long-term vision is to build a **modern open-wheel racing manager**
inspired by classic management games, but architected with
**contemporary engineering principles**.

The engine is designed to support:

- Multi-season progression
- Regulation changes
- Long-term team and driver evolution
- Extensible rulesets
- AI-generated narratives and events
- Expansion into multiple racing series

This repository represents the **foundation layer** of that vision.

---

## 🚧 Project Status

**Current Phase:** Simulation Core & Architecture

### Current Focus
- Defining the simulation backbone
- Designing systems *before* UI
- Establishing strict data + logic separation
- Ensuring long-term extensibility

### Explicitly Out of Scope (By Design)
- User Interface
- Graphics / Visuals
- Audio
- Player Input

Skipping these now avoids architectural debt later.

---

## 🧩 Simulation Philosophy

> *A manager game lives or dies by its systems.*

This engine follows these principles:

- **Simulation-first** (logic is the product)
- **Data-driven** over hard-coded behavior
- **Deterministic where possible**
- **Presentation-agnostic**
- **Expandable without rewrites**

UI will eventually **consume** the simulation — never control it.

---

## 🗺️ Roadmap & Milestones

Development is tracked using **GitHub Issues and Milestones**.

### 🔹 Milestone v0.1 — MVP Simulation Core
Goal: a full season can be simulated end-to-end via logs.

Planned systems:
- Calendar System
- Race Weekend Model
- Results & Points Engine
- Standings System
- Season Flow Orchestrator

---

### 🔹 Milestone v0.2 — Simulation Depth
- Regulation variants
- Tie-breaker rules
- Expanded race formats
- Data validation & consistency checks

---

### 🔹 Milestone v0.3 — Stability & Documentation
- Full system documentation
- Architecture diagrams
- Refactoring for clarity
- Test coverage expansion

---

## 🧠 Architecture Overview

This project follows a **system-oriented, simulation-first architecture**.

> Status: In design / iterating

### Core Architectural Principles
- **ScriptableObjects** for data definitions
- **Pure logic systems** with no UI dependency
- **Explicit orchestration layer**
- **Clear ownership of responsibilities**
- **Minimal hidden coupling**

---

## 🧩 Core Systems (Planned & In Progress)

### 📅 Calendar System
Defines the structure of a season:
- Number of rounds
- Event ordering
- Weekend types (standard, sprint, future formats)

Acts as the backbone for all seasonal progression.

---

### 🏁 Race Weekend Model
Defines how a weekend is composed:
- Sessions (practice, qualifying, sprint, race)
- Session order driven by rulesets
- Extensible for new formats

---

### 🧮 Results & Points Engine
Responsible for:
- Processing session results
- Applying scoring rules
- Awarding driver and team points

Fully data-driven and regulation-aware.

---

### 📊 Standings System
Maintains championship state:
- Driver standings
- Team standings
- Tie-breaker logic

Updated incrementally after each scoring event.

---

### ⏱️ Season Flow Controller
The orchestration layer:
- Advances time through the season
- Coordinates calendar, weekends, results, and standings
- Acts as the single entry point for simulation progression

---

## 🛠️ Tech Stack

- **Engine:** Unity
- **Language:** C#
- **Architecture:** System-oriented, data-driven
- **Data Layer:** ScriptableObjects
- **Persistence:** Planned (JSON-based, engine-agnostic)

No UI frameworks. No shortcuts.

---

## 📁 Project Structure (High-Level)

```text
Assets/
 └─ Scripts/
    ├─ Core/
    │  ├─ Calendar/
    │  ├─ Season/
    │  ├─ Standings/
    │  ├─ Results/
    │  └─ Rulesets/
    ├─ Orchestration/
    └─ Tests/

## 🧪 What This Project Demonstrates

- Simulation-first game architecture  
- Clean separation of systems and data  
- Long-term thinking over quick visuals  
- Transferable patterns for:
  - Manager games
  - Strategy simulations
  - Data-heavy systems

---

## ⚠️ Disclaimer

This is an **original simulation project**.

It is **not affiliated with Formula 1, FIA, teams, drivers, or Liberty Media**.  
No official trademarks, logos, or licensed assets are used or intended.

---

## 👤 Author

**Deangelo Marques**  
Full-Stack Developer • Game Systems & Simulation Architect  

**Focus:**
- Data-driven systems  
- Simulation engines  
- Long-term architecture for manager games  

Licensed under the **MIT License**.  
See the [LICENSE](LICENSE) file for details.

<p align="center">
  <i>Systems first. Speed later.</i>
</p>

<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=rect&color=0:FF3C00,100:0B0B0B&height=120&section=footer" />
</p>

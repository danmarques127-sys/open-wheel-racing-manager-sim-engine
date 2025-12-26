# Open Wheel Racing Manager — Simulation Engine

A **simulation-first** racing manager engine built in **Unity (C#)**, focused on
data-driven systems, long-term extensibility, and clean architecture.

This project is designed as a **management and simulation backbone**, not a UI-driven prototype.
All gameplay layers (UI, visuals, presentation) are intentionally deferred until the
core simulation is robust and stable.

---

## 🎯 Project Vision

The goal of this project is to build a **modern open-wheel racing manager simulation engine**
inspired by classic manager games, architected with:

- Clear system boundaries
- Strong data-driven design
- Season-to-season evolution
- Extensibility for future features (rule changes, AI-generated narratives, new series)

This repository represents the **foundation layer** of the game.

---

## 🚧 Project Status

This project is currently in the **architecture and simulation-core phase**.

### Current focus:
- Defining the simulation backbone
- Designing core systems before any UI
- Establishing clean data + logic separation

### Explicitly out of scope (for now):
- User Interface
- Graphics / Visuals
- Audio
- Input systems

This is intentional and by design.

---

## 🗺️ Roadmap

Development is tracked publicly using **GitHub Issues and Milestones**.

### Current Milestone
**MVP Simulation Core (v0.1)**  
Focus: a full season can be simulated via logs.

Planned systems:
- Calendar System
- Race Weekend Model
- Results & Points Engine
- Standings
- Season Flow Orchestrator

### Future Milestones
- **Simulation Depth Expansion (v0.2)**
- **Documentation & Stability (v0.3)**

---

## 🧠 Architecture Overview

This project follows a **simulation-first architecture**, where systems are designed
to function independently of any presentation layer.

> Status: Draft / In progress

### Core Principles
- **Data-driven design** using ScriptableObjects
- **Clear separation** between data, logic, and orchestration
- **Deterministic simulation** where possible
- **No UI dependency** in core systems

---

## 🧩 Main Systems (Planned)

### Calendar System
Defines the season structure:
- Number of rounds
- Order of events
- Weekend types (normal, sprint, future extensions)

Acts as the backbone for all seasonal logic.

---

### Race Weekend Model
Defines how a race weekend is composed:
- Sessions (practice, qualifying, sprint, race)
- Session order driven by rulesets
- Extensible for new formats

---

### Results & Points Engine
Responsible for:
- Processing session results
- Applying rulesets
- Awarding driver and team points

Fully data-driven and regulation-aware.

---

### Standings System
Maintains championship state:
- Driver standings
- Team standings
- Tie-breaker logic (extensible)

Updated incrementally after each scoring session.

---

### Season Flow Controller
The orchestration layer:
- Advances time through the season
- Coordinates calendar, weekends, results, and standings
- Acts as the single entry point for simulation progression

---

## 🛠️ Tech Stack

- **Runtime / Host:** Unity
- **Language:** C#
- **Architecture:** Data-driven, system-oriented
- **Data Layer:** ScriptableObjects
- **Persistence:** Planned (JSON-based, no Unity object references)

---

## 📁 Project Structure (High-Level)

```text
Assets/
 └─ Scripts/
    ├─ Core/
    │  ├─ Calendar/
    │  ├─ Season/
    │  ├─ Standings/
    │  └─ Results/
    └─ Test/
---

## 🧪 What This Project Demonstrates

- Simulation-first system design
- Clean separation of data, logic, and orchestration
- Long-term thinking over short-term visuals
- Transferable architecture patterns for management and simulation games

---

## 📌 Disclaimer

This is an **original prototype** and is **not affiliated with Formula 1, FIA, teams, drivers, or Liberty Media**.  
No official trademarks, logos, or licensed assets are intended for use.

---

## 👤 Author

**Dangelo Marques**  
Full Stack Developer / Game Systems Prototyping  
Focus: data-driven systems, simulation engines, and manager-game architectures.

Licensed under the **MIT License**.  
See the [LICENSE](LICENSE) file for details.


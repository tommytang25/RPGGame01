# Unity RPG Game

A flexible 2D RPG game framework built with Unity, featuring advanced equipment systems, dynamic monster spawning, and procedural content generation.

## Features

### Equipment System
- **Set-based equipment** that provides extra buffs when multiple pieces from the same set are worn
- Different equipment rarities with visual indicators
- Equipment slots system with specialized bonuses
- Visual feedback for equipped items

### Monster System
- **Dynamic monster database** for easy creation and management of enemy types
- Level-based stat scaling and difficulty progression
- Different monster rarities (Common to Boss) with appropriate rewards
- Specialized loot tables per monster type

### Spawning System
- **Area-based spawning** with configurable monster types
- Boss spawn timers and announcements
- Smart respawn queues for efficient enemy management
- Visualization tools for spawn areas in the editor

## Getting Started

### Prerequisites
- Unity 2022.3 or newer
- Basic understanding of Unity and C#

### Installation
1. Clone this repository
2. Open the project in Unity
3. Open the \SampleScene\ in the Scenes folder

## Project Structure

- \Assets/Scripts/EquipmentSystem.cs\ - Core equipment management
- \Assets/Scripts/Item.cs\ - Base class for all items
- \Assets/Scripts/MonsterDatabase.cs\ - Monster type definitions
- \Assets/Scripts/Enemy.cs\ - Individual enemy behaviors
- \Assets/Scripts/SpawnArea.cs\ - Spawn area configuration
- \Assets/Scripts/LootSystem.cs\ - Loot generation and drop mechanics

## License

This project is available for personal and commercial use.

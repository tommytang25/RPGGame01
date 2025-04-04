# Timebound - 2D Action RPG

This is a 2D Action RPG about a modern archaeologist who is transported to a medieval fantasy world through an ancient artifact. The game features real-time combat, exploration, crafting with modern technology, and a narrative about finding a way back home.

## Core Systems

### Player Systems
- **PlayerController.cs**: Handles player movement, combat, and stats
- **TimeAbilityController.cs**: Manages the time-slowing ability unlocked during the game

### Game Management
- **GameManager.cs**: Core singleton for managing game state, progression, and quests
- **GameInitializer.cs**: Sets up the game scene, spawns prefabs, and initializes systems
- **CameraFollow.cs**: Controls camera movement to follow the player

### Inventory & Items
- **InventorySystem.cs**: Manages the player's inventory and item usage
- **CraftingSystem.cs**: Handles recipe unlocking and item crafting, including special modern technology items

### NPC & Enemy Systems
- **NPC.cs**: Controls NPC behavior, dialogue, and shop functionality
- **Enemy.cs**: Handles enemy AI, combat, and stats

### Dialogue & Story
- **DialogueSystem.cs**: Manages conversations with NPCs and choice-based dialogue

### Development Helpers
- **PlaceholderGenerator.cs**: Creates temporary visual elements for testing and development

## Current State

The project is in early development with core systems implemented but using placeholder graphics. The fundamental gameplay loop is functional:
- Player can move and attack
- Enemies can chase and attack the player
- NPCs can have conversations with the player
- Items can be collected and used
- Crafting system is functional
- Time abilities are integrated

## Next Steps

1. Replace placeholder graphics with proper sprites and animations
2. Add sound effects and music
3. Design specific levels and quests
4. Expand the story content
5. Balance gameplay systems
6. Add more unique items, enemies, and NPCs

## Controls

- **WASD**: Move the player
- **Mouse**: Aim
- **Left Mouse Button**: Attack
- **E**: Interact with NPCs and objects
- **I**: Open inventory
- **C**: Open crafting menu
- **F**: Activate time slow ability (once unlocked)
- **ESC**: Pause game

## Development Notes

This project is built in Unity. All code is structured to be modular and extensible for future development. The placeholder graphics system allows for rapid prototyping and testing before final art assets are created. 
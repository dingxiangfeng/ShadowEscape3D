================================================================================
                           SHADOW ESCAPE 3D
                     Unity 3D Game Development Project
================================================================================

PROJECT OVERVIEW
--------------------------------------------------------------------------------
Shadow Escape 3D is a 3D stealth-action game developed in Unity where players 
navigate through mysterious environments while avoiding enemies and collecting 
items to achieve the highest score possible.

GAME CONCEPT
--------------------------------------------------------------------------------
Genre: 3D Stealth-Action Adventure
Target Audience: Casual to mid-core gamers aged 13+
Platform: PC (Windows/Mac/Linux)

CORE FEATURES
--------------------------------------------------------------------------------
1. Third-Person Character Controller
   - Smooth movement with WASD controls
   - Sprint functionality (Left Shift)
   - Jump mechanics (Space)
   - Camera-relative movement

2. Score System with Combo Mechanics
   - Points for collecting items
   - Bonus points for defeating enemies
   - Combo multiplier system (up to 10x)
   - High score persistence

3. Enemy AI System
   - Patrol behavior
   - Player detection (vision cone)
   - Chase and attack states
   - Line of sight checking

4. User Interface
   - HUD with score, combo, lives, and timer
   - Pause menu
   - Game over screen
   - Level complete screen

PROJECT STRUCTURE
--------------------------------------------------------------------------------
ShadowEscape3D/
├── Assets/
│   ├── Scripts/           # C# game scripts
│   │   ├── PlayerController.cs
│   │   ├── ScoreSystem.cs
│   │   ├── GameManager.cs
│   │   ├── EnemyAI.cs
│   │   ├── UIManager.cs
│   │   ├── CameraController.cs
│   │   └── Collectible.cs
│   ├── Scenes/            # Unity scenes
│   ├── Prefabs/           # Reusable game objects
│   ├── Materials/         # Materials and shaders
│   ├── Animations/        # Animation clips and controllers
│   ├── UI/                # UI assets
│   └── Audio/             # Sound effects and music
├── ProjectSettings/       # Unity project settings
├── Packages/              # Package dependencies
├── .gitignore            # Git ignore rules
└── README.txt            # This file

CONTROLS
--------------------------------------------------------------------------------
Movement:       WASD
Sprint:         Left Shift (hold)
Jump:           Space
Look Around:    Mouse
Pause:          Escape

DEVELOPMENT NOTES
--------------------------------------------------------------------------------
- Unity Version: 2022.3 LTS or later recommended
- Render Pipeline: Built-in Render Pipeline
- Target Resolution: 1920x1080

AUTHOR
--------------------------------------------------------------------------------
Game Development Coursework - CMP-6056B/CMP-7042B
University of East Anglia
School of Computing Sciences

================================================================================

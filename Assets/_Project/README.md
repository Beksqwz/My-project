# SAÑLAQ — Unity vertical slice

Open this project in **Unity 6000.6.0f1**, open `Assets/_Project/Scenes/Sanlaq.unity`, then press **Play**. The arena is already authored and the match automatically reveals your role, counts down and begins. Click the Game view to give keyboard focus.

## Controls and match

- WASD or arrow keys: move. Hold either Shift: sprint.
- One human and three bots; exactly one hunter is assigned each round.
- Hunter: approach a runner to catch automatically, then choose the matching clothing name from three buttons within seven seconds. The icon is the item to identify.
- Correct: runner eliminated. Wrong/timeout: runner escapes, hunter slowed by 50% for three seconds, catch disabled during penalty.
- Runner: survive 90 seconds. Water slows everyone to 65%; enter the cream yurt area for concealment. Elimination changes the camera to follow the hunter while the round continues.
- The hunter wins by eliminating all runners. Otherwise surviving runners win at timeout.
- Results offer random restart, Play Hunter, and Play Runner for engine evaluation.

## Content and implementation

One authored arena, one Player prefab, twelve ClothingItem ScriptableObjects (three per slot), twelve generated icons, reusable primitive sprites. No Godot code or assets were imported. The old repository was read only for product/gameplay reference: https://github.com/mmeirbek/sanlaq

`GameManager` owns the short match state machine. `PlayerController` uses interpolated Rigidbody2D velocity in FixedUpdate and the installed Input System. `BotController` uses local steering and circle casts. `QuizManager` handles quiz timing and answers. `PlayerVisual` builds simple modular placeholder shapes. `GameHud` uses scaled immediate-mode Unity UI with safe-area padding and a soft vision overlay. `AudioFeedback` provides optional replaceable clips and synthesized placeholder tones.

Player visual shapes are runtime children of the prefab. Base shirt/pants/head/shoe layers are independently selected data-driven placeholders; the prior Stage 1 raster sheet is intentionally not used. This is not a final animated character pipeline.

The yurt is a walkable concealment area, not a separate interior. Bots overlook hidden runners until within 1.5 units; hidden runners can still be caught on contact. Characters do not physically push each other; catches use range and a wall visibility check. Other runners keep moving during quizzes, and round time continues. Bot hunters answer automatically after a short delay with 72% accuracy.

## Validation and builds

- **SAÑLAQ → Run gameplay validation** runs actual Play Mode checks including physics, quiz outcomes, role resets, visibility, keyboard input and a complete natural bot match. Results and screenshots are written to `Logs/sanlaq-*`. Run from stopped Play Mode. Validation restarts a normal round when done.
- **SAÑLAQ → Build Windows MVP** produces `Builds/Windows/Sanlaq.exe` using the existing editor's Windows target.
- **SAÑLAQ → Build or refresh MVP scene** regenerates the authored MVP scene, prefab and placeholder data. This overwrites those generated MVP assets; use only when intentionally resetting their authored content.
- The editor helper also consumes one local `Temp/sanlaq-command` file with `setup`, `smoke`, `stop`, or `build` for repeatable local validation. It does not execute arbitrary commands or access a network.

## MVP limits

Desktop keyboard and mouse first. Landscape/safe-area UI is prepared, but touch movement and device builds are not included. AI uses steering, not global pathfinding; bots may take inefficient routes. Clothing names and HUD are predominantly English, with the hunter role also shown in Kazakh. Art, audio, idle bobbing and effects are deliberately simple placeholders. No persistence, main menu, networking, wardrobe unlocks, or final character animation.

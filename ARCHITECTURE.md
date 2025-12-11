# Architecture NoScope

## Structure

```
Assets/Source/
├── GameConfig.cs (ScriptableObject)
├── Managers/  (Singleton MonoBehaviour)
│   ├── GameManager, StateMachine, QTEManager, PipeGenerator
│   ├── UIManager, InputManager, DebugHUD, GameInitializer
├── States/ (Singleton C# - NoScope.States namespace)
│   ├── IState.cs (interface)
│   ├── StatePlay, StatePaused, StateStyle (pure C# Singletons)
├── Player/    Player, Bullet, CameraFollow
├── Enemies/   EnemyBase (abstract), EnemyMass, SmallEnemy
└── Pipes/     Pipes
```

**Namespaces** :
- `NoScope` : Managers, Player, Enemies, Pipes, GameConfig
- `NoScope.States` : IState, StatePlay, StatePaused, StateStyle


---

## Patterns

### 1. Singleton
**Managers** : GameManager, StateMachine, QTEManager, PipeGenerator, UIManager, InputManager (MonoBehaviour)
```csharp
public static GameManager Instance { get; private set; }
void Awake() {
    if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
    else { Destroy(gameObject); }
}
```

**States** : StatePlay, StatePaused, StateStyle (pure C# Singletons)
```csharp
public static StatePlay Instance { get; private set; }
static StatePlay() { Instance = new StatePlay(); }
private StatePlay() {}
```

### 2. State Pattern (Interface)
```csharp
public interface IState {
    void Enter();
    IState Execute();
    void Exit();
}
```

**States** :
- `StatePlay.Instance` : Gameplay normal, mouvement latéral (← →), timeScale=1
- `StatePaused.Instance` : Menu pause, timeScale=0
- `StateStyle.Instance` : QTE actif, temps figé (timeScale=0), WaitForSecondsRealtime

### 3. Object Pooling
**PipeGenerator** : LinkedList + Queue pour pipes réutilisés (pas de GC spike)

### 4. Observer Pattern
Events C# pour communication découplée :
```csharp
public event Action<bool> OnQTEComplete;
QTEManager.Instance.OnQTEComplete += HandleQTE;
```

---

## Flux

### Démarrage
```
GameInitializer → Instantiate Managers → GameManager.StartGame()
→ Spawn Player/EnemyMass → StateMachine.ChangeState(StatePlay.Instance)
```

### Boucle Jeu
```
Update → StateMachine.Update() → CurrentState.Execute()
→ Player.Move(latéral + avant) → Player.Shoot(auto) → EnemyMass.FollowPlayer()
→ PipeGenerator.CheckPlayerPosition() → SpawnNextPipe()
```

### Cycle QTE
```
Player OnTriggerEnter(PipeEnd) → QTEManager.StartQTE()
→ Time.timeScale = 0 → StateStyle.Instance
→ Input flèches (Keyboard.current) → Check sequence
→ Success : Player speed↑, fire rate↑, OnQTEComplete(true), StatePlay.Instance
→ Fail : Reset bonuses, OnQTEComplete(false), StatePlay.Instance
```

### Mouvement Player
```
StatePlay : Keyboard.current.qKey → Lateral velocity (gauche)
          : Keyboard.current.dKey → Lateral velocity (droite)
          : Forward velocity (toujours actif)
          : Auto-jump OnTriggerEnter(PipeEnd)
StateStyle : Mouvement désactivé (QTE uniquement avec flèches ↑↓←→)
```

---

## Input System

**Migration** : `Input.GetKeyDown` → `Keyboard.current.key.wasPressedThisFrame`

**InputManager.cs** :
```csharp
using UnityEngine.InputSystem;
public bool GetUpArrow() => Keyboard.current.upArrowKey.wasPressedThisFrame;
public bool GetLeftArrow() => Keyboard.current.leftArrowKey.wasPressedThisFrame;
```

**Usage** : QTE (flèches ↑↓←→), Pause (ESC), Restart (R), Debug (F1), Lateral (Q/D)

---

## Optimisations

1. **Object Pooling** : Pipes réutilisés (Dequeue/Enqueue)
2. **LinkedList** : Pipes O(1) ajout/suppression
3. **Events** : Moins de checks Update()
4. **States Singleton C#** : Pas de MonoBehaviour overhead

---

## Extensibilité

**Nouvel Ennemi** : Hériter `EnemyBase` → Prefab → Spawn  
**Nouvel État** : Implémenter `IState` → Singleton C# → `StateMachine.ChangeState(NewState.Instance)`  
**Power-Up** : Subscribe events → Modifier stats Player → Prefab + spawn  
**Stats** : Variables GameManager → Events → UIManager

---

## Dépendances

```
GameManager → StateMachine, Player, EnemyMass, UIManager
Player → QTEManager (events), PipeGenerator (position), GameManager (death)
StateMachine → StatePlay.Instance, StatePaused.Instance, StateStyle.Instance
QTEManager → StatePlay.Instance, StateStyle.Instance, UIManager
InputManager → Keyboard.current (UnityEngine.InputSystem)
```

---

**Architecture modulaire, extensible, optimisée pour endless runner.**

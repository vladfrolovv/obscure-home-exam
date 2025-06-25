# Obscure Games Exam
## Solution Overview

Unity Version (Fusion 2 minimal requirements): 2022.3.50f <br>
The original idea was to create three independent Modules (Assemblies) with clearly defined responsibilities:
- **OGClient:** Views logic and Inputs;
- **OGServer:** Dedicated Server logic and Game Rules;
- **OGShared:** Serves a bridge between Client and Server.

I've selected MVC programming pattern to keep the achitecture consistent. The only exeptions are: GameManager with NetworkGameManager, 
and Shared Modules which are called DataProxies.  

However, the current implementation is not fully implemented, it has some broken mechanics, and the Client with
Server are coupled tighter than it should be.

## Dedicated Server overiview:
![obscure-diagrams drawio](https://github.com/user-attachments/assets/7023fb48-1c0d-4e08-b405-38b8034611aa)

#### How to start local server (PlayFab TBD)?
Right now - matchmaking solution is straightforward single room, which is created when local server executed.
1. Run og-builds/og-dedicated-server/OGExam.exe for single match;
2. Connect Clients.

#### Inputs Handling
Server receives input RPCs from the current player via DataProxies Subjects, validates them, and executes actions.
It then syncs the updated state to all clients via broadcast RPCs.


## Demo:
https://github.com/user-attachments/assets/36f19c79-948e-4fcc-a2e6-f9ff03ef7d3e

## Further Actions
- Fix broken Views;
- Move all rules (Game, Grid, Linking, etc.) to OGServer;
- Remake delayed Invoke of Collect method using Moves Queueing;
- ...

Game Name: Hats Off!

Multiplayer Functions:
- Able to connect locally via two instances
- Gameplay interactions involves the host and client interacting with a hat that spawns on the map after players spawn. The players interact with the hat to wear it, and can interact with the other player to steal it
- Player states and actions are visible between host and client

Gameplay:
- PvP gameplay where the host and client fight over holding the hat
- Players can walk, run, and jump on trampolines to avoid and chase after one another

Art:
- Simplistic player models that are animated
- Propeller hat item that is fought over
- Platforms, trampolines, and background

Sound:
- No BGM
- SFX for:
  - Game start
  - Grabbing the hat
  - Losing the hat
  - Trampoline interaction
  - Falling off ledges
  - Landing on the ground

Code Review:
- Project is uploaded to GitHub
- Code is commented

P.S.
Hats Off! Is a mobile game. I show off the touch screen joypad that I'm controlling through my connected phone in the beginning which controls the host. I did not have two applicable devices to show off each character moving at the same time, but if you go into the project settings for the host and disable the Unity Remote/Android connection and enable it in the client, the phone will control the client character. Because I was not able to show this feature off with two devices, I used Unity's features to drag the player models to the crown, trampoline, and other player to show that they do indeed interact with one another, and this interaction is visible to both the host and client.
P.P.S.
Ignore the player falling into the ground on one of the trampoline occasions (oops)! Ray casting only fixed that issue some of the time

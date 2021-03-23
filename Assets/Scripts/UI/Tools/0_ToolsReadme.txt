The namespace for Tools follows this logic:

General: The code in our project generally avoids specifics around graphical layout and styles as we should handle this stuff in the editor and prefabs. The code should pertain to the most basic common functions that ANY GUI will use. The goal of this is to allow easy future modification of the GUI while keeping a consistent codebase.

"Panels": Contain tools but don't themselves relay messages, and are merely for display of tools and elements. They should not talk to the scene in any way.

"Tools": These are the things that actually signal a change in the scene (actions). They can do many things, and can talk to both other UI elements (such as panels) and the view manager of the current scene. 

"Elements": These are displays of information. They should not have any action attached to them.

"Followers": Follow game objects in the scene. They should not have any action attached to them.

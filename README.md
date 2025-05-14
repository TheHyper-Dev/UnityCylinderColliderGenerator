# Unity Cylinder Collider Generator
An editor script that allows you to create and optionally save cylinder colliders with adjustable variables, using the mesh collider component.

Unity's Physx is missing the cylinder shape as one of the primitive collision shapes, which is still kinda crazy to me considering how popular the engine is.

I've seen some "cylinder collider" plugins where they create many, many box colliders just to simulate a cylinder, I thought that was horribly unoptimized and ridiculous so I've come up with this.


Things to note:
* CTRL+Z and CTRL+Y undo/redo actions are supported.
* You don't need the script to keep the collider working, it's simply a mesh creator/modifier that you can remove after being done with it.
* Make sure to toggle on the "Convex" option for better physics performance
  
![image](https://github.com/user-attachments/assets/fe5ae3d1-eeab-411e-a959-cb5b0b64aa83)

Using the saved collider (it's a mesh .asset file)
![image](https://github.com/user-attachments/assets/24aac96d-a639-4cde-aa84-d98e6d64cad7)



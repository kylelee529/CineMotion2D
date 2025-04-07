# CineMotion2D - Version 1.1.0beta Documentation

CineMotion2D is a versatile 2D camera tool designed to provide a wide range of cinematic and gameplay-focused camera behaviors for your Unity 2D projects. This beta version includes features for smooth following, offset, dead zones, grid snapping, boundaries, camera shake, zoom control, basic cinematic panning, focus functionality, and parallax scrolling.

## Table of Contents
- [Installation](#installation)
- [Component Overview](#component-overview)
- [CineMotion2D Script](#cinemotion2d-script)
- [ParallaxLayer Class](#parallaxlayer-class)
- [CineMotion2D Script Properties](#cinemotion2d-script-properties)
- [Public Methods of CineMotion2D](#public-methods-of-cinemotion2d)
- [How to Use CineMotion2D](#how-to-use-cinemotion2d)
- [Utility Functions](#utility-functions)
- [Known Issues (Beta)](#known-issues-beta)
- [Future Features (Planned)](#future-features-planned)
- [Support and Feedback](#support-and-feedback)

## 1. Installation
To use CineMotion2D in your Unity project:
- Download the `CineMotion2D.cs` script and place it in a relevant folder within your project's `Assets` directory (e.g., `Assets/Scripts/Camera`).
- (Optional) Download the example scripts or assets that may accompany this beta release.

## 2. Component Overview
### CineMotion2D Script
This script is the core of the CineMotion2D tool. Attach this script to a GameObject in your scene that you want to act as your main 2D camera. It is recommended to attach it to the main Camera GameObject in your scene.

### ParallaxLayer Class
This is a serializable class used within the CineMotion2D script to define individual parallax scrolling layers. Each layer has a Transform and a parallaxFactor.

## 3. CineMotion2D Script Properties
These properties are accessible and configurable directly within the Unity Inspector when the `CineMotion2D` script is attached to a GameObject.

### Target Following
- **Target Player (Transform):** Assign the Transform of the GameObject that the camera should follow (typically your player character). If this is not assigned, the camera will not follow anything, and a `Debug.LogError` will be displayed.
- **Follow Speed (float):** Controls the speed at which the camera smoothly moves towards the target player's position when smooth follow is enabled. Higher values result in faster movement.

### Offset
- **Offset (Vector2):** Allows you to offset the camera's target position relative to the Target Player. This is useful for framing the player in a specific part of the screen (e.g., slightly below the center). The X component controls the horizontal offset, and the Y component controls the vertical offset.

### Dead Zone
- **Deadzone Radius (float):** Defines a radius around the camera's current position. The camera will only start moving to follow the target player if the player moves outside this radius. This can create a looser, more reactive following behavior. A value of 0 means the camera will always try to move towards the target.

### Smooth Follow
- **Use Smooth Follow (bool):** Enables or disables smooth damping of the camera's movement. When enabled, the camera will smoothly interpolate towards the target position using the Follow Speed.
- **Velocity (Vector3) (Read-only in Inspector):** This internal variable is used by the `Vector3.SmoothDamp` function to track the camera's velocity. You typically don't need to modify this directly.

### Grid Snap
- **Use Grid Snap (bool):** Enables or disables grid snapping for the camera's position. When enabled, the camera's position will be rounded to the nearest multiple of the Grid Size. This is useful for pixel-perfect games or aligning the camera with a tilemap grid.
- **Grid Size (float):** Determines the size of the grid to which the camera's position will snap when Use Grid Snap is enabled.

> Important: Use Smooth Follow and Use Grid Snap are mutually exclusive. If both are checked in the Inspector, Use Grid Snap will be automatically disabled by the `OnValidate()` function.

### Boundaries
- **Use Boundaries (bool):** Enables or disables camera boundaries. When enabled, the camera's position will be clamped within the defined minimum and maximum boundary values.
- **Min Boundary (Vector2):** Defines the minimum X and Y coordinates that the camera's position can reach.
- **Max Boundary (Vector2):** Defines the maximum X and Y coordinates that the camera's position can reach.

### Camera Shake
- **Shake Duration (float) (Read-only in Inspector):** The current duration of the camera shake effect. This value decreases over time when a shake is active.
- **Shake Magnitude (float):** The intensity of the camera shake effect. Higher values result in more pronounced shaking.

### Zoom Control
- **Cam (Camera) (Auto-assigned if left empty):** A reference to the Camera component attached to the same GameObject or the main camera in the scene. It's automatically assigned in Start() if left unassigned in the Inspector.
- **Default Zoom (float):** The default orthographic size of the camera.
- **Zoom Speed (float):** Controls the speed at which the camera's orthographic size interpolates towards the Target Zoom.
- **Target Zoom (float) (Read-only in Inspector):** The desired orthographic size of the camera. This is modified by the SetZoom() method.

### Cinematic Panning
- **Is Cinematic Active (bool) (Read-only in Inspector):** Indicates whether the cinematic panning mode is currently active.
- **Waypoints (Transform[]):** An array of Transform components representing the points the camera will move through during a cinematic sequence. The camera will move through these waypoints in the order they appear in the array.
- **Current Waypoint Index (int) (Read-only in Inspector):** The index of the current waypoint the camera is moving towards.
- **Cinematic Speed (float):** The speed at which the camera moves between waypoints during a cinematic sequence.

### Parallax Scrolling
- **Parallax Layers (ParallaxLayer[]):** An array of ParallaxLayer objects. Each object defines a layer to apply parallax scrolling to and the factor by which it should move relative to the camera's movement.

### Focus
- **Original Target (Transform) (Read-only in Inspector):** Stores the initially assigned Target Player when the FocusOn() method is called.
- **Is Cinematic Active (bool):** Note that the isCinematicActive flag is also used to determine if the camera is in a "focused" state, temporarily overriding normal player following.

## 4. ParallaxLayer Class Properties
- **Layer Transform (Transform):** Assign the Transform of the GameObject that represents a parallax scrolling layer (e.g., a background sprite).
- **Parallax Factor (Vector2):** Controls the speed and direction of the parallax effect for this layer.

## 5. Public Methods of CineMotion2D
- **ShakeCamera(float duration, float magnitude, Vector2 impactDirection):** Triggers a camera shake effect.
- **SetZoom(float newZoom):** Sets the target zoom level for the camera. The actual zoom will smoothly interpolate to this value.
- **SetTarget(Transform newTarget):** Changes the GameObject that the camera is following.
- **StartCinematic():** Initiates the cinematic panning mode, starting from the first waypoint in the Waypoints array.
- **FocusOn(Transform newFocus, float duration):** Temporarily sets a new target for the camera to follow for a specified duration.
- **CenterOnPlayer():** Immediately sets the camera's position to be centered on the Target Player with the current Offset.
- **SnapToGrid():** Immediately snaps the camera's position to the nearest grid cell based on the Grid Size and the Target Player's position.

## 6. How to Use CineMotion2D
### Basic Setup
1. Create a new GameObject in your scene to act as the camera or select your existing main Camera GameObject.
2. Attach the `CineMotion2D` script to this GameObject.
3. In the Inspector, you will see the various properties of the CineMotion2D script.

### Following a Player
1. Drag the Transform of your player character GameObject from the Hierarchy into the `Target Player` field in the Inspector.
2. By default, with Use Smooth Follow enabled, the camera will smoothly follow the player. Adjust the Follow Speed to control how quickly it catches up.

### Using Offset
1. Modify the Offset (Vector2) values in the Inspector. For example, setting Y to 1 will position the player slightly below the center of the screen.

### Implementing a Dead Zone
1. Adjust the Deadzone Radius value in the Inspector. A value greater than 0 will create a dead zone around the camera's current position where the player can move without the camera moving.

### Enabling Smooth Follow
1. Ensure the Use Smooth Follow checkbox is checked in the Inspector.
2. Adjust the Follow Speed as needed.

### Using Grid Snap
1. Check the Use Grid Snap checkbox in the Inspector.
2. Set the Grid Size to the desired grid unit (e.g., 1 for whole units, 0.5 for half units). The camera's position will now snap to this grid based on the target player's position.

> Important: Make sure Use Smooth Follow is unchecked when using Use Grid Snap, as they are mutually exclusive.

### Setting Up Camera Boundaries
1. Check the Use Boundaries checkbox in the Inspector.
2. Set the Min Boundary (Vector2) to the minimum allowed X and Y coordinates for the camera.
3. Set the Max Boundary (Vector2) to the maximum allowed X and Y coordinates for the camera.

### Triggering Camera Shake
1. Call the `ShakeCamera()` method in your code to trigger the shake. Pass in the duration, magnitude, and optional impact direction for the shake effect.

```csharp
camera.ShakeCamera(0.5f, 0.5f, Vector2.zero);

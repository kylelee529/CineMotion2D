using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CineMotion2D))]
public class CineMotion2DEditor : Editor
{
    SerializedProperty targetPlayer, followSpeed, offset, deadzoneRadius, velocity;
    SerializedProperty useSmoothFollow, useGridSnap, gridSize;
    SerializedProperty useBoundaries, minBoundary, maxBoundary;
    SerializedProperty shakeDuration, shakeMagnitude;
    SerializedProperty cam, defaultZoom, zoomSpeed, targetZoom;
    SerializedProperty isCinematicActive, waypoints, currentWaypointIndex, cinematicSpeed;
    SerializedProperty parallaxLayers;

    CineMotion2D cameraScript;

    void OnEnable()
    {
        cameraScript = (CineMotion2D)target;

        targetPlayer = serializedObject.FindProperty("targetPlayer");
        followSpeed = serializedObject.FindProperty("followSpeed");

        // Make sure the field names match exactly in the CineMotion2D script
        offset = serializedObject.FindProperty("offset");
        deadzoneRadius = serializedObject.FindProperty("deadzoneRadius");
        velocity = serializedObject.FindProperty("velocity");

        useSmoothFollow = serializedObject.FindProperty("useSmoothFollow");
        useGridSnap = serializedObject.FindProperty("useGridSnap");
        gridSize = serializedObject.FindProperty("gridSize");

        useBoundaries = serializedObject.FindProperty("useBoundaries");
        minBoundary = serializedObject.FindProperty("minBoundary");
        maxBoundary = serializedObject.FindProperty("maxBoundary");

        shakeDuration = serializedObject.FindProperty("shakeDuration");
        shakeMagnitude = serializedObject.FindProperty("shakeMagnitude");

        cam = serializedObject.FindProperty("cam");
        defaultZoom = serializedObject.FindProperty("defaultZoom");
        zoomSpeed = serializedObject.FindProperty("zoomSpeed");
        targetZoom = serializedObject.FindProperty("targetZoom");

        isCinematicActive = serializedObject.FindProperty("isCinematicActive");
        waypoints = serializedObject.FindProperty("waypoints");
        currentWaypointIndex = serializedObject.FindProperty("currentWaypointIndex");
        cinematicSpeed = serializedObject.FindProperty("cinematicSpeed");

        parallaxLayers = serializedObject.FindProperty("parallaxLayers");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("CineMotion 2D", EditorStyles.boldLabel);

        GUIStyle versionStyle = new GUIStyle(EditorStyles.label);
        versionStyle.fontSize = 10;
        versionStyle.normal.textColor = Color.gray;
        EditorGUILayout.LabelField("Version 1.1.0 - Beta", versionStyle);
        EditorGUILayout.Space();

        DrawSection("Follow Settings", () =>
        {
            EditorGUILayout.PropertyField(targetPlayer);
            EditorGUILayout.Slider(followSpeed, 0f, 20f, new GUIContent("Follow Speed"));
            EditorGUILayout.PropertyField(offset);
            EditorGUILayout.Slider(deadzoneRadius, 0f, 1f, new GUIContent("Deadzone Radius"));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(velocity, new GUIContent("Velocity (Debug)"));
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Center On Player"))
            {
                cameraScript.CenterOnPlayer();
            }
        });

        DrawSection("Snapping & Boundaries", () =>
        {
            EditorGUILayout.PropertyField(useSmoothFollow);
            EditorGUILayout.PropertyField(useGridSnap);
            if (useGridSnap.boolValue)
            {
                EditorGUILayout.PropertyField(gridSize);
                if (GUILayout.Button("Snap Camera To Grid"))
                {
                    cameraScript.SnapToGrid();
                }
            }

            EditorGUILayout.PropertyField(useBoundaries);
            if (useBoundaries.boolValue)
            {
                EditorGUILayout.PropertyField(minBoundary);
                EditorGUILayout.PropertyField(maxBoundary);
            }
        });

        DrawSection("Camera Shake", () =>
        {
            EditorGUILayout.Slider(shakeDuration, 0f, 5f, new GUIContent("Shake Duration"));
            EditorGUILayout.Slider(shakeMagnitude, 0f, 2f, new GUIContent("Shake Magnitude"));
        });

        DrawSection("Zoom Settings", () =>
        {
            EditorGUILayout.PropertyField(cam);
            EditorGUILayout.Slider(defaultZoom, 1f, 20f, new GUIContent("Default Zoom"));
            EditorGUILayout.Slider(zoomSpeed, 0.1f, 10f, new GUIContent("Zoom Speed"));
            EditorGUILayout.Slider(targetZoom, 1f, 20f, new GUIContent("Target Zoom"));

            if (GUILayout.Button("Reset Zoom"))
            {
                cameraScript.ResetZoom();
            }
        });

        DrawSection("Cinematic Mode", () =>
        {
            EditorGUILayout.PropertyField(isCinematicActive);
            EditorGUILayout.PropertyField(waypoints, true);
            EditorGUILayout.Slider(cinematicSpeed, 0.1f, 10f, new GUIContent("Cinematic Speed"));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntSlider(currentWaypointIndex, 0, Mathf.Max(waypoints.arraySize - 1, 0), new GUIContent("Current Waypoint Index"));
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Start Cinematic"))
            {
                cameraScript.StartCinematic();
            }
        });

        DrawSection("Parallax Layers", () =>
        {
            EditorGUILayout.PropertyField(parallaxLayers, true);
        });

        serializedObject.ApplyModifiedProperties();
    }

    void DrawSection(string title, System.Action content)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        content();
        EditorGUILayout.EndVertical();
    }

    void OnSceneGUI()
    {
        if (!cameraScript) return;

        // Deadzone
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(cameraScript.transform.position + (Vector3)cameraScript.offset, Vector3.forward, cameraScript.deadzoneRadius);

        // Boundaries
        if (cameraScript.useBoundaries)
        {
            Handles.color = new Color(1f, 0f, 0f, 0.4f);
            Vector3 min = cameraScript.minBoundary;
            Vector3 max = cameraScript.maxBoundary;
            Vector3 topLeft = new Vector3(min.x, max.y);
            Vector3 topRight = max;
            Vector3 bottomRight = new Vector3(max.x, min.y);
            Vector3 bottomLeft = min;

            Handles.DrawSolidRectangleWithOutline(new Vector3[] { topLeft, topRight, bottomRight, bottomLeft }, new Color(1, 0, 0, 0.1f), Color.red);

            // Allow dragging handles in scene
            EditorGUI.BeginChangeCheck();
            Vector3 newMin = Handles.PositionHandle(min, Quaternion.identity);
            Vector3 newMax = Handles.PositionHandle(max, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(cameraScript, "Move Boundaries");
                cameraScript.minBoundary = newMin;
                cameraScript.maxBoundary = newMax;
            }
        }
    }
}


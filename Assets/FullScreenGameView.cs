#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class FullscreenGameWindow : EditorWindow
{
    static readonly Type GameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
    static readonly PropertyInfo ShowToolbarProperty = GameViewType.GetProperty("showToolbar", BindingFlags.Instance | BindingFlags.NonPublic);
    static readonly object False = false;

    static FullscreenGameWindow fullscreenWindow;

    [MenuItem("Window/General/Game (Fullscreen) %#&2", priority = 2)]
    public static void Toggle()
    {
        if (fullscreenWindow != null)
        {
            fullscreenWindow.Close();
            fullscreenWindow = null;
            return;
        }

        if (GameViewType == null)
        {
            Debug.LogError("GameView type not found.");
            return;
        }

        EditorWindow gameViewInstance = (EditorWindow)ScriptableObject.CreateInstance(GameViewType);
        ShowToolbarProperty?.SetValue(gameViewInstance, False);

        fullscreenWindow = CreateInstance<FullscreenGameWindow>();
        fullscreenWindow.titleContent = new GUIContent("Game Fullscreen");
        fullscreenWindow.position = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
        fullscreenWindow.ShowPopup();
        fullscreenWindow.Focus();

        // Embed the Game View into this window
        gameViewInstance.ShowUtility();
        gameViewInstance.Focus();
    }

    private void OnGUI()
    {
        // Press Escape to close
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            Close();
            fullscreenWindow = null;
            GUIUtility.ExitGUI();
        }
    }
}

#endif

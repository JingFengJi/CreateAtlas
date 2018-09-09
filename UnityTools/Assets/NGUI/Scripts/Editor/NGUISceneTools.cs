using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NGUISceneTools : MonoBehaviour 
{
	[InitializeOnLoadMethod]
	public static void Init()
	{
		SceneView.onSceneGUIDelegate += OnSceneGUI;
	}

	public static void OnSceneGUI(SceneView sceneView)
	{
		Event e = Event.current;
		if((e.modifiers & EventModifiers.Command) == 0) return;
        bool is_handled = false;
		if (e.type == EventType.KeyDown)
		{
			foreach (var item in Selection.transforms)
            {
                Transform trans = item;
				Vector3 oldPos = trans.localPosition;
				Vector3 newPos = oldPos;
                if (trans != null)
                {
                    if (e.keyCode == KeyCode.UpArrow)
                    {
						newPos.y += 1;
                        is_handled = true;
                    }
                    else if (e.keyCode == KeyCode.DownArrow)
                    {
						newPos.y -= 1;
                        is_handled = true;
                    }
                    else if (e.keyCode == KeyCode.LeftArrow)
                    {
						newPos.x -= 1;
                        is_handled = true;
                    }
                    else if (e.keyCode == KeyCode.RightArrow)
                    {
						newPos.x += 1;
                        is_handled = true;
                    }
                }
				trans.localPosition = newPos;
            }
		}
		if (is_handled)
            Event.current.Use();
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class UIAtlasDataCopy : EditorWindow 
{
	private static UIAtlasDataCopy window = null;
	private static UIAtlas originAtlas;
	private static UIAtlas targetAtlas;
	private float interval = 20f;
	private bool isSelectOriginAtlas = false;
	private bool isSelectTargetAtlas = false;
	private bool borderLeft = true;
	private bool borderRight = true;
	private bool borderTop = true;
	private bool borderBottom = true;
	private bool paddingLeft = true;
	private bool paddingRight = true;
	private bool paddingTop = true;
	private bool paddingBottom = true;

	Rect dataListRect 
	{
        get { return new Rect (interval, interval + replaceAtalsRect.yMax, window.position.width - 2 * interval, window.position.height - replaceAtalsRect.height - 3 * interval); }
    }
	
	Rect replaceAtalsRect
	{
		get{return new Rect(interval,interval,window.position.width - 2 * interval,50);}
	}
	void OnGUI()
	{
		DrawReplaceToolBar();
		DrawDataList();
	}

	void OnEnable ()
	{
		window = this;
	}

	void OnSelectAtlas (Object obj)
	{
		UIAtlas atlas = obj as UIAtlas;
		if(isSelectOriginAtlas)
		{
			originAtlas = obj as UIAtlas;
		}
		else if(isSelectTargetAtlas)
		{
			targetAtlas = obj as UIAtlas;
		}
		isSelectOriginAtlas = false;
		isSelectTargetAtlas = false;
	}

	private void DrawDataList()
	{
		GUI.backgroundColor = Color.white;
			
		GUI.Box(dataListRect,"");
		GUILayout.BeginArea(dataListRect);
		borderLeft = GUILayout.Toggle(borderLeft,"borderLeft");
		borderRight = GUILayout.Toggle(borderLeft,"borderRight");
		borderTop = GUILayout.Toggle(borderLeft,"borderTop");
		borderBottom = GUILayout.Toggle(borderLeft,"borderBottom");
		paddingLeft = GUILayout.Toggle(borderLeft,"paddingLeft");
		paddingRight = GUILayout.Toggle(borderLeft,"paddingRight");
		paddingTop = GUILayout.Toggle(borderLeft,"paddingTop");
		paddingBottom = GUILayout.Toggle(borderLeft,"paddingBottom");

		if(GUILayout.Button("一键替换"))
		{
			if(originAtlas != null && targetAtlas != null)
			{
				List<UISpriteData> originSpriteDatas = originAtlas.spriteList;
				List<UISpriteData> targetSpriteDatas = targetAtlas.spriteList;
				if(originSpriteDatas != null && targetSpriteDatas != null)
				{
					for (int i = 0; i < originSpriteDatas.Count; i++)
					{
						for (int j = 0; j < targetSpriteDatas.Count; j++)
						{
							if(originSpriteDatas[i] != null && targetSpriteDatas[j] != null && originSpriteDatas[i].name == targetSpriteDatas[j].name)
							{
								if(borderLeft)
									targetSpriteDatas[j].borderLeft = originSpriteDatas[i].borderLeft;
								if(borderRight)
									targetSpriteDatas[j].borderRight = originSpriteDatas[i].borderRight;
								if(borderTop)
									targetSpriteDatas[j].borderTop = originSpriteDatas[i].borderTop;
								if(borderBottom)
									targetSpriteDatas[j].borderBottom = originSpriteDatas[i].borderBottom;
								
								if(paddingLeft)
									targetSpriteDatas[j].paddingLeft = originSpriteDatas[i].paddingLeft;
								if(paddingRight)
									targetSpriteDatas[j].paddingRight = originSpriteDatas[i].paddingRight;
								if(paddingTop)
									targetSpriteDatas[j].paddingTop = originSpriteDatas[i].paddingTop;
								if(paddingBottom)
									targetSpriteDatas[j].paddingBottom = originSpriteDatas[i].paddingBottom;
							}
						}
					}
					targetAtlas.spriteList = targetSpriteDatas;
					EditorUtility.SetDirty(targetAtlas);
					EditorUtility.DisplayDialog("提示", "替换完成！", "确定");
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			}
		}
		GUILayout.EndArea();
	}

	private void DrawReplaceToolBar()
	{
		GUI.backgroundColor = Color.white;
        GUI.Box(replaceAtalsRect, "");
		GUILayout.BeginArea(replaceAtalsRect);

		GUILayout.BeginHorizontal();
		originAtlas = (UIAtlas)EditorGUILayout.ObjectField("被替换图集", originAtlas, typeof(UIAtlas), true);
		if (NGUIEditorTools.DrawPrefixButton("选择Atlas",GUILayout.Width(200)))
		{
			isSelectOriginAtlas = true;
			ComponentSelector.Show<UIAtlas>(OnSelectAtlas);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		targetAtlas = (UIAtlas)EditorGUILayout.ObjectField("目标图集", targetAtlas, typeof(UIAtlas), true);
		if (NGUIEditorTools.DrawPrefixButton("选择Atlas",GUILayout.Width(200)))
		{
			isSelectTargetAtlas = true;
			ComponentSelector.Show<UIAtlas>(OnSelectAtlas);
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}

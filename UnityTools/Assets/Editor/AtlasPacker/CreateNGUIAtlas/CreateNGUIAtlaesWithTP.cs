using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

public class CreateNGUIAtlaesWithTP : EditorWindow
{
    private static string atlasConfigFileName = "Atlas.ini";
    private static string atlasImporterShell = "AtlasImporter.sh";
    private static string artsFolder = "Arts";
    private static string atlasConfigFilePath = "";
    
    private static List<AtlasConfig> atlasConfigList;

    private static CreateNGUIAtlaesWithTP window = null;
    private static bool isLimitSize = true;
    private static bool initialized = false;
    private SearchField searchField;
    private float interval = 20f;
    private string searchStr = "";
    private Vector2 atlasListScrollViewPos = Vector2.zero;
    Rect SearchFieldRect
	{
		get
		{
			return new Rect(interval,interval,position.width - 2 * interval,20f);
		}
	}
    Rect ToolbarRect
    {
        get
        {
            return new Rect(interval,AtlasListRect.yMax + interval,SearchFieldRect.width,70);
        }
    }
    Rect AtlasListRect
    {
        get
        {
            return new Rect(interval,SearchFieldRect.yMax + interval,SearchFieldRect.width,window.position.height - SearchFieldRect.height - 70 - 4 * interval);
        }
    }

    [MenuItem("Tools/一键打图集工具",false,30)]
    static void ShowWindow()
    {
		if (window == null)
			window = EditorWindow.GetWindow(typeof(CreateNGUIAtlaesWithTP)) as CreateNGUIAtlaesWithTP;
		window.titleContent = new GUIContent("TexturePacker");
		if(isLimitSize)
		{
			window.minSize = new Vector2(800,700);
			window.maxSize = new Vector2(800,700);
		}
        initialized = false;
		window.Show();
    }

    void OnGUI()
	{
		InitIfNeeded();
		DrawWindow();
	}

    private void InitIfNeeded () 
	{
        if (!initialized) 
		{
            if (null == searchField)
                searchField = new SearchField ();
            atlasConfigFilePath = GetAtlasConfigPath();
            atlasConfigList = ParseAtlasConfig(atlasConfigFilePath);
            initialized = true;
        }
    }

    private void DrawWindow()
    {
        DrawSearchField();
		DrawAtlasList();
		DrawToolBar();
    }

    private void DrawSearchField()
    {
        GUI.backgroundColor = Color.white;
		searchStr = searchField.OnGUI (SearchFieldRect, searchStr);
		searchStr = searchStr.ToLower();
    }

    private void DrawAtlasList()
    {
        GUI.backgroundColor = Color.white;
			
		GUI.Box(AtlasListRect,"");

        GUILayout.BeginArea(AtlasListRect);
		//图集列表
		atlasListScrollViewPos = EditorGUILayout.BeginScrollView(atlasListScrollViewPos);
		if(atlasConfigList != null && atlasConfigList.Count > 0)
        {
            for (int i = 0; i < atlasConfigList.Count; i++)
            {
                if(atlasConfigList[i] != null && atlasConfigList[i].IsUseful() && atlasConfigList[i].AtlasPrefabName.ToLower().Contains(searchStr))
                    atlasConfigList[i].Pack = GUILayout.Toggle(atlasConfigList[i].Pack,atlasConfigList[i].AtlasOutputPath);
            }
        }
        
		EditorGUILayout.EndScrollView();
		GUILayout.EndArea();
    }

    private void DrawToolBar()
    {
        GUI.backgroundColor = Color.white;
		GUI.Box(ToolbarRect,"");
        GUILayout.BeginArea(ToolbarRect);
        EditorGUILayout.BeginHorizontal();
        GUILayoutOption option = GUILayout.Height(ToolbarRect.height);
        if(GUILayout.Button("全部取消勾选",option))
        {
            for (int i = 0; i < atlasConfigList.Count; i++)
            {
                atlasConfigList[i].Pack = false;
            }
        }
        if(GUILayout.Button("一键全选",option))
        {
            for (int i = 0; i < atlasConfigList.Count; i++)
            {
                atlasConfigList[i].Pack = true;
            }
        }
        if(GUILayout.Button("一键打图集",option))
        {
            TexturePacker();
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private static Queue<AtlasConfig> replaceAtlasConfigList = new Queue<AtlasConfig>();
    static void TexturePacker()
    {
        replaceAtlasConfigList.Clear();

        for (int i = 0; i < atlasConfigList.Count; i++)
        {
            if(atlasConfigList[i].Pack)
            {
                replaceAtlasConfigList.Enqueue(atlasConfigList[i]);
            }
        }
        curTime = System.DateTime.Now;
        if(replaceAtlasConfigList.Count > 0)
            DoPackerCommond(replaceAtlasConfigList.Dequeue());
    }
    private static System.DateTime curTime;
    private static System.DateTime finishTime;
    static void DoPackerCommond(AtlasConfig config)
    {
        if(config == null) return;
        string cmd = TexturePackerCommond.GetPackCommond(config.PhotosFolderPath,config.PhotoFullPath,config.AtlasTxtFullPath);
        ShellHelper.ShellRequest request = ShellHelper.ProcessCommand(cmd,null);
        EditorUtility.DisplayProgressBar("批量处理中...","Shell脚本生成图集信息...", 0);
        request.onDone  += ()=>
        {
            if(replaceAtlasConfigList.Count > 0)
            {
                AtlasConfig next_config = replaceAtlasConfigList.Dequeue();
                DoPackerCommond(next_config);
            }
            else
            {
                EditorUtility.DisplayProgressBar("批量处理中...","Shell脚本执行完毕...开始打图集...", 0);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                BatchCreateUIAtlasPrefabs();
            }
        };
        request.onError += ()=>
        {
            Debug.LogError("Texture Packer Error!!! Please Check Your Config");
        };
    }
    
    static void BatchCreateUIAtlasPrefabs()
    {
        List<AtlasConfig> tmpConfig = new List<AtlasConfig>();

        if(atlasConfigList != null && atlasConfigList.Count > 0)
        {
            for (int i = 0; i < atlasConfigList.Count; i++)
            {
                if(atlasConfigList[i].Pack)
                {
                    // ShowProgress(i,atlasConfigList.Count);
                    // CreateAtlas(atlasConfigList[i]);
                    tmpConfig.Add(atlasConfigList[i]);
                }
            }
        }

        if(tmpConfig != null && tmpConfig.Count > 0)
        {
            for (int i = 0; i < tmpConfig.Count; i++)
            {
                ShowProgress(i,tmpConfig.Count);
                CreateAtlas(tmpConfig[i]);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        finishTime = System.DateTime.Now;
        Debug.LogError("本次打图集总耗时：" + (finishTime - curTime));
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/清理进度条")]
    static void ClearProgressbar()
    {
        EditorUtility.ClearProgressBar();
    }

    static string GetAtlasConfigPath()
    {
        return Application.dataPath.Replace("Assets",atlasConfigFileName);
    }

    private static List<AtlasConfig> ParseAtlasConfig(string configFilePath)
    {
        if(string.IsNullOrEmpty(configFilePath)) return null;
        if (!File.Exists(configFilePath)) return null;
        List<AtlasConfig> configs = new List<AtlasConfig>();
        using(StreamReader sr = new StreamReader(configFilePath))
        {
            while(sr.Peek() >= 0)
            {
                string str = sr.ReadLine();
                if(!string.IsNullOrEmpty(str))
                {
                    string[] datas = str.Split(new char[] {','}, System.StringSplitOptions.RemoveEmptyEntries);
                    if(datas != null && datas.Length >= 3)
                    {
                        AtlasConfig config = null;
                        string photoFolderPath = datas[0];
                        photoFolderPath = Application.dataPath.Replace("Assets",artsFolder) + Path.DirectorySeparatorChar + photoFolderPath;
                        if(datas.Length == 3)
                        {
                            config = new AtlasConfig(photoFolderPath,datas[1],datas[2]);
                        }
                        else if(datas.Length == 5)
                        {
                            config = new AtlasConfig(photoFolderPath,datas[1],datas[2],datas[3],datas[4]);
                        }
                        configs.Add(config);
                    }
                }
            }
        }
        return configs;
    }

    private static void CreateAtlas(AtlasConfig config)
    {
        if(config == null) return;
        if(!config.IsUseful())
        {
            Debug.LogError("Config :[" + config +  "] Error ,Please check!!");
            return;
        }

        #region 第一步：修改图片设置
        TextureSetting(config.PhotoFilePath,TextureImporterType.Sprite);
        #endregion

        #region 第二步：根据图片创建材质
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(config.MaterialOutputPath);
        if(mat == null)
        {
            mat = new Material(Shader.Find("Unlit/Transparent Colored"));
            AssetDatabase.CreateAsset(mat, config.MaterialOutputPath);
        }
        else
        {
            mat.shader = Shader.Find("Unlit/Transparent Colored");
        }
        mat.mainTexture = AssetDatabase.LoadAssetAtPath(config.PhotoFilePath, typeof(Texture2D)) as Texture2D;
        #endregion

        #region 第三步：创建预设
        GameObject go = null;
        UIAtlas uiAtlas = null;        
        if ((go = AssetDatabase.LoadAssetAtPath(config.AtlasOutputPath, typeof(GameObject)) as GameObject) != null)
        {
            //已存在预制体
            uiAtlas = SetAtlasInfo(go, config, mat);
        }
        else
        {
            go = new GameObject(config.AtlasPrefabName);
            go.AddComponent<UIAtlas>();
            uiAtlas = SetAtlasInfo(go, config, mat);            
            CreatePrefab(go, config);
        } 
        #endregion
    }
    
    private static UIAtlas SetAtlasInfo(GameObject go,AtlasConfig config,Material mat)
    {
        if(go == null || config == null || !config.IsUseful() || mat == null) return null;
        TextAsset atlasTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(config.AtlasTxtFilePath);
        if (atlasTextAsset != null)
        {
            UIAtlas uiAtlas = go.GetComponent<UIAtlas>();
            uiAtlas.spriteMaterial = mat;
			if (uiAtlas.texture != null) 
            {
                NGUIEditorTools.ImportTexture(uiAtlas.texture, false, false, !uiAtlas.premultipliedAlpha);
            }
                
			uiAtlas.MarkAsChanged();

            NGUIJson.LoadSpriteData(uiAtlas, atlasTextAsset);
            uiAtlas.MarkAsChanged();

            return uiAtlas;
        }
        return null;
    }

    public static Object CreatePrefab(GameObject go,AtlasConfig config)
    {
        if(go == null || config == null || !config.IsUseful()) return null;
        Object tmpPrefab = PrefabUtility.CreateEmptyPrefab(config.AtlasOutputPath);
        tmpPrefab = PrefabUtility.ReplacePrefab(go,tmpPrefab,ReplacePrefabOptions.ConnectToPrefab);
        Object.DestroyImmediate(go);
        return tmpPrefab;
    }

    /// <summary>
    /// 设置图片格式
    /// </summary>
    /// <param name="path"></param>
    /// <param name="mTextureImporterType"></param>
    public static void TextureSetting(string path, TextureImporterType mTextureImporterType = TextureImporterType.Default,bool wrapMode = true,int maxSize = 4096)
    {
        if(string.IsNullOrEmpty(path) || !IsTextureFile(path)) return;
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        if (textureImporter == null) return;
        textureImporter.textureType = mTextureImporterType;
        if (textureImporter.textureType == TextureImporterType.Default)
        {
            textureImporter.spriteImportMode = SpriteImportMode.None;
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = false;
            textureImporter.alphaIsTransparency = false;
        }
        else if (textureImporter.textureType == TextureImporterType.Sprite)
        {
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = false;
            if(wrapMode)
                textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.filterMode = FilterMode.Trilinear;
            textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
        }
//#if UNITY_STANDALONE_OSX
        TextureImporterPlatformSettings iPhoneSetting = new TextureImporterPlatformSettings();
        iPhoneSetting.overridden = true;
        iPhoneSetting.format = TextureImporterFormat.ASTC_RGBA_4x4;
        iPhoneSetting.maxTextureSize = maxSize;
        iPhoneSetting.name = "iPhone";
        iPhoneSetting.compressionQuality = (int)TextureCompressionQuality.Normal;
        textureImporter.SetPlatformTextureSettings(iPhoneSetting);
//#endif

        TextureImporterPlatformSettings androidSetting = new TextureImporterPlatformSettings();
        androidSetting.overridden = true;
        androidSetting.maxTextureSize = maxSize;
        androidSetting.name = "Android";
        androidSetting.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        androidSetting.format = TextureImporterFormat.ETC2_RGBA8;
        androidSetting.compressionQuality = (int)TextureCompressionQuality.Normal;
        androidSetting.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
        textureImporter.SetPlatformTextureSettings(androidSetting);

        //textureImporter.SetAllowsAlphaSplitting(true);
        AssetDatabase.ImportAsset(path);
    }

    /// <summary>
    /// 显示进度条
    /// </summary>
    /// <param name="path"></param>
    /// <param name="val"></param>
    static public void ShowProgress(int curIndex,int num)
    {
        EditorUtility.DisplayProgressBar("批量处理中...", string.Format("Please wait...  {0}/{1}", curIndex,num), curIndex * 1.0f / num);
    }

    /// <summary>
    /// 判断是否是图片格式
    /// </summary>
    /// <param name="_path"></param>
    /// <returns></returns>
    static bool IsTextureFile(string _path)
    {
        string path = _path.ToLower();
        return path.EndsWith(".psd") || path.EndsWith(".tga") || path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".dds") || path.EndsWith(".bmp") || path.EndsWith(".tif") || path.EndsWith(".gif");
    }
}

public class AtlasConfig
{
    public string PhotoFilePath;        //.png
    public string AtlasTxtFilePath;     //.txt
    public string AtlasOutputPath;      //.prefab
    public string MaterialOutputPath;   //.mat
    public string PhotosFolderPath;     //原图文件夹路径

    public string PhotoFullPath;        //图集photo全路径
    public string AtlasTxtFullPath;        //图集Txt全路径

    public string AtlasPrefabName;

    private static string atlasFolderRootPath = "Assets" + Path.DirectorySeparatorChar;

    public bool Pack = false;

    private void DisposePath(ref string path)
    {
        if(!string.IsNullOrEmpty(path))
        {
            if(!path.StartsWith("Assets"))
            {
                path = atlasFolderRootPath + path;   
            }
        }
    }

    private void DisposeData()
    {
        DisposePath(ref PhotoFilePath);
        DisposePath(ref AtlasTxtFilePath);
        DisposePath(ref AtlasOutputPath);
        DisposePath(ref MaterialOutputPath);
    }

    public static string AssetPath2FullPath(string assetPath)
    {
        return Path.Combine(ApplicationDataPath, assetPath);
    }

    private static string ApplicationDataPath
    {
        get { return new DirectoryInfo(Application.dataPath).Parent.FullName; }
    }

    public AtlasConfig(string photosFolderPath,string photoPath,string txtFilePath)
    {
        PhotoFullPath = Application.dataPath + Path.DirectorySeparatorChar + photoPath;
        AtlasTxtFullPath = Application.dataPath + Path.DirectorySeparatorChar + txtFilePath;
        PhotosFolderPath = photosFolderPath;
        PhotoFilePath = photoPath;
        AtlasTxtFilePath = txtFilePath;                
        string extension = Path.GetExtension(PhotoFilePath);        
        string atlasOutputPath = PhotoFilePath.Replace(extension,".prefab");                
        AtlasOutputPath = PhotoFilePath.Replace(extension,".prefab");
        MaterialOutputPath = PhotoFilePath.Replace(extension,".mat");
        string file = AtlasOutputPath.Substring(AtlasOutputPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        AtlasPrefabName = file.Replace(Path.GetExtension(AtlasOutputPath),"");
        DisposeData();
    }

    public AtlasConfig(string photosFolderPath,string photoPath,string txtFilePath,string atlasOutPath,string materialPath)
    {
        PhotoFullPath = Application.dataPath + Path.DirectorySeparatorChar + photoPath;
        AtlasTxtFullPath = Application.dataPath + Path.DirectorySeparatorChar + txtFilePath;
        PhotosFolderPath = photosFolderPath;
        PhotoFilePath = photoPath;
        AtlasTxtFilePath = txtFilePath;   
        AtlasOutputPath = atlasOutPath;
        MaterialOutputPath = materialPath;
        string file = AtlasOutputPath.Substring(AtlasOutputPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        AtlasPrefabName = file.Replace(Path.GetExtension(AtlasOutputPath),"");
        DisposeData();
    }
    
    public override string ToString()
    {
        string s = "";
        if(!string.IsNullOrEmpty(PhotoFilePath))
        {
            s += "PhotoFilePath = " + PhotoFilePath;
        }
        if(!string.IsNullOrEmpty(AtlasTxtFilePath))
        {
            s += " AtlasTxtFilePath = " + AtlasTxtFilePath;
        }
        if(!string.IsNullOrEmpty(AtlasOutputPath))
        {
            s += " AtlasOutputPath = " + AtlasOutputPath;
        }
        if(!string.IsNullOrEmpty(MaterialOutputPath))
        {
            s += " MaterialOutputPath = " + MaterialOutputPath;
        }
        return s;
    }

    public bool IsUseful()
    {
        return !string.IsNullOrEmpty(PhotoFilePath) && !string.IsNullOrEmpty(AtlasTxtFilePath) 
        && !string.IsNullOrEmpty(AtlasOutputPath) && !string.IsNullOrEmpty(MaterialOutputPath);
    }
}
namespace KFramework.Editor
{
    using DG.DOTweenEditor.UI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using Object = UnityEngine.Object;
    public class GenerateRoot : EditorWindow
    {
        private static GameObject canvans;

        public static string rootName = "UIRoot";
        public static string rootLayerNamw = "UIObj";
        public static string windowName = "配置置Root相关属性";
        public string width = "1920";
        private bool getIndex;
        public string height = "1080";
        private bool executeOne = false;


        //配置Root对象的所有属性
        private static void SetRootObjConfigData()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("RootName：", GUILayout.Width(65));
            rootName = GUILayout.TextField(rootName, GUILayout.Width(70f));
            GUILayout.Label(" ");
            GUILayout.Label("LayerName：", GUILayout.Width(65));
            rootLayerNamw = GUILayout.TextField(rootLayerNamw, GUILayout.Width(70f));
            GUILayout.EndHorizontal();
        }


        [MenuItem("KToolWindow/Tools/1.生成UIRoot对象预制体", true)]
        private static bool ValidateUIRoots()
        {
            return !GameObject.Find("UIRoot");
        }
        [MenuItem("KToolWindow/Tools/1.生成UIRoot对象预制体")]
        private static void CreateRoots()
        {
            ShowWindom();
        }

        private void OnGUI()
        {     
            if (!executeOne)
            {
                executeOne = true;
                GetGameViewSize(out width, out height);
                icon = AssetDatabase.LoadAssetAtPath("Assets/KTools/Texture/toolIcon.jpg", typeof(Texture2D)) as Texture2D;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Box(icon, GUILayout.Height(45), GUILayout.Width(50));           
            GUILayout.BeginVertical();
            GUILayout.Space(2.5f);
            if (GUILayout.Button("Kmz出品,联系方式 QQ:940636956 欢迎来 B站 Github 知乎一起讨论！！！", GUILayout.Height(43.5f)))
            {
                System.Diagnostics.Process.Start("https://space.bilibili.com/32211044/video");
                GUIUtility.ExitGUI();
            }       
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            SetRootObjConfigData();
            GUILayout.BeginVertical();
            GUILayout.Space(1.5f);
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("屏幕分辨率宽度:",GUILayout.Width(85f));
            width = GUILayout.TextField(width, GUILayout.Width(60f));
            GUILayout.Label(" ");
            GUILayout.Label("*");
            GUILayout.Label("屏幕分辨率高度:", GUILayout.Width(85f));
            height = GUILayout.TextField(height,GUILayout.Width(60f));
            GUILayout.EndHorizontal();
            GUILayout.Space(1.5f);
            GUILayout.BeginVertical();
            if (GUILayout.Button("Apply"))
            {
                int widthValue = 1920;
                int.TryParse(width, out widthValue);
                int heightValue = 1080;
                int.TryParse(height, out heightValue);
                Setup(widthValue, heightValue);
                int index = 0;
                getIndex = GetIndex(width + "*" + height, out index);
                SetScreenResulation(index);
                AssetDatabase.Refresh();
                SetGameViewScale();
                Close();
            }
            GUILayout.EndVertical();
        }
     

        static void Setup(int width, int height)
        {
            //1 创建Root对象
            var root = new GameObject(rootName);
            AddOrSetLayer(root, rootLayerNamw);
            //UICameara
            CreateObjAddType("UICamera", root.transform, false, (UICamera) => {
                AddOrSetTag("UICamera", UICamera);
                AddOrSetLayer(UICamera, rootLayerNamw);
                var camera = UICamera.GetComponent<Camera>();
                camera.clearFlags = CameraClearFlags.Depth;
                //  camera.cullingMask = 0; //  什么层都不渲染 nothing
                camera.cullingMask = -1; //渲染所有层  everything
                camera.cullingMask &= ~(1 << 0); //   在原来的基础上减掉第x层 
                camera.cullingMask &= ~(1 << 1); //   在原来的基础上减掉第x层 
                camera.cullingMask &= ~(1 << 2); //   在原来的基础上减掉第x层 
                camera.cullingMask &= ~(1 << 4); //   在原来的基础上减掉第x层 
                //Post层
            //    camera.cullingMask &= ~(1 << 8); //   在原来的基础上减掉第x层 
                camera.orthographic = true;
                camera.depth = 10;
                camera.renderingPath = RenderingPath.Forward;
                camera.useOcclusionCulling = true;
                camera.allowHDR = true;
                camera.allowMSAA = true;
            }, typeof(Camera));
            //2 创建Canvas对象
            CreateObjAddType("UICanvas", root.transform, true, (_Canvas) =>
            {
                canvans = _Canvas;
                var ca = _Canvas.GetComponent<Canvas>();
                ca.renderMode = RenderMode.ScreenSpaceCamera;
              //  强制画布中的元素按像素对齐。仅在 renderMode 为屏幕空间时适用
                //ca.pixelPerfect = true;
                var uiCamera = GameObject.FindWithTag("UICamera");
                if (uiCamera==null)
                {
                    Debug.Log("Null");
                }
                ca.worldCamera = uiCamera.GetComponent<Camera>();
                var canvasScaler = _Canvas.GetComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(width, height);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = width > height ? 1f : 0f;
                AddOrSetLayer(_Canvas, rootLayerNamw);
            }, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            //2 创建EventSystem对象

            CreateObjAddType("EventSystem", root.transform, false, (es) =>
            {
            }, typeof(EventTrigger), typeof(StandaloneInputModule));

            // 设置父对象

            //设置层级


            //效果圖
            CreateObjAddType("Effect_Img", canvans.transform, true, (effect) =>
            {
                var rect = effect.GetComponent<RectTransform>();
                rect.SetSiblingIndex(0);
                
                rect.sizeDelta = Vector2.zero;
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 1);
                var img = effect.GetComponent<Image>();
                img.raycastTarget = false;
                img.color = new Color(0f, 0f, 0f, 0.5f);
                AddOrSetLayer(effect, rootLayerNamw);
            }, typeof(Image));
            //弹窗
            CreateObjAddType("Popup", canvans.transform, true, (popWindow) => {
                popWindow.transform.SetSiblingIndex(canvans.transform.childCount - 1);
                //其实应该创建个脚本加载上面
                //Todo
            });


            //制作成Prefab
            string saveFolderPath = Application.dataPath + "/Resources/Prefabs";
            if (!Directory.Exists(saveFolderPath))
            {
                Directory.CreateDirectory(saveFolderPath);
            }
            string prefabPath = saveFolderPath + "/UIRoot.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, prefabPath, InteractionMode.AutomatedAction);


        }
        #region API
        //static readonly Vector2 _WinSize = new Vector2(370, 650);
        //static Vector2 _headerSize, _footerSize;
        static Texture2D icon;
        private static void ShowWindom()
        {
            var window = GetWindow<GenerateRoot>(true, windowName, true);
            window.ShowUtility();
            icon = AssetDatabase.LoadAssetAtPath("Assets/KTools/Texture/toolIcon.jpg", typeof(Texture2D)) as Texture2D;
            GUIContent titleContent = new GUIContent(windowName, icon, "Root对象属性配置窗口");//标题 图标 Tips
            window.titleContent = titleContent;
         

        }
        private static void CreateObjAddType(string objNamw, Transform parent, bool isUI = true, Action<GameObject> act = null, params Type[] types)
        {
            var obj = new GameObject(objNamw);
            obj.transform.SetParent(parent);
            if (isUI)
            {
                var rect = obj.AddComponent<RectTransform>();
                rect.localPosition = Vector3.zero;
            }
            foreach (var item in types)
            {
                obj.AddComponent(item);
            }
            if (act != null)
            {
                act(obj);
            }
        }
        #endregion

        #region 关于层
 
        public bool ContainSelflayerSelectLayerMask(LayerMask layerMasks,GameObject targetObj)
        {
            if ((layerMasks.value & (int)Mathf.Pow(2, targetObj.layer)) == (int)Mathf.Pow(2, targetObj.layer))
            {
                Debug.Log("在层中");
                return true;
            }
            else
            {
                Debug.Log("不在层中");
                return false;
            }

        }


        static bool IsHasLayer(string layer)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/Tagmanager.asset"));
            SerializedProperty it = tagManager.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.name == "layers")
                {
                    for (int i = 0; i < it.arraySize; i++)
                    {
                        SerializedProperty sp = it.GetArrayElementAtIndex(i);
                        if (!string.IsNullOrEmpty(sp.stringValue))
                        {
                            if (sp.stringValue.Equals(layer))
                            {
                                sp.stringValue = string.Empty;
                                tagManager.ApplyModifiedProperties();
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.layers.Length; i++)
            {
                if (UnityEditorInternal.InternalEditorUtility.layers[i].Contains(layer))
                    return true;
            }
            return false;
        }

        //Todo 添加层
        // <summary>
        /// 添加layer
        /// </summary>
        /// <param name="layer"></param>
        private static void AddOrSetLayer( GameObject targetObj, string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                Debug.LogError(string.Format("{0}层级名称为空", layerName));
            }
            if (!IsHasLayer(layerName))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty it = tagManager.GetIterator();
                while (it.NextVisible(true))
                {
                    if (it.name == "layers")
                    {
                        for (int i = 0; i < it.arraySize; i++)
                        {
                            if (i == 3 || i == 6 || i == 7)
                            {
                                continue;
                            }
                            SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                            if (string.IsNullOrEmpty(dataPoint.stringValue))
                            {
                                dataPoint.stringValue = layerName;
                                tagManager.ApplyModifiedProperties();
                                break;
                            }
                        }
                    }
                }
            }
            if (targetObj!=null)
            {
                Debug.Log("设置UILayer");
                targetObj.layer = LayerMask.NameToLayer(layerName);
            }
            else
            {
                Debug.Log(targetObj.name + "：对象为空");
            }

        }

        #endregion

        #region 关于Tag
        //添加tag标签
        static void AddOrSetTag(string tag, GameObject targetObj)
        {
            if (!isHasTag(tag))
            {
                SerializedObject tagManager = new SerializedObject(targetObj);//序列化物体
                SerializedProperty it = tagManager.GetIterator();//序列化属性
                while (it.NextVisible(true))//下一属性的可见性
                {
                    if (it.name == "m_TagString")
                    {
                        it.stringValue = tag;
                        tagManager.ApplyModifiedProperties();
                    }
                }
            }
            else
            {
                targetObj.tag = tag;
            }
        }
       static bool isHasTag(string tag)
        {
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
            {
                if (UnityEditorInternal.InternalEditorUtility.tags[i].Equals(tag))
                    return true;
            }
            return false;
        }

        #endregion

        #region GameViewData
        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<string, int> resulationDic = new Dictionary<string, int>()
        {
            {"0*0",0},
            {"5*4",1},
            {"4*3",2},
            {"3*2",3},
            {"16*10",4},
            {"16*9",5},
            {"1024*768",6},
            {"1920*1080",7},
            {"1080*1920",8},
            {"720*1280",9},
            {"405*720",10},
            {"100*320",11},
            {"1280*720",12},
            {"1334*750",13},
            {"800*1280",14},
            {"640*1136",15},
            {"2560*1440",16},
            {"480*854",17},
            {"1536*2048",18},
            {"768*1024",19},
        };

        static bool GetIndex(string name, out int index)
        {
            if (resulationDic.ContainsKey(name))
            {
                index = resulationDic[name];
                return true;
            }
            index = 0;
            return false;
        }
        #endregion

        #region GameView
        //设置GameView分辨率
        static void SetScreenResulation(int index)
        {
            var type = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            var window = GetWindow(type);
            var SizeSelectionCallback = type.GetMethod("SizeSelectionCallback",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            SizeSelectionCallback.Invoke(window, new object[] { index, null });
        }

        /// <summary>
        /// 获取Game View的分辨率
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void GetGameViewSize(out string width, out string height)
        {
            Type T = Type.GetType("UnityEditor.GameView,UnityEditor");
             MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", BindingFlags.NonPublic |BindingFlags.Static);
            System.Object Res = GetMainGameView.Invoke(null, null);
            var gameView = (EditorWindow)Res;
            var prop = gameView.GetType().GetProperty("currentGameViewSize", BindingFlags.NonPublic | BindingFlags.Instance);
            var gvsize = prop.GetValue(gameView, new object[0] { });
            var gvSizeType = gvsize.GetType();
            height = ((int)gvSizeType.GetProperty("height", BindingFlags.Public | BindingFlags.Instance).GetValue(gvsize, new object[0] { })).ToString();
            width = ((int)gvSizeType.GetProperty("width", BindingFlags.Public | BindingFlags.Instance).GetValue(gvsize, new object[0] { })).ToString();
        }
        #endregion

        #region 解决GameView窗口滑动条自动缩放中间的Bug
        private static void SetGameViewScale()
        {
            Type gameViewType = GetGameViewType();
            EditorWindow gameViewWindow = GetGameViewWindow(gameViewType);

            if (gameViewWindow == null)
            {
                Debug.LogError("GameView is null!");
                return;
            }

            var defScaleField = gameViewType.GetField("m_defaultScale", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            //whatever scale you want when you click on play
            float defaultScale = 0.1f;

            var areaField = gameViewType.GetField("m_ZoomArea", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var areaObj = areaField.GetValue(gameViewWindow);

            var scaleField = areaObj.GetType().GetField("m_Scale", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            scaleField.SetValue(areaObj, new Vector2(defaultScale, defaultScale));
        }

        private static Type GetGameViewType()
        {
            Assembly unityEditorAssembly = typeof(EditorWindow).Assembly;
            Type gameViewType = unityEditorAssembly.GetType("UnityEditor.GameView");
            return gameViewType;
        }

        private static EditorWindow GetGameViewWindow(Type gameViewType)
        {
            Object[] obj = Resources.FindObjectsOfTypeAll(gameViewType);
            if (obj.Length > 0)
            {
                return obj[0] as EditorWindow;
            }
            return null;
        }
        #endregion
    }
}
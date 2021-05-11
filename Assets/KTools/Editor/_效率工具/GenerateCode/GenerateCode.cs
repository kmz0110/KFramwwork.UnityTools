namespace KFramework.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.IO;

    public class GenerateCode
    {
        //[MenuItem("Component/*GenerateCode", true)]
        //private static bool ValidateMenu()
        //{
        //    return !GameObject.Find("UIRoot");
        //}
        [MenuItem("GameObject/@GenerateCode", false, 0)]
        private static void MenuFunc()
        {
            var codePath = Application.dataPath + "/Script/Logic";
            if (!Directory.Exists(codePath))
            {
                Directory.CreateDirectory(codePath);
            }
            var code = File.Create(codePath + "/Root.cs");
            code.Close();
            AssetDatabase.Refresh();

            Debug.Log("Code");
        }
        //生成各种类型脚本
        //1.编辑器脚本
        //2、Mono脚本
        //3.Shader

    }
}
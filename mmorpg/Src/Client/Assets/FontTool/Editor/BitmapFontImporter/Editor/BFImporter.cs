using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;

namespace litefeel
{
    public class BFImporter : AssetPostprocessor
    {
        /*static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                //Debug.Log("Reimported Asset: " + str);
                DoImportBitmapFont(str);
            }
            foreach (string str in deletedAssets)
            {
                //Debug.Log("Deleted Asset: " + str);
                DelBitmapFont(str);
            }

            for (var i = 0; i < movedAssets.Length; i++)
            {
                //Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
                MoveBitmapFont(movedFromAssetPaths[i], movedAssets[i]);
            }
        }*/

        public static bool IsFnt(string path)
        {
            return path.EndsWith(".fnt", StringComparison.OrdinalIgnoreCase);
        }

        public static string DoImportBitmapFont(string fntPatn,string outName="",Action<string> ac = null)
        {
            if (!IsFnt(fntPatn)) return "这不是个字体啊！";

            TextAsset fnt = AssetDatabase.LoadAssetAtPath<TextAsset>(fntPatn);
            if (fnt == null) return "没有在资源中找到该字体！";
            string text = fnt.text;

            FntParse parse = FntParse.GetFntParse(ref text);
            if (parse == null) return "字体解析错误";
            string hz = new FileInfo(fntPatn).Extension;
            string rootPath = Path.GetDirectoryName(fntPatn);
            DirectoryInfo parentPath = new FileInfo(fntPatn).Directory;
            //if (File.Exists(fntPatn))
            //{
            //    File.Move(fntPatn, rootPath + "/" + parentPath.Name + hz);
            //}
            fntPatn = rootPath + "/" + parentPath.Name + hz;
            string fntName = string.IsNullOrEmpty(outName)?parentPath.Name:outName;

            
            string fontPath = string.Format("{0}/{1}.fontsettings", rootPath, fntName);
            string texPath = string.Format("{0}/{1}", rootPath, parse.textureName);

            Texture2D texture = AssetDatabase.LoadMainAssetAtPath(texPath) as Texture2D;
            if (texture == null)
            {
                Debug.LogErrorFormat(fnt, "{0}: not found '{1}'.", typeof(BFImporter), texPath);
                return "找不到字体图片";
            }

            TextureImporter texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
            texImporter.textureType = TextureImporterType.GUI;
            texImporter.mipmapEnabled = false;
            texImporter.SaveAndReimport();

            Material material = new Material(Shader.Find("UI/Default"));
            material.name = "Font Material";
            material.mainTexture = texture;
            Font font = new Font();
            font.material = material;
            font.characterInfo = parse.charInfos;


            //AssetDatabase.ImportAsset(fontPath);

            SerializedObject so = new SerializedObject(font);

            so.Update();
            so.FindProperty("m_FontSize").floatValue = Mathf.Abs(parse.fontSize);
            so.FindProperty("m_LineSpacing").floatValue = parse.lineHeight;
            //这种做法是把基准线定在中心点，所以要求字体图片必须高度一致，否则出问题
            so.FindProperty("m_Ascent").floatValue = parse.lineBaseHeight;
            SerializedProperty prop = so.FindProperty("m_Descent");
            if (prop != null)
                prop.floatValue = parse.lineBaseHeight - parse.lineHeight;
            UpdateKernings(so, parse.kernings);
            so.ApplyModifiedProperties();
            so.SetIsDifferentCacheDirty();
            AssetDatabase.CreateAsset(font, fontPath);
            AssetDatabase.AddObjectToAsset(material, fontPath);
            //AssetDatabase.WriteImportSettingsIfDirty(fontPath);
            //AssetDatabase.ImportAsset(fontPath);
            AssetDatabase.SaveAssets();

#if UNITY_5_5_OR_NEWER
            //unity 5.5 can not load custom font
            //Debug.Log("Reload");
            //ReloadFont(fontPath,ac);
#endif
            return "";
        }

        private static void UpdateKernings(SerializedObject so, Kerning[] kernings)
        {
            int len = kernings != null ? kernings.Length : 0;
            SerializedProperty kerningsProp = so.FindProperty("m_KerningValues");

            if (len == 0)
            {
                kerningsProp.ClearArray();
                return;
            }

            int propLen = kerningsProp.arraySize;
            for (int i = 0; i < len; i++)
            {
                if (propLen <= i)
                {
                    kerningsProp.InsertArrayElementAtIndex(i);
                }

                SerializedProperty kerningProp = kerningsProp.GetArrayElementAtIndex(i);
                kerningProp.FindPropertyRelative("second").floatValue = kernings[i].amount;
                SerializedProperty pairProp = kerningProp.FindPropertyRelative("first");
                pairProp.Next(true);
                pairProp.intValue = kernings[i].first;
                pairProp.Next(false);
                pairProp.intValue = kernings[i].second;
            }
            for (int i = propLen - 1; i >= len; i--)
            {
                kerningsProp.DeleteArrayElementAtIndex(i);
            }
        }
        
        private static void DelBitmapFont(string fntPath)
        {
            if (!IsFnt(fntPath)) return;

            string fontPath = fntPath.Substring(0, fntPath.Length - 4) + ".fontsettings";
            AssetDatabase.DeleteAsset(fontPath);
        }

        private static void MoveBitmapFont(string oldFntPath, string nowFntPath)
        {
            if (!IsFnt(nowFntPath)) return;

            string oldFontPath = oldFntPath.Substring(0, oldFntPath.Length - 4) + ".fontsettings";
            string nowFontPath = nowFntPath.Substring(0, nowFntPath.Length - 4) + ".fontsettings";
            AssetDatabase.MoveAsset(oldFontPath, nowFontPath);
        }

        // new font can not display via Text in unity 5.5
        // must import import it
        private static void ReloadFont(string fontPath,System.Action<string> action)
        {
            var tmpPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            AssetDatabase.ExportPackage(fontPath, tmpPath);
            AssetDatabase.DeleteAsset(fontPath);
            
            var startTime = DateTime.Now;
            EditorApplication.CallbackFunction func = null;
            func = () =>
            {
                TimeSpan dalt = DateTime.Now - startTime;
                if (dalt.TotalSeconds >= 0.1)
                {
                    EditorApplication.update -= func;
                    AssetDatabase.ImportPackage(tmpPath, true);
                    File.Delete(tmpPath);
                    action("");
                }
            };

            EditorApplication.update += func;
        }
    }

}


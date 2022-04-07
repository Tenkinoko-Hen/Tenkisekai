using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Text;
using System;

public class FontWindow : EditorWindow
{

    private string FontPath;
    public int outWidth = 512;
    public int outHeight = 512;
    public string outName;
    public string outTextureType = "png";
    public string outAsc;
    public string outAscTotal;
    public string resPath;
    private Dictionary<string, string> dict = new Dictionary<string, string>();


    //暂时先不提供这种用法吧
    //[MenuItem("Tools/Gen Font")]
    //public static void Run()
    //{

    //    Rect wr = new Rect(0, 0, 300, 175);
    //    FontWindow windows = (FontWindow)EditorWindow.GetWindowWithRect(typeof(FontWindow), wr, true, "创建美术字体");
    //    windows.FontPath = "Assets/HotResources/OutResources/Font/";
    //    windows.Show();

    //}

    [MenuItem("Assets/FontTools/创建字体在此文件夹")]
    public static void Create()
    {
        string guid = Selection.assetGUIDs[0];
        var selectPath = AssetDatabase.GUIDToAssetPath(guid);
        Rect wr = new Rect(0, 0, 300, 175);
        FontWindow windows = (FontWindow)EditorWindow.GetWindowWithRect(typeof(FontWindow), wr, true, "创建美术字体");
        windows.FontPath = selectPath + "/";
        windows.Show();
    }

    private GUIStyle style;
    private GUIStyle style2;
    private string errorContent;
    private Color errorColor;

    private string tempName = "";
    private void Init()
    {
        if (this.style == null)
        {
            this.style = new GUIStyle(GUI.skin.label);
            this.style.normal.textColor = Color.red;
            this.style.alignment = TextAnchor.MiddleCenter;


        }
        this.style.normal.textColor = this.errorColor;
        if (this.style2 == null)
        {
            this.style2 = new GUIStyle(GUI.skin.label);
            this.style2.normal.textColor = Color.gray;
            this.style2.alignment = TextAnchor.MiddleCenter;
        }
        if (this.dict.Count == 0)
        {

            this.dict.Add("wh", "63");
            this.dict.Add("zx", "47");
            this.dict.Add("fx", "92");
            this.dict.Add("xh", "42");
            this.dict.Add("mh", "58");
            this.dict.Add("yh", "34");
            this.dict.Add("xy", "60");
            this.dict.Add("dy", "62");
            this.dict.Add("sx", "124");
        }
    }
    private bool isReplace = false;
    bool boo = false;
    private void OnGUI()
    {
        this.Init();
        this.resPath = PlayerPrefs.GetString("Editor_FontTools_LastPath");

        this.outName = EditorGUILayout.TextField("字体名", this.outName);

        int[] intArr = { 32, 64, 128, 256, 512, 1024, 2048 };
        string[] strArr = { "png", "jpg" };
        this.outWidth = EditorGUILayout.IntPopup("图集宽度", this.outWidth, System.Array.ConvertAll(intArr, (int i) =>
        {
            return "x" + i.ToString();
        }), intArr);

        this.outHeight = EditorGUILayout.IntPopup("图集高度", this.outHeight, System.Array.ConvertAll(intArr, (int i) =>
        {
            return "x" + i.ToString();
        }), intArr);

        this.outTextureType = strArr[EditorGUILayout.Popup("图片类型", 0, strArr)];
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("美术路径", this.resPath);

        if (GUILayout.Button("...", GUILayout.Width(20)))
        {

            this.resPath = EditorUtility.OpenFolderPanel("选择图片文件夹", this.resPath, "");

            PlayerPrefs.SetString("Editor_FontTools_LastPath", this.resPath);
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.LabelField(this.errorContent, this.style);
        if (this.outName != this.tempName)
        {
            this.isReplace = false;
            this.SetError("");
        }
        GUILayout.BeginHorizontal();
        if (!this.isReplace)
        {

            GUILayout.Space(100);
            if (GUILayout.Button("确认", GUILayout.Width(100)))
            {
                this.tempName = this.outName;
                this.SetError("");
                if (string.IsNullOrEmpty(this.outName))
                {
                    this.SetError("请输入字体名字！");
                    return;
                }
                if (string.IsNullOrEmpty(this.resPath))
                {
                    this.SetError("请选择美术资源路径！");
                    return;
                }
                FileInfo[] fileInfos = new DirectoryInfo(this.resPath).GetFiles("*." + this.outTextureType);
                if (fileInfos.Length == 0)
                {
                    this.SetError("该文件夹下没有图片哦！");
                    return;
                }
                DirectoryInfo[] DirectoryInfos = new DirectoryInfo(FontPath).GetDirectories();
                for (int i = 0; i < DirectoryInfos.Length; i++)
                {
                    if (DirectoryInfos[i].Name == this.outName)
                    {
                        this.isReplace = true;
                        this.SetWarning("已经存在该字体，是否覆盖？");
                        return;
                    }
                }
                this.CreateFont(fileInfos);
            }
        }
        else
        {
            GUILayout.Space(40);
            if (GUILayout.Button("替换", GUILayout.Width(90)))
            {
                FileInfo[] fileInfos = new DirectoryInfo(this.resPath).GetFiles("*." + this.outTextureType);
                this.CreateFont(fileInfos);
            }
            GUILayout.Space(40);
            if (GUILayout.Button("取消", GUILayout.Width(90)))
            {
                this.isReplace = false;
                this.SetError("");
                return;
            }

        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        boo = EditorGUILayout.Foldout(boo, "帮助信息");
        if (boo)
        {
            this.maxSize = this.minSize = new Vector2(300, 350);
            EditorGUILayout.LabelField("特殊字符请按照以下规则命名", this.style2);
            EditorGUILayout.LabelField(@"【:】    mh", this.style2);
            EditorGUILayout.LabelField(@"【""】    yh", this.style2);
            EditorGUILayout.LabelField(@"【|】    sx", this.style2);
            EditorGUILayout.LabelField(@"【>】    dy", this.style2);
            EditorGUILayout.LabelField(@"【<】    xy", this.style2);
            EditorGUILayout.LabelField(@"【?】    wh", this.style2);
            EditorGUILayout.LabelField(@"【/】    zx", this.style2);
            EditorGUILayout.LabelField(@"【\】    fx", this.style2);
            EditorGUILayout.LabelField(@"【*】    xh", this.style2);
        }
        else
        {
            this.maxSize = this.minSize = new Vector2(300, 175);
        }
    }

    private void CreateFont(FileInfo[] fileInfos)
    {
        //这里开始就是能执行了哦
        string fullPath = Path.GetFullPath(FontPath + this.outName);
        string tempPath = Path.GetFullPath(FontPath + this.outName + "/__temp");
        string tempPath2 = FontPath + this.outName + "/__temp";
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        if (Directory.Exists(tempPath))
        {
            Directory.Delete(tempPath, true);
        }
        Directory.CreateDirectory(tempPath);




        //string path = new DirectoryInfo("Tools/FontGen/").FullName;
        string path = new DirectoryInfo(AssetDatabase.GUIDToAssetPath("7809c7eb167eaf945abae3638a4e727a") + "/").FullName ;
        //string path = new DirectoryInfo(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("FontGen")[0]) + "/").FullName;
        Debug.Log(path);

        this.outAsc = "";
        this.outAscTotal = "";
        ASCIIEncoding charToASCII = new ASCIIEncoding();
        if (File.Exists(path + "config.bmfc")) File.Delete(path + "config.bmfc");
        var utf8WithoutBom = Encoding.GetEncoding("gb2312");
        StreamWriter sw = new StreamWriter(tempPath + "/config.bmfc", false, utf8WithoutBom);
        sw.WriteLine(string.Format(config, this.outWidth, this.outHeight, this.outTextureType));
        for (int i = 0; i < fileInfos.Length; i++)
        {
            string name = null;
            string fileName = Path.GetFileNameWithoutExtension(fileInfos[i].Name);
            if (dict.ContainsKey(fileName))
            {
                name = dict[fileName];
            }
            else
            {
                name = ((int)charToASCII.GetBytes(fileName)[0]).ToString();
            }
            if (string.IsNullOrEmpty(name))
            {
                this.SetError("图片命名不规范，请检查问题！");
                return;
            }
            string str = string.Format(@"icon=""{0}"",{1},0,0,0", fileInfos[i].FullName, name);
            str = str.Replace("\\", "/");
            sw.WriteLine(str);
            if (i == fileInfos.Length - 1)
            {
                this.outAscTotal += name;
            }
            else
            {

                this.outAscTotal += name + ",";
            }
        }
        this.outAscTotal = "chars=" + this.outAscTotal;
        sw.WriteLine(this.outAscTotal);

        sw.Close();
        CSharpCallBat.RunBat(path + "_FontGen.bat", tempPath+","+ path, "", true);
        AssetDatabase.Refresh();
        string error = litefeel.BFImporter.DoImportBitmapFont(tempPath2 + "/font.fnt", this.outName, (string result) =>
        {

        });

        if (!string.IsNullOrEmpty(error))
        {
            this.SetInfo(error);
        }
        else
        {
            this.SetInfo("制作成功");
            this.isReplace = false;

            DirectoryInfo dinfo = new DirectoryInfo(fullPath);
            var finfo = dinfo.GetFiles();
            for (int i = 0; i < finfo.Length; i++)
            {
                if (finfo[i].Name == this.outName + ".fontsettings.meta") continue;
                finfo[i].Delete();
            }
            if (File.Exists(tempPath + "/config.bmfc"))
            {
                File.Delete(tempPath + "/config.bmfc");
            }
            if (File.Exists(tempPath + "/config.bmfc.meta"))
            {
                File.Delete(tempPath + "/config.bmfc.meta");
            }

            DirectoryInfo dinfo2 = new DirectoryInfo(tempPath);
            var finfo2 = dinfo2.GetFiles();
            for (int i = 0; i < finfo2.Length; i++)
            {
                if (finfo2[i].Name == this.outName + ".fontsettings.meta")
                {
                    finfo2[i].Delete();
                }
                else
                {
                    string str = Path.Combine(fullPath, finfo2[i].Name);
                    finfo2[i].MoveTo(str);
                }
            }

            dinfo2.Delete();
        }
        var startTime = DateTime.Now;
        EditorApplication.CallbackFunction func = null;
        func = () =>
        {
            TimeSpan dalt = DateTime.Now - startTime;
            if (dalt.TotalSeconds >= 0.5)
            {
                EditorApplication.update -= func;

                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath);
                    AssetDatabase.Refresh();
                }
            }
        };

        EditorApplication.update += func;
        AssetDatabase.Refresh();
    }
    private void SetError(string content)
    {
        this.errorContent = content;
        this.errorColor = Color.red;
    }
    private void SetWarning(string content)
    {
        this.errorContent = content;
        this.errorColor = Color.yellow;
    }
    private void SetInfo(string content)
    {
        this.errorContent = content;
        this.errorColor = Color.green;
    }
    void OnInspectorUpdate()
    {
        //重画
        this.Repaint();
    }


    private const string config =
@"fileVersion=1
fontName=Arial
fontFile =
charSet = 0
fontSize=32
aa=1
scaleH=100
useSmoothing=1
isBold=0
isItalic=0
useUnicode=1
disableBoxChars=1
outputInvalidCharGlyph=0
dontIncludeKerningPairs=0
useHinting=1
renderFromOutline=0
useClearType=1
paddingDown=0
paddingUp=0
paddingRight=0
paddingLeft=0
spacingHoriz=1
spacingVert=1
useFixedHeight=0
forceZero=0
outWidth={0}
outHeight={1}
outBitDepth=32
fontDescFormat=0
fourChnlPacked=0
textureFormat={2}
textureCompression=0
alphaChnl=1
redChnl=0
greenChnl=0
blueChnl=0
invA=0
invR=0
invG=0
invB=0
outlineThickness=0";
}


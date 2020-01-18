using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFK
{
    public class UIConfig
    {
        public class FontField
        {
            public string Name;
            public string Path;
        }

        private static bool s_inited = false;
        private static Dictionary<string, object>  s_dict;

        public static string Path;

        private static FontField[] _Fonts;
        public static FontField[] Fonts
        {
            get
            {
                CheckAndLoad();
                if (s_dict == null)
                {
                    return null;
                }

#if !UNITY_EDITOR
                if (_Fonts != null)
                {
                    return _Fonts;
                }
#endif

                List<System.Object> list = s_dict["fonts"] as List <System.Object>;
                int count = list.Count;
                _Fonts = new FontField[count];

                for (int i = 0; i < count; ++i)
                {
                    Dictionary<string, object> dict = list[i] as Dictionary<string, object>;
                    if (dict.Count != 1)
                    {
                        Debug.LogWarningFormat("ui配置表fonts子项长度不为1\t", dict.Count);
                        continue;
                    }

                    foreach(var itr in dict)
                    {
                        FontField f = new FontField();
                        f.Name = itr.Key;
                        f.Path = itr.Value as string;
                        _Fonts[i] = f;
                    }
                }

                return _Fonts;
            }
        }

        private static void CheckAndLoad()
        {
#if !UNITY_EDITOR
            if (s_inited == false)
            {
                s_inited = true;
#endif
                if (string.IsNullOrEmpty(Path))
                {
                    // 默认在Asset目录下查找UIConfig.json
                    Path = Application.dataPath + "/UIConfig.json";
                }

                // Debug.LogFormat("UIConfig {0}", Path);

                // todo: 尝试对比文件最后的修改日期，以减少IO次数

                if (File.Exists(Path))
                {
                    try
                    {
                        string txt = File.ReadAllText(Path);
                        s_dict = MiniJSON.Json.Deserialize(txt) as Dictionary<string, object>;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarningFormat("加载ui配配置文本失败 {0}", e.ToString());
                    }
                }
                else
                {
                    Debug.LogWarning("找不到ui配置表");
                }
#if !UNITY_EDITOR
            }
#endif
        }

        public static string GetFontPath(string alias)
        {
            if (Fonts == null || Fonts.Length <= 0)
            {
                return null;
            }

            for (int i = 0, iMax = Fonts.Length; i < iMax; ++i)
            {
                if (Fonts[i].Name == alias)
                {
                    return Fonts[i].Path;
                }
            }

            return null;
        }
    }
}
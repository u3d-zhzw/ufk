using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFK
{
    public static class ResMgr
    {
        private static string FONT_SRC_DIR = "Assets/ArtRes/Font";
        private static Dictionary<string, Font> s_dictFonts = new Dictionary<string, Font>();

        public delegate void ResFontEvt(string name, Font go);

        public static void LoadFont(string name, ResFontEvt evt)
        {
#if UNITY_EDITOR
            if (s_dictFonts.ContainsKey(name) == false || s_dictFonts[name] == null)
            {
                string file = string.Format("{0}/{1}.ttf", FONT_SRC_DIR, name);
                // if (System.IO.File.Exists(file) == false)
                // {
                //     if (evt != null)
                //     {
                //         evt(name, null);
                //     }
                //     return;
                // }
                s_dictFonts[name] = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(file);
            }

            if (evt != null)
            {
                evt(name, s_dictFonts[name]);
            }
#else
            // todo: ab加载过程
            throw new System.NotImplementedException("未实现ab加载");
#endif
        }
    }
}
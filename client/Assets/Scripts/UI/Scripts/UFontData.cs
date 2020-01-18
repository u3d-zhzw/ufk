using System;

using UnityEngine;
using UnityEngine.UI;

namespace UFK
{
    [Serializable]
    public class UFontData : FontData
    {
        /// <summary>
        /// 字库索引.
        /// todo：参照哪个配置
        /// </summary>
        [SerializeField]
        public string FontAlias;

        /// <summary>
        /// Get a font data with sensible defaults.
        /// </summary>
        public static UFontData defaultuFontData
        {
            get
            {
                UFontData fontData = new UFontData();

                fontData.fontSize = 14;
                fontData.lineSpacing = 1f;
                fontData.fontStyle = FontStyle.Normal;
                fontData.bestFit = false;
                fontData.minSize = 10;
                fontData.maxSize = 40;
                fontData.alignment = TextAnchor.UpperLeft;
                fontData.horizontalOverflow = HorizontalWrapMode.Wrap;
                fontData.verticalOverflow = VerticalWrapMode.Truncate;
                fontData.richText = true;
                fontData.alignByGeometry = false;

                return fontData;
            }
        }

        // private void UpdateFont()
        // {
        //     if (string.IsNullOrEmpty(m_FontAlias))
        //     {
        //         base.font = null;
        //         return;
        //     }

        //     string fontName = UIConfig.GetFontPath(m_FontAlias);
        //     ResMgr.LoadFont(fontName, (string name, Font font) =>
        //     {
        //         if (font != null)
        //         {
        //             base.font = font;
        //             if (base.font != null && !base.font.dynamic)
        //             {
        //                 base.fontSize = base.font.fontSize;
        //             }
        //         }
        //         else
        //         {
        //             Debug.LogWarningFormat("{0}加载失败", name);
        //         }
        //     });
        // }
    }
}

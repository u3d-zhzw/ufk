using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UFK
{
    public class UText : Text
    {   
        [SerializeField] private UFontData m_UFontData = UFontData.defaultuFontData;

        public string FontAlias
        {
            get
            {
                return this.m_UFontData.FontAlias;
            }

            set
            {
                this.m_UFontData.FontAlias = value;
                this.UpdateFontData();
            }
        }

        private Color m_tmpHexColor = Color.white;
        private int HEX_COLOR_MASK = 0x000000FF;

        public override string text
        {
            get { return base.text; }
            set
            {
                // todo: 过滤敏感字词
                base.text = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            this.UpdateFontData();
        }

        public void SetHexColor(int value)
        {
            m_tmpHexColor.r = (value & HEX_COLOR_MASK) / 255f;
            m_tmpHexColor.g = ((value >> 8) & HEX_COLOR_MASK) / 255f;
            m_tmpHexColor.b = ((value >> 16) & HEX_COLOR_MASK) / 255f;
            m_tmpHexColor.a = ((value >> 24) & HEX_COLOR_MASK) / 255f;
            
            base.color = m_tmpHexColor;
        }

        public void SetHexColor(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogWarningFormat("{0} is not a hex color", value);
                return;
            }

            int reuslt = 0;
            if (int.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out reuslt))
            {
                this.SetHexColor(reuslt);
            }
            else
            {
                Debug.LogWarningFormat("fail to set {0} to hex color", value);
            }
        }

        public void UpdateFontData()
        {
            if (string.IsNullOrEmpty(FontAlias))
            {
                base.font = null;
                return;
            }

            string fontName = UIConfig.GetFontPath(FontAlias);
            ResMgr.LoadFont(fontName, (string name, Font font) =>
            {
                if (font != null)
                {
                    base.font = font;
                }
                else
                {
                    Debug.LogWarningFormat("{0}加载失败", name);
                }
            });

            base.fontSize = m_UFontData.fontSize;
            base.lineSpacing = m_UFontData.lineSpacing;
            base.fontStyle = m_UFontData.fontStyle;
            base.resizeTextForBestFit = m_UFontData.bestFit;
            base.resizeTextMinSize = m_UFontData.minSize;
            base.resizeTextMaxSize = m_UFontData.maxSize;
            base.alignment = m_UFontData.alignment;
            base.horizontalOverflow = m_UFontData.horizontalOverflow;
            base.verticalOverflow = m_UFontData.verticalOverflow;
            base.supportRichText = m_UFontData.richText;
            base.alignByGeometry = m_UFontData.alignByGeometry;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            this.UpdateFontData();
        }
#endif
    }
}
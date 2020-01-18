using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UFK
{
    public class UImage : Image
    {
        // todo: 配置默认图片
        [SerializeField]
        private Sprite m_DefaultSprite = null;

        [SerializeField]
        private bool m_HideWhenNoneSprite = true;

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            // if (this.activeSprite == null)
            if (m_HideWhenNoneSprite && base.sprite == null)
            {
                toFill.Clear();
                return;
            }

            base.OnPopulateMesh(toFill);
        }

        public void SetImage(string sprite, System.Action cbDone = null)
        {
            // todo: load image finish
            // if (cbDone != null) { cbDone() ;}
        }
    }
}
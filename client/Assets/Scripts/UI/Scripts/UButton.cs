using UnityEngine;
using UnityEngine.UI;

namespace UFK
{
    public class UButton : Button
    {
        

        protected override void OnDestroy()
        { 
            base.OnDestroy();

            base.onClick.RemoveAllListeners();
        }
    }
}
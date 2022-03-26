using System.Collections;
using System.Collections.Generic;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.UI
{
    public class ScreenShading : MonoBehaviour
    {
        public GameObject ShadePanel;
        public GameObject BlurPanel;

        public Material Blur;

        

        private const float Duration = 1f;
        private const float BlurStrenght = 4f;
        private const float ShadeStrenght = 1f;

        private Image ShadeImage;


        private void Awake()
        {
            ShadeImage = ShadePanel.GetComponent<Image>();
            UnshadeInstant();
        }

        public IPromise Shade(bool shade = true, float duration = Duration, float strenght = ShadeStrenght)
        {
            var deferred = new Deferred();

            float shadeStart, shadeFinish, blurStart, blurFinish;

            if (shade)
            {
                shadeStart = ShadeImage.color.a;
                shadeFinish = strenght;
                blurStart = Blur.GetFloat("_Size");
                blurFinish = BlurStrenght;
            }
            else
            {
                shadeStart = ShadeImage.color.a;
                shadeFinish = 0f;
                blurStart = Blur.GetFloat("_Size");
                blurFinish = 0f;
            }

            BlurPanel.SetActive(true);
            ShadePanel.SetActive(true);

            var shading = new Tween<float>();

            shading.SetAutoCleanup(true)
                .SetStartValue(shadeStart)
                .SetEndValue(shadeFinish)
                .SetDuration(duration)
                .OnExecute(current =>
                {
                    ShadeImage.color = new Color(0, 0, 0, current);
                })
                .OnCompleted(t =>
               {
                   if (!shade) SetPanelActive(false);
                   deferred.Resolve();
               })
                .Play();

            var bluring = new Tween<float>();

            bluring.SetAutoCleanup(true)
                .SetStartValue(blurStart)
                .SetEndValue(blurFinish)
                .SetDuration(duration)
                .OnExecute(current =>
                {
                    Blur.SetFloat("_Size", current);
                })
                .Play();

            return deferred;
        }

        public void ShadeInstant(float shadeStrenght = 1f)
        {
            Blur.SetFloat("_Size", BlurStrenght);
            ShadeImage.color = new Color(0, 0, 0, shadeStrenght);

            SetPanelActive(true);
        }

        public void UnshadeInstant()
        {
            Blur.SetFloat("_Size", 0);
            ShadeImage.color = new Color(0, 0, 0, 0);

            SetPanelActive(false);
        }

        public void BlockScreen(bool block)
        {
            ShadeImage.color = new Color(0, 0, 0, 0);
            ShadePanel.SetActive(block);
        }

        private void SetPanelActive(bool isActive)
        {
            BlurPanel.SetActive(isActive);
            ShadePanel.SetActive(isActive);
        }
    }
}
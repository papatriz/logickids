using DaikonForge.Tween;
using Orbox.Async;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.UI
{
    public class MazeProgressBar : MonoBehaviour
    {
        public Image BarImage;

        public Transform Transform5Stars;
        public Transform Transform4Stars;
        public Transform Transform3Stars;
        public Transform Transform2Stars;

        private int CurrentStep;
        private int TotalSteps;

        private int Step5star;
        private int Step4star;
        private int Step3star;
        private int Step2star;

        private Color Color5s = new Color32(84, 225, 0, 170);
        private Color Color4s = new Color32(182, 225, 0, 170);
        private Color Color3s = new Color32(217, 225, 0, 170);
        private Color Color2s = new Color32(217, 170, 0, 170);
        private Color Color1s = new Color32(217, 0, 0, 170);

        public void Setup(int maxSteps, int step5star, int step4star, int step3star, int step2star)
        {
            TotalSteps = maxSteps;
            Step5star = step5star;
            Step4star = step4star;
            Step3star = step3star;
            Step2star = step2star;

            var lenght = BarImage.rectTransform.sizeDelta.x;

            var x5stars = ((float)(TotalSteps - Step5star) / (float)TotalSteps) * lenght - lenght / 2;

            Transform5Stars.localPosition = new Vector2(x5stars, 0);

            var x4stars = ((float)(TotalSteps - Step4star) / (float)TotalSteps * lenght) - lenght / 2;
            Transform4Stars.localPosition = new Vector2(x4stars, 0);

            var x3stars = ((float)(TotalSteps - Step3star) / (float)TotalSteps * lenght) - lenght / 2;
            Transform3Stars.localPosition = new Vector2(x3stars, 0);

            var x2stars = ((float)(TotalSteps - Step2star) / (float)TotalSteps * lenght) - lenght / 2;
            Transform2Stars.localPosition = new Vector2(x2stars, 0);

            CurrentStep = 0;

            BarImage.color = Color5s;
            BarImage.fillAmount = ValuefromSteps(CurrentStep);
        }

        public IPromise SetNewValue(int steps) // toDo: Color gradient
        {
            var deferred = new Deferred();
            

            var tween = new Tween<float>();
            var start = ValuefromSteps(CurrentStep);
            var end = ValuefromSteps(steps);

            var startColor = ColorFromSteps(CurrentStep);
            var endColor = ColorFromSteps(steps);

            var diff = Mathf.Abs(CurrentStep - steps);
            var duration = 0.3f * diff;

            tween.SetStartValue(start)
                .SetEndValue(end)
                .SetDuration(duration)
                .OnExecute(current =>
                {
                    var lerp = current - start;
                    BarImage.color = Color.Lerp(startColor, endColor, lerp);
                    BarImage.fillAmount = current;
                })
                .OnCompleted(t =>
                {
                    CurrentStep = steps;
                    deferred.Resolve();
                })
                .Play();

            return deferred;

        }

        private Color ColorFromSteps(int steps) // toDO: когда нефиг делать будет, грамотно сделать ситуацию, когда начальный и конечный шаг в разных диапазонах
        {
            var result = new Color();
            var alpha = Color5s.a;

            float totalSteps = 0;
            float r1, g1, b1, r0, g0, b0;
            float r, g, b;

            result = Color1s;

            if (steps <= Step5star)
            {
                totalSteps = Step5star;

                float t = (totalSteps - (float)steps) / totalSteps;
                r0 = Color4s.r;
                g0 = Color4s.g;
                b0 = Color4s.b;
                r1 = Color5s.r;
                g1 = Color5s.g;
                b1 = Color5s.b;

                r = r1 * t + r0 * (1 - t);
                g = g1 * t + g0 * (1 - t);
                b = b1 * t + b0 * (1 - t);

                result = new Color(r, g, b, alpha);
            }
            else
                if(steps > Step5star && steps <= Step4star)
            {
                totalSteps = Step4star- Step5star;
                steps -= Step5star;

                float t = (totalSteps - (float)steps) / totalSteps;
                r0 = Color3s.r;
                g0 = Color3s.g;
                b0 = Color3s.b;
                r1 = Color4s.r;
                g1 = Color4s.g;
                b1 = Color4s.b;

                r = r1 * t + r0 * (1 - t);
                g = g1 * t + g0 * (1 - t);
                b = b1 * t + b0 * (1 - t);

                result = new Color(r, g, b, alpha);
            }
            else
                if(steps > Step4star && steps <= Step3star)
            {
                totalSteps = Step3star - Step4star;
                steps -= Step4star;

                float t = (totalSteps - (float)steps) / totalSteps;
                r0 = Color2s.r;
                g0 = Color2s.g;
                b0 = Color2s.b;
                r1 = Color3s.r;
                g1 = Color3s.g;
                b1 = Color3s.b;

                r = r1 * t + r0 * (1 - t);
                g = g1 * t + g0 * (1 - t);
                b = b1 * t + b0 * (1 - t);

                result = new Color(r, g, b, alpha);
            }
            else
                if(steps > Step3star && steps <= Step2star)
            {
                totalSteps = Step2star - Step3star;
                steps -= Step3star;

                float t = (totalSteps - (float)steps) / totalSteps;

                Debug.Log("Total: " + totalSteps + " Steps: " + steps + " t=" + t);

                r0 = Color1s.r;
                g0 = Color1s.g;
                b0 = Color1s.b;
                r1 = Color2s.r;
                g1 = Color2s.g;
                b1 = Color2s.b;

                r = r1 * t + r0 * (1 - t);
                g = g1 * t + g0 * (1 - t);
                b = b1 * t + b0 * (1 - t);

                result = new Color(r, g, b, alpha);
            }

            return result;
        }

        private float ValuefromSteps(int steps)
        {
            var result = ((float)TotalSteps - (float)steps) / (float)TotalSteps;
            if (result <= 0.01f) result = 0.01f;

            return result;
        }

    }
}

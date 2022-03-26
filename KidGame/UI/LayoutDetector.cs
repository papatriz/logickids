using UnityEngine;

namespace KidGame.UI
{

    public enum Layouts
    {
        Tablet,
        Smartphone
    }

    public class LayoutDetector : ILayoutDetector
    {
        private IGame Game;

        public LayoutDetector(IGame game)
        {
            Game = game;
        }

        public Layouts Layout
        {
            get
            {
                float screenWidthInch = Screen.width / Screen.dpi;
                float screenHeightInch = Screen.height / Screen.dpi;

                float aspectRatio = screenWidthInch / screenHeightInch;

                Game.AspectRatio = aspectRatio;

                var layout = Layouts.Smartphone;

                if (aspectRatio < 1.67f)
                    layout = Layouts.Tablet;

                var msg = string.Format("Device layout: {0} Aspect Ratio: {1}", layout, aspectRatio);
                Debug.Log(msg);

                return layout;
            }
        }

    }
}
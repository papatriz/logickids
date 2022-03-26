
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.UI
{
	public class ButtonClickEffect : MonoBehaviour
	{
		public bool Downward = true;
		public float Timing = 0.1f;

		private ISoundManager SoundManager;

		private bool IsTweening;

		void Awake()
		{
			SoundManager = CompositionRoot.GetSoundManager();
			IsTweening = false;

			GetComponent<Button>().onClick.AddListener(() => PlayEffect());
		}

		public void PlayEffect()
		{
			SoundManager.Play(Sounds.ECommon.Click);

			if (!IsTweening)
			{
				IsTweening = true;

				var targetSize = (Downward) ? 0.9f : 1.1f;
				var targetVector = new Vector3(targetSize, targetSize, 1);

				transform.TweenScale()
						 .SetAutoCleanup(true)
						 .SetEndValue(targetVector)
						 .SetDuration(Timing)
						 .SetLoopType(DaikonForge.Tween.TweenLoopType.Pingpong)
						 .SetLoopCount(2)
						 .OnCompleted(t => IsTweening = false)
						 .Play();
			}
		}
	}
}

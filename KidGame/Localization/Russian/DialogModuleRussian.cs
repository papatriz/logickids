using UnityEngine;
using Orbox.Localization;


namespace KidGame.Localization
{
	public class DialogModuleRussian : LocalizationModule<DialogKeys>
	{
		public DialogModuleRussian() : base(SystemLanguage.Russian)
		{

			this[DialogKeys.BuyTrialHeader] = "Получите полный доступ на 3 дня бесплатно, затем всего {0} / месяц";
			this[DialogKeys.BuyPaidHeader] = "Получите полный доступ всего за {0} в месяц";
			this[DialogKeys.BuyTrialButton] = "Начните пробный период!";
			this[DialogKeys.BuyPaidButton] = "Получите полный доступ!";
			this[DialogKeys.RestorePurchase] = "Восстановить";
			this[DialogKeys.Music] = "Музыка";
			this[DialogKeys.Sound] = "Звуки";
			this[DialogKeys.Close] = "Закрыть";

		}



	}
}
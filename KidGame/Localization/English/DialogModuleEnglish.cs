using UnityEngine;
using Orbox.Localization;


namespace KidGame.Localization
{
	public class DialogModuleEnglish : LocalizationModule<DialogKeys>
	{
		public DialogModuleEnglish() : base(SystemLanguage.English)
		{

			this[DialogKeys.BuyTrialHeader] = "Get free full access for 3 days, then only {0} / month";
			this[DialogKeys.BuyPaidHeader] = "Get full access for only {0}/month";
			this[DialogKeys.BuyTrialButton] = "START FREE TRIAL!";
			this[DialogKeys.BuyPaidButton] = "GET FULL ACCESS!";
			this[DialogKeys.RestorePurchase] = "RESTORE";
			this[DialogKeys.Music] = "Music";
			this[DialogKeys.Sound] = "Sound";
			this[DialogKeys.Close] = "Close";
		}



	}
}
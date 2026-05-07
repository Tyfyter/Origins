using CalamityMod.World;
using Origins.World;
using System.Reflection;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.CrossMod.Calamity {
	[ExtendsFromMod("CalamityMod")]
	class SkipUnreachableEvilBosses : ModType, IBroken {
		static string IBroken.BrokenReason => "Use event directly";
		static void Trigger() {
			AerialiteOreGen.Enchant();
			const string key = "Mods.CalamityMod.Status.Progression.SkyOreText";
			switch (Main.netMode) {
				case NetmodeID.SinglePlayer:
				Main.NewText(Language.GetTextValue(key), Color.Cyan);
				break;
				case NetmodeID.Server:
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key), Color.Cyan);
				break;
			}
		}
		public override void SetupContent() {
			if (typeof(ProgressFlag).GetEvent("OnSet") is EventInfo @event) {
				@event.AddEventHandler(ProgressFlags.DownedDefiledAmalgamation, Trigger);
				@event.AddEventHandler(ProgressFlags.DownedWorldCracker, Trigger);
				@event.AddEventHandler(ProgressFlags.DownedTrenchmaker, Trigger);
			}
		}
		protected override void Register() { }
	}
}

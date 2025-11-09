using Origins.Dev;
using Origins.Items.Materials;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Waist)]
	public class Eitrite_Watch : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Info
		];
		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 28;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			
		}
		public override void UpdateInfoAccessory(Player player) {
			Max(ref player.accWatch, 4);
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<Eitrite_Bar>(10)
			.AddIngredient(ItemID.Chain, 5)
			.AddTile(TileID.Tables)
			.AddTile(TileID.Chairs)
			.Register();
	}
	public class Eitrite_Watch_Display : GlobalInfoDisplay {
		static List<NoveltyWatchTime> noveltyTimes = [];
		public override void SetStaticDefaults() {
			noveltyTimes = new TopoSort<NoveltyWatchTime>(noveltyTimes,
				mode => mode.SortAfter(),
				mode => mode.SortBefore()
			).Sort();
			for (int i = 0; i < noveltyTimes.Count; i++) noveltyTimes[i].SetStaticDefaults();
		}
		public override void ModifyDisplayParameters(InfoDisplay currentDisplay, ref string displayValue, ref string displayName, ref Color displayColor, ref Color displayShadowColor) {
			if (currentDisplay == InfoDisplay.Watches && Main.LocalPlayer.accWatch >= 4) {
				string AM = Language.GetTextValue("GameUI.TimeAtMorning");
				string PM = Language.GetTextValue("GameUI.TimePastMorning");
				Regex regex = new($"^\\d{{1,2}}:\\d\\d ({AM}|{PM})$");
				if (!regex.IsMatch(displayValue)) return;
				ContentExtensions.GetDisplayedDayTime(out string hours, out string minutes, out string seconds, out string half);
				displayValue = $"{hours}:{minutes}:{seconds}{half}";
				for (int i = 0; i < noveltyTimes.Count; i++) {
					if (noveltyTimes[i].IsActive) {
						displayValue = noveltyTimes[i].GetText(displayValue, hours, minutes, seconds, half);
						break;
					}
				}
			}
		}
		public abstract class NoveltyWatchTime : ILoadable {
			public LocalizedText Text { get; protected set; }
			public void Load(Mod mod) {
				noveltyTimes.Add(this);
				Load();
				Text ??= mod.GetLocalization($"NoveltyWatchTime.{GetType().Name}", () => "{0}");
			}
			public void Unload() { }
			public virtual void Load() { }
			public virtual void SetStaticDefaults() { }
			public abstract bool IsActive { get; }
			public virtual string GetText(string normalText, string hours, string minutes, string seconds, string half) => Text.Format(normalText, hours, minutes, seconds, half);
			public virtual IEnumerable<NoveltyWatchTime> SortAfter() => this is ImpendingDoom ? [] : [ModContent.GetInstance<ImpendingDoom>()];
			public virtual IEnumerable<NoveltyWatchTime> SortBefore() => [];
		}
		public class PartyTime : NoveltyWatchTime {
			public override bool IsActive => BirthdayParty.PartyIsUp;
		}
		public class ImpendingDoom : NoveltyWatchTime {
			public override bool IsActive => NPC.MoonLordCountdown > 0;
			public override string GetText(string normalText, string hours, string minutes, string seconds, string half) => Text.Format(float.Ceiling(NPC.MoonLordCountdown / 60f));
			public override IEnumerable<NoveltyWatchTime> SortBefore() => noveltyTimes.Except([this]);
		}
	}
}

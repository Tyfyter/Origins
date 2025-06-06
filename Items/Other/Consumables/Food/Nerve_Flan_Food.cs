using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Origins.Items.Weapons.Magic;
using Origins.Buffs.Food;

namespace Origins.Items.Other.Consumables.Food {
	public class Nerve_Flan_Food : ModItem {
		public override LocalizedText DisplayName => Language.GetText(Mod.GetLocalizationKey($"{LocalizationCategory}.{nameof(Nerve_Flan)}.{nameof(DisplayName)}"));
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Nerve_Flan>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Nerve_Flan>()] = Type;
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(186, 179, 106),
				new Color(175, 157, 75),
				new Color(136, 116, 62)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				20, 18,
				ModContent.BuffType<Nerve_Flan_Food_Buff>(),
				60 * 60 * 8
			);
			Item.value = Item.sellPrice(silver: 1);
			Item.rare = ItemRarityID.Blue;
		}
	}
}

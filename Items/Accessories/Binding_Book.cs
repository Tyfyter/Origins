using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Binding_Book : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.ManaShielding,
			WikiCategories.MagicBoostAcc
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 5, silver: 20);
			Item.rare = ItemRarityID.LightPurple;
			Item.expert = true;
		}
        public override void AddRecipes() {
            CreateRecipe()
            .AddIngredient(ItemID.CelestialCuffs)
            .AddIngredient(ModContent.ItemType<Refactoring_Pieces>())
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
        }
        public override void UpdateAccessory(Player player, bool hideVisual) {
			player.manaMagnet = true;
			player.magicCuffs = true;
			player.statManaMax2 += 20;
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.refactoringPieces = true;
			originPlayer.manaShielding += 0.1f;
			player.endurance += (1 - player.endurance) * 0.1f;
			if (!hideVisual) UpdateVanity(player);
		}
		public override void UpdateVanity(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.bindingBookVisual = true;
			Physics.Chain[] chains = originPlayer.bindingBookChains;
			for (int i = 0; i < chains.Length; i++) {
				Physics.Chain chain = chains[i];
				if (chain is null || chain.links[0].position.HasNaNs() || chain.links[0].position.DistanceSQ(player.position) > 512 * 512) {
					Vector2 offset = Vector2.Zero;
					Vector2 gravMod = Vector2.One;
					switch (i) {
						case 0:
						offset = new Vector2(8, -4);
						gravMod = new(1.1f, 0.7f);
						break;

						case 1:
						offset = new Vector2(-8, 0);
						gravMod.X = -1;
						break;

						case 2:
						offset = new Vector2(6, 10);
						gravMod = new(0.5f, 1.2f);
						break;
					}
					var anchor = new Physics.EntityAnchorPoint() {
						entity = player,
						offset = offset
					};
					const float spring = 0.5f;
					chains[i] = chain = new Physics.Chain() {
						anchor = anchor,
						links = [
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 8, [new Physics.EntityDirectionGravity(new Vector2(0.12f, -0.28f) * gravMod, player)], drag: 0.93f, spring: spring)
						]
					};
				}
				int kMax = 2;
				for (int k = 0; k < kMax; k++) {
					Vector2[] deltas = chain.Update();
					if (OriginsModIntegrations.CheckAprilFools()) {
						for (int j = 0; j < deltas.Length; j++) {
							player.velocity -= deltas[j] * 0.004f;
						}
					}
					if (kMax < 20 && (chain.links[0].position - chain.anchor.WorldPosition).LengthSquared() > 128 * 128) {
						kMax++;
					}
				}
			}
		}
		public override void UpdateItemDye(Player player, int dye, bool hideVisual) => player.OriginPlayer().bindingBookDye = dye;
	}
}

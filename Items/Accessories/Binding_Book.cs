using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Binding_Book : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat",

		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 12);
			Item.rare = ItemRarityID.LightPurple;
		}
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CelestialCuffs);
            recipe.AddIngredient(ModContent.ItemType<Reshaping_Chunk>());
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
        public override void UpdateAccessory(Player player, bool hideVisual) {
			player.manaMagnet = true;
			player.magicCuffs = true;
			player.statManaMax2 += 20;
			player.GetModPlayer<OriginPlayer>().reshapingChunk = true;
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
						links = new Physics.Chain.Link[] {
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 8, new Physics.Gravity[] { new Physics.EntityDirectionGravity(new Vector2(0.12f, -0.28f) * gravMod, player) }, drag: 0.93f, spring: spring)
						}
					};
				}
				Vector2[] deltas = chain.Update();
				if (OriginsModIntegrations.CheckAprilFools()) {
					for (int j = 0; j < deltas.Length; j++) {
						player.velocity -= deltas[j] * 0.004f;
					}
				}
			}
		}
	}
}

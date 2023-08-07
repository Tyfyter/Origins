using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Eyndum_Scar : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Eyndum Scar");
			// Tooltip.SetDefault("A very lame end-game sword");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Katana);
			Item.damage = 407;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = false;
			Item.noMelee = false;
			Item.width = 70;
			Item.height = 70;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.knockBack = 9.5f;
			Item.shoot = ProjectileID.None;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = CrimsonRarity.ID;
			Item.autoReuse = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eyndum_Bar>(), 30);
			//recipe.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 20); Undecided material
			//recipe.AddTile(TileID.Anvils); Omni-printer
			recipe.Register();
		}
		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
			if (!(target.boss || NPCID.Sets.ShouldBeCountedAsBoss[target.type])) {
				int quarterHealth = target.lifeMax / 4;
				if (target.life <= quarterHealth) {
					modifiers.SetInstantKill();
				}
			}
		}
	}
}

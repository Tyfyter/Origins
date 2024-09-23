using Origins.Items.Accessories;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Microsoft.Xna.Framework;
namespace Origins.Items.Weapons.Melee {
	public class The_Bird : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Sword",
			"DeveloperItem",
			"ReworkExpected"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WoodenSword);
			Item.useStyle = ItemUseStyleID.Swing;
			Item.damage = 99;
			Item.useAnimation = Item.useTime = 20;
			Item.rare = ItemRarityID.Cyan;
			Item.knockBack = 16;
		}
		public override void AddRecipes() {
			//Recipe.Create(Type)
			//.AddIngredient(ModContent.ItemType<Baseball_Bat>())
			//.AddIngredient(ModContent.ItemType<Razorwire>())
			//.AddCondition(player.name == "Pandora");
			//.Register();
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void UseItemFrame(Player player) {
			
		}
		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			Vector2 knockback = hit.GetKnockbackFromHit(false, false, yMult: 1);
			if (player.altFunctionUse == 2) {
				target.velocity = new(knockback.Y * player.direction * 0.25f, -knockback.X);
			} else {
				target.velocity = new(knockback.X * player.direction, knockback.Y * (target.noGravity ? -0.5f : -0.75f));
			}
		}
	}
}

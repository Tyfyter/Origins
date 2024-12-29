using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	public class Unmisfire : ModItem {
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				global.shouldUnmiss = true;
			});
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.DemonBow);
			Item.damage = 12;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 5;
			Item.shootSpeed = 8;
			Item.noMelee = true;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.width = 50;
			Item.height = 10;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 12)
			.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}

﻿using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Melee
{
    public class Nineball : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Yoyo"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Code1);
			Item.damage = 18;
			Item.crit = 5;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shoot = ModContent.ProjectileType<Nineball_P>();
			Item.knockBack = 4.5f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 99, copper: 99);
		}
        public override void AddRecipes()
        {
            Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 9)
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
    public class Nineball_P : ModProjectile
    {
        
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.WoodYoyo);
            ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 9f;
            ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 144;
            ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 9;
        }
        public override void AI()
        {
            Dust dust = Main.dust[Terraria.Dust.NewDust(Projectile.Center, 0, 0, DustID.GemAmethyst, 0f, 0f, 125, new Color(200, 100, 0), 0.36f)];
            dust.noGravity = true;
            dust.velocity *= 1.5f;
        }
    }
}

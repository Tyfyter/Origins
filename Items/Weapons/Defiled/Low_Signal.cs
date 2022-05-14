using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.NPCs;
using Origins.Projectiles.Weapons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Defiled {
	public class Low_Signal : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Low Signal");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
			item.damage = 60;
			item.magic = true;
			item.mana = 7;
            item.noMelee = true;
            item.noUseGraphic = false;
			item.width = 30;
			item.height = 36;
			item.useTime = 30;
			item.useAnimation = 30;
			item.useStyle = 5;
			item.knockBack = 5;
            item.shoot = ModContent.ProjectileType<Low_Signal_P>();
            item.shootSpeed = 14f;
			item.value = 5000;
            item.useTurn = false;
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(8, 0);
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			return true;
		}
	}
    public class Low_Signal_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Defiled/Infusion_P";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Low Signal");
		}
		public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			projectile.ranged = false;
			projectile.magic = true;
			projectile.timeLeft = 40;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 60;
			projectile.aiStyle = 0;
			projectile.width = 8;
			projectile.height = 8;
			projectile.penetrate = 1;
			projectile.hide = true;
		}
		public override void Kill(int timeLeft) {
			int[] immune = projectile.localNPCImmunity.ToArray();
			Projectile.NewProjectileDirect(
				projectile.Center,
				Vector2.Zero,
				ModContent.ProjectileType<Defiled_Spike_Explosion>(),
				projectile.damage,
				0,
				projectile.owner,
				7).localNPCImmunity = immune;
		}
	}
}

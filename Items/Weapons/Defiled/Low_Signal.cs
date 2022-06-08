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
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
			Item.damage = 60;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 7;
            Item.noMelee = true;
            Item.noUseGraphic = false;
			Item.width = 30;
			Item.height = 36;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
            Item.shoot = ModContent.ProjectileType<Low_Signal_P>();
            Item.shootSpeed = 14f;
			Item.value = 5000;
            Item.useTurn = false;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(8, 0);
		}
	}
    public class Low_Signal_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Defiled/Infusion_P";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Low Signal");
		}
		public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 40;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
			Projectile.aiStyle = 0;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = 1;
			Projectile.hide = true;
		}
		public override void Kill(int timeLeft) {
			int[] immune = Projectile.localNPCImmunity.ToArray();
			Projectile.NewProjectileDirect(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				Vector2.Zero,
				ModContent.ProjectileType<Defiled_Spike_Explosion>(),
				Projectile.damage,
				0,
				Projectile.owner,
				7).localNPCImmunity = immune;
		}
	}
}

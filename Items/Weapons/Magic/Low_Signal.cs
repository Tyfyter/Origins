using Microsoft.Xna.Framework;
using Origins.Projectiles.Weapons;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Low_Signal : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Low Signal");
			Item.staff[Type] = true;
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 40;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 9;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.width = 30;
			Item.height = 36;
			Item.useTime = 46;
			Item.useAnimation = 46;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
			Item.shoot = ModContent.ProjectileType<Low_Signal_P>();
			Item.shootSpeed = 14f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = Origins.Sounds.DefiledIdle.WithPitchRange(-0.6f, -0.4f);
			Item.autoReuse = true;
			Item.glowMask = glowmask;
		}
	}
	public class Low_Signal_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Infusion_P";
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Nerve Signal");
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC npc) {
				Projectile.npcProj = true;
				Projectile.hostile = false;//!npc.friendly;
				Projectile.friendly = npc.friendly;
				if ((Main.masterMode || Main.expertMode) && !npc.friendly) {
					Projectile.hostile = true;
				}
				Projectile.DamageType = DamageClass.Default;
			}
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
		public override void AI() {
			Dust.NewDustPerfect(Projectile.Center, DustID.AncientLight, default, newColor: Color.White, Scale: 0.5f + (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.15f);
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			// sort of oddly phrased, and if there's a remotely simple way to add custom death reasons for projectiles this isn't it
			// as seen with this functionally identical code the reason is never actually used
			PlayerDeathReason reason = PlayerDeathReason.ByCustomReason(target.name + " watched defiled spikes come out of their body");
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

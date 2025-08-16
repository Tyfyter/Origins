using Origins.Buffs;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Materials;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Felnum_Hamaxe : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"ToolWeapon"
		];
		public override void SetStaticDefaults() {
			Origins.DamageBonusScale[Type] = 1.5f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MoltenHamaxe);
			Item.damage = 27;
			Item.DamageType = DamageClass.Melee;
			Item.pick = 0;
			Item.hammer = 65;
			Item.axe = 22;
			Item.width = 42;
			Item.height = 38;
			Item.useTime = 13;
			Item.shoot = ModContent.ProjectileType<Felnum_Hamaxe_P>();
			Item.shootSpeed = 9;
			Item.useAnimation = 25;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 40);
			Item.UseSound = SoundID.Item1;
			Item.rare = ItemRarityID.Green;
		}
		public override float UseTimeMultiplier(Player player) {
			return (player.pickSpeed - 1) * 0.75f + 1;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 16)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool MeleePrefix() => true;
		public override bool AltFunctionUse(Player player) => true;
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] <= 0;
		}
		public override void UseItemFrame(Player player) {
			if (player.altFunctionUse == 2) player.itemLocation = Vector2.Zero;
		}
		public override bool? CanHitNPC(Player player, NPC target) {
			if (player.altFunctionUse == 2) return false;
			return null;
		}
		public override bool CanHitPvp(Player player, Player target) {
			return player.altFunctionUse != 2;
		}
		public override bool CanShoot(Player player) => player.altFunctionUse == 2 && player.ItemUsesThisAnimation == 0;
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			Static_Shock_Debuff.Inflict(target, Main.rand.Next(120, 210));
		}
	}
	public class Felnum_Hamaxe_P : ModProjectile {
		public override string Texture => typeof(Felnum_Hamaxe).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Melee;
			Projectile.friendly = true;
			Projectile.width = 42;
			Projectile.height = 42;
			Projectile.aiStyle = ProjAIStyleID.ThrownProjectile;
			Projectile.penetrate = -1;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Static_Shock_Debuff.Inflict(target, Main.rand.Next(120, 210));
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 24;
			height = 24;
			return true;
		}
		public override void AI() {
			Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
			return true;
		}
		public override void OnKill(int timeLeft) {
			Dust.NewDustPerfect(Main.player[Projectile.owner].MountedCenter, ModContent.DustType<Static_Shock_Arc_Dust>(), Projectile.Center).alpha = 3;
		}
	}
}

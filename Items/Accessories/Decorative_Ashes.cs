using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace Origins.Items.Accessories;
[AutoloadEquip(EquipType.Back)]
public class Decorative_Ashes : ModItem {
	public static int ThornsCount => Main.rand.Next(8, 13);
	public override void SetDefaults() {
		Item.DefaultToAccessory();
		Item.rare = ItemRarityID.Yellow;
		Item.master = true;
		Item.damage = 60;
		Item.DamageType = DamageClasses.Explosive;
		Item.shoot = ModContent.ProjectileType<Decorative_Ashes_Missile>();
		Item.knockBack = 1;
		Item.useTime = 8;
		Item.useAnimation = Item.useTime;
		Item.reuseDelay = 30;
		Item.useLimitPerAnimation = 8;
		Item.value = Item.sellPrice(gold: 5);
	}
	public override void UpdateAccessory(Player player, bool hideVisual) => player.OriginPlayer().decorativeAshes = Item;
	public override int ChoosePrefix(UnifiedRandom rand) {
		return OriginExtensions.GetAllPrefixes(Item, rand, (PrefixCategory.AnyWeapon, 1), (PrefixCategory.Accessory, 2));
	}
	public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
		if (Main.gameMenu) return;
		if (OriginPlayer.LocalOriginPlayer?.decorativeAshes is null) return;
		float inventoryScale = Main.inventoryScale;
		ChatManager.DrawColorCodedStringWithShadow(
			spriteBatch,
			FontAssets.ItemStack.Value,
			$"{OriginPlayer.LocalOriginPlayer.decorativeAshesCount}/{Item.useLimitPerAnimation}",
			position + origin * scale * new Vector2(-1f, 0.4f),
			drawColor,
			0f,
			Vector2.Zero,
			new Vector2(inventoryScale),
			-1f,
			inventoryScale
		);
	}
}
public class Decorative_Ashes_Missile : ModProjectile {
	public override void SetDefaults() {
		Projectile.DamageType = DamageClasses.Explosive;
		Projectile.width = 10;
		Projectile.height = 10;
		Projectile.aiStyle = -1;
		Projectile.friendly = true;
		Projectile.timeLeft = 360;
		Projectile.localNPCHitCooldown = -1;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.appliesImmunityTimeOnSingleHits = true;
		Projectile.penetrate = 1;
	}
	public override void OnSpawn(IEntitySource source) {
	}
	public override void AI() {
		const int range = 16 * 16;
		const int boss_range = 16 * 27;
		const float boss_ratio = range / (float)boss_range;
		float targetWeight = range;
		Vector2 targetPos = default;
		bool foundTarget = Main.player[Projectile.owner].DoHoming((target) => {
			Vector2 currentPos = Projectile.Center.Clamp(target.Hitbox);
			float dist = Projectile.Center.Distance(currentPos);
			if (target is Player) dist *= 1.5f;
			if (target is NPC npc && (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type])) dist *= boss_ratio;
			if (dist < targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
				targetWeight = dist;
				targetPos = currentPos;
				return true;
			}
			return false;
		});

		if (foundTarget) {
			Vector2 targetVelocity = (targetPos - Projectile.Center).Normalized(out _);

			targetVelocity *= 16f;
			float speed = Projectile.velocity.Length();
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.083333336f).Normalized(out float newSpeed) * float.Max(speed, newSpeed);
		}
		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		ref Vector2 dustVel = ref Dust.NewDustPerfect(Projectile.Center - Projectile.velocity, DustID.Torch).velocity;
		dustVel *= 0.5f;
		dustVel -= Projectile.velocity * 0.25f * Math.Min(++Projectile.localAI[0] / 15f, 1);
	}
	public override void OnKill(int timeLeft) {
		ExplosiveGlobalProjectile.DoExplosion(Projectile, 48, sound: SoundID.Item14);
	}
}

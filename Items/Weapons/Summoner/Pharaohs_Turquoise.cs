using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Origins.CrossMod;
using Origins.Dev;
using Origins.NPCs;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Pharaohs_Turquoise : ModItem, ICustomDrawItem {
		public static int HitsForBonus => 3;
		private Asset<Texture2D> _smolTexture;
		public Texture2D SmolTexture => (_smolTexture ??= this.GetSmallTexture())?.Value;
		public override void SetStaticDefaults() {
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
			if (Item.useAnimation / Item.useTime < HitsForBonus) PegasusLib.PegasusLib.LogLoadingWarning(this.GetLocalization("CantGetBonus"));
		}
		public override void SetDefaults() {
			Item.damage = 50;
			Item.DamageType = DamageClasses.Incantation;
			Item.noMelee = true;
			Item.width = 22;
			Item.height = 34;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 7;
			Item.useAnimation = 27;
			Item.shoot = ModContent.ProjectileType<Pharaohs_Turquoise_P>();
			Item.shootSpeed = 8f;
			Item.mana = 14;
			Item.knockBack = 1f;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item8;
			Item.holdStyle = ItemHoldStyleID.HoldLamp;
			Item.useLimitPerAnimation = HitsForBonus;
		}
		public override void UseItemFrame(Player player) => Incantations.HoldItemFrame(player);
		public override void HoldItemFrame(Player player) => Incantations.HoldItemFrame(player);
		public bool BackHand => true;
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Incantations.DrawInHand(
				SmolTexture,
				ref drawInfo,
				lightColor
			);
		}
		public override bool AltFunctionUse(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<Pharaohs_Turquoise_Freeze>()] <= 0;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) velocity = player.Directions(yMultiplier: -1);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<Pharaohs_Turquoise_Freeze>();
				if (player.ownedProjectileCounts[type] <= 0) {
					player.StartChanneling(type);
					Projectile.NewProjectile(source, position, velocity, type, damage, knockback);
				}
				return false;
			}
			return true;
		}
	}
	public class Pharaohs_Turquoise_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.ignoreWater = true;
			Projectile.friendly = true;
			Projectile.timeLeft = 3600;
			Projectile.alpha = 50;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		static Counter currentCounter;
		Counter counter;
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse && itemUse.Player.ItemAnimationJustStarted) currentCounter = new Counter();
			if (currentCounter is not null) counter = currentCounter;
			Projectile.localAI[0] = Main.rand.Next(2);
		}
		public override bool ShouldUpdatePosition() {
			Player player = Main.player[Projectile.owner];
			return !player.channel || (player.HeldItem?.shoot != Type);
		}
		public override void AI() {
			Main.projFrames[Type] = 10;
			if (Projectile.velocity != default) Projectile.rotation = Projectile.velocity.ToRotation();
			const int HalfSpriteWidth = 30 / 2;

			int HalfProjWidth = Projectile.width / 2;

			// Vanilla configuration for "hitbox towards the end"
			if (Projectile.spriteDirection == 1) {
				DrawOriginOffsetX = -(HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = (int)-DrawOriginOffsetX * 2;
				DrawOriginOffsetY = 0;
			} else {
				DrawOriginOffsetX = (HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = 0;
				DrawOriginOffsetY = 0;
			}
			Projectile.frame = (int)Projectile.localAI[0] * 5;
			int addFrame = (int)(Projectile.ai[2] * (1f / 15) * 5);
			if (addFrame >= 5) Projectile.Kill();
			Projectile.frame += addFrame;
			if (Projectile.ai[2] != 0) Projectile.ai[2]++;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.penetrate++;
			target.AddBuff(Pharaohs_Turquoise_Buff.ID, 240);
			if (counter.HitTarget(target.whoAmI)) {
				target.AddBuff(Pharaohs_Turquoise_Buff2.ID, 240);
				ParticleOrchestrator.RequestParticleSpawn(false, ParticleOrchestraType.Keybrand, new() {
					PositionInWorld = Projectile.Center
				});
			}
			if (target.life > 0) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			Dissipate();
		}
		public void Dissipate() {
			Projectile.velocity = default;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			if (Projectile.ai[2] != 0) return;
			Projectile.ai[2] = 1;
			Projectile.netUpdate = true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Dissipate();
			oldVelocity = oldVelocity.Normalized(out float maxDist);
			Projectile.position += oldVelocity * CollisionExt.Raymarch(Projectile.Center, oldVelocity, maxDist);
			return false;
		}
		class Counter {
			int count = 0;
			int targetIndex = -1;
			public int Count => count;
			public bool HitTarget(int target) {
				if (targetIndex == -1) targetIndex = target;
				else if (targetIndex != target) count = -9999;
				return ++count >= Pharaohs_Turquoise.HitsForBonus;
			}
		}
		public class Pharaohs_Turquoise_Crit_Type : CritType<Pharaohs_Turquoise> {
			public override LocalizedText Description => base.Description.WithFormatArgs(Pharaohs_Turquoise.HitsForBonus);
			public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) {
				if (projectile?.ModProjectile is not Pharaohs_Turquoise_P proj) return false;
				return proj.counter.Count + 1 >= Pharaohs_Turquoise.HitsForBonus;
			}
			public override float CritMultiplier(Player player, Item item) => 1.25f;
		}
	}
	public class Pharaohs_Turquoise_Freeze : ModProjectile {
		public override string Texture => typeof(Pharaohs_Turquoise).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (!Projectile.TryGetOwner(out Player player)) {
				Projectile.Kill();
				return;
			}
			Projectile.position = player.MountedCenter;
			if (!player.channel) {
				Projectile.Kill();
				return;
			}
			Max(ref player.manaRegenDelay, 5);
			//player.SetDummyItemTime(5);
		}
	}
	public class Pharaohs_Turquoise_Buff : ModBuff, ICustomWikiStat {
		public bool CanExportStats => false;
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().forbiddenIncantationDebuff = true;
		}
	}
	public class Pharaohs_Turquoise_Buff2 : ModBuff, ICustomWikiStat {
		public bool CanExportStats => false;
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().forbiddenIncantationDebuffStrengthen = true;
		}
	}
}

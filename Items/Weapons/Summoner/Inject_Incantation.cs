using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.NPCs;
using Origins.Projectiles.Weapons;
using ReLogic.Content;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Inject_Incantation : ModItem, ICustomDrawItem {
		private Asset<Texture2D> _smolTexture;
		public Texture2D SmolTexture => (_smolTexture ??= this.GetSmallTexture())?.Value;
		public override void SetDefaults() {
			Item.damage = 10;
			Item.DamageType = DamageClasses.Incantation;
			Item.noMelee = true;
			Item.width = 22;
			Item.height = 34;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 26;
			Item.useAnimation = 26;
			Item.shoot = ModContent.ProjectileType<Inject_Incantation_P>();
			Item.shootSpeed = 10f;
			Item.mana = 14;
			Item.knockBack = 1f;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item8;
			Item.channel = true;
			Item.holdStyle = ItemHoldStyleID.HoldLamp;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Book, 5)
			.AddIngredient<Defiled_Bar>(10)
			.AddIngredient<Undead_Chunk>(6)
			.AddTile(TileID.Anvils)
			.Register();
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
	}
	public class Inject_Incantation_P : ModProjectile {
		static readonly AutoLoadingAsset<Texture2D> glowTexture = typeof(Inject_Incantation_P).GetDefaultTMLName() + "_Glow";
		static readonly AutoLoadingAsset<Texture2D> texture2 = typeof(Inject_Incantation_P).GetDefaultTMLName() + "2";
		static readonly AutoLoadingAsset<Texture2D> glowTexture2 = typeof(Inject_Incantation_P).GetDefaultTMLName() + "2_Glow";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 8;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.ignoreWater = true;
			Projectile.friendly = true;
			Projectile.timeLeft = 90;
			Projectile.alpha = 50;
			Projectile.manualDirectionChange = true;
			Projectile.usesLocalNPCImmunity = true;
			// not actually making it ignore i-frames, because then it's all but guaranteed to hit the first enemy it hits 4 times as long as they have enough health, even if the player actively tries to stop that
			//Projectile.localNPCHitCooldown = 0;
			Projectile.localNPCHitCooldown = 5;
		}
		(bool manual, Vector2 target) targetData;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (Projectile.ai[0] == 0 && Projectile.velocity.X != 0) Projectile.direction = Math.Sign(Projectile.velocity.X);
			#region Find target
			Vector2 targetCenter = Projectile.Center;
			bool foundTarget = false;
			if (player.channel && Projectile.ai[0] == 0) {
				foundTarget = true;
				if (Main.myPlayer == Projectile.owner) targetCenter = Main.MouseWorld;
				else if (targetData.manual) targetCenter = targetData.target;
				Projectile.timeLeft = 90;
				Vector2 oldTarg = targetData.target;
				targetData = (true, targetCenter);
				if (targetData.manual && oldTarg != targetCenter) Projectile.netUpdate = true;
			} else {
				targetData = (false, default);
				float targetDist = 640f;
				int target = -1;
				void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
					if (isPriorityTarget) return;
					if (npc.CanBeChasedBy()) {
						Vector2 diff = npc.Center - Projectile.Center;
						float dist = diff.Length();
						if (dist > targetDist) return;
						if (Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height)) {
							targetDist = dist;
							targetCenter = npc.Center;
							target = npc.whoAmI;
							foundTarget = true;
						}
					}
				}
				foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
				Projectile.ai[0] = 1;
			}
			#endregion

			#region Movement
			// Default movement parameters (here for attacking)
			if (foundTarget) {
				Vector2 direction = targetCenter - Projectile.Center;
				float distance = direction.Length();
				if (distance > 10) {
					Projectile.ai[1] = 0;
					direction /= distance;
					Projectile.velocity = direction * 10;
				} else {
					Projectile.ai[1] = 1;
					Projectile.velocity = direction;
				}
			} else {
				Projectile.velocity.Y += 0.08f;
			}
			#endregion

			#region Animation and visuals

			Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;

			// This is a simple "loop through all frames from top to bottom" animation
			if (Projectile.velocity != Vector2.Zero) {
				Projectile.rotation = Projectile.velocity.ToRotation() - 0.6435f * Projectile.direction;
				if (Projectile.direction == -1) Projectile.rotation += MathHelper.Pi;
			}
			if (Main.rand.NextBool(2)) {
				Dust dust = Dust.NewDustDirect(
					Projectile.position - Vector2.One * 2,
					Projectile.width + 4,
					Projectile.height + 4,
					DustID.SnowBlock,
					Projectile.velocity.X * 0.4f,
					Projectile.velocity.Y * 0.4f,
					100,
					Scale: 1.2f
				);
				dust.noGravity = true;
				dust.velocity *= 1.8f;
				dust.velocity.Y -= 0.5f;
				//dust.noLight = true;
			}
			#endregion
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(targetData.manual);
			if (targetData.manual) writer.WriteVector2(targetData.target);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			targetData = reader.ReadBoolean() ? (true, reader.ReadVector2()) : (false, default);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Inject_Incantation_Buff.ID, 240);
			if (target.CanBeChasedBy()) {
				if (Projectile.ai[2] == target.whoAmI) {
					if(++Projectile.localAI[2] >= 4) {
						Projectile.NewProjectile(
							Projectile.GetSource_OnHit(target),
							Projectile.Center,
							Vector2.Zero,
							ModContent.ProjectileType<Defiled_Spike_Explosion>(),
							Projectile.damage * 4,
							Projectile.knockBack,
							ai0: 7
						);
						Projectile.Kill();
					}
				} else {
					Projectile.ai[2] = target.whoAmI;
					Projectile.localAI[2] = 1;
				}
				if (target.life > 0) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			}
			Projectile.damage = (int)(Projectile.damage * 0.98f);
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			for (int i = 0; i < 7; i++) {
				Dust dust = Dust.NewDustDirect(
					Projectile.position - Vector2.One * 2,
					Projectile.width + 4,
					Projectile.height + 4,
					DustID.WhiteTorch,
					Projectile.oldVelocity.X * 0.4f,
					Projectile.oldVelocity.Y * 0.4f,
					100,
					Scale: 1.2f
				);
				dust.noGravity = true;
				dust.velocity *= 3.6f;
				dust.velocity.Y -= 0.5f;
			}
			if (Projectile.owner == Main.myPlayer) {

			}
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 24;
			height = 24;
			fallThrough = true;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) => true;
		public override bool PreDraw(ref Color lightColor) {
			bool clamp = Projectile.ai[1] == 1 && !targetData.manual;
			Main.EntitySpriteDraw(
				clamp ? texture2 : TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				new(20, 14),
				1,
				Projectile.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
			Main.EntitySpriteDraw(
				clamp ? glowTexture2 : glowTexture,
				Projectile.Center - Main.screenPosition,
				null,
				Color.White,
				Projectile.rotation,
				new(20, 14),
				1,
				Projectile.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
			return false;
		}
	}
	public class Inject_Incantation_Buff : ModBuff, ICustomWikiStat {
		public bool CanExportStats => false;
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().injectIncantationDebuff = true;
		}
	}
}

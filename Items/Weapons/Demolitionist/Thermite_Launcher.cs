using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Weapons.Ammo.Canisters;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Thermite_Launcher : ModItem {
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Thermite_Canister_P>(38, 34, 16f, 44, 18);
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.LightRed;
		}
		//can't just chain rules since OneFromOptionsNotScaledWithLuckDropRule drops all the items directly
		//but that's fine since other bosses that drop a ranged weapon don't show the ammo in the bestiary
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemOpen or EntitySource_Loot) {
				Main.timeItemSlotCannotBeReusedFor[Item.whoAmI] = 1;
				int index = Item.NewItem(source, Item.position, ModContent.ItemType<Napalm_Canister>(), Main.rand.Next(60, 100));
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
				}
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			CanisterGlobalItem.ItemToCanisterID.TryGetValue(source.AmmoItemIdUsed, out type);
			Projectile.NewProjectile(
				source,
				position,
				velocity,
				Item.shoot,
				damage,
				knockback,
				player.whoAmI,
				ai2: type
			);
			return false;
		}
	}
	public class Thermite_Canister_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Canister_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Canister_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			ProjectileID.Sets.NeedsUUID[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 900;
			Projectile.scale = 0.85f;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 6;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[1] = -1;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X == 0f) {
				Projectile.velocity.X = -oldVelocity.X;
			}
			if (Projectile.velocity.Y == 0f) {
				Projectile.velocity.Y = -oldVelocity.Y;
			}
			Projectile.timeLeft = 1;
			return true;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
		}
		public override void AI() {
			this.DoGravity(0.2f);
			Projectile.rotation += Projectile.velocity.X * 0.1f;
			Projectile auraProj = Projectile.GetRelatedProjectile_Depreciated(1);
			if (auraProj is null) {
				if (Projectile.owner == Main.myPlayer) Projectile.ai[1] = Projectile.NewProjectileDirect(
					Projectile.GetSource_FromAI(),
					Projectile.position,
					default,
					Thermite_P.ID,
					Projectile.damage / 3,
					0,
					Projectile.owner,
					Projectile.identity
				).identity;
			} else {
				if (auraProj.active && auraProj.type == Thermite_P.ID && auraProj.ai[0] == Projectile.identity) {
					auraProj.Center = Projectile.Center;
					auraProj.rotation = Projectile.rotation;
				} else {
					Projectile.ai[1] = -1;
				}
			}
		}
		public void DefaultExplosion(Projectile projectile, int fireDustType = DustID.Torch, int size = 96) {
			CanisterGlobalProjectile.DefaultExplosion(projectile, false, fireDustType: fireDustType, size: size);
			Projectile.NewProjectile(
				projectile.GetSource_Death(),
				projectile.Center,
				default,
				Thermite_Lingering_P.ID,
				projectile.damage / 10,
				projectile.knockBack
			);
		}
	}
	public class Thermite_P : ModProjectile, ICanisterChildProjectile, IIsExplodingProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.NeedsUUID[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.friendly = true;
			Projectile.width = 36;
			Projectile.height = 36;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 3600;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 5;
			Projectile.hide = true;
			Projectile.tileCollide = false;
			Projectile.ArmorPenetration += 25;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, 1, 0.75f, 0);
			Projectile ownerProj = Projectile.GetRelatedProjectile_Depreciated(0);
			if (ownerProj is null) {
				Projectile.scale *= 0.85f;
				Projectile.scale -= 0.15f;
				if (Projectile.scale <= 0) Projectile.Kill();
				Projectile.ai[0] = -1;
			} else {
				if (ownerProj.active && ownerProj.type == Thermite_Canister_P.ID && ownerProj.ai[1] == Projectile.identity) {
					Projectile.scale = ownerProj.scale * 2;
					Projectile.Center = ownerProj.Center;
					Projectile.rotation = ownerProj.rotation;
					Projectile.timeLeft = 60 * 10;
				} else {
					Projectile.Center = ownerProj.Center;
					Projectile.ai[0] = -1;
				}
			}
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float range = projHitbox.Width * Projectile.scale * 0.5f;
			return Projectile.Center.DistanceSQ(Projectile.Center.Clamp(targetHitbox)) < range * range;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, Main.rand.Next(300, 451));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.OnFire3, Main.rand.Next(300, 451));
		}
		public override Color? GetAlpha(Color lightColor) {
			return new Color(255, 180, 50, 0);
		}
		public bool IsExploding => true;
	}
	public class Thermite_Lingering_P : ModProjectile, ICanisterChildProjectile, IIsExplodingProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.friendly = true;
			Projectile.width = 36;
			Projectile.height = 36;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 7;
			Projectile.timeLeft = 60 * 5;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.ArmorPenetration += 25;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, 1, 0.75f, 0);
			if (Projectile.timeLeft > 60 * 4.5f && Projectile.scale < 2) Projectile.scale += 0.1f;
			Projectile.scale *= 0.96f;
			Projectile.scale += 0.08f * Math.Min(Projectile.timeLeft, 60 * 1) / 60f;
			if (Projectile.scale <= 0.2f) Projectile.Kill();
			if (Projectile.timeLeft < 2) Projectile.timeLeft = 2;
			Dust.NewDustDirect(
				Projectile.Center, 0, 0,
				ModContent.DustType<Thermite_Dust>(),
				Scale: Projectile.scale / 2,
				newColor: Projectile.GetGlobalProjectile<CanisterChildGlobalProjectile>().CanisterData.InnerColor
			).velocity *= 2 * Projectile.scale;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float range = projHitbox.Width * Projectile.scale * 0.5f;
			return Projectile.Center.DistanceSQ(Projectile.Center.Clamp(targetHitbox)) < range * range;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, Main.rand.Next(300, 451));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.OnFire3, Main.rand.Next(300, 451));
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override Color? GetAlpha(Color lightColor) {
			return new Color(255, 180, 50, 0);
		}
		private static VertexStrip _vertexStrip = new();
		Color glowColor;
		public override bool PreDraw(ref Color lightColor) {
			glowColor = Projectile.GetGlobalProjectile<CanisterChildGlobalProjectile>().CanisterData.InnerColor;
			glowColor.A = 0;
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			int count = 4;
			for (int i = 0; i < count; i++) {
				Main.EntitySpriteDraw(
					tex,
					Projectile.Center - Main.screenPosition,
					null,
					glowColor * 0.666f,
					Projectile.rotation + MathHelper.PiOver2 * (i / (float)count),
					tex.Size() * 0.5f,
					(Projectile.width / (float)tex.Width) * Projectile.scale,
					SpriteEffects.None
				);
			}
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:AnimatedTrail"];
			int num = 1;//1
			int num2 = 0;//0
			int num3 = 0;//0
			float w = 0f;//0.6f
			miscShaderData.UseShaderSpecificData(new Vector4(num, num2, num3, w));
			miscShaderData.UseImage0(TextureAssets.Extra[ExtrasID.MagicMissileTrailShape]);
			miscShaderData.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
			//miscShaderData.UseImage0(TextureAssets.Extra[189]);
			float uTime = (float)Main.timeForVisualEffects / 22;
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 1, 1, 0));
			miscShaderData.Shader.Parameters["uAlphaMatrix1"].SetValue(new Vector4(0.7f, 0, 0, 0));
			miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(uTime, 0, 1, 1));
			miscShaderData.Shader.Parameters["uSourceRect1"].SetValue(new Vector4(0, (float)Main.timeForVisualEffects * 0.02f, 1, 1));
			miscShaderData.Apply();
			const int verts = 128;
			float[] rot = new float[verts + 1];
			Vector2[] pos = new Vector2[verts + 1];
			Vector2 center = Projectile.Center;
			for (int i = 0; i < verts + 1; i++) {
				rot[i] = (i * MathHelper.TwoPi) / verts;
				pos[i] = center + new Vector2(StripWidth() * Main.rand.NextFloat(0.9f, 1f), 0).RotatedBy(rot[i] + MathHelper.PiOver2);
			}
			_vertexStrip.PrepareStrip(pos, rot, StripColors, StripWidth, -Main.screenPosition, pos.Length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			for (int i = 0; i < verts + 1; i++) {
				rot[i] = (i * MathHelper.TwoPi) / verts + uTime * -2;
				pos[i] = center + new Vector2(StripWidth2() * 0.9f, 0).RotatedBy(rot[i] + MathHelper.PiOver2);
			}
			_vertexStrip.PrepareStrip(pos, rot, StripColors, StripWidth2, -Main.screenPosition, pos.Length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			for (int i = 0; i < verts + 1; i++) {
				rot[i] = (i * MathHelper.TwoPi) / verts + uTime * 0.5f;
				pos[i] = center + new Vector2(StripWidth3() * 0.8f, 0).RotatedBy(rot[i] + MathHelper.PiOver2);
			}
			_vertexStrip.PrepareStrip(pos, rot, StripColors, StripWidth3, -Main.screenPosition, pos.Length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			return false;
		}
		private Color StripColors(float progressOnStrip) => glowColor;
		private float StripWidth(float progressOnStrip = 0) => 16 * Projectile.scale;
		private float StripWidth2(float progressOnStrip = 0) => 12 * Projectile.scale;
		private float StripWidth3(float progressOnStrip = 0) => 8 * Projectile.scale;
		public bool IsExploding => true;
	}
}

using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Journal;
using Origins.NPCs;
using PegasusLib;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	public class Infusion : ModItem, ICustomWikiStat, IJournalEntrySource {
		static short glowmask;
        public string[] Categories => [
            "MagicGun"
        ];
		public string EntryName => "Origins/" + typeof(Infusion_Entry).Name;
		public class Infusion_Entry : JournalEntry {
			public override string TextKey => "Infusion";
			public override JournalSortIndex SortIndex => new("The_Defiled", 4);
		}
		public override void SetStaticDefaults() {
			Origins.FlatDamageMultiplier[Type] = 2f / 8f;
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 4;
			Item.ArmorPenetration = 14;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Item.mana = 3;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.width = 30;
			Item.height = 36;
			Item.useTime = 9;
			Item.useAnimation = 9;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
			Item.shoot = ModContent.ProjectileType<Infusion_P>();
			Item.shootSpeed = 16f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = Origins.Sounds.DefiledIdle.WithPitchRange(0.9f, 1f);
			Item.autoReuse = true;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.FallenStar, 8)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 8)
			.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 6)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-3, -2);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(0.075f);
		}
	}
	public class Infusion_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Infusion_P";
		protected PolarVec2 embedPos;
		protected float embedRotation;
		const int embed_duration = 600;
		protected int EmbedTime { get => (int)Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
		protected int EmbedTarget { get => (int)Projectile.localAI[1]; set => Projectile.localAI[1] = value; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 15;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.hide = true;
		}
		public override void AI() {
			if (EmbedTime > 0) {//embedded in enemy
				EmbedTime++;
				NPC target = Main.npc[EmbedTarget];
				Projectile.Center = target.Center + (Vector2)embedPos.RotatedBy(target.rotation);
				Projectile.rotation = embedRotation + target.rotation;
				if (Projectile.numUpdates == 0 && EmbedTime > 10) OriginGlobalNPC.AddInfusionSpike(target, Projectile.whoAmI);
				if (!target.active) {
					EmbedTime = embed_duration + 1;
				}
				if (EmbedTime > embed_duration) {
					Projectile.Kill();
				}
			} else if(Projectile.velocity != Vector2.Zero) {//not embedded
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi * 0.25f;
				Vector2 boxSize = (Vector2)new PolarVec2(3, Projectile.rotation - MathHelper.PiOver2);
				Rectangle tipHitbox = OriginExtensions.BoxOf(Projectile.Center + boxSize, Projectile.Center - boxSize, 2);
				for (int i = 0; i < Projectile.localNPCImmunity.Length; i++) {
					if (Projectile.localNPCImmunity[i] > 0) {
						NPC target = Main.npc[i];
						Rectangle targetHitbox = target.Hitbox;
						if (target.active && targetHitbox.Intersects(tipHitbox)) {
							EmbedTime++;
							EmbedTarget = i;
							Projectile.aiStyle = 0;
							Projectile.velocity = Vector2.Zero;
							embedPos = ((PolarVec2)(Projectile.Center - target.Center)).RotatedBy(-target.rotation);
							embedRotation = Projectile.rotation - target.rotation;
							Projectile.netUpdate = true;
							break;
						}
					}
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = Vector2.Zero;
			Projectile.position += oldVelocity * 1.5f;
			Projectile.aiStyle = 0;
			Projectile.friendly = false;
			return false;
		}
		public override bool? CanHitNPC(NPC target) {
			if (EmbedTime > 0) {
				return false;
			}
			return null;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, new Color(Lighting.GetSubLight(Projectile.Center)), Projectile.rotation, new Vector2(13, 3), Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[0]);
			writer.Write(Projectile.localAI[1]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[0] = reader.ReadSingle();
			Projectile.localAI[1] = reader.ReadSingle();
		}
	}
}

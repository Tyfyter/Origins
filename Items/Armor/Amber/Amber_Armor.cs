using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Other.Dyes;
using Origins.Items.Weapons.Ranged;
using Origins.NPCs;
using Origins.Tiles.Other;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Amber {
	[AutoloadEquip(EquipType.Head)]
	public class Amber_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string[] Categories => [
			WikiCategories.ArmorSet,
			WikiCategories.ExplosiveBoostGear,
			WikiCategories.SelfDamageProtek
		];
		public bool? Hardmode => true;
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Amber_Breastplate>() && legs.type == ModContent.ItemType<Amber_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Amber");
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveBlastRadius -= 0.38f;
			originPlayer.amberSet = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Amber, 4)
			.AddIngredient(ItemID.SoulofNight, 3)
			.AddIngredient(ModContent.ItemType<Carburite_Item>(), 12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public string ArmorSetName => "Amber_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Amber_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Amber_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Amber_Breastplate : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.34f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Amber, 12)
			.AddIngredient(ItemID.SoulofNight, 3)
			.AddIngredient(ModContent.ItemType<Carburite_Item>(), 36)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Amber_Greaves : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.15f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Amber, 8)
			.AddIngredient(ItemID.SoulofNight, 3)
			.AddIngredient(ModContent.ItemType<Carburite_Item>(), 24)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Amber_Shard : ModProjectile, IShadedProjectile {
		public override string Texture => "Terraria/Images/Item_" + ItemID.Amber;
		public int Shader => Amber_Dye.ShaderID;
		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.extraUpdates = 1;
			Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
			Projectile.localAI[2] = ModContent.ProjectileType<Shardcannon_P1>() + Main.rand.Next(3);
		}
		public override void AI() {
			bool gravity = true;
			Player player = Main.player[Projectile.owner];
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.selfDamageRally > 0) {
				if (Projectile.Hitbox.Intersects(player.Hitbox)) {
					int maxHeal = Math.Min(originPlayer.selfDamageRally, Projectile.damage / 3);
					Main.LocalPlayer.Heal(maxHeal);
					ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(Amber_Dye.ID);
					for (int i = 0; i < 16; i++) {
						Dust dust = Dust.NewDustDirect(
							player.position,
							player.width,
							player.height,
							DustID.CrystalSerpent
						);
						dust.noGravity = true;
						dust.shader = shader;
					}
					originPlayer.selfDamageRally -= maxHeal;
					Projectile.Kill();
				} else if (Projectile.timeLeft < 3600 - 15) {
					Vector2 diff = player.MountedCenter - Projectile.Center;
					const int grabRange = 6 * 16;
					if (diff.LengthSquared() < grabRange * grabRange) {
						diff.Normalize();
						Projectile.velocity = Vector2.Lerp(Projectile.velocity, diff * 4, 0.5f);
						gravity = false;
					}
				}
			}
			if (gravity) Projectile.velocity.Y += 0.04f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Amber_Debuff.Inflict(target, 300);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[(int)Projectile.localAI[2]].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				texture.Size() * 0.5f,
				1,
				SpriteEffects.None
			);
			return false;
		}
	}
	public class Amber_Debuff_Shard : ModDust {
		public record struct AmberDebuffShardData(Entity Parent, int Texture, Vector2 Offset, float Rotation);
		public override string Texture => "Terraria/Images/Item_" + ItemID.Amber;
		public override void OnSpawn(Dust dust) {
			dust.customData = new AmberDebuffShardData(null, ModContent.ProjectileType<Shardcannon_P1>() + Main.rand.Next(3), default, Main.rand.NextFloat(MathHelper.TwoPi));
		}
		public override bool Update(Dust dust) {
			bool remain = true;
			if(dust.customData is not AmberDebuffShardData data || data.Parent is null) {
				dust.active = false;
				return false;
			}
			if (!data.Parent.active) remain = false;
			Vector2 center = data.Parent.Center;

			Vector2 direction = new(data.Parent.direction, 1);
			if (data.Parent is NPC npc) {
				if (remain && !npc.HasBuff(Amber_Debuff.ID)) remain = false;
				center += npc.netOffset;
				center.Y += npc.gfxOffY;
				direction.Y = npc.directionY;
			} else if (data.Parent is Projectile && !data.Parent.active) {
				dust.active = false;
			}

			float rotation = data.Parent.GetRotation();
			dust.position = (data.Offset * direction).RotatedBy(rotation) + center;
			dust.rotation = data.Rotation + rotation;

			if (!remain && (dust.alpha += 16) >= 255) dust.active = false;
			return false;
		}
		public override bool PreDraw(Dust dust) {
			if (dust.customData is not AmberDebuffShardData data) {
				dust.active = false;
				return false;
			}
			Color lightColor = Lighting.GetColor((int)(dust.position.X + 4) / 16, (int)(dust.position.Y + 4) / 16);
			Texture2D texture = TextureAssets.Projectile[data.Texture].Value;
			Main.spriteBatch.Draw(
				texture,
				dust.position - Main.screenPosition,
				null,
				lightColor * (1 - dust.alpha / 255f),
				dust.rotation,
				texture.Size() * 0.5f,
				1,
				SpriteEffects.None,
			0);
			return false;
		}
		public static void SpawnDusts(Entity entity) {
			if (Main.dedServ) return;
			ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(Amber_Dye.ID);
			int type = ModContent.DustType<Amber_Debuff_Shard>();
			int size = entity.width + entity.height;
			List<Vector2> points = OriginExtensions.FelisCatusSampling(entity.Hitbox, (int)(size * 0.3f), size * 0.075f + 1, size * 0.25f + 8);
			Vector2 direction = new(entity.direction, 1);
			if (entity is NPC npc) {
				direction.Y = npc.directionY;
			}
			for (int i = 0; i < points.Count; i++) {
				Dust dust = Dust.NewDustPerfect(
					points[i],
					type,
					Vector2.Zero
				);
				dust.shader = shader;
				if (dust.customData is AmberDebuffShardData data) {
					dust.customData = data with {
						Parent = entity,
						Offset = (points[i] - entity.Center).RotatedBy(-entity.GetRotation()) * direction
					};
				}
				
			}
		}
	}
	public class Amber_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Item_" + ItemID.Amber;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public static void Inflict(NPC npc, int duration) {
			if (!npc.HasBuff(ID)) {
				Amber_Debuff_Shard.SpawnDusts(npc);
			}
			npc.AddBuff(ID, duration);
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.AddBuff(Slow_Debuff.ID, 5);
			npc.GetGlobalNPC<OriginGlobalNPC>().amberDebuff = true;
		}
	}
}

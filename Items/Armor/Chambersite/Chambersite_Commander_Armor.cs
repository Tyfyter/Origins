using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Armor.Chambersite;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Summoner;
using Origins.NPCs;
using Origins.Projectiles;
using Origins.Tiles.Other;
using PegasusLib;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Chambersite {
	[AutoloadEquip(EquipType.Head)]
	public class Chambersite_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public static int HeadSlot { get; private set; }
		public string[] Categories => [
			"ArmorSet",
			"GenericBoostGear"
		];
		public override void SetStaticDefaults() {
			HeadSlot = Item.headSlot;
		}
		public override void SetDefaults() {
			Item.defense = 14;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) += 0.12f;
			player.OriginPlayer().projectileSpeedBoost += 0.12f;
			player.maxMinions += 1;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Chambersite_Breastplate>() && legs.type == ModContent.ItemType<Chambersite_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.ChambersiteCommander");
			player.OriginPlayer().chambersiteCommandoSet = true;
			player.OriginPlayer().setActiveAbility = SetActiveAbility.chambersite_armor;
			player.GetKnockback(DamageClass.Generic) += 0.15f;
			player.maxMinions += 1;
			player.AddBuff(ModContent.BuffType<Voidsight_Buff>(), 60);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.LunarTabletFragment, 3)
			.AddIngredient<Carburite_Item>(12)
			.AddIngredient<Chambersite_Item>(4)
			.AddTile(TileID.MythrilAnvil)
			.AddCondition(Condition.DownedPlantera)
			.Register();
		}
		public string ArmorSetName => "Chambersite_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Chambersite_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Chambersite_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Chambersite_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.defense = 22;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClass.Generic) += 0.12f;
			player.endurance += (1 - player.endurance) * 0.08f;
			player.statManaMax2 += 60;
			player.manaCost *= 0.9f;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.LunarTabletFragment, 3)
			.AddIngredient<Carburite_Item>(36)
			.AddIngredient<Chambersite_Item>(12)
			.AddTile(TileID.MythrilAnvil)
			.AddCondition(Condition.DownedPlantera)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Chambersite_Greaves : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.defense = 15;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.2f;
			player.GetAttackSpeed(DamageClass.Generic) += 0.12f;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.LunarTabletFragment, 3)
			.AddIngredient<Carburite_Item>(24)
			.AddIngredient<Chambersite_Item>(8)
			.AddTile(TileID.MythrilAnvil)
			.AddCondition(Condition.DownedPlantera)
			.Register();
		}
	}
	public class Chambersite_Commander_Sentinel : ModProjectile {
		public override string Texture => "Origins/NPCs/MiscB/Chambersite_Sentinel";
		public static int ID { get; private set; }
		public static int MaxActiveAims => 5;
		public static float SpeedMult => 2.5f;
		public override void SetStaticDefaults() {
			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ID = Type;
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Generic;
			Projectile.timeLeft = 15;
			Projectile.width = 46;
			Projectile.height = 54;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.netImportant = true;
		}
		public override bool ShouldUpdatePosition() => false;
		Aim[] aims;
		Aim[] decayingAims;

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.GetGlobalProjectile<OriginGlobalProj>().weakShimmer = player.OriginPlayer()?.weakShimmer ?? false;

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active || !player.OriginPlayer().chambersiteCommandoSet) {
				player.ClearBuff(Chambersite_Commander_Sentinel_Buff.ID);
			}
			if (player.HasBuff(Chambersite_Commander_Sentinel_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			if (Main.myPlayer == player.whoAmI) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = player.MountedCenter - new Vector2(Projectile.width / 2, 88);
				Projectile.velocity = Vector2.Zero;
				Projectile.netUpdate = true;
			}
			#endregion

			#region Animation and visuals
			Projectile.spriteDirection = -player.direction;
			#endregion

			#region Attack targets
			float highestProgress = 0;
			ActiveSound sound;
			Rectangle screen = new(0, 0, NPC.sWidth, NPC.sHeight);
			screen = screen.Recentered(player.Center);
			if (Projectile.ai[2] == 1) {
				Projectile.ai[2] = 2;
				for (int i = 0; i < aims.Length; i++) {
					if (aims[i].active) {
						aims[i].active = false;
						AddDecayingAim(aims[i]);
					}
				}
			}
			if (Projectile.ai[2] == 2) {
				Projectile.aiStyle = -1;
				for (int i = 0; i < decayingAims.Length; i++) {
					decayingAims[i].UpdateDecaying(Projectile.ai[1]);
					if (decayingAims[i].active) Projectile.timeLeft = 2;
					highestProgress = float.Max(highestProgress, decayingAims[i].Progress);
				}
				if (SoundEngine.TryGetActiveSound(soundSlot, out sound)) {
					sound.Volume = highestProgress;
					sound.Position = Projectile.position;
				}
				if (SoundEngine.TryGetActiveSound(soundSlot2, out sound)) {
					sound.Volume = highestProgress;
					sound.Position = Projectile.position;
				}
				return;
			}
			aims ??= new Aim[Main.maxNPCs];
			decayingAims ??= new Aim[20];
			float maxLengthSQ = Projectile.velocity.LengthSquared();
			if (--Projectile.ai[0] <= 0) {
				if (Main.myPlayer == Projectile.owner) {
					DoShoot();
				}
			}
			Vector2 center = Projectile.Center;
			int activeAims = 0;
			for (int i = 0; i < aims.Length; i++) {
				if (aims[i].active) {
					activeAims++;
					if (aims[i].Update(i, center, Projectile.ai[1], screen)) AddDecayingAim(aims[i]);
					highestProgress = float.Max(highestProgress, aims[i].Progress);
				}
			}
			for (int i = 0; i < decayingAims.Length; i++) {
				decayingAims[i].UpdateDecaying(Projectile.ai[1]);
				highestProgress = float.Max(highestProgress, decayingAims[i].Progress);
			}
			if (SoundEngine.TryGetActiveSound(soundSlot, out sound)) {
				sound.Volume = highestProgress;
				sound.Position = Projectile.position;
			} else if (highestProgress > 0) {
				int projType = Type;
				soundSlot = SoundEngine.PlaySound(SoundID.Pixie.WithPitch(-1), Projectile.Center, soundInstance => soundInstance.Volume > 0 && Projectile.active && Projectile.type == projType);
			}
			if (SoundEngine.TryGetActiveSound(soundSlot2, out sound)) {
				sound.Volume = highestProgress;
				sound.Position = Projectile.position;
			} else if (highestProgress > 0) {
				int projType = Type;
				soundSlot2 = SoundEngine.PlaySound(Origins.Sounds.LightningCharging.WithPitch(-1), Projectile.Center, soundInstance => soundInstance.Volume > 0 && Projectile.active && Projectile.type == projType);
			}
			Triangle hitTri;
			Vector2 perp;
			int totalDamage = 0;
			OriginPlayer originPlayer = player.OriginPlayer();
			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.CanBeChasedBy(Projectile)) continue;
				Rectangle npcHitbox = npc.Hitbox;
				for (int i = 0; i < aims.Length; i++) {
					if (!aims[i].active) continue;
					Vector2 motion = aims[i].Motion;
					if (motion == Vector2.Zero) continue;
					Vector2 norm = motion.SafeNormalize(Vector2.Zero);
					perp.X = norm.Y;
					perp.Y = -norm.X;
					hitTri = new(center + perp * 16, center - perp * 16, center + motion * Projectile.scale + norm * 16);
					if (hitTri.Intersects(npcHitbox)) {
						NPC.HitModifiers hitModifiers = npc.GetIncomingStrikeModifiers(Projectile.DamageType, 0);
						hitModifiers.Defense.Base = Math.Min(hitModifiers.Defense.Base, 8);
						int damage = hitModifiers.GetDamage(Projectile.damage, false);
						damage = Main.rand.RandomRound(damage * Projectile.ai[1]);
						OriginGlobalNPC globalNPC = npc.GetGlobalNPC<OriginGlobalNPC>();
						globalNPC.sentinelDamage += damage;
						globalNPC.sentinelSpeed = Projectile.ai[1];
						totalDamage += damage;
						break;
					}
				}
			}
			player.addDPS(Main.rand.RandomRound(totalDamage / 30f));
			/*Mod.Logger.Info($"CCAS Damage: {totalDamage} [{string.Join(", ", aims.TrySelect((Aim a, out string text) => {
				text = a.ToString();
				return a.Type != default;
			}))}]");*/
			#endregion
		}
		SlotId soundSlot;
		SlotId soundSlot2;
		void AddDecayingAim(Aim aim) {
			aim.active = true;
			float bestLength = float.PositiveInfinity;
			int bestDecaying = 0;
			for (int i = 0; i < decayingAims.Length; i++) {
				if (decayingAims[i].active) {
					float length = decayingAims[i].Motion.LengthSquared();
					if (bestLength > length) {
						bestLength = length;
						bestDecaying = i;
					}
				} else {
					bestDecaying = i;
					break;
				}
			}
			decayingAims[bestDecaying] = aim;
		}
		bool DoShoot() {
			Player player = Main.player[Projectile.owner];
			List<byte> newAims = null;
			if (Main.netMode != NetmodeID.SinglePlayer) newAims = [];
			int activeAimsCount = 0;
			for (int i = 0; i < aims.Length; i++) {
				if (aims[i].active) activeAimsCount++;
			}
			foreach (NPC npc in Main.ActiveNPCs) {
				if (activeAimsCount >= MaxActiveAims) break;
				if (aims[npc.whoAmI].active) continue;
				if (!npc.CanBeChasedBy(Projectile)) continue;
				aims[npc.whoAmI].Set(npc);
				newAims?.Add((byte)npc.whoAmI);
				activeAimsCount++;
			}
			if (Main.netMode != NetmodeID.SinglePlayer && newAims.Count > 0) {
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.shinedown_spawn_shadows);
				packet.Write((byte)Projectile.owner);
				packet.Write((ushort)Projectile.identity);
				packet.Write((byte)newAims.Count);
				packet.Write(newAims.ToArray());
				packet.Send();
			}

			Projectile.ai[1] = SpeedMult;
			Projectile.ai[0] = Projectile.ai[1] * 20;
			Projectile.netUpdate = true;
			return true;
		}
		public void RecieveSync(byte[] indices) {
			for (int i = 0; i < indices.Length; i++) {
				byte index = indices[i];
				aims[indices[i]].Set(Main.npc[index]);
			}
			/*Mod.Logger.Info($"Sentinel sync: {Main.player[Projectile.owner].name} {Projectile.identity} [{string.Join(", ", indices)}] [{string.Join(", ", aims.TrySelect((Aim a, out string text) => {
				text = a.ToString();
				return a.Type != default;
			}))}]");*/
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[0] = 2;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D rayTexture = TextureAssets.Projectile[ModContent.ProjectileType<Shinedown_Staff_P>()].Value;
			Vector2 position = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
			Vector2 origin = rayTexture.Frame().Bottom();
			float spriteLengthFactor = 1f / rayTexture.Height;
			Color color = Color.Black * 0.4f;
			for (int i = 0; i < aims.Length; i++) {
				if (!aims[i].active) continue;
				Vector2 motion = aims[i].Motion;
				Main.EntitySpriteDraw(
					rayTexture,
					position,
					null,
					color,
					motion.ToRotation() + MathHelper.PiOver2,
					origin,
					new Vector2(1f, motion.Length() * spriteLengthFactor),
					SpriteEffects.None
				);
			}
			for (int i = 0; i < decayingAims.Length; i++) {
				if (!decayingAims[i].active) continue;
				Vector2 motion = decayingAims[i].Motion;
				Main.EntitySpriteDraw(
					rayTexture,
					position,
					null,
					color,
					motion.ToRotation() + MathHelper.PiOver2,
					origin,
					new Vector2(1f, motion.Length() * spriteLengthFactor),
					SpriteEffects.None
				);
			}
			return true;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			return false;
		}
		struct Aim {
			int type;
			Vector2 motion;
			float progress;
			public bool active;
			public readonly int Type => type;
			public readonly float Progress => progress;
			public readonly Vector2 Motion => motion;
			public void Set(NPC target) {
				type = target.type;
				motion = default;
				active = true;
				progress = 0;
			}
			public bool Update(int index, Vector2 position, float speed, Rectangle screen) {
				NPC target = Main.npc[index];
				if (!target.active || target.type != type) target = null;
				if (target is null) {
					active = false;
					return true;
				}
				if (!screen.Intersects(target.Hitbox)) {
					active = false;
					return true;
				}
				Vector2 diff = target.Center - position;
				MathUtils.LinearSmoothing(ref progress, 1, 1 / 60f);
				speed *= progress + 1;
				MathUtils.LinearSmoothing(ref motion, diff, 4 * speed);
				motion = Utils.rotateTowards(Vector2.Zero, motion, diff, 0.3f * speed);
				return false;
			}
			public void UpdateDecaying(float speed) {
				MathUtils.LinearSmoothing(ref progress, 0, 1 / 60f);
				if (active) {
					speed *= 2 - progress;
					float length = Motion.Length();
					motion *= 1 - (1 - 0.99f * ((length - 2) / length)) * speed;
					active = length > 4;
				}
			}
			public override readonly string ToString() => $"({Lang.GetNPCNameValue(type)} {active} {progress})";
		}
	}
}
namespace Origins.Buffs {
	public class Chambersite_Commander_Sentinel_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Chambersite_Commander_Sentinel.ID
		];
		public override bool ShowCount => false;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.buffNoTimeDisplay[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex) {
			bool foundAny = false;
			foreach (int proj in ProjectileTypes()) {
				if (player.ownedProjectileCounts[proj] > 0) {
					foundAny = true;
				}
			}
			if (!foundAny) {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}

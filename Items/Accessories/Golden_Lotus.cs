using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoMod.Cil;
using Origins.Dev;
using PegasusLib;
using PegasusLib.Reflection;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Golden_Lotus : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Misc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.shoot = ModContent.ProjectileType<Golden_Lotus_Fairy>();
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.fairyLotus = true;
			originPlayer.goldenLotus = true;
			originPlayer.goldenLotusItem = Item;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.SubstituteKeybind(Keybindings.GoldenLotus);
		}
	}
	public class Golden_Lotus_Fairy : ModProjectile {
		public const float max_range = 16 * 35;
		public const int expand_frames = 30;
		public const float range_per_frame = max_range / expand_frames;
		Dictionary<int, ChestItems> chestItems = [];
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}
		public override void AI() {
			Projectile.timeLeft = 2;
			Player player = Main.player[Projectile.owner];
			OriginPlayer originPlayer = player.OriginPlayer();
			if (Projectile.owner == Main.myPlayer && (!originPlayer.goldenLotus || originPlayer.goldenLotusItem.shoot != Type || originPlayer.goldenLotusProj != Projectile.whoAmI)) {
				Projectile.ai[0] = -1;
			}
			void DisplayRange() {
				if (Projectile.owner != Main.myPlayer) return;
				for (float angle = 0; angle < MathHelper.TwoPi; angle += 1 / Projectile.localAI[0]) {
					Dust.NewDustPerfect(
						Projectile.position + GeometryUtils.Vec2FromPolar(Projectile.localAI[0] * range_per_frame, angle + Projectile.frameCounter),
						DustID.GoldFlame,
						Vector2.Zero
					).noGravity = true;
				}
			}
			switch ((int)Projectile.ai[0]) {
				case 0:
				if (Projectile.localAI[0] < expand_frames) Projectile.localAI[0]++;
				Projectile.position = player.Center;
				DisplayRange();
				if (Projectile.owner == Main.myPlayer) {
					for (int i = 0; i < Main.chest.Length; i++) {
						if (chestItems.ContainsKey(i)) continue;
						Chest chest = Main.chest[i];
						if (chest is null) continue;
						Vector2 pos = new Vector2(chest.x + 1, chest.y + 1) * 16;
						if (!Projectile.position.WithinRange(pos, Projectile.localAI[0] * range_per_frame)) continue;
						chestItems.Add(i, new(chest));
					}
				}
				break;

				case 1:
				if (Projectile.frameCounter >= 6) {
					Projectile.frameCounter = 0;
					if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
				}
				if (Main.chest.IndexInRange((int)Projectile.ai[1])) {
					Chest chest = Main.chest[(int)Projectile.ai[1]];
					if (chest is not null) {
						Vector2 direction = (new Vector2(chest.x + 1, chest.y + 1) * 16 - Projectile.position);
						float distance = direction.Length();
						if (distance > 8) {
							Projectile.velocity += direction * 2f / distance;
						} else {
							Projectile.ai[0] = 2;
						}
					}
				}
				if (Projectile.localAI[0] > 0) Projectile.localAI[0]--;
				Projectile.velocity *= 0.8f;
				Lighting.AddLight(Projectile.position, 1f, 0.882f, 0.686f);
				break;

				case 2:
				if (Projectile.frameCounter >= 6) {
					Projectile.frameCounter = 0;
					if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
				}
				if (Projectile.owner == Main.myPlayer && chestItems.TryGetValue((int)Projectile.ai[1], out ChestItems itemCluster)) {
					itemCluster.Drag(Projectile.position);
				}
				Projectile.velocity += Projectile.DirectionTo(player.Center) * 1f;
				if (player.Hitbox.Contains(Projectile.position)) {
					int oldChest = player.chest;
					try {
						player.chest = (int)Projectile.ai[1];
						ChestUI.LootAll();
					} finally {
						player.chest = oldChest;
					}
					Projectile.Kill();
				}
				if (Projectile.localAI[0] > 0) Projectile.localAI[0]--;
				Projectile.velocity *= 0.8f;
				Lighting.AddLight(Projectile.position, 1f, 0.882f, 0.686f);
				break;

				default:
				Projectile.position = player.Center;
				DisplayRange();
				if (--Projectile.localAI[0] <= 0) Projectile.Kill();
				break;
			}
			Projectile.spriteDirection = Projectile.direction; 
			DrawOffsetX = -12;
			DrawOriginOffsetY = -12;
			Projectile.frameCounter++;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overWiresUI.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			float vanishTimer = Projectile.localAI[0] - 10;
			if (vanishTimer < 0) vanishTimer = 0;
			bool hideFarie = Projectile.ai[0] <= 0;
			if (hideFarie || vanishTimer > 0) {
				if (Projectile.owner == Main.myPlayer) {
					foreach (KeyValuePair<int, ChestItems> chest in chestItems) {
						chest.Value.Draw(Projectile.position, chest.Key == Projectile.ai[1] ? 1 : vanishTimer / (expand_frames - 10));
					}
				}
				if (hideFarie) return false;
			} else if (chestItems.TryGetValue((int)Projectile.ai[1], out ChestItems itemCluster)) {
				itemCluster.Draw(Projectile.position, 1);
			}
			lightColor = Color.White * 0.8f;
			return true;
		}
		class ChestItems {
			List<(Item item, Vector2 offset, Vector2 velocity)> items = [];
			Vector2 basePosition; 
			public ChestItems(Chest chest) {
				basePosition = new Vector2(chest.x + 1, chest.y + 1) * 16;
				for (int i = 0; i < Chest.maxItems; i++) {
					Item item = chest.item[i];
					if (item?.IsAir ?? true) continue;
					items.Add((item, Vector2.Zero, Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(2f, 4f)));
					if (items.Count >= 10) break;
				}
			}
			public void Draw(Vector2 center, float alpha) {
				for (int i = 0; i < items.Count; i++) {
					(Item item, Vector2 offset, Vector2 velocity) = items[i];
					Vector2 position = basePosition + offset;
					Color color = new Color(255, 225, 175, 200) * alpha * 0.8f;
					float distSQ = center.DistanceSQ(position);
					if (distSQ > max_range * max_range) {
						distSQ = MathF.Sqrt(distSQ);
						color *= 1 - ((distSQ - max_range) / 32);
					}
					Main.DrawItemIcon(
						Main.spriteBatch,
						items[i].item,
						position - Main.screenPosition,
						color,
						32
					);
					items[i] = (item, offset + velocity, velocity * 0.93f);
				}
			}
			public void Drag(Vector2 center) {
				basePosition = center;
				for (int i = 0; i < items.Count; i++) {
					(Item item, Vector2 offset, Vector2 velocity) = items[i];
					float dist = offset.Length();
					if (dist > 24) velocity -= offset * 0.2f / dist;
					items[i] = (item, offset + velocity, velocity * 0.93f);
				}
			}
		}
	}
}

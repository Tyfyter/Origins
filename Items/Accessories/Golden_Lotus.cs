using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using Origins.Dev;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Golden_Lotus : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Misc
		];
		public override void SetStaticDefaults() {
			ArmorIDs.Face.Sets.DrawInFaceFlowerLayer[Item.faceSlot] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.shoot = ModContent.ProjectileType<Golden_Lotus_Fairy>();
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 4);
		}
		public override void UpdateAccessory(Player player, bool isHidden) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.fairyLotus = true;
			player.treasureMagnet = true;
			originPlayer.goldenLotus = true;
			originPlayer.goldenLotusItem = Item;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.TreasureMagnet)
			.AddIngredient(ModContent.ItemType<Fairy_Lotus>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.SubstituteKeybind(Keybindings.GoldenLotus);
		}
	}
	public class Golden_Lotus_Fairy : ModProjectile, IChestSyncRecipient {
		public const float max_range = 16 * 35;
		public const int expand_frames = 30;
		public const float range_per_frame = max_range / expand_frames;
		readonly Dictionary<int, ChestItems> chestItems = [];

		protected static List<int> fairiesOpeningChests = [];
		protected static List<int> nextFairiesOpeningChests = [];
		public override void Load() {
			if (GetType() != typeof(Golden_Lotus_Fairy)) return;
			IL_Chest.UpdateChestFrames += IL_Chest_UpdateChestFrames;
			On_Chest.UsingChest += On_Chest_UsingChest;
			On_Player.OpenChest += On_Player_OpenChest;
		}
		private static void IL_Chest_UpdateChestFrames(ILContext il) {
			ILCursor c = new(il);
			Type a = typeof(ILPatternMatchingExt);
			FieldReference _chestInUse = default;
			c.GotoNext(MoveType.After,
				i => i.MatchLdsfld(out _chestInUse) && _chestInUse.DeclaringType.Is(typeof(Chest)) && _chestInUse.Name == nameof(_chestInUse),
				i => i.MatchCallvirt<HashSet<int>>(nameof(HashSet<int>.Clear))
			);
			c.EmitLdsfld(_chestInUse);
			c.EmitDelegate(static (HashSet<int> _chestInUse) => {
				OriginExtensions.SwapClear(ref nextFairiesOpeningChests, ref fairiesOpeningChests);
				for (int i = 0; i < fairiesOpeningChests.Count; i++) _chestInUse.Add((int)Main.projectile[fairiesOpeningChests[i]].ai[1]);
			});
		}
		private static int On_Chest_UsingChest(On_Chest.orig_UsingChest orig, int i) {
			int value = orig(i);
			if (value == -1) {
				for (int j = 0; j < fairiesOpeningChests.Count; j++) {
					Projectile fairy = Main.projectile[fairiesOpeningChests[j]];
					if (i == fairy.ai[1]) return fairy.owner;
				}
			}
			return value;
		}
		private static void On_Player_OpenChest(On_Player.orig_OpenChest orig, Player self, int x, int y, int newChest) {
			for (int j = 0; j < fairiesOpeningChests.Count; j++) {
				Projectile fairy = Main.projectile[fairiesOpeningChests[j]];
				if (newChest == fairy.ai[1]) return;
			}
			orig(self, x, y, newChest);
		}


		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.netImportant = true;
		}
		int ChestIndex => (int)Projectile.ai[1];
		public override void AI() {
			Projectile.timeLeft = 2;
			Player player = Main.player[Projectile.owner];
			OriginPlayer originPlayer = player.OriginPlayer();
			if (Projectile.owner == Main.myPlayer && (!originPlayer.goldenLotus || originPlayer.goldenLotusItem.shoot != Type || originPlayer.goldenLotusProj != Projectile.whoAmI)) {
				Projectile.ai[0] = -1;
			}
			switch ((int)Projectile.ai[0]) {
				case 0:
				if (Projectile.localAI[0] < expand_frames) Projectile.localAI[0]++;
				Projectile.position = player.Center;
				DisplayRange();
				if (Projectile.owner == Main.myPlayer) {
					for (int i = 0; i < Main.chest.Length; i++) {
						Chest chest = Main.chest[i];
						if (chest is null) continue;

						int opener = Chest.UsingChest(i);
						bool isChestOpen = opener != -1 && (opener != Projectile.owner || player.chest == i);
						if (chestItems.ContainsKey(i)) {
							if (isChestOpen) chestItems.Remove(i);
							continue;
						}
						if (isChestOpen) continue;

						Vector2 pos = new Vector2(chest.x + 1, chest.y + 1) * 16;
						if (!Projectile.position.WithinRange(pos, Projectile.localAI[0] * range_per_frame)) continue;
						if (Main.netMode == NetmodeID.SinglePlayer) {
							chestItems.Add(i, new(chest));
						} else {
							chestItems.Add(i, null);
							this.RequestChestSync(i);
						}
					}
				}
				break;

				case -2:
				Projectile.netUpdate = true;
				if (!chestItems.TryGetValue(ChestIndex, out ChestItems targetChest) || targetChest is null) {
					Projectile.ai[0] = 0;
					goto case 0;
				}
				Projectile.ai[0] = 1;
				goto case 1;
				case 1:
				if (Projectile.frameCounter >= 6) {
					Projectile.frameCounter = 0;
					if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
				}
				if (Main.chest.IndexInRange(ChestIndex)) {
					Chest chest = Main.chest[ChestIndex];
					if (chest is not null) {
						Vector2 direction = (new Vector2(chest.x + 1, chest.y + 1) * 16 - Projectile.position);
						float distance = direction.Length();
						if (distance > 8) {
							Projectile.velocity += direction * 2f / distance;
						} else {
							Projectile.ai[0] = 2;
						}
					}
					nextFairiesOpeningChests.Add(Projectile.whoAmI);
				} else {
					Projectile.localAI[0] = 0;
					Projectile.ai[0] = -1;
				}
				if (Projectile.localAI[0] > 0) Projectile.localAI[0]--;
				Projectile.velocity *= 0.8f;
				AddFairyLight();
				break;

				case 2:
				nextFairiesOpeningChests.Add(Projectile.whoAmI);
				if (Projectile.frameCounter >= 6) {
					Projectile.frameCounter = 0;
					if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
				}
				if (Projectile.owner == Main.myPlayer && chestItems.TryGetValue((int)Projectile.ai[1], out ChestItems itemCluster)) {
					itemCluster.Drag(Projectile.position);
				}
				Projectile.velocity += Projectile.DirectionTo(player.Center) * 1f;
				if (player.Hitbox.Contains(Projectile.position)) {
					GetItemSettings lootAllSettingsRegularChest = GetItemSettings.LootAllSettingsRegularChest;
					Chest chest = Main.chest[ChestIndex];
					for (int i = 0; i < 40; i++) {
						if (chest.item[i]?.IsAir == false) {
							chest.item[i].position = player.Center;
							chest.item[i] = player.GetItem(Main.myPlayer, chest.item[i], lootAllSettingsRegularChest);
							if (Main.netMode == NetmodeID.MultiplayerClient)
								NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, ChestIndex, i);
						}
					}
					Projectile.Kill();
				}
				if (Projectile.localAI[0] > 0) Projectile.localAI[0]--;
				Projectile.velocity *= 0.8f;
				AddFairyLight();
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
		public virtual void DisplayRange() {
			if (Projectile.owner != Main.myPlayer) return;
			for (float angle = 0; angle < MathHelper.TwoPi; angle += 1 / Projectile.localAI[0]) {
				Dust dust = Dust.NewDustPerfect(
					Projectile.position + GeometryUtils.Vec2FromPolar(Projectile.localAI[0] * range_per_frame, angle + Projectile.frameCounter),
					DustID.GoldFlame,
					Vector2.Zero
				);
				dust.noGravity = true;
				dust.noLight = true;
			}
		}
		public virtual void AddFairyLight() {
			Lighting.AddLight(Projectile.position, 1f, 0.882f, 0.686f);
		}
		public virtual Color GetItemColor(Item item, Vector2 position, Vector2 velocity) => new Color(255, 225, 175, 200) * 0.8f;
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
						chest.Value?.Draw(Projectile.position, this, chest.Key == ChestIndex ? 1 : vanishTimer / (expand_frames - 10));
					}
				}
				if (hideFarie) return false;
			} else if (chestItems.TryGetValue(ChestIndex, out ChestItems itemCluster)) {
				itemCluster.Draw(Projectile.position, this, 1);
			}
			lightColor = Color.White * 0.8f;
			return true;
		}
		public void ReceiveChestSync(int i) {
			chestItems[i] = new(Main.chest[i]);
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
			public void Draw(Vector2 center, Golden_Lotus_Fairy colorProvider, float alpha) {
				for (int i = 0; i < items.Count; i++) {
					(Item item, Vector2 offset, Vector2 velocity) = items[i];
					Vector2 position = basePosition + offset;
					Color color = colorProvider.GetItemColor(item, offset, velocity) * alpha;
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

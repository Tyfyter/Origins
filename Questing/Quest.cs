using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	public abstract class Quest : ModType {
		/// <summary>
		/// not yet synced, don't use until it is
		/// </summary>
		public virtual bool SaveToWorld => false;
		public virtual bool Started => false;
		public virtual bool Completed => false;
		public string NameKey { get; protected set; }
		public string NameValue => Language.GetTextValue(NameKey);
		public virtual int Stage { get; set; }
		public bool ShouldSync { get; protected set; }
		public bool LocalPlayerStarted {
			get => Main.LocalPlayer.GetModPlayer<OriginPlayer>().startedQuests.Contains(NameKey);
			set {
				if (value) {
					Main.LocalPlayer.GetModPlayer<OriginPlayer>().startedQuests.Add(NameKey);
				} else {
					Main.LocalPlayer.GetModPlayer<OriginPlayer>().startedQuests.Remove(NameKey);
				}
			}
		}
		public virtual bool HasStartDialogue(NPC npc) {
			return false;
		}
		public virtual bool HasDialogue(NPC npc) {
			return false;
		}
		public virtual string GetDialogue() {
			return "";
		}
		public virtual void OnDialogue() {

		}
		public virtual bool ShowInJournal() {
			return Started;
		}
		public virtual bool GrayInJournal() {
			return Completed;
		}
		public virtual string GetJournalPage() {
			return "";
		}
		public virtual void SaveData(TagCompound tag) { }
		public virtual void LoadData(TagCompound tag) { }
		#region events
		public Action<NPC> KillEnemyEvent { get; protected set; }
		public Action PreUpdateInventoryEvent { get; protected set; }
		public Action<Item> UpdateInventoryEvent { get; protected set; }
		#endregion events
		public sealed override void SetupContent() {
			SetStaticDefaults();
		}
		protected sealed override void Register() {
			if (SaveToWorld && Mod.Side != ModSide.Both) {
				throw new Exception($"The Quest \"{NameValue}\" is saved to the world but not in a \"Both\" side mod, this is not allowed as it would break networking");
			}
			ModTypeLookup<Quest>.Register(this);
			NameKey ??= $"Mods.{FullName}";
			Quest_Registry.RegisterQuest(this);
		}
		public int Type { get; internal set; }
		public int NetID { get; internal set; } = -1;
		public static string StageTagOption(bool completed) => completed ? "/completed" : "";
		public static void ConsumeItems(Item[] inventory, params (Predicate<Item> match, int count)[] items) {
			for (int j = 0; j < inventory.Length; j++) {
				Item item = inventory[j];
				for (int i = 0; i < items.Length; i++) {
					var current = items[i];
					if (current.match(item)) {
						if (current.count >= item.stack) {
							current.count -= item.stack;
							item.TurnToAir();
						} else {
							item.stack -= current.count;
							current.count = 0;
						}
					}
				}
			}
		}
		public void CheckSync() {
			if (ShouldSync) {
				Sync();
			}
		}
		public void Sync(int toClient = -1, int ignoreClient = -1) {
			if (Main.netMode == NetmodeID.SinglePlayer) return;
			ModPacket packet = Origins.instance.GetPacket();
			packet.Write(Origins.NetMessageType.sync_quest);
			packet.Write(Type);
			SendSync(packet);
			packet.Send(toClient, ignoreClient);
			ShouldSync = false;
		}
		public virtual void SendSync(BinaryWriter writer) { }
		public virtual void ReceiveSync(BinaryReader reader) { }
		public static Condition QuestCondition<T>() where T : Quest {
			return new Condition(
				Language.GetOrRegister("Mods.Origins.Conditions.QuestCompleted").WithFormatArgs(ModContent.GetInstance<T>().NameKey),
				() => ModContent.GetInstance<T>().Completed
			);
		}
	}
}

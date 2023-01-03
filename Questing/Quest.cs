using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
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
		public virtual void SaveData(TagCompound tag) {}
		public virtual void LoadData(TagCompound tag) {}
		#region events
		public Action<NPC> KillEnemyEvent { get; protected set; }
		public Action PreUpdateInventoryEvent { get; protected set; }
		public Action<Item> UpdateInventoryEvent { get; protected set; }
		#endregion events
		public sealed override void SetupContent() {
			SetStaticDefaults();
		}
		protected sealed override void Register() {
			ModTypeLookup<Quest>.Register(this);
			NameKey ??= $"Mods.{FullName}";
			if (Quest_Registry.Quests is null) Quest_Registry.Quests = new Dictionary<string, Quest>();
			Quest_Registry.Quests.Add(FullName, this);
		}
	}
}

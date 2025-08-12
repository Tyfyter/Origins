using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Core {
	public abstract record class SyncedAction : ILoadable {
		static readonly List<SyncedAction> actions = [];
		static readonly Dictionary<Type, ushort> actionIDsByType = [];
		public static SyncedAction Get(int type) => actions[type];
		protected SyncedAction() {
			actionIDsByType?.TryGetValue(GetType(), out type);
		}
		private ushort type;
		public virtual bool ServerOnly => false;
		protected virtual bool ShouldPerform => true;
		public void Load(Mod mod) {
			type = (ushort)actions.Count;
			actions.Add(this);
			actionIDsByType.Add(GetType(), type);
		}
		public void Unload() { }
		public SyncedAction Read(BinaryReader reader) {
			return NetReceive(reader);
		}
		/// <summary>
		/// Performs the action, then sends it if appropriate
		/// </summary>
		/// <param name="fromClient"></param>
		public void Perform(int fromClient = -2) {
			if (!ShouldPerform) return;
			if (NetmodeActive.Server || !ServerOnly) Perform();
			if ((NetmodeActive.Server && !ServerOnly) || (NetmodeActive.MultiplayerClient && fromClient == -2)) {
				Send(ignoreClient: fromClient);
			}
		}
		public void Send(int toClient = -1, int ignoreClient = -1) {
			if (NetmodeActive.SinglePlayer) return;
			ModPacket packet = Origins.instance.GetPacket();
			packet.Write(Origins.NetMessageType.synced_action);
			packet.Write(type);
			NetSend(packet);
			packet.Send(toClient, ignoreClient);
		}
		protected abstract void Perform();
		public abstract SyncedAction NetReceive(BinaryReader reader);
		public abstract void NetSend(BinaryWriter writer);
	}
}

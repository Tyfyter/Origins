using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader;

namespace Origins.Core {
	public abstract record class SyncedAction : ILoadable {
		static readonly List<SyncedAction> actions = [];
		public static SyncedAction Get(int type) => actions[type];
		private ushort type;
		public virtual bool ServerOnly => false;
		public void Load(Mod mod) {
			type = (ushort)actions.Count;
			actions.Add(this);
		}
		public void Unload() { }
		public SyncedAction Read(BinaryReader reader) {
			return NetReceive(reader);
		}
		public void Perform(int fromClient = -2) {
			Perform();
			if ((NetmodeActive.Server && !ServerOnly) || (NetmodeActive.MultiplayerClient && fromClient == -2)) {
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.synced_action);
				packet.Write(type);
				NetSend(packet);
				packet.Send(ignoreClient: fromClient);
			}
		}
		protected abstract void Perform();
		public abstract SyncedAction NetReceive(BinaryReader reader);
		public abstract void NetSend(BinaryWriter writer);
	}
}

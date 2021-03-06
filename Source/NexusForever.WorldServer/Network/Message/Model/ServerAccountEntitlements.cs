﻿using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerAccountEntitlements, MessageDirection.Server)]
    public class ServerAccountEntitlements : IWritable
    {
        public class AccountEntitlementInfo : IWritable
        {
            public Entitlement Entitlement { get; set; }
            public uint Count { get; set; }

            public void Write(GamePacketWriter writer)
            {
                writer.Write(Entitlement, 32u);
                writer.Write(Count);
            }
        }

        public List<AccountEntitlementInfo> Entitlements { get; } = new List<AccountEntitlementInfo>();

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Entitlements.Count);
            Entitlements.ForEach(e => e.Write(writer));
        }
    }
}

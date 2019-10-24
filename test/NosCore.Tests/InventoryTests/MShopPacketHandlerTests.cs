﻿//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// 
// Copyright (C) 2019 - NosCore
// 
// NosCore is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using ChickenAPI.Packets.ClientPackets.Inventory;
using ChickenAPI.Packets.ClientPackets.Shops;
using ChickenAPI.Packets.Enumerations;
using ChickenAPI.Packets.ServerPackets.Chats;
using ChickenAPI.Packets.ServerPackets.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.Core.I18N;
using NosCore.Data;
using NosCore.Data.Enumerations.Group;
using NosCore.Data.Enumerations.I18N;
using NosCore.Data.StaticEntities;
using NosCore.GameObject;
using NosCore.GameObject.Networking;
using NosCore.GameObject.Networking.ClientSession;
using NosCore.GameObject.Providers.InventoryService;
using NosCore.GameObject.Providers.ItemProvider;
using NosCore.GameObject.Providers.ItemProvider.Item;
using NosCore.PacketHandlers.Shops;
using NosCore.Tests.Helpers;

namespace NosCore.Tests.InventoryTests
{
    [TestClass]
    public class MShopPacketHandlerTests
    {
        private readonly MShopPacket _shopPacket = new MShopPacket
        {
            Type = CreateShopPacketType.Open,
            ItemList = new List<MShopItemSubPacket>
            {
                new MShopItemSubPacket {Type = PocketType.Etc, Slot = 0, Amount = 1, Price = 10000},
                new MShopItemSubPacket {Type = PocketType.Etc, Slot = 1, Amount = 2, Price = 20000},
                new MShopItemSubPacket {Type = PocketType.Etc, Slot = 2, Amount = 3, Price = 30000}
            },
            Name = "TEST SHOP"
        };

        private MShopPacketHandler _mShopPacketHandler;

        private ClientSession _session;

        [TestInitialize]
        public void Setup()
        {
            TestHelpers.Reset();
            Broadcaster.Reset();

            _session = TestHelpers.Instance.GenerateSession();
            _session.Character.MapInstance.Portals = new List<Portal>
            {
                new Portal
                {
                    DestinationMapId = _session.Character.MapInstance.Map.MapId,
                    Type = PortalType.Open,
                    SourceMapInstanceId = _session.Character.MapInstance.MapInstanceId,
                    DestinationMapInstanceId = _session.Character.MapInstance.MapInstanceId,
                    DestinationX = 5,
                    DestinationY = 5,
                    PortalId = 1,
                    SourceMapId = _session.Character.MapInstance.Map.MapId,
                    SourceX = 0,
                    SourceY = 0
                }
            };
            _mShopPacketHandler = new MShopPacketHandler();
        }

        [TestMethod]
        public void UserCanNotCreateShopCloseToPortal()
        {
            _mShopPacketHandler.Execute(_shopPacket, _session);
            var packet = (MsgPacket) _session.LastPackets.FirstOrDefault(s => s is MsgPacket);
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.SHOP_NEAR_PORTAL, _session.Account.Language));
            Assert.IsNull(_session.Character.Shop);
        }

        [TestMethod]
        public void UserCanNotCreateShopInTeam()
        {
            _session.Character.PositionX = 7;
            _session.Character.PositionY = 7;
            _session.Character.Group = new Group(GroupType.Team);
            _mShopPacketHandler.Execute(_shopPacket, _session);
            var packet = (MsgPacket) _session.LastPackets.FirstOrDefault(s => s is MsgPacket);
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.SHOP_NOT_ALLOWED_IN_RAID, _session.Account.Language));
            Assert.IsNull(_session.Character.Shop);
        }

        [TestMethod]
        public void UserCanCreateShopInGroup()
        {
            _session.Character.PositionX = 7;
            _session.Character.PositionY = 7;
            _session.Character.Group = new Group(GroupType.Group);
            _mShopPacketHandler.Execute(_shopPacket, _session);
            var packet = (MsgPacket) _session.LastPackets.FirstOrDefault(s => s is MsgPacket);
            Assert.IsTrue(packet.Message !=
                Language.Instance.GetMessageFromKey(LanguageKey.SHOP_NOT_ALLOWED_IN_RAID, _session.Account.Language));
        }

        [TestMethod]
        public void UserCanNotCreateShopInNotShopAllowedMaps()
        {
            _session.Character.PositionX = 7;
            _session.Character.PositionY = 7;
            _mShopPacketHandler.Execute(_shopPacket, _session);
            var packet = (MsgPacket) _session.LastPackets.FirstOrDefault(s => s is MsgPacket);

            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.SHOP_NOT_ALLOWED, _session.Account.Language));
            Assert.IsNull(_session.Character.Shop);
        }


        [TestMethod]
        public void UserCanNotCreateShopWithMissingItem()
        {
            var items = new List<ItemDto>
            {
                new Item {Type = NoscorePocketType.Etc, VNum = 1}
            };
            var itemBuilder = new ItemProvider(items,
                new List<IEventHandler<Item, Tuple<InventoryItemInstance, UseItemPacket>>>());

            _session.Character.Inventory.AddItemToPocket(InventoryItemInstance.Create(itemBuilder.Create(1, 1), 0));
            _session.Character.MapInstance = TestHelpers.Instance.MapInstanceProvider.GetBaseMapById(1);
            _mShopPacketHandler.Execute(_shopPacket, _session);
            Assert.IsNull(_session.Character.Shop);
        }


        [TestMethod]
        public void UserCanNotCreateShopWithMissingAmountItem()
        {
            var items = new List<ItemDto>
            {
                new Item {Type = NoscorePocketType.Etc, VNum = 1}
            };
            var itemBuilder = new ItemProvider(items,
                new List<IEventHandler<Item, Tuple<InventoryItemInstance, UseItemPacket>>>());

            _session.Character.Inventory.AddItemToPocket(InventoryItemInstance.Create(itemBuilder.Create(1, 1), 0),
                NoscorePocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(InventoryItemInstance.Create(itemBuilder.Create(1, 1), 0),
                NoscorePocketType.Etc, 1);
            _session.Character.Inventory.AddItemToPocket(InventoryItemInstance.Create(itemBuilder.Create(1, 1), 0),
                NoscorePocketType.Etc, 2);

            _session.Character.MapInstance = TestHelpers.Instance.MapInstanceProvider.GetBaseMapById(1);
            _mShopPacketHandler.Execute(_shopPacket, _session);
            Assert.IsNull(_session.Character.Shop);
            var packet = (SayPacket) _session.LastPackets.FirstOrDefault(s => s is SayPacket);
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.SHOP_ONLY_TRADABLE_ITEMS, _session.Account.Language));
        }

        [TestMethod]
        public void UserCanCreateShop()
        {
            var items = new List<ItemDto>
            {
                new Item {Type = NoscorePocketType.Etc, VNum = 1, IsTradable = true}
            };
            var itemBuilder = new ItemProvider(items,
                new List<IEventHandler<Item, Tuple<InventoryItemInstance, UseItemPacket>>>());

            _session.Character.Inventory.AddItemToPocket(InventoryItemInstance.Create(itemBuilder.Create(1, 1), 0),
                NoscorePocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(InventoryItemInstance.Create(itemBuilder.Create(1, 2), 0),
                NoscorePocketType.Etc, 1);
            _session.Character.Inventory.AddItemToPocket(InventoryItemInstance.Create(itemBuilder.Create(1, 3), 0),
                NoscorePocketType.Etc, 2);

            _session.Character.MapInstance = TestHelpers.Instance.MapInstanceProvider.GetBaseMapById(1);
            _mShopPacketHandler.Execute(_shopPacket, _session);
            Assert.IsNotNull(_session.Character.Shop);
        }

        public void UserCanNotCreateShopInExchange()
        {
            _session.Character.InExchangeOrTrade = true;
            var items = new List<ItemDto>
            {
                new Item {Type = NoscorePocketType.Etc, VNum = 1, IsTradable = true}
            };
            var itemBuilder = new ItemProvider(items,
                new List<IEventHandler<Item, Tuple<InventoryItemInstance, UseItemPacket>>>());

            _session.Character.Inventory.AddItemToPocket(InventoryItemInstance.Create(itemBuilder.Create(1, 1), 0),
                NoscorePocketType.Etc, 0);
            _session.Character.Inventory.AddItemToPocket(InventoryItemInstance.Create(itemBuilder.Create(1, 2), 0),
                NoscorePocketType.Etc, 1);
            _session.Character.Inventory.AddItemToPocket(InventoryItemInstance.Create(itemBuilder.Create(1, 3), 0),
                NoscorePocketType.Etc, 2);

            _session.Character.MapInstance = TestHelpers.Instance.MapInstanceProvider.GetBaseMapById(1);
            _mShopPacketHandler.Execute(_shopPacket, _session);
            Assert.IsNull(_session.Character.Shop);
        }

        [TestMethod]
        public void UserCanNotCreateEmptyShop()
        {
            _session.Character.MapInstance = TestHelpers.Instance.MapInstanceProvider.GetBaseMapById(1);

            _mShopPacketHandler.Execute(new MShopPacket
            {
                Type = CreateShopPacketType.Open,
                ItemList = new List<MShopItemSubPacket>(),
                Name = "TEST SHOP"
            }, _session);
            Assert.IsNull(_session.Character.Shop);
            var packet = (SayPacket) _session.LastPackets.FirstOrDefault(s => s is SayPacket);
            Assert.IsTrue(packet.Message ==
                Language.Instance.GetMessageFromKey(LanguageKey.SHOP_EMPTY, _session.Account.Language));
        }
    }
}
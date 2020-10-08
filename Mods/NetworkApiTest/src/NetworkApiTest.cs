﻿using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace NetworkApiTest
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class NetworkApiTestMessage
    {
        public string message;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class NetworkApiTestResponse
    {
        public string response;
    }

    /// <summary>
    /// A basic example of client<->server networking using a custom network communication
    /// </summary>
    public class NetworkApiTest : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.Network.RegisterChannel("networkapitest")
                .RegisterMessageType(typeof(NetworkApiTestMessage))
                .RegisterMessageType(typeof(NetworkApiTestResponse))
            ;
        }

        #region Client
        ICoreClientAPI clientApi;

        public override void StartClientSide(ICoreClientAPI api)
        {
            clientApi = api;

            api.Network.GetChannel("networkapitest")
                .SetMessageHandler<NetworkApiTestMessage>(OnServerMessage)
            ;
        }

        private void OnServerMessage(NetworkApiTestMessage networkMessage)
        {
            clientApi.ShowChatMessage("Received following message from server: " + networkMessage.message);
            clientApi.ShowChatMessage("Sending response.");
            clientApi.Network.GetChannel("networkapitest").SendPacket(new NetworkApiTestResponse()
            {
                response = "RE: Hello World!"
            });
        }

        #endregion

        #region Server
        ICoreServerAPI serverApi;

        public override void StartServerSide(ICoreServerAPI api)
        {
            serverApi = api;

            api.Network.GetChannel("networkapitest")
                .SetMessageHandler<NetworkApiTestResponse>(OnClientMessage)
            ;

            api.RegisterCommand("nwtest", "Send a test network message", "", OnNwTestCmd, Privilege.controlserver);
        }

        private void OnNwTestCmd(IServerPlayer player, int groupId, CmdArgs args)
        {
            serverApi.Network.GetChannel("networkapitest").BroadcastPacket(new NetworkApiTestMessage()
            {
                message = "Hello World!",
            });
        }

        private void OnClientMessage(IPlayer fromPlayer, NetworkApiTestResponse networkMessage)
        {
            serverApi.SendMessageToGroup(
                GlobalConstants.GeneralChatGroup,
                "Received following response from " + fromPlayer.PlayerName + ": " + networkMessage.response,
                EnumChatType.Notification
            );
        }

        
        #endregion
    }
}

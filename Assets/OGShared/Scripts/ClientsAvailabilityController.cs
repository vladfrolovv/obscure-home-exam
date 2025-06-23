using System;
using System.Collections.Generic;
using Fusion;
using UniRx;
using UnityEngine;
namespace OGShared
{
    public class ClientsAvailabilityController
    {
        private readonly HashSet<PlayerRef> _readyPlayers = new();
        private readonly Subject<Unit> _clientsReady = new();

        public IObservable<Unit> ClientsReady => _clientsReady;

        public void OnClientReady(PlayerRef playerRef)
        {
            _readyPlayers.Add(playerRef);
            if (_readyPlayers.Count != BaseConstants.PLAYERS_PER_MATCH) return;

            Debug.Log("[SERVER] All players are ready!");
            _clientsReady?.OnNext(Unit.Default);
        }
    }
}

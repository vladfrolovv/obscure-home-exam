using System;
using System.Collections.Generic;
using UnityEngine;
namespace OGClient.Gameplay.Mathchmaking
{
    [CreateAssetMenu(fileName = "MatchmakingState", menuName = "ScriptableObjects/Matchmaking State")]
    public class ScriptableMatchmakingState : ScriptableObject
    {

        [SerializeField] private List<MatchmakingDescription> _matchmakingStates = new();

        public string this [MatchmakingState state] =>
            _matchmakingStates.Find(x => x.CurrentState == state)?.Description ?? "Unknown State";

        [Serializable]
        private class MatchmakingDescription
        {

            [field: SerializeField]
            public MatchmakingState CurrentState { get; private set; }

            [field: SerializeField]
            public string Description { get; private set; }

        }

    }
}

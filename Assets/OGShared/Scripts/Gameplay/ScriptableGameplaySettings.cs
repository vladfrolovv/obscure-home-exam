using System;
using OGShared.Gameplay;
using UnityEngine;
namespace OGShared.Gameplay
{
    [CreateAssetMenu(fileName = "GameplaySettings", menuName = "ScriptableObjects/Gameplay Settings")]
    public class ScriptableGameplaySettings : ScriptableObject
    {

        [field: SerializeField, Header("Base Gameplay Settings")]
        public float StartDelay { get; private set; } = 3f;

        [field: SerializeField]
        public float TimePerTurn { get; private set; } = 20f;

        [field: SerializeField]
        public int MovesPerTurn { get; private set; } = 2;

        [field: SerializeField]
        public int ExtraMovesPerLink { get; private set; } = 15;

        [field: SerializeField]
        public int RoundsPerGame { get; private set; } = 5;

        [Header("Texts")]
        [SerializeField] private TextInfo<MatchPhase>[] _matchPhaseTexts;

        public string this[MatchPhase phase] =>
            Array.Find(_matchPhaseTexts, textInfo => textInfo.Type == phase)?.Text ?? string.Empty;

        [Serializable]
        public class TextInfo<T>
        {
            [field: SerializeField] public T Type { get; private set; }
            [field: SerializeField] public string Text { get; private set; }
        }

    }
}

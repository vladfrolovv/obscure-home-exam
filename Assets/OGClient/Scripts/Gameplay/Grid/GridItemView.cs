using System;
using System.Linq;
using OGClient.Gameplay.Grid.Configs;
using OGClient.Gameplay.Grid.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using UnityEngine.Serialization;
namespace OGClient.Gameplay.Grid
{
    public class GridItemView : MonoBehaviour
    {

        [Header("Item Base")]
        [SerializeField] private ScriptableGridItem _customItem;
        [SerializeField, FormerlySerializedAs("_type")] private int _gridItemType;
        [SerializeField] private Canvas _canvas;

        [Header("Item View")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _shadowImage;
        [SerializeField] private Image _glowImage;
        [SerializeField] private TextMeshProUGUI _text;

        [Header("Animations")]
        [SerializeField] private Animator _itemAnimator;
        [SerializeField] private Animator _glowAnimator;
        [SerializeField] private string _clearAnimationProperty;

        [SerializeField] private string _collectAnimationProperty;
        [SerializeField] private GridItemCollectEffectView _collectEffect;

        [field: SerializeField, Header("Other")] public bool CanMerge { get; private set; } = true;
        [field: SerializeField] public float ThrowDistance { get; private set; } = 0.4f;
        [field: SerializeField] public float ExtraExecuteTime { get; private set; }

        [Header("Oriented Items"), NonReorderable]
        [SerializeField] private OrientedItemModel[] _otherOrientations;

        [HideInInspector] public bool IsSpawning;
        [HideInInspector] public bool IsClearing;
        [HideInInspector] public bool IsMerging;
        [HideInInspector] public bool IsLastInLink;

        public Color Color { get; private set; } = Color.white;

        public int GridItemType => _gridItemType;
        public Canvas GridItemCanvas => _canvas;
        public bool HasOtherOrientations => _otherOrientations.Length > 0;

        public void SetGlowAnimator(bool active) => _glowAnimator.enabled = active;
        public void SetType(int setValue) => _gridItemType = setValue;
        public void PlayAnimation(string setValue) => _itemAnimator.Play(setValue);
        public void SetAnimatorBool(string stateName, bool setValue) => _itemAnimator.SetBool(stateName, setValue);

        public string ClearAnimationProperty => _clearAnimationProperty;

        public void InstallGridItem(ScriptableGridItem gridItemInfo)
        {
            if (gridItemInfo == null) return;

            _iconImage.sprite = gridItemInfo.Icon;
            _shadowImage.sprite = gridItemInfo.Shadow;
            _glowImage.sprite = gridItemInfo.Glow;
            _text.SetText(string.Empty);

            Color = gridItemInfo.Color;
        }

        public void PlayDelayedAnimation(string setValue, float delay)
        {
            Observable.Timer(TimeSpan.FromSeconds(delay)).Subscribe(_ => PlayAnimation(setValue));
        }

        public GridItemView GetOtherOrientation(Vector2 orientation)
        {
            return _otherOrientations
                .FirstOrDefault(item => item.Orientations.Any(o => o == orientation))
                ?.GridItemView;
        }

        public void TryToClear()
        {
            if (!_collectEffect) return;

            GridItemCollectEffectView effect = Instantiate(_collectEffect, transform.position, Quaternion.identity);
            effect.DestroyInSeconds(2f);

            ParticleSystem.MainModule particle = effect.GetComponent<ParticleSystem>().main;
            particle.startColor = Color;

            if (string.IsNullOrEmpty(_clearAnimationProperty) || _gridItemType < 0) return;
            _canvas.enabled = false;
        }

        public void TryToCollect()
        {
            if (string.IsNullOrEmpty(_collectAnimationProperty)) return;
            PlayAnimation(_collectAnimationProperty);
        }

        public void DisableGlow()
        {
            _glowAnimator.enabled = false;
            _glowImage.color = Color.white;
        }

        private void Start()
        {
            InstallGridItem(_customItem);
        }

        private void OnValidate()
        {
            if (!Application.isEditor) return;
            InstallGridItem(_customItem);
        }

    }
}

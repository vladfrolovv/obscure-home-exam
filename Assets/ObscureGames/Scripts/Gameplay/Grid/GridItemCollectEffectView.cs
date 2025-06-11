using UnityEngine;
namespace ObscureGames.Gameplay.Grid
{
    public class GridItemCollectEffectView : MonoBehaviour
    {

        [field: SerializeField]
        public ParticleSystem ParticleSystem { get; private set; }

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public void DestroyInSeconds(float seconds)
        {
            Destroy(gameObject, seconds);
        }

    }
}

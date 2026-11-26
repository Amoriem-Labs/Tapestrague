using System;
using DG.Tweening;
using UnityEngine;

namespace PrototypeLevel
{
    public class DoorControl : MonoBehaviour, IInteractablePrototype
    {
        [SerializeField] private SpriteRenderer _doorSprite;
        private Collider2D _coll;

        [SerializeField] private AudioSource _audioSource;
        
        private void Awake()
        {
            _coll = GetComponent<Collider2D>();
            _coll.enabled = false;
        }

        public void OpenDoor()
        {
            _doorSprite.transform
                .DORotate(new Vector3(0, 90, 0), 0.5f)
                .SetDelay(2f)
                .OnComplete(() => _coll.enabled = true);
        }

        public void Interact()
        {
            Debug.Log("Demo Complete!");
            _audioSource.Play();
            
            // TODO: consider add a transition back to main screen
        }
    }
}
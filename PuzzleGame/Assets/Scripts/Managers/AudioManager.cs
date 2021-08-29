using PuzzleGame.EventSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public class AudioManager : MonoBehaviour
    {
        //item in the audio pooling system
        //this is used for one shot sounds
        private class AudioPoolItem
        {
            public float unscaledVolume = 1;
            public GameObject gameObj = null;
            public Transform transform = null;
            public AudioSource audioSrc = null;
            public float unImportance = float.MaxValue;
            public bool isPlaying = false;
            public IEnumerator routine = null;
            public ulong id = 0;
        }

        [SerializeField] int _poolSize = 10;
        [SerializeField] float _constSoundFadeDuration = 1.5f;
        float _volumeScale = 1;
        public float volumeScale 
        {
            get => _volumeScale; 
            set
            {
                _volumeScale = Mathf.Clamp(value, 0, 1);
                
                if (_constantAudioSrc.isPlaying)
                    _constantAudioSrc.audioSrc.volume = _constantAudioSrc.unscaledVolume * value;

                foreach(var poolItem in _activePool.Values)
                {
                    poolItem.audioSrc.volume = poolItem.unscaledVolume * value;
                }
            }
        }
        ulong _nextId = 0;

        AudioPoolItem _constantAudioSrc;
        List<AudioPoolItem> _pool = new List<AudioPoolItem>();
        //an id-to-source dictionary to keep track of active audio sources in the pool
        private Dictionary<ulong, AudioPoolItem> _activePool = new Dictionary<ulong, AudioPoolItem>();

        private void Awake()
        {
            if (GameContext.s_audioMgr != null)
                Destroy(this);
            else
                GameContext.s_audioMgr = this;

            Messenger.AddListener(M_EventType.ON_GAME_PAUSED, OnGamePaused);
            Messenger.AddListener(M_EventType.ON_GAME_RESUMED, OnGameResumed);

            GameObject go;
            AudioSource src;
            // Generate Pool
            for (int i = 0; i < _poolSize; i++)
            {
                // Create GameObject and assigned AudioSource and Parent
                go = new GameObject("Audio Pool Item");
                src = go.AddComponent<AudioSource>();
                go.transform.parent = transform;

                // Create and configure Pool Item
                AudioPoolItem poolItem = new AudioPoolItem();
                poolItem.gameObj = go;
                poolItem.audioSrc = src;
                poolItem.transform = go.transform;
                poolItem.isPlaying = false;
                go.SetActive(false);
                _pool.Add(poolItem);
            }

            _constantAudioSrc = new AudioPoolItem();
            go = new GameObject("Constant Audio Source");
            src = go.AddComponent<AudioSource>();
            go.transform.parent = transform;
            _constantAudioSrc.gameObj = go;
            _constantAudioSrc.audioSrc = src;
            _constantAudioSrc.audioSrc.loop = true;
            _constantAudioSrc.audioSrc.spatialBlend = 0;
            _constantAudioSrc.transform = go.transform;
            _constantAudioSrc.isPlaying = false;
            go.SetActive(false);
        }

        #region Messenger Events
        private void OnGamePaused()
        {
            //pause all pooled sounds
            foreach (AudioPoolItem item in _activePool.Values)
            {
                item.audioSrc.Pause();
            }
        }

        private void OnGameResumed()
        {
            //resume all pooled sounds
            foreach (AudioPoolItem item in _activePool.Values)
            {
                item.audioSrc.UnPause();
            }
        }
        #endregion

        private IEnumerator _stopSoundDelayedRoutine(ulong id, float duration)
        {
            yield return new WaitForSeconds(duration);
            AudioPoolItem activeSound;

            // If this if exists in our active pool
            if (_activePool.TryGetValue(id, out activeSound))
            {
                activeSound.audioSrc.Stop();
                activeSound.audioSrc.clip = null;
                activeSound.gameObj.SetActive(false);
                _activePool.Remove(id);

                // Make it available again
                activeSound.isPlaying = false;
            }
        }

        private ulong ConfigurePoolObject(int poolIndex, AudioClip clip, Vector2 position, float volume, float unimportance)
        {
            // If poolIndex is out of range abort request
            if (poolIndex < 0 || poolIndex >= _pool.Count) 
                return 0;

            // Get the pool item
            AudioPoolItem poolItem = _pool[poolIndex];

            // Configure the audio source's position and colume
            AudioSource source = poolItem.audioSrc;
            source.clip = clip;
            source.volume = volume * volumeScale;

            // Position source at requested position
            source.transform.position = position;

            // Enable GameObject and record that it is now playing
            poolItem.unscaledVolume = volume;
            poolItem.isPlaying = true;
            poolItem.unImportance = unimportance;
            poolItem.id = _nextId++;
            poolItem.gameObj.SetActive(true);
            source.Play();

            //use a coroutine to make it available again after the clip is finished
            poolItem.routine = _stopSoundDelayedRoutine(poolItem.id, source.clip.length);
            StartCoroutine(poolItem.routine);

            // Add this sound to our active pool with its unique id
            _activePool[poolItem.id] = poolItem;

            // Return the id to the caller
            return poolItem.id;
        }
        private IEnumerator _playOneShotSoundDelayedRoutine(AudioClip clip, Vector2 position, float volume, float duration, byte priority = 255)
        {
            yield return new WaitForSeconds(duration);
            PlayOneShotSound(clip, position, volume, priority);
        }
        private IEnumerator _playConstSoundRoutine(AudioClip clip, float volume)
        {
            AudioSource src = _constantAudioSrc.audioSrc;

            //fade out last bgm
            if (_constantAudioSrc.isPlaying)
            {
                float start = _constantAudioSrc.audioSrc.volume;

                for (float t = 0; t < _constSoundFadeDuration;)
                {
                    src.volume = Mathf.Lerp(start, 0, t / _constSoundFadeDuration);

                    yield return new WaitForEndOfFrame();
                    t += Time.deltaTime;
                }
            }
            _constantAudioSrc.isPlaying = true;
            _constantAudioSrc.unscaledVolume = volume;
            src.clip = clip;
            src.volume = volume * volumeScale;
            src.Play();
        }
        #region public API
        public ulong PlayOneShotSound(AudioClip clip, Vector2 position, float volume, byte priority = 255)
        {
            // Do nothing if track does not exist, clip is null or volume is zero
            if (clip == null || volume.Equals(0.0f))
                return 0;

            float unimportance = ((Vector2)Camera.main.transform.position - position).sqrMagnitude / Mathf.Max(1, priority);

            int leastImportantIndex = -1;
            float leastImportanceValue = float.MinValue;

            // Find an available audio source to use
            for (int i = 0; i < _pool.Count; i++)
            {
                AudioPoolItem poolItem = _pool[i];

                // Is this source available
                if (!poolItem.isPlaying)
                    return ConfigurePoolObject(i, clip, position, volume, unimportance);


                // We have a pool item that is less important than the one we are going to play
                if (poolItem.unImportance > leastImportanceValue)
                {
                    // Record the least important sound we have found so far
                    // as a candidate to relace with our new sound request
                    leastImportanceValue = poolItem.unImportance;
                    leastImportantIndex = i;
                }
            }

            // If we get here all sounds are being used but we know the least important sound currently being
            // played so if it is less important than our sound request then use replace it
            if (leastImportanceValue > unimportance)
                return ConfigurePoolObject(leastImportantIndex, clip, position, volume, unimportance);

            // Could not be played (no sound in the pool available)
            return 0;
        }

        public void PlayOneShotSoundDelayed(AudioClip clip, Vector2 position, float volume, float duration, byte priority = 255)
        {
            StartCoroutine(_playOneShotSoundDelayedRoutine(clip, position, volume, duration, priority));
        }
        public void StopSound(ulong id)
        {
            AudioPoolItem activeSound;

            // If this if exists in our active pool
            if (_activePool.TryGetValue(id, out activeSound))
            {
                activeSound.audioSrc.Stop();
                activeSound.audioSrc.clip = null;
                activeSound.gameObj.SetActive(false);
                _activePool.Remove(id);

                // Make it available again
                activeSound.isPlaying = false;
            }
        }
        public void StopSoundDelayed(ulong id, float duration)
        {
            StartCoroutine(_stopSoundDelayedRoutine(id, duration));
        }
        public void PlayConstantSound(AudioClip clip, float volume)
        {
            _constantAudioSrc.gameObj.SetActive(true);
            StartCoroutine(_playConstSoundRoutine(clip, volume));
        }
        #endregion
    }
}

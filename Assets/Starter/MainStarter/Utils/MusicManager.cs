using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class MusicManager : MonoBehaviour {
	public AudioClip[] tracks;
	public bool playOnStart = true;
	public enum CameraFollowMode {
		None,
		Follow,
		Child
	};
	public CameraFollowMode cameraFollowMode = CameraFollowMode.Follow;
	public int defaultTrack = 0;
	
	public bool initialVolumeFromPreference = true;
	public float _volume = 0.9f;
	public float _pitch = 1.0f;
	public bool ScaleOutputVolume = true;
	
	// Static singleton property
	public static MusicManager Instance { get; private set; }
	
	public float volume {
		get { return _volume; }
		set {
			if (ScaleOutputVolume) {
				GetComponent<AudioSource>().volume = ScaleVolume(value);
			} else {
				GetComponent<AudioSource>().volume = value;
			}
			_volume = value;
		}
	}
	
	public float pitch {
		get { return _pitch; }
		set {
			GetComponent<AudioSource>().pitch = value;
			_pitch = value;
		}
	}
	
	void Awake() {
		// First we check if there are any other instances conflicting
		if (Instance != null && Instance != this) {
			// If that is the case, we destroy other instances
			Destroy(gameObject);
			return;
		}
		
		// Here we save our singleton instance
		Instance = this;
		
		// Furthermore we make sure that we don't destroy between scenes (this is optional)
		DontDestroyOnLoad(gameObject);
		

	}
	
	void Start() {
		if (cameraFollowMode == CameraFollowMode.Follow) {
			if (Camera.main != null)
				transform.position = Camera.main.transform.position;
		} else if (cameraFollowMode == CameraFollowMode.Child) {
			if (Camera.main != null)
				transform.parent = Camera.main.transform;
		} 
		
		GetComponent<AudioSource>().clip = tracks[defaultTrack];
		volume = _volume;
        if (initialVolumeFromPreference)
            volume = Prefs.Instance.GetVolumeMusic();
		
		AudioSource audioSrc = GetComponent<AudioSource>();
		audioSrc.rolloffMode = AudioRolloffMode.Linear;
		audioSrc.loop = true;
		audioSrc.dopplerLevel = 0f;
		audioSrc.spatialBlend = 0f;
		if (playOnStart && volume!=0) {
			Play();
		}
	}
	
	void OnLevelWasLoaded(int level) {
		if (cameraFollowMode == CameraFollowMode.Follow) {
			if (Camera.main != null)
				transform.position = Camera.main.transform.position;
		} else if (cameraFollowMode == CameraFollowMode.Child) {
			if (Camera.main != null)
				transform.parent = Camera.main.transform;
		} 
	}
	
	void Update() {
		if (cameraFollowMode == CameraFollowMode.Follow) {
			if (Camera.main != null)
				transform.position = Camera.main.transform.position;
		}
	}
	
	public void PlayTrack(int i) {
		GetComponent<AudioSource>().Stop();
		GetComponent<AudioSource>().clip = tracks[i];
		GetComponent<AudioSource>().Play();
	}
	
	public void Pause() {
		GetComponent<AudioSource>().Pause();
	}
	
	public void Play() {
		GetComponent<AudioSource>().Play();
	}
	
	public void Stop() {
		GetComponent<AudioSource>().Stop();
	}
	
	public void Fade(float targetVolume, float fadeTime) {
		LeanTween.value(gameObject, "SetVolume", volume, targetVolume, fadeTime);
	}
	
	public void FadeOut(float fadeTime) {
		StartCoroutine(FadeOutAsync(fadeTime));
	}
	
	IEnumerator FadeOutAsync(float fadeTime) {
		Fade(0f, fadeTime);
		yield return new WaitForSeconds(fadeTime);
		Pause();
	}
	
	public void FadeIn(float fadeTime) {
		Play();
		Fade(1.0f, fadeTime);
	}
	
	public void SlidePitch(float targetPitch, float fadeTime) {
		LeanTween.value(gameObject, "SetPitch", pitch, targetPitch, fadeTime);
	}
	
	public float GetVolumePreference() {
        return Prefs.Instance.GetVolumeMusic
            ();
	}
	
	public void SaveCurrentVolumePreference() {
		SaveVolumePreference(volume);
	}
	
	public void SaveVolumePreference(float v) {
        Prefs.Instance.SetVolumeMusic(v);
	}
	
	public void SetPitch(float p) {
		pitch = p;
	}
	
	// TODO: we should consider using this dB scale as an option when porting these changes 
	//       over to unity-bowerbird: http://wiki.unity3d.com/index.php?title=Loudness
	/*
	 *   Quadratic scaling of actual volume used by AudioSource. Approximates the proper exponential.
	 */
	public float ScaleVolume(float v) {
		v = Mathf.Pow(v, 4);
		return Mathf.Clamp(v, 0f, 1f);
	}
}

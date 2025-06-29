using HoloLight.Isar;
using System;
using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class AudioStreamer : MonoBehaviour, ISerializationCallbackReceiver
{
	private IsarCustomAudioSource _audioSource;
	private bool _running;
	private bool _togglePreDeserialized = true;
	[SerializeField]
	private bool _enableAudioStream = true;

	public bool Toggle
	{
		get => _enableAudioStream;
		set
		{
			_enableAudioStream = _togglePreDeserialized = value;
			if(_running && _audioSource != null) _audioSource.SetEnabled(_enableAudioStream);
		}
	}

	public void OnAfterDeserialize()
	{
		if (_enableAudioStream == _togglePreDeserialized) return;
		_togglePreDeserialized = _enableAudioStream;
		_audioSource?.SetEnabled(_enableAudioStream);
	}

	public void OnBeforeSerialize()
	{
	}

	private void OnEnable()
	{
		int systemSampleRate = AudioSettings.GetConfiguration().sampleRate;
		int systemDSPBufferSize = AudioSettings.GetConfiguration().dspBufferSize;
		AudioSpeakerMode speakerMode = AudioSettings.GetConfiguration().speakerMode;
		bool isAudioSettingsSupported = IsarCustomAudioSource.IsAudioSettingsSupported(systemSampleRate,SpeakerModeToNumChannels(speakerMode), systemDSPBufferSize);

		if (isAudioSettingsSupported)
		{
			_audioSource = new IsarCustomAudioSource(systemSampleRate,
													 systemDSPBufferSize);
		}
		_audioSource.SetEnabled(_enableAudioStream);
		_running = true;
	}

	private void OnDisable()
	{
		_audioSource.SetEnabled(false);
		_running = false;
	}

	int SpeakerModeToNumChannels(AudioSpeakerMode mode)
	{
		switch (mode)
		{
			case AudioSpeakerMode.Mono:
				return 1;
			case AudioSpeakerMode.Stereo:
				return 2;
			case AudioSpeakerMode.Quad:
				return 4;
			case AudioSpeakerMode.Surround:
				return 5;
			case AudioSpeakerMode.Mode5point1:
				return 6;
			case AudioSpeakerMode.Mode7point1:
				return 8;
			default:
				return 0;
		}
	}

	void OnAudioFilterRead(float[] data, int channels)
	{
		if (!_running)
			return;

		_audioSource.PushAudioData(data, channels);
	}

	private void OnApplicationQuit()
	{
		_running = false;
	}

	private void OnDestroy()
	{
		_running = false;
		if(_audioSource != null)
			_audioSource.Dispose();
	}
}

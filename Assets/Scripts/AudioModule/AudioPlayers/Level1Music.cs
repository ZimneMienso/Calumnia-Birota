// using System;
// using FMOD.Studio;
// using UnityEngine;
// using Zenject;
//
// namespace Game.Scripts.Core.AudioModule.AudioPlayers
// {
// 	[Serializable]
// 	public class Level1Music
// 	{
// 		private AudioManager _audioManager;
// 		private AudioTracksBase _audioTracksBase;
// 		private EventInstance _sourse;
//
// 		[Inject]
// 		void Construct(AudioManager audioManager, AudioTracksBase audioTracksBase)
// 		{
// 			_audioTracksBase = audioTracksBase;
// 			_audioManager = audioManager;
// 		}
//
// 		public void Play()
// 		{
// 			_sourse = _audioManager.PlaySoundWithInstance(_audioTracksBase.level1Misic, useInstance: true);
// 		}
//
// 		public void Stop()
// 		{
// 			_sourse.stop(STOP_MODE.ALLOWFADEOUT);
// 		}
// 	}
// }
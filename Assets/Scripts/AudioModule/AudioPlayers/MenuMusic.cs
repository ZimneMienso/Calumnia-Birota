// using FMOD.Studio;
// using Zenject;
//
// namespace Game.Scripts.Core.AudioModule.AudioPlayers
// {
// 	public class MenuMusic
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
// 			_sourse = _audioManager.PlaySoundWithInstance(_audioTracksBase.menuMisic, useInstance: true);
// 		}
//
// 		public void Stop()
// 		{
// 			_sourse.stop(STOP_MODE.ALLOWFADEOUT);
// 		}
// 	}
// }
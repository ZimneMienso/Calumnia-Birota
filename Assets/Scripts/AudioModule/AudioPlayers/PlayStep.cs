// using UnityEngine;
// using Zenject;
//
// namespace Game.Scripts.Core.AudioModule.AudioPlayers
// {
//     public class PlayStep : MonoBehaviour
//     {
//         private AudioManager audioManager1;
//         private AudioTracksBase audioTracksBase1;
//
//         [Inject]
//         void Construct(AudioManager audioManager, AudioTracksBase audioTracksBase)
//         {
//             audioTracksBase1 = audioTracksBase;
//             audioManager1 = audioManager;
//         }
//         
//         public void Play()
//         {
//             audioManager1.PlaySound(audioTracksBase1.step);
//         }
//     }
// }
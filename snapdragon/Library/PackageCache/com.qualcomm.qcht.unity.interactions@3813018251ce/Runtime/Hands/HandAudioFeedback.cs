// /******************************************************************************
//  * File: HandAudioFeedback.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core;
using UnityEngine;

namespace QCHT.Interactions.Hands
{
    [RequireComponent(typeof(AudioSource))]
    public class HandAudioFeedback : MonoBehaviour
    {
        private enum HAND_SOUND
        {
            SELECT = 0,
            UNSELECT,
            RESET,
            COUNT
        }

        [SerializeField] private bool IsLeftHand;
        [SerializeField] private HandAudioClips audioClips;

        private AudioSource _handAudioSource;

        private bool _selectSoundEnabled = true;
        private bool _unselectSoundEnabled = true;
        private bool _resetSoundEnabled = true;

        #region Helpers

        private AudioClip GetClip(HAND_SOUND sound)
        {
            return sound switch
            {
                HAND_SOUND.SELECT => audioClips.Select,
                HAND_SOUND.UNSELECT => audioClips.Unselect,
                HAND_SOUND.RESET => audioClips.Reset,
                _ => null
            };
        }

        #endregion

        #region MonoBehavior

        private void Start()
        {
            Setup();
        }

        private void Setup()
        {
            _handAudioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (QCHTInput.GestureDown(IsLeftHand, GestureId.PINCH))
            {
                if (_selectSoundEnabled)
                    PlaySound(HAND_SOUND.SELECT);
            }
            else if (QCHTInput.GestureUp(IsLeftHand, GestureId.PINCH))
            {
                if (_unselectSoundEnabled)
                    PlaySound(HAND_SOUND.UNSELECT);
            }
            else if (QCHTInput.LongGesture(IsLeftHand, GestureId.GRAB))
            {
                if (_resetSoundEnabled)
                    PlaySound(HAND_SOUND.RESET);
            }
        }

        #endregion

        #region Sound Playback API

        private void PlaySound(HAND_SOUND sound)
        {
            var clip = GetClip(sound);

            if (clip == null)
                return;

            if (_handAudioSource.isPlaying)
            {
                _handAudioSource.Stop();
            }

            _handAudioSource.PlayOneShot(GetClip(sound), 1);
        }

        public void ToggleSelectSound(bool on)
        {
            _selectSoundEnabled = on;
        }

        public void ToggleUnSelectSound(bool on)
        {
            _unselectSoundEnabled = on;
        }

        public void ToggleResetSound(bool on)
        {
            _resetSoundEnabled = on;
        }

        #endregion
    }
}
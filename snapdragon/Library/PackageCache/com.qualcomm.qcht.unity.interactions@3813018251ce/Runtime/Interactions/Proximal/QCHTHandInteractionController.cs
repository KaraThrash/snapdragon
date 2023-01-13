// /******************************************************************************
//  * File: QCHTHandInteractionController.cs
//  * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using QCHT.Core;
using QCHT.Interactions.Hands;
using UnityEngine;

namespace QCHT.Interactions.Proximal
{
    public class QCHTHandInteractionController : MonoBehaviour, IInteractionController
    {
        public HandType Type;
        public HandPoseDriver Driver;
        public HandPartPresenter Root;
        private InteractionResult _result;
        
        private void OnEnable()
        {
            QCHTProximalInteractor.RegisterController(this, Type);
            Root.OnTriggerEntered.AddListener(OnTriggerBegin);
            Root.OnTriggerExited.AddListener(OnTriggerEnded);
        }

        private void OnDisable()
        {
            // Force releasing interaction if the Controller is disabled
            ReleaseInteraction();
            
            QCHTProximalInteractor.UnRegisterController(Type);
            Root.OnTriggerEntered.RemoveListener(OnTriggerBegin);
            Root.OnTriggerExited.RemoveListener(OnTriggerEnded);
        }

        #region Root Callback

        private void OnTriggerBegin(GameObject interactable)
        {
            _result.currentGameObject = interactable;
        }

        private void OnTriggerEnded()
        {
            _result.currentGameObject = null;
        }

        #endregion

        #region IInteractionController

        public InteractionResult Process()
        {
            return _result;
        }

        public void OnBeginInteraction()
        {
            Root.ForceMatching(true);
        }

        public void OnEndInteraction()
        {
            Root.ForceMatching(false);
        }

        public void ReleaseInteraction()
        {
            OnTriggerEnded();
        }

        #endregion
    }
}
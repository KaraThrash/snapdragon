using System;
using System.Collections;
using System.Collections.Generic;
using QCHT.Core;
using UnityEngine;

public class RecordingTrigger : MonoBehaviour
{
    
    [SerializeField] private CustomEvent StartRecordingEvent;
    [SerializeField] private CustomEvent StopRecordingEvent;

    private Material _m;
    private bool _isRecording;
    private float _timer;
    private float _lockDelay = 3;
    
    void Start()
    {
        _m = GetComponent<MeshRenderer>().material;
        UpdateRecordingState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time > _timer + _lockDelay)
        {
            _timer = Time.time;
            _isRecording = !_isRecording;
            UpdateRecordingState();    
        }
    }

    private void UpdateRecordingState()
    {
        if (_isRecording)
        {
            StartRecordingEvent.SendEvent();
            _m.color = Color.red;
        }
        else
        {
            StopRecordingEvent.SendEvent();
            _m.color = Color.green;
        }
    }
}

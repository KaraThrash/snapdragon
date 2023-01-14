using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingControl : MonoBehaviour
{
    public ARTrackedImageManager arImageManager;

    public List<GameObject> profilePrefabs;
    private float textSpeed = 1.0f;
    private GameObject clone = null;

    [Serializable]
    public struct TrackableInfo
    {
        public Text TrackingStatusText;
        public Text[] PositionTexts;
    }
    public TrackableInfo[] trackableInfos;

    private Dictionary<TrackableId, TrackableInfo> _trackedImages = new Dictionary<TrackableId, TrackableInfo>();

    public void OnEnable()
    {
        arImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    public void OnDisable()
    {
        arImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            if (trackedImage.referenceImage.name == "Pizza")
            {
                _trackedImages.Add(trackedImage.trackableId, trackableInfos[0]);
  //              if (clone == null)
                {
                    clone = Instantiate(profilePrefabs[0]);
                    clone.transform.position = Vector3.zero;
                }
            }
        }

        foreach (var trackedImage in args.updated)
        {
            Vector3 position = trackedImage.transform.position;
            TrackableInfo info = _trackedImages[trackedImage.trackableId];

            if (clone != null)
            {
                //var step = textSpeed * Time.deltaTime; //calculate distance to move
                //clone.transform.position = Vector3.MoveTowards(clone.transform.position, position, step);
                clone.transform.position = position;
            }

            info.TrackingStatusText.text = trackedImage.trackingState.ToString();
            info.PositionTexts[0].text = position.x.ToString("#0.00");
            info.PositionTexts[1].text = position.y.ToString("#0.00");
            info.PositionTexts[2].text = position.z.ToString("#0.00");
        }

        foreach (var trackedImage in args.removed)
        {
            
            TrackableInfo info = _trackedImages[trackedImage.trackableId];
            info.TrackingStatusText.text = "None";
            info.PositionTexts[0].text = "0.00";
            info.PositionTexts[1].text = "0.00";
            info.PositionTexts[2].text = "0.00";
            _trackedImages.Remove(trackedImage.trackableId);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

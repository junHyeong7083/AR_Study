using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;

public class ARMultiTrackedImageController : MonoBehaviour
{
    public ARTrackedImageManager arTrackedImageManager;
    public GameObject[] prefabs;

    private Dictionary<string, GameObject> spawnObjs= new Dictionary<string, GameObject>();

    private void OnEnable()
    {
        if (arTrackedImageManager != null) arTrackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach(ARTrackedImage trackedImage in eventArgs.added) HandleAddedImage(trackedImage);
        foreach(ARTrackedImage trackedImage in eventArgs.updated) HandleUpdatedImage(trackedImage);
        foreach (var removed in eventArgs.removed) HandleRemovedImage(removed.Value);

    }


    private void HandleAddedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;   // 이름 매핑

        int prefabIdx = GetPrefabIndexByImageName(imageName);
        if (prefabIdx < 0 || prefabIdx >= prefabs.Length || prefabs[prefabIdx] == null) return;

        GameObject spawnedObj = Instantiate(prefabs[prefabIdx], trackedImage.transform);
        spawnedObj.transform.localPosition = Vector3.zero;
        spawnedObj.transform.localRotation = Quaternion.identity;

        spawnObjs[imageName] = spawnedObj;

    }

    private void HandleUpdatedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if(!spawnObjs.TryGetValue(imageName, out GameObject spawnObj))
        {
            HandleAddedImage(trackedImage);
            return;
        }

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            spawnObj.SetActive(true);
            spawnObj.transform.SetPositionAndRotation(
                trackedImage.transform.position,
                trackedImage.transform.rotation);
        }
        else if (trackedImage.trackingState == TrackingState.Limited) spawnObj.SetActive(false);
    }
    private void HandleRemovedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        if (spawnObjs.TryGetValue(imageName, out GameObject spawnedObj)) spawnedObj.SetActive(false);
    }

    private int GetPrefabIndexByImageName(string imageName)
    {
        if(arTrackedImageManager.referenceLibrary != null)
        {
            for(int i = 0; i< arTrackedImageManager.referenceLibrary.count; ++i)
            {
                if (arTrackedImageManager.referenceLibrary[i].name == imageName) return i;
            }
        }
        return -1;
    }

    public GameObject GetSpawnedObject(string imageName)
    {
        spawnObjs.TryGetValue(imageName, out GameObject obj);
        return obj;
    }

    public void ClearAllSpawnedObjects()
    {
        foreach(var kvp in spawnObjs)
        {
            if (kvp.Value != null) Destroy(kvp.Value);
        }
        spawnObjs.Clear();
    }
}

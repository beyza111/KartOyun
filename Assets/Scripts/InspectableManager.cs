using System.Collections.Generic;
using UnityEngine;

public class InspectableManager : MonoBehaviour
{
    public List<GameObject> inspectableObjects; // Sahnedeki inspectable objeler

    private int currentIndex = 0;

    public void ActivateNextObject()
    {
        if (currentIndex < inspectableObjects.Count)
        {
            inspectableObjects[currentIndex].SetActive(true);
            currentIndex++;
        }
        else
        {
            Debug.Log("No more objects to activate!");
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackedImageInfoMultipleManager : MonoBehaviour
{
    

    [SerializeField]
    private GameObject[] arObjectsToPlace;
   

    [SerializeField]
    private Vector3 scaleFactor = new Vector3(0.01f,0.01f,0.1f);
    

    private ARTrackedImageManager m_TrackedImageManager;
    private AudioSource audio;

    private Dictionary<string, GameObject> arObjects = new Dictionary<string, GameObject>();
    public Vector3 posa;
    public Vector3 posb;
    private Vector2 startPosition;
    public float rotationSpeed = 27;
    public bool canAnimate = false;

    void Awake()
    {
        
         m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
        audio = gameObject.AddComponent<AudioSource>();



        // setup all game objects in dictionary
        foreach (GameObject arObject in arObjectsToPlace)
        {
            GameObject newARObject = Instantiate(arObject, Vector3.zero, Quaternion.identity);
            newARObject.name = arObject.name;
            arObjects.Add(arObject.name, newARObject);
             
        }
         
    }
    

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void Update()
    {
        HandleSizeChange();
       
       if(arObjectsToPlace != null)
        {
            foreach(GameObject arObject in arObjectsToPlace)
            {
                arObject.transform.localScale = scaleFactor;
            }
        }

        if(canAnimate){
            transform.Rotate(new Vector3(0f, rotationSpeed * Time.deltaTime, 0f));
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateARImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateARImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            arObjects[trackedImage.name].SetActive(false);
        }
    }

    private float calculer_distance()
    {
        float dist = Vector3.Distance(posa, posb);
        return dist;
    }

    private void UpdateARImage(ARTrackedImage trackedImage)
    {
        // Assign and Place Game Object
        AssignGameObject(trackedImage.referenceImage.name, trackedImage.transform.position);

        if(calculer_distance() < 0.25 && calculer_distance() > 0)
        {
            canAnimate = true;
            if(!audio.isPlaying){
                audio.PlayOneShot((AudioClip)Resources.Load("opening"));
            }
        
            var cubeRenderer = arObjectsToPlace[1].GetComponent<Renderer>();
           
            //Call SetColor using the shader property name "_Color" and setting the color to red
            cubeRenderer.material.SetColor("_Color", Color.red);
        }
        else
        {
            canAnimate = false;
            audio.Stop();
        }
        // Debug.Log($"trackedImage.referenceImage.name: {trackedImage.referenceImage.name}");
        //  Debug.Log(trackedImage.transform.position + "position" );
    }

    void AssignGameObject(string name, Vector3 newPosition)
    {
        if(arObjectsToPlace != null)
        {
            GameObject goARObject = arObjects[name];
            goARObject.SetActive(true);
            goARObject.transform.position = newPosition;
            goARObject.transform.localScale = scaleFactor;
            foreach(GameObject go in arObjects.Values)
            {

                if(go.name == "rouge")
                {
                  posa = go.transform.position;
                }
                if(go.name == "vert")
                {
                  posb = go.transform.position;
                }
            
            } 
        }
        
    }
    private void showAndroidToast(string toastText)
    {
        //create a Toast class object
        AndroidJavaClass toastClass =
                    new AndroidJavaClass("android.widget.Toast");

        //create an array and add params to be passed
        object[] toastParams = new object[3];
        AndroidJavaClass unityActivity =
          new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        toastParams[0] =
                     unityActivity.GetStatic<AndroidJavaObject>
                               ("currentActivity");
        toastParams[1] = toastText;
        toastParams[2] = toastClass.GetStatic<int>
                               ("LENGTH_LONG");

        //call static function of Toast class, makeText
        AndroidJavaObject toastObject =
                        toastClass.CallStatic<AndroidJavaObject>
                                      ("makeText", toastParams);

        //show toast
        toastObject.Call("show");

    }

    private void HandleSizeChange(){
        if(Input.touchCount > 0){
            Touch currentTouch = Input.GetTouch(0);
            if(currentTouch.phase == TouchPhase.Ended){
                Vector2 endPosition = currentTouch.position;
                HandleSwipe(startPosition, endPosition);
            } else if (currentTouch.phase == TouchPhase.Began) {
                startPosition = currentTouch.position;
            }
        }
    }

    private void HandleSwipe(Vector2 startPosition, Vector2 endPosition)
    {
        bool isUpwardSwipe = startPosition.y < endPosition.y;
        bool isDownwardSwipe = startPosition.y > endPosition.y;

        if(isUpwardSwipe)
        {
            scaleFactor.x = scaleFactor.x + 0.01f;
            scaleFactor.y = scaleFactor.y + 0.01f;
            scaleFactor.z = scaleFactor.z + 0.01f;
        }
        else if(isDownwardSwipe)
        {
            if( scaleFactor.x > 0.01f && scaleFactor.y > 0.01f && scaleFactor.z > 0.01f ) {
                scaleFactor.x = scaleFactor.x - 0.01f;
                scaleFactor.y = scaleFactor.y - 0.01f;
                scaleFactor.z = scaleFactor.z - 0.01f;
            } else {
                scaleFactor = new Vector3(0.1f,0.1f,0.1f);
            }
        }
    }
}

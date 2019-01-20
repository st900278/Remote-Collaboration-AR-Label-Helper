using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Intel.RealSense;
using OpenCVForUnity;
using OpenCVForUnityExample;

public class MultiCamera : MonoBehaviour {

    [SerializeField]
    List<WebCam> webCams;

    // Use this for initialization
    void Start () {
        for (int i = 0; i < webCams.Count; i++)
        {
            if (webCams[i].cameraType == WebCam.CameraType.WebCam)
            {
                webCams[i].container = new GameObject();
                webCams[i].container.transform.parent = gameObject.GetComponent<Transform>();
                webCams[i].Init();
                webCams[i].texture.onInitialized.AddListener(() => WebCamInitialized(i));
                webCams[i].texture.Initialize();
            }
            else if(webCams[i].cameraType == WebCam.CameraType.Realsense)
            {
                webCams[i].Init();
            }

        }
    }
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < webCams.Count; i++)
        {
            if (webCams[i].cameraType == WebCam.CameraType.WebCam)
            {
                if (webCams[i].texture.IsPlaying() && webCams[i].texture.DidUpdateThisFrame())
                {
                    Mat rgbaMat = webCams[i].texture.GetMat();

                    Mat rgbMat = new Mat();
                    Mat rgbMat2 = new Mat();
                    rgbMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC3);
                    //Debug.Log(String.Join(" ", rgbaMat.get(10, 10).Select(p => p.ToString()).ToArray()));
                    rgbMat2 = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC3);
                    Imgproc.cvtColor(rgbaMat, rgbMat, Imgproc.COLOR_RGBA2RGB);
                    Imgproc.cvtColor(rgbaMat, rgbMat2, Imgproc.COLOR_RGBA2RGB);
                    if (webCams[i].calibrator != null && webCams[i].calibrate)
                    {
                        webCams[i].calibrator.GetComponent<CameraCalibration>().Calibrate(rgbMat);
                    }
                    if (webCams[i].detector != null && webCams[i].detect)
                    {
                        webCams[i].detector.GetComponent<ARObjectDetector>().Detect(rgbMat2);
                    }
                    Utils.fastMatToTexture2D(rgbMat2, webCams[i].GetTexture());
                }

            }
            else if (webCams[i].cameraType == WebCam.CameraType.Realsense)
            {
                RsAruco rsAruco = (RsAruco)(GameObject.Find("RsProcessingPipe").GetComponent<RsProcessingPipe>().profile._processingBlocks[0]);
                Mat data = rsAruco.getRgbMat();


                if (data != null)
                {
                    using (Mat rgbMat = data.clone())
                    {
                        if(rgbMat != null)
                        {

                            Mat rgbMat2 = rgbMat.clone();

                            if (webCams[i].calibrator != null && webCams[i].calibrate)
                            {
                                webCams[i].calibrator.GetComponent<CameraCalibration>().Calibrate(rgbMat);
                            }
                            if (webCams[i].calibrator != null && webCams[i].detect)
                            {
                                webCams[i].detector.GetComponent<ARObjectDetector>().Detect(rgbMat2);
                            }

                            Utils.fastMatToTexture2D(rgbMat2, webCams[i].GetTexture());
                        }
                    }
                }



                //webCams[i].calibrator.GetComponent<CameraCalibration>().Calibrate(webCams[i].rgbMat);
            }

        }
    }

    public void WebCamInitialized(int webcamId)
    {
        Debug.Log("test");

        Mat webCamTextureMat = webCams[webcamId].texture.GetMat();

        webCams[webcamId].InitCanvasTexture(webCamTextureMat.cols(), webCamTextureMat.rows());

        webCams[webcamId].canvas.GetComponent<Renderer>().material.mainTexture = webCams[webcamId].GetTexture();

        webCams[webcamId].canvas.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);

    }

    

}

[System.SerializableAttribute]
public class WebCam{
    public string deviceName;
    public float width;
    public float height;

    public enum CameraType
    {
        WebCam,
        Realsense,
    }
    public CameraType cameraType;

    [HideInInspector]
    public WebCamTextureToMatHelper texture;

    [HideInInspector]
    public GameObject container;

    public GameObject canvas;
    private Texture2D canvasTexture;

    public bool calibrate;
    public GameObject calibrator;
    public bool detect;
    public GameObject detector;
    public RsFrameProvider realsenseSource;
    public VideoFrame rsframe;

    [HideInInspector]
    public bool isNewRsframe;

    public void Init()
    {
        if (cameraType == CameraType.WebCam)
        {
            texture = container.AddComponent<WebCamTextureToMatHelper>();
            texture._requestedDeviceName = deviceName;
            texture.flipHorizontal = true;
            texture.onInitialized = new UnityEngine.Events.UnityEvent();
        }
        else if(cameraType == CameraType.Realsense)
        {
            isNewRsframe = false;
            Debug.Log("test set");
            canvasTexture = new Texture2D(640, 480, TextureFormat.RGB24, false);
            canvas.GetComponent<Renderer>().material.mainTexture = canvasTexture;
            realsenseSource.OnNewSample += RealsenseProcessFrame;
        }
        
    }

    public void InitCanvasTexture(int width, int height)
    {
        canvasTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    public void RealsenseProcessFrame(Frame f)
    {
        if (f.IsComposite)
        {       
            using (var fs = FrameSet.FromFrame(f))
            {
                isNewRsframe = true;
                
            }
            
        }

    }

    public Texture2D GetTexture()
    {
        return canvasTexture;
    }
}
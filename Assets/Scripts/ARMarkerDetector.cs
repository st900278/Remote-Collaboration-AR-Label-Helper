using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;
using OpenCVForUnityExample;

public class ARMarkerDetector : MonoBehaviour {

    public int dictionaryId = Aruco.DICT_4X4_50;
    public Camera arCamera;
    private WebCamTextureToMatHelper webCamTextureToMatHelper;
    PoseData oldPoseData;
    Texture2D texture;

    Mat rgbMat;
    Mat camMatrix;
    MatOfDouble distCoeffs;

    Matrix4x4 ARM;

    Mat ids;
    public float markerLength = 0.1f;

    List<Mat> corners;

    List<Mat> rejectedCorners;

    Mat rvecs;

    Mat tvecs;
    Mat rotMat;

    public GameObject arGameObject;


    DetectorParameters detectorParams;

    Dictionary dictionary;
    FpsMonitor fpsMonitor;

    Mat rvec;
    Mat tvec;
    Mat recoveredIdxs;

    public static Matrix4x4[] arObjectTransform;

    // Use this for initialization
    void Start () {

        arObjectTransform = new Matrix4x4[100];


        webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();
        webCamTextureToMatHelper.Initialize();
    }
    public void OnWebCamTextureToMatHelperInitialized()
    {
        Debug.Log("OnWebCamTextureToMatHelperInitialized");

        Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

        texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGB24, false);

        gameObject.GetComponent<Renderer>().material.mainTexture = texture;

        gameObject.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);
        Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

        if (fpsMonitor != null)
        {
            fpsMonitor.Add("width", webCamTextureMat.width().ToString());
            fpsMonitor.Add("height", webCamTextureMat.height().ToString());
            fpsMonitor.Add("orientation", Screen.orientation.ToString());
        }


        float width = webCamTextureMat.width();
        float height = webCamTextureMat.height();

        float imageSizeScale = 1.0f;
        float widthScale = (float)Screen.width / width;
        float heightScale = (float)Screen.height / height;
        if (widthScale < heightScale)
        {
            Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            imageSizeScale = (float)Screen.height / (float)Screen.width;
        }
        else
        {
            Camera.main.orthographicSize = height / 2;
        }


        // set camera parameters.
        double fx;
        double fy;
        double cx;
        double cy;

        /*
        string loadDirectoryPath = Path.Combine(Application.persistentDataPath, "ArUcoCameraCalibrationExample");
        string calibratonDirectoryName = "camera_parameters" + width + "x" + height;
        string loadCalibratonFileDirectoryPath = Path.Combine(loadDirectoryPath, calibratonDirectoryName);
        string loadPath = Path.Combine(loadCalibratonFileDirectoryPath, calibratonDirectoryName + ".xml");
        if (useStoredCameraParameters && File.Exists(loadPath))
        {
            CameraParameters param;
            XmlSerializer serializer = new XmlSerializer(typeof(CameraParameters));
            using (var stream = new FileStream(loadPath, FileMode.Open))
            {
                param = (CameraParameters)serializer.Deserialize(stream);
            }

            camMatrix = param.GetCameraMatrix();
            distCoeffs = new MatOfDouble(param.GetDistortionCoefficients());

            fx = param.camera_matrix[0];
            fy = param.camera_matrix[4];
            cx = param.camera_matrix[2];
            cy = param.camera_matrix[5];

            Debug.Log("Loaded CameraParameters from a stored XML file.");
            Debug.Log("loadPath: " + loadPath);

        }
        else
        {
            int max_d = (int)Mathf.Max(width, height);
            fx = max_d;
            fy = max_d;
            cx = width / 2.0f;
            cy = height / 2.0f;

            camMatrix = new Mat(3, 3, CvType.CV_64FC1);
            camMatrix.put(0, 0, fx);
            camMatrix.put(0, 1, 0);
            camMatrix.put(0, 2, cx);
            camMatrix.put(1, 0, 0);
            camMatrix.put(1, 1, fy);
            camMatrix.put(1, 2, cy);
            camMatrix.put(2, 0, 0);
            camMatrix.put(2, 1, 0);
            camMatrix.put(2, 2, 1.0f);

            distCoeffs = new MatOfDouble(0, 0, 0, 0);

            Debug.Log("Created a dummy CameraParameters.");
        }
        */
        /*
        int max_d = (int)Mathf.Max(width, height);
        fx = max_d;
        fy = max_d;
        cx = width / 2.0f;
        cy = height / 2.0f;
        */
        fx = 690.12;
        fy = 692.41;
        cx = 300.33;
        cy = 241.15;

        camMatrix = new Mat(3, 3, CvType.CV_64FC1);
        camMatrix.put(0, 0, fx);
        camMatrix.put(0, 1, 0);
        camMatrix.put(0, 2, cx);
        camMatrix.put(1, 0, 0);
        camMatrix.put(1, 1, fy);
        camMatrix.put(1, 2, cy);
        camMatrix.put(2, 0, 0);
        camMatrix.put(2, 1, 0);
        camMatrix.put(2, 2, 1.0f);

        distCoeffs = new MatOfDouble(0.1129668, -0.8695, -0.00226, -0.021473, 2.86566);

        Debug.Log("camMatrix " + camMatrix.dump());
        Debug.Log("distCoeffs " + distCoeffs.dump());


        // calibration camera matrix values.
        Size imageSize = new Size(width * imageSizeScale, height * imageSizeScale);
        double apertureWidth = 0;
        double apertureHeight = 0;
        double[] fovx = new double[1];
        double[] fovy = new double[1];
        double[] focalLength = new double[1];
        Point principalPoint = new Point(0, 0);
        double[] aspectratio = new double[1];

        Calib3d.calibrationMatrixValues(camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);

        Debug.Log("imageSize " + imageSize.ToString());
        Debug.Log("apertureWidth " + apertureWidth);
        Debug.Log("apertureHeight " + apertureHeight);
        Debug.Log("fovx " + fovx[0]);
        Debug.Log("fovy " + fovy[0]);
        Debug.Log("focalLength " + focalLength[0]);
        Debug.Log("principalPoint " + principalPoint.ToString());
        Debug.Log("aspectratio " + aspectratio[0]);


        double fovXScale = (2.0 * Mathf.Atan((float)(imageSize.width / (2.0 * fx)))) / (Mathf.Atan2((float)cx, (float)fx) + Mathf.Atan2((float)(imageSize.width - cx), (float)fx));
        double fovYScale = (2.0 * Mathf.Atan((float)(imageSize.height / (2.0 * fy)))) / (Mathf.Atan2((float)cy, (float)fy) + Mathf.Atan2((float)(imageSize.height - cy), (float)fy));

        Debug.Log("fovXScale " + fovXScale);
        Debug.Log("fovYScale " + fovYScale);


        if (widthScale < heightScale)
        {
            arCamera.fieldOfView = (float)(fovx[0] * fovXScale);
        }
        else
        {
            arCamera.fieldOfView = (float)(fovy[0] * fovYScale);
        }
        arCamera.nearClipPlane = 0.01f;


        rgbMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC3);
        ids = new Mat();
        corners = new List<Mat>();
        rejectedCorners = new List<Mat>();
        rvecs = new Mat();
        tvecs = new Mat();
        rotMat = new Mat(3, 3, CvType.CV_64FC1);


        detectorParams = DetectorParameters.create();
        dictionary = Aruco.getPredefinedDictionary((int)dictionaryId);

        rvec = new Mat();
        tvec = new Mat();
        recoveredIdxs = new Mat();



        // if WebCamera is frontFaceing, flip Mat.
        if (webCamTextureToMatHelper.GetWebCamDevice().isFrontFacing)
        {
            webCamTextureToMatHelper.flipHorizontal = true;
        }
    }

    /// <summary>
    /// Raises the webcam texture to mat helper disposed event.
    /// </summary>
    public void OnWebCamTextureToMatHelperDisposed()
    {

        if (rgbMat != null)
            rgbMat.Dispose();

        if (texture != null)
        {
            Texture2D.Destroy(texture);
            texture = null;
        }

        if (ids != null)
            ids.Dispose();
        foreach (var item in corners)
        {
            item.Dispose();
        }
        corners.Clear();
        foreach (var item in rejectedCorners)
        {
            item.Dispose();
        }
        rejectedCorners.Clear();
        if (rvecs != null)
            rvecs.Dispose();
        if (tvecs != null)
            tvecs.Dispose();
        if (rotMat != null)
            rotMat.Dispose();

        if (rvec != null)
            rvec.Dispose();
        if (tvec != null)
            tvec.Dispose();
        if (recoveredIdxs != null)
            recoveredIdxs.Dispose();

    }


    // Update is called once per frame
    void Update()
    {
        if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
        {

            Mat rgbaMat = webCamTextureToMatHelper.GetMat();
            Imgproc.cvtColor(rgbaMat, rgbMat, Imgproc.COLOR_RGBA2RGB);
            Aruco.detectMarkers(rgbMat, dictionary, corners, ids, detectorParams, rejectedCorners, camMatrix, distCoeffs);
            if (ids.total() > 0)
            {
                
                Aruco.drawDetectedMarkers(rgbMat, corners, ids, new Scalar(0, 255, 0));
                EstimatePoseCanonicalMarker(rgbMat);
            }



            Utils.fastMatToTexture2D(rgbMat, texture);
        }
    }

    private void EstimatePoseCanonicalMarker(Mat rgbMat)
    {
        Aruco.estimatePoseSingleMarkers(corners, markerLength, camMatrix, distCoeffs, rvecs, tvecs);

        for (int i = 0; i < ids.total(); i++)
        {

            int id = (int)ids.get(i, 0)[0];

            using (Mat rvec = new Mat(rvecs, new OpenCVForUnity.Rect(0, i, 1, 1)))
            using (Mat tvec = new Mat(tvecs, new OpenCVForUnity.Rect(0, i, 1, 1)))
            {
                // In this example we are processing with RGB color image, so Axis-color correspondences are X: blue, Y: green, Z: red. (Usually X: red, Y: green, Z: blue)
                Aruco.drawAxis(rgbMat, camMatrix, distCoeffs, rvec, tvec, markerLength * 0.5f);

                // This example can display the ARObject on only first detected marker.
                if (i == 0)
                {
                    //UpdateARObjectTransform(rvec, tvec);
                }
                UpdateIDTransform(id, rvec, tvec);
            }
        }
    }

    private void UpdateARObjectTransform(Mat rvec, Mat tvec)
    {
        // Convert to unity pose data.
        PoseData poseData = ARUtils.ConvertRvecTvecToPoseData(rvec.get(0, 0), tvec.get(0, 0));
        

        // Convert to transform matrix.
        ARM = ARUtils.ConvertPoseDataToMatrix(ref poseData, true, true);

        ARM = arCamera.transform.localToWorldMatrix * ARM;

        ARUtils.SetTransformFromMatrix(arGameObject.transform, ref ARM);
        
        
        //arGameObject.GetComponent<Transform>().localScale = new Vector3(100f, 100f, 100f);
        arGameObject.GetComponent<Transform>().localScale = new Vector3(.1f, .1f, .1f);
        //arGameObject.GetComponent<Transform>

        //arGameObject.GetComponent<Transform>().rotation = GameObject.Find("Main Camera").GetComponent<Transform>().rotation;
    }

    private void UpdateIDTransform(int id, Mat rvec, Mat tvec)
    {
        //Debug.Log(id);
        PoseData poseData = ARUtils.ConvertRvecTvecToPoseData(rvec.get(0, 0), tvec.get(0, 0));
        ARM = ARUtils.ConvertPoseDataToMatrix(ref poseData, true, true);
        ARM = arCamera.transform.localToWorldMatrix * ARM;
        
        arObjectTransform[id] = ARM;
        ARUtils.SetTransformFromMatrix(GameObject.Find("ARCube (" + id + ")").GetComponent<Transform>(), ref arObjectTransform[id]);
        GameObject.Find("ARCube (" + id + ")").GetComponent<Transform>().localScale = new Vector3(.1f, .1f, .1f);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OpenCVForUnity;
using OpenCVForUnityExample;


public class ARObjectDetector : MonoBehaviour {
    public bool faceCamera;

    public float markerSize;
    public double fx, fy, cx, cy;
    public double[] distortion;
    public Mat camMatrix;
    public MatOfDouble distCoeffs;
    public GameObject target;


    private Mat oldTranslation;

    public string readImagePath;
    private List<Mat> readImage;

    private Dictionary<int, GameObject> markerDict;
    private Dictionary<int, string> markerText;
    // Use this for initialization
    void Start () {
        markerDict = new Dictionary<int, GameObject>();
        markerText = new Dictionary<int, string>();
        List<MarkerToGameObject> markerList = GameObject.Find("ARMarkerList").GetComponent<ARMarkerList>().markers;

        readImage = new List<Mat>();


        for (int i = 0; i < markerList.Count; i++) {
            //readImage.Add(Imgcodecs.imread(readImagePath + i.ToString() + ".png", Imgcodecs.IMREAD_UNCHANGED));
            //readImage.Add(Imgcodecs.imread(readImagePath + "0.png", Imgcodecs.IMREAD_UNCHANGED));
            Debug.Log(i);
            readImage.Add(Imgcodecs.imread(readImagePath + i.ToString() + ".png", Imgcodecs.IMREAD_UNCHANGED));

            Debug.Log(readImage[i]);
        }
        Debug.Log(readImage[0].dump());

        for (int i = 0; i < markerList.Count; i++)
        {
            markerDict[markerList[i].id] = markerList[i].item;
            markerText[markerList[i].id] = markerList[i].text;
        }

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
        distCoeffs = new MatOfDouble(distortion);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Detect(Mat rgbMat)
    {
        Mat ids;

        List<Mat> corners;

        List<Mat> rejectedCorners;

        Mat rvecs;
        Mat tvecs;

        Mat otv = GameObject.Find("WebCamCalibrator").GetComponent<CameraCalibration>().newtvec;
        if(otv == null) otv = new Mat(3, 1, CvType.CV_64FC1);
        Mat orv = GameObject.Find("WebCamCalibrator").GetComponent<CameraCalibration>().newrvec;
        if (orv == null) orv = new Mat(3, 1, CvType.CV_64FC1);
        Mat composedT = new Mat();
        Mat composedR = new Mat();



        DetectorParameters detectorParams;
        Dictionary dictionary;



        ids = new Mat();
        corners = new List<Mat>();
        rejectedCorners = new List<Mat>();
        rvecs = new Mat();
        tvecs = new Mat();



        detectorParams = DetectorParameters.create();
        dictionary = Aruco.getPredefinedDictionary(Aruco.DICT_4X4_50);


        //ARObjectDetector.test();
        //
        //Core.flip(rgbMat, rgbMat, 0);
        Aruco.detectMarkers(rgbMat, dictionary, corners, ids, detectorParams, rejectedCorners, camMatrix, distCoeffs);

        //Aruco.refineDetectedMarkers(rgbMat, charucoBoard, corners, ids, rejectedCorners, webCams[i].camMatrix, webCams[i].distCoeffs, 10f, 3f, true, recoveredIdxs, detectorParams);
        if (ids.total() > 0)
        {
            //Aruco.drawDetectedMarkers(rgbMat, corners, ids, new Scalar(0, 255, 0));

            Aruco.estimatePoseSingleMarkers(corners, markerSize, camMatrix, distCoeffs, rvecs, tvecs);
            for (int i = 0; i < ids.total(); i++)
            {


                int id = (int)ids.get(i, 0)[0];

                float x1 = (float)corners[i].get(0, 0)[0];
                float y1 = (float)corners[i].get(0, 0)[1];

                float x2 = (float)corners[i].get(0, 1)[0];
                float y2 = (float)corners[i].get(0, 1)[1];
                float x3 = (float)corners[i].get(0, 2)[0];
                float y3 = (float)corners[i].get(0, 2)[1];
                float x4 = (float)corners[i].get(0, 3)[0];
                float y4 = (float)corners[i].get(0, 3)[1];

                //Imgproc.line(rgbMat, new Point(x1, y1), new Point(x2, y2), new Scalar(10, 20, 30));


                if (markerText.ContainsKey(id) && markerText[id] != "")
                {
                    Point[] pointList = new Point[4];
                    pointList[0] = new Point(x1, y1);
                    pointList[1] = new Point(x2, y2);
                    pointList[2] = new Point(x3, y3);
                    pointList[3] = new Point(x4, y4);
                 
                    Imgproc.fillConvexPoly(rgbMat, new MatOfPoint(pointList), new Scalar(255, 255, 255, 0.4));
                    //Imgproc.putText(rgbMat, markerText[id], new Point((x1 + x3) / 2, (y1 + y3) / 2), Core.FONT_HERSHEY_SIMPLEX, 0.8, new Scalar(200, 0, 0), 2, 16);
                    //Imgproc.putText(rgbMat, "黒い", new Point((x1 + x3) / 2, (y1 + y3) / 2), Core.FONT_ITALIC, 0.8, new Scalar(200, 0, 0), 2, 16);

                    int xCentroid = new int();
                    int yCentroid = new int();
                    xCentroid = (int)Math.Floor((x1 + x2 + x3 + x4) / 4);
                    yCentroid = (int)Math.Floor((y1 + y2 + y3 + y4) / 4);
                    if ( (xCentroid > readImage[id].cols() && xCentroid < 640-readImage[id].cols()) && (yCentroid > readImage[id].rows() && yCentroid < 480 - readImage[id].rows()))
                    {
                        Debug.Log(rgbMat.cols());
                        Debug.Log(rgbMat.rows());
                        Debug.Log(String.Join(" ", rgbMat.get(10, 10).Select(p => p.ToString()).ToArray()));
                        for(int col = xCentroid; col < xCentroid + readImage[id].cols(); col++)
                        {
                            for (int row = yCentroid; row < yCentroid + readImage[id].rows(); row++)
                            {
                                //mixed with white color

                                double[] colorMat = rgbMat.get(row, col);
                                colorMat[0] = (colorMat[0] * 2 + 255) / 3;
                                colorMat[1] = (colorMat[1] * 2 + 255) / 3;
                                colorMat[2] = (colorMat[2] * 2+ 255) / 3;
                                rgbMat.put(row, col, colorMat);
                                if (readImage[id].get(row - yCentroid, col - xCentroid)[3] == 255) rgbMat.put(row, col, new double[] { 0, 0, 0 });
                            }
                        }
                        // readImage[id].copyTo(new Mat(rgbMat, new OpenCVForUnity.Rect(xCentroid, yCentroid, readImage[id].cols(), readImage[id].rows())));
                    }
                }
                else
                {
                    Imgproc.putText(rgbMat, id.ToString(), new Point((x1 + x3) / 2, (y1 + y3) / 2), Core.FONT_HERSHEY_SIMPLEX, 0.8, new Scalar(0, 100, 200), 2, 16);

                }


                using (Mat rvec = new Mat(rvecs, new OpenCVForUnity.Rect(0, i, 1, 1)))
                using (Mat tvec = new Mat(tvecs, new OpenCVForUnity.Rect(0, i, 1, 1)))
                {
                    // Aruco.drawAxis(rgbMat, camMatrix, distCoeffs, rvec, tvec, markerSize * 0.5f);

                    Mat _tvec = new Mat(3, 1, CvType.CV_64FC1);

                    //Debug.Log("rvec " + tvec.get(0, 0)[0] + " " + tvec.get(0, 0)[1] + " " + tvec.get(0, 0)[2]);

                    _tvec.put(0, 0, new double[] { tvec.get(0, 0)[0] });
                    _tvec.put(1, 0, new double[] { tvec.get(0, 0)[1] });
                    _tvec.put(2, 0, new double[] { tvec.get(0, 0)[2] });

                    Mat _rvec = new Mat(3, 1, CvType.CV_64FC1);

                    _rvec.put(0, 0, new double[] { rvec.get(0, 0)[0], 0, 0 });
                    _rvec.put(1, 0, new double[] { rvec.get(0, 0)[1], 0, 0 });
                    _rvec.put(2, 0, new double[] { rvec.get(0, 0)[2], 0, 0 });


                    Mat cameraR = new Mat(3, 3, CvType.CV_64FC1);
                    Mat cameraRotation = new Mat(3, 1, CvType.CV_64FC1);
                    Mat cameraTranslation = new Mat(3, 1, CvType.CV_64FC1);


                    Calib3d.Rodrigues(_rvec, cameraR);

                    cameraR = cameraR.t();
                    Calib3d.Rodrigues(cameraR, cameraRotation);


                    cameraTranslation = -cameraR * _tvec;

                    
                    /*
                    Debug.Log("orv");
                    Debug.Log(orv.dump());
                    Debug.Log("otv");
                    Debug.Log(otv.dump());
                    Debug.Log("_rvec");
                    Debug.Log(_rvec.dump());
                    Debug.Log("_tvec");
                    Debug.Log(_tvec.dump());
                    */
                    Calib3d.composeRT(_rvec, _tvec, orv, otv, composedR, composedT);

                    //relativePosition(orv, otv, _rvec, _tvec, composedR, composedT); //rvec, tvec are the current marker transforms

                    /*
                    Debug.Log("composedR");
                    Debug.Log(composedR.dump());
                    Debug.Log("composedT");
                    Debug.Log(composedT.dump());
                    */
                    if (oldTranslation == null)
                    {
                        oldTranslation = cameraTranslation;
                    }
                    //double diff = Math.Sqrt(Math.Pow(cameraTranslation.get(0, 0)[0] - oldTranslation.get(0, 0)[0], 2) + Math.Pow(cameraTranslation.get(1, 0)[0] - oldTranslation.get(1, 0)[0], 2) + Math.Pow(cameraTranslation.get(2, 0)[0] - oldTranslation.get(2, 0)[0], 2));

                    if (cameraTranslation.get(0, 0)[0] * cameraTranslation.get(0, 0)[0] > 0.0001)
                    {
                        if (markerDict.ContainsKey(id) && markerDict[id] != null)
                        {
                            //Debug.Log(((float)-cameraTranslation.get(2, 0)[0]));
                            //GameObject.Find("Cube" + id).transform.parent = gameObject.transform;



                            markerDict[id].transform.localScale = new Vector3(markerSize, markerSize, markerSize);
                            //markerDict[id].transform.localPosition = new Vector3((float)-cameraTranslation.get(0, 0)[0] + target.transform.position.x, (float)(-cameraTranslation.get(2, 0)[0]) + target.transform.position.y, (float)-cameraTranslation.get(1, 0)[0] + target.transform.position.z);
                            markerDict[id].transform.localPosition = new Vector3((float)composedT.get(0,0)[0], (float)composedT.get(2, 0)[0], (float)composedT.get(1, 0)[0]);

                            //markerDict[id].transform.rotation = Camera.main.transform.rotation; 
                            MarkerPosition.markerTransforms[id] = markerDict[id].transform;
                        }
                        else
                        {
                            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            //cube.transform.parent = gameObject.transform;
                            cube.name = "Cube" + id;
                            cube.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
                            cube.transform.localPosition = new Vector3((float)-cameraTranslation.get(0, 0)[0] + target.transform.position.x, (float)(-cameraTranslation.get(2, 0)[0]) + target.transform.position.y, (float)-cameraTranslation.get(1, 0)[0] + target.transform.position.z);
                            //cube.transform.rotation = Camera.main.transform.rotation;
                            markerDict.Add(id, cube);
                            MarkerPosition.markerTransforms[id] = cube.transform;

                        }

                        //Destroy(cube, 0.2f);
                    }
                    oldTranslation = cameraTranslation;

                }
            }




        }


        //Utils.fastMatToTexture2D(rgbMat, texture);
        //}
    }

    private void relativePosition(Mat rvec1, Mat tvec1, Mat rvec2, Mat tvec2, Mat composedRvec, Mat composedTvec)
    {
        // Inverse the second marker, the right one in the image
        Mat invRvec, invTvec;
        invRvec = new Mat();
        invTvec = new Mat();

        inversePerspective(rvec2, tvec2, invRvec, invTvec);

        Calib3d.composeRT(rvec1, tvec1, invRvec, invTvec, composedRvec, composedTvec);
    }

    private void inversePerspective(Mat rvec, Mat tvec, Mat invRvec, Mat invTvec)
    {
        Mat R = new Mat();
        Calib3d.Rodrigues(rvec, R);
        R = R.t();
        invTvec = -R * tvec;
        Calib3d.Rodrigues(R, invRvec);
    }

    private void drawLabel(int id, float x, float y)
    {

        if (markerDict.ContainsKey(id) && markerDict[id] != null)
        {
            markerDict[id].transform.localScale = new Vector3(1,1,1);
            markerDict[id].transform.localPosition = new Vector3();



            MarkerPosition.markerTransforms[id] = markerDict[id].transform;
        }
    }
}

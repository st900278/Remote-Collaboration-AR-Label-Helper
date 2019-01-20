using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;
using OpenCVForUnityExample;
using System.IO;

public class CameraCalibration : MonoBehaviour {

    public int boardWidth;
    public int boardHeight;
    public float squareSize;
    public float markerSize;

    public double fx, fy, cx, cy;
    public double[] distortion;
    public Mat camMatrix;
    public MatOfDouble distCoeffs;

    public GameObject target;

    public string filePath;

    public Mat newrvec;
    public Mat newtvec;

    // Use this for initialization
    void Start () {
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

    public void Calibrate(Mat rgbMat)
    {

        Mat ids;

        List<Mat> corners;

        List<Mat> rejectedCorners;


        Mat rvec;

        Mat tvec;
        Mat rotMat;
        DetectorParameters detectorParams;
        CharucoBoard charucoBoard;
        Dictionary dictionary;

        Mat recoveredIdxs;
        const int charucoMinMarkers = 2;
        Mat charucoCorners;
        Mat charucoIds;

        ids = new Mat();
        corners = new List<Mat>();
        rejectedCorners = new List<Mat>();
        rvec = new Mat();
        tvec = new Mat();
        rotMat = new Mat(3, 3, CvType.CV_64FC1);


        detectorParams = DetectorParameters.create();
        dictionary = Aruco.getPredefinedDictionary(Aruco.DICT_4X4_50);

        charucoCorners = new Mat();
        charucoIds = new Mat();
        charucoBoard = CharucoBoard.create(boardWidth, boardHeight, squareSize, markerSize, dictionary);

        recoveredIdxs = new Mat();

            

            //ARObjectDetector.test();
            //
            //Core.flip(rgbMat, rgbMat, 0);
        Aruco.detectMarkers(rgbMat, dictionary, corners, ids, detectorParams, rejectedCorners, camMatrix, distCoeffs);
        //Aruco.refineDetectedMarkers(rgbMat, charucoBoard, corners, ids, rejectedCorners, webCams[i].camMatrix, webCams[i].distCoeffs, 10f, 3f, true, recoveredIdxs, detectorParams);
        if (ids.total() > 0)
        {
            
            Aruco.interpolateCornersCharuco(corners, ids, rgbMat, charucoBoard, charucoCorners, charucoIds, camMatrix, distCoeffs, charucoMinMarkers);
            Aruco.drawDetectedMarkers(rgbMat, corners, ids, new Scalar(0, 255, 0));
            if (charucoIds.total() > 0)
            {
                Aruco.drawDetectedCornersCharuco(rgbMat, charucoCorners, charucoIds, new Scalar(0, 0, 255));
                bool valid = Aruco.estimatePoseCharucoBoard(charucoCorners, charucoIds, charucoBoard, camMatrix, distCoeffs, rvec, tvec);

                // if at least one board marker detected
                if (valid)
                {

                    // In this example we are processing with RGB color image, so Axis-color correspondences are X: blue, Y: green, Z: red. (Usually X: red, Y: green, Z: blue)
                    Aruco.drawAxis(rgbMat, camMatrix, distCoeffs, rvec, tvec, markerSize * 0.5f);

                    Mat cameraR = new Mat();
                    Mat cameraRotation = new Mat();
                    Mat cameraTranslation = new Mat();
                    Calib3d.Rodrigues(rvec, cameraR);


                    cameraR = cameraR.t();
                    Calib3d.Rodrigues(cameraR, cameraRotation);

                    cameraTranslation = -cameraR * tvec;

                    Debug.Log(cameraTranslation.dump());


                    newrvec = cameraRotation;
                    newtvec = cameraTranslation;

                    double cosine_for_pitch = Math.Sqrt((double)(cameraR.get(0, 0)[0] * cameraR.get(0, 0)[0] + cameraR.get(1, 0)[0] * cameraR.get(1, 0)[0]));

                    bool is_singular = cosine_for_pitch < 0.00001;

                    double yaw = 0, pitch = 0, roll = 0;
                    if (!is_singular)
                    {
                        yaw = Math.Atan2(-cameraR.get(1, 0)[0], cameraR.get(0, 0)[0]);

                        pitch = Math.Atan2(cameraR.get(2, 0)[0], cosine_for_pitch);

                        roll = Math.Atan2(-cameraR.get(2, 1)[0], cameraR.get(2, 2)[0]);

                        //Debug.Log((roll * 180 / 3.14) - 90 + " " + yaw * 180 / 3.14 + " " + pitch * 180 / 3.14);
                    }




                    target.transform.position = new Vector3((float)cameraTranslation.get(0, 0)[0], (float)cameraTranslation.get(2, 0)[0], (float)cameraTranslation.get(1, 0)[0]);
                    target.transform.eulerAngles = new Vector3((float)(roll * 180 / 3.14) - 90, (float)(yaw * 180 / 3.14), (float)(pitch * 180 / 3.14));
                }
            }



        }
            
            
            //Utils.fastMatToTexture2D(rgbMat, texture);
        //}
    }
}

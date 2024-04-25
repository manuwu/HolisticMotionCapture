using System;
using System.Collections.Generic;
using System.IO;
using Boomlagoon.JSON;
// using Boomlagoon.JSON;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using HolisticMotionCapture;

//如果好用，请收藏地址，帮忙分享。
public class ZaItem
{
    /// <summary>
    /// 
    /// </summary>
    public float x { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public float y { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public float z { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public float visibility { get; set; }
    
    public Vector4 ToVector4()
    {
        return new Vector4(x,y,z,visibility);
    }
}

public class PoseLandmarksItem
{
    /// <summary>
    /// 
    /// </summary>
    public float x { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public float y { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public float z { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public float visibility { get; set; }
}

public class LeftHandLandmarksItem
{
    /// <summary>
    /// 
    /// </summary>
    public float x { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public float y { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public float z { get; set; }
}

public class RightHandLandmarksItem
{
    /// <summary>
    /// 
    /// </summary>
    public float x { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public float y { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public float z { get; set; }
}

public class RecordsItem
{
    /// <summary>
    /// 
    /// </summary>
    public List <string > faceLandmarks { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List <ZaItem > za { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List <PoseLandmarksItem > poseLandmarks { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List <LeftHandLandmarksItem > leftHandLandmarks { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public List <RightHandLandmarksItem > rightHandLandmarks { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public long timestamp { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public double playTime { get; set; }
}

public class VideoSize
{
    /// <summary>
    /// 
    /// </summary>
    public int width { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int height { get; set; }
}

public class RecordRoot
{
    /// <summary>
    /// 
    /// </summary>
    public List <RecordsItem > records { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public VideoSize videoSize { get; set; }
}


public class VideoDataDriverMotion : MonoBehaviour
{

    public static VideoDataDriverMotion Instance;
    [SerializeField, Range(0, 1)] float lerpPercentage = 0.3f;
    [SerializeField, Range(0, 1)] float humanPoseThreshold = 0.5f;
    [SerializeField, Range(0, 1)] float faceScoreThreshold = 0.5f;
    [SerializeField, Range(0, 1)] float handScoreThreshold = 0.5f;
    HolisticMocapType holisticMocapType = HolisticMocapType.full;
    bool isUpperBodyOnly=false;
    Transform lookTarget;
    Animator avatarAnimator;
    HolisticMotionCapturePipeline motionCapture;
    const int BODY_LINE_NUM = 35;
    // Pairs of vertex indices of the lines that make up body's topology.
    // Defined by the figure in https://google.github.io/mediapipe/solutions/pose.
    readonly List<Vector4> linePair = new List<Vector4>{
        new Vector4(0, 1), new Vector4(1, 2), new Vector4(2, 3), new Vector4(3, 7), new Vector4(0, 4),
        new Vector4(4, 5), new Vector4(5, 6), new Vector4(6, 8), new Vector4(9, 10), new Vector4(11, 12),
        new Vector4(11, 13), new Vector4(13, 15), new Vector4(15, 17), new Vector4(17, 19), new Vector4(19, 15),
        new Vector4(15, 21), new Vector4(12, 14), new Vector4(14, 16), new Vector4(16, 18), new Vector4(18, 20),
        new Vector4(20, 16), new Vector4(16, 22), new Vector4(11, 23), new Vector4(12, 24), new Vector4(23, 24),
        new Vector4(23, 25), new Vector4(25, 27), new Vector4(27, 29), new Vector4(29, 31), new Vector4(31, 27),
        new Vector4(24, 26), new Vector4(26, 28), new Vector4(28, 30), new Vector4(30, 32), new Vector4(32, 28)
    };

    [SerializeField] private Animator animator;
    // Array of pose landmarks for accessing data with CPU (C#). 
    Vector4[] poseWorldLandmarks;
    private int currentNote=0;
    private double tempPlayTime=0;
    private JSONArray recordRoot;
    private string filePath;

    private void Awake()
    {
        Instance = this;
        filePath = Application.persistentDataPath + "/motionData.json";
    }

    void Start()
    {
        SetAnimator(animator);
        AnalysisFile();
    }
    
    void LateUpdate()
    {
        if (currentNote<recordRoot.Length)
        {
            if (tempPlayTime >= recordRoot[currentNote].Obj.GetNumber("playTime"))
            {
                AnalysisFileZa();
                motionCapture.AvatarPoseRender(lookTarget, humanPoseThreshold, handScoreThreshold, faceScoreThreshold,
                    isUpperBodyOnly, lerpPercentage, holisticMocapType);

                currentNote++;
            }
        }

        tempPlayTime += Time.deltaTime;
    }
    
    void AnalysisFileZa()
    {
        // var zaArray = recordRoot[currentNote].Obj.GetArray("poseLandmarks");
        var zaArray = recordRoot[currentNote].Obj.GetArray("za");
        for (int i = 0; i < zaArray.Length; i++)
        {
            Vector4 vector4=new Vector4((float)zaArray[i].Obj.GetNumber("x"),
                (float)zaArray[i].Obj.GetNumber("y"),(float)zaArray[i].Obj.GetNumber("z"),(float)zaArray[i].Obj.GetNumber("visibility")
                );
            motionCapture.PoseWorldLandmark[i] = vector4;
        }
        
        var leftHandLandmarksArray = recordRoot[currentNote].Obj.GetArray("leftHandLandmarks");
        for (int i = 0; i < leftHandLandmarksArray.Length; i++)
        {
            Vector4 vector4=new Vector4((float)leftHandLandmarksArray[i].Obj.GetNumber("x"),
                (float)leftHandLandmarksArray[i].Obj.GetNumber("y"),(float)leftHandLandmarksArray[i].Obj.GetNumber("z"));
            motionCapture.leftHandLandmarks[i] = vector4;
        }
        
        var rightHandLandmarksArray = recordRoot[currentNote].Obj.GetArray("rightHandLandmarks");
        for (int i = 0; i < rightHandLandmarksArray.Length; i++)
        {
            Vector4 vector4=new Vector4((float)rightHandLandmarksArray[i].Obj.GetNumber("x"),
                (float)rightHandLandmarksArray[i].Obj.GetNumber("y"),(float)rightHandLandmarksArray[i].Obj.GetNumber("z"));
            motionCapture.rightHandLandmarks[i] = vector4;
        }
        
        var faceLandmarksArray = recordRoot[currentNote].Obj.GetArray("faceLandmarks");
        for (int i = 0; i < motionCapture.faceLandmarks.Length; i++)
        {
            Vector4 vector4=new Vector4((float)faceLandmarksArray[i].Obj.GetNumber("x"),
                (float)faceLandmarksArray[i].Obj.GetNumber("y"),(float)faceLandmarksArray[i].Obj.GetNumber("z"));
            motionCapture.faceLandmarks[i] = vector4;
        }
        
        var leftEyeLandmarksArray = recordRoot[currentNote].Obj.GetArray("leftEyeLandmarks");
        for (int i = 0; i < leftEyeLandmarksArray.Length; i++)
        {
            Vector4 vector4=new Vector4((float)leftEyeLandmarksArray[i].Obj.GetNumber("x"),
                (float)leftEyeLandmarksArray[i].Obj.GetNumber("y"),(float)leftEyeLandmarksArray[i].Obj.GetNumber("z"));
            motionCapture.leftEyeLandmarks[i] = vector4;
        }
        
        var rightEyeLandmarksArray = recordRoot[currentNote].Obj.GetArray("rightEyeLandmarks");
        for (int i = 0; i < rightEyeLandmarksArray.Length; i++)
        {
            Vector4 vector4=new Vector4((float)rightEyeLandmarksArray[i].Obj.GetNumber("x"),
                (float)rightEyeLandmarksArray[i].Obj.GetNumber("y"),(float)rightEyeLandmarksArray[i].Obj.GetNumber("z"));
            motionCapture.rightEyeLandmarks[i] = vector4;
        }
    }
    
    void AnalysisFile()
    {
        JSONObject infoFile = JSONObject.Parse(File.ReadAllText(filePath));
        recordRoot = infoFile.GetArray("records");
    }

    public void SetAnimator(Animator avatar)
    {
        if (avatarAnimator != null)
        {
            Destroy(avatarAnimator.gameObject);
        }
        if (motionCapture != null)
        {
            motionCapture.Dispose();
        }
        avatarAnimator = avatar;
        motionCapture = new HolisticMotionCapturePipeline(avatar);
    }

    public void ResetPose()
    {
        if (motionCapture == null) return;
        motionCapture.ResetAvatar(1);
    }
}

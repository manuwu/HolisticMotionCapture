using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MediaPipe.Holistic;
using Mediapipe.BlazePose;


public partial class HolisticMotionCapture : System.IDisposable
{
    HolisticPipeline _holisticPipeline;
    public HolisticPipeline holisticPipeline{
        get {return this._holisticPipeline;}
        private set {this._holisticPipeline = value;}
    }
    
    Animator avatar;

    public HolisticMotionCapture(Animator avatarAnimator, BlazePoseModel blazePoseModel = BlazePoseModel.full){
        avatar = avatarAnimator;
        holisticPipeline = new HolisticPipeline(blazePoseModel);
        PoseInit();
        HandInit();
        FaceInit();
    }

    public void Dispose(){
        holisticPipeline.Dispose();
    }

    public void AvatarPoseRender(
        Texture inputTexture, 
        float poseScoreThreshold = 0.5f,
        float handScoreThreshold = 0.5f,
        float faceScoreThreshold = 0.5f,
        bool isSeparateEyeBlink = false,
        bool isUpperBodyOnly = false,
        HolisticMocapType mocapType = HolisticMocapType.full,
        BlazePoseModel blazePoseModel = BlazePoseModel.full,
        float poseDetectionThreshold = 0.75f,
        float poseDetectionIouThreshold = 0.3f)
    {
        holisticPipeline.ProcessImage(inputTexture, (HolisticInferenceType)mocapType, blazePoseModel, poseDetectionThreshold, poseDetectionIouThreshold);
        PoseRender(mocapType, poseScoreThreshold, isUpperBodyOnly);
        HandRender(mocapType, true, handScoreThreshold);
        HandRender(mocapType, false, handScoreThreshold);
        FaceRender(mocapType, faceScoreThreshold, isSeparateEyeBlink);
    }
}

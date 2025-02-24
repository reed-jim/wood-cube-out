#if UNITY_ANDROID
//using Google.Play.Common;
//using Google.Play.Review;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ReviewInAppController : MonoBehaviour
{
#if UNITY_ANDROID
   // private ReviewManager _reviewManager;
   // PlayAsyncOperation<PlayReviewInfo, ReviewErrorCode> playReviewInfoAsyncOperation;
  

    public void ShowReview(UnityAction actionErorr)
    {
        //_reviewManager = new ReviewManager();
        //playReviewInfoAsyncOperation = null;

        //playReviewInfoAsyncOperation = _reviewManager.RequestReviewFlow();
        //playReviewInfoAsyncOperation.Completed += playReviewInfoAsync => Complete(playReviewInfoAsync, actionErorr);
    }
    
    //void Complete(PlayAsyncOperation<PlayReviewInfo, ReviewErrorCode> playReviewInfoAsync, UnityAction actionErorr)
    //{
    //    if (playReviewInfoAsync.Error == ReviewErrorCode.NoError)
    //    {
    //        // display the review prompt
    //        var playReviewInfo = playReviewInfoAsync.GetResult();
    //        _reviewManager.LaunchReviewFlow(playReviewInfo);
    //    }
    //    else
    //    {
    //        if (actionErorr != null)
    //            actionErorr();
    //        // handle error when loading review prompt
    //        Debug.Log("Erro Review Inapp "+ playReviewInfoAsync.Error);
    //    }
    //}
#endif
}

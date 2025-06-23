using System.Threading.Tasks;
using Firebase.Auth;
// using GooglePlayGames;
// using GooglePlayGames.BasicApi;
using UnityEngine;

public class GooglePlayGamesSignInManager : MonoBehaviour
{
    // void Start()
    // {
    //     if (!PlayGamesPlatform.Instance.IsAuthenticated())
    //     {
    //         PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    //     }
    // }

    // internal void ProcessAuthentication(SignInStatus status)
    // {
    //     if (status == SignInStatus.Success)
    //     {
    //         GamePersistentVariable.FirebaseUserId = PlayGamesPlatform.Instance.GetUserId();
    //     }
    //     else
    //     {
    //         FallbackLoginAnonymously();
    //     }
    // }

    // private async Task FallbackLoginAnonymously()
    // {
    //     await Task.Delay(3000);

    //     try
    //     {
    //         if (FirebaseAuth.DefaultInstance.CurrentUser == null)
    //         {
    //             FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(task =>
    //             {
    //                 if (task.IsCanceled)
    //                 {
    //                     return;
    //                 }
    //                 if (task.IsFaulted)
    //                 {
    //                     return;
    //                 }

    //                 AuthResult result = task.Result;

    //                 GamePersistentVariable.FirebaseUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
    //             });
    //         }
    //         else
    //         {
    //             GamePersistentVariable.FirebaseUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
    //         }
    //     }
    //     catch (System.Exception)
    //     {

    //     }

    //     // Disable your integration with Play Games Services or show a login button
    //     // to ask users to sign-in. Clicking it should call
    //     // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
    // }
}

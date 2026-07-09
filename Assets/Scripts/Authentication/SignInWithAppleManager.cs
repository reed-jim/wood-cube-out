// using System.Text;
// using System.Threading.Tasks;
// using AppleAuth;
// using AppleAuth.Enums;
// using AppleAuth.Interfaces;
// using AppleAuth.Native;
// using Firebase.Auth;
// using UnityEngine;

// public class SignInWithAppleManager : MonoBehaviour
// {
//     private IAppleAuthManager appleAuthManager;

//     void Start()
//     {
//         FallbackLoginAnonymously();

//         // // If the current platform is supported
//         // if (AppleAuthManager.IsCurrentPlatformSupported)
//         // {
//         //     // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
//         //     var deserializer = new PayloadDeserializer();
//         //     // Creates an Apple Authentication manager with the deserializer
//         //     this.appleAuthManager = new AppleAuthManager(deserializer);

//         //     QuickLogin();
//         // }
//     }

//     // void Update()
//     // {
//     //     // Updates the AppleAuthManager instance to execute
//     //     // pending callbacks inside Unity's execution loop
//     //     if (this.appleAuthManager != null)
//     //     {
//     //         this.appleAuthManager.Update();
//     //     }
//     // }

//     // private void QuickLogin()
//     // {
//     //     var quickLoginArgs = new AppleAuthQuickLoginArgs();

//     //     this.appleAuthManager.QuickLogin(
//     //         quickLoginArgs,
//     //         credential =>
//     //         {
//     //             // Received a valid credential!
//     //             // Try casting to IAppleIDCredential or IPasswordCredential

//     //             // Previous Apple sign in credential
//     //             var appleIdCredential = credential as IAppleIDCredential;

//     //             // Saved Keychain credential (read about Keychain Items)
//     //             var passwordCredential = credential as IPasswordCredential;

//     //             if (appleIdCredential != null)
//     //             {
//     //                 var userId = appleIdCredential.User;

//     //                 GamePersistentVariable.FirebaseUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
//     //             }
//     //         },
//     //         error =>
//     //         {
//     //             Debug.Log("SAFERIO Quick Login" + error);

//     //             SignIn();
//     //             // Quick login failed. The user has never used Sign in With Apple on your app. Go to login screen
//     //         });
//     // }

//     // private void SignIn()
//     // {
//     //     var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

//     //     this.appleAuthManager.LoginWithAppleId(
//     //         loginArgs,
//     //         credential =>
//     //         {
//     //             var appleIdCredential = credential as IAppleIDCredential;

//     //             if (appleIdCredential != null)
//     //             {
//     //                 // Apple User ID
//     //                 // You should save the user ID somewhere in the device
//     //                 var userId = appleIdCredential.User;

//     //                 GamePersistentVariable.FirebaseUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

//     //                 // // Email (Received ONLY in the first login)
//     //                 // var email = appleIdCredential.Email;

//     //                 // // Full name (Received ONLY in the first login)
//     //                 // var fullName = appleIdCredential.FullName;

//     //                 // // Identity token
//     //                 // var identityToken = Encoding.UTF8.GetString(
//     //                 //     appleIdCredential.IdentityToken,
//     //                 //     0,
//     //                 //     appleIdCredential.IdentityToken.Length);

//     //                 // // Authorization code
//     //                 // var authorizationCode = Encoding.UTF8.GetString(
//     //                 //     appleIdCredential.AuthorizationCode,
//     //                 //     0,
//     //                 //     appleIdCredential.AuthorizationCode.Length);

//     //                 // // And now you have all the information to create/login a user in your system
//     //             }
//     //         },
//     //         error =>
//     //         {
//     //             Debug.Log("SAFERIO Login" + error);

//     //             FallbackLoginAnonymously();

//     //             // // Something went wrong
//     //             // var authorizationErrorCode = error.GetAuthorizationErrorCode();
//     //         });
//     // }

//     private async Task FallbackLoginAnonymously()
//     {
//         await Task.Delay(3000);

//         try
//         {
//             if (FirebaseAuth.DefaultInstance.CurrentUser == null)
//             {
//                 FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(task =>
//                 {
//                     if (task.IsCanceled)
//                     {
//                         return;
//                     }
//                     if (task.IsFaulted)
//                     {
//                         return;
//                     }

//                     AuthResult result = task.Result;

//                     GamePersistentVariable.FirebaseUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
//                 });
//             }
//             else
//             {
//                 GamePersistentVariable.FirebaseUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
//             }
//         }
//         catch (System.Exception)
//         {

//         }
//     }
// }

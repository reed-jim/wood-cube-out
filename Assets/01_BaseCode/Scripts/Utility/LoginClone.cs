using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class LoginClone : MonoBehaviour
{


    //public string userName;
    //public string userEmail;
    //public string userPass;
    //public string userNameCharector;
    //public TextAsset level;
    //public List<DataScore> dataScore;

    //public List<PlayFabLeaderboardMember> leaderBoardInEditer;

    //public void TakeUserName(string UserName)
    //{

    //    userName = UserName;
    //}
    //public void TakeUserEmail(string UserEmail)
    //{
    //    userEmail = UserEmail;
    //}
    //public void TakeUserPass(string UserPass)
    //{
    //    userPass = UserPass;
    //}
    //public void LoginWithEmail()
    //{
    //    var requestLogin = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPass };
    //    PlayFabClientAPI.LoginWithEmailAddress(requestLogin, (ok) => { GetTest(); }, (noOK) => { Debug.LogError("noOK" + noOK); });
    //    string data = JsonConvert.SerializeObject(dataScore);
    //    var ranId = Random.Range(0, 99);
    //    GConnection.UpdatePvPScore(ranId);


    //}

    //public void GetTest()
    //{
    //    Debug.LogError("GetTest");
    //    var request = new GetStoreItemsRequest() { CatalogVersion = "ConfigGame", StoreId = Config.StoreIDPlayFab };
    //    PlayFabClientAPI.GetStoreItems(request,
    //       (s) =>
    //       {
    //           Debug.LogError("GetStoreItems");
    //           var data = JsonConvert.DeserializeObject<ConfigGamePlayfab>(s.Store[0].CustomData.ToString());
    //           Debug.LogError(data);
    //           //if (data != null)
    //           //    success?.Invoke(data);

    //       }, delegate { });
    //}
    //public void HandelRegister()
    //{
    //    var ranId = Random.Range(0, 99);
    //    var register = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPass, Username = "khanhClone" + ranId };
    //    PlayFabClientAPI.RegisterPlayFabUser(register, (ok) => { Debug.LogError("ok"); }, (noOK) => { Debug.LogError("noOK" + noOK); });

    //}
    //public void ChangeSence()
    //{
    //    SceneManager.LoadSceneAsync("GamePlay");
    //}
    //int coutn;
  
    //public void OnButton()
    //{
    //    coutn = 200000;
    //    StartCoroutine(SendData());
    //}
    //public IEnumerator SendData()
    //{
    //    yield return new WaitForSeconds(5);
    //    coutn += 1;
    //    HandelRegister100(coutn);
    //    if (coutn == 200000)
    //    {
    //        StopCoroutine(SendData());
    //        Debug.LogError("==============1000OK");
    //    }
    //    else
    //    {
    //        StartCoroutine(SendData());
    //    }
    //}
    //public void OnbtnLogginAndPvPScore()
    //{
    //    coutn = 200000;
    //    StartCoroutine(Login());
    //}
    //public IEnumerator Login()
    //{
    //    yield return new WaitForSeconds(5);
    //    coutn += 1;
    //    LoginWithEmail100(coutn);
    //    if (coutn == 200100)
    //    {
    //        StopCoroutine(Login());
    //        Debug.LogError("==============1000OK");
    //    }
    //    else
    //    {
    //        StartCoroutine(Login());
    //    }
    //}
    //public void HandelRegister100(int id)
    //{

    //    var register = new RegisterPlayFabUserRequest { Email = "Khanhdeptrai" + id.ToString() + "@gmail.com", Password = "123456" + id.ToString(), Username = "Khanhdeptrai" + id };
    //    PlayFabClientAPI.RegisterPlayFabUser(register, delegate { LoginWithEmail100(id); Debug.LogError("==============HandelRegister100"); }, (noOK) => { Debug.LogError("noOK" + noOK); });

    //}
    //public void LoginWithEmail100(int id)
    //{
    //    var requestLogin = new LoginWithEmailAddressRequest { Email = "Khanhdeptrai" + id.ToString() + "@gmail.com", Password = "123456" + id.ToString() };
    //    PlayFabClientAPI.LoginWithEmailAddress(requestLogin, HandleSendData, (noOK) => { Debug.LogError("noOK" + noOK); });
    //    string data = JsonConvert.SerializeObject(dataScore);
    //    var ranId = Random.Range(0, 99);
     


    //}
    //public void HandleSendData(LoginResult result)
    //{
    //    //  GConnection.UpdateData("PLAY_FAB_ID", result.PlayFabId.ToString(), );
    //    var ranId = Random.Range(0, 10000);
    //    PlayerData playerData = new PlayerData { version = -1, currentLevel = 10, currentLevelPlay = 10, numReturn = 10, numAddStand = 10, namePlayer = "KhanhRD_" + ranId.ToString() };
    //    PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest() { DisplayName = playerData.namePlayer }, (s) => {
    //        string dataLevelnormal = JsonConvert.SerializeObject(playerData);
    //        GConnection.UpdateData("PLAY_FAB_ID", result.PlayFabId.ToString(), delegate {
    //            GConnection.UpdateData("DATA_NORMAL_PLAYER", dataLevelnormal, delegate {

    //                GConnection.UpdateData("DATA_LEVEL", level.ToString(), delegate { }); ;
    //            });
    //        });
    //        Scoresdsds();
    //        Debug.LogError("==============HandleSendData");
    //    }, (e) => { });    
     


    //}
    //public int count = 0;
 
    //private void Scoresdsds()
    //{
    //    if (count < 100)
    //    {
    //        GConnection.UpdatePvPScore(5);
    //        count += 1;
    //    }
    //    //if (count >= 10 && count < 20)
    //    //{
    //    //    GConnection.UpdatePvPScore(10);
    //    //    count += 1;
    //    //}
    //    //if (count >= 20 && count < 30)
    //    //{
    //    //    GConnection.UpdatePvPScore(20);
    //    //    count += 1;
    //    //}
    //    //if (count >= 30 && count < 40)
    //    //{
    //    //    GConnection.UpdatePvPScore(30);
    //    //    count += 1;
    //    //}
    //    //if (count >= 40 && count < 50)
    //    //{
    //    //    GConnection.UpdatePvPScore(40);
    //    //    count += 1;
    //    //}
    //    //if (count >= 50 && count < 60)
    //    //{
    //    //    GConnection.UpdatePvPScore(50);
    //    //    count += 1;
    //    //}
    //    //if (count >= 60 && count < 70)
    //    //{
    //    //    GConnection.UpdatePvPScore(60);
    //    //    count += 1;
    //    //}
    //    //if (count >= 70 && count < 80)
    //    //{
    //    //    GConnection.UpdatePvPScore(70);
    //    //    count += 1;
    //    //}
    //    //if (count >= 80 && count < 90)
    //    //{
    //    //    GConnection.UpdatePvPScore(80);
    //    //    count += 1;
    //    //}
    //    //if (count >= 80 && count < 90)
    //    //{
    //    //    GConnection.UpdatePvPScore(80);
    //    //    count += 1;
    //    //}
    //    //if (count >= 90)
    //    //{
    //    //    count = 0;


    //    //}

    //}



}

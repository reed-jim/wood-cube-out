using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class IQScoreController : MonoBehaviour
{
    private IDisposable _downloadAvatarDispose;
    private IDisposable _downloadFlagDispose;
    public List<IQPlayerBadgeData> aroundPlayers;

    public void Init()
    {
        aroundPlayers = new List<IQPlayerBadgeData>();
        GetPlayersAround();
   
        if (Application.internetReachability == NetworkReachability.NotReachable || !LoginController.IsConnected)
        {
            return;
        }
    }


    public void GetPlayersAround()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable || !LoginController.IsConnected)
        {
            return;
        }
        if (_downloadAvatarDispose != null)
            _downloadAvatarDispose.Dispose();
        if (_downloadFlagDispose != null)
            _downloadFlagDispose.Dispose();
        GConnection.GetLeaderBoardAroundPlayer2("IQ_Ranking", 10, (lst) =>
        {
            if (lst != null)
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i].id == UseProfile.PlayFabId)
                    {
                        for (int j = i + 1; j < lst.Count; j++)
                        {
                            int score = System.Int32.Parse(lst[j].statistic);
                            IQPlayerBadgeData data = new IQPlayerBadgeData();
                            data.playerName = lst[j].name;
                            data.linkAvatar = lst[j].linkAvatar;
                            data.linkFlag = string.Format(Config.FLAG_API, RandomCountryCode());
                            data.score = score;
                            aroundPlayers.Add(data);
                            if (aroundPlayers.Count == 3)
                            {
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        });
    }


    private string RandomCountryCode()
    {
        string[] codes = {"AD","AE","AF","AG","AI","AL","AM","AO","AQ","AR","AS","AT","AU","AW","AX","AZ","BA","BB","BD","BE","BF","BG","BH","BI","BJ","BL","BM","BN","BO","BQ","BR","BS","BT","BV","BW","BY","BZ","CA","CC","CD","CF","CG","CH","CI","CK","CL","CM","CN","CO","CR","CU","CV","CW","CX","CY","CZ","DE","DJ","DK","DM","DO","DZ","EC","EE","EG","EH","ER","ES","ET","FI","FJ","FK","FM","FO","FR","GA","GB","GD","GE","GF","GG","GH","GI","GL","GM","GN","GP","GQ","GR","GS","GT","GU","GW","GY","HK","HM","HN","HR","HT","HU","ID","IE","IL","IM","IN","IO","IQ","IR","IS","IT","JE","JM","JO","JP","KE","KG","KH","KI","KM","KN","KP","KR","KW","KY","KZ","LA","LB","LC","LI","LK","LR","LS","LT","LU","LV","LY","MA","MC","MD","ME","MF","MG","MH","MK","ML","MM","MN","MO","MP","MQ","MR","MS","MT","MU","MV","MW","MX","MY","MZ","NA","NC","NE","NF","NG","NI","NL","NO","NP","NR","NU","NZ","OM","PA","PE","PF","PG","PH","PK","PL","PM","PN","PR","PS","PT","PW","PY","QA","RE","RO","RS","RU","RW","SA","SB","SC","SD","SE","SG","SH","SI","SJ","SK","SL","SM","SN","SO","SR","SS","ST","SV","SX","SY","SZ","TC","TD","TF","TG","TH","TJ","TK","TL","TM","TN","TO","TR","TT","TV","TW","TZ","UA","UG","UM","US","UY","UZ","VA","VC","VE","VG","VI","VN","VU","WF","WS","YE","YT","ZA","ZM","ZW"};
        int rIndex = UnityEngine.Random.Range(0, codes.Length);
        return codes[rIndex];
    }
}

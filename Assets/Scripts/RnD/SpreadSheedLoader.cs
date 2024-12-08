using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Reflection;

public class SpreadSheedLoader : MonoBehaviour
{
    // 구글 스프레드 시트를 TSV형식으로 읽어올 수 있도록 주소를 만들어줍니다.
    public static string GetSheetDataAddress(string address, string range, long sheetID)
    {
        return $"{address}/export?format=tsv&range={range}&gid={sheetID}";
    }

    public readonly string ADDRESS = "https://docs.google.com/spreadsheets/d/1GyHHFX3jWy41xQ47DWbtmVa6wGO7RC4XUriGkyF0zIU";
    public readonly string RANGE = "A2:B10";
    public readonly long SHEET_ID = 0;
    // 읽어온 스트링 데이터를 임시 저장해놓습니다.
    private string loadString = string.Empty;
    // 구글 스프레드 시트의 TSV 얻는 주소를 이용해 데이터를 읽어옵니다.
    private IEnumerator LoadData(Action<string> onMessageReceived)
    {
        // 구글 데이터 로딩 시작.
        UnityWebRequest www = UnityWebRequest.Get(GetSheetDataAddress(ADDRESS, RANGE, SHEET_ID));
        yield return www.SendWebRequest();
        //데이터 로딩 완료
        Debug.Log(www.downloadHandler.text);
        if(onMessageReceived != null)
        {
            onMessageReceived(www.downloadHandler.text);
        }

        yield return null;
    }
    public string StartLoader()
    {
        StartCoroutine(LoadData(output => loadString = output));

        return loadString;
    }
}

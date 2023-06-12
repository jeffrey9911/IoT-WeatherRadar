using AirtableUnity.PX.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class IAirtable : MonoBehaviour
{
    public bool AutoSetup = true;

    private string ApiVersion;
    private string AppKey;
    private string ApiKey;
    private string TableName;

    public void SetProxy(string ConApiVersion, string ConAppToken, string ConApiKey, string ConTableName)
    {
        ApiVersion = ConApiVersion;
        AppKey = ConAppToken;
        ApiKey = ConApiKey;
        TableName = ConTableName;
        if (AutoSetup)
        {
            AirtableUnity.PX.Proxy.SetEnvironment(ApiVersion, AppKey, ApiKey);
        }
    }


    /// <summary>
    /// Creating a record with specific new data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="newData"></param>
    /// <param name="callback"></param>
    public void CreateRecord<T>(string newData, Action<BaseRecord<T>> callback)
    {
        StartCoroutine(CreateRecordCo(newData, callback));
    }

    private IEnumerator CreateRecordCo<T>(string newData, Action<BaseRecord<T>> callback)
    {
        yield return StartCoroutine(AirtableUnity.PX.Proxy.CreateRecordCo<T>(TableName, newData, (createdRecord) =>
        {
            OnCreateResponseFinish(createdRecord);
            callback?.Invoke(createdRecord);
        }));
    }

    private static void OnCreateResponseFinish<T>(BaseRecord<T> record)
    {
        var msg = "record id: " + record?.id + "\n";
        msg += "created at: " + record?.createdTime;

        Debug.Log("[Airtable Unity] - Create Record: " + "\n" + msg);
    }


    /// <summary>
    /// Deleting a record
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="recordID"></param>
    /// <param name="callback"></param>
    public void DeleteRecord<T>(string recordID, Action<BaseRecord<T>> callback)
    {
        StartCoroutine(DeleteRecordCo(recordID, callback));
    }

    private IEnumerator DeleteRecordCo<T>(string recordId, Action<BaseRecord<T>> callback)
    {
        yield return StartCoroutine(AirtableUnity.PX.Proxy.DeleteRecordCo<T>(TableName, recordId, (recordDeleted) =>
        {
            OnDeleteResponseFinish(recordDeleted);
            callback?.Invoke(recordDeleted);
        }));
    }

    private static void OnDeleteResponseFinish<T>(BaseRecord<T> record)
    {
        var msg = "record id: " + record?.id + "\n";

        Debug.Log("[Airtable Unity] - Delete Record: " + "\n" + msg);
    }


    /// <summary>
    /// Update record with new data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="recordID"></param>
    /// <param name="newData"></param>
    /// <param name="callback"></param>
    /// <param name="useHardUpdate"></param>
    public void UpdateRecord<T>(string recordID, string newData, Action<BaseRecord<T>> callback, bool useHardUpdate = false)
    {
        StartCoroutine(GetRecordCo(TableName, recordID, newData, callback));
    }

    private IEnumerator GetRecordCo<T>(string tableName, string recordId, string newData, Action<BaseRecord<T>> callback, bool useHardUpdate = false)
    {
        yield return StartCoroutine(AirtableUnity.PX.Proxy.UpdateRecordCo<T>(tableName, recordId, newData,
            (baseRecordUpdated) =>
            {
                OnUpdateResponseFinish(baseRecordUpdated);
                callback?.Invoke(baseRecordUpdated);
            }, useHardUpdate));
    }

    private static void OnUpdateResponseFinish<T>(BaseRecord<T> record)
    {
        var msg = "record id: " + record?.id + "\n";
        msg += "created at: " + record?.createdTime;

        Debug.Log("[Airtable Unity] - Update Record: " + "\n" + msg);
    }

    /// <summary>
    /// Get specific field
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="recordID"></param>
    /// <param name="callback"></param>
    public void GetRecord(string recordID, Action<string[]> callback)
    {
        StartCoroutine(GetRecordCo(recordID, callback));
    }

    private IEnumerator GetRecordCo(string recordID, Action<string[]> callback)
    {
        yield return StartCoroutine(AirtableUnity.PX.Proxy.GetRecordField<BaseField>(TableName, recordID, 
            callback
            ));
    }

    private void OnGetResponseFinish(string[] record)
    {
        foreach (string str in record)
        {
            Debug.Log(str);
        }
    }
}

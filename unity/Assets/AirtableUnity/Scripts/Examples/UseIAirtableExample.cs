using AirtableUnity.PX.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;

public class UseIAirtableExample : MonoBehaviour
{
    /*
     * API Version: v0
     * App Token: appMyhojgXDHPge6s
     * Api Key: keyBWmSRLypBsJUvo
     * Airtable Name: WeatherTable
     */
    IAirtable myAirTable;

    private void Awake()
    {
        myAirTable = this.AddComponent<IAirtable>();
        myAirTable.SetProxy("v0", "appMyhojgXDHPge6s", "keyBWmSRLypBsJUvo", "WeatherTable");

    }

    [ContextMenu("Get Record")]
    public void GetTestRecord()
    {
        
    }

}

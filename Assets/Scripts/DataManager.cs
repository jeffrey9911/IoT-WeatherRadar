using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    /* 
     * Temperature: recnrKp7BbQOarpCm
     * Humidity: recxxnqEI0F3Ahz42
     * SunBright: reclZM4dzrlFPbRmn
     * Rain: recHIIbfGqdSEDiKR
     */
    public WeatherManager weatherManager;

    private IAirtable airtable;

    private int Temperature;
    private int Humidity;
    private int SunLight;
    private int Rain;

    private float weatherUpdateTimer = 0f;
    private float weatherUpdateInterval = 5f;

    private bool isTempUpdated, isHumidUpdated, isSunUpdated, isRainUpdated = false;

    private void Start()
    {
        airtable = this.GetComponent<IAirtable>();
        airtable.SetProxy("v0", "appMyhojgXDHPge6s", "keyBWmSRLypBsJUvo", "WeatherTable");
    }

    private void Update()
    {
        if(weatherUpdateTimer >= weatherUpdateInterval)
        {
            airtable.GetRecord("recnrKp7BbQOarpCm", OnGetResponseCallback);
            airtable.GetRecord("recxxnqEI0F3Ahz42", OnGetResponseCallback);
            airtable.GetRecord("reclZM4dzrlFPbRmn", OnGetResponseCallback);
            airtable.GetRecord("recHIIbfGqdSEDiKR", OnGetResponseCallback);
            weatherUpdateTimer = 0;
        }
        weatherUpdateTimer += Time.deltaTime;

        if(isTempUpdated && isHumidUpdated && isSunUpdated && isRainUpdated)
        {
            UpdateWeather();
            isTempUpdated = isHumidUpdated = isSunUpdated = isRainUpdated = false;
            Debug.Log("" + isTempUpdated + isHumidUpdated + isSunUpdated + isRainUpdated);
        }
    }

    private void OnGetResponseCallback(string[] record)
    {
        string dataNum = "";
        string dataType = "";

        foreach (string str in record)
        {
            int dataInd = str.IndexOf("Data\"");
            int typeInd = str.IndexOf("DataType\"");

            if(dataInd >= 0)
            {
                dataInd += 6;
                dataNum = str.Substring(dataInd);
            }

            if (typeInd >= 0)
            {
                typeInd += 11;
                dataType = str.Substring(typeInd, str.Length - typeInd - 1);

            }
        }

        if(dataNum.Length > 0 && dataType.Length > 0)
        {
            switch (dataType)
            {
                case "Temperature":
                    Temperature = GetIntData(dataNum);
                    isTempUpdated = true;
                    //Debug.Log($"Temperature: {Temperature}");
                    break;

                case "Humidity":
                    Humidity = GetIntData(dataNum);
                    isHumidUpdated = true;
                    //Debug.Log($"Humidity: {Humidity}");
                    break;

                case "SunBright":
                    SunLight = GetIntData(dataNum);
                    isSunUpdated = true;
                    //Debug.Log($"SunBright: {SunLight}");
                    break;

                case "Rain":
                    Rain = GetIntData(dataNum);
                    isRainUpdated = true;
                    //Debug.Log($"Rain: {Rain}");
                    break;
            }
        }
    }

    //"Data":111999
    //"DataType":

    private int GetIntData(string strNum)
    {
        int intNum;
        if(int.TryParse(strNum, out intNum))
        {
            return intNum;
        }
        return -999999;
    }

    private float GetPercent(int min, int max, int target)
    {
        float range = max - min;
        float pos = target - min;

        return pos / range;
    }

    private void UpdateWeather()
    {
        float tempPerc = GetPercent(0, 35, Temperature);
        Debug.Log($"TEMP: {tempPerc}\nSunlight: {SunLight}\nHumidity: {Humidity}\nRain: {Rain}");
        weatherManager.UpdateTempBri(new Vector2(tempPerc, SunLight));

        if (Humidity > 80 || Rain == 1)
        {
            if (Temperature <= 5) weatherManager.Snow();
            else weatherManager.Rain();
        }
        else if (Humidity > 50) weatherManager.Mist();
        else weatherManager.Clear();
    }

}

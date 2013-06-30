using System;
[Serializable]
public class StringSwitchTest
{
    public static string TestMethod(string switchCondition)
	{
		string result = string.Empty;
        switch (switchCondition)
        {
            case "Item1":
                result = "1";
                break;
            case "Item2":
                result = "2";
                break;
            case "Item3":
                result = "3";
                break;
            case "Item4":
                result = "4";
                break;
            case "Item5":
                result = "5";
                break;
            case "Item6":
                result = "6";
                break;
            case "Item7":
                result = "7";
                break;
            case "Item8":
                result = "8";
                break;
            case "Item9":
                result = "9";
                break;
            case "Item10":
                result = "10";
                break;
            case "Item11":
                result = "11";
                break;
            case "Item12":
                result = "12";
                break;
            case "Item13":
                result = "13";
                break;
            case "Item14":
                result = "14";
                break;
            case "Item15":
                result = "15";
                break;
        }
		return result;
	}
}

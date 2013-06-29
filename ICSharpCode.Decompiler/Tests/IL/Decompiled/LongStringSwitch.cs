using System;
using System.Collections.Generic;
[Serializable]
public class LongSwitchTest
{
    public static string TestMethod(string switchCondition)
	{
		string result = string.Empty;
        switch (switchCondition)
        {
            case "Item1":
                result = "V7Y44-9T38C-R2VJK-666HK-T7DDX";
                break;
            case "Item2":
                result = "H62QG-HXVKF-PP4HP-66KMR-CW9BM";
                break;
            case "Item3":
                result = "QYYW6-QP4CB-MBV6G-HYMCJ-4T3J4";
                break;
            case "Item4":
                result = "K96W8-67RPQ-62T9Y-J8FQJ-BT37T";
                break;
            case "OneNote":
                result = "Q4Y4M-RHWJM-PY37F-MTKWH-D3XHX";
                break;
            case "Outlook":
                result = "7YDC2-CWM8M-RRTJC-8MDVC-X3DWQ";
                break;
            case "PowerPoint":
                result = "RC8FX-88JRY-3PF7C-X8P67-P4VTT";
                break;
            case "Professional Plus":
                result = "VYBBJ-TRJPB-QFQRF-QFT4D-H3GVB";
                break;
            case "Project Professional":
                result = "YGX6F-PGV49-PGW3J-9BTGG-VHKC6";
                break;
            case "Project Standard":
                result = "4HP3K-88W3F-W2K3D-6677X-F9PGB";
                break;
            case "Publisher":
                result = "BFK7F-9MYHM-V68C7-DRQ66-83YTP";
                break;
            case "Small Business Basics":
                result = "D6QFG-VBYP2-XQHM7-J97RH-VVRCK";
                break;
            case "Standard":
                result = "V7QKV-4XVVR-XYV4D-F7DFM-8R6BM";
                break;
            case "Visio":
                result = "D9DWC-HPYVV-JGF4P-BTWQB-WX8BJ";
                break;
            case "Word":
                result = "HVHB3-C6FV7-KQX9W-YQG79-CRY7T";
                break;
        }
		return result;
	}
}

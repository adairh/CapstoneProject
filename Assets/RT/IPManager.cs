using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public static class IPManager
{
    public enum ADDRESSFAM
    {
        IPv4,
        IPv6
    }

    public static string GetIP(ADDRESSFAM Addfam)
    {
        var ret = "";
        var IPs = GetAllIPs(Addfam, false);

        foreach (var ip in IPs)
            if (ip != "127.0.0.1")
                ret = ip;

        return ret;
    }

    public static List<string> GetAllIPs(ADDRESSFAM Addfam, bool includeDetails)
    {
        //Return null if ADDRESSFAM is Ipv6 but Os does not support it
        if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6) return null;

        var output = new List<string>();

        foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
            var _type1 = NetworkInterfaceType.Wireless80211;
            var _type2 = NetworkInterfaceType.Ethernet;

            var isCandidate = item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // as of MacOS (10.13) and iOS (12.1), OperationalStatus seems to be always "Unknown".
            isCandidate = isCandidate && item.OperationalStatus == OperationalStatus.Up;
#endif

            if (isCandidate)
#endif
                foreach (var ip in item.GetIPProperties().UnicastAddresses)
                    //IPv4
                    if (Addfam == ADDRESSFAM.IPv4)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            var s = ip.Address.ToString();
                            if (includeDetails)
                                s += "  " + item.Description.PadLeft(6) +
                                     item.NetworkInterfaceType.ToString().PadLeft(10);
                            output.Add(s);
                        }
                    }

                    //IPv6
                    else if (Addfam == ADDRESSFAM.IPv6)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6) output.Add(ip.Address.ToString());
                    }
        }

        return output;
    }
}
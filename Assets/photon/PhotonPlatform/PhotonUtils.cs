using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt
{
    public static class PhotonUtils
    {
        //public static UdpEndPoint ToEndPoint(this CSteamID id)
       // {
       //     return new UdpEndPoint(new UdpSteamID(id.m_SteamID));
       // }

        public static UdpEndPoint ToEndPoint(this ulong steamId)
        {
            return new UdpEndPoint(new UdpSteamID(steamId));
        }
    }
}
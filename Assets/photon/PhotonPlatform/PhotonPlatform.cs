using System;
using System.Collections.Generic;
using System.Diagnostics;
using UdpKit;

public class PhotonPlatformConfig
{
    public String AppId;
    public String RegionMaster;
    public Boolean UsePunchThrough = false;
    public Boolean UseOnPremise = false;
    public String OnPremiseServerIpAddress = "127.0.0.1";
}

public class PhotonPlatform : UdpPlatform
{
    readonly PhotonPlatformConfig _config;
    readonly UdpPlatform _platform;

    class PrecisionTimer
    {
        static readonly long start = Stopwatch.GetTimestamp();
        static readonly double freq = 1.0 / (double)Stopwatch.Frequency;

        internal static uint GetCurrentTime()
        {
            long diff = Stopwatch.GetTimestamp() - start;
            double seconds = (double)diff * freq;
            return (uint)(seconds * 1000.0);
        }
    }

    public UdpPlatform PunchPlatform
    {
        get { return _platform; }
    }

    public override bool SupportsBroadcast
    {
        get
        {
            if (_config.UsePunchThrough)
            {
                return _platform.SupportsBroadcast;
            }

            return false;
        }
    }

    public override Boolean ShutdownOnConnectFailure
    {
        get { return false; }
    }

    public override bool SupportsMasterServer
    {
        get { return _config.UsePunchThrough; }
    }

    public PhotonPlatform(PhotonPlatformConfig config)
    {
        _config = config;

        if (_config.UsePunchThrough)
        {
            // create platform
            _platform = BoltLauncher.CreateUdpPlatform();

        }
    }

    public PhotonPlatform()
        : this(new PhotonPlatformConfig
        {
            AppId = BoltRuntimeSettings.instance.photonAppId,
            RegionMaster = BoltRuntimeSettings.photonCloudRegions[BoltRuntimeSettings.instance.photonCloudRegionIndex],
            UsePunchThrough = BoltRuntimeSettings.instance.photonUsePunch,
            UseOnPremise = BoltRuntimeSettings.instance.photonUseOnPremise,
            OnPremiseServerIpAddress = BoltRuntimeSettings.instance.photonOnPremiseIpAddress
        })
    {
    }

    public override UdpPlatformSocket CreateSocket()
    {
        return new PhotonSocket(this, _platform);
    }

    public override UdpIPv4Address GetBroadcastAddress()
    {
        if (_config.UsePunchThrough)
        {
            return _platform.GetBroadcastAddress();
        }

        return UdpIPv4Address.Any;
    }

    public override List<UdpPlatformInterface> GetNetworkInterfaces()
    {
        if (_config.UsePunchThrough)
        {
            return _platform.GetNetworkInterfaces();
        }

        return new List<UdpPlatformInterface>();
    }

    public override uint GetPrecisionTime()
    {
        return PrecisionTimer.GetCurrentTime();
    }

    public override Boolean SessionListProvidedExternally
    {
        get { return true; }
    }

    public override Boolean HandleConnectToSession(UdpSession session, System.Object protocolToken)
    {
        return PhotonPoller.Instance.JoinSession(session, protocolToken as Bolt.IProtocolToken);
    }

    public override Boolean HandleSetHostInfo(String servername, Boolean dedicated, System.Object protocolToken)
    {
        // set host info
        PhotonPoller.Instance.SetHostInfo(servername, dedicated, protocolToken as Bolt.IProtocolToken);

        // connect to zeus
        if (_config.UsePunchThrough)
        {
            Bolt.Zeus.Connect();
        }

        return _platform == null;
    }

    public override UdpIPv4Address[] ResolveHostAddresses(string host)
    {
        if (_config.UsePunchThrough)
        {
            return _platform.ResolveHostAddresses(host);
        }

        return new UdpIPv4Address[0];
    }

    public override void OnStartDone()
    {
        base.OnStartDone();

        if (_config.UsePunchThrough)
        {
            _platform.OnStartDone();

            // register token class
            BoltNetwork.RegisterTokenClass<PhotonHostInfoToken>();

            if (BoltNetwork.isClient)
            {
                Bolt.Zeus.Connect();
            }
        }
    }

    public override UdpPlatformSocket CreateBroadcastSocket(UdpEndPoint endpoint)
    {
        if (_config.UsePunchThrough)
        {
            return _platform.CreateBroadcastSocket(endpoint);
        }

        return base.CreateBroadcastSocket(endpoint);
    }

    public override void OnStartupFailed()
    {
        base.OnStartupFailed();

        if (_config.UsePunchThrough)
        {
            _platform.OnStartupFailed();
        }
    }

    public override void OnStartBegin()
    {
        base.OnStartBegin();

        PhotonPoller.CreatePoller(_config);

        if (_config.UsePunchThrough)
        {
            _platform.OnStartBegin();
        }
    }
}

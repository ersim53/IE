using UdpKit;
using UnityEngine;

public class PhotonSocket : UdpPlatformSocket
{
    bool _bound;
    UdpEndPoint _endpoint;

    readonly PhotonPlatform _platform;

    readonly UdpPlatformSocket _udpSocket;
    readonly UdpPlatform _udpPlatform;

    public PhotonSocket(PhotonPlatform platform, UdpPlatform udpPlatform)
    {
        _platform = platform;
        _udpPlatform = udpPlatform;

        if (_udpPlatform != null)
        {
            _udpSocket = _udpPlatform.CreateSocket();
        }
    }

    public override UdpPlatform Platform
    {
        get { return _platform; }
    }

    public override bool Broadcast
    {
        get
        {
            if (_udpSocket != null)
            {
                return _udpSocket.Broadcast;
            }

            return false;
        }

        set
        {
            if (_udpSocket != null)
            {
                _udpSocket.Broadcast = value;
            }
            else
            {
                Debug.LogErrorFormat("{0} does not support broadcasting", typeof(PhotonSocket).Name);
            }
        }
    }

    public override UdpEndPoint EndPoint
    {
        get
        {
            if (_udpSocket != null)
            {
                return _udpSocket.EndPoint;
            }

            return _endpoint;
        }
    }

    public override string Error
    {
        get
        {
            if (_udpSocket != null)
            {
                return _udpSocket.Error;
            }

            return null;
        }
    }

    public override bool IsBound
    {
        get
        {
            if (_udpSocket != null)
            {
                return _bound && _udpSocket.IsBound;
            }

            return _bound;
        }
    }

    public override void Bind(UdpEndPoint ep)
    {
        _bound = true;
        _endpoint = ep;

        if (_udpSocket != null)
        {
            _udpSocket.Bind(ep);
        }
    }

    public override void Close()
    {
        _bound = false;
        _endpoint = UdpEndPoint.Any;

        if (_udpSocket != null)
        {
            _udpSocket.Close();
        }
    }

    public override int RecvFrom(byte[] buffer, int bufferSize, ref UdpEndPoint endpoint)
    {
        var r = PhotonPoller.Instance.RecvFrom(buffer, bufferSize, ref endpoint);
        if (r == -1)
        {
            if (_udpSocket != null)
            {
                return _udpSocket.RecvFrom(buffer, bufferSize, ref endpoint);
            }
            else
            {
                return -1;
            }
        }
        else
        {
            return r;
        }
    }

    public override bool RecvPoll()
    {
        return RecvPoll(0);
    }

    public override bool RecvPoll(int timeout)
    {
        return PhotonPoller.Instance.RecvPoll() || (_udpSocket != null && _udpSocket.RecvPoll(timeout));
    }

    public override int SendTo(byte[] buffer, int bytesToSend, UdpEndPoint endpoint)
    {
        if (endpoint.Port == 0)
        {
            return PhotonPoller.Instance.SendTo(buffer, bytesToSend, endpoint);
        }
        else
        {
            if (_udpSocket != null)
            {
                return _udpSocket.SendTo(buffer, bytesToSend, endpoint);
            }
        }

        return 0;
    }
}

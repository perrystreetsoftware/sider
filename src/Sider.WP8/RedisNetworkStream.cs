﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sider
{
    public class RedisNetworkStream : Stream
    {
        Socket mSocket;
        private int mTimeout;
        private IPEndPoint mEndpoint;
        int mLastOperationCount = 0;

        private const int DefaultTimeout = 60000;

        public RedisNetworkStream(Socket socket, string host, int port, int timeout)
        {
            this.mSocket = socket;
            var ipAddress = IPAddress.Parse(host);
            this.mEndpoint = new IPEndPoint(ipAddress, port);
            this.mTimeout = timeout > 1000 ? timeout : RedisNetworkStream.DefaultTimeout;
        }

        public void Connect()
        {

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = this.mEndpoint;
            args.Completed += socket_Connected;
            args.UserToken = new ManualResetEvent(false);
            this.mSocket.ConnectAsync(args);
            ((ManualResetEvent)args.UserToken).WaitOne(this.mTimeout);
        }

        void socket_Connected(object sender, SocketAsyncEventArgs e)
        {
            ((ManualResetEvent)e.UserToken).Set();
        }

        public override int ReadByte()
        {
            byte [] oneByte = new byte[1];
            this.Read(oneByte, 0, 1);
            return oneByte[0];
        }

        public override int Read(
            byte[] buffer,
            int offset,
            int size)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = this.mEndpoint;
            args.SetBuffer(buffer, offset, size);
            args.Completed += socket_ReadByteCompleted;
            args.UserToken = new ManualResetEvent(false);

            this.mSocket.ReceiveAsync(args);
            if (((ManualResetEvent)args.UserToken).WaitOne(this.mTimeout))
            {
                return this.mLastOperationCount;
            }

            return 0;
        }

        void socket_ReadByteCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.mLastOperationCount = e.Count;
            ((ManualResetEvent)e.UserToken).Set();
        }

        public override void WriteByte(byte value)
        {
            byte[] buf = new byte[1];
            buf[0] = value;
            Write(buf, 0, 1);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = this.mEndpoint;
            args.SetBuffer(buffer, offset, count);
            args.Completed += write_Completed;
            args.UserToken = new ManualResetEvent(false);
            this.mSocket.SendAsync(args);
            ((ManualResetEvent)args.UserToken).WaitOne(this.mTimeout);
        }

        void write_Completed(object sender, SocketAsyncEventArgs e)
        {
            this.mLastOperationCount = e.Count;
            ((ManualResetEvent)e.UserToken).Set();
        }

        public override void Flush()
        {
            // Flush is not defined in the .NET socket implementation
            return;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanRead
        {
            get { return true; }
        }
    }
}


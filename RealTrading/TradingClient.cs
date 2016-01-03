using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTrading
{
    public sealed class TradingClient : IDisposable
    {
        public const int InvalidClientId = -1;
        private const int MaxErrorStringSize = 1024;
        private const int MaxResultStringSize = 1024 * 1024;

        private bool _disposed = false;

        public int ClientId { get; private set; }

        public TradingClient()
        {
            ClientId = InvalidClientId;
        }

        ~TradingClient()
        {
            Dispose();
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        private void CheckLoggedOn()
        {
            if (!IsLoggedOn())
            {
                throw new InvalidOperationException("client has not logged on yet");
            }
        }

        public bool LogOn(
            string address, 
            short port, 
            string protocolVersion, 
            short yybId, 
            string accountNo,
            short accountType,
            string tradeAccount, 
            string tradePassword, 
            string communicationPassword,
            out string error)
        {
            CheckDisposed();

            error = string.Empty;

            if (IsLoggedOn())
            {
                throw new InvalidOperationException("Client has logged on");
            }

            StringBuilder errorInfo = new StringBuilder(MaxErrorStringSize);

            int clientId = TdxWrapper.Logon(
                address,
                port,
                protocolVersion,
                yybId,
                accountNo,
                tradeAccount,
                tradePassword,
                communicationPassword,
                errorInfo);

            if (clientId < 0)
            {
                error = errorInfo.ToString();
            }
            else
            {
                ClientId = clientId;
            }

            return IsLoggedOn();
        }

        public bool IsLoggedOn()
        {
            CheckDisposed();

            return ClientId != InvalidClientId;
        }
    
        public void LogOff()
        {
            CheckDisposed();

            if (!IsLoggedOn())
            {
                return;
            }

            TdxWrapper.Logoff(ClientId);

            ClientId = InvalidClientId;
        }

        public bool QueryData(DataCategory category, out string result, out string error)
        {
            CheckDisposed();
            CheckLoggedOn();

            StringBuilder resultInfo = new StringBuilder(MaxResultStringSize);
            StringBuilder errorInfo = new StringBuilder(MaxErrorStringSize);

            TdxWrapper.QueryData(ClientId, (int)category, resultInfo, errorInfo);

            error = errorInfo.ToString();
            result = resultInfo.ToString();

            return  string.IsNullOrEmpty(error);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (IsLoggedOn())
                {
                    LogOff();
                }

                _disposed = true;
            }
        }
    }
}

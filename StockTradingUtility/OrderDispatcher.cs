using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    sealed class OrderDispatcher
    {
        private readonly TradingClient _client = null;

        private object _dispatcherLockObj = new object();
        private object _orderLockObj = new object();

        private bool _isStopped = false;

        private IDictionary<int, Guid> _orderNoToRequestIdMap = new Dictionary<int, Guid>();

        public OrderDispatcher(TradingClient client)
        {
            if (client == null)
            {
                throw new ArgumentException();
            }

            _client = client;
        }

        public void Stop()
        {
            lock (_dispatcherLockObj)
            {
                _isStopped = true;
            }
        }

        public DispatchOrderResult DispatchOrder(OrderRequest request, out string error)
        {
            var result = _client.SendOrder(request, out error);

            if (result == null)
            {
                return null;
            }

            lock (_orderLockObj)
            {
                _orderNoToRequestIdMap.Add(result.OrderNo, request.RequestId);
            }

            DispatchOrderResult dispatchOrderResult = new DispatchOrderResult()
            {
                OrderNo = result.OrderNo,
                Request = request
            };


            return dispatchOrderResult;
        }

        public bool CancelOrder(DispatchOrderResult result, out string error)
        {
            bool isCancelled = _client.CancelOrder(result.Request.SecurityCode, result.OrderNo, out error);

            if (isCancelled)
            {
                lock (_orderLockObj)
                {
                    _orderNoToRequestIdMap.Remove(result.OrderNo);
                }
            }

            return isCancelled;
        }
    }
}

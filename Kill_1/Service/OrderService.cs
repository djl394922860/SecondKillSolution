﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Kill_1.Common;
using Kill_1.Data;
using Kill_1.Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Kill_1.Service
{
    public class OrderService : IOrderService
    {
        private readonly KillDbContext _dbContext;

        public OrderService(KillDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int CreateOrder(int stockId)
        {
            // 参数进程内限流(ConcurrentDictionary实现且不带清楚历史限流记录功能)
            RateLimite();

            var stock = _dbContext.Stocks.FirstOrDefault(x => x.Id == stockId);
            if (stock == null)
            {
                throw new ArgumentNullException(nameof(stock));
            }

            if (stock.Count <= 0)
            {
                throw new ArgumentException("库存不足");
            }

            stock.Count--;
            stock.Sale++;

            var order = new Order()
            {
                Name = stock.Name,
                StockId = stock.Id
            };

            _dbContext.Orders.Add(order);

            _dbContext.SaveChanges();

            return order.Id;
        }

        private static readonly ConcurrentDictionary<string, int> TimeDic = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// 设置为1s中只能给2次请求,进程级别的限流
        /// </summary>
        private static void RateLimite()
        {
            string timeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (TimeDic.TryGetValue(timeStr, out int num))
            {
                if (num > 2)
                {
                    throw new RateLimiteException($"1s内只允许2次请求,您的请求超出范围 key:{timeStr} value:{num}");
                }
                TimeDic[timeStr]++;
            }
            else
            {
                TimeDic.TryAdd(timeStr, 1);
            }
        }

        /// <summary>
        /// 在限定时间内限制指定的并发量
        /// </summary>
        /// <param name="second">秒</param>
        /// <param name="limitNum">次数</param>
        private static void RateLimit(int second, int limitNum)
        {

        }
    }
}
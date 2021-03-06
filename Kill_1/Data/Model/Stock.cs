﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Kill_1.Data.Model
{
    public class Stock
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public int Sale { get; set; }
        [Timestamp]
        public DateTime RowVersion { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAggregator.Unit.Tests.Models
{
    public class GetRecordsRequest
    {
        public string ContentType { get; set; } = string.Empty;
        public int PageSize { get; set; }
    }
}

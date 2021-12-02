using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDataProject
{
    public class YahooFinanceStock
    {
        [JsonProperty("chart")]
        public YahooFinanceChart Chart { get; set; }
    }
    
    public class YahooFinanceChart
    {
        [JsonProperty("result")]
        public YahooFinanceChartResult[] Results { get; set; }

        [JsonProperty("error")]
        public YahooFinanceChartError Error { get; set; }
    }

    public class YahooFinanceChartResult
    {
        [JsonProperty("timestamp")]
        public long[] Timestamps { get; set; }

        [JsonProperty("indicators")]
        public YahooFinanceChartIndicator Indicator { get; set; }
    }

    public class YahooFinanceChartIndicator
    {
        [JsonProperty("quote")]
        public YahooFinanceChartQuote[] Quotes { get; set; }
    }

    public class YahooFinanceChartQuote
    {
        [JsonProperty("open")]
        public double[] Open { get; set; }

        [JsonProperty("low")]
        public double[] Low { get; set; }

        [JsonProperty("high")]
        public double[] High { get; set; }

        [JsonProperty("close")]
        public double[] Close { get; set; }
    }

    public class YahooFinanceChartError
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class YahooFinanceError
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}

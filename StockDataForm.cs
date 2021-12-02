using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace StockDataProject
{
    public partial class StockDataForm : Form
    {
        double candleWidth = 0.8;

        public string ticker, range;
        public DateTime dateFrom, dateTo;
        public int pattern;

        public bool retrieveSuccess;

        public StockDataForm()
        {
            InitializeComponent();
        }

        public void GenerateChart(bool download = false, string filePath = null)
        {
            lblTicker.Text = ticker;
            lblDateFrom.Text = dateFrom.ToShortDateString();
            lblDateTo.Text = dateTo.ToShortDateString();
            lblPeriod.Text = Constants.GetRanges().Where(x => x.Value == range).First().Key;

            retrieveSuccess = false;

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add("period1", ((DateTimeOffset)dateFrom).ToUnixTimeSeconds().ToString());
            queryParams.Add("period2", ((DateTimeOffset)dateTo).ToUnixTimeSeconds().ToString());
            queryParams.Add("interval", range);
            queryParams.Add("events", "true");
            queryParams.Add("includeAdjustedClose", "true");

            string query = string.Empty;

            for (int i = 0; i < queryParams.Count; i++)
            {
                query += queryParams.ElementAt(i).Key + "=" + queryParams.ElementAt(i).Value;
                if (i < queryParams.Count - 1) query += "&";
            }

            UriBuilder uriBuilder = new UriBuilder(Constants.BASE_URL)
            {
                Path = "v7/finance/download/" + ticker,
                Query = query
            };

            WebRequest request = WebRequest.Create(uriBuilder.Uri.AbsoluteUri);
            Stream stream;

            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    stream = response.GetResponseStream();

                    Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        bool header = true;

                        List<Candlestick> candlesticks = new List<Candlestick>();

                        string data = reader.ReadToEnd();

                        string[] data_array = data.Split(char.Parse("\n"));

                        for (int i = 0; i < data_array.Length; i++)
                        {
                            string datum = data_array[i];
                            if (!header)
                            {
                                string[] values = datum.Split(',');
                                Candlestick candlestick = new Candlestick()
                                {
                                    DateTime = DateTime.Parse(values[0]),
                                    Open = double.Parse(values[1]),
                                    High = double.Parse(values[2]),
                                    Low = double.Parse(values[3]),
                                    Close = double.Parse(values[4])
                                };
                                candlesticks.Add(candlestick);
                            }
                            header = false;
                        }

                        if (candlesticks.Count > 0)
                        {
                            if (download)
                            {
                                FileStream fileStream = File.Create(filePath);

                                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                                {
                                    streamWriter.Write(data);
                                }

                                MessageBox.Show("CSV File successfully downloaded.", "Download success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            else
                            {
                                chtStock.Series["Stock Data"].Points.Clear();

                                double highest = candlesticks.Max(x => x.High);
                                double lowest = candlesticks.Min(x => x.Low);

                                double margin = (highest - lowest) * 0.1;

                                double maxMargin = highest + margin;
                                double minMargin = lowest - margin;

                                if (minMargin < 0) minMargin = 0;

                                chtStock.ChartAreas[0].AxisY.Maximum = maxMargin;
                                chtStock.ChartAreas[0].AxisY.Minimum = minMargin;

                                double chartAreaWidth = chtStock.ChartAreas[0].InnerPlotPosition.Width / 100 * chtStock.ChartAreas[0].Position.Width;
                                double chartAreaHeight = chtStock.ChartAreas[0].InnerPlotPosition.Height / 100 * chtStock.ChartAreas[0].Position.Height;

                                double annotWidth = chartAreaWidth / (candlesticks.Count + 1);

                                chtStock.Annotations.Clear();

                                for (int i = 0; i < candlesticks.Count; i++)
                                {
                                    DataPoint dataPoint = new DataPoint(i, new[] { candlesticks[i].High, candlesticks[i].Low, candlesticks[i].Open, candlesticks[i].Close })
                                    {
                                        AxisLabel = candlesticks[i].DateTime.ToString()
                                    };
                                    chtStock.Series["Stock Data"].Points.Add(dataPoint);
                                }
                                
                                double chartYValueHeight = chtStock.ChartAreas[0].AxisY.Maximum - chtStock.ChartAreas[0].AxisY.Minimum;

                                double widthInPixels = chtStock.Width;
                                double chartAreaWidthInPixels = chtStock.ChartAreas[0].Position.Width / 100d * widthInPixels;
                                double chartPlotWidthInPixels = chtStock.ChartAreas[0].InnerPlotPosition.Width / 100d * chartAreaWidthInPixels;
                                double candleWidthInPixels = candleWidth * chartPlotWidthInPixels / (candlesticks.Count + 1);

                                double heightInPixels = chtStock.Height;
                                double chartAreaHeightInPixels = chtStock.ChartAreas[0].Position.Height / 100d * heightInPixels;
                                double chartPlotHeightInPixels = chtStock.ChartAreas[0].InnerPlotPosition.Height / 100d * chartAreaHeightInPixels;

                                for (int i = 0; i < candlesticks.Count; i++)
                                {
                                    double diffLeg = candlesticks[i].High - candlesticks[i].Low;
                                    double percentage = diffLeg / chartYValueHeight * 100d;
                                    double annotHeight = percentage / chartAreaHeight * 100d;

                                    double diffCandle = Math.Abs(candlesticks[i].Open - candlesticks[i].Close);

                                    double diffWick = 0, diffShadow = 0;

                                    if (candlesticks[i].Open >= candlesticks[i].Close) diffWick = candlesticks[i].High - candlesticks[i].Open;
                                    else if (candlesticks[i].Close >= candlesticks[i].Open) diffWick = candlesticks[i].High - candlesticks[i].Close;

                                    if (candlesticks[i].Open <= candlesticks[i].Close) diffShadow = candlesticks[i].Open - candlesticks[i].Low;
                                    else if (candlesticks[i].Close <= candlesticks[i].Open) diffShadow = candlesticks[i].Close - candlesticks[i].Low;

                                    double candleStickHeightInPixels = diffLeg / chartYValueHeight * chartPlotHeightInPixels;
                                    double candleHeightInPixels = diffCandle / chartYValueHeight * chartPlotHeightInPixels;
                                    double candleWickInPixels = diffWick / chartYValueHeight * chartPlotHeightInPixels;
                                    double candleShadowInPixels = diffShadow / chartYValueHeight * chartPlotHeightInPixels;

                                    double nextCandleHeightInPixels = 0;

                                    if (i < candlesticks.Count - 1)
                                        nextCandleHeightInPixels = Math.Abs(candlesticks[i + 1].Open - candlesticks[i + 1].Close) / chartYValueHeight * chartPlotHeightInPixels;

                                    bool isDoji =
                                        pattern == 1 &&
                                        Math.Ceiling(candleHeightInPixels) <= 1;

                                    bool isLongLeggedDoji =
                                        pattern == 2 &&
                                        Math.Ceiling(candleHeightInPixels) <= 1 &&
                                        candleWidthInPixels * 2 < candleWickInPixels &&
                                        candleWidthInPixels * 2 < candleShadowInPixels &&
                                        (
                                            (candleWickInPixels / candleShadowInPixels <= 1 && candleWickInPixels / candleShadowInPixels > 0.75) ||
                                            (candleShadowInPixels / candleWickInPixels <= 1 && candleShadowInPixels / candleWickInPixels > 0.75)
                                        );

                                    bool isGravestoneDoji =
                                        pattern == 3 &&
                                        Math.Ceiling(candleHeightInPixels) <= 1 &&
                                        Math.Ceiling(candleShadowInPixels) <= 1 &&
                                        Math.Ceiling(candleWickInPixels) >= 1;

                                    bool isDragonflyDoji =
                                        pattern == 4 &&
                                        Math.Ceiling(candleHeightInPixels) <= 1 &&
                                        Math.Ceiling(candleWickInPixels) <= 1 &&
                                        Math.Ceiling(candleShadowInPixels) >= 1;

                                    bool isBullishMarubozu =
                                        pattern == 5 &&
                                        Math.Ceiling(candleHeightInPixels) > 1 &&
                                        Math.Ceiling(candleWickInPixels) == 0 &&
                                        Math.Ceiling(candleShadowInPixels) == 0 &&
                                        candlesticks[i].Open < candlesticks[i].Close;

                                    bool isBearishMarubozu =
                                        pattern == 6 &&
                                        Math.Ceiling(candleHeightInPixels) > 1 &&
                                        Math.Ceiling(candleWickInPixels) == 0 &&
                                        Math.Ceiling(candleShadowInPixels) == 0 &&
                                        candlesticks[i].Open > candlesticks[i].Close;

                                    bool isBullishHarami =
                                        pattern == 7 &&
                                        Math.Ceiling(candleHeightInPixels) > 1 &&
                                        Math.Ceiling(nextCandleHeightInPixels) >= 1 &&
                                        i < candlesticks.Count - 1 &&
                                        candlesticks[i].Open > candlesticks[i].Close &&
                                        candlesticks[i + 1].Open < candlesticks[i + 1].Close &&
                                        candlesticks[i].Close < candlesticks[i + 1].Open &&
                                        candlesticks[i].Open > candlesticks[i + 1].Close;

                                    bool isBearishHarami =
                                        pattern == 8 &&
                                        Math.Ceiling(candleHeightInPixels) > 1 &&
                                        Math.Ceiling(nextCandleHeightInPixels) >= 1 &&
                                        i < candlesticks.Count - 1 &&
                                        candlesticks[i].Open < candlesticks[i].Close &&
                                        candlesticks[i + 1].Open > candlesticks[i + 1].Close &&
                                        candlesticks[i].Close > candlesticks[i + 1].Open &&
                                        candlesticks[i].Open < candlesticks[i + 1].Close;

                                    if (isDoji || isLongLeggedDoji || isGravestoneDoji || isDragonflyDoji || isBullishMarubozu || isBearishMarubozu || isBullishHarami || isBearishHarami)
                                    {
                                        RectangleAnnotation annotation = new RectangleAnnotation();
                                        annotation.BackColor = Color.Transparent;
                                        annotation.ToolTip = "rectangle annotation";
                                        annotation.SmartLabelStyle.Enabled = false;

                                        int ins = 1, n = i;
                                        if (isBullishHarami || isBearishHarami)
                                        {
                                            double lst = candlesticks[i].Low;
                                            double hst = candlesticks[i].High;

                                            if (candlesticks[i].Low > candlesticks[i + 1].Low)
                                            {
                                                lst = candlesticks[i + 1].Low;
                                            }

                                            if (candlesticks[i].High < candlesticks[i + 1].High)
                                            {
                                                hst = candlesticks[i + 1].High;
                                                ins = -1;
                                                n = i + 1;
                                            }

                                            double diffTwoLegs = hst - lst;
                                            percentage = diffTwoLegs / chartYValueHeight * 100d;
                                            annotHeight = percentage / chartAreaHeight * 100d;
                                            annotation.Width = annotWidth * 2;
                                            annotation.AnchorOffsetX = ins * annotWidth / 2;
                                        }
                                        else
                                        {
                                            annotation.Width = annotWidth;
                                            annotation.AnchorOffsetX = 0;
                                        }

                                        annotation.Height = annotHeight;
                                        annotation.AnchorOffsetY = -percentage;
                                        annotation.SetAnchor(chtStock.Series[0].Points[n]);
                                        chtStock.Annotations.Add(annotation);
                                    }
                                }
                            }
                            retrieveSuccess = true;
                        }
                        else
                        {
                            MessageBox.Show("There is no stock data associated with that ticker.", "Invalid ticker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

            catch (WebException e)
            {
                stream = e.Response.GetResponseStream();

                using (StreamReader reader = new StreamReader(stream))
                {
                    string error = reader.ReadToEnd();

                    MessageBox.Show(error, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message, "App Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StockDataForm_Load(object sender, EventArgs e)
        {
            new StockDataStartForm(this).ShowDialog();
            chtStock.Series["Stock Data"].SetCustomProperty("PointValue", candleWidth.ToString());
        }

        private void cbPattern_SelectedIndexChanged(object sender, EventArgs e)
        {
            pattern = cbPattern.SelectedIndex;
            GenerateChart();
        }

        private void btnRetrieve_Click(object sender, EventArgs e)
        {
            new StockDataStartForm(this, true).ShowDialog();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (saveAsCSVDialog.ShowDialog() == DialogResult.OK)
            {
                GenerateChart(true, saveAsCSVDialog.FileName);
            }
        }
    }
}

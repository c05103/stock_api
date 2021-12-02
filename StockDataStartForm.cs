using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockDataProject
{
    public partial class StockDataStartForm : Form
    {
        bool again;

        Dictionary<string, string> tickers = Constants.GetTickers();
        Dictionary<string, string> ranges = Constants.GetRanges();

        StockDataForm stockDataForm;

        public StockDataStartForm(StockDataForm stockDataForm, bool again = false)
        {
            this.stockDataForm = stockDataForm;
            this.again = again;

            InitializeComponent();
        }

        private void btnRetrieve_Click(object sender, EventArgs e)
        {
            if (cbTicker.SelectedIndex < 0)
            {
                MessageBox.Show("Please select ticker", "Invalid ticker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cbPeriod.SelectedIndex < 0)
            {
                MessageBox.Show("Please select candlestick period", "Invalid candlestick period", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            stockDataForm.ticker = cbTicker.SelectedItem.ToString();
            stockDataForm.dateFrom = dtFrom.Value;
            stockDataForm.dateTo = dtTo.Value;
            stockDataForm.range = ranges[cbPeriod.SelectedItem.ToString()];
            stockDataForm.pattern = 0;
            if (stockDataForm.cbPattern.SelectedIndex == 0) stockDataForm.GenerateChart();
            stockDataForm.cbPattern.SelectedIndex = 0;
            stockDataForm.cbPattern.Enabled = true;
            stockDataForm.btnDownload.Enabled = true;

            Close();
        }

        private void StockDataStartForm_Load(object sender, EventArgs e)
        {
            LoadTickers();
        }

        private void LoadTickers()
        {
            Dictionary<string, string> tickers = this.tickers.Where(x => x.Key.ToLower().StartsWith(cbTicker.Text.ToLower())).Take(10).ToDictionary(x => x.Key, x => x.Value);

            foreach (var ticker in tickers)
            {
                cbTicker.Items.Add(ticker.Key);
            }
        }

        private void cbTicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
            {
                if (cbTicker.SelectedIndex == 9)
                {
                    if (e.KeyCode == Keys.Down)
                    {
                        int nextIndex = tickers.ToList().IndexOf(tickers.Where(x => x.Key == cbTicker.SelectedItem.ToString()).First()) + 1;
                        if (nextIndex < tickers.Count)
                        {
                            cbTicker.Items.RemoveAt(0);
                            cbTicker.Items.Add(tickers.ElementAt(nextIndex).Key);
                            cbTicker.SelectedIndex++;
                        }
                    }
                }
                if (cbTicker.SelectedIndex == 0)
                {
                    if (e.KeyCode == Keys.Up)
                    {
                        int prevIndex = tickers.ToList().IndexOf(tickers.Where(x => x.Key == cbTicker.SelectedItem.ToString()).First()) - 1;
                        if (prevIndex >= 0)
                        {
                            cbTicker.Items.RemoveAt(cbTicker.Items.Count - 1);
                            cbTicker.Items.Insert(0, tickers.ElementAt(prevIndex).Key);
                            cbTicker.SelectedIndex--;
                        }
                    }
                }
            }
            else
            {
                char c = Convert.ToChar(e.KeyCode);

                if (char.IsLetter(c))
                {
                    Dictionary<string, string> tickerFiltered = tickers.Where(x => x.Key.ToLower().StartsWith(c.ToString().ToLower())).ToDictionary(x => x.Key, x => x.Value);

                    if (tickerFiltered.Count > 0)
                    {
                        cbTicker.Items.Clear();

                        int index = tickers.ToList().IndexOf(tickerFiltered.First());

                        if (index < 10)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                cbTicker.Items.Add(tickers.ElementAt(i).Key);
                            }
                        }

                        else if (index >= 10 && index < tickers.Count - 10)
                        {
                            for (int i = index; i < index + 10; i++)
                            {
                                cbTicker.Items.Add(tickers.ElementAt(i).Key);
                            }
                        }

                        else if (index >= tickers.Count - 10 && index < tickers.Count)
                        {
                            for (int i = tickers.Count - 10; i < tickers.Count; i++)
                            {
                                cbTicker.Items.Add(tickers.ElementAt(i).Key);
                            }
                        }
                    }
                }
            }
        }
    }
}

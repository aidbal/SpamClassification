using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpamClassification

{
    public partial class Form1 : Form
    {
        private Data table;

        private class Data
        {
            //int[0] žodžio pasikartojimas spamo laiškuose
            //int[1] žodžio pasikartojimas ne spamo laiškuose
            //int[2] žodžio tikimybė, kad jis spam
            //int[3] žodžio tikimybė, kad jis ne spam
            private Dictionary<string, int[]> table;
            public int SpamCount { set; get; }
            public int HamCount { set; get; }

            public Data()
            {
                table = new Dictionary<string, int[]> { };
                SpamCount = 0;
                HamCount = 0;
            }

            public void addSpamPair(string word)
            {
                if (table.ContainsKey(word))
                    table[word][0] += 1;
                else
                {
                    int[] arr = new int[4];
                    arr[0] = 1;
                    arr[1] = 0;
                    arr[2] = 0;
                    arr[3] = 0;
                    table.Add(word, arr);
                }
            }
            public void addNotSpamPair(string word)
            {
                if (table.ContainsKey(word))
                    table[word][1] += 1;
                else
                {
                    int[] arr = new int[4];
                    arr[0] = 0;
                    arr[1] = 1;
                    arr[2] = 0;
                    arr[3] = 0;
                    table.Add(word, arr);
                }
            }
            public int[] getValue(string word)
            {
                return table[word];
            }
            public Dictionary<string, int[]> getAllValues()
            {
                return table;
            }
        }

        public Form1()
        {
            InitializeComponent();
            table = new Data();
        }


        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog fop = new OpenFileDialog();
                fop.Multiselect = true;
                fop.InitialDirectory = "D:\\STUDIJOS\\6 semestras\\Intelektikos pagrindai\\L3";
                fop.Filter = "txt|*.txt";
                fop.Title = "Pasirinkti spamo failus";

                String pattern = @"[A-Za-z0-9$'""]+";

                if (fop.ShowDialog() == DialogResult.OK)
                {
                    foreach (String filename in fop.FileNames)
                    {
                        FileStream FS = new FileStream(filename, FileMode.Open, FileAccess.Read);
                        var lines = File.ReadLines(filename);
                        foreach (var line in lines)
                        {
                            foreach (Match m in Regex.Matches(line, pattern))
                            {
                                table.addSpamPair(m.Value);
                            }
                        }
                        table.SpamCount += 1;
                        FS.Close();
                    }
                    MessageBox.Show("Spamo failai sėkmingai nuskaityti!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                foreach (KeyValuePair<string, int[]> pair in table.getAllValues())
                {
                    richTextBox1.Text += pair.Key + ": " + pair.Value[0] + ", " + pair.Value[1] + "\n";
                    //Console.WriteLine("Key: {0} Values: {1},{2}",
                    //    pair.Key,
                    //    pair.Value[0], pair.Value[1]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog fop = new OpenFileDialog();
                fop.Multiselect = true;
                fop.InitialDirectory = "D:\\STUDIJOS\\6 semestras\\Intelektikos pagrindai\\L3";
                fop.Filter = "txt|*.txt";
                fop.Title = "Pasirinkti ne spamo failus";

                String pattern = @"[A-Za-z0-9$'""]+";

                if (fop.ShowDialog() == DialogResult.OK)
                {
                    foreach (String filename in fop.FileNames)
                    {
                        FileStream FS = new FileStream(filename, FileMode.Open, FileAccess.Read);
                        var lines = File.ReadLines(filename);
                        foreach (var line in lines)
                        {
                            foreach (Match m in Regex.Matches(line, pattern))
                            {
                                table.addNotSpamPair(m.Value);
                            }
                        }
                        table.HamCount += 1;
                        FS.Close();
                    }
                    MessageBox.Show("Ne spamo failai sėkmingai nuskaityti!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }
    }
}

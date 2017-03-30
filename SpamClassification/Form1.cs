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
        private List<fileData> filesData;
        public static double defaultProbability = 0.4;
        public static string defaultLexemCount = "15";

        private class fileData
        {
            public string fileName { set; get; }
            private Dictionary<string, double> wordsProbabilities;
            public double probability { set; get; }
            public fileData()
            {
                wordsProbabilities = new Dictionary<string, double> { };
                fileName = "";
                probability = 0.0;
            }
            public void addWord(string word, Data data)
            {
                if (!this.wordsProbabilities.ContainsKey(word))
                {
                    if (data.ContainsKey(word))
                        this.wordsProbabilities.Add(word, data.getProb(word)[2]);
                    else
                        this.wordsProbabilities.Add(word, defaultProbability);
                }
            }

            public void countProbability(IEnumerable<KeyValuePair<string, double>> top)
            {
                double p1 = 1.0,
                    p2 = 1.0;
                foreach (var item in top)
                {
                    p1 *= item.Value;
                    p2 *= (1 - item.Value);
                }
                probability = p1 / (p1 + p2);
            }

            public void countProbability(int length)
            {
                double p1 = 1.0,
                    p2 = 1.0;
                if (length >= wordsProbabilities.Count)
                {
                    foreach (var item in wordsProbabilities)
                    {
                        p1 *= item.Value;
                        p2 *= (1 - item.Value);
                    }
                    probability = p1 / (p1 + p2);
                }
                else
                {
                    Dictionary<string, double> temp = new Dictionary<string, double> (wordsProbabilities);
                    for (int i = 0; i < temp.Count; i++)
                        temp[temp.ElementAt(i).Key] = Math.Abs(0.5 - temp.ElementAt(i).Value);
                    var top = temp.OrderByDescending(pair => pair.Value).Take(length);
                    foreach (var item in top)
                    {
                        p1 *= wordsProbabilities[item.Key];
                        p2 *= (1 - wordsProbabilities[item.Key]);
                    }
                    probability = p1 / (p1 + p2);
                }
            }

            public Dictionary<string, double> getAllValues()
            {
                return wordsProbabilities;
            }
        }

        private class Data
        {
            /*
            @param int[0] leksemos pasikartojimas spamo laiškuose
            @param int[1] leksemos pasikartojimas ne spamo laiškuose
            */
            private Dictionary<string, int[]> table;

            /*
            @param double[0] leksemos tikimybė spamo laiškuose | P(W|S)
            @param double[1] leksemos tikimybė ne spamo laiškuose | P(W|H)
            @param duoble[2] P(S|W) Apsimokymo duomenų leksemos spamiškumo tikimybė
            */
            private Dictionary<string, double[]> probTable;
            
            public int SpamCount { set; get; } // leksemų kiekis spam laiškuose
            public int HamCount { set; get; } // leksemų kiekis ne spam laiškuose

            public Data()
            {
                table = new Dictionary<string, int[]> { };
                probTable = new Dictionary<string, double[]> { };
                SpamCount = 0;
                HamCount = 0;
            }

            public void CalculateProbabilities()
            {
                foreach (KeyValuePair<string, int[]> pair in this.table)
                {
                    double[] arr = new double[3];
                    arr[0] = (double)pair.Value[0] / SpamCount;
                    arr[1] = (double)pair.Value[1] / HamCount;

                    if (pair.Value[0] == 0)
                        arr[2] = 0.01;
                    else if (pair.Value[1] == 0)
                        arr[2] = 0.99;
                    else
                        arr[2] = arr[0] / (arr[0] + arr[1]);

                    probTable.Add(pair.Key, arr);
                }
            }

            public void addSpamPair(string word)
            {
                if (table.ContainsKey(word))
                    table[word][0] += 1;
                else
                {
                    int[] arr = new int[2];
                    arr[0] = 1;
                    arr[1] = 0;
                    table.Add(word, arr);
                    this.SpamCount++;
                }
            }
            public void addNotSpamPair(string word)
            {
                if (table.ContainsKey(word))
                    table[word][1] += 1;
                else
                {
                    int[] arr = new int[2];
                    arr[0] = 0;
                    arr[1] = 1;
                    table.Add(word, arr);
                    this.HamCount++;
                }
            }
            public int[] getValue(string word)
            {
                return table[word];
            }

            public double[] getProb(string word)
            {
                return probTable[word];
            }

            public bool ContainsKey(string word)
            {
                return probTable.ContainsKey(word);
            }

            public Dictionary<string, int[]> getAllValues()
            {
                return table;
            }
        }

        public Form1()
        {
            InitializeComponent();
            button1.Focus();
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            textBox1.Enabled = false;
            table = new Data();
            filesData = new List<fileData>();
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
                int spamLettersCount = 0;
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
                        spamLettersCount++;
                        FS.Close();
                    }
                    MessageBox.Show("Spamo failai sėkmingai nuskaityti!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    richTextBox1.Text += "Iš viso nuskaityta SPAM failų: " + spamLettersCount + "\n";
                    button2.Enabled = true;
                    button2.Focus();
                }
                foreach (KeyValuePair<string, int[]> pair in table.getAllValues())
                {
                    //richTextBox1.Text += pair.Key + ": " + pair.Value[0] + ", " + pair.Value[1] + "\n";
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
                int HamLettersCount = 0;
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
                        HamLettersCount++;
                        FS.Close();
                    }
                    richTextBox1.Text += "Iš viso nuskaityta HAM failų: " + HamLettersCount + "\n";
                    MessageBox.Show("Ne spamo failai sėkmingai nuskaityti!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    button3.Enabled = true;
                    button3.Focus();
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

        private void button3_Click(object sender, EventArgs e)
        {
            table.CalculateProbabilities();
            double[] arr = new double[3];
            arr = table.getProb("is");
            Console.WriteLine("Is tikimybės: " + arr[0] + ", " + arr[1] + ", " + arr[2]);
            button4.Enabled = true;
            button4.Focus();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog fop = new OpenFileDialog();
                fop.Multiselect = true;
                fop.InitialDirectory = "D:\\STUDIJOS\\6 semestras\\Intelektikos pagrindai\\L3";
                fop.Filter = "txt|*.txt";
                fop.Title = "Pasirinkti spamo failus";

                String pattern = @"[A-Za-z0-9$'""]+";
                int lettersCount = 0;
                if (fop.ShowDialog() == DialogResult.OK)
                {
                    foreach (String filename in fop.FileNames)
                    {
                        fileData temp = new fileData();
                        temp.fileName = filename;

                        FileStream FS = new FileStream(filename, FileMode.Open, FileAccess.Read);
                        var lines = File.ReadLines(filename);
                        foreach (var line in lines)
                        {
                            foreach (Match m in Regex.Matches(line, pattern))
                            {
                                temp.addWord(m.Value, table);
                            }
                        }
                        lettersCount++;
                        FS.Close();
                        filesData.Add(temp);
                    }
                    richTextBox1.Text += "Pasirinktų testuoti laiškų kiekis: " + lettersCount + "\n";
                    MessageBox.Show("Failai tikrinimui nuskaityti!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Enabled = true;
                    textBox1.Text = defaultLexemCount;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!Regex.IsMatch(textBox1.Text, @"^\d+$"))
            {
                textBox1.Text = "Įveskite natūralų skaičių!";
                textBox1.SelectAll();
                textBox1.Focus(); //you need to call this to show selection if it doesn't has focus
                button5.Enabled = false;
            }
            else if(textBox1.Enabled) button5.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox1.Text += "\n-----------------------------------------------------------------\n";
            richTextBox1.Text +=  "Kai leksemų skaičius: " + textBox1.Text + "\n\n";
            foreach (var file in filesData)
            {
                file.countProbability(int.Parse(textBox1.Text));
                richTextBox1.Text += "Tikimybė, kad failas " + file.fileName + " yra SPAM: " + file.probability + "\n";
            }
        }

       
        
        
    }
}

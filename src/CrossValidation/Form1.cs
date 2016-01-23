﻿using System;
using System.Windows.Forms;
using System.Xml;
using SharpNeat.Decoders;
using SharpNeat.Genomes.Neat;
using SharpNeat.Decoders.Neat;
using SharpNeat.Domains;
using SharpNeat.Phenomes;
using SharpNeat.Domains.Classification;

namespace CrossValidation
{
    public partial class Form1 : Form
    {
        private IBlackBox phenome;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int inputsCount;
            int outputsCount;
            Int32.TryParse(textBox1.Text, out inputsCount);
            Int32.TryParse(textBox2.Text, out outputsCount);
            if (inputsCount == 0 || outputsCount == 0)
            {
                MessageBox.Show("You should define inputs and outputs count.");
                return;
            }

            // Have the user choose the genome XML file.
            var result = openFileDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;

            var _activationScheme = NetworkActivationScheme.CreateAcyclicScheme();
            var _neatGenomeParams = new NeatGenomeParameters();
            _neatGenomeParams.FeedforwardOnly = _activationScheme.AcyclicNetwork;
            var genomeFactory = new NeatGenomeFactory(inputsCount, outputsCount, _neatGenomeParams);
            try
            {
                using (XmlReader xr = XmlReader.Create(openFileDialog1.FileName))
                {
                    NeatGenome genome = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, genomeFactory)[0];
                    // Get a genome decoder that can convert genomes to phenomes.
                    var genomeDecoder = new NeatGenomeDecoder(_activationScheme);

                    // Decode the genome into a phenome (neural network).
                    phenome = genomeDecoder.Decode(genome);                    
                    neatGenomeView1.RefreshView(genome);
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show("Error loading genome from file!\nLoading aborted.\n" + e1.Message);
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int inputsCount;
            int outputsCount;
            Int32.TryParse(textBox1.Text, out inputsCount);
            Int32.TryParse(textBox2.Text, out outputsCount);
            if (inputsCount == 0 || outputsCount == 0)
            {
                MessageBox.Show("You should define inputs and outputs count.");
                return;
            }
            var result = openFileDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;
            CrossValidationDatasetProvider provider = new CrossValidationDatasetProvider.Builder()
                .filename(openFileDialog1.FileName)
                .delimeter(";")
                .inputsCount(inputsCount)
                .outputsCount(outputsCount)
                .build();
            Dataset dataset;
            try
            {
                dataset = provider.getData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to parse the dataset file. \n" + ex.Message);
                return;
            }
            
            if (dataset.InputCount != inputsCount && dataset.OutputCount != outputsCount)
            {
                MessageBox.Show("Defined inputs and outputs count are not match to the provided dataset.");
                return;
            }
            var evaluator = new Evaluator();
            var info = evaluator.Evaluate(phenome, dataset);
            string text = "";
            text += "Samples count: " + dataset.Samples.Count + "\n";
            text += " ------------------------------------------------------------------------------------------------------------\n";
            text += "|                                 |   Expected positive   |   Expected negative   |\n";
            text += " -------------------------------------------------------------\n";
            text += "|  Predicted positive   |        " + info.TP.ToString("0.00") + " (TP)" + "        |         " + info.FP.ToString("0.00") + " (FP)" +  "         |\n";
            text += " ------------------------------------------------------------------------------------------------------------\n";
            text += "|  Predicted negative |        " + info.FN.ToString("0.00") + " (FN)" + "        |         " + info.TN.ToString("0.00") + " (TN)" + "         |\n";
            text += " ------------------------------------------------------------------------------------------------------------\n";
            text += "\n\n";
            text += "Accuracy: " + info.Accuracy.ToString("0.00") + "\n";
            text += "Precision: " + info.Precision.ToString("0.00") + "\n";
            text += "Recall: " + info.Recall.ToString("0.00") + " \n";
            text += "FMeasure: " + info.FMeasure.ToString("0.00") + "\n";
            text += "Correctly classified: " + info.CorrectlyClassified + "\n";
            text += "Incorrectly classified: " + info.IncorrectlyClassified + "\n";
            text += "\n\n";
            text += "Accuracy = (TP + TN) / (TP + TN + FP + FN)\n";
            text += "Precision = TP / (TP + FP)\n";
            text += "Recall = TP / (TP + FN)\n";
            text += "FMeasure = 2 * (precision * recall / (precision + recall))";
            label1.Text = text;
        }
    }
}
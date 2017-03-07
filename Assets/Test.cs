using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using FANNCSharp;
using FANNCSharp.Float;
using DataType = System.Single;
using System.IO;

class Test : MonoBehaviour
{
    private void Start()
    {
        print("HI");

        Momentum();
    }

    private void Update()
    {

    }

    static void Momentum()
    {
        const uint num_layers = 3;
        const uint num_neurons_hidden = 96;
        const float desired_error = 0.00007F;


        using (TrainingData trainData = new TrainingData())
        using (TrainingData testData = new TrainingData())
        {
            trainData.CreateTrainFromCallback(374, 48, 3, TrainingDataCallback);
            testData.CreateTrainFromCallback(594, 48, 3, TestDataCallback);

            // Test Accessor classes
            for (int i = 0; i < trainData.TrainDataLength; i++)
            {
                print(String.Format("Input {0}: ", i));
                for (int j = 0; j < trainData.InputCount; j++)
                {
                    print(String.Format("{0}, ", trainData.InputAccessor[i][j]));
                }
                print(String.Format("\nOutput {0}: ", i));
                for (int j = 0; j < trainData.OutputCount; j++)
                {
                    print(String.Format("{0}, ", trainData.OutputAccessor[i][j]));
                }
                print(String.Format(""));
            }

            for (float momentum = 0.0F; momentum < 0.7F; momentum += 0.1F)
            {
                print(String.Format("============= momentum = {0} =============\n", momentum));
                using (NeuralNet net = new NeuralNet(NetworkType.LAYER, num_layers, trainData.InputCount, num_neurons_hidden, trainData.OutputCount))
                {
                    net.SetCallback(TrainingCallback, "Hello!");

                    net.TrainingAlgorithm = TrainingAlgorithm.TRAIN_INCREMENTAL;

                    net.LearningMomentum = momentum;

                    net.TrainOnData(trainData, 20000, 500, desired_error);

                    print(String.Format("MSE error on train data: {0}", net.TestData(trainData)));
                    print(String.Format("MSE error on test data: {0}", net.TestData(testData)));
                }

            }
        }

    }

    static StreamReader trainingFile = null;
    static StreamReader testFile = null;
    static void TrainingDataCallback(uint number, uint inputCount, uint outputCount, DataType[] input, DataType[] output)
    {
        if (trainingFile == null)
        {
            trainingFile = new StreamReader("datasets\\robot.train");
            trainingFile.ReadLine(); // The info on the first line is provided by the callee
        }
        if (number % 100 == 99)
        {
            System.GC.Collect(); // Make sure nothing's getting garbage-collected prematurely
            GC.WaitForPendingFinalizers();
        }
        GetDataFromStream(trainingFile, inputCount, input);
        GetDataFromStream(trainingFile, outputCount, output);
    }

    static void TestDataCallback(uint number, uint inputCount, uint outputCount, DataType[] input, DataType[] output)
    {
        if (testFile == null)
        {
            testFile = new StreamReader("datasets\\robot.test");
            testFile.ReadLine(); // The info on the first line is provided by the callee
        }
        if (number % 100 == 99)
        {
            System.GC.Collect(); // Make sure nothing's getting garbage-collected prematurely
            GC.WaitForPendingFinalizers();
        }
        GetDataFromStream(testFile, inputCount, input);
        GetDataFromStream(testFile, outputCount, output);
    }

    static void GetDataFromStream(StreamReader file, uint count, DataType[] output)
    {
        string[] tokens = file.ReadLine().Split(new char[] { ' ' });
        for (int i = 0; i < count; i++)
        {
            output[i] = DataType.Parse(tokens[i]);
        }
    }

    static int TrainingCallback(NeuralNet net, TrainingData data, uint maxEpochs, uint epochsBetweenReports, float desiredError, uint epochs, object userData)
    {
        System.GC.Collect(); // Make sure nothing's getting garbage-collected prematurely
        GC.WaitForPendingFinalizers();
        print(String.Format("Callback: Last neuron weight: {0}, Last data input: {1}, Max epochs: {2}\nEpochs between reports: {3}, Desired error: {4}, Current epoch: {5}\nGreeting: \"{6}\"",
                            net.ConnectionArray[net.TotalConnections - 1].Weight, data.InputAccessor.Get((int)data.TrainDataLength - 1, (int)data.InputCount - 1),
                            maxEpochs, epochsBetweenReports, desiredError, epochs, userData));
        return 1;
    }

    void Train()
    {
        const uint num_input = 3;
        const uint num_output = 1;
        const uint num_layers = 4;
        const uint num_neurons_hidden = 5;
        const float desired_error = 0.0001F;
        const uint max_epochs = 5000;
        const uint epochs_between_reports = 1000;
        using (NeuralNet net = new NeuralNet(NetworkType.LAYER, num_layers, num_input, num_neurons_hidden, num_neurons_hidden, num_output))
        {
            net.ActivationFunctionHidden = ActivationFunction.SIGMOID_SYMMETRIC;
            net.ActivationFunctionOutput = ActivationFunction.LINEAR;
            net.TrainingAlgorithm = TrainingAlgorithm.TRAIN_RPROP;
            using (TrainingData data = new TrainingData("datasets\\scaling.data"))
            {
                net.SetScalingParams(data, -1, 1, -1, 1);
                net.ScaleTrain(data);

                net.TrainOnData(data, max_epochs, epochs_between_reports, desired_error);
                net.Save("scaling.net");

            }
        }
    }

    void TestNet()
    {
        DataType[] calc_out;
        print("Creating network.");

        using (NeuralNet net = new NeuralNet("scaling.net"))
        {
            net.PrintConnections();
            net.PrintParameters();
            print("Testing network.");
            using (TrainingData data = new TrainingData("datasets\\scaling.data"))
            {
                for (int i = 0; i < data.TrainDataLength; i++)
                {
                    net.ResetMSE();
                    net.ScaleInput(data.GetTrainInput((uint)i));
                    calc_out = net.Run(data.GetTrainInput((uint)i));
                    net.DescaleOutput(calc_out);
                    print(String.Format("Result {0} original {1} error {2}", calc_out[0], data.OutputAccessor[i][0],
                                      FannAbs(calc_out[0] - data.OutputAccessor[i][0])));
                }

            }
        }
    }

    static float FannAbs(float value)
    {
        return (((value) > 0) ? (value) : -(value));
    }
}
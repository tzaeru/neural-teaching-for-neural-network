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
    static public ImageData image_data;

    private void Start()
    {
        image_data = FindObjectOfType<ImageData>();

        Momentum();
    }

    private void Update()
    {

    }

    static void Momentum()
    {
        const uint num_layers = 3;
        const uint num_neurons_hidden = 20;
        const float desired_error = 0.01F;

        using (TrainingData trainData = new TrainingData())
        using (TrainingData testData = new TrainingData())
        {
            trainData.CreateTrainFromCallback(500, (uint)(image_data.image_size * image_data.image_size), 1, TrainingDataCallback);
            testData.CreateTrainFromCallback(500, (uint)(image_data.image_size * image_data.image_size), 1, TestDataCallback);

            // Test Accessor classes
            /*for (int i = 0; i < trainData.TrainDataLength; i++)
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
            }*/

            using (NeuralNet net = new NeuralNet(NetworkType.LAYER, num_layers, trainData.InputCount, num_neurons_hidden, trainData.OutputCount))
            {
                net.SetCallback(TrainingCallback, "");

                net.TrainingAlgorithm = TrainingAlgorithm.TRAIN_RPROP;
                net.LearningMomentum = 0.7f;
                net.LearningRate = 0.2f;
                net.ActivationFunctionHidden = ActivationFunction.SIGMOID_SYMMETRIC;
                net.ActivationFunctionOutput = ActivationFunction.SIGMOID_SYMMETRIC;
                

                net.TrainOnData(trainData, 1000, 100, desired_error);

                print(String.Format("MSE error on train data: {0}", net.TestData(trainData)));
                print(String.Format("MSE error on test data: {0}", net.TestData(testData)));
            }
            print("Ending1");
        }

    }

    static StreamReader trainingFile = null;
    static StreamReader testFile = null;
    static void TrainingDataCallback(uint number, uint inputCount, uint outputCount, DataType[] input, DataType[] output)
    {
        print("input count: " + inputCount);

        bool take_target = number%2 == 0 ? true : false;
        float[] image;


        if (take_target)
        {
            image = image_data.target_images[(int)number/2 % image_data.target_images.Count];
        }
        else
            image = image_data.other_images[(int)number/2 % image_data.other_images.Count];
        /*if (take_target)
            image = image_data.GetRandomTargetImage();
        else
            image = image_data.GetRandomOtherImage();*/

        for (int i = 0; i < image_data.image_size * image_data.image_size; i++)
        {
            input[i] = image[i];
        }

        if (take_target)
            output[0] = 1.0f;
        else
            output[0] = -1.0f;

        /*
                if (take_target)
        {
            output[0] = 1.0f;
            output[1] = -1.0f;
        }
        else
        {
            output[0] = -1.0f;
            output[1] = 1.0f;
        }
        */
    }

    static void TestDataCallback(uint number, uint inputCount, uint outputCount, DataType[] input, DataType[] output)
    {
        print("Outpout c: " + outputCount);
        bool take_target = number % 2 == 0 ? true : false;
        float[] image;
        number += 500;

        if (take_target)
        {
            image = image_data.target_images[(int)number/2 % image_data.target_images.Count];
        }
        else
            image = image_data.other_images[(int)number/2 % image_data.other_images.Count];

        for (int i = 0; i < image_data.image_size * image_data.image_size; i++)
        {
            input[i] = image[i];
        }

        if (take_target)
            output[0] = 1.0f;
        else
            output[0] = -1.0f;
    }

    static int TrainingCallback(NeuralNet net, TrainingData data, uint maxEpochs, uint epochsBetweenReports, float desiredError, uint epochs, object userData)
    {
        print(String.Format("CAAAAAAAAALLLBAAAAAAAACK: MSE error on train data: {0}", net.TestData(data)));
        System.GC.Collect(); // Make sure nothing's getting garbage-collected prematurely
        GC.WaitForPendingFinalizers();
        print(String.Format("Callback: Last neuron weight: {0}, Last data input: {1}, Max epochs: {2}\nEpochs between reports: {3}, Desired error: {4}, Current epoch: {5}\nGreeting: \"{6}\"",
                            net.ConnectionArray[net.TotalConnections - 1].Weight, data.InputAccessor.Get((int)data.TrainDataLength - 1, (int)data.InputCount - 1),
                            maxEpochs, epochsBetweenReports, desiredError, epochs, userData));
        return 1;
    }
}
 
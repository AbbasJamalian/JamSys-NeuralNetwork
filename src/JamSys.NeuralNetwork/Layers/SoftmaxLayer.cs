﻿#region License
/*
 * Copyright (c) 2020 - Abbas Jamalian
 * This file is part of JamSys Project and is licensed under the MIT License. 
 * For more details see the License file provided with the software
 */
#endregion License

using System;
using System.Text.Json.Serialization;

namespace JamSys.NeuralNetwork.Layers
{
    /// <summary>
    /// Implementation of Softmax Layer
    /// </summary>
    public class SoftmaxLayer : ILayer
    {

        [JsonIgnore]
        public Tensor Input { get; private set; }

        [JsonIgnore]
        public Tensor Output { get; private set; }

        public int OutputCount { get; private set; }

        public SoftmaxLayer(int outputCount)
        {
            OutputCount = outputCount;
        }

        public void Build(ILayer previousLayer)
        {
            ValidateLayer(previousLayer);
            Input = previousLayer.Output;
            Output = new Tensor(OutputCount);
        }

        private void ValidateLayer(ILayer previousLayer)
        {
            if (OutputCount <= 0)
                throw new ArgumentException("the number of outputs must be one or greater");
            if (previousLayer.Output.Width != OutputCount)
                throw new ArgumentException("The number of input and outputs must be identical in a Softmax layer");
            if (previousLayer.Output.Height > 1 || previousLayer.Output.Depth > 1)
                throw new ArgumentException("input of a softmax Layer must be one dimensional");
        }

        public void Run()
        {
            double total = 0;
            for (int x = 0; x < Output.Width; x++)
            {
                Output[x] = Math.Exp(Input[x]);
                total += Output[x];
            }

            for (int x = 0; x < Output.Width; x++)
            {
                Output[x] /= total;
            }
        }

        public void Clear()
        {
            Output?.Clear();
        }

        public Tensor Backpropagate(Tensor delta, double rate)
        {
            Tensor layerDelta = Input.CreateSimilar();

            for (int x = 0; x < layerDelta.Width; x++)
            {
                layerDelta[x] = 0;

                for (int d = 0; d < delta.Width; d++)
                {
                    layerDelta[x] += d == x ? delta[d] * Output[d] * (1 - Output[d]) : delta[d] * Output[x] * Output[d] * -1;
                }
            }

            return layerDelta;
        }
    }

}


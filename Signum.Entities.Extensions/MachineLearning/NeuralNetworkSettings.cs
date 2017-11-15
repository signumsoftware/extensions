﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signum.Entities.MachineLearning
{
    [Serializable, EntityKind(EntityKind.Part, EntityData.Master)]
    public class NeuralNetworkSettingsEntity : Entity, IPredictorAlgorithmSettings
    {
        public PredictionType PredictionType { get; set; }
        public int MinibatchSize { get; set; } = 1000;
        public bool? SparseMatrix { get; set; }
        [Unit("Minibaches")]
        public int SaveProgressEvery { get; set; } = 5;

        public IPredictorAlgorithmSettings Clone() => new NeuralNetworkSettingsEntity
        {
            MinibatchSize = MinibatchSize,

        };
    }

    public enum PredictionType
    {
        Regression,
        Classification,
    }
}
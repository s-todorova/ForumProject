using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.TorchSharp;

namespace ForumApp
{
    public static class TrainModel
    {
        private class ModelInput
        {
            [LoadColumn(0)]
            [ColumnName("label")]
            public float label { get; set; }

            [LoadColumn(1)]
            [ColumnName("Text")]
            public string Text { get; set; } = "";
        }

        public static void Train()
        {
            var mlContext = new MLContext(seed: 1);

            var data = mlContext.Data.LoadFromTextFile<ModelInput>(
                path: "dataset/comments_dataset.tsv",
                hasHeader: true,
                separatorChar: '\t');

            var pipeline = mlContext.Transforms
                .Conversion.MapValueToKey("Label", "label",
                    keyOrdinality: Microsoft.ML.Transforms.ValueToKeyMappingEstimator.KeyOrdinality.ByValue)
                .Append(mlContext.MulticlassClassification.Trainers
                    .TextClassification(
                        labelColumnName: "Label",
                        sentence1ColumnName: "Text",
                        maxEpochs: 20))
                .Append(mlContext.Transforms.Conversion
                    .MapKeyToValue("PredictedLabel"));

            Console.WriteLine("Тренирам модела...");
            var model = pipeline.Fit(data);

            Directory.CreateDirectory("MLModels");
            mlContext.Model.Save(model, data.Schema, "MLModels/SentimentAnalysis.zip");
            Console.WriteLine("Моделът е запазен в MLModels/SentimentAnalysis.zip");
        }
    }
}
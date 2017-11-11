using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;


namespace Sandwych.Hmm.Benchmark
{
    public class HmmForwardAlgorithm
    {
        [Benchmark]
        public void EmptyHmm()
        {

        }

        [Benchmark]
        public void MyHmm()
        {
            List<Rain> candidates = new List<Rain>();
            candidates.Add(Rain.T);
            candidates.Add(Rain.F);

            var initialStateProbabilities = new Dictionary<Rain, double>();
            initialStateProbabilities.Add(Rain.T, 0.5);
            initialStateProbabilities.Add(Rain.F, 0.5);

            var emissionProbabilitiesForUmbrella = new Dictionary<Rain, double>();
            emissionProbabilitiesForUmbrella.Add(Rain.T, 0.9);
            emissionProbabilitiesForUmbrella.Add(Rain.F, 0.2);

            var emissionProbabilitiesForNoUmbrella = new Dictionary<Rain, double>();
            emissionProbabilitiesForNoUmbrella.Add(Rain.T, 0.1);
            emissionProbabilitiesForNoUmbrella.Add(Rain.F, 0.8);

            var transitionProbabilities = new Dictionary<Transition<Rain>, double>();
            transitionProbabilities.Add(new Transition<Rain>(Rain.T, Rain.T), 0.7);
            transitionProbabilities.Add(new Transition<Rain>(Rain.T, Rain.F), 0.3);
            transitionProbabilities.Add(new Transition<Rain>(Rain.F, Rain.T), 0.3);
            transitionProbabilities.Add(new Transition<Rain>(Rain.F, Rain.F), 0.7);

            ForwardBackwardAlgorithm<Rain, Umbrella> fw = new ForwardBackwardAlgorithm<Rain, Umbrella>();
            fw.StartWithInitialStateProbabilities(candidates, initialStateProbabilities);
            fw.NextStep(Umbrella.T, candidates, emissionProbabilitiesForUmbrella,
                    transitionProbabilities);
            fw.NextStep(Umbrella.T, candidates, emissionProbabilitiesForUmbrella,
                    transitionProbabilities);
            fw.NextStep(Umbrella.F, candidates, emissionProbabilitiesForNoUmbrella,
                    transitionProbabilities);
            fw.NextStep(Umbrella.T, candidates, emissionProbabilitiesForUmbrella,
                    transitionProbabilities);
            fw.NextStep(Umbrella.T, candidates, emissionProbabilitiesForUmbrella,
                    transitionProbabilities);

            var result = fw.ComputeSmoothingProbabilities();
        }

    }
}

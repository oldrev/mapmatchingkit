# Overview

[![Build status](https://ci.appveyor.com/api/projects/status/oh77641k0s78g4b2/branch/master?svg=true)](https://ci.appveyor.com/project/oldrev/mapmatchingkit/branch/master)

Sandwych.MapMatchingKit is a GPS map-matching solution for .NET platform.

This solution is porting from the GraphHopper's "map-matching" project which developed in Java.

## Sandwych.MapMatchingKit

The map-matching library.

## Sandwych.Hmm

A general purpose utility library implements Hidden Markov Models (HMM) for time-inhomogeneous Markov processes for .NET.

This library provides an implementation of

* The Viterbi algorithm, which computes the most likely sequence of states.
* The forward-backward algorithm, which computes the probability of all state candidates given

the entire sequence of observations. This process is also called smoothing.

# Roadmap and Current Status

**Working in progress.**

# Requirements

* .NET Standard 2.0 or .NET Standard 4.6.1

# Applications

This library was initially created for HMM-based map matching according to the paper
"NEWSON, Paul; KRUMM, John. Hidden Markov map matching through noise and sparseness.
In: Proceedings of the 17th ACM SIGSPATIAL international conference on advances in geographic
information systems. ACM, 2009. S. 336-343."

Besides map matching, the hmm-lib can also be used for other applications.

# License

This library is licensed under the
[Apache 2.0 license](http://www.apache.org/licenses/LICENSE-2.0.html).

# Contribute
Contributions are welcome! For bug reports, please create an issue. 
For code contributions (e.g. new features or bugfixes), please create a pull request.

# Credits

* "map-matching" from GraphHopper Project: [https://github.com/graphhopper/map-matching](https://github.com/graphhopper/map-matching)
* "hmm-lib" from BMW Car IT GmbH: [https://github.com/bmwcarit/hmm-lib](https://github.com/bmwcarit/hmm-lib)

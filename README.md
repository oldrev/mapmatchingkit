# Overview

A library implements Hidden Markov Models (HMM) for time-inhomogeneous Markov processes for .NET Standard 2.0.

This library provides an implementation of
* The Viterbi algorithm, which computes the most likely sequence of states.
* The forward-backward algorithm, which computes the probability of all state candidates given
the entire sequence of observations. This process is also called smoothing.

# Applications

This library was initially created for HMM-based map matching according to the paper
"NEWSON, Paul; KRUMM, John. Hidden Markov map matching through noise and sparseness.
In: Proceedings of the 17th ACM SIGSPATIAL international conference on advances in geographic
information systems. ACM, 2009. S. 336-343."

[Graphhopper](https://graphhopper.com/) [map matching](https://github.com/graphhopper/map-matching)
is now using the hmm-lib for matching GPS positions to OpenStreetMap maps. 

The [offline-map-matching](https://github.com/bmwcarit/offline-map-matching) project
demonstrates how to use the hmm-lib for map matching but does not provide integration to any
particular map.

Besides map matching, the hmm-lib can also be used for other applications.

# License

This library is licensed under the
[Apache 2.0 license](http://www.apache.org/licenses/LICENSE-2.0.html).

# Dependencies

Except for testing, there are no dependencies to other libraries.

# Contribute
Contributions are welcome! For bug reports, please create an issue. 
For code contributions (e.g. new features or bugfixes), please create a pull request.

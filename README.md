# Overview

[![NuGet Stats](https://img.shields.io/nuget/v/Sandwych.MapMatchingKit.svg)](https://www.nuget.org/packages/Sandwych.MapMatchingKit) 
[![Build status](https://ci.appveyor.com/api/projects/status/oh77641k0s78g4b2/branch/master?svg=true)](https://ci.appveyor.com/project/oldrev/mapmatchingkit/branch/master)
[![Build Status](https://travis-ci.org/oldrev/mapmatchingkit.svg?branch=master)](https://travis-ci.org/oldrev/mapmatchingkit)

Sandwych.MapMatchingKit is a GPS map-matching solution for .NET platform.

This solution is porting from the [Barefoot](https://github.com/bmwcarit/barefoot) project which developed in Java.

## Sandwych.MapMatchingKit

The map-matching library.

## Sandwych.Hmm

A general purpose utility library implements Hidden Markov Models (HMM) for time-inhomogeneous Markov processes for .NET.

# Roadmap and Current Status

**Alpha** - Basic functions works. 

The API can and will change frequently, do not use it for production.

# Requirements

* .NET Standard 2.0 or .NET Standard 4.6.1

# Applications

This library was initially created for HMM-based map matching according to the paper
"NEWSON, Paul; KRUMM, John. Hidden Markov map matching through noise and sparseness.
In: Proceedings of the 17th ACM SIGSPATIAL international conference on advances in geographic
information systems. ACM, 2009. S. 336-343."

Besides map matching, the hmm-lib can also be used for other applications.

# License

* Copyright 2015-2017 BMW Car IT GmbH
* Copyright 2017-2018 Wei "oldrev" Li and Contributors

This library is licensed under the [Apache 2.0 license](http://www.apache.org/licenses/LICENSE-2.0.html).

# Contribute

Contributions are always welcome! For bug reports, please create an issue. 
For code contributions (e.g. new features or bugfixes), please create a pull request.

# Credits

* "barefoot" from BMW Car IT GmbH: [https://github.com/bmwcarit/barefoot](https://github.com/bmwcarit/barefoot)
* "map-matching" from GraphHopper Project: [https://github.com/graphhopper/map-matching](https://github.com/graphhopper/map-matching)
* "hmm-lib" from BMW Car IT GmbH: [https://github.com/bmwcarit/hmm-lib](https://github.com/bmwcarit/hmm-lib)
* "Nito.Collections.Deque" from Stephen Cleary: [https://github.com/StephenCleary/Deque](https://github.com/StephenCleary/Deque)
* "NetTopologySuite & ProjNET4GeoAPI" from NetTopologySuite Project: [https://github.com/NetTopologySuite](https://github.com/NetTopologySuite)
* PriorityQueue class from Rx.NET Project: [https://github.com/Reactive-Extensions/Rx.NET](https://github.com/Reactive-Extensions/Rx.NET)

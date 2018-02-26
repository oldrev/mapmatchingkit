#!/usr/bin/env groovy

node {
    stage('compile') {
        checkout scm
            stash 'everything'
            dir('src/cafe') {
                bat 'dotnet restore'
                bat "dotnet build src/Sandwych.Hmm -f netstandard1.1"
                bat "dotnet build src/Sandwych.MapMatchingKit -f netstandard2.0"
            }
    }
}

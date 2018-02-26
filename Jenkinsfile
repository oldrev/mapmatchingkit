#!/usr/bin/env groovy

stage('compile') {
    node {
        checkout scm
        stash 'everything'
        bat 'dotnet restore'
        bat 'dotnet build src/Sandwych.Hmm -f netstandard1.1'
        bat 'dotnet build src/Sandwych.MapMatchingKit -f netstandard2.0'
    }
}

stage('test') {
    parallel unitTests: {
        test('Test')
    }, 
    integrationTests: {
        test('IntegrationTest')
    },
    failFast: false
}

def test(type) {
    node {
        unstash 'everything'
        bat 'dotnet test test/Sandwych.Hmm.Tests/Sandwych.Hmm.Tests.csproj'
        bat 'dotnet test test/Sandwych.MapMatchingKit.Tests/Sandwych.MapMatchingKit.Tests.csproj'
    }
}

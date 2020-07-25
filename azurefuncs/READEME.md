# RSS Azure Function implementation of RSS Feed Generetor for a site
This project implements a Azure Function API for generating RSS Feeds

** Pre-requisites: ** You need to have .netcore 3.1 installed 

## Setting up the project locally
`` git clone https://github.com/thisisrajiraj/sites ``

Then cd into the folder containing azurefuncs.csproj. Run:

`` dotnet clean ``
`` dotnet restore ``
`` dotnet build ``

## Running the project
To run the project, create a local.settings.json file in the folder containing azurefuncs.csproj
Add to the file the following:

Setting name | Setting Value| Default value
------------ | -------------| -------------
title| Title for your RSS feed|empty string
description| Description for your RSS feed| empty string
language| Language of your RSS feed| en
indexfilelocation| URL of a json file containing the feed index| required
contentfileroot| Root URL of each feed item| required
enablecontent| If the output XML should contain content tag| 0
maxitems| Max number of items to return | 10

Index file should be served as a json in a HTTP location. The format 
is as in the following example:
```json
 [
    {
        "date": "September 6, 2018",
        "name": "foo",
        "title": "How to not foo"
    },
    {
        "date": "September 11, 2018",
        "name": "bar",
        "title": "How to not bar"
    }
 ]
 ```
 In this example, if indexfilelocation is "http://contoso.com/blogindex.json" and contentfileroot 
 is "http://contoso.com/posts", then the following locations should be valid and reachable:

 * http://contoso.com/posts/bar
 * http://contoso.com/posts/foo
 * http://contoso.com/blogindex.json

## Deploying to Azure using VSCode
Once you have built, updated local.settings.json, tested locally, now it's
time to deploy to Azure. To do that, there are two ways:

1. Use Azure App Service extension from VS code
2. Use Azure DevOps pipeline and use a GitHub YAML file. The checked in
YAML file should work

Before running your Azure App, make sure you have set all your application
settings from local.settings.json file in Azure Function App through the 
Azure portal.
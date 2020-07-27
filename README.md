
# Common Azure Fuctions needed to support a personal website
This project implements backend Azure Function API for common website tasks
like blog feed generation, contact form that sends an email to an owner etc.


## Setting up the project locally
`` git clone https://github.com/thisisrajiraj/sites ``

** Pre-requisites: ** You need to have .netcore 3.1 installed 

Then cd into the folder containing azurefuncs.csproj. Run:

`` dotnet clean ``
`` dotnet restore ``
`` dotnet build ``

## Running the project
To run the project, create a local.settings.json file in the folder containing azurefuncs.csproj.

### For using /Feed API
Feed API is used to generate a RSS feed from your blog site
Add to the local.settings file the following for using the /Feed API:

Setting name | Setting Value| Default value
------------ | -------------| -------------
rootURL| Root URL of the website| null (this is required)
blogURL| URL of the blog| null (this is required)
title| Title for your RSS feed| "Website feed" 
description| Description for your RSS feed| "This is a generated blog feed" 
language| Language of your RSS feed| "en"
indexfilelocation| URL of a json file containing the feed index| null (this is required)
contentfileroot| Root URL of each html post| value of the rootURL
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
 In this example, if indexfilelocation is "http://contoso.com/blogindex.json",
 blogURL is  http://contoso.com/blog, and contentfileroot  is
 "http://contoso.com/posts", then the following locations should be valid and reachable:

 * http://contoso.com/posts/bar.html
 * http://contoso.com/posts/foo.html
 * http://contoso.com/blogindex.json
 *  http://contoso.com/blog/foo

### For using /SendMail API
SendMail API is used from website contact forms where individuals
want to contact the website owners.
Add to the file the following for using the /SendMail API:

Setting name | Setting Value| Default value
------------ | -------------| -------------
emailfrom| Email address of who the email should be sent from. This typically is the website owner email| null (this is required)
fromname| Name of who the email should be sent from. This typically is the website owner email.| null (this is required)
emailto| Who the email should be sent to.  This typically is the website owner email.| emailfrom value
toname| Name of who the email should be sent to. This typically is the website owner email.| fromname value
emailsubject| Subject of the email| "New email from &lt;emailfrom&gt;
sendgridapikey| API key from send grid. You SendGrid dev API key | empty (this is required)

SendMail uses SendGrid API to send email. Please make sure you 
have a [SendGrid] (https://sendgrid.com/) account and configure it right.
Check [here] (https://app.sendgrid.com/settings/api_keys) for your API key
and add your from address [here] (https://app.sendgrid.com/settings/sender_auth).

Parameters to /SendMail should be sent as a JSON body. Parameters needed include:
Setting name | Setting Value
------------ | -------------
from| The name of the individual trying to contact
fromemail| Who the email should be sent to
message| Content of the email

E.g.
``` json
{
    "from":"foo",
    "fromemail":"bar@bar.com",
    "message":"Hello world"
}
```
### For using /MinsToRead API
MinsToRead returns the approximate number of mins needed
to read an article. 

Add to the local.settings file the following for using the API:
Setting name | Setting Value| Default value
------------ | -------------| -------------
contentfileroot| Root URL where posts can be fetched from | If not set, pass in JSON body to request
azurestorageconnstring| Connection string to Azure Blog storage account | null (this is required)

Parameters to /SendMail should be sent as a JSON body. Parameters needed include:
Setting name | Setting Value
------------ | -------------
name| Unique name of the blog
fileurl| Entire URL to get the blog content from. Can be left out if contentfileroot is configured.

E.g.
``` json
{
    "name":"foo"
}
```

In this example, if contentfileroot  is
 "http://contoso.com/posts", and then the following locations should be valid and reachable:

 * http://contoso.com/posts/foo.html


## Running tests
cd into the azurefuncs_test folder, and run:

`` dotnet test``

## Running the application

If you are using VSCode, simply hit Run -> Start 
Debugging (F5 on Windows) from the menu to debug.

## Deploying to Azure using VSCode
Once you have built, updated local.settings.json, tested locally, now it's
time to deploy to Azure. To do that, there are two ways:

1. Use Azure App Service extension from [VS code] (https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs-code?tabs=csharp)
2. Use [Azure DevOps pipeline] (https://docs.microsoft.com/en-us/azure/devops/pipelines/targets/azure-functions-window)
and use a GitHub YAML file. The checked in YAML file should work.

Before running your Azure App, make sure you have set all your application
settings from local.settings.json file in Azure Function App through the 
Azure portal.

Happy development!
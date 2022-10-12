# DurableUniqueIdGenerator
Generate new numeric ids in sequence for any resource id string. Integer ids will always be in sequence, and will always be unique.

All api calls can use http get or post.

## GenerateIds

### api/GenerateIds/{resourceId}/{count}/{waitForResultMilliseconds?}

Authorization: GenerateIdsKey

{resourceId} - any string that identifies the resource

{count} - how many ids to generate

{waitForResultMilliseconds?} - optional, default to 1,5 seconds, how many milliseconds to wait for a result before a 202 accepted is returned

For example, a call to http://localhost:7231/api/GenerateIds/mycounter/10/5000, and the response might look like this:
```json
{
    "StartId": 11,
    "EndId": 20
}
```
10 New ids, from 11 to 20, have been created for the resource "mycounter". The ids are guaranteed to always be unique, that is if the couter was not reset or deleted. The value 5000 is 5 seconds to wait for the result. If the result was not yet returned within the 5 seconds, then a 202 accepted will be returned with a durable function status check payload. In this case, use the status query url provided to check the status and retrieve the result.

## MasterReset

### api/MasterReset/{resourceId}/{id}/{waitForResultMilliseconds?}

Authorization: MasterKey

{id} - this is the new value of the counter, this can be set to 0 or any interger value, including negatives

## DeleteResourceCounter

### api/DeleteResourceCounter/{resourceId}

Authorization: MasterKey

{resourceId} - the string that identifies the resource

## ListResourceCounters

### api/ListResourceCounters

Authorization: MasterKey and GenerateIdsKey

Example response:
```json
[
   {
      "entityId":{
         "name":"resourcecounter",
         "key":"mycounter1"
      },
      "lastOperationTime":"2022-10-11T13:34:06.1793106Z",
      "state":20
   },
   {
      "entityId":{
         "name":"resourcecounter",
         "key":"mycounter2"
      },
      "lastOperationTime":"2022-10-11T13:34:06.2274173Z",
      "state":30
   }
]
```
The "key" is the resource id and "state" is the value of the current count of the resource.

## Token Based Security:

In the local.settings.json file:

    "MasterKey": "xxxxxxxxxxxxxxxxxxx", // used to authorize -> MasterReset
                                        //                   -> DeleteResourceCounter
                                        //                   -> ListResourceCounters
    
    "GenerateIdsKey": "xxxxxxxxxxxxxxxxxxx" // used to authorize -> GenerateIds
                                            //                   -> ListResourceCounters
    
The key must be passed in the auth header as a bearer token.

It is recommended to also use function keys and the app system master key to add further security, and also to implement token based security using Azure AD.

## Unit Tests:

Open the separate test solution TestIdGenerator.sln and run the unit tests. Make sure the DurableUniqueIdGenerator app is running before running the tests.

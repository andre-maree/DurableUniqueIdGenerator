# DurableUniqueIdGenerator
Generate new numeric ids in sequence for any resource id string. Integer ids will always be in sequence, and will always be unique.

## GenerateIds

### api/GenerateIds/{resourceId}/{count}/{waitForResultMilliseconds?}

Authorization: GetIdsKey

{resourceId} - any string that identifies the resource

{count} - how many ids to generate

{waitForResultMilliseconds?} - optional, defaulkt to 1,5 seconds, how many milliseconds to wait for a result before a 202 accepted is returned

For example, a call to http://localhost:7231/api/GenerateIds/mycounter/10/5000, and the response will look like this:
```json
{
    "StartId": 11,
    "EndId": 20
}
```

10 New ids, from 11 to 20, have been created for the resource "mycounter". The ids are garenteed to always be unique, that is if the couter was not reset or deleted.

## MasterReset

### api/MasterReset/{resourceId}/{id}/{waitForResultMilliseconds?}

Authorization: MasterKey

{id} - this is the new value of the counter, this can be rest to 0 or any interger value, including negatives

## DeleteCounterResource

### api/DeleteCounterResource/{resourceId}

Authorization: MasterKey

{resourceId} - the string that identifies the resource

## ListCounterResources

### api/ListCounterResources

Authorization: MasterKey and GetIdsKey

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
The "key" is the resource id and "state" is value of the current count of the resource.

## Authorization:

In the local.settings.json file:

    "MasterKey": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", // used to authorize -> set the counter to any value
    
    "GetIdsKey": "yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy" // used to authorize -> get new ids

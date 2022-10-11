# DurableUniqueIdGenerator
Generate new numeric ids in sequence for any resource id string. Integer ids will always be in sequence, and will always be unique.

## GenerateIds

api/GenerateIds/{resourceId}/{count}/{waitForResultMilliseconds?}

{resourceId} - any string that identifies the resource
{count} - how many ids to generate
{waitForResultMilliseconds?} - optional, defaulkt to 1,5 seconds, how many milliseconds to wait for a result before a 202 accepted is returned

For example, a call to http://localhost:7231/api/GenerateIds/mycounter/10/5000, and the response will look like this:

{
    "StartId": 11,
    "EndId": 20
}

10 New ids, from 11 to 20, have been created for the resource "mycounter". The ids are garenteed to always be unique, that is if the couter was not reset or deleted.

## MasterReset

api/MasterReset/{resourceId}/{id}/{waitForResultMilliseconds?}

{id} - this is the new value of the counter, this can be rest to 0 or any interger value, incuding negatives

## DeleteCounterResource

api/DeleteCounterResource/{resourceId}

{resourceId} - the string that identifies the resource

## ListCounterResources
## api/ListCounterResources, and the response will look like this:

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

Authorization:
        DeleteCounterResource: [GET,POST] http://localhost:7231/api/DeleteCounterResource/{resourceId}

        GenerateIds: [GET,POST] http://localhost:7231/api/GenerateIds/{resourceId}/{count}/{waitForResultMilliseconds?}

        ListCounterResources: [GET,POST] http://localhost:7231/api/ListCounterResources

        MasterReset: [GET,POST] http://localhost:7231/api/MasterReset/{resourceId}/{id}/{waitForResultMilliseconds?}

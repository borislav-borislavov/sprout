using Xunit;

//this is making the tests run sequentially instead of in parallel, this is needed because I am using the same database name
//if Db names are dynamic and cleaned up afterwards paralelization can be enabled again
[assembly: CollectionBehavior(DisableTestParallelization = true)]
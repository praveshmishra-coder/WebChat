using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly IMongoClient _client;

    public TestController(IMongoClient client)
    {
        _client = client;
    }

    [HttpGet("mongo")]
    public IActionResult TestMongo()
    {
        var dbs = _client.ListDatabaseNames().ToList();
        return Ok(dbs);
    }
}
